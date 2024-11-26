using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;

namespace RightVisionBotDb.Locations
{
    public sealed class ControlPanel : RvLocation
    {

        #region Constructor

        public ControlPanel(
            Bot bot,
            LocationService
            locationService,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationService, logger, locationsFront)
        {

        }

        #endregion

    }
}