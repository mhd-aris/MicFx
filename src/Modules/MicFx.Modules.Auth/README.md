# 🔐 MicFx.Modules.Auth

## 📝 Overview

**MicFx.Modules.Auth** adalah module inti untuk sistem autentikasi dan autorisasi dalam MicFx Framework. Module ini menggunakan ASP.NET Core Identity dan menyediakan fitur lengkap untuk manajemen user, role-based access control (RBAC), serta integrasi seamless dengan admin panel.

## ✨ Features

### 🔐 Authentication Features
- ✅ **Login/Logout** - Sistem login dengan email dan password
- ✅ **User Registration** - Registrasi user baru dengan validasi
- ✅ **Role-based Authorization** - SuperAdmin, Admin, User, dan custom roles
- ✅ **Cookie Authentication** - Secure cookie-based authentication
- ✅ **Account Lockout** - Protection dari brute force attacks
- ✅ **Password Validation** - Configurable password requirements
- ✅ **Account Status Management** - Active/inactive user management

### 👥 User Management Features
- ✅ **User CRUD Operations** - Create, Read, Update, Delete users
- ✅ **User Profile Management** - FirstName, LastName, Department, JobTitle
- ✅ **User Search & Pagination** - Efficient user listing dengan search
- ✅ **Role Assignment** - Assign/remove roles dari users
- ✅ **User Activity Tracking** - CreatedAt, UpdatedAt, LastLoginAt
- ✅ **Bulk Operations** - Mass user status updates

### 🛡️ Role & Permission Management
- ✅ **Role CRUD Operations** - Complete role management
- ✅ **Permission System** - Granular permission control
- ✅ **System Roles** - Protected system roles (SuperAdmin, Admin)
- ✅ **Custom Roles** - User-defined roles dengan custom permissions
- ✅ **Role Hierarchy** - Priority-based role ordering
- ✅ **Permission Categories** - Organized permission grouping

### 🎛️ Admin Panel Integration
- ✅ **Admin Dashboard** - Statistics dan overview
- ✅ **User Management UI** - Complete user management interface
- ✅ **Role Management UI** - Role dan permission management
- ✅ **Navigation Integration** - Auto-discovered admin navigation
- ✅ **Responsive Design** - Mobile-friendly admin interface

## 🏗️ Architecture

### Module Structure
```
src/Modules/MicFx.Modules.Auth/
├── Areas/Admin/                    # Admin panel controllers & views
│   ├── Controllers/
│   │   ├── DashboardController.cs  # Auth dashboard
│   │   ├── UserManagementController.cs
│   │   └── RoleManagementController.cs
│   ├── Views/                      # Razor views untuk admin UI
│   │   ├── UserManagement/
│   │   └── RoleManagement/
│   └── AuthAdminNavContributor.cs  # Navigation integration
├── Api/                           # API controllers (future)
├── Controllers/                   # MVC controllers
│   └── AuthController.cs          # Public auth endpoints
├── Data/                         # Database context & migrations
│   ├── AuthDbContext.cs
│   └── Migrations/
├── Domain/                       # Domain models & DTOs
│   ├── Entities/                 # User, Role entities
│   ├── DTOs/                     # Data transfer objects
│   ├── Configuration/            # AuthConfig
│   └── Exceptions/               # Custom exceptions
├── Services/                     # Business logic services
│   ├── AuthService.cs
│   ├── AuthHealthCheck.cs
│   └── AuthDatabaseInitializer.cs
├── Views/                        # Public auth views
│   └── Auth/                     # Login, Register, etc.
├── Manifest.cs                   # Module manifest
└── Startup.cs                    # Module configuration
```

### Database Integration

**Important**: Module Auth **TIDAK menggunakan connection string terpisah**. Module ini menggunakan **shared database** dengan connection string `DefaultConnection` yang dikonfigurasi di host application.

#### Database Tables
Module akan membuat tables berikut di shared database:
```sql
-- ASP.NET Core Identity Tables
Users                 # User accounts (extended)
Roles                 # User roles (extended)
AspNetUserRoles      # User-Role relationships
AspNetUserClaims     # User claims
AspNetUserLogins     # External login providers
AspNetUserTokens     # User tokens
AspNetRoleClaims     # Role claims

-- Custom Auth Tables
Permissions          # Available permissions
RolePermissions      # Role-Permission relationships
```

### Key Components

#### **1. Entities**
- `User` - Extended IdentityUser dengan custom properties (FirstName, LastName, Department, JobTitle, IsActive)
- `Role` - Extended IdentityRole dengan system role support dan priority
- `Permission` - Granular permission definitions
- `RolePermission` - Many-to-many relationship

#### **2. Services**
- `AuthService` - Core authentication business logic
- `AuthHealthCheck` - Module health monitoring
- `AuthDatabaseInitializer` - Database seeding dan default data

#### **3. Configuration**
- `AuthConfig` - Strongly-typed configuration
- Password policies, lockout settings, cookie configuration

## 🚀 Quick Start

### 1. Configuration

Module Auth dikonfigurasi melalui section `MicFx:Auth` di `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MicFxApp;Trusted_Connection=true;"
  },
  "MicFx": {
    "Auth": {
      "RoutePrefix": "auth",
      "Password": {
        "RequiredLength": 6,
        "RequireDigit": true,
        "RequireUppercase": true,
        "RequireLowercase": true,
        "RequireNonAlphanumeric": false
      },
      "Lockout": {
        "MaxFailedAccessAttempts": 5,
        "DefaultLockoutTimeSpanMinutes": 5
      },
      "Cookie": {
        "LoginPath": "/auth/login",
        "LogoutPath": "/auth/logout",
        "AccessDeniedPath": "/auth/access-denied",
        "ExpireTimeSpanHours": 2,
        "CookieName": "MicFx.Auth"
      },
      "DefaultRoles": ["SuperAdmin", "Admin", "User"],
      "DefaultAdmin": {
        "Email": "admin@micfx.dev",
        "Password": "Admin123!",
        "FirstName": "System",
        "LastName": "Admin"
      }
    }
  }
}
```

### 2. Database Migration

Module akan otomatis membuat tables yang diperlukan saat aplikasi pertama kali dijalankan. Tidak perlu manual migration.

### 3. Default Users

Module akan otomatis membuat default admin user:
- **Email**: `admin@micfx.dev`
- **Password**: `Admin123!`
- **Role**: SuperAdmin

## 💻 Usage Examples

### Authentication Service

```csharp
public class HomeController : Controller
{
    private readonly IAuthService _authService;

    public HomeController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return RedirectToAction("Index", "Dashboard");
        }
        catch (InvalidCredentialsException)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View(request);
        }
        catch (AccountLockedException ex)
        {
            ModelState.AddModelError("", $"Account is locked until {ex.Details["LockoutEnd"]}");
            return View(request);
        }
    }
}
```

### User Management

```csharp
// Get user information
var userInfo = await _authService.GetUserInfoAsync(userId);

// Assign role to user
await _authService.AssignRoleToUserAsync(userId, "Admin");

// Set user active status
await _authService.SetUserActiveStatusAsync(userId, true);

// Get user roles
var roles = await _authService.GetUserRolesAsync(userId);
```

### Authorization

```csharp
// Controller level authorization
[Authorize(Policy = AuthorizationPolicyService.AdminAreaPolicy)]
public class AdminController : Controller { }

// Action level authorization
[Authorize(Policy = AuthorizationPolicyService.SuperAdminPolicy)]
public async Task<IActionResult> DeleteUser(string id) { }

// Role-based authorization
[Authorize(Roles = "Admin,SuperAdmin")]
public async Task<IActionResult> ManageUsers() { }
```

## 🔧 Configuration Options

### Password Policy
```json
{
  "Password": {
    "RequiredLength": 8,           // Minimum password length
    "RequireDigit": true,          // Require at least one digit
    "RequireLowercase": true,      // Require lowercase letter
    "RequireUppercase": true,      // Require uppercase letter
    "RequireNonAlphanumeric": true // Require special character
  }
}
```

### Account Lockout
```json
{
  "Lockout": {
    "DefaultLockoutTimeSpanMinutes": 15,  // 15 minutes lockout
    "MaxFailedAccessAttempts": 5,         // Max failed attempts
    "AllowedForNewUsers": true            // Enable for new users
  }
}
```

### Cookie Settings
```json
{
  "Cookie": {
    "CookieName": "MicFx.Auth",        // Cookie name
    "ExpireTimeSpanHours": 24,         // 24 hours expiry
    "LoginPath": "/auth/login",        // Login page path
    "LogoutPath": "/auth/logout",      // Logout path
    "AccessDeniedPath": "/auth/access-denied"  // Access denied path
  }
}
```

## 🛡️ Security Features

### Exception Handling
Module menggunakan custom exceptions yang inherit dari `MicFxException`:

```csharp
// Authentication failures
throw new InvalidCredentialsException(email);

// Account status issues
throw new AccountLockedException(email, lockoutEnd);
throw new AccountInactiveException(email);

// User management errors
throw new UserNotFoundException(userId);
throw new DuplicateUserException(email);

// Role management errors
throw new RoleManagementException("Failed to assign role");
```

### Input Validation
- Email format validation
- Password strength requirements
- XSS protection dengan anti-forgery tokens
- SQL injection protection dengan Entity Framework

### Authorization Policies
```csharp
// Built-in policies
AuthorizationPolicyService.AdminAreaPolicy      // Admin + SuperAdmin
AuthorizationPolicyService.SuperAdminPolicy     // SuperAdmin only
AuthorizationPolicyService.UserManagementPolicy // User management access
```

## 📊 Health Checks

Module menyediakan health check untuk monitoring:

```csharp
// Check database connectivity
// Check Identity services
// Check basic functionality
```

Access health check: `/health` atau `/health/auth`

## 🎨 Admin UI

### Navigation Structure
```
Authentication
├── Auth Dashboard           # /admin/auth (redirects to roles)
├── User Management         # /admin/auth/users
│   ├── User List
│   ├── User Details
│   ├── Edit User
│   └── User Roles
└── Role Management         # /admin/auth/roles
    ├── Role List
    ├── Role Details
    ├── Create Role
    ├── Edit Role
    └── Role Permissions
```

### Features
- 📱 **Responsive Design** - Mobile-friendly interface
- 🔍 **Search & Filter** - Quick user/role search
- 📄 **Pagination** - Efficient large dataset handling
- 📊 **Statistics** - User dan role statistics
- 🎯 **Bulk Actions** - Mass operations support
- ⚡ **Real-time Updates** - Live status updates

## 🧪 Testing

### Manual Testing
1. **Login**: Akses `/auth/login` dengan credentials default
2. **Admin Panel**: Akses `/admin` setelah login sebagai admin
3. **User Management**: Test CRUD operations di `/admin/auth/users`
4. **Role Management**: Test role assignment di `/admin/auth/roles`

### Test Scenarios
- ✅ Authentication flows
- ✅ User management operations
- ✅ Role assignment/removal
- ✅ Exception handling
- ✅ Configuration validation

## 🚀 Deployment

### Production Checklist
- [ ] Update default admin password
- [ ] Configure secure connection string
- [ ] Enable HTTPS only
- [ ] Set production cookie settings
- [ ] Configure proper logging
- [ ] Set up health check monitoring
- [ ] Review security policies

### Environment Variables
```bash
# Database (shared)
ConnectionStrings__DefaultConnection="Server=prod;Database=MicFxApp;User Id=app;Password=SecurePass123!;"

# Auth Configuration
MicFx__Auth__DefaultAdmin__Password="NewSecurePassword123!"
MicFx__Auth__Cookie__CookieName="MicFxAuth"
MicFx__Auth__Password__RequiredLength="12"
```

## 🤝 Contributing

### Development Setup
1. Clone repository
2. Install dependencies: `dotnet restore`
3. Configure `appsettings.json` dengan connection string yang valid
4. Run application: `dotnet run --project src/MixFc.Web`
5. Access admin panel: `/admin` (login dengan `admin@micfx.dev` / `Admin123!`)

### Code Standards
- Follow C# coding conventions
- Use proper exception handling dengan MicFxException
- Add XML documentation
- Write unit tests
- Update documentation

### Module Architecture
Module Auth mengikuti MicFx module pattern:
- `Manifest.cs` - Module metadata dan capabilities
- `Startup.cs` - Module configuration dan service registration
- Menggunakan shared database (tidak ada connection string terpisah)
- Auto-discovery untuk admin navigation
- Structured exception handling
- Health check integration

## 📚 Additional Resources

- [MicFx Framework Documentation](../../../docs/)
- [ASP.NET Core Identity Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)

## 📄 License

This module is part of the MicFx Framework and follows the same license terms.

---

**Version**: 1.0.0  
**Last Updated**: December 2024  
**Maintainer**: MicFx Team 