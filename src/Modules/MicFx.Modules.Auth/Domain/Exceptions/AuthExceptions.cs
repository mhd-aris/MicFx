using MicFx.SharedKernel.Common.Exceptions;

namespace MicFx.Modules.Auth.Domain.Exceptions
{
    /// <summary>
    /// Base exception untuk Auth module
    /// </summary>
    public abstract class AuthException : MicFxException
    {
        protected AuthException(string message, string errorCode, ErrorCategory category = ErrorCategory.Security, int httpStatusCode = 400)
            : base(message, errorCode, category, httpStatusCode)
        {
            SetModule("Auth");
        }

        protected AuthException(string message, Exception innerException, string errorCode, ErrorCategory category = ErrorCategory.Security, int httpStatusCode = 400)
            : base(message, innerException, errorCode, category, httpStatusCode)
        {
            SetModule("Auth");
        }
    }

    /// <summary>
    /// Exception untuk authentication failures
    /// </summary>
    public class AuthenticationException : AuthException
    {
        public AuthenticationException(string message, string errorCode = "AUTH_FAILED")
            : base(message, errorCode, ErrorCategory.Security, 401)
        {
        }

        public AuthenticationException(string message, Exception innerException, string errorCode = "AUTH_FAILED")
            : base(message, innerException, errorCode, ErrorCategory.Security, 401)
        {
        }
    }

    /// <summary>
    /// Exception untuk user registration failures
    /// </summary>
    public class UserRegistrationException : AuthException
    {
        public UserRegistrationException(string message, string errorCode = "REGISTRATION_FAILED")
            : base(message, errorCode, ErrorCategory.Business, 400)
        {
        }

        public UserRegistrationException(string message, Exception innerException, string errorCode = "REGISTRATION_FAILED")
            : base(message, innerException, errorCode, ErrorCategory.Business, 400)
        {
        }
    }

    /// <summary>
    /// Exception untuk user not found scenarios
    /// </summary>
    public class UserNotFoundException : AuthException
    {
        public UserNotFoundException(string userId)
            : base($"User with ID '{userId}' was not found", "USER_NOT_FOUND", ErrorCategory.Business, 404)
        {
            AddDetail("UserId", userId);
        }

        public UserNotFoundException(string message, string errorCode)
            : base(message, errorCode, ErrorCategory.Business, 404)
        {
        }
    }

    /// <summary>
    /// Exception untuk account lockout scenarios
    /// </summary>
    public class AccountLockedException : AuthException
    {
        public AccountLockedException(string email, DateTime? lockoutEnd = null, string errorCode = "ACCOUNT_LOCKED")
            : base($"Account '{email}' is locked out", errorCode, ErrorCategory.Security, 423)
        {
            AddDetail("Email", email);
            if (lockoutEnd.HasValue)
            {
                AddDetail("LockoutEnd", lockoutEnd.Value);
            }
        }
    }

    /// <summary>
    /// Exception untuk inactive account scenarios
    /// </summary>
    public class AccountInactiveException : AuthException
    {
        public AccountInactiveException(string email, string errorCode = "ACCOUNT_INACTIVE")
            : base($"Account '{email}' is inactive", errorCode, ErrorCategory.Business, 403)
        {
            AddDetail("Email", email);
        }
    }

    /// <summary>
    /// Exception untuk role management failures
    /// </summary>
    public class RoleManagementException : AuthException
    {
        public RoleManagementException(string message, string errorCode = "ROLE_MANAGEMENT_FAILED")
            : base(message, errorCode, ErrorCategory.Business, 400)
        {
        }

        public RoleManagementException(string message, Exception innerException, string errorCode = "ROLE_MANAGEMENT_FAILED")
            : base(message, innerException, errorCode, ErrorCategory.Business, 400)
        {
        }
    }

    /// <summary>
    /// Exception untuk duplicate user scenarios
    /// </summary>
    public class DuplicateUserException : AuthException
    {
        public DuplicateUserException(string email, string errorCode = "USER_ALREADY_EXISTS")
            : base($"User with email '{email}' already exists", errorCode, ErrorCategory.Business, 409)
        {
            AddDetail("Email", email);
        }
    }

    /// <summary>
    /// Exception untuk invalid credentials
    /// </summary>
    public class InvalidCredentialsException : AuthException
    {
        public InvalidCredentialsException(string email, string errorCode = "INVALID_CREDENTIALS")
            : base("Invalid email or password", errorCode, ErrorCategory.Security, 401)
        {
            AddDetail("Email", email);
        }
    }

    /// <summary>
    /// Exception untuk password validation failures
    /// </summary>
    public class PasswordValidationException : AuthException
    {
        public PasswordValidationException(string message, List<string> validationErrors, string errorCode = "PASSWORD_VALIDATION_FAILED")
            : base(message, errorCode, ErrorCategory.Validation, 400)
        {
            AddDetail("ValidationErrors", validationErrors);
        }
    }

    /// <summary>
    /// Exception untuk database initialization failures
    /// </summary>
    public class AuthDatabaseException : AuthException
    {
        public AuthDatabaseException(string message, string errorCode = "DATABASE_ERROR")
            : base(message, errorCode, ErrorCategory.Technical, 500)
        {
        }

        public AuthDatabaseException(string message, Exception innerException, string errorCode = "DATABASE_ERROR")
            : base(message, innerException, errorCode, ErrorCategory.Technical, 500)
        {
        }
    }
} 