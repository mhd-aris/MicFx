using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Modularity;

namespace MicFx.Modules.TEMPLATE_NAME.Data;

/// <summary>
/// Data seeder untuk TEMPLATE_NAME module
/// Initialize default data untuk development/demo purposes
/// </summary>
public class TEMPLATE_NAMEModuleSeeder : IModuleSeeder
{
    public string ModuleName => "TEMPLATE_NAME";
    public int Priority => 100; // Standard priority (Auth=1, Core=10, Business=100)

    public async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TEMPLATE_NAMEModuleSeeder>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TEMPLATE_NAMEDbContext>();

        logger.LogInformation("🌱 Starting {ModuleName} module data seeding...", ModuleName);

        try
        {
            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync();

            // Add your seeding logic here
            await SeedDefaultDataAsync(dbContext, logger);

            // Save changes
            await dbContext.SaveChangesAsync();

            logger.LogInformation("✅ {ModuleName} module data seeding completed successfully", ModuleName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during {ModuleName} module data seeding", ModuleName);
            throw;
        }
    }

    private static async Task SeedDefaultDataAsync(TEMPLATE_NAMEDbContext dbContext, ILogger logger)
    {
        // Example seeding logic - customize based on your entities
        
        // Check if data already exists to avoid duplicates
        // if (!await dbContext.YourEntities.AnyAsync())
        // {
        //     var defaultEntity = new YourEntity
        //     {
        //         Name = "Default Item",
        //         CreatedBy = "TEMPLATE_NAMEModuleSeeder"
        //     };
        //     
        //     dbContext.YourEntities.Add(defaultEntity);
        //     logger.LogInformation("✅ Created default entity: {EntityName}", defaultEntity.Name);
        // }
        
        await Task.CompletedTask; // Remove this when you add actual seeding logic
    }
} 