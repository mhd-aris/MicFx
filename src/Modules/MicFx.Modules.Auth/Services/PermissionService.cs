using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MicFx.Modules.Auth.Data;

namespace MicFx.Modules.Auth.Services
{
    /// <summary>
    /// Implementation of permission service dengan wildcard support dan caching
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IMemoryCache _cache;
        private readonly AuthDbContext _context;
        private readonly ILogger<PermissionService> _logger;
        
        // Cache settings
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(15);
        private const string CacheKeyPrefix = "user_permissions_";

        public PermissionService(
            IMemoryCache cache,
            AuthDbContext context,
            ILogger<PermissionService> logger)
        {
            _cache = cache;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var userId = GetUserId(user);
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            // Step 1: Check claims first (fastest path)
            if (HasPermissionInClaims(user, permission))
            {
                _logger.LogDebug("Permission {Permission} found in claims for user {UserId}", permission, userId);
                return true;
            }

            // Step 2: Check wildcard permissions in claims
            if (HasWildcardInClaims(user, permission))
            {
                _logger.LogDebug("Permission {Permission} matched wildcard in claims for user {UserId}", permission, userId);
                return true;
            }

            // Step 3: Get user permissions from cache/database
            var userPermissions = await GetUserPermissionsAsync(userId);
            var hasPermission = MatchesWildcardPattern(permission, userPermissions);

            _logger.LogDebug("Permission {Permission} checked via cache/database for user {UserId}: {Result}", 
                permission, userId, hasPermission);

            return hasPermission;
        }

        public async Task<List<string>> GetUserPermissionsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<string>();
            }

            var cacheKey = CacheKeyPrefix + userId;

            // Check memory cache first
            if (_cache.TryGetValue(cacheKey, out List<string>? cachedPermissions) && cachedPermissions != null)
            {
                _logger.LogDebug("Permissions loaded from cache for user {UserId}", userId);
                return cachedPermissions;
            }

            // Load from database
            var permissions = await LoadPermissionsFromDatabaseAsync(userId);

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheExpiry,
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, permissions, cacheOptions);

            _logger.LogInformation("Loaded {Count} permissions from database for user {UserId}", 
                permissions.Count, userId);

            return permissions;
        }

        public Task ClearUserCacheAsync(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                var cacheKey = CacheKeyPrefix + userId;
                _cache.Remove(cacheKey);
                _logger.LogInformation("Cleared permission cache for user {UserId}", userId);
            }

            return Task.CompletedTask;
        }

        public string GetFullPermissionName(string permission, string moduleName)
        {
            if (string.IsNullOrEmpty(permission) || string.IsNullOrEmpty(moduleName))
            {
                return permission ?? string.Empty;
            }

            // If permission already has module prefix, return as-is
            if (permission.Contains('.') && permission.Split('.').Length > 2)
            {
                return permission;
            }

            return $"{moduleName.ToLowerInvariant()}.{permission}";
        }

        public bool MatchesWildcardPattern(string permission, List<string> userPermissions)
        {
            if (string.IsNullOrEmpty(permission) || userPermissions == null || !userPermissions.Any())
            {
                return false;
            }

            // Check exact match first
            if (userPermissions.Contains(permission))
            {
                return true;
            }

            // Check wildcard patterns
            foreach (var userPermission in userPermissions)
            {
                if (IsWildcardMatch(permission, userPermission))
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasPermissionInClaims(ClaimsPrincipal user, string permission)
        {
            return user.HasClaim("permission", permission);
        }

        private bool HasWildcardInClaims(ClaimsPrincipal user, string permission)
        {
            // Check for global wildcard "*"
            if (user.HasClaim("permission", "*"))
            {
                return true;
            }

            // Check for entity wildcard "users.*", "roles.*", etc.
            var parts = permission.Split('.');
            if (parts.Length >= 2)
            {
                var entityWildcard = $"{parts[0]}.*";
                if (user.HasClaim("permission", entityWildcard))
                {
                    return true;
                }
            }

            // Check for module wildcard "auth.*"
            if (parts.Length >= 3)
            {
                var moduleWildcard = $"{parts[0]}.*";
                if (user.HasClaim("permission", moduleWildcard))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsWildcardMatch(string permission, string pattern)
        {
            // Global wildcard
            if (pattern == "*")
            {
                return true;
            }

            // Pattern wildcard
            if (pattern.EndsWith("*"))
            {
                var prefix = pattern[..^1]; // Remove the "*"
                return permission.StartsWith(prefix);
            }

            // Exact match
            return permission == pattern;
        }

        private async Task<List<string>> LoadPermissionsFromDatabaseAsync(string userId)
        {
            try
            {
                var permissions = await _context.Users
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.UserRoles
                        .Where(ur => ur.IsActive && ur.Role.IsActive)
                        .SelectMany(ur => ur.Role.RolePermissions
                            .Where(rp => rp.IsActive && rp.Permission.IsActive)
                            .Select(rp => rp.Permission.Name)))
                    .Distinct()
                    .ToListAsync();

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading permissions from database for user {UserId}", userId);
                return new List<string>();
            }
        }

        private static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst("user_id")?.Value ?? 
                   user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
} 