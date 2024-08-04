using DryIoc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RightVisionBotDb.Data;
using RightVisionBotDb.Lang;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace RightVisionBotDb
{
    class Bot
    {
        public IConfiguration Configuration;
        public ITelegramBotClient Client { get; set; }
        private readonly ILogger _logger;

        public Bot(ILogger logger)
        {
            _logger = logger;
        }

        public async void Build()
        {
            _logger.Information("Загрузка конфигураций...");
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("config.json", false)
                .AddJsonFile("Resources/Lang/ru.json", false)
                .Build();
            _logger.Information("Готово. Запуск бота...");
            try
            {
                Client = new TelegramBotClient(Configuration.GetSection("Bot:Token").Value!);
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
            App.Container.Resolve<Db>().OpenDatabaseConnection();
            BuildLanguage();
        }

        private void BuildLanguage()
        {
            _logger.Information("Сборка языка...");
            Language.Build([ Enums.Lang.Ru ]);
            _logger.Information("Готово.");
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            _logger.Information(JsonConvert.SerializeObject(update));

        }
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            _logger.Error(JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }
    }
}
