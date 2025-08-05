using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Collections.Concurrent;

namespace MicFx.Web.Infrastructure;

/// <summary>
/// View location expander for MicFx Framework
/// Uses convention-based discovery for modular view resolution with caching
/// </summary>
public class MicFxViewLocationExpander : IViewLocationExpander
{
    private static readonly Lazy<string[]> _moduleCache = new(ScanModulesOnce);
    private static readonly object _lock = new();

    /// <summary>
    /// Populate context values for view location expansion
    /// </summary>
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        var controllerName = context.ActionContext.RouteData.Values["controller"]?.ToString() ?? "";
        var area = context.ActionContext.RouteData.Values["area"]?.ToString();

        if (!string.IsNullOrEmpty(controllerName))
        {
            context.Values["controller"] = controllerName;
        }

        if (!string.IsNullOrEmpty(area))
        {
            context.Values["area"] = area;
        }
    }

    /// <summary>
    /// Expand view locations with dynamic modular support
    /// </summary>
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        var expandedLocations = new List<string>();

        // Add module-specific view locations
        if (context.Values.TryGetValue("controller", out var controllerName) && !string.IsNullOrEmpty(controllerName))
        {
            var cleanController = controllerName.Replace("Controller", "");
            var area = context.Values.TryGetValue("area", out var areaValue) ? areaValue : null;

            // Use cached modules - no file system access
            var availableModules = GetAvailableModules();
            var matchingModule = FindMatchingModule(cleanController, availableModules);

            if (matchingModule != null)
            {
                // Add basic module-specific paths
                expandedLocations.AddRange(new[]
                {
                    $"~/Views/{cleanController}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{matchingModule}/Views/{{1}}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{matchingModule}/Views/{cleanController}/{{0}}.cshtml",
                    $"~/MicFx.Modules.{matchingModule}/Views/Shared/{{0}}.cshtml"
                });

                // Add area support for any area (not just Admin)
                if (!string.IsNullOrEmpty(area))
                {
                    expandedLocations.AddRange(new[]
                    {
                        $"~/MicFx.Modules.{matchingModule}/Areas/{area}/Views/{{1}}/{{0}}.cshtml",
                        $"~/MicFx.Modules.{matchingModule}/Areas/{area}/Views/{cleanController}/{{0}}.cshtml",
                        $"~/MicFx.Modules.{matchingModule}/Areas/{area}/Views/Shared/{{0}}.cshtml"
                    });
                }
            }
        }

        // Add framework shared locations (support all areas)
        var currentArea = context.Values.TryGetValue("area", out var areaVal) ? areaVal : null;
        if (!string.IsNullOrEmpty(currentArea))
        {
            expandedLocations.Add($"~/Areas/{currentArea}/Views/Shared/{{0}}.cshtml");
        }
        
        expandedLocations.AddRange(new[]
        {
            "~/Views/Shared/{0}.cshtml"
        });

        // Append original view locations as fallback
        expandedLocations.AddRange(viewLocations);

        return expandedLocations;
    }

    /// <summary>
    /// Get available modules using cached results - no file system access after first call
    /// </summary>
    private string[] GetAvailableModules()
    {
        return _moduleCache.Value;
    }

    /// <summary>
    /// Scan modules once during application startup - static method for lazy initialization
    /// </summary>
    private static string[] ScanModulesOnce()
    {
        var modules = new List<string>();

        try
        {
            // Try to find modules directory relative to current assembly
            var currentDirectory = Directory.GetCurrentDirectory();
            var possiblePaths = new[]
            {
                Path.Combine(currentDirectory, "..", "Modules"),           // From MicFx.Web
                Path.Combine(currentDirectory, "Modules"),                // Direct
                Path.Combine(currentDirectory, "..", "..", "Modules"),    // Alternative structure
                Path.Combine(currentDirectory, "src", "Modules")          // From root
            };

            foreach (var modulePath in possiblePaths)
            {
                if (Directory.Exists(modulePath))
                {
                    var moduleDirectories = Directory.GetDirectories(modulePath, "MicFx.Modules.*")
                        .Select(dir => Path.GetFileName(dir))
                        .Where(name => name.StartsWith("MicFx.Modules."))
                        .Select(name => name.Substring("MicFx.Modules.".Length))
                        .ToArray();

                    modules.AddRange(moduleDirectories);
                    break; // Use first found path
                }
            }
        }
        catch
        {
            // If file system access fails, return known modules
            // This prevents exceptions during view resolution
            modules.AddRange(new[] { "HelloWorld", "Auth" });
        }

        return modules.Distinct().ToArray();
    }

    /// <summary>
    /// Find matching module for a controller name
    /// Uses fuzzy matching for flexibility
    /// </summary>
    private string? FindMatchingModule(string controllerName, string[] availableModules)
    {
        if (string.IsNullOrEmpty(controllerName) || availableModules.Length == 0)
            return null;

        // Exact match first
        var exactMatch = availableModules.FirstOrDefault(module => 
            string.Equals(controllerName, module, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
            return exactMatch;

        // Prefix match
        var prefixMatch = availableModules.FirstOrDefault(module =>
            controllerName.StartsWith(module, StringComparison.OrdinalIgnoreCase));
        if (prefixMatch != null)
            return prefixMatch;

        // Contains match (for compound names like "AuthAdmin" matching "Auth")
        var containsMatch = availableModules.FirstOrDefault(module =>
            controllerName.Contains(module, StringComparison.OrdinalIgnoreCase) && module.Length > 2);
        
        return containsMatch;
    }
}