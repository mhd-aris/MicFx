# 🏗️ MicFx Framework - Architecture Overview

## 🎯 **Architecture Pattern**

MicFx menggunakan **Clean Architecture** dengan **Modular Monolith** pattern yang memisahkan concerns dan memungkinkan scalability horizontal.

## 📦 **Layer Structure**

```
┌─────────────────────────────────────┐
│        MicFx.Web                │ ← Presentation Layer
│        ├── Program.cs               │   - Application startup
│        ├── appsettings.json         │   - Configuration
│        └── Infrastructure/          │   - View resolution
├─────────────────────────────────────┤
│        MicFx.Core                   │ ← Application Layer
│        ├── Configuration/           │   - Configuration management
│        ├── Extensions/              │   - Service registration
│        ├── Filters/                 │   - Action filters
│        ├── Middleware/              │   - Custom middleware
│        └── Modularity/              │   - Module system
├─────────────────────────────────────┤
│        MicFx.Infrastructure         │ ← Infrastructure Layer
│        ├── Extensions/              │   - DI registration
│        ├── Logging/                 │   - Serilog implementation
│        └── Swagger/                 │   - API documentation
├─────────────────────────────────────┤
│        MicFx.Abstractions           │ ← Interface Layer
│        ├── Caching/                 │   - Cache contracts
│        ├── Extensions/              │   - DI extensions
│        ├── Logging/                 │   - Logging contracts
│        └── Security/                │   - Security contracts
├─────────────────────────────────────┤
│        MicFx.SharedKernel           │ ← Domain Layer
│        ├── Common/                  │   - Common types
│        └── Modularity/              │   - Module contracts
├─────────────────────────────────────┤
│        Modules                      │ ← Business Logic Layer
│        ├── MicFx.Modules.HelloWorld │   - Demo module
│        └── MicFx.Modules.Auth       │   - Auth module
└─────────────────────────────────────┘
```

## 🔄 **Dependency Flow**

Framework mengikuti **Dependency Inversion Principle**:

```
Presentation → Application → Infrastructure
     ↓              ↓              ↓
   Core ←─── Abstractions ←─── SharedKernel
     ↓
   Modules
```

### **Rules:**
- **Outer layers depend on inner layers**
- **Core tidak depend pada Infrastructure**
- **Modules hanya depend pada SharedKernel & Abstractions**
- **No circular dependencies**

## 🧩 **Module Architecture**

### **Standard Module Structure**
```
📦 MicFx.Modules.{Name}/
├── 📂 Api/                    # API Controllers
│   └── {Name}Controller.cs    # RESTful endpoints
├── 📂 Controllers/            # MVC Controllers
│   ├── {Name}Controller.cs    # Web pages
│   └── {Name}AdminController.cs # Admin interface
├── 📂 Areas/                  # Area-based controllers
├── 📂 Views/                  # Razor views
├── 📂 ViewModels/             # View models
├── 📂 Services/               # Business logic
├── 📂 Domain/                 # Domain entities
├── Manifest.cs                # Module metadata
├── Startup.cs                 # Module configuration
└── {Name}.csproj             # Project file
```

### **Module Lifecycle**
1. **Discovery**: Framework auto-discovers modules
2. **Registration**: Services registered via DI
3. **Configuration**: Module-specific configuration loaded
4. **Initialization**: Startup.cs executed
5. **Runtime**: Controllers & services available

## 🚀 **Key Architectural Decisions**

### **1. Clean Architecture**
- **Separation of Concerns**: Each layer has single responsibility
- **Testability**: Easy to unit test each layer
- **Maintainability**: Clear boundaries between components
- **Flexibility**: Easy to swap implementations

### **2. Modular Monolith**
- **Single Deployment**: One application with multiple modules
- **Shared Infrastructure**: Common logging, config, etc.
- **Independent Development**: Teams can work on separate modules
- **Easy Migration**: Can extract to microservices later

### **3. Convention over Configuration**
- **Auto-Discovery**: Zero configuration for standard patterns
- **Folder-Based Routing**: Structure determines routes
- **Naming Conventions**: Consistent naming patterns
- **Smart Defaults**: Sensible defaults for common scenarios

## 📊 **Data Flow**

### **Request Processing Flow**
```
HTTP Request
    ↓
Middleware Pipeline
    ↓
Exception Handling
    ↓
Routing (Auto-Discovery)
    ↓
Module Controller
    ↓
Business Service
    ↓
Response (ApiResponse<T>)
    ↓
Structured Logging
```

### **Configuration Flow**
```
appsettings.json
    ↓
Configuration Manager
    ↓
Module Configuration
    ↓
Validation
    ↓
Dependency Injection
    ↓
Runtime Usage
```

## 🛡️ **Cross-Cutting Concerns**

### **Logging**
- **Structured Logging**: Serilog dengan structured properties
- **Module Context**: Automatic module identification
- **Performance Tracking**: Built-in timing measurements
- **Audit Trail**: Security dan business events

### **Exception Handling**
- **Global Middleware**: Centralized exception processing
- **Typed Exceptions**: Specific exception types untuk different scenarios
- **Structured Responses**: Consistent error response format
- **Environment Aware**: Different detail levels per environment

### **Configuration**
- **Centralized Management**: Single configuration system
- **Type Safety**: Strongly-typed configuration classes
- **Validation**: Built-in validation dengan data annotations
- **Hot Reload**: Runtime configuration updates

## 🔧 **Extensibility Points**

### **1. Custom Modules**
- Implement `ModuleManifestBase` dan `ModuleStartupBase`
- Follow standard folder structure
- Register services dalam `ConfigureModuleServices`

### **2. Custom Middleware**
- Add dalam `Program.cs` pipeline
- Use structured logging untuk consistency
- Follow error handling patterns

### **3. Custom Configuration**
- Extend `ModuleConfigurationBase<T>`
- Add validation attributes
- Register dalam module startup

### **4. Custom Logging Sinks**
- Extend Serilog configuration
- Add custom enrichers
- Maintain structured format

## 📈 **Performance Considerations**

### **Startup Performance**
- **Lazy Loading**: Modules loaded on-demand
- **Efficient Discovery**: Fast assembly scanning
- **Minimal Dependencies**: Only load what's needed

### **Runtime Performance**
- **Zero Overhead**: Framework adds minimal processing cost
- **Async Patterns**: Full async/await support
- **Memory Efficient**: Proper disposal patterns

### **Scalability**
- **Horizontal Scaling**: Can scale across multiple instances
- **Module Isolation**: Independent module scaling
- **Resource Sharing**: Efficient shared resources

## 🎯 **Design Principles**

1. **SOLID Principles**: Fully compliant dengan all SOLID principles
2. **DRY**: Don't Repeat Yourself - shared infrastructure
3. **KISS**: Keep It Simple Stupid - clear dan straightforward
4. **YAGNI**: You Ain't Gonna Need It - only implement what's needed
5. **Convention over Configuration**: Reduce boilerplate code

## 🔮 **Future Architecture Considerations**

### **Microservices Migration**
- **Module Boundaries**: Clear boundaries untuk service extraction
- **Shared Contracts**: Abstractions layer dapat dijadi shared libraries
- **Configuration**: Centralized config dapat menjadi config service

### **Event-Driven Architecture**
- **Message Bus**: Inter-module communication
- **Event Sourcing**: For complex business domains
- **CQRS**: Command Query Responsibility Segregation

### **Advanced Patterns**
- **Circuit Breaker**: For external service calls
- **Rate Limiting**: Traffic control
- **Caching**: Distributed caching layer

---
