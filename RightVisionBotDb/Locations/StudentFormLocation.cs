using RightVisionBotDb.Services;
using RightVisionBotDb.Singletons;

namespace RightVisionBotDb.Locations
{
    internal class StudentFormLocation : RvLocation
    {
        #region Properties

        private StudentFormService StudentFormService { get; }

        #endregion

        #region Constructor

        public StudentFormLocation(
            Bot bot, 
            LocationService locationService,
            RvLogger logger, 
            LocationsFront locationsFront,
            StudentFormService studentFormService) 
            : base(bot, locationService, logger, locationsFront)
        {
            StudentFormService = studentFormService;
        }

        #endregion

        #region RvLocation overrides



        #endregion

    }
}
