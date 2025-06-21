using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.Modules.Auth.Domain.DTOs;

namespace MicFx.Modules.Auth.Api;

/// <summary>
/// Auth API Controller - Authentication and authorization endpoints
/// Uses attribute routing with [ApiController] and [Route] for precise control
/// Routes: /api/auth/* (defined by [Route("api/auth")] attribute)
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// User login endpoint
    /// ROUTE: POST /api/auth/login
    /// </summary>
    /// <param name="loginRequest">Login credentials</param>
    /// <returns>Authentication result</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<AuthResult>>> Login([FromBody] LoginRequest loginRequest)
    {
        _logger.LogInformation("Processing login request for user: {Email}", loginRequest.Email);

        try
        {
            // Simulate authentication logic
            await Task.Delay(100);

            if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest(ApiResponse<object>.Error("Email and password are required", new[] { "INVALID_CREDENTIALS" }));
            }

            // Mock successful login
            var authResult = new AuthResult
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = "mock_jwt_token_" + Guid.NewGuid().ToString("N")[..16],
                TokenExpiry = DateTime.UtcNow.AddHours(24),
                User = new UserInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = loginRequest.Email,
                    FirstName = "Demo",
                    LastName = "User",
                    FullName = "Demo User",
                    Roles = new List<string> { "User" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                }
            };

            _logger.LogInformation("User {Email} logged in successfully", loginRequest.Email);
            return Ok(ApiResponse<AuthResult>.Ok(authResult, "Login successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Email}", loginRequest.Email);
            return StatusCode(500, ApiResponse<object>.Error("An error occurred during login", new[] { "LOGIN_ERROR" }));
        }
    }

    /// <summary>
    /// User registration endpoint
    /// ROUTE: POST /api/auth/register
    /// </summary>
    /// <param name="registerRequest">Registration data</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResult>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<AuthResult>>> Register([FromBody] RegisterRequest registerRequest)
    {
        _logger.LogInformation("Processing registration request for user: {Email}", registerRequest.Email);

        try
        {
            // Simulate registration logic
            await Task.Delay(150);

            if (string.IsNullOrEmpty(registerRequest.FirstName) || 
                string.IsNullOrEmpty(registerRequest.LastName) ||
                string.IsNullOrEmpty(registerRequest.Email) || 
                string.IsNullOrEmpty(registerRequest.Password))
            {
                return BadRequest(ApiResponse<object>.Error("All required fields must be provided", new[] { "INVALID_INPUT" }));
            }

            // Mock successful registration
            var authResult = new AuthResult
            {
                IsSuccess = true,
                Message = "Registration successful",
                User = new UserInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    FullName = $"{registerRequest.FirstName} {registerRequest.LastName}",
                    Email = registerRequest.Email,
                    Roles = new List<string> { "User" },
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    Department = registerRequest.Department,
                    JobTitle = registerRequest.JobTitle
                }
            };

            _logger.LogInformation("User {Email} registered successfully", registerRequest.Email);
            return CreatedAtAction(nameof(GetProfile), new { id = authResult.User.Id }, 
                ApiResponse<AuthResult>.Ok(authResult, "Registration successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Email}", registerRequest.Email);
            return StatusCode(500, ApiResponse<object>.Error("An error occurred during registration", new[] { "REGISTRATION_ERROR" }));
        }
    }

    /// <summary>
    /// Get user profile
    /// ROUTE: GET /api/auth/profile/{id}
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User profile information</returns>
    [HttpGet("profile/{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserInfo>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<UserInfo>>> GetProfile(string id)
    {
        _logger.LogInformation("Retrieving profile for user: {UserId}", id);

        try
        {
            // Simulate profile retrieval
            await Task.Delay(50);

            // Mock profile data
            var user = new UserInfo
            {
                Id = id,
                FirstName = "Demo",
                LastName = "User",
                FullName = "Demo User",
                Email = "demo@example.com",
                Roles = new List<string> { "User" },
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                LastLoginAt = DateTime.UtcNow,
                Department = "Engineering",
                JobTitle = "Software Developer"
            };

            return Ok(ApiResponse<UserInfo>.Ok(user, "Profile retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile for user: {UserId}", id);
            return StatusCode(500, ApiResponse<object>.Error("An error occurred while retrieving profile", new[] { "PROFILE_ERROR" }));
        }
    }

    /// <summary>
    /// Logout endpoint
    /// ROUTE: POST /api/auth/logout
    /// </summary>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        _logger.LogInformation("Processing logout request");

        try
        {
            // Simulate logout logic (token invalidation, etc.)
            await Task.Delay(25);

            return Ok(ApiResponse<object>.Ok(new { }, "Logout successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, ApiResponse<object>.Error("An error occurred during logout", new[] { "LOGOUT_ERROR" }));
        }
    }

    /// <summary>
    /// Validate token endpoint
    /// AUTO-ROUTE: GET /api/auth/validate-token
    /// </summary>
    /// <returns>Token validation result</returns>
    [HttpGet("validate-token")]
    [ProducesResponseType(typeof(ApiResponse<TokenValidationResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    public async Task<ActionResult<ApiResponse<TokenValidationResult>>> ValidateToken()
    {
        _logger.LogInformation("Validating token");

        try
        {
            // Simulate token validation
            await Task.Delay(25);

            var result = new TokenValidationResult
            {
                IsValid = true,
                ExpiresAt = DateTime.UtcNow.AddHours(23),
                UserEmail = "demo@example.com"
            };

            return Ok(ApiResponse<TokenValidationResult>.Ok(result, "Token is valid"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, ApiResponse<object>.Error("An error occurred during token validation", new[] { "VALIDATION_ERROR" }));
        }
    }

    /// <summary>
    /// Get authentication status
    /// AUTO-ROUTE: GET /api/auth/status
    /// </summary>
    /// <returns>Current authentication status</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<ActionResult<ApiResponse<object>>> GetAuthStatus()
    {
        _logger.LogInformation("Getting authentication status");

        try
        {
            await Task.Delay(10);

            var status = new
            {
                IsAuthenticated = false, // Mock status
                AuthenticationType = "JWT",
                SupportedMethods = new[] { "Email/Password", "OAuth2", "JWT Bearer" },
                SessionTimeout = TimeSpan.FromHours(24).TotalMinutes,
                LastActivity = DateTime.UtcNow
            };

            return Ok(ApiResponse<object>.Ok(status, "Authentication status retrieved"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting authentication status");
            return StatusCode(500, ApiResponse<object>.Error("An error occurred while retrieving auth status", new[] { "STATUS_ERROR" }));
        }
    }
}

/// <summary>
/// Token validation result DTO for API responses
/// </summary>
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string UserEmail { get; set; } = string.Empty;
} 