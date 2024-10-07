using DryIoc;
using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Locations;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RightVisionBotDb
{
    public class Bot
    {
        public IConfiguration Languages;
        public ITelegramBotClient Client { get; private set; }
        private readonly ILogger _logger;
        private LocationManager LocationManager { get; }
        private Keyboards Keyboards { get; }
        private DatabaseService DatabaseService { get; }
        private RvLogger RvLogger { get; set; }
        private LogMessages LogMessages { get; }

        public Bot(
            ILogger logger,
            LocationManager locationManager,
            DatabaseService databaseService,
            LogMessages logMessages,
            Keyboards keyboards)
        {
            _logger = logger;
            LocationManager = locationManager ?? throw new NullReferenceException(nameof(locationManager));
            DatabaseService = databaseService ?? throw new NullReferenceException(nameof(databaseService));
            LogMessages = logMessages ?? throw new NullReferenceException(nameof(logMessages));
            Keyboards = keyboards ?? throw new NullReferenceException(nameof(keyboards));

        }

        public void Build()
        {
            _logger.Information("Запуск бота...");
            try
            {
                Client = new TelegramBotClient(App.Configuration.GetSection(nameof(Bot))["Token"]!);
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                var receiverOptions = new ReceiverOptions { AllowedUpdates = [UpdateType.CallbackQuery, UpdateType.Message, UpdateType.ChatMember] };
                Client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
                _logger.Information("Бот успешно запущен!");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Не удалось запустить бота!");
                throw;
            }
        }

        public void RegisterBot()
        {
            _logger.Information("Загрузка языковых файлов...");
            Languages = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("Resources/Lang/ru.json", false)
                .Build();

            Language.Build(Languages, Enums.Lang.Ru);
            _logger.Information("Сборка языковых файлов завершена.");

            _logger.Information("Инициализация RvLogger...");
            RvLogger = App.Container.Resolve<RvLogger>();

            _logger.Information("Сборка конфигурации...");

            App.Configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("config.json")
                .Build();

            App.DefaultRightVision = App.Configuration.GetSection("Contest")["DefaultRightVision"] ?? throw new NullReferenceException(nameof(App.DefaultRightVision));

            RegisterLocations();

            Build();
        }

        private void RegisterLocations()
        {
            _logger.Information("Регистрация локаций...");
            App.Container.Register<Start>();
            App.Container.Register<MainMenu>();
            App.Container.Register<Profile>();
            App.Container.Register<CriticForm>();

            LocationManager
                .RegisterLocation(nameof(Start), typeof(Start))
                .RegisterLocation(nameof(MainMenu), typeof(MainMenu))
                .RegisterLocation(nameof(Profile), typeof(Profile))
                .RegisterLocation(nameof(CriticForm), typeof(CriticForm));
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            RvUser? rvUser = null;
            using var db = DatabaseService.GetApplicationDbContext();
            if (callbackQuery != null)
            {
                _logger.Information("=== {0} ===" +
                    $"\nCallbackId: {callbackQuery.Id}" +
                    $"\nCallbackData: {callbackQuery.Data}" +
                    $"\n" +
                    $"\nId отправителя: {callbackQuery.From.Id}" +
                    $"\nUsername отправителя: {"@" + callbackQuery.From.Username}" +
                    $"\nИмя отправителя: {callbackQuery.From.FirstName}" +
                    $"\n" +
                    $"\nЧат: {callbackQuery.Message?.Chat.Type}" +
                    $"\nId чата: {callbackQuery.Message?.Chat.Id}" +
                    $"\n", "Входящий Callback");
                rvUser = db.RvUsers.FirstOrDefault(u => u.UserId == callbackQuery.From.Id);
                if (rvUser != null)
                {
                    var callbackContext = new CallbackContext(rvUser, callbackQuery, db);
                    var containsArgs = false;
                    if (callbackContext.CallbackQuery.Data!.Contains('-'))
                        containsArgs = true;

                    rvUser.LocationChanged += OnLocationChanged;
                    await rvUser.Location.HandleCallbackAsync(callbackContext, containsArgs, token);
                }
                else
                {
                    if (callbackQuery.Message!.Chat.Type == ChatType.Private)
                        await Client.DeleteMessageAsync(callbackQuery.Message.Chat, callbackQuery.Message.MessageId, token);

                    await Client.AnswerCallbackQueryAsync(callbackQuery.Id, "Похоже, твои данные повреждены или утеряны. Для дальнейшего взаимодействия с ботом тебе необходимо повторно выбрать свой язык", cancellationToken: token);
                    rvUser = new RvUser(callbackQuery.From.Id, Enums.Lang.Na, callbackQuery.From.FirstName, callbackQuery.From.Username, LocationManager["Start"]);
                    var success = false;
                    try
                    {
                        await Client.SendTextMessageAsync(rvUser.UserId, "Choose Lang:", replyMarkup: Keyboards.СhooseLang, cancellationToken: token);
                        success = true;
                    }
                    catch
                    {
                        _logger.Warning("Произошла попытка отправить сообщение пользователю, с которым ещё не было чата!");
                    }

                    if (success)
                    {
                        db.RvUsers.Add(rvUser);
                        await db.SaveChangesAsync(token);
                    }
                }
            }
            else if (message != null && message.From != null)
            {
                _logger.Information("=== {0} ===" +
                    $"\nId отправителя: {message.From?.Id}" +
                    $"\nUsername отправителя: {"@" + message.From?.Username}" +
                    $"\nИмя отправителя: {message.From?.FirstName}" +
                    $"\n" +
                    $"\nТекст сообщения: {message.Text}" +
                    $"\n" +
                    $"\nЧат: {message.Chat.Type}" +
                    $"\nId чата: {message.Chat.Id}" +
                    $"\n", "Входящее сообщение");

                rvUser = db.RvUsers.FirstOrDefault(u => u.UserId == message.From!.Id);
                if (message.Text == "/start")
                {
                    var location = LocationManager[nameof(Start)];
                    if (rvUser == null)
                    {
                        rvUser = new RvUser(message.From!.Id, Enums.Lang.Na, message.From.FirstName, message.From.Username, location);
                        db.RvUsers.Add(rvUser);
                        await db.SaveChangesAsync(token);
                    }
                    else
                    {
                        rvUser.Location = location;
                        await db.SaveChangesAsync(token);
                    }
                }

                if (rvUser != null)
                {
                    var commandContext = new CommandContext(rvUser, message, db);
                    var containsArgs = false;
                    if (commandContext.Message.Text != null && commandContext.Message.Text.Contains(' '))
                        containsArgs = true;

                    rvUser.LocationChanged += OnLocationChanged;
                    await rvUser.Location.HandleCommandAsync(commandContext, containsArgs, token);
                }
            }
            if (rvUser != null)
                rvUser.LocationChanged -= OnLocationChanged;

            async void OnLocationChanged(object? sender, (IRvLocation, IRvLocation) e)
            {
                await RvLogger.Log(LogMessages.UserChangedLocation(rvUser, e), rvUser, token);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Error(exception, "Произошла ошибка при обработке входящего обновления!");
            return Task.CompletedTask;
        }
    }
}
