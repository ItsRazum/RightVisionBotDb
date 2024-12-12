using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<RvUser> RvUsers { get; }
        public DbSet<CriticForm> CriticForms { get; }
    }
}
