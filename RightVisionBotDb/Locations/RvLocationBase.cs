using RightVisionBotDb.Services;

namespace RightVisionBotDb.Locations
{
    internal class RvLocationBase
    {

        #region Constructor

        public RvLocationBase(
            Bot bot,
            Keyboards inlineKeyboards,
            LocationManager locationManager,
            RvLogger logger,
            LogMessages logMessages,
            LocationsFront locationsFront)
        {
            Bot = bot ?? throw new ArgumentNullException(nameof(bot));
            InlineKeyboards = inlineKeyboards ?? throw new ArgumentNullException(nameof(inlineKeyboards));
            LocationManager = locationManager ?? throw new ArgumentNullException(nameof(locationManager));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            LogMessages = logMessages ?? throw new ArgumentNullException(nameof(logMessages));
            LocationsFront = locationsFront ?? throw new ArgumentNullException(nameof(locationsFront));
        }

        #endregion

        #region Properties

        protected Bot Bot { get; }
        protected Keyboards InlineKeyboards { get; }
        protected LocationManager LocationManager { get; }
        protected RvLogger Logger { get; }
        protected LogMessages LogMessages { get; }
        protected LocationsFront LocationsFront { get; }

        #endregion

    }
}
