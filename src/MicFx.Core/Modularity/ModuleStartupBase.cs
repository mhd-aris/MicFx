using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using MicFx.SharedKernel.Modularity;
using MicFx.Core.Extensions;
using MicFx.SharedKernel.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace MicFx.Core.Modularity
{
    /// <summary>
    /// Base class for module startup with predictable routing conventions
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
            // Simple controller registration
            RegisterControllers(services);

            // Configure module configuration
            ConfigureModuleConfiguration(services);

            // Allow derived classes to add custom services
            ConfigureModuleServices(services);
        }

        /// <summary>
        /// Configures endpoints for the module. Override ConfigureModuleEndpoints for custom endpoints.
        /// </summary>
        public virtual void Configure(IEndpointRouteBuilder endpoints)
        {
            // Simple conventional routing for MVC controllers
            MapConventionalRoutes(endpoints);

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
        }

        /// <summary>
        /// Override this method to add custom endpoints for your module
        /// </summary>
        protected virtual void ConfigureModuleEndpoints(IEndpointRouteBuilder endpoints)
        {
            // Default implementation does nothing
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
        /// Registers controllers for the module
        /// </summary>
        private void RegisterControllers(IServiceCollection services)
        {
            var moduleAssembly = GetType().Assembly;

            var controllerTypes = moduleAssembly.GetTypes()
                .Where(t => t.Name.EndsWith("Controller") && 
                           (t.IsSubclassOf(typeof(Controller)) || t.IsSubclassOf(typeof(ControllerBase))) &&
                           !t.IsAbstract)
                .ToList();

            foreach (var controllerType in controllerTypes)
            {
                services.AddTransient(controllerType);
            }

            _logger?.LogInformation("Registered {ControllerCount} controllers for module {ModuleName}: {Controllers}",
                controllerTypes.Count, Manifest.Name, string.Join(", ", controllerTypes.Select(c => c.Name)));
        }

        /// <summary>
        /// Simple conventional routing - predictable and debuggable
        /// 
        /// Routing Convention:
        /// - API Controllers: Use [ApiController] + [Route] attributes for full control
        /// - Area Controllers: Use [Area] + [Route] attributes for area-specific routing
        /// - MVC Controllers: /{module}/{controller}/{action}/{id?} (conventional routing)
        /// </summary>
        private void MapConventionalRoutes(IEndpointRouteBuilder endpoints)
        {
            var moduleName = GetModuleName();
            
            _logger?.LogInformation("Setting up conventional routes for module: {ModuleName}", moduleName);

            // Standard MVC routes for module controllers (not in Areas)
            // This handles regular MVC controllers in Controllers folder
            endpoints.MapControllerRoute(
                name: $"{moduleName}_default",
                pattern: $"{moduleName}/{{controller=Home}}/{{action=Index}}/{{id?}}"
            );

            _logger?.LogInformation("Conventional routes configured for module {ModuleName}:", moduleName);
            _logger?.LogInformation("  - MVC: /{ModuleName}/{{controller}}/{{action}}/{{id?}}", moduleName);
            _logger?.LogInformation("  - API: Controllers should use [ApiController] + [Route] attributes");
            _logger?.LogInformation("  - Area: Controllers should use [Area] + [Route] attributes");
        }

        /// <summary>
        /// Extract module name from assembly in simple, predictable way
        /// </summary>
        private string GetModuleName()
        {
            var assemblyName = GetType().Assembly.GetName().Name ?? "Unknown";
            
            // Handle MicFx.Modules.ModuleName format
            if (assemblyName.StartsWith("MicFx.Modules."))
            {
                var parts = assemblyName.Split('.');
                if (parts.Length >= 3)
                {
                    return parts[2].ToLower(); // Simple lowercase, no kebab-case conversion
                }
            }

            // Fallback to assembly name
            return assemblyName.ToLower();
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
            _logger?.LogInformation("Initializing module {ModuleName}", Manifest.Name);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Called when module is being shut down. Override for custom implementation.
        /// </summary>
        public virtual async Task ShutdownAsync(CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("Shutting down module {ModuleName}", Manifest.Name);
            await Task.CompletedTask;
        }
    }
}