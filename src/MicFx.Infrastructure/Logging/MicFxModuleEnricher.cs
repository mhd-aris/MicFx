using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace MicFx.Infrastructure.Logging;

/// <summary>
/// Simple Serilog enricher that adds basic module context to log entries  
/// SIMPLIFIED: Removed complex stack trace parsing and expensive assembly operations
/// </summary>
public class MicFxModuleEnricher : ILogEventEnricher
{
    private const string ModulePropertyName = "Module";
    private const string AssemblyPropertyName = "Assembly";

    /// <summary>
    /// Enriches log event with basic module information
    /// SIMPLIFIED: Basic assembly name extraction only
    /// </summary>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        try
        {
            // Get simple calling assembly without complex stack trace analysis
            var assembly = Assembly.GetCallingAssembly();
            var assemblyName = assembly.GetName().Name ?? "Unknown";

            // Extract basic module information
            var moduleName = ExtractModuleName(assemblyName);

            // Add module name
            var moduleProperty = propertyFactory.CreateProperty(ModulePropertyName, moduleName);
            logEvent.AddPropertyIfAbsent(moduleProperty);

            // Add assembly name
            var assemblyProperty = propertyFactory.CreateProperty(AssemblyPropertyName, assemblyName);
            logEvent.AddPropertyIfAbsent(assemblyProperty);
        }
        catch
        {
            // Fail silently - enrichers should never break logging
        }
    }

    /// <summary>
    /// Simple module name extraction from assembly name
    /// SIMPLIFIED: Basic string parsing without complex reflection
    /// </summary>
    private static string ExtractModuleName(string assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
            return "Unknown";

        // Handle MicFx module pattern: MicFx.Modules.{ModuleName}
        if (assemblyName.StartsWith("MicFx.Modules."))
        {
            var parts = assemblyName.Split('.');
            return parts.Length >= 3 ? parts[2] : assemblyName;
        }

        // Handle MicFx core components: MicFx.{Component}
        if (assemblyName.StartsWith("MicFx."))
        {
            var parts = assemblyName.Split('.');
            return parts.Length >= 2 ? parts[1] : assemblyName;
        }

        // Return assembly name as-is for non-MicFx assemblies
        return assemblyName;
    }
}