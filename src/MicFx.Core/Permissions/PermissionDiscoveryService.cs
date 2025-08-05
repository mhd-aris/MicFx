using System.Reflection;
using Microsoft.Extensions.Logging;

namespace MicFx.Core.Permissions;

/// <summary>
/// Service for discovering permissions from assemblies using reflection
/// Scans for classes with [PermissionModule] and fields with [Permission]
/// </summary>
public class PermissionDiscoveryService : IPermissionDiscoveryService
{
    private readonly ILogger<PermissionDiscoveryService> _logger;

    public PermissionDiscoveryService(ILogger<PermissionDiscoveryService> logger)
    {
        _logger = logger;
    }

    public List<PermissionDiscoveryResult> DiscoverPermissions()
    {
        var results = new List<PermissionDiscoveryResult>();
        
        // Get all loaded assemblies that might contain permissions
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && 
                       (a.FullName?.Contains("MicFx") == true || 
                        a.FullName?.Contains("Modules") == true))
            .ToList();

        foreach (var assembly in assemblies)
        {
            try
            {
                var assemblyResults = DiscoverPermissions(assembly);
                results.AddRange(assemblyResults);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to discover permissions from assembly {AssemblyName}", assembly.FullName);
            }
        }

        _logger.LogInformation("Discovered {PermissionCount} permission modules from {AssemblyCount} assemblies", 
            results.Count, assemblies.Count);

        return results;
    }

    public List<PermissionDiscoveryResult> DiscoverPermissions(Assembly assembly)
    {
        var results = new List<PermissionDiscoveryResult>();

        try
        {
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsPublic)
                .Where(t => t.GetCustomAttribute<PermissionModuleAttribute>() != null)
                .ToList();

            foreach (var type in types)
            {
                var moduleAttribute = type.GetCustomAttribute<PermissionModuleAttribute>();
                if (moduleAttribute == null) continue;

                var result = new PermissionDiscoveryResult
                {
                    ModuleName = moduleAttribute.ModuleName,
                    Permissions = DiscoverPermissionsFromType(type, moduleAttribute.ModuleName)
                };

                if (result.Permissions.Any())
                {
                    results.Add(result);
                    _logger.LogDebug("Discovered {PermissionCount} permissions from module {ModuleName} in {TypeName}", 
                        result.Permissions.Count, result.ModuleName, type.Name);
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            _logger.LogWarning("Could not load types from assembly {AssemblyName}: {LoaderExceptions}", 
                assembly.FullName, string.Join(", ", ex.LoaderExceptions?.Select(e => e?.Message) ?? new string[0]));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering permissions from assembly {AssemblyName}", assembly.FullName);
        }

        return results;
    }

    private List<DiscoveredPermission> DiscoverPermissionsFromType(Type type, string moduleName)
    {
        var permissions = new List<DiscoveredPermission>();

        // Get all const string fields with Permission attribute
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Where(f => f.GetCustomAttribute<PermissionAttribute>() != null)
            .ToList();

        foreach (var field in fields)
        {
            var permissionAttribute = field.GetCustomAttribute<PermissionAttribute>();
            var permissionValue = field.GetRawConstantValue() as string;

            if (permissionAttribute != null && !string.IsNullOrEmpty(permissionValue))
            {
                permissions.Add(new DiscoveredPermission
                {
                    Name = permissionValue,
                    Module = moduleName,
                    DisplayName = permissionAttribute.DisplayName,
                    Description = permissionAttribute.Description,
                    Category = permissionAttribute.Category,
                    IsSystemPermission = permissionAttribute.IsSystemPermission
                });
            }
        }

        return permissions;
    }
}
