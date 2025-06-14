# 🎛️ MicFx Framework - Admin Panel System

## 🎯 **Overview**

Admin Panel System dalam MicFx Framework menyediakan interface administrasi yang modern dan modular dengan auto-discovery navigation, real-time dashboard, dan comprehensive module management capabilities.

---

## 🏗️ **Architecture**

### **Admin Panel Flow**
```
Module Registration → Auto-Discovery → Navigation Generation → Admin Interface → Real-time Dashboard
```

### **Component Structure**
```
📦 Admin Panel System
├── 🎛️ Dashboard Controller         → Main admin dashboard
├── 🧭 Navigation Discovery        → Auto-discovery navigation items
├── 🔍 Diagnostics System         → System monitoring & debugging
├── 🎨 Modern UI Components        → Responsive admin interface
├── 📊 Real-time Metrics          → Live system information
└── 🔧 Module Integration         → Seamless module admin interfaces
```

---

## 🚀 **Key Features**

### **Auto-Discovery Navigation**
- ✅ **Automatic Module Detection**: Scans all `MicFx.Modules.*` assemblies
- ✅ **Navigation Contributors**: Modules implement `IAdminNavContributor`
- ✅ **Permission-Based Filtering**: Role-based navigation visibility
- ✅ **Category Organization**: Navigation items grouped by category
- ✅ **Memory Caching**: 15-minute cache with user-specific keys

### **Real-time Dashboard**
- ✅ **Live Clock**: Real-time date and time updates
- ✅ **System Metrics**: Memory usage, uptime, performance indicators
- ✅ **Module Statistics**: Active modules, navigation items, contributors
- ✅ **Quick Actions**: Functional buttons for common admin tasks
- ✅ **Health Indicators**: System status and module health

### **Diagnostics & Monitoring**
- ✅ **Module Scanner**: Comprehensive module information
- ✅ **Navigation Testing**: Role-based navigation testing
- ✅ **System Information**: Environment, configuration, dependencies
- ✅ **Health Checks**: Integrated health monitoring
- ✅ **Performance Metrics**: Response times and system performance

---

## 🧭 **Navigation System**

### **IAdminNavContributor Interface**
```csharp
using MicFx.SharedKernel.Interfaces;

namespace MicFx.Modules.YourModule.Admin;

public class YourModuleAdminNavContributor : IAdminNavContributor
{
    public Task<IEnumerable<AdminNavItem>> GetNavigationItemsAsync(AdminNavContext context)
    {
        var items = new List<AdminNavItem>
        {
            new AdminNavItem
            {
                Title = "Your Module",
                Url = "/admin/your-module",
                Icon = "settings",
                Category = "Management",
                Order = 100,
                RequiredRoles = new[] { "Admin" },
                IsActive = context.CurrentPath.StartsWith("/admin/your-module")
            },
            new AdminNavItem
            {
                Title = "Module Settings",
                Url = "/admin/your-module/settings",
                Icon = "settings",
                Category = "Configuration",
                Order = 200,
                RequiredRoles = new[] { "Admin", "Moderator" },
                IsActive = context.CurrentPath == "/admin/your-module/settings"
            }
        };

        return Task.FromResult<IEnumerable<AdminNavItem>>(items);
    }
}
```

### **AdminNavItem Properties**
```csharp
public class AdminNavItem
{
    /// <summary>
    /// Display title for the navigation item
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL path for the navigation item
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Icon identifier (maps to SVG icons)
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Category for grouping navigation items
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Display order within category
    /// </summary>
    public int Order { get; set; } = 100;

    /// <summary>
    /// Required roles to view this navigation item
    /// </summary>
    public string[]? RequiredRoles { get; set; }

    /// <summary>
    /// Whether this navigation item is currently active
    /// </summary>
    public bool IsActive { get; set; }
}
```

---

## 🎨 **Admin Interface Structure**

### **Layout Components**
```
📱 Admin Layout (_AdminLayout.cshtml)
├── 🏠 Header Bar                  → Logo, page title, user menu
├── 📋 Sidebar Navigation         → Module navigation, quick links
│   ├── Dashboard Link            → /admin
│   ├── System Links              → Diagnostics, health, API docs
│   └── Dynamic Module Navigation → Auto-discovered module items
├── 📄 Main Content Area          → Page-specific content
└── 🔧 JavaScript Enhancements   → Real-time updates, interactions
```

### **Dashboard Components**
```
📊 Admin Dashboard (Dashboard/Index.cshtml)
├── 🎉 Welcome Section            → Greeting, system status, live clock
├── 📈 Statistics Cards           → System metrics, module counts
│   ├── System Status             → Online/offline indicator
│   ├── Active Modules            → Module count and status
│   ├── Memory Usage              → Current memory consumption
│   └── Uptime                    → System uptime information
├── ℹ️ System Information         → Application details, environment
├── 📦 Module Information         → Loaded modules, contributors
├── 🎯 Quick Actions              → Functional admin shortcuts
└── 🧭 Module Navigation          → Dynamic navigation items display
```

---

## 🔧 **Implementation Guide**

### **1. Creating Admin Controllers**

**Areas/Admin Pattern (Recommended)**
```csharp
// File: Areas/Admin/Controllers/YourModuleController.cs
namespace MicFx.Modules.YourModule.Areas.Admin.Controllers;

[Area("Admin")]
[Route("admin/your-module")]
public class YourModuleController : Controller
{
    private readonly IYourModuleService _service;
    private readonly ILogger<YourModuleController> _logger;

    public YourModuleController(
        IYourModuleService service,
        ILogger<YourModuleController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // AUTO-ROUTE: GET /admin/your-module
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Your Module Admin";
        
        var model = new YourModuleAdminViewModel
        {
            Statistics = await _service.GetStatisticsAsync(),
            RecentActivity = await _service.GetRecentActivityAsync()
        };

        return View(model);
    }

    // AUTO-ROUTE: GET /admin/your-module/settings
    [HttpGet("settings")]
    public async Task<IActionResult> Settings()
    {
        ViewData["Title"] = "Module Settings";
        
        var settings = await _service.GetSettingsAsync();
        return View(settings);
    }

    // AUTO-ROUTE: POST /admin/your-module/settings
    [HttpPost("settings")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(YourModuleSettingsViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _service.UpdateSettingsAsync(model);
        TempData["SuccessMessage"] = "Settings updated successfully";
        
        return RedirectToAction(nameof(Settings));
    }
}
```

### **2. Creating Admin Views**

**Admin View Structure**
```
📂 Areas/Admin/Views/
├── 📂 Shared/
│   ├── _AdminLayout.cshtml        → Main admin layout
│   └── _AdminNavigation.cshtml    → Dynamic navigation partial
└── 📂 YourModule/
    ├── Index.cshtml               → Main admin page
    ├── Settings.cshtml            → Settings page
    └── _ModuleStats.cshtml        → Reusable stats partial
```

**Example Admin View**
```html
@model YourModuleAdminViewModel
@{
    ViewData["Title"] = "Your Module Admin";
    Layout = "~/Areas/Admin/Views/Shared/_AdminLayout.cshtml";
}

<!-- Module Statistics -->
<div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
    <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <div class="flex items-center justify-between mb-4">
            <div class="p-3 bg-blue-100 rounded-lg">
                <svg class="w-6 h-6 text-blue-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path>
                </svg>
            </div>
            <span class="px-2 py-1 bg-blue-100 text-blue-800 text-xs font-semibold rounded-full">
                @Model.Statistics.TotalItems Active
            </span>
        </div>
        <div>
            <p class="text-sm font-medium text-gray-600 mb-1">Total Items</p>
            <p class="text-2xl font-bold text-gray-900">@Model.Statistics.TotalItems</p>
        </div>
    </div>
    
    <!-- More statistics cards... -->
</div>

<!-- Recent Activity -->
<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
    <h3 class="text-lg font-bold text-gray-900 mb-4">Recent Activity</h3>
    
    @if (Model.RecentActivity.Any())
    {
        <div class="space-y-3">
            @foreach (var activity in Model.RecentActivity)
            {
                <div class="flex items-center p-3 bg-gray-50 rounded-lg">
                    <div class="w-3 h-3 bg-green-500 rounded-full mr-3"></div>
                    <div class="flex-1">
                        <p class="text-sm font-medium text-gray-900">@activity.Description</p>
                        <p class="text-xs text-gray-500">@activity.Timestamp.ToString("MMM dd, yyyy HH:mm")</p>
                    </div>
                </div>
            }
        </div>
    }
    else
    {
        <p class="text-gray-500 text-center py-8">No recent activity</p>
    }
</div>
```

### **3. Module Registration**

**Register Navigation Contributor**
```csharp
// Module Startup.cs
public class Startup : ModuleStartupBase
{
    protected override void ConfigureModuleServices(IServiceCollection services)
    {
        // Register admin navigation contributor
        services.AddScoped<IAdminNavContributor, YourModuleAdminNavContributor>();
        
        // Register admin services
        services.AddScoped<IYourModuleAdminService, YourModuleAdminService>();
    }
}
```

---

## 📊 **Available Admin Endpoints**

### **Core Admin Routes**
```
📍 Main Admin Routes:
GET    /admin                     → Main dashboard
GET    /admin/dashboard           → Dashboard (alias)

📍 Diagnostics Routes:
GET    /admin/diagnostics         → System diagnostics
GET    /admin/diagnostics/test-roles → Role testing interface

📍 System Routes:
GET    /health                    → Health checks (opens in new tab)
GET    /api/docs                  → API documentation (opens in new tab)
```

### **Module-Specific Routes**
```
📍 HelloWorld Admin Routes:
GET    /admin/hello-world         → HelloWorld admin interface
GET    /admin/hello-world/settings → HelloWorld settings

📍 Auth Admin Routes:
GET    /admin/auth                → Auth module admin
GET    /admin/auth/users          → User management
GET    /admin/auth/roles          → Role management
```

---

## 🎛️ **Dashboard Features**

### **Real-time Components**
```javascript
// Real-time clock update (automatically included)
function updateDateTime() {
    const now = new Date();
    
    // Update time every second
    const timeElement = document.getElementById('currentTime');
    if (timeElement) {
        timeElement.textContent = now.toLocaleTimeString('id-ID', {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
    }
    
    // Update date
    const dateElement = document.getElementById('currentDate');
    if (dateElement) {
        const dateString = now.toLocaleDateString('id-ID', {
            weekday: 'long',
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
        dateElement.textContent = dateString;
    }
}
```

### **Quick Actions**
```
🎯 Available Quick Actions:
├── Manage Modules     → /admin/diagnostics
├── View Logs          → /api/docs (API documentation)
├── Configuration      → /health (Health checks)
├── User Management    → /admin/diagnostics/test-roles
└── System Analytics   → /admin/diagnostics
```

---

## 🔍 **Diagnostics System**

### **Module Scanner Information**
```csharp
public class AdminModuleScanner
{
    public ModuleScanResult GetScanResults()
    {
        return new ModuleScanResult
        {
            ScannedAssemblies = _scannedAssemblies.Count,
            AssemblyNames = _scannedAssemblies.ToList(),
            Contributors = _contributors,
            ScanTimestamp = _scanTimestamp
        };
    }
}
```

### **Navigation Discovery Service**
```csharp
public class AdminNavDiscoveryService
{
    // Get all navigation items for current user
    public async Task<IEnumerable<AdminNavItem>> GetNavigationItemsAsync(HttpContext context)
    
    // Get navigation items grouped by category
    public async Task<Dictionary<string, List<AdminNavItem>>> GetNavigationItemsByCategoryAsync(HttpContext context)
    
    // Check if user has required roles for navigation item
    private bool HasRequiredRoles(AdminNavItem item, ClaimsPrincipal user)
}
```

---

## 🎨 **UI Components & Styling**

### **Color Scheme by Category**
```css
/* Category-based color coding */
.category-users     { @apply bg-orange-50 text-orange-700 border-orange-600; }
.category-content   { @apply bg-green-50 text-green-700 border-green-600; }
.category-settings  { @apply bg-purple-50 text-purple-700 border-purple-600; }
.category-reports   { @apply bg-indigo-50 text-indigo-700 border-indigo-600; }
.category-general   { @apply bg-blue-50 text-blue-700 border-blue-600; }
```

### **Icon Mapping**
```csharp
// Built-in icon mappings
private string GetIconSvg(string iconName)
{
    return iconName.ToLower() switch
    {
        "users" => "<!-- User management icon -->",
        "content" => "<!-- Content management icon -->",
        "settings" => "<!-- Settings icon -->",
        "reports" => "<!-- Reports icon -->",
        "hello" or "helloworld" => "<!-- Hello world icon -->",
        _ => "<!-- Default icon -->"
    };
}
```

---

## 🔐 **Security & Permissions**

### **Role-Based Access**
```csharp
// Controller-level authorization
[Area("Admin")]
[Authorize(Roles = "Admin")] // Uncomment when auth is fully configured
public class YourModuleController : Controller

// Action-level authorization
[Authorize(Roles = "Admin,Moderator")]
public async Task<IActionResult> Settings()

// Navigation-level authorization
new AdminNavItem
{
    RequiredRoles = new[] { "Admin", "Moderator" }
}
```

### **Permission Filtering**
```csharp
// Navigation items are automatically filtered based on user roles
private bool HasRequiredRoles(AdminNavItem item, ClaimsPrincipal user)
{
    if (item.RequiredRoles == null || !item.RequiredRoles.Any())
        return true; // Public access

    return item.RequiredRoles.Any(role => user.IsInRole(role));
}
```

---

## 💡 **Best Practices**

### **Admin Interface Design Guidelines**
1. **Consistent Layout**: Use the standard admin layout for all admin pages
2. **Clear Navigation**: Implement navigation contributors for module discoverability
3. **Responsive Design**: Ensure admin interfaces work on all device sizes
4. **User Feedback**: Provide clear success/error messages for admin actions
5. **Performance**: Use efficient queries and caching for admin data

### **Navigation Design**
```csharp
// ✅ Good - Clear, descriptive navigation
new AdminNavItem
{
    Title = "User Management",
    Url = "/admin/users",
    Icon = "users",
    Category = "Administration",
    Order = 100,
    RequiredRoles = new[] { "Admin" }
}

// ❌ Bad - Vague, unclear navigation
new AdminNavItem
{
    Title = "Stuff",
    Url = "/admin/things",
    Category = "Other"
}
```

### **Admin Controller Patterns**
```csharp
// ✅ Good - RESTful admin actions
[HttpGet]                          // List/Index
[HttpGet("{id}")]                  // View/Details
[HttpGet("create")]                // Create form
[HttpPost("create")]               // Create action
[HttpGet("{id}/edit")]             // Edit form
[HttpPost("{id}/edit")]            // Edit action
[HttpPost("{id}/delete")]          // Delete action

// ✅ Good - Proper error handling
public async Task<IActionResult> Create(CreateModel model)
{
    try
    {
        if (!ModelState.IsValid)
            return View(model);

        await _service.CreateAsync(model);
        TempData["SuccessMessage"] = "Item created successfully";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to create item");
        ModelState.AddModelError("", "Failed to create item. Please try again.");
        return View(model);
    }
}
```

---

## 🚨 **Troubleshooting**

### **Common Admin Panel Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Navigation items not showing | Module not implementing IAdminNavContributor | Implement navigation contributor interface |
| Access denied to admin pages | Missing authorization attributes | Add [Authorize] attributes to controllers |
| Admin layout not loading | Wrong layout path in view | Use correct layout path: `~/Areas/Admin/Views/Shared/_AdminLayout.cshtml` |
| Navigation cache not updating | Cache not invalidated | Restart application or wait for cache expiration |
| Dashboard not loading | Missing dependencies | Check service registration in module startup |

### **Debugging Navigation Issues**
```csharp
// Enable detailed logging for navigation discovery
"Serilog": {
  "MinimumLevel": {
    "Override": {
      "MicFx.Mvc.Web.Admin.Services.AdminNavDiscoveryService": "Debug",
      "MicFx.Mvc.Web.Admin.Services.AdminModuleScanner": "Debug"
    }
  }
}

// Check navigation discovery logs
[12:34:56 DBG] AdminNavDiscoveryService Scanning for navigation contributors
[12:34:57 DBG] AdminNavDiscoveryService Found 3 navigation contributors
[12:34:58 DBG] AdminNavDiscoveryService Generated 5 navigation items
[12:34:59 DBG] AdminNavDiscoveryService Filtered to 3 items for current user
```

---

## 🔮 **Future Enhancements**

### **Planned Features**
- **Dashboard Widgets**: Customizable dashboard widgets per module
- **User Preferences**: Personalized admin interface settings
- **Advanced Permissions**: Granular permission system
- **Audit Logging**: Comprehensive admin action logging
- **Module Management**: Enable/disable modules from admin interface

### **Extension Points**
```csharp
// Custom dashboard widgets
public interface IAdminDashboardWidget
{
    string Title { get; }
    int Order { get; }
    Task<string> RenderAsync(HttpContext context);
}

// Custom admin themes
public interface IAdminThemeProvider
{
    string ThemeName { get; }
    string CssPath { get; }
    string JsPath { get; }
}
```

---

## 📞 **Support & Resources**

### **Available Endpoints for Testing**
- **Admin Dashboard**: `http://localhost:5000/admin`
- **Diagnostics**: `http://localhost:5000/admin/diagnostics`
- **Role Testing**: `http://localhost:5000/admin/diagnostics/test-roles`
- **Health Checks**: `http://localhost:5000/health`
- **API Documentation**: `http://localhost:5000/api/docs`

### **Example Modules**
- **HelloWorld**: Complete example with admin interface
- **Auth**: Authentication module with user management

---

*Admin Panel System menyediakan interface administrasi yang powerful dan extensible untuk mengelola aplikasi MicFx dengan mudah dan efisien.* 