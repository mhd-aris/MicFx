# MicFx.Tests.Core

Unit tests untuk komponen core MicFx Framework.

## 🎯 **Filosofi Testing**

Tests ini menggunakan pendekatan **pragmatic testing** yang fokus pada:
- **Business logic validation** dibanding infrastructure testing
- **Concrete implementations** dibanding complex mocking
- **Real component integration** untuk high-value scenarios
- **Simple, readable test patterns** yang mudah di-maintain

## 📁 **Struktur Tests**

```
MicFx.Tests.Core/
├── Configuration/           # Tests untuk configuration management
├── Dependency/             # Tests untuk dependency resolution
├── Lifecycle/              # Tests untuk module lifecycle management
└── _TestUtilities/         # Shared testing utilities
```

## 🧪 **Test Categories**

### **Configuration Tests (17 tests)**
- Module configuration registration dan validation
- Type-based dan name-based configuration retrieval
- Error handling scenarios

### **Dependency Resolution Tests (26 tests)**
- Module dependency registration dan validation
- Startup/shutdown ordering logic
- Direct dependencies/dependents mapping

### **Lifecycle Management Tests (12 tests)**
- Module registration workflows
- Basic start/stop operations
- Real dependency integration scenarios

## 🛠️ **Test Utilities**

### **TestModuleFactory**
Factory untuk membuat test modules dengan configurasi yang berbeda.

```csharp
// Basic module
var module = TestModuleFactory.CreateBasicModule("TestModule");

// Module dengan dependencies
var module = TestModuleFactory.CreateBasicModule("ModuleB", 
    dependencies: new[] { "ModuleA" }, 
    priority: 200);
```

### **TestServiceProviderFactory**
Factory untuk membuat mock service providers yang diperlukan tests.

```csharp
var mockServiceProvider = TestServiceProviderFactory.CreateMockServiceProvider();
```

## 🚀 **Menjalankan Tests**

```bash
# Run semua tests
dotnet test

# Run dengan verbose output
dotnet test --verbosity normal

# Run specific test category
dotnet test --filter "ClassName~Configuration"
```

## 🎯 **Best Practices**

1. **Simple Test Names**: Descriptive test names yang menjelaskan scenario
2. **Arrange-Act-Assert**: Consistent test structure
3. **Real Components**: Prefer real implementations over mocks when possible
4. **Minimal Dependencies**: Test utilities hanya mengandung yang benar-benar digunakan
5. **Fast Execution**: Tests harus berjalan cepat untuk feedback loop yang optimal 