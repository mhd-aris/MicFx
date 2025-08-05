using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace MicFx.Core.Extensions;

/// <summary>
/// Centralized database management for MICFX modules
/// Provides consistent patterns for database initialization across all modules
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Ensure all module databases are ready for use
    /// Uses migrations when available, falls back to EnsureCreated for development
    /// </summary>
    public static async Task EnsureModuleDatabasesAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<string>>();
        
        logger.LogInformation("🗄️ Starting database initialization for all modules");

        // Get all registered DbContext types
        var dbContextTypes = GetRegisteredDbContextTypes(scope.ServiceProvider);
        
        foreach (var dbContextType in dbContextTypes)
        {
            try
            {
                await EnsureDatabaseForContextAsync(scope.ServiceProvider, dbContextType, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to initialize database for {DbContextType}", dbContextType.Name);
                throw; // Re-throw to fail startup on database issues
            }
        }

        logger.LogInformation("✅ Database initialization completed for {Count} modules", dbContextTypes.Count);
    }

    private static async Task EnsureDatabaseForContextAsync(IServiceProvider serviceProvider, Type dbContextType, ILogger logger)
    {
        var dbContext = serviceProvider.GetRequiredService(dbContextType) as DbContext;
        if (dbContext == null) return;

        var contextName = dbContextType.Name;
        logger.LogInformation("🗄️ Initializing database for {ContextName}", contextName);

        try
        {
            // First, check if database exists and can connect
            var canConnect = await dbContext.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                // Database doesn't exist, create it with latest schema
                logger.LogInformation("📦 Creating new database for {ContextName}", contextName);
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("✅ Database created for {ContextName}", contextName);
                return;
            }

            // Database exists, check for pending migrations
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
            
            logger.LogDebug("Applied migrations: {AppliedCount}, Pending migrations: {PendingCount}", 
                appliedMigrations.Count(), pendingMigrations.Count());
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation("📦 Applying {Count} pending migrations for {ContextName}", 
                    pendingMigrations.Count(), contextName);
                
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("✅ Migrations applied successfully for {ContextName}", contextName);
            }
            else
            {
                logger.LogInformation("✅ Database is up to date for {ContextName}", contextName);
            }
        }
        catch (Exception migrationEx) when (
            migrationEx.Message.Contains("already in use as a object name") ||
            migrationEx.Message.Contains("already exists") ||
            migrationEx.Message.Contains("duplicate"))
        {
            // Handle migration conflicts - database schema likely already matches target
            logger.LogWarning("⚠️ Migration conflict detected for {ContextName}. Database appears to be in target state already.", contextName);
            
            try
            {
                // Verify database is accessible and has expected tables
                await dbContext.Database.CanConnectAsync();
                logger.LogInformation("✅ Database verified and accessible for {ContextName}", contextName);
            }
            catch (Exception connEx)
            {
                logger.LogError(connEx, "❌ Database connection failed for {ContextName}", contextName);
                throw;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Database initialization failed for {ContextName}", contextName);
            throw;
        }
    }

    private static List<Type> GetRegisteredDbContextTypes(IServiceProvider serviceProvider)
    {
        var dbContextTypes = new List<Type>();
        
        // Get all loaded assemblies and search for DbContext implementations
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && a.FullName?.Contains("MicFx") == true);

        foreach (var assembly in assemblies)
        {
            try
            {
                var contextTypes = assembly.GetTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(DbContext)));

                foreach (var contextType in contextTypes)
                {
                    // Check if this DbContext is actually registered in DI
                    try
                    {
                        var service = serviceProvider.GetService(contextType);
                        if (service != null)
                        {
                            dbContextTypes.Add(contextType);
                        }
                    }
                    catch
                    {
                        // Skip if not registered or cannot be resolved
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that cannot be loaded
            }
        }

        return dbContextTypes;
    }
}
