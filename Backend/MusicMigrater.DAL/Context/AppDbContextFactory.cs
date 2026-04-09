using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MusicMigrater.DAL.Context;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var projectRoot = currentDirectory.EndsWith("MusicMigrater.DAL")
            ? Directory.GetParent(currentDirectory)!.FullName
            : currentDirectory;

        string apiProjectPath = Path.Combine(projectRoot, "MusicMigrater");

        var configPath = Path.Combine(apiProjectPath, "appsettings.json");
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Couldn't find config in {configPath}");
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        var connectionString = configuration.GetConnectionString("AzureDefaultConnection");

        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
