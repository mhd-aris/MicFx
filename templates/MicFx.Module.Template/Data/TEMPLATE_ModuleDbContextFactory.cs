using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MicFx.Modules.TEMPLATE_NAME.Data;

/// <summary>
/// Design-time factory untuk TEMPLATE_NAMEDbContext untuk enable EF Core migrations
/// Memungkinkan CLI commands seperti: dotnet ef migrations add --project ./Modules/MicFx.Modules.TEMPLATE_NAME
/// </summary>
public class TEMPLATE_NAMEDbContextFactory : IDesignTimeDbContextFactory<TEMPLATE_NAMEDbContext>
{
    public TEMPLATE_NAMEDbContext CreateDbContext(string[] args)
    {
        // Build configuration dari host application
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "MicFx.Web"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        // Get shared connection string dari host configuration
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration. Ensure the host app has proper database configuration.");
        }

        // Configure DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<TEMPLATE_NAMEDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TEMPLATE_NAMEDbContext(optionsBuilder.Options);
    }
} 