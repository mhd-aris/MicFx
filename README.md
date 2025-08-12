# 🚀 MicFX Framework

**Modern Modular Framework for .NET 8** - Building scalable applications with clean architecture and powerful developer experience.

[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](https://github.com/micfx/micfx)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## 📋 Table of Contents

- [Quick Start](#-quick-start)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Project Structure](#-project-structure)
- [Configuration](#-configuration)
- [Running the Application](#-running-the-application)
- [Development Guide](#-development-guide)
- [Module Development](#-module-development)
- [Testing](#-testing)
- [Deployment](#-deployment)
- [Contributing](#-contributing)

---

## 🚀 Quick Start

### 1. Clone Repository

```bash
# Clone from Github
git clone git@github.com:mhd-aris/MicFx.git
cd micfx

# Or using HTTPS
git clone https://github.com/mhd-aris/MicFx.git
cd micfx
```

### 2. Setup & Run

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run the application
dotnet run --project src/MicFx.Web
```

🎉 **Application will be available at:** `https://localhost:7001` or `http://localhost:5001`

---

## 📋 Prerequisites

### Required Software

- **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)** (8.0 or later)
- **[Git](https://git-scm.com/)** for version control
- **[SQL Server](https://www.microsoft.com/sql-server)** or **[SQL Server LocalDB](https://docs.microsoft.com/sql-server/express-localdb)** for database

### Recommended Tools

- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** or **[Visual Studio Code](https://code.visualstudio.com/)**
- **[SQL Server Management Studio (SSMS)](https://docs.microsoft.com/sql-server/ssms/)**
- **[Postman](https://www.postman.com/)** for API testing

### Verify Installation

```bash
# Check .NET version
dotnet --version
# Should output: 8.0.x or higher

# Check Git
git --version


```

---

## 🛠️ Installation

### 1. Clone & Setup

```bash
# Clone the repository
git clone https://github.com/mhd-aris/MicFx.git
cd micfx

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build
```

### 2. Database Setup

#### Option A: SQL Server LocalDB (Recommended for Development)

```bash
# No additional setup needed - LocalDB will be created automatically
dotnet run --project src/MicFx.Web
```

#### Option B: SQL Server Instance

1. **Update Connection String** in `src/MicFx.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MicFxDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

2. **Run Migrations**:

```bash
# The application will automatically create and seed the database
dotnet run --project src/MicFx.Web
```

### 3. Verify Setup

```bash
# Test build
dotnet build --configuration Release

# Run tests
dotnet test

# Check application startup
dotnet run --project src/MicFx.Web --urls "http://localhost:5000"
```

---

## 📁 Project Structure

```
micfx/
├── 📁 src/                          # Source code
│   ├── 📁 MicFx.Abstractions/       # Core abstractions & interfaces
│   ├── 📁 MicFx.SharedKernel/       # Shared domain logic
│   ├── 📁 MicFx.Core/               # Core business logic & modularity
│   ├── 📁 MicFx.Infrastructure/     # Infrastructure implementations
│   ├── 📁 MicFx.Web/                # Web application (Startup project)
│   └── 📁 Modules/                  # Feature modules
│       ├── 📁 MicFx.Modules.Auth/   # Authentication & authorization
│       └── 📁 MicFx.Modules.HelloWorld/ # Example module
├── 📁 tests/                        # Test projects
│   └── 📁 MicFx.Tests.Core/         # Core functionality tests
├── 📁 templates/                    # Project templates
│   └── 📁 MicFx.Module.Template/    # Module template
├── 📁 scripts/                      # Development scripts
├── 📁 docs/                         # Documentation
├── 🔧 MicFx.sln                     # Solution file
├── 🔧 Directory.Packages.props      # Centralized package management
├── 🔧 global.json                   # .NET SDK version
└── 📖 README.md                     # This file
```

### Key Components

| Component | Purpose | Technology |
|-----------|---------|------------|
| **MicFx.Web** | Web application & API | ASP.NET Core 8, MVC |
| **MicFx.Core** | Business logic & modularity | Clean Architecture |
| **MicFx.Infrastructure** | Data access & external services | Entity Framework Core |
| **MicFx.Modules.Auth** | Authentication & permissions | JWT, Claims-based |
| **Module System** | Pluggable feature modules | Dynamic loading |

---

## ⚙️ Configuration

### Environment Settings

The application supports multiple environments with specific configuration files:

```
📁 src/MicFx.Web/
├── appsettings.json              # Base configuration
├── appsettings.Development.json  # Development overrides
├── appsettings.Staging.json      # Staging overrides
└── appsettings.Production.json   # Production overrides
```

### Key Configuration Sections

#### Database Connection

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MicFxDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

#### JWT Authentication

```json
{
  "Jwt": {
    "Key": "your-super-secret-jwt-key-that-is-at-least-32-characters-long",
    "Issuer": "MicFx",
    "Audience": "MicFxUsers",
    "ExpiryInMinutes": 60
  }
}
```

#### Logging

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "MicFx": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

---

## 🏃‍♂️ Running the Application

### Development Mode

```bash
# Run with hot reload
dotnet watch run --project src/MicFx.Web

# Or run normally
dotnet run --project src/MicFx.Web
```

**Access Points:**
- **Web Application:** https://localhost:7001
- **API Documentation:** https://localhost:7001/api/docs
- **Health Checks:** https://localhost:7001/health

### Production Mode

```bash
# Build for production
dotnet build --configuration Release

# Run production build
dotnet run --project src/MicFx.Web --configuration Release --urls "https://0.0.0.0:443;http://0.0.0.0:80"
```

### Docker (Optional)

```bash
# Build Docker image
docker build -t micfx-app .

# Run container
docker run -p 8080:80 -p 8443:443 micfx-app
```

### First Run Setup

When you run the application for the first time:

1. **Database Creation:** Automatically creates and seeds the database
2. **Default Admin User:** 
   - **Email:** `admin@micfx.com`
   - **Password:** `Admin123!`
3. **Sample Data:** Creates sample roles and permissions

---

## 👨‍💻 Development Guide

### Getting Started with Development

1. **Fork the Repository**
2. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Development Environment**
   ```bash
   # Install development tools
   dotnet tool restore
   
   # Run with file watching
   dotnet watch run --project src/MicFx.Web
   ```

### Code Style & Standards

- **Language:** C# 
- **Framework:** .NET 8 with minimal APIs where appropriate
- **Architecture:** Clean Architecture with CQRS patterns
- **Testing:** xUnit with FluentAssertions
- **Documentation:** XML comments for public APIs

### Debugging

#### Visual Studio
1. Set `MicFx.Web` as startup project
2. Press F5 to start debugging

#### Visual Studio Code
1. Use `.vscode/launch.json` configuration
2. Press F5 or use Debug panel

#### Command Line
```bash
# Run with debugger attached
dotnet run --project src/MicFx.Web --verbosity diagnostic
```

---

## 🧩 Module Development

MicFX uses a powerful modular architecture. Create new modules easily:


This creates:
- ✅ **Controller** with CRUD operations
- ✅ **Models** and DTOs
- ✅ **Permissions** with IntelliSense
- ✅ **Database** integration
- ✅ **Tests** structure

### Manual Module Creation

1. **Create Module Structure**
   ```
   src/Modules/MicFx.Modules.YourModule/
   ├── Controllers/
   ├── Models/
   ├── Domain/
   │   └── Permissions/
   ├── Infrastructure/
   └── MicFx.Modules.YourModule.csproj
   ```

2. **Define Permissions**
   ```csharp
   [PermissionModule("PRODUCTS")]
   public static class ProductPermissions
   {
       public const string VIEW = "PRODUCTS.VIEW";
       public const string CREATE = "PRODUCTS.CREATE";
       public const string EDIT = "PRODUCTS.EDIT";
       public const string DELETE = "PRODUCTS.DELETE";
   }
   ```

3. **Create Controller**
   ```csharp
   [ApiController]
   [Route("api/[controller]")]
   public class ProductsController : ControllerBase
   {
       [HttpGet]
       [RequirePermission(ProductPermissions.VIEW)]
       public async Task<IActionResult> GetAll()
       {
           // Implementation
       }
   }
   ```

### Module Guidelines

- **Follow naming conventions:** `MicFx.Modules.{ModuleName}`
- **Use permission constants:** Type-safe with IntelliSense
- **Implement seeding:** Consistent data initialization
- **Add tests:** Ensure reliability

---

## 🧪 Testing

### Run All Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/MicFx.Tests.Core/
```

### Test Categories

#### Unit Tests
```bash
# Core business logic tests
dotnet test tests/MicFx.Tests.Core/ --filter Category=Unit
```

#### Integration Tests
```bash
# Database and API integration tests
dotnet test tests/MicFx.Tests.Core/ --filter Category=Integration
```

#### End-to-End Tests
```bash
# Full application workflow tests
dotnet test tests/MicFx.Tests.Core/ --filter Category=E2E
```

### Writing Tests

```csharp
[Fact]
public async Task CreateUser_WithValidData_ShouldReturnSuccess()
{
    // Arrange
    var request = new CreateUserRequest("test@example.com", "Test User");
    
    // Act
    var result = await _userService.CreateAsync(request);
    
    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
}
```

---

## 🚀 Deployment

### Environment Preparation

#### Staging
```bash
# Build for staging
dotnet publish src/MicFx.Web -c Release -o ./publish/staging

# Deploy to staging environment
# (Copy files to staging server)
```

#### Production
```bash
# Build for production
dotnet publish src/MicFx.Web -c Release -o ./publish/production

# Deploy to production environment
# (Copy files to production server)
```

### Database Migration

```bash
# Generate migration script
dotnet ef migrations script --output migration.sql --project src/MicFx.Infrastructure

# Apply to production database
# (Execute migration.sql on production database)
```
