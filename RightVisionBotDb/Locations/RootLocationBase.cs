using RightVisionBotDb.Helpers;
using RightVisionBotDb.Lang;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RightVisionBotDb.Locations
{
    public abstract class RootLocationBase : RvLocation
    {

        #region Properties

        protected readonly ILogger _logger;

        #endregion

        public RootLocationBase(
            Bot bot,
            LocationManager locationManager,
            RvLogger rvLogger,
            LocationsFront locationsFront,
            ILogger logger)
            : base(bot, locationManager, rvLogger, locationsFront)
        {
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
