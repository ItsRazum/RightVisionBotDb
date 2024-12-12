using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Interfaces
{
    public interface IAcademyDbContext : IMultipleDbContext
    {
        DbSet<StudentForm> StudentForms { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
