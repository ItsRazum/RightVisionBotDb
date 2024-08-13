using DryIoc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel.Resolution;
using Newtonsoft.Json;
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
                Client = new TelegramBotClient(Configuration.GetSection(nameof(Bot))["Token"]!);
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
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            _logger.Information(JsonConvert.SerializeObject(update));
            var message = update.Message;
            if (message != null && message.From != null)
            {
                var lowercase = message.Text?.ToLower();
                switch (lowercase)
                {
                    case "/start":
                        var rvUser = Core.GetRvUser(message.From.Id);
                        if (rvUser is not null) 
                        {
                            rvUser.Goto(App.Container.Resolve<Start>());
                        }
                        else
                        {
                            var newUser = new RvUser(message.From.Id, Enums.Lang.Na, message.From.FirstName, message.From.Username);

                        }
                        break;
                }
            }

        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Error(JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }
    }
}
