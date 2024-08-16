using DryIoc;
using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Bot.Keyboards.InlineKeyboards;
using RightVisionBotDb.Bot.Lang;
using RightVisionBotDb.Bot.Locations;
using RightVisionBotDb.Bot.Services;
using RightVisionBotDb.Models;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Bot
{
    public class Bot
    {

        public IConfiguration Configuration;
        internal Core.Core Core { get; private set; }
        public ITelegramBotClient Client { get; set; }
        private readonly ILogger _logger;

        public Bot(ILogger logger)
        {
            _logger = logger;
        }

        public void Build()
        {
            _logger.Information("Загрузка языковых файлов...");
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("Resources/Lang/ru.json", false)
                .Build();
            _logger.Information("Готово. Запуск бота...");
            try
            {
                Client = new TelegramBotClient(Core.Configuration.GetSection(nameof(Bot))["Token"]!);
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                var receiverOptions = new ReceiverOptions { AllowedUpdates = { } };
                Client.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);
                _logger.Information("Готово.");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Не удалось запустить бота!");
            }

            BuildLanguage();
        }

        private void BuildLanguage()
        {
            _logger.Information("Сборка языка...");
            Language.Build(Configuration, Enums.Lang.Ru);
            _logger.Information("Готово.");
        }

        public void RegisterTypes()
        {
            Log.Logger?.Information("Регистрация типов и сервисов из {project}...", "RightVisionBotDb.Bot");

            App.Container.Register<Core.Core>(Reuse.Singleton);

            App.Container.Register<RvLogger>(Reuse.Singleton);
            App.Container.Register<ProfileStringService>(Reuse.Singleton);


            _logger.Information("Регистрация клавиатур...");
            App.Container.Register<InlineKeyboards>(Reuse.Singleton);

            _logger.Information("Регистрация локаций...");
            App.Container.Register<Start>();

            App.Container.Resolve<Core.Core>().RegisterTypes();

            Core = App.Container.Resolve<Core.Core>();
            App.Container.Resolve<Bot>().Build();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var message = update.Message;
            var callbackQuery = update.CallbackQuery;

            RvUser rvUser;
            if (callbackQuery != null)
            {
                _logger.Information($"Входящий Callback:" +
                    $"\nCallbackId: {callbackQuery.Id}" +
                    $"\nCallbackData: {callbackQuery.Data}" +
                    $"\n" +
                    $"\nId отправителя: {callbackQuery.From.Id}" +
                    $"\nUsername отправителя: {"@" + callbackQuery.From.Username}" +
                    $"\nИмя отправителя: {callbackQuery.From.FirstName}" +
                    $"\n" +
                    $"\nЧат: {callbackQuery.Message?.Chat.Type}" +
                    $"\nId чата: {callbackQuery.Message?.Chat.Id}" +
                    $"\n");

                rvUser = Core.GetRvUser(callbackQuery.From.Id)!;
                await rvUser.Location.HandleCallbackAsync(botClient, callbackQuery, cancellationToken);
            }
            else if (message != null && message.From != null)
            {
                _logger.Information($"Входящее сообщение:" +
                    $"\nId отправителя: {message.From?.Id}" +
                    $"\nUsername отправителя: {"@" + message.From?.Username}" +
                    $"\nИмя отправителя: {message.From?.FirstName}" +
                    $"\n" +
                    $"\nТекст сообщения: {message.Text}" +
                    $"\n" +
                    $"\nЧат: {message.Chat.Type}" +
                    $"\nId чата: {message.Chat.Id}" +
                    $"\n");

                if (message.Text == "/start")
                {
                    if (Core.GetRvUser(message.From!.Id) == null)
                    {
                        var newUser = new RvUser(message.From!.Id, Enums.Lang.Na, message.From!.FirstName, message.From!.Username);
                        Core.AddNewRvUser(newUser);
                    }

                    rvUser = Core.GetRvUser(message.From!.Id)!;
                    rvUser.Goto(App.Container.Resolve<Start>());
                }
            }
        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Error(exception, "Произошла ошибка при обработке входящего обновления!");
            return Task.CompletedTask;
        }
    }
}
