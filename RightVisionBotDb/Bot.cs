using Microsoft.Extensions.Configuration;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Locations;
using RightVisionBotDb.Services;
using RightVisionBotDb.Text;
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

        public BotParameters Parameters { get; private set; }
        public ITelegramBotClient Client { get; private set; }
        private LocationService LocationService { get; }
        private ShellService ShellService { get; }

        #endregion

        #region Constructor

        public Bot(
            ILogger logger,
            LocationService locationService,
            ShellService shellService)
        {
            _logger = logger;
            LocationService = locationService ?? throw new NullReferenceException(nameof(locationService));
            ShellService = shellService ?? throw new NullReferenceException(nameof(shellService));

        }

        #endregion

        #region Methods

        public void Build()
        {
            _logger.Information("Запуск бота...");
            try
            {
                Client = new TelegramBotClient(
                    !string.IsNullOrEmpty(App.Configuration.BotSettings.Token)
                    ? App.Configuration.BotSettings.Token
                    : App.Configuration.HiddenToken ?? throw new NullReferenceException("Токен не указан!"));

                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                var receiverOptions = new ReceiverOptions { AllowedUpdates = [UpdateType.CallbackQuery, UpdateType.Message, UpdateType.ChatMember] };
                var root = (RootLocation)LocationService[nameof(RootLocation)];

                Client.StartReceiving(root.HandleUpdateAsync, root.HandleErrorAsync, receiverOptions, cancellationToken);
                _logger.Information("Бот успешно запущен!");
                _isInitialized = true;
                Thread thread = new(ShellService.Run);
                thread.Start();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Не удалось запустить бота!");
                throw;
            }
        }

        public void Configure(string[] args)
        {
            if (_isInitialized)
            {
                _logger.Warning("Произошёл повторный вызов Bot.Configure()!");
                return;
            }

            _logger.Information("Загрузка языковых файлов...");

            Phrases.Build(Lang.Ru, Lang.Kz, Lang.Ua);
            _logger.Information("Сборка языковых файлов завершена.");

            _logger.Information("Сборка конфигурации...");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Secret.json", true)
                .Build();

            App.Configuration = new(configuration);

            _logger.Information("Проверка баз данных...");
            if (!Directory.Exists(App.Configuration.AcademySettings.AcademyDatabasesPath))
            {
                _logger.Warning("Не удалось найти папку академий. Создаётся новая...");
                var academyDirInfo = Directory.CreateDirectory(App.Configuration.AcademySettings.AcademyDatabasesPath);
                _logger.Information("Успешно создана папка по пути {0}", academyDirInfo.FullName);
            }

            DirectoryInfo? info = null;

            if (!Directory.Exists(App.Configuration.RightVisionSettings.RightVisionDatabasesPath))
            {
                _logger.Warning("Не удалось найти папку RightVision. Создаётся новая...");
                info = Directory.CreateDirectory(App.Configuration.RightVisionSettings.RightVisionDatabasesPath);
                _logger.Information("Успешно создана папка по пути {0}", info.FullName);
            }

            info ??= new DirectoryInfo(App.Configuration.RightVisionSettings.RightVisionDatabasesPath);

            var rightVisions =
                Directory.GetFiles(info.FullName)
                .Where(s => s.EndsWith(".db"))
                .Select(s => s.Split(Path.DirectorySeparatorChar)
                .Last())
                .OrderBy(s => s)
                .ToArray();

            if (rightVisions.Length == 0)
            {
                _logger.Warning("В папке нет ни одной базы данных, создаётся база данных с названием {0}...", App.Configuration.RightVisionSettings.DefaultRightVision);
                using var rvdb = DatabaseHelper.GetRightVisionContext(App.Configuration.RightVisionSettings.DefaultRightVision);
                rightVisions =
                    [.. Directory.GetFiles(info.FullName)
                    .Where(s => s.EndsWith(".db"))
                    .Select(s => s.Split(Path.DirectorySeparatorChar)
                    .Last())
                    .OrderBy(s => s)];
            }

            App.AllRightVisions =
                [.. rightVisions
                .Select(s => s.Replace(".db", string.Empty))
                .OrderByDescending(s => s)];

            if (args.Contains("-enableAcademy"))
            {
                _logger.Information("Присвоен флаг {0}", BotParameters.EnableAcademy);
                Parameters |= BotParameters.EnableAcademy;
            }

            _logger.Information("Регистрация локаций...");

            LocationService
                .RegisterLocation<RootLocation>()
                .RegisterLocation<PublicChat>()
                .RegisterLocation<Start>()
                .RegisterLocation<MainMenu>()
                .RegisterLocation<Profile>()
                .RegisterLocation<CriticFormLocation>()
                .RegisterLocation<ParticipantFormLocation>()
                .RegisterLocation<StudentFormLocation>()
                .RegisterLocation<ChangeTrackLocation>()
                .RegisterLocation<TrackCardLocation>();

            Build();
        }

        #endregion
    }
}
