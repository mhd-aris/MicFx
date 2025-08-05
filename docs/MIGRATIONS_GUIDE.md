# 📦 MICFX - Migration Guide

**Panduan lengkap untuk membuat dan mengelola migrations di MICFX Framework**

---

## 🎯 Overview

MICFX menggunakan **Entity Framework Core** untuk database migrations. Setiap module memiliki **DbContext terpisah** yang memungkinkan migrations independen per module.

### ⚠️ **PENTING: 1 FILE = 1 MIGRATION**
- **Satu migration = satu file .cs**
- **Satu tujuan = satu migration**
- **Jangan gabungkan multiple changes dalam 1 migration**

---

## 🏗️ Architecture Overview

```
src/Modules/
├── MicFx.Modules.Auth/
│   ├── Data/
│   │   ├── AuthDbContext.cs          # ✅ DbContext untuk Auth
│   │   └── Migrations/               # ✅ Auto-generated migrations
│   └── Startup.cs                    # ✅ DbContext registration
├── MicFx.Modules.Products/           # Example module
│   ├── Data/
│   │   ├── ProductsDbContext.cs      # ✅ DbContext untuk Products
│   │   └── Migrations/               # ✅ Auto-generated migrations
```

**Key Points:**
- ✅ **1 Module = 1 DbContext = 1 Migrations folder**
- ✅ **1 Change = 1 Migration file**
- ✅ **Setiap migration file independent**

---

## 🚀 Quick Start: Membuat Migration Baru

### **Step 1: Identifikasi Module Target**

Tentukan module mana yang akan diubah database-nya:

```bash
# Contoh: Module Auth
cd src/Modules/MicFx.Modules.Auth

# Contoh: Module Products (jika ada)
cd src/Modules/MicFx.Modules.Products
```

### **Step 2: Buat Migration (1 FILE)**

**Template Command:**
```bash
dotnet ef migrations add [MigrationName] \
  --project . \
  --startup-project ../../MicFx.Web \
  --context [ModuleDbContext] \
  --output-dir Data/Migrations
```

**✅ CONTOH SUKSES - Auth Module:**
```bash
# Command yang berhasil dijalankan
cd src/Modules/MicFx.Modules.Auth
dotnet ef migrations add InitialAuthSchema \
  --project . \
  --startup-project ../../MicFx.Web \
  --context AuthDbContext \
  --output-dir Data/Migrations
```

**Generated Files:**
- `20250805184037_InitialAuthSchema.cs` ✅
- `20250805184037_InitialAuthSchema.Designer.cs` ✅  
- `AuthDbContextModelSnapshot.cs` ✅

### **Step 3: Contoh Real Implementation**

#### 🔐 **Auth Module - Add New User Field**

**Scenario:** Menambahkan field `PhoneNumber` ke tabel Users

```bash
# 1. Navigate to Auth module
cd src/Modules/MicFx.Modules.Auth

# 2. Create migration (1 FILE untuk 1 perubahan)
dotnet ef migrations add AddPhoneNumberToUsers \
  --project . \
  --startup-project ../../MicFx.Web \
  --context AuthDbContext \
  --output-dir Data/Migrations
```

**Generated File:** `20250806123456_AddPhoneNumberToUsers.cs`

#### 📦 **Products Module - Create Product Table**

```bash
# 1. Navigate to Products module
cd src/Modules/MicFx.Modules.Products

# 2. Create migration (1 FILE untuk 1 table)
dotnet ef migrations add CreateProductTable \
  --project . \
  --startup-project ../../MicFx.Web \
  --context ProductsDbContext \
  --output-dir Data/Migrations
```

**Generated File:** `20250806123457_CreateProductTable.cs`

---

## 📝 Migration Naming Conventions

### ✅ **GOOD Examples (1 Purpose)**

```bash
# ✅ Adding single field
dotnet ef migrations add AddEmailVerifiedToUsers

# ✅ Creating single table  
dotnet ef migrations add CreateProductsTable

# ✅ Adding single index
dotnet ef migrations add AddIndexOnUserEmail

# ✅ Removing single column
dotnet ef migrations add RemoveOldPasswordFromUsers

# ✅ Adding single relationship
dotnet ef migrations add AddUserToOrdersRelationship
```

### ❌ **BAD Examples (Multiple Purposes)**

```bash
# ❌ Multiple changes in 1 migration
dotnet ef migrations add AddFieldsAndCreateTablesAndIndexes

# ❌ Vague naming
dotnet ef migrations add UpdateDatabase

# ❌ Multiple tables
dotnet ef migrations add CreateUsersAndProductsAndOrders
```

---

## 🗂️ File Structure Setelah Migration

```
src/Modules/MicFx.Modules.Auth/Data/Migrations/
├── 20250806120000_InitialCreate.cs                # ✅ 1 FILE: Initial setup
├── 20250806121000_AddPhoneNumberToUsers.cs        # ✅ 1 FILE: Add phone field
├── 20250806122000_CreateRolesTable.cs             # ✅ 1 FILE: Create roles
├── 20250806123000_AddIndexOnUserEmail.cs          # ✅ 1 FILE: Add index
└── AuthDbContextModelSnapshot.cs                  # ✅ Auto-generated snapshot
```

**Key Points:**
- ✅ **Setiap file memiliki purpose yang jelas**
- ✅ **Timestamp automatic dari EF Core**
- ✅ **1 migration = 1 database change**

---

## ⚡ Quick Commands Reference

### **Template untuk Module Baru**

```bash
# 1. Navigate to module
cd src/Modules/[ModuleName]

# 2. Add migration (replace placeholders)
dotnet ef migrations add [MigrationName] \
  --project . \
  --startup-project ../../MicFx.Web \
  --context [ModuleName]DbContext \
  --output-dir Data/Migrations

# 3. Apply migration
dotnet ef database update \
  --project . \
  --startup-project ../../MicFx.Web \
  --context [ModuleName]DbContext
```

### **Common Scenarios (Copy-Paste Ready)**

#### 🔐 **Auth Module**
```bash
cd src/Modules/MicFx.Modules.Auth

# Add field to Users
dotnet ef migrations add AddFieldToUsers \
  --project . --startup-project ../../MicFx.Web \
  --context AuthDbContext --output-dir Data/Migrations

# Apply migration
dotnet ef database update \
  --project . --startup-project ../../MicFx.Web \
  --context AuthDbContext
```

#### 👋 **HelloWorld Module**
```bash
cd src/Modules/MicFx.Modules.HelloWorld

# Create Greetings table
dotnet ef migrations add CreateGreetingsTable \
  --project . --startup-project ../../MicFx.Web \
  --context HelloWorldDbContext --output-dir Data/Migrations

# Apply migration
dotnet ef database update \
  --project . --startup-project ../../MicFx.Web \
  --context HelloWorldDbContext
```

---

## 🔍 Best Practices

### **1. Pre-Migration Checklist**

✅ **Sebelum membuat migration:**
- [ ] Pastikan perubahan model sudah final
- [ ] Backup database jika production
- [ ] Test di development environment dulu
- [ ] Pastikan 1 migration = 1 purpose

### **2. Migration Naming**

✅ **Gunakan format:**
- `Add[Field]To[Table]` - untuk menambah field
- `Remove[Field]From[Table]` - untuk hapus field  
- `Create[Table]Table` - untuk buat table baru
- `AddIndexOn[Field]` - untuk menambah index
- `Update[Table][Purpose]` - untuk update specific

### **3. Safety Practices**

✅ **Development:**
```bash
# Preview migration sebelum apply
dotnet ef migrations script \
  --project . --startup-project ../../MicFx.Web \
  --context AuthDbContext
```

✅ **Production:**
```bash
# Generate script untuk manual review
dotnet ef migrations script \
  --project . --startup-project ../../MicFx.Web \
  --context AuthDbContext \
  --output migration.sql
```

---

## 🚨 Troubleshooting

### **Error: Unable to create DbContext**

```
Unable to create a 'DbContext' of type 'AuthDbContext'. 
The exception '/path/to/project/' was thrown while attempting to create an instance.
```

**Root Cause:** EF Core tidak bisa membuat DbContext instance pada design time.

**Important:** EF Core migration tools selalu berjalan dari **startup project directory** (MicFx.Web), bukan dari module directory. Ini mengapa `IDesignTimeDbContextFactory` harus mencari `appsettings.json` di current directory (yang adalah Web project).

**Solution:** Pastikan `IDesignTimeDbContextFactory` sudah ada dan benar:

```csharp
// File: Data/[ModuleName]DbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MicFx.Modules.[ModuleName].Data;

/// <summary>
/// Design-time factory for [ModuleName]DbContext to enable EF Core migrations
/// Best practice implementation for MICFX modules
/// </summary>
public class [ModuleName]DbContextFactory : IDesignTimeDbContextFactory<[ModuleName]DbContext>
{
    public [ModuleName]DbContext CreateDbContext(string[] args)
    {
        try
        {
            var configuration = BuildConfiguration();
            var connectionString = GetConnectionString(configuration);
            
            // Configure DbContext
            var optionsBuilder = new DbContextOptionsBuilder<[ModuleName]DbContext>();
            optionsBuilder.UseSqlServer(connectionString, options =>
            {
                options.MigrationsAssembly(typeof([ModuleName]DbContext).Assembly.FullName);
            });

            return new [ModuleName]DbContext(optionsBuilder.Options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateDbContext: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            return new ConfigurationBuilder()
                .SetBasePath(currentDir)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        throw new DirectoryNotFoundException(
            $"Web project not found. Current directory: {currentDir}. " +
            "Expected appsettings.json in current directory.");
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var configConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(configConnection))
            return configConnection;

        throw new InvalidOperationException(
            "No connection string found. Please ensure:\n" +
            "1. DefaultConnection exists in appsettings.json");
    }
}
```

**Key Points untuk Factory Implementation:**
- ✅ **Simple & Clean:** Tidak perlu path resolver yang kompleks
- ✅ **EF Core Context:** EF tools selalu run dari Web project directory  
- ✅ **Clear Error Messages:** Helpful untuk debugging
- ✅ **Environment Support:** Reads from appsettings + environment variables

### **Debug Tips untuk Factory Issues**

Jika masih ada masalah dengan DbContextFactory, tambahkan debug logging:

```bash
# Jalankan dengan verbose output untuk debugging
dotnet ef migrations add TestMigration \
  --project . --startup-project ../../MicFx.Web \
  --context AuthDbContext --output-dir Data/Migrations \
  --verbose
```

**Check Current Working Directory:**
```csharp
// Tambahkan di CreateDbContext untuk debugging
Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"Looking for: {Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")}");
Console.WriteLine($"File exists: {File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"))}");
```

**Expected Output:**
```
Current Directory: /path/to/setup-micfx/src/MicFx.Web
Looking for: /path/to/setup-micfx/src/MicFx.Web/appsettings.json
File exists: True
```

```
Unable to create an object of type 'AuthDbContext'
```

**Solution:** Pastikan menggunakan `--context` parameter:
```bash
dotnet ef migrations add MigrationName \
  --context AuthDbContext  # ✅ Specify context
```

### **Error: Connection String**

```
No connection string was found
```

**Solution:** Pastikan `--startup-project` benar:
```bash
dotnet ef migrations add MigrationName \
  --startup-project ../../MicFx.Web  # ✅ Correct path
```

### **Error: Multiple Migrations**

```
Multiple migrations with same name
```

**Solution:** Gunakan nama yang unique dan descriptive:
```bash
# ❌ Generic
dotnet ef migrations add Update

# ✅ Specific  
dotnet ef migrations add AddPhoneNumberToUsers_20250806
```

---

## 📚 Advanced Scenarios

### **Rollback Migration**

```bash
# Rollback to previous migration
dotnet ef database update [PreviousMigrationName] \
  --project . --startup-project ../../MicFx.Web \
  --context AuthDbContext
```

### **Remove Last Migration (belum di-apply)**

```bash
# Remove migration file (jika belum di-apply)
dotnet ef migrations remove \
  --project . --startup-project ../../MicFx.Web \
  --context AuthDbContext
```

### **Generate SQL Script**

```bash
# Generate SQL untuk review manual
dotnet ef migrations script \
  --project . --startup-project ../../MicFx.Web \
  --context AuthDbContext \
  --output migration-script.sql
```

---

## 🎯 Summary

### **KEY RULES:**
1. **1 Migration = 1 File = 1 Purpose**
2. **Descriptive naming conventions**
3. **Always specify `--context` dan `--startup-project`**
4. **Test di development sebelum production**
5. **Backup database sebelum major changes**

### **VERIFIED WORKING SETUP:**
✅ **AuthDbContextFactory sudah tested dan working**  
✅ **Template command sudah validated**  
✅ **Error handling sudah comprehensive**  

### **QUICK REFERENCE:**
```bash
# Basic template (TESTED ✅)
cd src/Modules/[ModuleName]
dotnet ef migrations add [DescriptiveName] \
  --project . --startup-project ../../MicFx.Web \
  --context [ModuleName]DbContext --output-dir Data/Migrations

# Apply migration
dotnet ef database update \
  --project . --startup-project ../../MicFx.Web \
  --context [ModuleName]DbContext
```

### **Factory Template untuk Module Baru:**
```bash
# Copy dari Auth module dan rename
cp src/Modules/MicFx.Modules.Auth/Data/AuthDbContextFactory.cs \
   src/Modules/MicFx.Modules.[NewModule]/Data/[NewModule]DbContextFactory.cs

# Edit namespace dan class names sesuai module baru
```

**Happy Migrating! 🚀**

---

*Generated: August 6, 2025*  
*MICFX Framework v1.0.0*
