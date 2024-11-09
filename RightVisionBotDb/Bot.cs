using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Locations;
using RightVisionBotDb.Singletons;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace RightVisionBotDb
{
    public sealed class Bot
    {

        #region Fields

        private readonly ILogger _logger;
        private bool _isInitialized = false;
        public const long ParticipantChatId = -1002074764678;
        public const long CriticChatId = -1001968408177;
        public IConfiguration Languages;

        #endregion

        #region Properties

        public ITelegramBotClient Client { get; private set; }
        private LocationManager LocationManager { get; }

        #endregion


        public Bot(
            ILogger logger,
            LocationManager locationManager)
        {
            _logger = logger;
            LocationManager = locationManager ?? throw new NullReferenceException(nameof(locationManager));

        }

        public void Build()
        {
            _logger.Information("Запуск бота...");
            try
            {
                Client = new TelegramBotClient(App.Configuration.BotSettings.Token);
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                var receiverOptions = new ReceiverOptions { AllowedUpdates = [UpdateType.CallbackQuery, UpdateType.Message, UpdateType.ChatMember] };
                var root = (RootLocation)LocationManager[nameof(RootLocation)];

                Client.StartReceiving(root.HandleUpdateAsync, root.HandleErrorAsync, receiverOptions, cancellationToken);
                _logger.Information("Бот успешно запущен!");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Не удалось запустить бота!");
                throw;
            }
        }

        public void Configure()
        {
            if (_isInitialized)
            {
                _logger.Warning("Произошёл повторный вызов Bot.Configure()!");
                return;
            }

            _logger.Information("Загрузка языковых файлов...");
            Languages = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("Resources/Lang/ru.json", false)
                .AddJsonFile("Resources/Lang/kz.json", false)
                .Build();

            Language.Build(Languages, Enums.Lang.Ru);
            _logger.Information("Сборка языковых файлов завершена.");

            _logger.Information("Сборка конфигурации...");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            App.Configuration = new(configuration);

            App.DefaultRightVision = App.Configuration.ContestSettings.DefaultRightVision;
            App.RightVisionDatabasesPath = App.Configuration.DataSettings.RightVisionDatabasesPath;

            _logger.Information("Проверка баз данных...");
            DirectoryInfo? info = null;
            if (!Directory.Exists(App.RightVisionDatabasesPath))
            {
                _logger.Warning("Не удалось найти папку RightVision. Создаётся новая...");
                info = Directory.CreateDirectory(App.RightVisionDatabasesPath);
                _logger.Information("Успешно создана папка по пути {0}", info.FullName);
            }

            info ??= new DirectoryInfo(App.RightVisionDatabasesPath);

            var rightVisions =
                Directory.GetFiles(info.FullName)
                .Where(s => s.EndsWith(".db"))
                .Select(s => s.Split("\\")
                .Last())
                .OrderBy(s => s)
                .ToArray();

            if (rightVisions.Length == 0)
            {
                _logger.Warning("В папке нет ни одной базы данных, создаётся база данных с названием {0}...", App.DefaultRightVision);
                using var rvdb = DatabaseHelper.GetRightVisionContext(App.DefaultRightVision);
                rightVisions =
                    [.. Directory.GetFiles(info.FullName)
                    .Where(s => s.EndsWith(".db"))
                    .Select(s => s.Split("\\")
                    .Last())
                    .OrderBy(s => s)];
            }
            App.AllRightVisions =
                rightVisions
                .Select(s => s.Replace(".db", string.Empty))
                .ToArray();


            _logger.Information("Регистрация локаций...");

            LocationManager
                .RegisterLocation<RootLocation>(nameof(RootLocation))
                .RegisterLocation<PublicChat>(nameof(PublicChat))
                .RegisterLocation<Start>(nameof(Start))
                .RegisterLocation<MainMenu>(nameof(MainMenu))
                .RegisterLocation<Profile>(nameof(Profile))
                .RegisterLocation<CriticFormLocation>(nameof(CriticFormLocation))
                .RegisterLocation<ParticipantFormLocation>(nameof(ParticipantFormLocation));

            Build();
        }
    }
}
