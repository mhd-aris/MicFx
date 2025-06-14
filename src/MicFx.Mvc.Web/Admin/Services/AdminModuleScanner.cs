using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Interfaces;

namespace MicFx.Mvc.Web.Admin.Services
{
    /// <summary>
    /// Service for automatically discovering and registering admin navigation contributors from modules
    /// </summary>
    public class AdminModuleScanner
    {
        private readonly ILogger<AdminModuleScanner> _logger;

        public AdminModuleScanner(ILogger<AdminModuleScanner> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Scans all loaded assemblies for IAdminNavContributor implementations
        /// </summary>
        /// <param name="services">Service collection to register discovered contributors</param>
        /// <returns>Number of contributors discovered and registered</returns>
        public int ScanAndRegisterContributors(IServiceCollection services)
        {
            var contributorsFound = 0;
            var assemblies = GetMicFxModuleAssemblies();

            _logger.LogInformation("🔍 Scanning {AssemblyCount} MicFx module assemblies for admin navigation contributors", assemblies.Count);

            foreach (var assembly in assemblies)
            {
                try
                {
                    var contributors = ScanAssemblyForContributors(assembly);
                    
                    foreach (var contributorType in contributors)
                    {
                        services.AddTransient(typeof(IAdminNavContributor), contributorType);
                        contributorsFound++;
                        
                        _logger.LogInformation("✅ Registered admin navigation contributor: {ContributorType} from {AssemblyName}", 
                            contributorType.Name, assembly.GetName().Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to scan assembly {AssemblyName} for admin navigation contributors", 
                        assembly.GetName().Name);
                }
            }

            _logger.LogInformation("🎯 Auto-discovery completed: {ContributorsFound} admin navigation contributors registered", contributorsFound);
            return contributorsFound;
        }

        /// <summary>
        /// Gets all loaded assemblies that match MicFx module pattern
        /// </summary>
        private List<Assembly> GetMicFxModuleAssemblies()
        {
            var assemblies = new List<Assembly>();
            
            // Get all loaded assemblies
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in loadedAssemblies)
            {
                var assemblyName = assembly.GetName().Name;
                
                // Include MicFx module assemblies
                if (assemblyName != null && (
                    assemblyName.StartsWith("MicFx.Modules.", StringComparison.OrdinalIgnoreCase) ||
                    assemblyName.StartsWith("MicFx.Mvc.Web", StringComparison.OrdinalIgnoreCase)))
                {
                    assemblies.Add(assembly);
                    _logger.LogDebug("📦 Found MicFx module assembly: {AssemblyName}", assemblyName);
                }
            }

            return assemblies;
        }

        /// <summary>
        /// Scans a specific assembly for IAdminNavContributor implementations
        /// </summary>
        private IEnumerable<Type> ScanAssemblyForContributors(Assembly assembly)
        {
            var contributors = new List<Type>();

            try
            {
                var types = assembly.GetTypes();
                
                foreach (var type in types)
                {
                    // Check if type implements IAdminNavContributor
                    if (typeof(IAdminNavContributor).IsAssignableFrom(type) && 
                        !type.IsInterface && 
                        !type.IsAbstract &&
                        type.IsClass)
                    {
                        contributors.Add(type);
                        _logger.LogDebug("🔍 Found admin navigation contributor: {TypeName} in {AssemblyName}", 
                            type.FullName, assembly.GetName().Name);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                _logger.LogWarning("⚠️ ReflectionTypeLoadException in assembly {AssemblyName}: {Message}", 
                    assembly.GetName().Name, ex.Message);
                
                // Try to get the types that were successfully loaded
                var loadedTypes = ex.Types.Where(t => t != null);
                foreach (var type in loadedTypes)
                {
                    if (typeof(IAdminNavContributor).IsAssignableFrom(type) && 
                        !type!.IsInterface && 
                        !type.IsAbstract &&
                        type.IsClass)
                    {
                        contributors.Add(type);
                    }
                }
            }

            return contributors;
        }

        /// <summary>
        /// Gets detailed information about discovered contributors for diagnostics
        /// </summary>
        public AdminScanResult GetScanResults()
        {
            var assemblies = GetMicFxModuleAssemblies();
            var result = new AdminScanResult
            {
                ScannedAssemblies = assemblies.Count,
                AssemblyNames = assemblies.Select(a => a.GetName().Name ?? "Unknown").ToList(),
                Contributors = new List<ContributorInfo>()
            };

            foreach (var assembly in assemblies)
            {
                try
                {
                    var contributors = ScanAssemblyForContributors(assembly);
                    foreach (var contributorType in contributors)
                    {
                        result.Contributors.Add(new ContributorInfo
                        {
                            TypeName = contributorType.FullName ?? contributorType.Name,
                            AssemblyName = assembly.GetName().Name ?? "Unknown",
                            Namespace = contributorType.Namespace ?? "Unknown"
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get scan results for assembly {AssemblyName}", 
                        assembly.GetName().Name);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Result of admin module scanning operation
    /// </summary>
    public class AdminScanResult
    {
        public int ScannedAssemblies { get; set; }
        public List<string> AssemblyNames { get; set; } = new();
        public List<ContributorInfo> Contributors { get; set; } = new();
    }

    /// <summary>
    /// Information about a discovered admin navigation contributor
    /// </summary>
    public class ContributorInfo
    {
        public string TypeName { get; set; } = string.Empty;
        public string AssemblyName { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
    }
} 