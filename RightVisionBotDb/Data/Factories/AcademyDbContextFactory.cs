using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RightVisionBotDb.Data.Contexts;

namespace RightVisionBotDb.Data.Factories
{
    //public class AcademyDbContextFactory : IDesignTimeDbContextFactory<AcademyDbContext>
    //{
    //    public AcademyDbContext CreateDbContext(string[] args)
    //    {
    //        var builder = new DbContextOptionsBuilder<AcademyDbContext>();

    //        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "Academy.db");
    //        builder.UseSqlite($"Data Source={dbPath}");

    //        return new AcademyDbContext(builder.Options);
    //    }
    //}
}
