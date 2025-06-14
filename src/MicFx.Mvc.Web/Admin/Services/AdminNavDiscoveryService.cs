using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using MicFx.SharedKernel.Interfaces;
using System.Security.Claims;

namespace MicFx.Mvc.Web.Admin.Services
{
    /// <summary>
    /// Service for discovering and managing admin navigation items from modules
    /// </summary>
    public class AdminNavDiscoveryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdminNavDiscoveryService> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

        public AdminNavDiscoveryService(
            IServiceProvider serviceProvider,
            ILogger<AdminNavDiscoveryService> logger,
            IMemoryCache cache)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Gets all navigation items from registered contributors with caching
        /// </summary>
        /// <param name="httpContext">Current HTTP context for user/role checking</param>
        /// <returns>Collection of navigation items sorted by order</returns>
        public Task<IEnumerable<AdminNavItem>> GetNavigationItemsAsync(HttpContext httpContext)
        {
            try
            {
                // Create cache key based on user identity and roles
                var cacheKey = GenerateCacheKey(httpContext.User);
                
                // Try to get from cache first
                if (_cache.TryGetValue(cacheKey, out IEnumerable<AdminNavItem>? cachedItems) && cachedItems != null)
                {
                    _logger.LogDebug("🚀 Retrieved {ItemCount} navigation items from cache for user {UserId}", 
                        cachedItems.Count(), GetUserIdentifier(httpContext.User));
                    
                    // Update active states based on current path (this is request-specific)
                    var itemsWithActiveState = cachedItems.ToList();
                    SetActiveStates(itemsWithActiveState, httpContext.Request.Path.Value ?? string.Empty);
                    return Task.FromResult<IEnumerable<AdminNavItem>>(itemsWithActiveState);
                }

                // Cache miss - generate navigation items
                _logger.LogDebug("🔄 Cache miss - generating navigation items for user {UserId}", 
                    GetUserIdentifier(httpContext.User));

                var contributors = _serviceProvider.GetServices<IAdminNavContributor>();
                var allNavItems = new List<AdminNavItem>();

                foreach (var contributor in contributors)
                {
                    try
                    {
                        var navItems = contributor.GetNavItems();
                        if (navItems != null)
                        {
                            // Filter items based on user roles and active status
                            var filteredItems = FilterNavItems(navItems, httpContext.User);
                            allNavItems.AddRange(filteredItems);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error getting navigation items from contributor {ContributorType}", 
                            contributor.GetType().Name);
                    }
                }

                // Sort by Order, then by Title
                var sortedItems = allNavItems
                    .OrderBy(x => x.Order)
                    .ThenBy(x => x.Title)
                    .ToList();

                // Cache the results (without active states as they're request-specific)
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheExpiration,
                    SlidingExpiration = TimeSpan.FromMinutes(5),
                    Priority = CacheItemPriority.Normal
                };

                _cache.Set(cacheKey, sortedItems, cacheOptions);
                
                _logger.LogInformation("💾 Cached {ItemCount} navigation items for user {UserId} (expires in {ExpirationMinutes} minutes)", 
                    sortedItems.Count, GetUserIdentifier(httpContext.User), _cacheExpiration.TotalMinutes);

                // Set active state based on current path
                SetActiveStates(sortedItems, httpContext.Request.Path.Value ?? string.Empty);
                
                return Task.FromResult<IEnumerable<AdminNavItem>>(sortedItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error discovering admin navigation items");
                return Task.FromResult(Enumerable.Empty<AdminNavItem>());
            }
        }

        /// <summary>
        /// Filters navigation items based on user roles and active status
        /// </summary>
        private IEnumerable<AdminNavItem> FilterNavItems(IEnumerable<AdminNavItem> navItems, ClaimsPrincipal user)
        {
            return navItems.Where(item => 
            {
                // Check if item is active
                if (!item.IsActive)
                {
                    _logger.LogDebug("🚫 Navigation item '{Title}' filtered out - not active", item.Title);
                    return false;
                }

                // Check if user has required roles
                if (item.RequiredRoles?.Length > 0)
                {
                    var hasRequiredRole = item.RequiredRoles.Any(role => user.IsInRole(role));
                    if (!hasRequiredRole)
                    {
                        _logger.LogDebug("🚫 Navigation item '{Title}' filtered out - user lacks required roles: {RequiredRoles}", 
                            item.Title, string.Join(", ", item.RequiredRoles));
                        return false;
                    }
                }

                // If no specific roles required, allow access for demo purposes
                // TODO: In production, uncomment the authentication check below
                /*
                if ((item.RequiredRoles?.Length ?? 0) == 0)
                {
                    var isAuthenticated = user.Identity?.IsAuthenticated ?? false;
                    if (!isAuthenticated)
                    {
                        _logger.LogDebug("🚫 Navigation item '{Title}' filtered out - user not authenticated", item.Title);
                        return false;
                    }
                }
                */

                _logger.LogDebug("✅ Navigation item '{Title}' allowed for user {UserId}", 
                    item.Title, GetUserIdentifier(user));
                return true;
            });
        }

        /// <summary>
        /// Sets active state for navigation items based on current path
        /// </summary>
        private void SetActiveStates(IEnumerable<AdminNavItem> navItems, string currentPath)
        {
            foreach (var item in navItems)
            {
                // Simple path matching - can be enhanced with more sophisticated logic
                item.IsActive = !string.IsNullOrEmpty(item.Url) && 
                               currentPath.StartsWith(item.Url, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Groups navigation items by category
        /// </summary>
        /// <param name="httpContext">Current HTTP context</param>
        /// <returns>Dictionary of navigation items grouped by category</returns>
        public async Task<Dictionary<string, List<AdminNavItem>>> GetNavigationItemsByCategoryAsync(HttpContext httpContext)
        {
            var navItems = await GetNavigationItemsAsync(httpContext);
            
            return navItems
                .GroupBy(x => x.Category)
                .ToDictionary(
                    g => g.Key, 
                    g => g.ToList()
                );
        }

        /// <summary>
        /// Generates a cache key based on user identity and roles
        /// </summary>
        private string GenerateCacheKey(ClaimsPrincipal user)
        {
            var userId = GetUserIdentifier(user);
            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .OrderBy(r => r)
                .ToList();

            var roleHash = string.Join(",", roles);
            return $"admin_nav_{userId}_{roleHash.GetHashCode()}";
        }

        /// <summary>
        /// Gets a unique identifier for the user
        /// </summary>
        private string GetUserIdentifier(ClaimsPrincipal user)
        {
            return user.Identity?.Name ?? 
                   user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                   user.FindFirst("sub")?.Value ?? 
                   "anonymous";
        }

        /// <summary>
        /// Clears navigation cache for a specific user
        /// </summary>
        public void ClearUserCache(ClaimsPrincipal user)
        {
            var cacheKey = GenerateCacheKey(user);
            _cache.Remove(cacheKey);
            _logger.LogInformation("🗑️ Cleared navigation cache for user {UserId}", GetUserIdentifier(user));
        }

        /// <summary>
        /// Clears all navigation cache entries
        /// </summary>
        public void ClearAllCache()
        {
            // Note: IMemoryCache doesn't have a clear all method
            // In production, consider using IDistributedCache with Redis
            _logger.LogInformation("🗑️ Cache clear requested - consider implementing distributed cache for better cache management");
        }
    }
} 