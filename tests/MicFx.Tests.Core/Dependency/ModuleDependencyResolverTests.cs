using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using MicFx.Core.Modularity;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Common.Exceptions;

namespace MicFx.Tests.Core.Dependency;

/// <summary>
/// Unit tests untuk ModuleDependencyResolver
/// Focus: Core dependency resolution dan validation logic
/// </summary>
public class ModuleDependencyResolverTests : IDisposable
{
    private readonly Mock<ILogger<ModuleDependencyResolver>> _mockLogger;
    private readonly ModuleDependencyResolver _sut; // System Under Test

    public ModuleDependencyResolverTests()
    {
        _mockLogger = new Mock<ILogger<ModuleDependencyResolver>>();
        _sut = new ModuleDependencyResolver(_mockLogger.Object);
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }

    #region RegisterModule Tests

    [Fact]
    public void RegisterModule_WithValidManifest_ShouldRegisterSuccessfully()
    {
        // Arrange
        var manifest = CreateMockManifest("TestModule", "1.0.0");

        // Act
        _sut.RegisterModule(manifest);

        // Assert
        _sut.RegisteredModuleCount.Should().Be(1);
    }

    [Fact]
    public void RegisterModule_WithNullManifest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => _sut.RegisterModule(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RegisterModule_WithEmptyModuleName_ShouldThrowModuleException()
    {
        // Arrange
        var manifest = CreateMockManifest("", "1.0.0");

        // Act & Assert
        var action = () => _sut.RegisterModule(manifest);
        action.Should().Throw<ModuleException>()
            .WithMessage("*Module name cannot be null or empty*");
    }

    [Fact]
    public void RegisterModule_WithWhitespaceModuleName_ShouldThrowModuleException()
    {
        // Arrange
        var manifest = CreateMockManifest("   ", "1.0.0");

        // Act & Assert
        var action = () => _sut.RegisterModule(manifest);
        action.Should().Throw<ModuleException>()
            .WithMessage("*Module name cannot be null or empty*");
    }

    [Fact]
    public void RegisterModule_WithDuplicateName_ShouldUpdateExistingRegistration()
    {
        // Arrange
        var manifest1 = CreateMockManifest("TestModule", "1.0.0");
        var manifest2 = CreateMockManifest("TestModule", "2.0.0");

        // Act
        _sut.RegisterModule(manifest1);
        _sut.RegisterModule(manifest2);

        // Assert
        _sut.RegisteredModuleCount.Should().Be(1);
        
        // Verify logging of replacement
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region ValidateDependencies Tests

    [Fact]
    public void ValidateDependencies_WithNoDependencies_ShouldReturnValid()
    {
        // Arrange
        var manifest = CreateMockManifest("ModuleA", "1.0.0");
        _sut.RegisterModule(manifest);

        // Act
        var result = _sut.ValidateDependencies();

        // Assert
        result.IsValid.Should().BeTrue();
        result.MissingDependencies.Should().BeEmpty();
    }

    [Fact]
    public void ValidateDependencies_WithSatisfiedDependencies_ShouldReturnValid()
    {
        // Arrange
        var moduleA = CreateMockManifest("ModuleA", "1.0.0");
        var moduleB = CreateMockManifest("ModuleB", "1.0.0", new[] { "ModuleA" });
        
        _sut.RegisterModule(moduleA);
        _sut.RegisterModule(moduleB);

        // Act
        var result = _sut.ValidateDependencies();

        // Assert
        result.IsValid.Should().BeTrue();
        result.MissingDependencies.Should().BeEmpty();
    }

    [Fact]
    public void ValidateDependencies_WithMissingDependency_ShouldReturnInvalid()
    {
        // Arrange
        var moduleB = CreateMockManifest("ModuleB", "1.0.0", new[] { "ModuleA" });
        _sut.RegisterModule(moduleB);

        // Act
        var result = _sut.ValidateDependencies();

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingDependencies.Should().HaveCount(1);
        result.MissingDependencies.First().Should().Contain("ModuleB");
        result.MissingDependencies.First().Should().Contain("ModuleA");
    }

    [Fact]
    public void ValidateDependencies_WithMultipleMissingDependencies_ShouldReturnAllMissing()
    {
        // Arrange
        var moduleC = CreateMockManifest("ModuleC", "1.0.0", new[] { "ModuleA", "ModuleB" });
        _sut.RegisterModule(moduleC);

        // Act
        var result = _sut.ValidateDependencies();

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingDependencies.Should().HaveCount(2);
    }

    [Fact]
    public void ValidateDependencies_WithEmptyDependencyNames_ShouldIgnoreEmptyEntries()
    {
        // Arrange
        var moduleA = CreateMockManifest("ModuleA", "1.0.0", new string[] { "", "  " });
        _sut.RegisterModule(moduleA);

        // Act
        var result = _sut.ValidateDependencies();

        // Assert
        result.IsValid.Should().BeTrue();
        result.MissingDependencies.Should().BeEmpty();
    }

    #endregion

    #region GetStartupOrder Tests

    [Fact]
    public void GetStartupOrder_WithNoModules_ShouldReturnEmptyList()
    {
        // Act
        var order = _sut.GetStartupOrder();

        // Assert
        order.Should().BeEmpty();
    }

    [Fact]
    public void GetStartupOrder_WithSingleModule_ShouldReturnSingleModule()
    {
        // Arrange
        var manifest = CreateMockManifest("ModuleA", "1.0.0", priority: 100);
        _sut.RegisterModule(manifest);

        // Act
        var order = _sut.GetStartupOrder();

        // Assert
        order.Should().HaveCount(1);
        order.First().Should().Be("ModuleA");
    }

    [Fact]
    public void GetStartupOrder_WithMultipleModules_ShouldOrderByPriorityAscending()
    {
        // Arrange - Lower number = higher priority, loads first
        var moduleA = CreateMockManifest("ModuleA", "1.0.0", priority: 100);
        var moduleB = CreateMockManifest("ModuleB", "1.0.0", priority: 200);
        var moduleC = CreateMockManifest("ModuleC", "1.0.0", priority: 300);
        
        _sut.RegisterModule(moduleC); // Register in random order
        _sut.RegisterModule(moduleA);
        _sut.RegisterModule(moduleB);

        // Act
        var order = _sut.GetStartupOrder();

        // Assert
        order.Should().HaveCount(3);
        order[0].Should().Be("ModuleA"); // Highest priority (100 - lower number)
        order[1].Should().Be("ModuleB"); // Medium priority (200)
        order[2].Should().Be("ModuleC"); // Lowest priority (300 - higher number)
    }

    [Fact]
    public void GetStartupOrder_WithSamePriority_ShouldOrderAlphabetically()
    {
        // Arrange - Same priority, should be alphabetical
        var moduleZ = CreateMockManifest("ZModule", "1.0.0", priority: 100);
        var moduleA = CreateMockManifest("AModule", "1.0.0", priority: 100);
        var moduleM = CreateMockManifest("MModule", "1.0.0", priority: 100);
        
        _sut.RegisterModule(moduleZ); // Register in random order
        _sut.RegisterModule(moduleA);
        _sut.RegisterModule(moduleM);

        // Act
        var order = _sut.GetStartupOrder();

        // Assert
        order.Should().HaveCount(3);
        order[0].Should().Be("AModule");
        order[1].Should().Be("MModule");
        order[2].Should().Be("ZModule");
    }

    #endregion

    #region GetShutdownOrder Tests

    [Fact]
    public void GetShutdownOrder_ShouldReturnReverseOfStartupOrder()
    {
        // Arrange - Lower number = higher priority
        var moduleA = CreateMockManifest("ModuleA", "1.0.0", priority: 100);
        var moduleB = CreateMockManifest("ModuleB", "1.0.0", priority: 200);
        var moduleC = CreateMockManifest("ModuleC", "1.0.0", priority: 300);
        
        _sut.RegisterModule(moduleA);
        _sut.RegisterModule(moduleB);
        _sut.RegisterModule(moduleC);

        // Act
        var startupOrder = _sut.GetStartupOrder();
        var shutdownOrder = _sut.GetShutdownOrder();

        // Assert
        shutdownOrder.Should().HaveCount(3);
        shutdownOrder[0].Should().Be("ModuleC"); // Lowest priority (highest number) shuts down first
        shutdownOrder[1].Should().Be("ModuleB");
        shutdownOrder[2].Should().Be("ModuleA"); // Highest priority (lowest number) shuts down last
        
        // Verify it's exactly the reverse
        shutdownOrder.Should().BeEquivalentTo(startupOrder.Reverse());
    }

    #endregion

    #region GetDirectDependencies Tests

    [Fact]
    public void GetDirectDependencies_WithExistingModule_ShouldReturnDependencies()
    {
        // Arrange
        var manifest = CreateMockManifest("ModuleB", "1.0.0", new[] { "ModuleA", "ModuleC" });
        _sut.RegisterModule(manifest);

        // Act
        var dependencies = _sut.GetDirectDependencies("ModuleB");

        // Assert
        dependencies.Should().HaveCount(2);
        dependencies.Should().Contain("ModuleA");
        dependencies.Should().Contain("ModuleC");
    }

    [Fact]
    public void GetDirectDependencies_WithNonExistentModule_ShouldReturnEmpty()
    {
        // Act
        var dependencies = _sut.GetDirectDependencies("NonExistentModule");

        // Assert
        dependencies.Should().BeEmpty();
    }

    [Fact]
    public void GetDirectDependencies_WithEmptyModuleName_ShouldReturnEmpty()
    {
        // Act
        var dependencies = _sut.GetDirectDependencies("");

        // Assert
        dependencies.Should().BeEmpty();
    }

    [Fact]
    public void GetDirectDependencies_WithNullModuleName_ShouldReturnEmpty()
    {
        // Act
        var dependencies = _sut.GetDirectDependencies(null!);

        // Assert
        dependencies.Should().BeEmpty();
    }

    [Fact]
    public void GetDirectDependencies_ShouldFilterOutEmptyDependencies()
    {
        // Arrange
        var manifest = CreateMockManifest("ModuleB", "1.0.0", new string[] { "ModuleA", "", "  ", "ModuleC" });
        _sut.RegisterModule(manifest);

        // Act
        var dependencies = _sut.GetDirectDependencies("ModuleB");

        // Assert
        dependencies.Should().HaveCount(2);
        dependencies.Should().Contain("ModuleA");
        dependencies.Should().Contain("ModuleC");
    }

    #endregion

    #region GetDirectDependents Tests

    [Fact]
    public void GetDirectDependents_WithExistingModule_ShouldReturnDependents()
    {
        // Arrange
        var moduleA = CreateMockManifest("ModuleA", "1.0.0");
        var moduleB = CreateMockManifest("ModuleB", "1.0.0", new[] { "ModuleA" });
        var moduleC = CreateMockManifest("ModuleC", "1.0.0", new[] { "ModuleA" });
        var moduleD = CreateMockManifest("ModuleD", "1.0.0", new[] { "ModuleB" });
        
        _sut.RegisterModule(moduleA);
        _sut.RegisterModule(moduleB);
        _sut.RegisterModule(moduleC);
        _sut.RegisterModule(moduleD);

        // Act
        var dependents = _sut.GetDirectDependents("ModuleA");

        // Assert
        dependents.Should().HaveCount(2);
        dependents.Should().Contain("ModuleB");
        dependents.Should().Contain("ModuleC");
    }

    [Fact]
    public void GetDirectDependents_WithNonExistentModule_ShouldReturnEmpty()
    {
        // Act
        var dependents = _sut.GetDirectDependents("NonExistentModule");

        // Assert
        dependents.Should().BeEmpty();
    }

    [Fact]
    public void GetDirectDependents_ShouldReturnDependentsInAlphabeticalOrder()
    {
        // Arrange
        var moduleA = CreateMockManifest("ModuleA", "1.0.0");
        var moduleZ = CreateMockManifest("ZModule", "1.0.0", new[] { "ModuleA" });
        var moduleB = CreateMockManifest("BModule", "1.0.0", new[] { "ModuleA" });
        var moduleM = CreateMockManifest("MModule", "1.0.0", new[] { "ModuleA" });
        
        _sut.RegisterModule(moduleA);
        _sut.RegisterModule(moduleZ); // Register in random order
        _sut.RegisterModule(moduleB);
        _sut.RegisterModule(moduleM);

        // Act
        var dependents = _sut.GetDirectDependents("ModuleA");

        // Assert
        dependents.Should().HaveCount(3);
        dependents[0].Should().Be("BModule");
        dependents[1].Should().Be("MModule");
        dependents[2].Should().Be("ZModule");
    }

    #endregion

    #region RegisteredModuleCount Tests

    [Fact]
    public void RegisteredModuleCount_InitiallyZero()
    {
        // Assert
        _sut.RegisteredModuleCount.Should().Be(0);
    }

    [Fact]
    public void RegisteredModuleCount_AfterRegistration_ShouldIncrement()
    {
        // Arrange
        var manifest1 = CreateMockManifest("ModuleA", "1.0.0");
        var manifest2 = CreateMockManifest("ModuleB", "1.0.0");

        // Act & Assert
        _sut.RegisterModule(manifest1);
        _sut.RegisteredModuleCount.Should().Be(1);

        _sut.RegisterModule(manifest2);
        _sut.RegisteredModuleCount.Should().Be(2);
    }

    [Fact]
    public void RegisteredModuleCount_WithDuplicateRegistration_ShouldNotIncrement()
    {
        // Arrange
        var manifest1 = CreateMockManifest("ModuleA", "1.0.0");
        var manifest2 = CreateMockManifest("ModuleA", "2.0.0"); // Same name

        // Act
        _sut.RegisterModule(manifest1);
        _sut.RegisterModule(manifest2);

        // Assert
        _sut.RegisteredModuleCount.Should().Be(1);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock IModuleManifest for testing
    /// </summary>
    private static IModuleManifest CreateMockManifest(
        string name, 
        string version, 
        string[]? dependencies = null, 
        int priority = 100)
    {
        var mock = new Mock<IModuleManifest>();
        mock.Setup(m => m.Name).Returns(name);
        mock.Setup(m => m.Version).Returns(version);
        mock.Setup(m => m.Dependencies).Returns(dependencies ?? Array.Empty<string>());
        mock.Setup(m => m.Priority).Returns(priority);
        mock.Setup(m => m.IsEnabled).Returns(true);
        mock.Setup(m => m.Description).Returns($"Mock module {name}");
        mock.Setup(m => m.Author).Returns("Test");
        
        return mock.Object;
    }

    #endregion
} 