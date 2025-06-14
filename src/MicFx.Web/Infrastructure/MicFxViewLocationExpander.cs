using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MicFx.Web.Infrastructure;

/// <summary>
/// Custom view location expander for MicFx Framework
/// Enables modular view resolution across different modules with intelligent path detection
/// </summary>
/// <remarks>
/// This expander supports multiple module patterns and controller types:
/// - Standard MVC controllers in modules
/// - Admin controllers with specialized paths
/// - API controllers with view support
/// </remarks>
public class MicFxViewLocationExpander : IViewLocationExpander
{
    /// <summary>
    /// Controller type enumeration for specialized routing
    /// </summary>
    private enum ControllerType
    {
        Mvc,
        Admin,
        Api
    }

    /// <summary>
    /// Populate context values for view location expansion
    /// Extracts module information and controller type for intelligent routing
    /// </summary>
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        // Extract controller and action information from ActionContext
        var controllerName = context.ActionContext.RouteData.Values["controller"]?.ToString() ?? "";
        var actionName = context.ActionContext.RouteData.Values["action"]?.ToString() ?? "";

        if (string.IsNullOrEmpty(controllerName))
        {
            return;
        }

        // Always try to determine module, even for simple controllers
        var moduleInfo = ExtractModuleInformation(controllerName);
        if (moduleInfo != null)
        {
            context.Values["module"] = moduleInfo.ModuleName;
            context.Values["controllerType"] = moduleInfo.ControllerType.ToString();
            context.Values["cleanController"] = moduleInfo.CleanControllerName;
        }
        else
        {
            // Fallback: treat controller as potential module name
            var cleanControllerName = controllerName.Replace("Controller", "");
            context.Values["module"] = cleanControllerName;
            context.Values["controllerType"] = "Mvc";
            context.Values["cleanController"] = cleanControllerName;
        }

        // Add additional context for enhanced view resolution
        context.Values["framework"] = "MicFx";
    }

    /// <summary>
    /// Expand view locations with modular support
    /// Provides comprehensive view location patterns for different scenarios
    /// </summary>
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        var expandedLocations = new List<string>();

        // Check if we have module context
        if (context.Values.TryGetValue("module", out var moduleName) && !string.IsNullOrEmpty(moduleName))
        {
            var controllerTypeValue = context.Values.TryGetValue("controllerType", out var ctrlType) ? ctrlType ?? "Mvc" : "Mvc";
            var controllerType = Enum.Parse<ControllerType>(controllerTypeValue);
            var cleanController = context.Values.TryGetValue("cleanController", out var cleanCtrl) ? cleanCtrl ?? "" : "";

            // Add module-specific view locations based on controller type
            var moduleLocations = GetModuleViewLocations(moduleName, cleanController, controllerType);
            expandedLocations.AddRange(moduleLocations);
            
            // Add shared locations within the module
            var sharedLocations = GetModuleSharedLocations(moduleName);
            expandedLocations.AddRange(sharedLocations);
        }

        // Add framework-wide shared locations
        expandedLocations.AddRange(GetFrameworkSharedLocations());

        // Append original view locations as fallback
        expandedLocations.AddRange(viewLocations);

        return expandedLocations;
    }

    /// <summary>
    /// Get module-specific view locations based on controller type
    /// Supports different patterns for MVC and Admin controllers
    /// </summary>
    private IEnumerable<string> GetModuleViewLocations(string moduleName, string cleanController, ControllerType controllerType)
    {
        var locations = new List<string>();
        
        switch (controllerType)
        {
            case ControllerType.Admin:
                // Areas/Admin pattern - prioritas utama
                locations.AddRange(new[]
                {
                    // Areas/Admin view locations
                    $"~/MicFx.Modules.{moduleName}/Areas/Admin/Views/{cleanController}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{moduleName}/Areas/Admin/Views/{{1}}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{moduleName}/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    
                    // Legacy admin view patterns (backward compatibility)
                    $"~/MicFx.Modules.{moduleName}/Views/Admin/{{1}}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{moduleName}/Views/Admin/{cleanController}/{{0}}.cshtml",
                    
                    // Fallback to standard patterns
                    $"~/MicFx.Modules.{moduleName}/Views/{cleanController}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{moduleName}/Views/{{1}}/{{0}}.cshtml",
                    
                    // Framework admin fallbacks
                    "/Areas/Admin/Views/{1}/{0}.cshtml",
                    "/Areas/Admin/Views/Shared/{0}.cshtml",
                    "/Views/Admin/{1}/{0}.cshtml",
                    "/Views/Admin/Shared/{0}.cshtml"
                });
                break;

            case ControllerType.Mvc:
            default:
                // Standard MVC view patterns - COMPREHENSIVE PATH RESOLUTION
                locations.AddRange(new[]
                {
                    // Primary module view locations from individual module file providers
                    $"~/Views/{{1}}/{{0}}.cshtml",
                    $"~/Views/{cleanController}/{{0}}.cshtml", 
                    $"~/Views/{moduleName}/{{0}}.cshtml",
                    
                    // Module-specific paths from parent file provider
                    $"~/MicFx.Modules.{moduleName}/Views/{{1}}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{moduleName}/Views/{cleanController}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{moduleName}/Views/{moduleName}/{{0}}.cshtml",
                    
                    // Alternative patterns
                    $"~/Modules/{moduleName}/Views/{{1}}/{{0}}.cshtml",
                    $"~/Modules/{moduleName}/Views/{cleanController}/{{0}}.cshtml",
                    $"~/Modules/{moduleName}/Views/{moduleName}/{{0}}.cshtml",
                    
                    // Nested patterns
                    $"~/MicFx.Modules.{moduleName}/Views/{cleanController}/{{1}}/{{0}}.cshtml",
                    $"~/Views/{cleanController}/{{1}}/{{0}}.cshtml"
                });
                break;
        }

        return locations;
    }

    /// <summary>
    /// Get shared view locations within a module
    /// Supports layouts, partials, and common components
    /// </summary>
    private IEnumerable<string> GetModuleSharedLocations(string moduleName)
    {
        return new[]
        {
            // Primary paths from individual module file providers
            $"~/Views/Shared/{{0}}.cshtml",
            $"~/Views/Shared/Components/{{0}}.cshtml",
            $"~/Views/Shared/Partials/{{0}}.cshtml",
            $"~/Views/Shared/Layouts/{{0}}.cshtml",
            
            // Module shared views from parent file provider
            $"~/MicFx.Modules.{moduleName}/Views/Shared/{{0}}.cshtml",
            $"~/MicFx.Modules.{moduleName}/Views/Shared/Components/{{0}}.cshtml",
            $"~/MicFx.Modules.{moduleName}/Views/Shared/Partials/{{0}}.cshtml",
            $"~/MicFx.Modules.{moduleName}/Views/Shared/Layouts/{{0}}.cshtml",
            
            // Alternative patterns
            $"~/Modules/{moduleName}/Views/Shared/{{0}}.cshtml",
            $"~/Modules/{moduleName}/Views/Shared/Components/{{0}}.cshtml",
            $"~/Modules/{moduleName}/Views/Shared/Partials/{{0}}.cshtml",
            $"~/Modules/{moduleName}/Views/Shared/Layouts/{{0}}.cshtml",
            
            // Components and partials patterns
            $"~/MicFx.Modules.{moduleName}/Views/Components/{{0}}.cshtml",
            $"~/MicFx.Modules.{moduleName}/Views/Partials/{{0}}.cshtml",
            $"~/Views/Components/{{0}}.cshtml",
            $"~/Views/Partials/{{0}}.cshtml"
        };
    }

    /// <summary>
    /// Get framework-wide shared locations
    /// Supports global layouts, components, and shared views
    /// </summary>
    private IEnumerable<string> GetFrameworkSharedLocations()
    {
        return new[]
        {
            "/Views/Shared/{0}.cshtml",
            "/Views/Shared/Components/{0}.cshtml",
            "/Views/Shared/Partials/{0}.cshtml",
            "/Views/Shared/Layouts/{0}.cshtml",
            "/Views/Admin/Shared/{0}.cshtml",
            "/Views/Admin/Components/{0}.cshtml",
            "/Views/Components/{0}.cshtml",
            "/Views/Partials/{0}.cshtml",
            "/Views/{0}.cshtml"
        };
    }

    /// <summary>
    /// Extract module information from controller name using multiple strategies
    /// Comprehensive detection including assembly analysis, reflection, and pattern matching
    /// </summary>
    private ModuleInformation? ExtractModuleInformation(string controllerName)
    {
        if (string.IsNullOrEmpty(controllerName))
            return null;

        // Strategy 1: Assembly-based detection (most reliable)
        var assemblyBasedInfo = ExtractFromAssembly(controllerName);
        if (assemblyBasedInfo != null)
        {
            return assemblyBasedInfo;
        }

        // Strategy 2: Controller type detection (reflection-based)
        var reflectionBasedInfo = ExtractFromControllerTypes(controllerName);
        if (reflectionBasedInfo != null)
        {
            return reflectionBasedInfo;
        }

        // Strategy 3: Known module patterns (enhanced)
        var patternBasedInfo = ExtractFromKnownPatterns(controllerName);
        if (patternBasedInfo != null)
        {
            return patternBasedInfo;
        }

        // Strategy 4: Fallback - treat controller name as module name
        var cleanControllerName = controllerName.Replace("Controller", "");
        var fallbackInfo = new ModuleInformation
        {
            ModuleName = cleanControllerName,
            ControllerType = DetermineControllerTypeFromName(cleanControllerName),
            CleanControllerName = cleanControllerName
        };
        
        return fallbackInfo;
    }

    /// <summary>
    /// Extract module information from assembly names
    /// Most reliable method using assembly metadata
    /// </summary>
    private ModuleInformation? ExtractFromAssembly(string controllerName)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies.Where(a => a.GetName().Name?.StartsWith("MicFx.Modules.") == true))
        {
            try
            {
                var controllerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Controller"))
                    .Where(t => t.Name.Equals($"{controllerName}Controller", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var controllerType in controllerTypes)
                {
                    var moduleInfo = AnalyzeControllerType(controllerType, controllerName);
                    if (moduleInfo != null)
                        return moduleInfo;
                }
            }
            catch (Exception)
            {
                // Skip assemblies that can't be analyzed
                continue;
            }
        }

        return null;
    }

    /// <summary>
    /// Extract module information by analyzing controller types across all assemblies
    /// Reflection-based approach for comprehensive detection
    /// </summary>
    private ModuleInformation? ExtractFromControllerTypes(string controllerName)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in allAssemblies)
        {
            try
            {
                var controllerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && 
                               t.Name.Equals($"{controllerName}Controller", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var controllerType in controllerTypes)
                {
                    var namespaceName = controllerType.Namespace ?? "";
                    if (namespaceName.Contains("MicFx") || namespaceName.Contains("Modules"))
                    {
                        var moduleInfo = AnalyzeControllerType(controllerType, controllerName);
                        if (moduleInfo != null)
                            return moduleInfo;
                    }
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // Skip assemblies that can't be reflected
                continue;
            }
        }

        return null;
    }

    /// <summary>
    /// Extract module information from known patterns
    /// Enhanced with more comprehensive module detection
    /// </summary>
    private ModuleInformation? ExtractFromKnownPatterns(string controllerName)
    {
        // Enhanced known modules list
        var knownModules = new[] { "Auth", "HelloWorld", "User", "Admin", "Dashboard", "Account", "Profile", "Settings" };
        
        var cleanControllerName = controllerName.Replace("Controller", "");
        
        // Check for direct module name matches (exact match first)
        foreach (var moduleName in knownModules)
        {
            if (cleanControllerName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
            {
                var controllerType = DetermineControllerTypeFromName(cleanControllerName);
                var finalCleanName = CleanControllerName(cleanControllerName, controllerType);
                
                return new ModuleInformation
                {
                    ModuleName = moduleName,
                    ControllerType = controllerType,
                    CleanControllerName = finalCleanName
                };
            }
        }
        
        // Check for prefix matches
        foreach (var moduleName in knownModules)
        {
            if (cleanControllerName.StartsWith(moduleName, StringComparison.OrdinalIgnoreCase))
            {
                var controllerType = DetermineControllerTypeFromName(cleanControllerName);
                var finalCleanName = CleanControllerName(cleanControllerName, controllerType);
                
                return new ModuleInformation
                {
                    ModuleName = moduleName,
                    ControllerType = controllerType,
                    CleanControllerName = finalCleanName
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Analyze controller type to extract module information
    /// Comprehensive analysis including namespace, attributes, and inheritance
    /// </summary>
    private ModuleInformation? AnalyzeControllerType(Type controllerType, string controllerName)
    {
        var namespaceName = controllerType.Namespace ?? "";
        var namespaceParts = namespaceName.Split('.');

        // Extract module name from namespace (MicFx.Modules.ModuleName.*)
        if (namespaceParts.Length >= 3 && namespaceParts[0] == "MicFx" && namespaceParts[1] == "Modules")
        {
            var moduleName = namespaceParts[2];
            var controllerTypeName = DetermineControllerTypeFromNamespace(namespaceName, controllerName);
            var cleanControllerName = CleanControllerName(controllerName.Replace("Controller", ""), controllerTypeName);

            return new ModuleInformation
            {
                ModuleName = moduleName,
                ControllerType = controllerTypeName,
                CleanControllerName = cleanControllerName
            };
        }

        // Alternative pattern: Any namespace containing "Modules"
        if (namespaceName.Contains("Modules", StringComparison.OrdinalIgnoreCase))
        {
            var modulesPart = namespaceName.Substring(namespaceName.IndexOf("Modules", StringComparison.OrdinalIgnoreCase));
            var parts = modulesPart.Split('.');
            if (parts.Length >= 2)
            {
                var moduleName = parts[1];
                var controllerTypeName = DetermineControllerTypeFromNamespace(namespaceName, controllerName);
                var cleanControllerName = CleanControllerName(controllerName.Replace("Controller", ""), controllerTypeName);

                return new ModuleInformation
                {
                    ModuleName = moduleName,
                    ControllerType = controllerTypeName,
                    CleanControllerName = cleanControllerName
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Determine controller type from namespace analysis
    /// Enhanced to support Areas/Admin pattern
    /// </summary>
    private ControllerType DetermineControllerTypeFromNamespace(string namespaceName, string controllerName)
    {
        // Prioritas utama: Areas/Admin pattern
        if (namespaceName.Contains("Areas.Admin", StringComparison.OrdinalIgnoreCase) ||
            namespaceName.Contains("Areas\\Admin", StringComparison.OrdinalIgnoreCase))
            return ControllerType.Admin;

        // Existing logic untuk backward compatibility
        if (namespaceName.Contains("Admin", StringComparison.OrdinalIgnoreCase) ||
            controllerName.Contains("Admin", StringComparison.OrdinalIgnoreCase))
            return ControllerType.Admin;

        if (namespaceName.Contains("Api", StringComparison.OrdinalIgnoreCase) ||
            controllerName.Contains("Api", StringComparison.OrdinalIgnoreCase))
            return ControllerType.Api;

        return ControllerType.Mvc;
    }

    /// <summary>
    /// Determine controller type from controller name
    /// Fallback method for controller type detection
    /// </summary>
    private ControllerType DetermineControllerTypeFromName(string controllerName)
    {
        if (controllerName.Contains("Admin", StringComparison.OrdinalIgnoreCase))
            return ControllerType.Admin;
        
        if (controllerName.Contains("Api", StringComparison.OrdinalIgnoreCase))
            return ControllerType.Api;

        return ControllerType.Mvc;
    }

    /// <summary>
    /// Clean controller name based on controller type
    /// Removes type-specific prefixes and suffixes
    /// </summary>
    private string CleanControllerName(string controllerName, ControllerType controllerType)
    {
        var cleanName = controllerName;

        switch (controllerType)
        {
            case ControllerType.Admin:
                cleanName = cleanName.Replace("Admin", "");
                break;
            case ControllerType.Api:
                cleanName = cleanName.Replace("Api", "");
                break;
        }

        return string.IsNullOrEmpty(cleanName) ? controllerName : cleanName;
    }

    /// <summary>
    /// Module information container
    /// Holds extracted module metadata for view resolution
    /// </summary>
    private class ModuleInformation
    {
        public string ModuleName { get; set; } = string.Empty;
        public ControllerType ControllerType { get; set; }
        public string CleanControllerName { get; set; } = string.Empty;
    }
}