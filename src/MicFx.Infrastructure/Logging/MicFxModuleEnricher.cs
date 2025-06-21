using Serilog.Core;
using Serilog.Events;
using System.Diagnostics;
using System.Reflection;

namespace MicFx.Infrastructure.Logging;

/// <summary>
/// Simple Serilog enricher that adds basic module context to log entries  
/// FIXED: Uses StackTrace to properly detect the actual calling module
/// </summary>
public class MicFxModuleEnricher : ILogEventEnricher
{
    private const string ModulePropertyName = "Module";
    private const string AssemblyPropertyName = "Assembly";

    /// <summary>
    /// Enriches log event with basic module information
    /// FIXED: Uses StackTrace to get the real calling assembly
    /// </summary>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        try
        {
            // Use StackTrace to find the actual calling assembly
            var assembly = GetCallingAssemblyFromStackTrace();
            var assemblyName = assembly?.GetName().Name ?? "Unknown";

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
    /// Gets the calling assembly by walking the stack trace to skip Serilog internal calls
    /// </summary>
    private static Assembly? GetCallingAssemblyFromStackTrace()
    {
        var stackTrace = new StackTrace();
        var frames = stackTrace.GetFrames();

        if (frames == null) return null;

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            if (method?.DeclaringType?.Assembly == null) continue;

            var assemblyName = method.DeclaringType.Assembly.GetName().Name;
            
            // Skip Serilog, Microsoft, and System assemblies
            if (assemblyName != null && 
                !assemblyName.StartsWith("Serilog") &&
                !assemblyName.StartsWith("Microsoft") &&
                !assemblyName.StartsWith("System") &&
                assemblyName.StartsWith("MicFx"))
            {
                return method.DeclaringType.Assembly;
            }
        }

        return Assembly.GetEntryAssembly();
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