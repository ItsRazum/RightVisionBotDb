using DryIoc;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;
using RightVisionBotDb.Services;
using RightVisionBotDb.Types;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace RightVisionBotDb.Locations
{
    public abstract class RootLocationBase : RvLocation
    {

        #region Properties

        protected DatabaseService DatabaseService { get; }
        protected readonly ILogger _logger;

        #endregion

        public RootLocationBase(
            Bot bot,
            Keyboards keyboards,
            LocationManager locationManager,
            RvLogger rvLogger,
            LogMessages logMessages,
            LocationsFront locationsFront,
            ILogger logger,
            DatabaseService databaseService)
            : base(bot, keyboards, locationManager, rvLogger, logMessages, locationsFront)
        {
            DatabaseService = databaseService;
            _logger = logger;
        }

        public override async Task HandleCallbackAsync(CallbackContext c, bool containsArgs, CancellationToken token = default)
        {
            await base.HandleCallbackAsync(c, containsArgs, token);
        }

        public override async Task HandleCommandAsync(CommandContext c, bool containsArgs, CancellationToken token = default)
        {
            await base.HandleCommandAsync(c, containsArgs, token);
        }

        public abstract Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token = default);

        public abstract Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken token = default);
    }
}
