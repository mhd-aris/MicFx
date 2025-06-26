using System.Security.Claims;

namespace MicFx.Modules.Auth.Services
{
    /// <summary>
    /// Service untuk permission checking dengan wildcard support
    /// Handles caching dan database fallback
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Check apakah user memiliki permission tertentu
        /// Supports wildcard matching (users.*, auth.*, *)
        /// </summary>
        /// <param name="user">ClaimsPrincipal user</param>
        /// <param name="permission">Permission name (e.g., "users.view")</param>
        /// <returns>True jika user memiliki permission</returns>
        Task<bool> HasPermissionAsync(ClaimsPrincipal user, string permission);

        /// <summary>
        /// Get semua permissions untuk user dengan caching
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of permission names</returns>
        Task<List<string>> GetUserPermissionsAsync(string userId);

        /// <summary>
        /// Clear cache untuk user tertentu
        /// Digunakan saat permissions berubah
        /// </summary>
        /// <param name="userId">User ID</param>
        Task ClearUserCacheAsync(string userId);

        /// <summary>
        /// Get full permission name dengan module prefix
        /// </summary>
        /// <param name="permission">Simple permission name (e.g., "users.view")</param>
        /// <param name="moduleName">Module name (e.g., "auth")</param>
        /// <returns>Full permission name (e.g., "auth.users.view")</returns>
        string GetFullPermissionName(string permission, string moduleName);

        /// <summary>
        /// Check if permission matches any wildcard patterns
        /// </summary>
        /// <param name="permission">Permission to check</param>
        /// <param name="userPermissions">User's permissions including wildcards</param>
        /// <returns>True if matches any pattern</returns>
        bool MatchesWildcardPattern(string permission, List<string> userPermissions);
    }
} 