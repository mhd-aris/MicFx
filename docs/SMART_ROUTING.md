# 🛣️ MicFx Framework - Smart Routing System

## 🎯 **Overview**

Smart Routing System dalam MicFx Framework menyediakan automatic route generation berdasarkan folder structure dan naming conventions, mendukung multiple routing patterns untuk API, MVC, dan Admin interfaces.

---

## 🏗️ **Architecture**

### **Routing Flow**
```
Folder Structure → Convention Analysis → Route Generation → URL Mapping → Controller Action
```

### **Routing Patterns**
```
📦 Module/
├── 📂 Api/                    → /api/{module-name}/*
│   └── Controller.cs          → Automatic kebab-case conversion
├── 📂 Controllers/            → /{module-name}/*
│   └── Controller.cs          → Standard MVC routing
└── 📂 Areas/                  → /{area}/{module-name}/*
    └── Admin/Controllers/     → /admin/{module-name}/* (Recommended)
        └── Controller.cs      → Admin interface controllers
```

**Legacy Support:**
```
📦 Module/Controllers/
├── Controller.cs              → /{module-name}/*
└── AdminController.cs         → /admin/{module-name}/* (Deprecated)
```

---

## 🔧 **Routing Conventions**

### **API Controllers**
```csharp
// File: Api/HelloWorldController.cs
namespace MicFx.Modules.HelloWorld.Api;

[ApiController]
public class HelloWorldController : ControllerBase
{
    // AUTO-ROUTE: GET /api/hello-world/greet
    [HttpGet("greet")]
    public async Task<ActionResult<ApiResponse<string>>> Greet()
    {
        return Ok(ApiResponse<string>.Ok("Hello from API!"));
    }

    // AUTO-ROUTE: POST /api/hello-world/greet
    [HttpPost("greet")]
    public async Task<ActionResult<ApiResponse<string>>> CreateGreeting([FromBody] GreetingRequest request)
    {
        return Ok(ApiResponse<string>.Ok($"Hello {request.Name}!"));
    }
}
```

### **MVC Controllers**
```csharp
// File: Controllers/HelloWorldController.cs
namespace MicFx.Modules.HelloWorld.Controllers;

public class HelloWorldController : Controller
{
    // AUTO-ROUTE: GET /hello-world
    public IActionResult Index()
    {
        return View();
    }

    // AUTO-ROUTE: GET /hello-world/about
    public IActionResult About()
    {
        return View();
    }
}
```

### **Admin Controllers (Areas/Admin Pattern)**
```csharp
// File: Areas/Admin/Controllers/HelloWorldController.cs
namespace MicFx.Modules.HelloWorld.Areas.Admin.Controllers;

[Area("Admin")]
public class HelloWorldController : Controller
{
    // AUTO-ROUTE: GET /admin/hello-world
    public IActionResult Index()
    {
        return View();
    }

    // AUTO-ROUTE: GET /admin/hello-world/settings
    public IActionResult Settings()
    {
        return View();
    }
}
```

### **Legacy Admin Controllers (Deprecated)**
```csharp
// File: Controllers/HelloWorldAdminController.cs (Not recommended)
namespace MicFx.Modules.HelloWorld.Controllers;

public class HelloWorldAdminController : Controller
{
    // AUTO-ROUTE: GET /admin/hello-world (Legacy)
    public IActionResult Index()
    {
        return View();
    }
}
```

---

## 🎯 **Naming Conventions**

### **Controller Naming**
```csharp
// Module: MicFx.Modules.UserManagement

// API Controller
// File: Api/UserManagementController.cs
UserManagementController → /api/user-management/*

// MVC Controller
// File: Controllers/UserManagementController.cs  
UserManagementController → /user-management/*

// Admin Controller (Recommended - Areas/Admin Pattern)
// File: Areas/Admin/Controllers/UserManagementController.cs
[Area("Admin")] UserManagementController → /admin/user-management/*

// Legacy Admin Controller (Deprecated)
// File: Controllers/UserManagementAdminController.cs
UserManagementAdminController → /admin/user-management/*
```

### **Route Generation Rules**
```
Module name extraction:
"MicFx.Modules.HelloWorld" → "hello-world"
"MicFx.Modules.UserManagement" → "user-management"

Controller name processing:
"HelloWorldController" → "hello-world"
"UserProfileController" → "user-profile"
"HelloWorldAdminController" → "admin/hello-world"
```

---

## 📋 **Examples dari Project**

### **HelloWorld Module Routes**
```
📍 API Routes:
GET    /api/hello-world/greet
POST   /api/hello-world/greet
GET    /api/hello-world/greet/{id}

📍 MVC Routes:
GET    /hello-world
GET    /hello-world/about
GET    /hello-world/greeting/{name}
GET    /hello-world/demo

📍 Admin Routes:
GET    /admin/hello-world
GET    /admin/hello-world/settings
```

### **Auth Module Routes**
```
📍 API Routes:
POST   /api/auth/login
POST   /api/auth/logout
POST   /api/auth/refresh
GET    /api/auth/profile

📍 MVC Routes:
GET    /auth/login
POST   /auth/login
GET    /auth/logout
GET    /auth/register
```

---

## 🎛️ **Configuration Options**

### **Routing Configuration**
```json
{
  "MicFx": {
    "Routing": {
      "EnableAutoRouting": true,
      "UseKebabCase": true,
      "ApiPrefix": "api",
      "AdminPrefix": "admin",
      "DefaultAction": "Index"
    }
  }
}
```

---

## 💡 **Best Practices**

### **Routing Design Guidelines**
1. **Follow Conventions**: Use standard naming patterns
2. **RESTful APIs**: Follow REST conventions for API endpoints
3. **Clear Hierarchy**: Logical route organization
4. **Versioning**: Plan for API versioning
5. **Documentation**: Document custom routes

### **Performance Considerations**
```csharp
// ✅ Good - Specific routes
[HttpGet("users/{id:int}")]
public async Task<IActionResult> GetUser(int id)

// ✅ Good - Route constraints
[HttpGet("products/{category:alpha}/{id:int}")]
public async Task<IActionResult> GetProduct(string category, int id)
```

---

## 🚨 **Troubleshooting**

### **Common Routing Issues**

| Problem | Cause | Solution |
|---------|-------|----------|
| Route not found | Controller not in correct folder | Check folder structure |
| Route conflicts | Multiple routes match same pattern | Use route constraints |
| Wrong HTTP method | Missing HTTP method attribute | Add proper HTTP method attribute |
| Case sensitivity | Route case mismatch | Enable lowercase URLs in configuration |

---

*Smart Routing System menyediakan routing yang intuitif dan powerful untuk aplikasi modular MicFx.*
