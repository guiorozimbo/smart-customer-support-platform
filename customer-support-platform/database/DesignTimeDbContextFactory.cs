using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Database;

/// <summary>
/// Used by EF Core tools (e.g. dotnet ef migrations) regardless of which project is the startup project.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var databaseDir = FindDatabaseDirectory();
        var config = new ConfigurationBuilder()
            .SetBasePath(databaseDir)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var conn = config.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=CustomerSupportPlatform;Trusted_Connection=True;MultipleActiveResultSets=true";
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(conn);

        return new AppDbContext(optionsBuilder.Options);
    }

    /// <summary>Walks up from <see cref="Directory.GetCurrentDirectory"/> until <c>database/appsettings.json</c> exists.</summary>
    private static string FindDatabaseDirectory()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "database", "appsettings.json");
            if (File.Exists(candidate))
                return Path.Combine(dir.FullName, "database");
            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate database/appsettings.json. Run EF commands from the repository root (folder that contains the 'database' project).");
    }
}
