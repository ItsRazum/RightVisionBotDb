using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;

namespace RightVisionBotDb.Locations
{
    internal class StudentFormLocation : RvLocation
    {
        public StudentFormLocation(
            Bot bot, 
            LocationService locationService,
            RvLogger logger, 
            LocationsFront locationsFront) 
            : base(bot, locationService, logger, locationsFront)
        {
        }
    }
}
