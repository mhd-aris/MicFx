using Serilog.Core;
using Serilog.Events;
using System.Reflection;

namespace MicFx.Infrastructure.Logging;

/// <summary>
/// Custom Serilog enricher that adds module context to log entries
/// Helps in debugging and monitoring module-specific issues
/// </summary>
public class MicFxModuleEnricher : ILogEventEnricher
{
    private const string ModulePropertyName = "Module";
    private const string AssemblyPropertyName = "Assembly";
    private const string VersionPropertyName = "Version";



    /// <summary>
    /// Enriches log event with module information
    /// </summary>
    /// <param name="logEvent">The log event to enrich</param>
    /// <param name="propertyFactory">Property factory for creating log properties</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        try
        {
            // Get the calling assembly
            var callingAssembly = GetCallingAssembly();
            if (callingAssembly == null) return;

            var assemblyName = callingAssembly.GetName();
            var fullName = assemblyName.Name ?? "Unknown";

            // Extract module information
            var moduleInfo = ExtractModuleInfo(fullName);

            if (moduleInfo != null)
            {
                // Add module name
                var moduleProperty = propertyFactory.CreateProperty(
                    ModulePropertyName,
                    moduleInfo.ModuleName);
                logEvent.AddPropertyIfAbsent(moduleProperty);

                // Add assembly name
                var assemblyProperty = propertyFactory.CreateProperty(
                    AssemblyPropertyName,
                    fullName);
                logEvent.AddPropertyIfAbsent(assemblyProperty);

                // Add version if available
                var version = assemblyName.Version?.ToString() ?? "Unknown";
                var versionProperty = propertyFactory.CreateProperty(
                    VersionPropertyName,
                    version);
                logEvent.AddPropertyIfAbsent(versionProperty);
            }
        }
        catch
        {
            // Fail silently - enrichers should never break logging
        }
    }

    /// <summary>
    /// Gets the calling assembly from stack trace
    /// </summary>
    private static Assembly? GetCallingAssembly()
    {
        try
        {
            var stackTrace = Environment.StackTrace;
            if (string.IsNullOrEmpty(stackTrace)) return null;

            var lines = stackTrace.Split('\n');

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Look for method calls that are not from Serilog or System namespaces
                if (line.Contains(" at ") &&
                    !line.Contains("Serilog") &&
                    !line.Contains("System.") &&
                    !line.Contains("Microsoft."))
                {
                    // Try to extract type information
                    var match = System.Text.RegularExpressions.Regex.Match(
                        line, @"at\s+([^\s\(]+)");

                    if (match.Success)
                    {
                        var typeName = match.Groups[1].Value;
                        var lastDotIndex = typeName.LastIndexOf('.');
                        if (lastDotIndex > 0)
                        {
                            var namespaceName = typeName.Substring(0, lastDotIndex);

                            // Try to find assembly by namespace
                            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                            var assembly = assemblies.FirstOrDefault(a =>
                            {
                                try
                                {
                                    return a.GetTypes().Any(t =>
                                        t.Namespace?.StartsWith(namespaceName) == true);
                                }
                                catch
                                {
                                    return false;
                                }
                            });

                            if (assembly != null) return assembly;
                        }
                    }
                }
            }

            // Fallback to calling assembly
            return Assembly.GetCallingAssembly();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Extracts module information from assembly name
    /// </summary>
    private static ModuleInfo? ExtractModuleInfo(string assemblyName)
    {
        if (string.IsNullOrEmpty(assemblyName))
            return null;

        // Handle MicFx module pattern: MicFx.Modules.{ModuleName}
        if (assemblyName.StartsWith("MicFx.Modules."))
        {
            var parts = assemblyName.Split('.');
            if (parts.Length >= 3)
            {
                return new ModuleInfo
                {
                    ModuleName = parts[2],
                    IsFrameworkModule = true,
                    FullAssemblyName = assemblyName
                };
            }
        }

        // Handle MicFx core components
        if (assemblyName.StartsWith("MicFx."))
        {
            var parts = assemblyName.Split('.');
            if (parts.Length >= 2)
            {
                return new ModuleInfo
                {
                    ModuleName = parts[1],
                    IsFrameworkModule = true,
                    FullAssemblyName = assemblyName
                };
            }
        }

        // Handle custom modules or applications
        var lastDotIndex = assemblyName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            var moduleName = assemblyName.Substring(lastDotIndex + 1);
            return new ModuleInfo
            {
                ModuleName = moduleName,
                IsFrameworkModule = false,
                FullAssemblyName = assemblyName
            };
        }

        // Fallback
        return new ModuleInfo
        {
            ModuleName = assemblyName,
            IsFrameworkModule = false,
            FullAssemblyName = assemblyName
        };
    }

    /// <summary>
    /// Module information extracted from assembly
    /// </summary>
    public record ModuleInfo
    {
        public string ModuleName { get; set; } = string.Empty;
        public bool IsFrameworkModule { get; set; }
        public string FullAssemblyName { get; set; } = string.Empty;
    }
}