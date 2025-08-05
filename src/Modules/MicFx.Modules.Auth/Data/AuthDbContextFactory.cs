using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MicFx.Modules.Auth.Data;

/// <summary>
/// Design-time factory for AuthDbContext to enable EF Core migrations
/// </summary>
public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        try
        {
            
            var configuration = BuildConfiguration();
            
            var connectionString = GetConnectionString(configuration);
            
            // Configure DbContext
            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseSqlServer(connectionString, options =>
            {
                options.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName);
            });

            var context = new AuthDbContext(optionsBuilder.Options);
            
            return context;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            return new ConfigurationBuilder()
                .SetBasePath(currentDir)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        throw new DirectoryNotFoundException(
            $"Web project not found. Current directory: {currentDir}. " +
            "Expected appsettings.json in current directory.");
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var configConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(configConnection))
            return configConnection;

        throw new InvalidOperationException(
            "No connection string found. Please ensure:\n" +
            "1. DefaultConnection exists in appsettings.json\n" );
    }
} 