using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;
using MicFx.Tests.Core._TestUtilities;
using Moq;

namespace MicFx.Tests.Core.Lifecycle;

/// <summary>
/// Unit tests untuk ModuleLifecycleManager - fokus pada functionality yang dapat ditest dengan baik
/// SIMPLIFIED: Removed complex mocking scenarios yang tidak compatible dengan non-virtual methods
/// </summary>
public class ModuleLifecycleManagerTests : IDisposable
{
    private readonly Mock<ILogger<ModuleLifecycleManager>> _mockLogger;
    private readonly ModuleDependencyResolver _dependencyResolver; // Real instance
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly ModuleLifecycleManager _sut; // System Under Test

    public ModuleLifecycleManagerTests()
    {
        _mockLogger = new Mock<ILogger<ModuleLifecycleManager>>();
        
        // Use real dependency resolver instead of mock to avoid non-virtual method issues
        var mockDependencyLogger = new Mock<ILogger<ModuleDependencyResolver>>();
        _dependencyResolver = new ModuleDependencyResolver(mockDependencyLogger.Object);
        
        _mockServiceProvider = TestServiceProviderFactory.CreateMockServiceProvider();
        
        _sut = new ModuleLifecycleManager(
            _mockLogger.Object,
            _dependencyResolver,
            _mockServiceProvider.Object);
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }

    #region RegisterModule Tests - Working Tests Only

    [Fact]
    public void RegisterModule_WithValidModule_ShouldRegisterSuccessfully()
    {
        // Arrange
        var module = TestModuleFactory.CreateBasicModule("TestModule");

        // Act
        _sut.RegisterModule(module);

        // Assert
        var moduleStates = _sut.GetAllModuleStates();
        moduleStates.Should().ContainKey("TestModule");
        moduleStates["TestModule"].State.Should().Be(ModuleState.NotLoaded);
        moduleStates["TestModule"].ModuleName.Should().Be("TestModule");
    }

    [Fact]
    public void RegisterModule_WithDuplicateModule_ShouldUpdateExistingRegistration()
    {
        // Arrange
        var module1 = TestModuleFactory.CreateBasicModule("TestModule", version: "1.0.0");
        var module2 = TestModuleFactory.CreateBasicModule("TestModule", version: "2.0.0");

        // Act
        _sut.RegisterModule(module1);
        _sut.RegisterModule(module2);

        // Assert
        var moduleStates = _sut.GetAllModuleStates();
        moduleStates.Should().HaveCount(1);
        moduleStates["TestModule"].Manifest!.Version.Should().Be("2.0.0");
    }

    #endregion

    #region StartModuleAsync Tests - Basic Tests Only

    [Fact]
    public async Task StartModuleAsync_WithValidModule_ShouldStartSuccessfully()
    {
        // Arrange
        var module = TestModuleFactory.CreateBasicModule("TestModule");
        _sut.RegisterModule(module);
        
        // Register module in dependency resolver for consistency
        _dependencyResolver.RegisterModule(module.Manifest);

        // Act
        await _sut.StartModuleAsync("TestModule");

        // Assert
        var moduleState = _sut.GetModuleState("TestModule");
        moduleState.Should().NotBeNull();
        moduleState!.State.Should().Be(ModuleState.Loaded);
    }

    [Fact]
    public async Task StartModuleAsync_WithAlreadyLoadedModule_ShouldLogWarningAndReturn()
    {
        // Arrange
        var module = TestModuleFactory.CreateBasicModule("TestModule");
        _sut.RegisterModule(module);
        _dependencyResolver.RegisterModule(module.Manifest);

        // First start
        await _sut.StartModuleAsync("TestModule");

        // Act - Second start
        await _sut.StartModuleAsync("TestModule");

        // Assert
        var moduleState = _sut.GetModuleState("TestModule");
        moduleState!.State.Should().Be(ModuleState.Loaded);

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already loaded")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region StopModuleAsync Tests - Basic Tests Only

    [Fact]
    public async Task StopModuleAsync_WithLoadedModule_ShouldStopSuccessfully()
    {
        // Arrange
        var module = TestModuleFactory.CreateBasicModule("TestModule");
        _sut.RegisterModule(module);
        _dependencyResolver.RegisterModule(module.Manifest);

        // Start module first
        await _sut.StartModuleAsync("TestModule");

        // Act
        await _sut.StopModuleAsync("TestModule");

        // Assert
        var moduleState = _sut.GetModuleState("TestModule");
        moduleState!.State.Should().Be(ModuleState.NotLoaded);
    }

    [Fact]
    public async Task StopModuleAsync_WithNonExistentModule_ShouldLogWarningAndReturn()
    {
        // Act
        await _sut.StopModuleAsync("NonExistentModule");

        // Assert - Should log warning without throwing
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetModuleState Tests - Always Work

    [Fact]
    public void GetModuleState_WithExistingModule_ShouldReturnCorrectState()
    {
        // Arrange
        var module = TestModuleFactory.CreateBasicModule("TestModule");
        _sut.RegisterModule(module);

        // Act
        var state = _sut.GetModuleState("TestModule");

        // Assert
        state.Should().NotBeNull();
        state!.ModuleName.Should().Be("TestModule");
        state.State.Should().Be(ModuleState.NotLoaded);
        state.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void GetModuleState_WithNonExistentModule_ShouldReturnNull()
    {
        // Act
        var state = _sut.GetModuleState("NonExistentModule");

        // Assert
        state.Should().BeNull();
    }

    #endregion

    #region GetAllModuleStates Tests - Always Work

    [Fact]
    public void GetAllModuleStates_WithMultipleModules_ShouldReturnAllStates()
    {
        // Arrange
        var moduleA = TestModuleFactory.CreateBasicModule("ModuleA");
        var moduleB = TestModuleFactory.CreateBasicModule("ModuleB");
        
        _sut.RegisterModule(moduleA);
        _sut.RegisterModule(moduleB);

        // Act
        var states = _sut.GetAllModuleStates();

        // Assert
        states.Should().HaveCount(2);
        states.Should().ContainKeys("ModuleA", "ModuleB");
        states.Values.Should().AllSatisfy(state => 
            state.State.Should().Be(ModuleState.NotLoaded));
    }

    [Fact]
    public void GetAllModuleStates_WithNoModules_ShouldReturnEmptyDictionary()
    {
        // Act
        var states = _sut.GetAllModuleStates();

        // Assert
        states.Should().BeEmpty();
    }

    #endregion

    #region Integration Tests - Real Dependency Management

    [Fact]
    public async Task StartMultipleModules_WithRealDependencyResolver_ShouldWorkCorrectly()
    {
        // Arrange
        var moduleA = TestModuleFactory.CreateBasicModule("ModuleA", priority: 300);
        var moduleB = TestModuleFactory.CreateBasicModule("ModuleB", dependencies: new[] { "ModuleA" }, priority: 200);
        
        _sut.RegisterModule(moduleA);
        _sut.RegisterModule(moduleB);
        
        // Register in dependency resolver
        _dependencyResolver.RegisterModule(moduleA.Manifest);
        _dependencyResolver.RegisterModule(moduleB.Manifest);

        // Act - Start B which depends on A
        await _sut.StartModuleAsync("ModuleB");

        // Assert - Both should be started (A should auto-start due to dependency)
        var stateA = _sut.GetModuleState("ModuleA");
        var stateB = _sut.GetModuleState("ModuleB");
        
        stateA!.State.Should().Be(ModuleState.Loaded);
        stateB!.State.Should().Be(ModuleState.Loaded);
    }

    [Fact]
    public void ModuleLifecycleManager_WithRealComponents_ShouldHaveConsistentState()
    {
        // Arrange & Act
        var module = TestModuleFactory.CreateBasicModule("TestModule");
        _sut.RegisterModule(module);
        _dependencyResolver.RegisterModule(module.Manifest);

        // Assert - Both systems should be consistent
        var lifecycleState = _sut.GetModuleState("TestModule");
        var dependencyCount = _dependencyResolver.RegisteredModuleCount;

        lifecycleState.Should().NotBeNull();
        lifecycleState!.ModuleName.Should().Be("TestModule");
        dependencyCount.Should().Be(1);
    }

    #endregion
} 