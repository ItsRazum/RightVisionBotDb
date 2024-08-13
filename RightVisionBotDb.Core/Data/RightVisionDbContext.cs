using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Core.Models;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Models.Forms;

namespace RightVisionBotDb.Core.Data
{
    public class RightVisionDbContext : DbContext
    {
        public string Name { get; }
        public string DatabasesDir { get; }
        public RightVisionStatus Status => Properties.First().RightVisionStatus;
        public EnrollmentStatus EnrollmentStatus => Properties.First().EnrollmentStatus;

        public RightVisionDbContext(string databaseName, string databasesDir)
        {
            Name = databaseName;
            DatabasesDir = databasesDir;
            if(Database.EnsureCreated())
            {
                Properties.Add(new RightVisionDbProperties());
                SaveChanges();
            }
        }

        public DbSet<ParticipantForm> ParticipantForms => Set<ParticipantForm>();
        private DbSet<RightVisionDbProperties> Properties => Set<RightVisionDbProperties>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabasesDir, $"{Name}");
            optionsBuilder.UseSqlite("Data Source=" + databasePath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RightVisionDbProperties>()
                .Property(r => r.RightVisionStatus)
                .HasConversion<string>();

            modelBuilder.Entity<RightVisionDbProperties>()
                .Property(r => r.EnrollmentStatus)
                .HasConversion<string>();
        }
    }
}
