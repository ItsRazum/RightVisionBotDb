using Microsoft.EntityFrameworkCore;
using RightVisionBotDb.Data.Configurations;
using RightVisionBotDb.Interfaces;
using RightVisionBotDb.Models;


namespace RightVisionBotDb.Data.Contexts;

public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext() => Database.EnsureCreated();

    public DbSet<RvUser> RvUsers => Set<RvUser>();
    public DbSet<CriticForm> CriticForms => Set<CriticForm>();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("Data Source=RightVision.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RvUserEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new CriticFormEntityTypeConfiguration());
    }
}