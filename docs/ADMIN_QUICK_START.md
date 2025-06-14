# 🚀 Admin Panel - Quick Start Guide

## 🎯 **Akses Admin Panel**

### **URL Admin Panel**
```
🏠 Main Dashboard:     http://localhost:5000/admin
🔍 System Diagnostics: http://localhost:5000/admin/diagnostics
🧪 Role Testing:       http://localhost:5000/admin/diagnostics/test-roles
```

---

## 📋 **Fitur Utama Dashboard**

### **1. Real-time Information**
- ⏰ **Live Clock**: Waktu dan tanggal yang update setiap detik
- 📊 **System Metrics**: Status sistem, memory usage, uptime
- 📦 **Module Statistics**: Jumlah module aktif dan navigation items

### **2. Quick Actions**
- 🔧 **Manage Modules**: Akses diagnostics sistem
- 📚 **View Logs**: Buka API documentation
- ⚙️ **Configuration**: Akses health checks
- 👥 **User Management**: Testing role dan permissions
- 📈 **System Analytics**: Monitor performa sistem

### **3. Module Navigation**
- 🧭 **Auto-Discovery**: Navigation items dari semua module
- 🏷️ **Category Grouping**: Organized berdasarkan kategori
- 🔐 **Permission-Based**: Hanya tampil sesuai role user
- 🎨 **Visual Indicators**: Status active/inactive dengan warna

---

## 🧩 **Module Admin Interfaces**

### **HelloWorld Module**
```
📍 Admin Interface:
GET /admin/hello-world          → Main admin page
GET /admin/hello-world/settings → Module settings

📍 Features:
- Module statistics dan metrics
- Configuration management
- Recent activity monitoring
- Settings dan preferences
```

### **Auth Module**
```
📍 Admin Interface:
GET /admin/auth                 → Auth admin dashboard
GET /admin/auth/users           → User management
GET /admin/auth/roles           → Role management

📍 Features:
- User account management
- Role dan permission assignment
- Authentication logs
- Security settings
```

---

## 🔍 **System Diagnostics**

### **Module Scanner Information**
- 📦 **Scanned Assemblies**: Daftar semua module yang di-scan
- 🧭 **Navigation Contributors**: Module yang menyediakan admin navigation
- ⏱️ **Scan Timestamp**: Waktu terakhir scan module
- 📊 **Statistics**: Total module, navigation items, contributors

### **Navigation Testing**
- 👤 **Role Scenarios**: Test navigation dengan berbagai role
- 🔐 **Permission Testing**: Verify access control
- 🧪 **User Simulation**: Simulate different user types
- 📋 **Results Display**: Clear indication of accessible items

---

## 🎨 **UI Components**

### **Navigation Sidebar**
```
📋 Sidebar Navigation:
├── 🏠 Dashboard              → /admin
├── 🔧 Modules                → /admin/diagnostics
├── ✅ Health Check           → /health (new tab)
├── 📚 API Docs               → /api/docs (new tab)
├── 🧪 Role Testing           → /admin/diagnostics/test-roles
└── 🧭 Dynamic Module Items   → Auto-discovered navigation
```

### **User Menu**
```
👤 User Dropdown:
├── 👤 Admin Profile          → /admin/diagnostics
├── ⚙️ Admin Settings         → /admin/diagnostics
└── 🚪 Sign out               → /auth/logout
```

---

## 🔧 **Development Tips**

### **Membuat Admin Interface untuk Module Baru**

**1. Implement Navigation Contributor**
```csharp
public class YourModuleAdminNavContributor : IAdminNavContributor
{
    public Task<IEnumerable<AdminNavItem>> GetNavigationItemsAsync(AdminNavContext context)
    {
        return Task.FromResult<IEnumerable<AdminNavItem>>(new[]
        {
            new AdminNavItem
            {
                Title = "Your Module",
                Url = "/admin/your-module",
                Icon = "settings",
                Category = "Management",
                Order = 100
            }
        });
    }
}
```

**2. Create Admin Controller**
```csharp
[Area("Admin")]
[Route("admin/your-module")]
public class YourModuleController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Your Module Admin";
        return View();
    }
}
```

**3. Register Services**
```csharp
// In Module Startup.cs
services.AddScoped<IAdminNavContributor, YourModuleAdminNavContributor>();
```

---

## 🚨 **Troubleshooting**

### **Common Issues**

| Problem | Solution |
|---------|----------|
| Admin page tidak load | Check apakah controller ada di `Areas/Admin/Controllers/` |
| Navigation tidak muncul | Verify `IAdminNavContributor` sudah di-register |
| Access denied | Uncomment `[Authorize]` attribute setelah auth configured |
| Layout tidak benar | Pastikan menggunakan admin layout yang benar |

### **Debug Navigation**
```csharp
// Check logs untuk navigation discovery
[12:34:56 INF] AdminNavDiscoveryService Found 3 navigation contributors
[12:34:57 INF] AdminNavDiscoveryService Generated 5 navigation items
[12:34:58 INF] AdminNavDiscoveryService Cached navigation for user
```

---

## 📞 **Quick Links**

- **📖 Full Documentation**: [ADMIN_PANEL.md](ADMIN_PANEL.md)
- **🏗️ Architecture**: [ARCHITECTURE.md](ARCHITECTURE.md)
- **🧩 Module System**: [MODULE_SYSTEM.md](MODULE_SYSTEM.md)
- **🛣️ Smart Routing**: [SMART_ROUTING.md](SMART_ROUTING.md)

---

*Admin Panel menyediakan interface yang powerful dan user-friendly untuk mengelola aplikasi MicFx dengan mudah.* 