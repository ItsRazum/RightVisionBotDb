using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Locations;
using RightVisionBotDb.Services;
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

        #endregion

        #region Properties

        public IConfiguration Languages;
        public ITelegramBotClient Client { get; private set; }
        private LocationManager LocationManager { get; }
        private Keyboards Keyboards { get; }
        private DatabaseService DatabaseService { get; }
        private LogMessages LogMessages { get; }

        #endregion


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
            LogMessages     = logMessages ?? throw new NullReferenceException(nameof(logMessages));
            Keyboards       = keyboards ?? throw new NullReferenceException(nameof(keyboards));

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
            if (_isInitialized) return;

            _logger.Information("Загрузка языковых файлов...");
            Languages = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("Resources/Lang/ru.json", false)
                .Build();

            Language.Build(Languages, Enums.Lang.Ru);
            _logger.Information("Сборка языковых файлов завершена.");

            _logger.Information("Сборка конфигурации...");

            App.Configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("config.json")
                .Build();

            App.DefaultRightVision = App.Configuration.GetSection("Contest")["DefaultRightVision"] ?? throw new NullReferenceException(nameof(App.DefaultRightVision));
            App.RightVisionDatabasesPath = App.Configuration.GetSection("Data")["RightVisionDatabasesPath"] ?? throw new NullReferenceException("RightVisionDatabasesPath");

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
                using var rvdb = DatabaseService.GetRightVisionContext(App.DefaultRightVision);
                rightVisions =
                    Directory.GetFiles(info.FullName) 
                    .Where(s => s.EndsWith(".db"))
                    .Select(s => s.Split("\\")
                    .Last())
                    .OrderBy(s => s)
                    .ToArray();
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
                .RegisterLocation<CriticFormLocation>(nameof(CriticFormLocation));

            Build();
        }
    }
}
