using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;

namespace MicFx.Core.Extensions;

/// <summary>
/// Extension methods for module seeder functionality
/// Handles data seeding across all registered modules with proper error handling
/// </summary>
public static class ModuleSeederExtensions
{
    /// <summary>
    /// Execute all registered module seeders in priority order
    /// Runs during application startup to initialize module data
    /// </summary>
    /// <param name="serviceProvider">Service provider for dependency resolution</param>
    /// <returns>Task that completes when all seeding is finished</returns>
    public static async Task RunModuleSeedersAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<string>>();
        var seeders = scope.ServiceProvider.GetServices<IModuleSeeder>();

        if (!seeders.Any())
        {
            logger.LogInformation("No module seeders found, skipping data seeding");
            return;
        }

        logger.LogInformation("🌱 Starting module data seeding for {SeederCount} modules", seeders.Count());

        // Sort by priority (lower number = higher priority, loads first)
        var sortedSeeders = seeders.OrderBy(s => s.Priority).ToList();

        foreach (var seeder in sortedSeeders)
        {
            try
            {
                logger.LogInformation("🌱 Seeding data for module: {ModuleName} (Priority: {Priority})", 
                    seeder.ModuleName, seeder.Priority);

                await seeder.SeedAsync(serviceProvider);

                logger.LogInformation("✅ Successfully seeded module: {ModuleName}", seeder.ModuleName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to seed module: {ModuleName}", seeder.ModuleName);
                
                // Continue with other seeders to prevent blocking startup
                // For production, this behavior can be configured via settings
                continue;
            }
        }

        logger.LogInformation("✅ Module data seeding completed for {CompletedCount}/{TotalCount} modules", 
            sortedSeeders.Count, seeders.Count());
    }
} 