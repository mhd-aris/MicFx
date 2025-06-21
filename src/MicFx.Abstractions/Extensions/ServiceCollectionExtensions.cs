using Microsoft.Extensions.DependencyInjection;
using MicFx.Abstractions.Logging;
using MicFx.Abstractions.Caching;

namespace MicFx.Abstractions.Extensions;

/// <summary>
/// Extension methods for IServiceCollection to register MicFx abstractions
/// These extensions are safe to use in modules as they only register interfaces
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds structured logging interfaces to the service collection
    /// Note: The actual implementation will be registered by Infrastructure layer
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMicFxLoggingAbstractions(this IServiceCollection services)
    {
        // Register structured logging interfaces
        // Implementation will be provided by MicFx.Infrastructure
        services.AddTransient<IStructuredLoggerFactory, DefaultStructuredLoggerFactory>();
        
        return services;
    }

    /// <summary>
    /// Adds caching interfaces to the service collection
    /// Note: The actual implementation will be registered by Infrastructure layer
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMicFxCachingAbstractions(this IServiceCollection services)
    {
        // Register caching interfaces
        // Implementation will be provided by MicFx.Infrastructure
        services.AddTransient<ICacheService, DefaultCacheService>();
        
        return services;
    }

    /// <summary>
    /// Adds all MicFx abstractions to the service collection
    /// This is a convenience method for modules that need multiple abstractions
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMicFxAbstractions(this IServiceCollection services)
    {
        services.AddMicFxLoggingAbstractions();
        services.AddMicFxCachingAbstractions();
        
        return services;
    }
}

// Default implementations that throw NotImplementedException
// These will be replaced by actual implementations from Infrastructure layer
internal class DefaultStructuredLoggerFactory : IStructuredLoggerFactory
{
    public IStructuredLogger<T> CreateLogger<T>()
    {
        throw new NotImplementedException("Structured logging implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public IStructuredLogger CreateLogger(string categoryName)
    {
        throw new NotImplementedException("Structured logging implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }
}

internal class DefaultCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        throw new NotImplementedException("Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        throw new NotImplementedException("Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }
} 