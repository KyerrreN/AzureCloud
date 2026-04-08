using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MusicMigrater.DAL.Entities;
using System.Reflection;

namespace MusicMigrater.DAL.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options)
        : base(options) { }

    public DbSet<AudioEntity> Audios { get; set; }

    public DbSet<UserSettingsEntity> UserSettings { get; set; }

    public DbSet<SyncLogsEntity> SyncLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
