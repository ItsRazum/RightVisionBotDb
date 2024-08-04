using DryIoc;
using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Types.Forms;

namespace RightVisionBotDb.Data
{
    public class RightVisionDbContext : DbContext
    {
        public RightVisionStatus Status { get; set; } = RightVisionStatus.Relevant;
        public ParticipationSubmissionStatus ParticipationSubmissionStatus { get; set; } = ParticipationSubmissionStatus.Closed;
        public string Name { get; }

        public RightVisionDbContext(string databaseName)
        {
            Name = databaseName;
            Database.EnsureCreated();
        }

        public DbSet<ParticipantForm> ParticipantForms => Set<ParticipantForm>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databaseDirFromConfig = App.Container.Resolve<Bot>().Configuration.GetSection("Data:RightVisionDatabasesPath").Value!;
            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseDirFromConfig, $"{Name}");
            optionsBuilder.UseSqlite("Data Source=" + databasePath);
        }
    }
}
