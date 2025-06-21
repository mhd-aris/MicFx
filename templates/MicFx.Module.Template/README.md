# MicFx Module Template

Template untuk membuat module baru dalam MicFx framework dengan database support.

## 📁 Struktur Module dengan Database

```
MicFx.Modules.YourModule/
├── Api/                           # JSON API Controllers
├── Controllers/                   # MVC Controllers  
├── Areas/Admin/                   # Admin area controllers & views
├── Data/                          # ⭐ Database layer
│   ├── Migrations/               # EF Core migrations (per module)
│   ├── YourModuleDbContext.cs    # DbContext dengan table prefixing
│   ├── YourModuleDbContextFactory.cs # Design-time factory untuk CLI
│   └── YourModuleSeeder.cs       # Data seeding untuk dev/demo
├── Domain/
│   ├── Entities/                 # Domain entities
│   └── Configuration/            # Domain configuration
├── Services/                     # Business logic
├── Views/                        # Razor views
├── Manifest.cs                   # Module metadata
└── Startup.cs                    # Module configuration
```

## 🚀 Quick Start

### 1. Setup Database Module

**a. Create DbContext:**
```csharp
public class YourModuleDbContext : DbContext
{
    public DbSet<YourEntity> YourEntities { get; set; }

    public YourModuleDbContext(DbContextOptions<YourModuleDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ⭐ Table prefixing untuk module isolation
        modelBuilder.Entity<YourEntity>().ToTable("yourmodule_entities");
        
        // Entity configuration
        modelBuilder.Entity<YourEntity>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name);
        });
    }
}
```

**b. Register di Startup.cs:**
```csharp
protected override void ConfigureModuleServices(IServiceCollection services)
{
    // Database context
    services.AddDbContext<YourModuleDbContext>(options =>
    {
        var connectionString = configuration?.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString);
    });
    
    // Module seeder
    services.AddScoped<IModuleSeeder, YourModuleSeeder>();
    
    // Your services
    services.AddScoped<IYourService, YourService>();
}
```

### 2. Database Migrations

**Commands untuk migration (run dari root project):**
```bash
# Add new migration
dotnet ef migrations add InitialSchema --project ./Modules/MicFx.Modules.YourModule

# Update database
dotnet ef database update --project ./Modules/MicFx.Modules.YourModule

# Remove last migration
dotnet ef migrations remove --project ./Modules/MicFx.Modules.YourModule
```

### 3. Data Seeding

**Implement YourModuleSeeder.cs:**
```csharp
public class YourModuleSeeder : IModuleSeeder
{
    public string ModuleName => "YourModule";
    public int Priority => 100; // Standard priority (Auth=1, Core=10, Business=100)

    public async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<YourModuleDbContext>();
        
        if (!await dbContext.YourEntities.AnyAsync())
        {
            var defaultEntity = new YourEntity 
            { 
                Name = "Default Item",
                CreatedBy = "YourModuleSeeder"
            };
            
            dbContext.YourEntities.Add(defaultEntity);
            await dbContext.SaveChangesAsync();
        }
    }
}
```

## 🎯 Database Best Practices

### ✅ Table Naming Convention
```csharp
// Use module prefix untuk avoid conflicts
modelBuilder.Entity<User>().ToTable("yourmodule_users");
modelBuilder.Entity<Settings>().ToTable("yourmodule_settings");
```

### ✅ Migration Isolation
- Setiap module punya migration folder sendiri
- Tidak ada conflict antar module migrations  
- Independent evolution per module

### ✅ Shared Connection String
- Semua module pakai connection string yang sama dari host
- Database shared, tapi schema isolated dengan table prefixes

### ✅ Seeder Pattern
- Auto-run di Development/Staging environment
- Priority-based ordering untuk handle dependencies
- Idempotent (safe untuk run multiple times)

## 🔧 Template Replacement

Saat menggunakan template ini, replace:

- `TEMPLATE_NAME` → `YourModule` (e.g., `Auth`, `CRM`, `Inventory`)
- `YourModule` → nama module yang sebenarnya
- `YourEntity` → nama entity utama module
- Sesuaikan namespace dan business logic

## 🧪 Testing

Module template sudah include pattern untuk testing:

```csharp
// Integration test dengan InMemory database
var options = new DbContextOptionsBuilder<YourModuleDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;

using var context = new YourModuleDbContext(options);
await context.Database.EnsureCreatedAsync();

// Test your module logic
```

## 📚 Additional Resources

- [Module System Documentation](../docs/MODULE_SYSTEM.md)
- [Database Pattern Documentation](../docs/DATABASE_PATTERNS.md)
- [Migration Best Practices](../docs/MIGRATION_GUIDE.md) 