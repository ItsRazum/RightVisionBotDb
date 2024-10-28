using DryIoc;
using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Helpers;
using RightVisionBotDb.Models;
using RightVisionBotDb.Singletons;
using RightVisionBotDb.Types;


namespace RightVisionBotDb.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext() => Database.EnsureCreated();

    public DbSet<RvUser> RvUsers => Set<RvUser>();
    public DbSet<CriticForm> CriticForms => Set<CriticForm>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=RightVision.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var locationManager = App.Container.Resolve<LocationManager>();

        //Enums
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Lang).HasConversion<string>();
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Status).HasConversion<string>();
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Role).HasConversion<string>();

        //Types
        modelBuilder.Entity<RvUser>()
            .Property(u => u.UserPermissions)
            .HasConversion(
                v => ConvertHelper.PermissionsToString(v),
                v => ConvertHelper.StringToPermissions(v)
            );
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Rewards)
            .HasConversion(
                v => ConvertHelper.RewardsToString(v),
                v => ConvertHelper.StringToRewards(v)
            );
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Punishments)
            .HasConversion(
                v => ConvertHelper.RvPunishmentsToString(v),
                v => ConvertHelper.StringToRvPunishments(v)
            );
        modelBuilder.Entity<RvUser>()
            .Property(u => u.Location).HasConversion(
            v => locationManager.LocationToString(v),
            v => locationManager.StringToLocation(v)
            );
    }
}