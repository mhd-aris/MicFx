using MicFx.Modules.Auth.Domain.DTOs;
using MicFx.Modules.Auth.Domain.Entities;

namespace MicFx.Modules.Auth.Services
{
    /// <summary>
    /// Interface untuk Authentication Service
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Login user dengan email dan password
        /// </summary>
        Task<AuthResult> LoginAsync(LoginRequest request);

        /// <summary>
        /// Register user baru
        /// </summary>
        Task<AuthResult> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Logout user
        /// </summary>
        Task<AuthResult> LogoutAsync(string userId);

        /// <summary>
        /// Get user information by ID
        /// </summary>
        Task<UserInfo?> GetUserInfoAsync(string userId);

        /// <summary>
        /// Update user profile
        /// </summary>
        Task<AuthResult> UpdateProfileAsync(string userId, UpdateProfileRequest request);

        /// <summary>
        /// Change user password
        /// </summary>
        Task<AuthResult> ChangePasswordAsync(string userId, ChangePasswordRequest request);

        /// <summary>
        /// Get all users (untuk admin)
        /// </summary>
        Task<List<UserInfo>> GetAllUsersAsync();

        /// <summary>
        /// Assign role to user
        /// </summary>
        Task<AuthResult> AssignRoleAsync(string userId, string roleName);

        /// <summary>
        /// Remove role from user
        /// </summary>
        Task<AuthResult> RemoveRoleAsync(string userId, string roleName);

        /// <summary>
        /// Get user roles
        /// </summary>
        Task<List<string>> GetUserRolesAsync(string userId);

        /// <summary>
        /// Activate/Deactivate user
        /// </summary>
        Task<AuthResult> SetUserActiveStatusAsync(string userId, bool isActive);

        /// <summary>
        /// Validate token (untuk JWT scenarios)
        /// </summary>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Refresh token (untuk JWT scenarios)
        /// </summary>
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
    }
}