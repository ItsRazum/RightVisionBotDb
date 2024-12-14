using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Data.Configurations;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Data.Contexts
{
    public class RightVisionDbContext : DbContext, IRightVisionDbContext
    {

        #region Properties

        public string Name { get; }
        public string DatabasesDir { get; }
        public RightVisionStatus Status
        {
            get => Properties.First().RightVisionStatus;
            protected set => Properties.First().RightVisionStatus = value;
        }

        public EnrollmentStatus EnrollmentStatus
        {
            get => Properties.First().EnrollmentStatus;
            protected set => Properties.First().EnrollmentStatus = value;
        }
        public DateOnly StartDate
        {
            get => Properties.First().StartDate;
            protected set => Properties.First().StartDate = value;
        }

        public DateOnly? EndDate
        {
            get => Properties.First().EndDate;
            protected set => Properties.First().EndDate = value;
        }

        #endregion

        #region Constructor

        public RightVisionDbContext(string databaseName)
        {
            Name = databaseName;
            DatabasesDir = App.Configuration.RightVisionSettings.RightVisionDatabasesPath;
            if (Database.EnsureCreated())
            {
                Properties.Add(new DbProperties());
                SaveChanges();
            }
        }

        #endregion

        #region Data

        public DbSet<ParticipantForm> ParticipantForms => Set<ParticipantForm>();
        public DbSet<DbProperties> Properties => Set<DbProperties>();

        #endregion

        #region Methods

        public void Open()
        {
            EnrollmentStatus = EnrollmentStatus.Open;
        }

        #endregion

        #region DbContext implementation

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabasesDir, $"{Name}.db");
            optionsBuilder.UseSqlite("Data Source=" + databasePath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ParticipantFormEntityTypeConfiguration());
            modelBuilder.ApplyConfiguration(new RightVisionDbPropertiesEntityTypeConfiguration());
        }

        #endregion
    }
}
