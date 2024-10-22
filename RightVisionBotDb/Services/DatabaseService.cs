using RightVisionBotDb.Data;

namespace RightVisionBotDb.Services
{
    public class DatabaseService
    {
        public RightVisionDbContext GetRightVisionContext(string rightvisionName)
            => new(rightvisionName);

        public ApplicationDbContext GetApplicationDbContext()
            => new();
    }
}
