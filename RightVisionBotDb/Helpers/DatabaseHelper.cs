using RightVisionBotDb.Data;

namespace RightVisionBotDb.Helpers
{
    public static class DatabaseHelper
    {
        public static RightVisionDbContext GetRightVisionContext(string rightvisionName)
            => new(rightvisionName);

        public static ApplicationDbContext GetApplicationDbContext()
            => new();
    }
}
