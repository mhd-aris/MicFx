using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicFx.Modules.Auth.Domain.DTOs;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Domain.Configuration;
using MicFx.Modules.Auth.Domain.Exceptions;

namespace MicFx.Modules.Auth.Services
{
    /// <summary>
    /// Simple implementation of AuthService
    /// Using ASP.NET Core Identity for best practices
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AuthService> _logger;
        private readonly AuthConfig _config;

        public AuthService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            SignInManager<User> signInManager,
            ILogger<AuthService> logger,
            AuthConfig config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _logger = logger;
            _config = config;
        }

        public async Task<AuthResult> LoginAsync(LoginRequest request)
        {
            using var scope = _logger.BeginScope("UserLogin");
            _logger.LogInformation("🔐 Login attempt for user {Email}", request.Email);
            
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("❌ Login failed: User not found {Email}", request.Email);
                    throw new InvalidCredentialsException(request.Email);
                }

                if (!user.IsActive)
                {
                    _logger.LogWarning("❌ Login failed: User inactive {Email} {UserId}", request.Email, user.Id);
                    throw new AccountInactiveException(request.Email);
                }

                var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);

                if (result.Succeeded)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Ensure user has proper role claims
                    await EnsureUserRoleClaimsAsync(user);

                    var userInfo = await CreateUserInfoAsync(user);
                    
                    _logger.LogInformation("✅ Login successful for user {Email} {UserId} with roles {Roles}", 
                        request.Email, user.Id, string.Join(", ", userInfo.Roles));
                    
                    return new AuthResult { IsSuccess = true, Message = "Login successful", User = userInfo };
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("🔒 Login failed: Account locked out {Email} {UserId}", request.Email, user.Id);
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    throw new AccountLockedException(request.Email, lockoutEnd?.DateTime);
                }

                _logger.LogWarning("❌ Login failed: Invalid password {Email} {UserId}", request.Email, user.Id);
                throw new InvalidCredentialsException(request.Email);
            }
            catch (AuthException)
            {
                // Re-throw auth exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Login error for user {Email}", request.Email);
                throw new AuthenticationException("Login failed due to system error", ex);
            }
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest request)
        {
            using var scope = _logger.BeginScope("UserRegistration");
            _logger.LogInformation("📝 Registration attempt for user {Email}", request.Email);
            
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("❌ Registration failed: User already exists {Email}", request.Email);
                    throw new DuplicateUserException(request.Email);
                }

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Department = request.Department,
                    JobTitle = request.JobTitle,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    // Assign default user role from config
                    var defaultRole = _config.DefaultRoles.Contains("User") ? "User" : _config.DefaultRoles.FirstOrDefault();
                    if (!string.IsNullOrEmpty(defaultRole))
                    {
                        await _userManager.AddToRoleAsync(user, defaultRole);
                        _logger.LogInformation("👤 Assigned default role {Role} to new user {Email} {UserId}", 
                            defaultRole, request.Email, user.Id);
                    }

                    var userInfo = await CreateUserInfoAsync(user);
                    
                    _logger.LogInformation("✅ Registration successful for user {Email} {UserId} {FullName}", 
                        request.Email, user.Id, userInfo.FullName);
                    
                    return new AuthResult { IsSuccess = true, Message = "Registration successful", User = userInfo };
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                _logger.LogWarning("❌ Registration failed for user {Email}: {Errors}", 
                    request.Email, string.Join(", ", errors));
                
                throw new UserRegistrationException("Registration failed due to validation errors")
                    .AddDetail("ValidationErrors", errors)
                    .AddDetail("Email", request.Email);
            }
            catch (AuthException)
            {
                // Re-throw auth exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Registration error for user {Email}", request.Email);
                throw new UserRegistrationException("Registration failed due to system error", ex)
                    .AddDetail("Email", request.Email);
            }
        }

        public async Task<AuthResult> LogoutAsync(string userId)
        {
            _logger.LogInformation("🚪 Logout for user {UserId}", userId);
            
            try
            {
                await _signInManager.SignOutAsync();
                _logger.LogInformation("✅ Logout successful for user {UserId}", userId);
                return new AuthResult { IsSuccess = true, Message = "Logout successful" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Logout error for user {UserId}", userId);
                throw new AuthenticationException("Logout failed due to system error", ex)
                    .AddDetail("UserId", userId);
            }
        }

        public async Task<UserInfo?> GetUserInfoAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var userInfo = await CreateUserInfoAsync(user);
                    _logger.LogDebug("📋 Retrieved user info for {UserId} {Email}", userId, user.Email);
                    return userInfo;
                }
                
                _logger.LogWarning("❌ User not found {UserId}", userId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error retrieving user info for {UserId}", userId);
                throw new AuthDatabaseException("Failed to retrieve user information", ex)
                    .AddDetail("UserId", userId);
            }
        }

        public async Task<List<UserInfo>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userInfos = new List<UserInfo>();

                foreach (var user in users)
                {
                    userInfos.Add(await CreateUserInfoAsync(user));
                }

                _logger.LogInformation("📊 Retrieved {UserCount} users", userInfos.Count);
                return userInfos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error retrieving all users");
                throw new AuthDatabaseException("Failed to retrieve users list", ex);
            }
        }

        public async Task<AuthResult> AssignRoleAsync(string userId, string roleName)
        {
            _logger.LogInformation("👤 Assigning role {Role} to user {UserId}", roleName, userId);
            
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("❌ Cannot assign role: User not found {UserId}", userId);
                    throw new UserNotFoundException(userId);
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ Role {Role} assigned to user {UserId} {Email}", 
                        roleName, userId, user.Email);
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("❌ Failed to assign role {Role} to user {UserId}: {Errors}", 
                        roleName, userId, string.Join(", ", errors));
                    
                    throw new RoleManagementException($"Failed to assign role '{roleName}' to user")
                        .AddDetail("UserId", userId)
                        .AddDetail("RoleName", roleName)
                        .AddDetail("Errors", errors);
                }
                
                return new AuthResult
                {
                    IsSuccess = result.Succeeded,
                    Message = "Role assigned successfully"
                };
            }
            catch (AuthException)
            {
                // Re-throw auth exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error assigning role {Role} to user {UserId}", roleName, userId);
                throw new RoleManagementException("Role assignment failed due to system error", ex)
                    .AddDetail("UserId", userId)
                    .AddDetail("RoleName", roleName);
            }
        }

        public async Task<AuthResult> RemoveRoleAsync(string userId, string roleName)
        {
            _logger.LogInformation("👤 Removing role {Role} from user {UserId}", roleName, userId);
            
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("❌ Cannot remove role: User not found {UserId}", userId);
                    throw new UserNotFoundException(userId);
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ Role {Role} removed from user {UserId} {Email}", 
                        roleName, userId, user.Email);
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("❌ Failed to remove role {Role} from user {UserId}: {Errors}", 
                        roleName, userId, string.Join(", ", errors));
                    
                    throw new RoleManagementException($"Failed to remove role '{roleName}' from user")
                        .AddDetail("UserId", userId)
                        .AddDetail("RoleName", roleName)
                        .AddDetail("Errors", errors);
                }
                
                return new AuthResult
                {
                    IsSuccess = result.Succeeded,
                    Message = "Role removed successfully"
                };
            }
            catch (AuthException)
            {
                // Re-throw auth exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error removing role {Role} from user {UserId}", roleName, userId);
                throw new RoleManagementException("Role removal failed due to system error", ex)
                    .AddDetail("UserId", userId)
                    .AddDetail("RoleName", roleName);
            }
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    _logger.LogDebug("📋 Retrieved {RoleCount} roles for user {UserId}", roles.Count, userId);
                    return roles.ToList();
                }
                
                _logger.LogWarning("❌ Cannot get roles: User not found {UserId}", userId);
                return new List<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error retrieving roles for user {UserId}", userId);
                throw new AuthDatabaseException("Failed to retrieve user roles", ex)
                    .AddDetail("UserId", userId);
            }
        }

        public async Task<AuthResult> SetUserActiveStatusAsync(string userId, bool isActive)
        {
            _logger.LogInformation("🔄 Setting user {UserId} active status to {IsActive}", userId, isActive);
            
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("❌ Cannot set status: User not found {UserId}", userId);
                    throw new UserNotFoundException(userId);
                }

                user.IsActive = isActive;
                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("✅ User {UserId} {Email} {Status}", 
                        userId, user.Email, isActive ? "activated" : "deactivated");
                }
                else
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("❌ Failed to update user {UserId} status: {Errors}", 
                        userId, string.Join(", ", errors));
                    
                    throw new AuthDatabaseException($"Failed to update user status")
                        .AddDetail("UserId", userId)
                        .AddDetail("IsActive", isActive)
                        .AddDetail("Errors", errors);
                }
                
                return new AuthResult
                {
                    IsSuccess = result.Succeeded,
                    Message = $"User {(isActive ? "activated" : "deactivated")} successfully"
                };
            }
            catch (AuthException)
            {
                // Re-throw auth exceptions as-is
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error setting user {UserId} active status", userId);
                throw new AuthDatabaseException("Status update failed due to system error", ex)
                    .AddDetail("UserId", userId)
                    .AddDetail("IsActive", isActive);
            }
        }

        // Simple implementations for methods that need minimal functionality
        public Task<AuthResult> UpdateProfileAsync(string userId, UpdateProfileRequest request)
        {
            _logger.LogWarning("⚠️ UpdateProfile not implemented for user {UserId}", userId);
            return Task.FromResult(new AuthResult { IsSuccess = false, Message = "Not implemented yet" });
        }

        public Task<AuthResult> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            _logger.LogWarning("⚠️ ChangePassword not implemented for user {UserId}", userId);
            return Task.FromResult(new AuthResult { IsSuccess = false, Message = "Not implemented yet" });
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            _logger.LogWarning("⚠️ ValidateToken not implemented");
            return Task.FromResult(false);
        }

        public Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            _logger.LogWarning("⚠️ RefreshToken not implemented");
            return Task.FromResult(new AuthResult { IsSuccess = false, Message = "Not implemented yet" });
        }

        private async Task<UserInfo> CreateUserInfoAsync(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserInfo
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                Department = user.Department,
                JobTitle = user.JobTitle,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            };
        }

        /// <summary>
        /// Ensure user has proper role claims for authorization
        /// </summary>
        private async Task EnsureUserRoleClaimsAsync(User user)
        {
            try
            {
                var roles = await _userManager.GetRolesAsync(user);
                var existingClaims = await _userManager.GetClaimsAsync(user);
                
                // Remove old role claims
                var roleClaims = existingClaims.Where(c => c.Type == "role" || c.Type == System.Security.Claims.ClaimTypes.Role).ToList();
                if (roleClaims.Any())
                {
                    await _userManager.RemoveClaimsAsync(user, roleClaims);
                }

                // Add current role claims
                var newClaims = new List<System.Security.Claims.Claim>();
                foreach (var role in roles)
                {
                    newClaims.Add(new System.Security.Claims.Claim("role", role));
                    newClaims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role));
                }

                if (newClaims.Any())
                {
                    await _userManager.AddClaimsAsync(user, newClaims);
                }
                
                _logger.LogDebug("🔄 Updated role claims for user {UserId} with roles {Roles}", 
                    user.Id, string.Join(", ", roles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error updating role claims for user {UserId}", user.Id);
            }
        }
    }
}