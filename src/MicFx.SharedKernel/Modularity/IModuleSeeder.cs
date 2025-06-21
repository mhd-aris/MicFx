namespace MicFx.SharedKernel.Modularity;

/// <summary>
/// Interface untuk module data seeding
/// Memungkinkan setiap module untuk initialize data awal secara konsisten
/// </summary>
public interface IModuleSeeder
{
    /// <summary>
    /// Seed data untuk module ini
    /// Dipanggil saat aplikasi startup untuk initialize data awal
    /// </summary>
    /// <param name="serviceProvider">Service provider untuk dependency resolution</param>
    /// <returns>Task yang akan complete ketika seeding selesai</returns>
    Task SeedAsync(IServiceProvider serviceProvider);
    
    /// <summary>
    /// Priority order untuk seeding (lower number = higher priority, loads first)
    /// Berguna untuk handle dependencies antar module seeds
    /// Auth = 1, Core modules = 10, Business modules = 100
    /// </summary>
    int Priority => 100;
    
    /// <summary>
    /// Nama module yang di-seed
    /// Untuk logging dan monitoring purposes
    /// </summary>
    string ModuleName { get; }
} 