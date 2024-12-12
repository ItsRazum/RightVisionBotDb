using RightVisionBotDb.Data.Contexts;
using RightVisionBotDb.Interfaces;

namespace RightVisionBotDb.Helpers
{
    public static class DatabaseHelper
    {
        public static AcademyDbContext GetAcademyDbContext(string academyName)
            => new AcademyDbContext(academyName);

        public static RightVisionDbContext GetRightVisionContext(string rightvisionName)
            => new RightVisionDbContext(rightvisionName);

        public static ApplicationDbContext GetApplicationDbContext()
            => new ApplicationDbContext();
    }
}
