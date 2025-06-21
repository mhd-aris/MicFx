using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using System.Linq;
using MicFx.SharedKernel.Modularity;
using MicFx.Core.Extensions;
using MicFx.SharedKernel.Common.Exceptions;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Controller type classification for auto-routing
    /// </summary>
    public enum ControllerType
    {
        Api,        // API controllers -> /api/[module]/[controller]
        Mvc,        // MVC controllers -> /[module]/[controller]/[action]
        Admin       // Admin controllers -> /admin/[module]/[controller]/[action]
    }

    /// <summary>
    /// Base class for module startup that provides automatic controller discovery and mapping,
    /// with comprehensive auto-routing support for API, MVC, and Admin patterns
    /// </summary>
    public abstract class ModuleStartupBase : IMicFxModule, IModuleLifecycle
    {
        private readonly ILogger<ModuleStartupBase>? _logger;

        public ModuleStartupBase(ILogger<ModuleStartupBase>? logger = null)
        {
            _logger = logger;
        }

        public abstract IModuleManifest Manifest { get; }

        // For backward compatibility with IMicFxModule
        public ModuleInfo ModuleInfo => new ModuleInfo
        {
            Name = Manifest.Name,
            Version = Manifest.Version,
            Description = Manifest.Description,
            Author = Manifest.Author
        };

        /// <summary>
        /// Configures services for the module. Override ConfigureModuleServices for custom services.
        /// </summary>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Auto-register all controllers in this module
            AutoRegisterControllers(services);

            // Configure module configuration first
            ConfigureModuleConfiguration(services);

            // Allow derived classes to add custom services
            ConfigureModuleServices(services);
        }

        /// <summary>
        /// Configures endpoints for the module. Override ConfigureModuleEndpoints for custom endpoints.
        /// </summary>
        public virtual void Configure(IEndpointRouteBuilder endpoints)
        {
            // Auto-discover and map all controllers in this module with smart routing
            AutoMapControllersWithSmartRouting(endpoints);

            // Allow derived classes to add custom endpoints
            ConfigureModuleEndpoints(endpoints);
        }

        /// <summary>
        /// Override this method to register custom services for your module
        /// </summary>
        protected virtual void ConfigureModuleServices(IServiceCollection services)
        {
            // Default implementation does nothing
        }

        /// <summary>
        /// Override this method to configure module-specific configuration
        /// </summary>
        protected virtual void ConfigureModuleConfiguration(IServiceCollection services)
        {
            // Default implementation does nothing
            // Override this to register module configurations
        }

        /// <summary>
        /// Helper method to create business exception with module context
        /// </summary>
        protected BusinessException CreateBusinessException(string message, string errorCode = "BUSINESS_ERROR")
        {
            return ModuleExceptionExtensions.CreateBusinessException(Manifest.Name, message, errorCode);
        }

        /// <summary>
        /// Helper method to create validation exception with module context
        /// </summary>
        protected ValidationException CreateValidationException(string message, List<ValidationError> validationErrors, string errorCode = "VALIDATION_ERROR")
        {
            return ModuleExceptionExtensions.CreateValidationException(Manifest.Name, message, validationErrors, errorCode);
        }

        /// <summary>
        /// Helper method to create module exception
        /// </summary>
        protected ModuleException CreateModuleException(string message, string errorCode = "MODULE_ERROR")
        {
            return ModuleExceptionExtensions.CreateModuleException(Manifest.Name, message, errorCode);
        }

        /// <summary>
        /// Helper method to create security exception with module context
        /// </summary>
        protected SecurityException CreateSecurityException(string message, string errorCode = "SECURITY_ERROR")
        {
            return ModuleExceptionExtensions.CreateSecurityException(Manifest.Name, message, errorCode);
        }

        /// <summary>
        /// Override this method to add custom endpoints for your module
        /// </summary>
        protected virtual void ConfigureModuleEndpoints(IEndpointRouteBuilder endpoints)
        {
            // Default implementation does nothing
        }

        private void AutoRegisterControllers(IServiceCollection services)
        {
            var moduleAssembly = this.GetType().Assembly;

            // Find all controllers in this module (both API and MVC)
            var controllerTypes = moduleAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Controller") && 
                           (t.IsSubclassOf(typeof(Controller)) || t.IsSubclassOf(typeof(ControllerBase))))
                .ToList();

            // Register controllers in DI container
            foreach (var controllerType in controllerTypes)
            {
                services.AddTransient(controllerType);
            }

            _logger?.LogInformation("🔧 Auto-registered {ControllerCount} controllers for module {ModuleName}: {Controllers}",
                controllerTypes.Count, Manifest.Name, string.Join(", ", controllerTypes.Select(c => c.Name)));
        }

        /// <summary>
        /// Auto-map controllers with smart routing patterns
        /// Maps different controller types to appropriate route patterns
        /// </summary>
        private void AutoMapControllersWithSmartRouting(IEndpointRouteBuilder endpoints)
        {
            var assembly = GetType().Assembly;
            var moduleNamespace = assembly.GetName().Name ?? "Unknown";
            var moduleName = ExtractModuleNameFromAssembly(moduleNamespace);

            _logger?.LogInformation("[MicFx.Core] Auto-mapping controllers for module {ModuleName}", moduleName);

            var controllerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                           (t.IsSubclassOf(typeof(Controller)) || t.IsSubclassOf(typeof(ControllerBase))) &&
                           t.Name.EndsWith("Controller"))
                .ToList();

            foreach (var controllerType in controllerTypes)
            {
                var detectedType = DetermineControllerType(controllerType);
                var controllerName = controllerType.Name.Replace("Controller", "");

                // Skip conventional routing for API controllers as they use attribute routing
                if (detectedType == ControllerType.Api)
                {
                    _logger?.LogInformation("[MicFx.Core] - Controller: {ControllerName} -> Type: {DetectedType} (Using Attribute Routing)",
                        controllerName, detectedType);
                    continue;
                }

                var routePattern = BuildSmartRoutePatternForController(moduleName, controllerName, detectedType);
                var routeName = $"{moduleName}_{detectedType}_{controllerName}";

                try
                {
                    endpoints.MapControllerRoute(
                        name: routeName,
                        pattern: routePattern,
                        defaults: new { controller = controllerName }
                    );

                    _logger?.LogInformation("[MicFx.Core] - Controller: {ControllerName} -> Type: {DetectedType} -> Route: {RoutePattern}",
                        controllerName, detectedType, routePattern);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "[MicFx.Core] Failed to map route for controller {ControllerName}", controllerName);
                }
            }

            _logger?.LogInformation("[MicFx.Core] Controller mapping completed for module {ModuleName}", moduleName);
        }

        /// <summary>
        /// Determines controller type based on folder structure and naming conventions
        /// </summary>
        private ControllerType DetermineControllerType(Type controllerType)
        {
            var controllerName = controllerType.Name.Replace("Controller", "");
            var namespaceName = controllerType.Namespace ?? "";
            
            // 1. Check folder structure - API controllers should be in Api namespace/folder
            if (namespaceName.Contains(".Api"))
            {
                return ControllerType.Api;
            }
            
            // 2. Check for explicit attributes
            if (controllerType.GetCustomAttribute<ApiControllerAttribute>() != null)
            {
                return ControllerType.Api;
            }

            // 3. Check Area-based Admin controllers - prioritas utama untuk Areas/Admin pattern
            if (namespaceName.Contains(".Areas.Admin") || namespaceName.Contains(".Areas.Admin.Controllers"))
            {
                return ControllerType.Admin;
            }

            // 4. Legacy support - Check naming conventions for Admin (untuk backward compatibility)
            if (controllerName.StartsWith("Admin", StringComparison.OrdinalIgnoreCase) ||
                controllerName.EndsWith("Admin", StringComparison.OrdinalIgnoreCase) ||
                namespaceName.Contains(".Admin"))
            {
                return ControllerType.Admin;
            }

            // 5. Check base class - ControllerBase suggests API, Controller suggests MVC
            if (controllerType.IsSubclassOf(typeof(ControllerBase)) && 
                !controllerType.IsSubclassOf(typeof(Controller)))
            {
                return ControllerType.Api;
            }

            // Default to MVC for Controller base class
            return ControllerType.Mvc;
        }

        /// <summary>
        /// Builds smart route pattern based on controller type and conventions
        /// </summary>
        private (string routePattern, string routeName) BuildSmartRoutePattern(
            string moduleName, string controllerName, string actionName, 
            ControllerType controllerType, string originalActionName)
        {
            string routePattern;
            string routeName;

            // Clean controller name by removing common suffixes and comparing with module name
            var cleanControllerName = controllerName
                .Replace("-api", "")  // Remove "-api" suffix
                .Replace("-admin", "") // Remove "-admin" suffix
                .Replace("-controller", ""); // Remove "-controller" suffix

            // Check if controller name (after cleaning) is same as module name
            // For example: HelloWorldController in HelloWorld module should use just module name
            var normalizedController = cleanControllerName.Replace("-", "").ToLowerInvariant();
            var normalizedModule = moduleName.Replace("-", "").ToLowerInvariant();
            
            if (normalizedController == normalizedModule || 
                normalizedController == normalizedModule + "controller")
            {
                cleanControllerName = "";
            }

            switch (controllerType)
            {
                case ControllerType.Api:
                    // 🧩 API JSON: /api/[modulename]/[controller]/[action] → /api/auth/login
                    if (originalActionName.Equals("Get", StringComparison.OrdinalIgnoreCase) ||
                        originalActionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
                    {
                        routePattern = string.IsNullOrEmpty(cleanControllerName) 
                            ? $"api/{moduleName}"
                            : $"api/{moduleName}/{cleanControllerName}";
                    }
                    else
                    {
                        routePattern = string.IsNullOrEmpty(cleanControllerName) 
                            ? $"api/{moduleName}/{actionName}"
                            : $"api/{moduleName}/{cleanControllerName}/{actionName}";
                    }
                    routeName = $"Api_{moduleName}_{controllerName}_{originalActionName}";
                    break;

                case ControllerType.Admin:
                    // ⚙️ Admin Panel: /admin/[modulename]/[controller]/[action] → /admin/auth/user/index
                    if (originalActionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
                    {
                        routePattern = string.IsNullOrEmpty(cleanControllerName) 
                            ? $"admin/{moduleName}"
                            : $"admin/{moduleName}/{cleanControllerName}";
                    }
                    else
                    {
                        routePattern = string.IsNullOrEmpty(cleanControllerName) 
                            ? $"admin/{moduleName}/{actionName}"
                            : $"admin/{moduleName}/{cleanControllerName}/{actionName}";
                    }
                    routeName = $"Admin_{moduleName}_{controllerName}_{originalActionName}";
                    break;

                case ControllerType.Mvc:
                default:
                    // 🌐 Public MVC: /[modulename]/[controller]/[action] → /auth/login/index
                    if (originalActionName.Equals("Index", StringComparison.OrdinalIgnoreCase))
                    {
                        routePattern = string.IsNullOrEmpty(cleanControllerName) 
                            ? $"{moduleName}"
                            : $"{moduleName}/{cleanControllerName}";
                    }
                    else
                    {
                        routePattern = string.IsNullOrEmpty(cleanControllerName) 
                            ? $"{moduleName}/{actionName}"
                            : $"{moduleName}/{cleanControllerName}/{actionName}";
                    }
                    routeName = $"Mvc_{moduleName}_{controllerName}_{originalActionName}";
                    break;
            }

            return (routePattern, routeName);
        }

        private static string ToKebabCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return System.Text.RegularExpressions.Regex
                .Replace(input, "([a-z])([A-Z])", "$1-$2")
                .ToLowerInvariant();
        }

        // IMicFxModule interface implementation for backward compatibility
        void IMicFxModule.RegisterServices(IServiceCollection services)
        {
            ConfigureServices(services);
        }

        void IMicFxModule.MapEndpoints(IEndpointRouteBuilder endpoints)
        {
            Configure(endpoints);
        }


        /// <summary>
        /// Called when module is being initialized. Override for custom implementation.
        /// </summary>
        public virtual async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            // Default implementation - does nothing
            await Task.CompletedTask;
        }

        /// <summary>
        /// Called when module is being shut down. Override for custom implementation.
        /// </summary>
        public virtual async Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            // Default implementation - does nothing
            await Task.CompletedTask;
        }



        /// <summary>
        /// Extract module name from assembly namespace
        /// </summary>
        private string ExtractModuleNameFromAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                return "Unknown";

            // Extract from MicFx.Modules.ModuleName format
            var parts = assemblyName.Split('.');
            if (parts.Length >= 3 && parts[0] == "MicFx" && parts[1] == "Modules")
            {
                return ToKebabCase(parts[2]);
            }

            return ToKebabCase(assemblyName);
        }

        /// <summary>
        /// Build smart route pattern for controller mapping based on controller type
        /// </summary>
        private string BuildSmartRoutePatternForController(string moduleName, string controllerName, ControllerType detectedType)
        {
            var cleanControllerName = ExtractControllerName(controllerName);
            var kebabControllerName = ToKebabCase(cleanControllerName);
            
            // Check if controller name should be omitted (when it matches module name)
            var normalizedController = cleanControllerName.Replace("-", "").ToLowerInvariant();
            var normalizedModule = moduleName.Replace("-", "").ToLowerInvariant();
            
            var shouldOmitController = normalizedController == normalizedModule || 
                                     normalizedController == normalizedModule + "controller";

            return detectedType switch
            {
                ControllerType.Api => $"api/{moduleName}/{{action=Index}}", // API controllers use attribute routing
                ControllerType.Admin => shouldOmitController ? 
                    $"admin/{moduleName}/{{action=Index}}/{{id?}}" : 
                    $"admin/{moduleName}/{kebabControllerName}/{{action=Index}}/{{id?}}",
                ControllerType.Mvc => shouldOmitController ? 
                    $"{moduleName}/{{action=Index}}/{{id?}}" : 
                    $"{moduleName}/{kebabControllerName}/{{action=Index}}/{{id?}}",
                _ => shouldOmitController ? 
                    $"{moduleName}/{{action=Index}}/{{id?}}" : 
                    $"{moduleName}/{kebabControllerName}/{{action=Index}}/{{id?}}"
            };
        }

        /// <summary>
        /// Build smart route pattern based on controller type
        /// </summary>
        private string BuildSmartRoutePattern(string moduleName, Type controllerType, ControllerType detectedType)
        {
            var controllerName = ExtractControllerName(controllerType);
            var kebabControllerName = ToKebabCase(controllerName);

            return detectedType switch
            {
                ControllerType.Api => $"api/{moduleName}/{{action=Index}}", // API controllers use attribute routing
                ControllerType.Admin => $"admin/{moduleName}/{kebabControllerName}/{{action=Index}}/{{id?}}",
                ControllerType.Mvc => $"{moduleName}/{kebabControllerName}/{{action=Index}}/{{id?}}",
                _ => $"{moduleName}/{kebabControllerName}/{{action=Index}}/{{id?}}"
            };
        }

        /// <summary>
        /// Extract clean controller name from controller type
        /// </summary>
        private string ExtractControllerName(Type controllerType)
        {
            return ExtractControllerName(controllerType.Name);
        }

        /// <summary>
        /// Extract clean controller name from controller name string
        /// </summary>
        private string ExtractControllerName(string controllerName)
        {
            var name = controllerName.Replace("Controller", "");
            
            // Remove common suffixes
            if (name.EndsWith("Admin", StringComparison.OrdinalIgnoreCase))
                name = name.Replace("Admin", "").Trim();
                
            return name;
        }
    }
}