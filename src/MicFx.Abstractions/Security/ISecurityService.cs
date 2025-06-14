using MicFx.SharedKernel.Common;

namespace MicFx.Abstractions.Security;

/// <summary>
/// Interface for security operations in MicFx framework
/// Provides authentication, authorization, and security policy enforcement
/// </summary>
public interface ISecurityService
{
    /// <summary>
    /// Validates a security token
    /// </summary>
    /// <param name="token">Security token to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token validation result</returns>
    Task<TokenValidationResult> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user has required permissions
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="permissions">Required permissions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authorization result</returns>
    Task<AuthorizationResult> CheckPermissionsAsync(string userId, string[] permissions, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs security event for audit trail
    /// </summary>
    /// <param name="securityEvent">Security event information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogSecurityEventAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Encrypts sensitive data
    /// </summary>
    /// <param name="data">Data to encrypt</param>
    /// <param name="keyId">Encryption key identifier</param>
    /// <returns>Encrypted data</returns>
    Task<string> EncryptAsync(string data, string? keyId = null);

    /// <summary>
    /// Decrypts sensitive data
    /// </summary>
    /// <param name="encryptedData">Encrypted data</param>
    /// <param name="keyId">Encryption key identifier</param>
    /// <returns>Decrypted data</returns>
    Task<string> DecryptAsync(string encryptedData, string? keyId = null);

    /// <summary>
    /// Generates a secure hash for the given data
    /// </summary>
    /// <param name="data">Data to hash</param>
    /// <param name="salt">Optional salt for hashing</param>
    /// <returns>Secure hash</returns>
    string GenerateHash(string data, string? salt = null);

    /// <summary>
    /// Verifies data against a hash
    /// </summary>
    /// <param name="data">Original data</param>
    /// <param name="hash">Hash to verify against</param>
    /// <param name="salt">Optional salt used in hashing</param>
    /// <returns>True if data matches hash, false otherwise</returns>
    bool VerifyHash(string data, string hash, string? salt = null);
}

/// <summary>
/// Result of token validation operation
/// </summary>
public class TokenValidationResult
{
    /// <summary>
    /// Indicates if the token is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// User identifier from the token
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// User claims from the token
    /// </summary>
    public Dictionary<string, string> Claims { get; set; } = new();

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional validation details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Result of authorization check
/// </summary>
public class AuthorizationResult
{
    /// <summary>
    /// Indicates if the user is authorized
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// List of missing permissions if not authorized
    /// </summary>
    public string[] MissingPermissions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Reason for authorization failure
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    /// Additional authorization context
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Security event for audit logging
/// </summary>
public class SecurityEvent
{
    /// <summary>
    /// Type of security event
    /// </summary>
    public SecurityEventType EventType { get; set; }

    /// <summary>
    /// User identifier involved in the event
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// IP address from which the event originated
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent information
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Resource that was accessed or attempted to be accessed
    /// </summary>
    public string? Resource { get; set; }

    /// <summary>
    /// Action that was performed or attempted
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Result of the security operation
    /// </summary>
    public SecurityEventResult Result { get; set; }

    /// <summary>
    /// Additional event details
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for tracking related events
    /// </summary>
    public string? CorrelationId { get; set; }
}

/// <summary>
/// Types of security events
/// </summary>
public enum SecurityEventType
{
    /// <summary>
    /// User authentication attempt
    /// </summary>
    Authentication,

    /// <summary>
    /// Authorization check
    /// </summary>
    Authorization,

    /// <summary>
    /// Data access event
    /// </summary>
    DataAccess,

    /// <summary>
    /// Configuration change
    /// </summary>
    ConfigurationChange,

    /// <summary>
    /// Security policy violation
    /// </summary>
    PolicyViolation,

    /// <summary>
    /// Suspicious activity detected
    /// </summary>
    SuspiciousActivity,

    /// <summary>
    /// System security event
    /// </summary>
    SystemSecurity
}

/// <summary>
/// Result of security event
/// </summary>
public enum SecurityEventResult
{
    /// <summary>
    /// Operation succeeded
    /// </summary>
    Success,

    /// <summary>
    /// Operation failed
    /// </summary>
    Failure,

    /// <summary>
    /// Operation was blocked by security policy
    /// </summary>
    Blocked,

    /// <summary>
    /// Operation requires additional verification
    /// </summary>
    RequiresVerification
} 