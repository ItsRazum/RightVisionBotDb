using RightVisionBotDb.Singletons;

namespace RightVisionBotDb.Locations
{
    public sealed class ControlPanel : RvLocation
    {

        #region Constructor

        public ControlPanel(
            Bot bot,
            LocationManager
            locationManager,
            RvLogger logger,
            LocationsFront locationsFront)
            : base(bot, locationManager, logger, locationsFront)
        {

        }

        #endregion

    }
}