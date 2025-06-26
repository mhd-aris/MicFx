using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using System.Reflection;

namespace MicFx.Modules.Auth.Authorization
{
    /// <summary>
    /// Permission-based authorization attribute dengan auto-module detection
    /// Usage: [Permission("users.view")] - framework otomatis detect module
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Simple permission name (e.g., "users.view")
        /// </summary>
        public string Permission { get; }

        /// <summary>
        /// Full permission name dengan module prefix (e.g., "auth.users.view")
        /// </summary>
        public string FullPermission { get; }

        /// <summary>
        /// Module yang terdeteksi dari assembly context
        /// </summary>
        public string ModuleName { get; }

        /// <summary>
        /// Constructor untuk permission attribute
        /// </summary>
        /// <param name="permission">Simple permission name seperti "users.view"</param>
        public PermissionAttribute(string permission) : base()
        {
            if (string.IsNullOrEmpty(permission))
            {
                throw new ArgumentException("Permission cannot be null or empty", nameof(permission));
            }

            Permission = permission;
            ModuleName = DetectCurrentModule();
            FullPermission = CreateFullPermissionName(permission, ModuleName);
            
            // Set authorization policy
            Policy = $"Permission:{FullPermission}";
        }

        /// <summary>
        /// Detect module name dari calling assembly context
        /// </summary>
        private static string DetectCurrentModule()
        {
            try
            {
                // Get the calling assembly (yang menggunakan attribute ini)
                var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: false);
                
                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    var method = frame?.GetMethod();
                    var declaringType = method?.DeclaringType;
                    
                    if (declaringType != null)
                    {
                        var assembly = declaringType.Assembly;
                        var moduleName = ExtractModuleNameFromAssembly(assembly);
                        
                        if (!string.IsNullOrEmpty(moduleName))
                        {
                            return moduleName;
                        }
                    }
                }

                // Fallback jika tidak bisa detect
                return "unknown";
            }
            catch
            {
                // Safe fallback
                return "unknown";
            }
        }

        /// <summary>
        /// Extract module name dari assembly name
        /// </summary>
        private static string ExtractModuleNameFromAssembly(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            
            if (string.IsNullOrEmpty(assemblyName))
            {
                return "unknown";
            }

            // Pattern: MicFx.Modules.{ModuleName}
            if (assemblyName.StartsWith("MicFx.Modules."))
            {
                var parts = assemblyName.Split('.');
                if (parts.Length >= 3)
                {
                    return parts[2].ToLowerInvariant(); // Extract ModuleName
                }
            }

            // Pattern: MicFx.{ModuleName}
            if (assemblyName.StartsWith("MicFx."))
            {
                var parts = assemblyName.Split('.');
                if (parts.Length >= 2)
                {
                    return parts[1].ToLowerInvariant();
                }
            }

            // Fallback: use last part of assembly name
            var lastPart = assemblyName.Split('.').LastOrDefault();
            return lastPart?.ToLowerInvariant() ?? "unknown";
        }

        /// <summary>
        /// Create full permission name dengan module prefix
        /// </summary>
        private static string CreateFullPermissionName(string permission, string moduleName)
        {
            // Jika permission sudah include module prefix, return as-is
            if (permission.Split('.').Length >= 3)
            {
                return permission;
            }

            // Jika moduleName unknown, return permission as-is
            if (moduleName == "unknown")
            {
                return permission;
            }

            return $"{moduleName}.{permission}";
        }
    }

    /// <summary>
    /// Multiple permissions attribute (ANY logic)
    /// User perlu minimal salah satu permission
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AnyPermissionAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Array of permissions (user needs any one of them)
        /// </summary>
        public string[] Permissions { get; }

        /// <summary>
        /// Full permission names dengan module prefix
        /// </summary>
        public string[] FullPermissions { get; }

        /// <summary>
        /// Constructor untuk multiple permissions (ANY logic)
        /// </summary>
        /// <param name="permissions">Array permission names</param>
        public AnyPermissionAttribute(params string[] permissions) : base()
        {
            if (permissions == null || permissions.Length == 0)
            {
                throw new ArgumentException("At least one permission is required", nameof(permissions));
            }

            Permissions = permissions;
            var moduleName = DetectCurrentModule();
            FullPermissions = permissions.Select(p => CreateFullPermissionName(p, moduleName)).ToArray();
            
            // Create policy untuk any permission
            Policy = $"AnyPermission:{string.Join(",", FullPermissions)}";
        }

        private static string DetectCurrentModule()
        {
            // Same logic as PermissionAttribute
            try
            {
                var stackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: false);
                
                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    var method = frame?.GetMethod();
                    var declaringType = method?.DeclaringType;
                    
                    if (declaringType != null)
                    {
                        var assembly = declaringType.Assembly;
                        var assemblyName = assembly.GetName().Name;
                        
                        if (!string.IsNullOrEmpty(assemblyName) && assemblyName.StartsWith("MicFx.Modules."))
                        {
                            var parts = assemblyName.Split('.');
                            if (parts.Length >= 3)
                            {
                                return parts[2].ToLowerInvariant();
                            }
                        }
                    }
                }

                return "unknown";
            }
            catch
            {
                return "unknown";
            }
        }

        private static string CreateFullPermissionName(string permission, string moduleName)
        {
            if (permission.Split('.').Length >= 3 || moduleName == "unknown")
            {
                return permission;
            }

            return $"{moduleName}.{permission}";
        }
    }
} 