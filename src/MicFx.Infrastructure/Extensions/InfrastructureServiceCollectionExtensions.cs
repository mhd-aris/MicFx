using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MicFx.Abstractions.Logging;
using MicFx.Abstractions.Caching;
using MicFx.Infrastructure.Logging;

namespace MicFx.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register Infrastructure implementations
/// These extensions replace the default throw-exception implementations with real ones
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers Infrastructure logging implementations
    /// Replaces the default implementations from Abstractions with real Serilog-based ones
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMicFxInfrastructureLogging(this IServiceCollection services)
    {
        // Replace default implementations with real ones
        services.Replace(ServiceDescriptor.Singleton<IStructuredLoggerFactory, StructuredLoggerFactory>());
        
        // Register generic IStructuredLogger<T> using the factory
        services.AddTransient(typeof(IStructuredLogger<>), typeof(StructuredLoggerImplementation<>));
        
        return services;
    }

    /// <summary>
    /// Registers Infrastructure caching implementations
    /// Replaces the default implementations from Abstractions with real cache implementations
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMicFxInfrastructureCaching(this IServiceCollection services)
    {
        // Replace default implementations with real ones
        // TODO: Implement actual caching service when ready
        // services.Replace(ServiceDescriptor.Singleton<ICacheService, RedisCacheService>());
        
        return services;
    }

    /// <summary>
    /// Registers all Infrastructure implementations
    /// This is typically called by the main application to set up all infrastructure services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMicFxInfrastructure(this IServiceCollection services)
    {
        services.AddMicFxInfrastructureLogging();
        services.AddMicFxInfrastructureCaching();
        
        return services;
    }

    /// <summary>
    /// Extension method for easy registration of structured logger in module services
    /// This allows modules to get structured loggers without referencing Infrastructure directly
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddStructuredLogger<T>(this IServiceCollection services)
    {
        services.AddTransient<IStructuredLogger<T>>();
        return services;
    }
} 