using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Interfaces
{
    public interface IRightVisionDbContext : IMultipleDbContext
    {
        DbSet<ParticipantForm> ParticipantForms { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }

}
