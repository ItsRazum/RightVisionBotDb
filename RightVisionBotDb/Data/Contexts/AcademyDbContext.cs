using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Data.Configurations;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Models;

namespace RightVisionBotDb.Data.Contexts
{
    public class AcademyDbContext : DbContext
    {
        #region Properties

        public string Name { get; set; }
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

        public AcademyDbContext(string databaseName)
        {
            Name = databaseName;
            DatabasesDir = App.AcademyDatabasesPath;
            if (Database.EnsureCreated())
            {
                Properties.Add(new DbProperties());
                SaveChanges();
            }
        }

        #endregion

        #region Data

        public DbSet<StudentForm> StudentForms => Set<StudentForm>();
        private DbSet<DbProperties> Properties => Set<DbProperties>();

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
