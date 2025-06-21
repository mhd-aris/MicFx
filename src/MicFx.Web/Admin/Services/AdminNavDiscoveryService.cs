using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Interfaces;

namespace MicFx.Web.Admin.Services
{
    /// <summary>
    /// Simplified service for collecting admin navigation items from modules
    /// </summary>
    public class AdminNavDiscoveryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AdminNavDiscoveryService> _logger;

        public AdminNavDiscoveryService(
            IServiceProvider serviceProvider,
            ILogger<AdminNavDiscoveryService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Gets all navigation items from registered contributors
        /// </summary>
        /// <returns>Collection of navigation items sorted by order</returns>
        public Task<IEnumerable<AdminNavItem>> GetNavigationItemsAsync()
        {
            try
            {
                var contributors = _serviceProvider.GetServices<IAdminNavContributor>();
                var allNavItems = new List<AdminNavItem>();

                foreach (var contributor in contributors)
                {
                    try
                    {
                        var navItems = contributor.GetNavItems();
                        if (navItems != null)
                        {
                            allNavItems.AddRange(navItems.Where(item => item.IsActive));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting navigation items from contributor {ContributorType}", 
                            contributor.GetType().Name);
                    }
                }

                // Sort by Order, then by Title
                var sortedItems = allNavItems
                    .OrderBy(x => x.Order)
                    .ThenBy(x => x.Title)
                    .ToList();

                _logger.LogDebug("Retrieved {ItemCount} navigation items", sortedItems.Count);
                
                return Task.FromResult<IEnumerable<AdminNavItem>>(sortedItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering admin navigation items");
                return Task.FromResult(Enumerable.Empty<AdminNavItem>());
            }
        }
    }
} 