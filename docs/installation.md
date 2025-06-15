# 🛠️ Installation & Setup Guide

## 📋 **System Requirements**

### **Minimum Requirements**
- **OS**: Windows 10/11, macOS 10.15+, atau Linux (Ubuntu 18.04+)
- **.NET**: .NET 8.0 SDK atau lebih baru
- **Node.js**: Node.js 18.0+ dan npm (untuk Tailwind CSS)
- **Memory**: 4GB RAM minimum, 8GB recommended
- **Storage**: 2GB free space untuk development
- **Database**: SQL Server 2019+, LocalDB, atau PostgreSQL 13+

### **Recommended Development Environment**
- **IDE**: Visual Studio 2022 (17.8+) atau VS Code dengan C# extension
- **Database**: SQL Server Developer Edition atau LocalDB
- **Node.js**: Node.js 20.x LTS (recommended) dengan npm atau yarn
- **Tools**: Git, Docker Desktop (optional)
- **Browser**: Chrome, Firefox, atau Edge untuk testing

## 🚀 **Installation Methods**

### **Method 1: Clone dari Repository (Recommended)**

#### **Step 1: Clone Repository**
```bash
# Clone repository
git clone https://github.com/your-org/setup-micfx.git
cd setup-micfx

# Atau dengan SSH
git clone git@github.com:your-org/setup-micfx.git
cd setup-micfx
```

#### **Step 2: Verify .NET & Node.js Installation**
```bash
# Check .NET version
dotnet --version
# Should show 8.0.x or higher

# List installed SDKs
dotnet --list-sdks

# List installed runtimes
dotnet --list-runtimes

# Check Node.js version
node --version
# Should show v18.0.0 or higher

# Check npm version
npm --version
# Should show 8.0.0 or higher
```

#### **Step 3: Restore Dependencies**
```bash
# Restore NuGet packages
dotnet restore

# Install Node.js dependencies (for Tailwind CSS)
cd src/MicFx.Web
npm install

# Build Tailwind CSS
npm run build

# Return to root directory
cd ../..

# Build solution
dotnet build

# Verify build success
echo $?  # Should return 0 on success
```

#### **Step 4: Setup Database**
```bash
# Update database (uses LocalDB by default)
dotnet ef database update --project src/MicFx.Web

# Or specify connection string
dotnet ef database update --project src/MicFx.Web --connection "Server=localhost;Database=MicFxDb;Trusted_Connection=true;"
```

#### **Step 5: Run Application**
```bash
# Run in development mode
dotnet run --project src/MicFx.Web

# Or with specific environment
dotnet run --project src/MicFx.Web --environment Development
```

### **Method 2: Docker Setup**

#### **Step 1: Create Docker Compose**
```yaml
# docker-compose.yml
version: '3.8'

services:
  micfx-web:
    build:
      context: .
      dockerfile: src/MicFx.Web/Dockerfile
    ports:
      - "5000:8080"
      - "5001:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sql-server;Database=MicFxDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;
    depends_on:
      - sql-server
    volumes:
      - ./logs:/app/logs

  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sql-data:/var/opt/mssql

volumes:
  sql-data:
```

#### **Step 2: Create Dockerfile**
```dockerfile
# src/MicFx.Web/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
    && apt-get install -y nodejs

WORKDIR /src

# Copy project files
COPY ["src/MicFx.Web/MicFx.Web.csproj", "src/MicFx.Web/"]
COPY ["src/MicFx.Core/MicFx.Core.csproj", "src/MicFx.Core/"]
COPY ["src/MicFx.Infrastructure/MicFx.Infrastructure.csproj", "src/MicFx.Infrastructure/"]
COPY ["src/MicFx.Abstractions/MicFx.Abstractions.csproj", "src/MicFx.Abstractions/"]
COPY ["src/MicFx.SharedKernel/MicFx.SharedKernel.csproj", "src/MicFx.SharedKernel/"]
COPY ["src/Modules/MicFx.Modules.Auth/MicFx.Modules.Auth.csproj", "src/Modules/MicFx.Modules.Auth/"]
COPY ["src/Modules/MicFx.Modules.HelloWorld/MicFx.Modules.HelloWorld.csproj", "src/Modules/MicFx.Modules.HelloWorld/"]

# Restore packages
RUN dotnet restore "src/MicFx.Web/MicFx.Web.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/src/MicFx.Web"

# Install Node.js dependencies and build Tailwind CSS
RUN npm install && npm run build

# Build .NET application
RUN dotnet build "MicFx.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MicFx.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicFx.Web.dll"]
```

#### **Step 3: Run with Docker**
```bash
# Build and run with Docker Compose
docker-compose up -d

# View logs
docker-compose logs -f micfx-web

# Stop services
docker-compose down
```

### **Method 3: Template Installation (Future)**

```bash
# Install MicFx template (when available)
dotnet new install MicFx.Templates

# Create new project from template
dotnet new micfx -n MyMicFxApp
cd MyMicFxApp

# Run the application
dotnet run
```

## 🗄️ **Database Setup**

### **SQL Server LocalDB (Default)**
```bash
# Check if LocalDB is installed
sqllocaldb info

# Create LocalDB instance (if needed)
sqllocaldb create MSSQLLocalDB

# Start LocalDB
sqllocaldb start MSSQLLocalDB

# Update connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MicFxDb;Trusted_Connection=true;MultipleActiveResultSets=true;"
}
```

### **SQL Server Express**
```bash
# Download and install SQL Server Express
# https://www.microsoft.com/en-us/sql-server/sql-server-downloads

# Update connection string
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=MicFxDb;Trusted_Connection=true;MultipleActiveResultSets=true;"
}
```

### **SQL Server (Full)**
```bash
# Update connection string with credentials
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=MicFxDb;User Id=your_username;Password=your_password;TrustServerCertificate=true;"
}
```

### **PostgreSQL Setup**
```bash
# Install PostgreSQL
# Windows: Download from https://www.postgresql.org/download/windows/
# macOS: brew install postgresql
# Linux: sudo apt-get install postgresql

# Update connection string
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=MicFxDb;Username=postgres;Password=your_password;"
}

# Install PostgreSQL provider
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### **Database Migration**
```bash
# Create initial migration (if not exists)
dotnet ef migrations add InitialCreate --project src/MicFx.Web

# Update database
dotnet ef database update --project src/MicFx.Web

# Reset database (development only)
dotnet ef database drop --project src/MicFx.Web
dotnet ef database update --project src/MicFx.Web
```

## ⚙️ **Environment Configuration**

### **Development Environment**
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "MicFx": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MicFxDb_Dev;Trusted_Connection=true;"
  },
  "MicFx": {
    "Environment": "Development",
    "EnableSwagger": true,
    "EnableDetailedErrors": true
  }
}
```

### **Production Environment**
```json
// appsettings.Production.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "MicFx": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=MicFxDb;User Id=app_user;Password=secure_password;"
  },
  "MicFx": {
    "Environment": "Production",
    "EnableSwagger": false,
    "EnableDetailedErrors": false
  }
}
```

### **Environment Variables**
```bash
# Set environment variables (Linux/macOS)
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="Server=localhost;Database=MicFxDb;Trusted_Connection=true;"

# Set environment variables (Windows)
set ASPNETCORE_ENVIRONMENT=Development
set ConnectionStrings__DefaultConnection="Server=localhost;Database=MicFxDb;Trusted_Connection=true;"

# PowerShell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=MicFxDb;Trusted_Connection=true;"
```

## 🔧 **IDE Setup**

### **Visual Studio 2022**
1. **Install Required Workloads**:
   - ASP.NET and web development
   - .NET desktop development
   - Data storage and processing

2. **Recommended Extensions**:
   - Web Essentials
   - Productivity Power Tools
   - CodeMaid
   - SonarLint

3. **Configure Debugging**:
   ```json
   // launchSettings.json
   {
     "profiles": {
       "MicFx.Web": {
         "commandName": "Project",
         "dotnetRunMessages": true,
         "launchBrowser": true,
         "applicationUrl": "https://localhost:5001;http://localhost:5000",
         "environmentVariables": {
           "ASPNETCORE_ENVIRONMENT": "Development"
         }
       }
     }
   }
   ```

### **VS Code**
1. **Install Extensions**:
   - C# for Visual Studio Code
   - .NET Install Tool
   - REST Client
   - GitLens
   - Bracket Pair Colorizer
   - Tailwind CSS IntelliSense
   - npm Intellisense
   - Auto Rename Tag

2. **Configure Settings**:
   ```json
   // .vscode/settings.json
   {
     "dotnet.defaultSolution": "MicFx.sln",
     "omnisharp.enableRoslynAnalyzers": true,
     "omnisharp.enableEditorConfigSupport": true,
     "files.exclude": {
       "**/bin": true,
       "**/obj": true
     }
   }
   ```

3. **Configure Tasks**:
   ```json
   // .vscode/tasks.json
   {
     "version": "2.0.0",
     "tasks": [
       {
         "label": "build",
         "command": "dotnet",
         "type": "process",
         "args": ["build", "${workspaceFolder}/MicFx.sln"],
         "problemMatcher": "$msCompile"
       },
       {
         "label": "run",
         "command": "dotnet",
         "type": "process",
         "args": ["run", "--project", "${workspaceFolder}/src/MicFx.Web"],
         "problemMatcher": "$msCompile"
       },
       {
         "label": "watch-css",
         "command": "npm",
         "type": "process",
         "args": ["run", "watch"],
         "options": {
           "cwd": "${workspaceFolder}/src/MicFx.Web"
         },
         "isBackground": true,
         "problemMatcher": []
       }
     ]
   }
   ```

## 🧪 **Verification Steps**

### **1. Application Health Check**
```bash
# Check if application is running
curl http://localhost:5000/health

# Expected response
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "database": {
      "status": "Healthy"
    },
    "auth_module": {
      "status": "Healthy"
    }
  }
}
```

### **2. API Endpoints**
```bash
# Test HelloWorld API
curl http://localhost:5000/api/hello-world/greeting

# Test Auth API (should require authentication)
curl http://localhost:5000/api/auth/status
```

### **3. Web Pages**
- Navigate to http://localhost:5000 (Home page)
- Navigate to http://localhost:5000/admin (Admin panel)
- Navigate to http://localhost:5000/swagger (API documentation)

### **4. Database Verification**
```sql
-- Connect to database and verify tables
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME LIKE 'Auth_%';

-- Should show Auth module tables:
-- Auth_Users, Auth_Roles, Auth_Permissions, etc.
```

## 🚨 **Troubleshooting**

### **Common Issues**

#### **1. .NET SDK Not Found**
```bash
# Error: The command could not be loaded, possibly because:
# * You intended to execute a .NET application

# Solution: Install .NET 8.0 SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/8.0
```

#### **2. Node.js Not Found**
```bash
# Error: 'node' is not recognized as an internal or external command

# Solutions:
# Windows: Download from https://nodejs.org/
# macOS: brew install node
# Linux: sudo apt-get install nodejs npm

# Verify installation
node --version
npm --version
```

#### **3. Database Connection Failed**
```bash
# Error: A network-related or instance-specific error occurred

# Solutions:
# 1. Check if SQL Server is running
net start MSSQLSERVER

# 2. Verify connection string
# 3. Check firewall settings
# 4. Ensure database exists
```

#### **4. Port Already in Use**
```bash
# Error: Unable to bind to https://localhost:5001

# Solution: Change port in launchSettings.json
"applicationUrl": "https://localhost:5002;http://localhost:5003"
```

#### **5. Package Restore Failed**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore --force

# Clear npm cache (if Node.js issues)
npm cache clean --force

# Reinstall Node.js packages
cd src/MicFx.Web
rm -rf node_modules package-lock.json
npm install
cd ../..
```

#### **6. Migration Failed**
```bash
# Drop and recreate database (development only)
dotnet ef database drop --project src/MicFx.Web --force
dotnet ef database update --project src/MicFx.Web
```

#### **7. Tailwind CSS Not Working**
```bash
# Error: CSS styles not applying

# Solutions:
# 1. Ensure Node.js dependencies are installed
cd src/MicFx.Web
npm install

# 2. Build Tailwind CSS
npm run build

# 3. Check if CSS file is generated
ls -la wwwroot/css/site.css

# 4. Restart application after CSS changes
dotnet run --project ../MicFx.Web

# 5. For development, use watch mode
npm run watch
```

### **Performance Issues**

#### **1. Slow Startup**
- Check antivirus exclusions for project folder
- Disable unnecessary Visual Studio extensions
- Use SSD for development

#### **2. High Memory Usage**
- Increase available memory
- Close unnecessary applications
- Use Release configuration for testing

## 📚 **Next Steps**

After successful installation:

1. **Read Getting Started Guide**: [getting-started.md](getting-started.md)
2. **Configure Application**: [configuration.md](configuration.md)
3. **Setup Frontend Development**: Run `npm run watch` untuk Tailwind CSS development
4. **Explore Modules**: Browse `/src/Modules/` folder
5. **Check Admin Panel**: Visit `/admin` for management interface
6. **Review API Documentation**: Visit `/swagger` for API reference
7. **Customize UI**: Edit Tailwind CSS classes untuk styling

## 🆘 **Support**

If you encounter issues:

1. **Check Logs**: Review application logs in `/logs` folder
2. **Verify Requirements**: Ensure all system requirements are met
3. **Update Dependencies**: Run `dotnet restore` and `dotnet build`
4. **Reset Environment**: Try with fresh database and clean build
5. **Seek Help**: Check documentation or contact support team

---

**Installation complete! Ready to start developing with MicFx Framework.** 🎉 