using Microsoft.Extensions.DependencyInjection;
using MicFx.Abstractions.Logging;
using MicFx.Abstractions.Caching;
using MicFx.Abstractions.Security;

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
    /// Adds security interfaces to the service collection
    /// Note: The actual implementation will be registered by Infrastructure layer
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMicFxSecurityAbstractions(this IServiceCollection services)
    {
        // Register security interfaces
        // Implementation will be provided by MicFx.Infrastructure
        services.AddTransient<ISecurityService, DefaultSecurityService>();
        
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
        services.AddMicFxSecurityAbstractions();
        
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

    public Task SetAsync<T>(string key, T value, CacheOptions options, CancellationToken cancellationToken = default) where T : class
    {
        throw new NotImplementedException("Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        throw new NotImplementedException("Caching implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }
}

internal class DefaultSecurityService : ISecurityService
{
    public Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Security implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task<AuthorizationResult> CheckPermissionsAsync(string userId, string[] permissions, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Security implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task LogSecurityEventAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Security implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task<string> EncryptAsync(string data, string? keyId = null)
    {
        throw new NotImplementedException("Security implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public Task<string> DecryptAsync(string encryptedData, string? keyId = null)
    {
        throw new NotImplementedException("Security implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public string GenerateHash(string data, string? salt = null)
    {
        throw new NotImplementedException("Security implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }

    public bool VerifyHash(string data, string hash, string? salt = null)
    {
        throw new NotImplementedException("Security implementation not registered. Please ensure MicFx.Infrastructure is properly configured.");
    }
} 