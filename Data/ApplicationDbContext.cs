using DryIoc;
using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Models;
using RightVisionBotDb.Types;
using Serilog;

namespace RightVisionBotDb.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext() => Database.EnsureCreated();

    public DbSet<RvUser> RvUsers => Set<RvUser>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=RightVision.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Enums
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Lang).HasConversion<string>();
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Category).HasConversion<string>();
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Status).HasConversion<string>();
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Role).HasConversion<string>();
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Location).HasConversion<string>();

        //Types
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Permissions)
            .HasConversion(
                v => $"{v.RvUserId}:{v}",
                v => UserPermissions.FromString(v)
            );
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Rewards)
            .HasConversion(
                v => $"{v.RvUserId}:{v}",
                v => Rewards.FromString(v)
            );
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Punishments)
            .HasConversion(
                v => $"{v.RvUserId}:{v}",
                v => RvPunishments.FromString(v)
            );
    }
}

public class Db
{
    public static ApplicationDbContext Context = null!;
    private readonly ILogger _logger;
    public Dictionary<string, RightVisionDbContext> RightVisions { get; set; } = new();
    public Db(ILogger logger)
    {
        _logger = logger;
    }

    public void OpenDatabaseConnection()
    {
        _logger.Information("Настройка подключения к базе данных...");
        SQLitePCL.Batteries.Init();

        _logger.Information("Подключение к основной базе...");
        Context = new ApplicationDbContext();

        _logger.Information("Готово. Загрузка баз данных RightVision...");
        var databaseDirFromConfig = App.Container.Resolve<Bot>().Configuration.GetSection("Data:RightVisionDatabasesPath").Value!;
        var databaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseDirFromConfig);
        if (!Directory.Exists(databaseDir))
        {
            _logger.Warning("Папка с базами данных не найдена, создаётся новая...");
            Directory.CreateDirectory(databaseDir);

            _logger.Information("Готово. Создаётся одна база данных...");
            RightVisionDbContext rightVision = new("RightVision" + (DateTime.Now.Year - 2000));

            _logger.Information("База данных {name} успешно создана", rightVision.Name);
        }

        if (Directory.GetFiles(databaseDir, "*.db").Length == 0) 
        {
            _logger.Information("Не найдено ни одной базы данных. Создаётся новая...");
            RightVisionDbContext rightVision = new("RightVision" + (DateTime.Now.Year - 2000) + ".db");

            _logger.Information("База данных {name} успешно создана", rightVision.Name);
        }

        var databaseFiles = Directory.GetFiles(databaseDir, "*.db");
        foreach (var file in databaseFiles)
        {
            _logger.Information("Загрузка {databaseName}...", file.Split("\\").Last());
            try
            {
                var context = new RightVisionDbContext(file);
                RightVisions.Add(context.Name, context);
            }
            catch (Exception ex) 
            {
                _logger.Error(ex, "Ошибка при загрузке {databaseName}", file);
            }
        }
        _logger.Information("Готово.");
    }
}