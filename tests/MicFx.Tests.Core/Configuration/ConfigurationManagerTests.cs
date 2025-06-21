using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MicFx.Core.Configuration;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Common.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace MicFx.Tests.Core.Configuration;

/// <summary>
/// Unit tests untuk ConfigurationManager
/// Focus: Module configuration registration, validation, dan management
/// </summary>
public class ConfigurationManagerTests : IDisposable
{
    private readonly Mock<ILogger<MicFxConfigurationManager>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly MicFxConfigurationManager _sut; // System Under Test

    public ConfigurationManagerTests()
    {
        _mockLogger = new Mock<ILogger<MicFxConfigurationManager>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _sut = new MicFxConfigurationManager(_mockConfiguration.Object, _mockLogger.Object);
    }

    public void Dispose()
    {
        // Clean up resources if needed
    }

    #region RegisterModuleConfiguration Tests

    [Fact]
    public void RegisterModuleConfiguration_WithValidConfiguration_ShouldRegisterSuccessfully()
    {
        // Arrange
        var mockConfig = CreateMockModuleConfiguration("TestModule", "TestSection");

        // Act
        _sut.RegisterModuleConfiguration(mockConfig.Object);

        // Assert
        var retrievedConfig = _sut.GetModuleConfiguration("TestModule");
        retrievedConfig.Should().NotBeNull();
        retrievedConfig.Should().Be(mockConfig.Object);
    }

    [Fact]
    public void RegisterModuleConfiguration_WithDuplicateModule_ShouldReplaceExisting()
    {
        // Arrange
        var firstConfig = CreateMockModuleConfiguration("TestModule", "TestSection");
        var secondConfig = CreateMockModuleConfiguration("TestModule", "TestSection");

        // Act
        _sut.RegisterModuleConfiguration(firstConfig.Object);
        _sut.RegisterModuleConfiguration(secondConfig.Object);

        // Assert
        var retrievedConfig = _sut.GetModuleConfiguration("TestModule");
        retrievedConfig.Should().Be(secondConfig.Object);
        
        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already registered")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void RegisterModuleConfiguration_WithValidationFailure_ShouldThrowConfigurationException()
    {
        // Arrange
        var mockConfig = CreateMockModuleConfiguration("InvalidModule", "InvalidSection");
        mockConfig.Setup(c => c.Validate())
            .Returns(new ValidationResult("Validation failed"));

        // Act & Assert
        var action = () => _sut.RegisterModuleConfiguration(mockConfig.Object);
        action.Should().Throw<ConfigurationException>()
            .WithMessage("*Configuration validation failed*");
    }

    [Fact]
    public void RegisterModuleConfiguration_WithValidationException_ShouldThrowConfigurationException()
    {
        // Arrange
        var mockConfig = CreateMockModuleConfiguration("ExceptionModule", "ExceptionSection");
        mockConfig.Setup(c => c.Validate())
            .Throws(new InvalidOperationException("Validation error"));

        // Act & Assert
        var action = () => _sut.RegisterModuleConfiguration(mockConfig.Object);
        action.Should().Throw<ConfigurationException>()
            .WithMessage("*Error validating configuration*");
    }

    #endregion

    #region GetModuleConfiguration<T> Tests

    [Fact]
    public void GetModuleConfiguration_ByType_WithExistingConfiguration_ShouldReturnConfiguration()
    {
        // Arrange
        var mockConfig = CreateMockModuleConfiguration<TestConfigType>("TestModule", "TestSection");
        _sut.RegisterModuleConfiguration(mockConfig.Object);

        // Act
        var result = _sut.GetModuleConfiguration<TestConfigType>();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(mockConfig.Object);
    }

    [Fact]
    public void GetModuleConfiguration_ByType_WithNonExistentConfiguration_ShouldReturnNull()
    {
        // Act
        var result = _sut.GetModuleConfiguration<TestConfigType>();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetModuleConfiguration(string) Tests

    [Fact]
    public void GetModuleConfiguration_ByName_WithValidName_ShouldReturnConfiguration()
    {
        // Arrange
        var mockConfig = CreateMockModuleConfiguration("TestModule", "TestSection");
        _sut.RegisterModuleConfiguration(mockConfig.Object);

        // Act
        var result = _sut.GetModuleConfiguration("TestModule");

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(mockConfig.Object);
    }

    [Fact]
    public void GetModuleConfiguration_ByName_WithNullName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => _sut.GetModuleConfiguration((string)null!);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Module name cannot be null or empty*")
            .WithParameterName("moduleName");
    }

    [Fact]
    public void GetModuleConfiguration_ByName_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => _sut.GetModuleConfiguration("");
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Module name cannot be null or empty*")
            .WithParameterName("moduleName");
    }

    [Fact]
    public void GetModuleConfiguration_ByName_WithWhitespaceName_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => _sut.GetModuleConfiguration("   ");
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Module name cannot be null or empty*")
            .WithParameterName("moduleName");
    }

    [Fact]
    public void GetModuleConfiguration_ByName_WithNonExistentName_ShouldReturnNull()
    {
        // Act
        var result = _sut.GetModuleConfiguration("NonExistentModule");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllConfigurations Tests

    [Fact]
    public void GetAllConfigurations_WithNoConfigurations_ShouldReturnEmptyCollection()
    {
        // Act
        var result = _sut.GetAllConfigurations();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAllConfigurations_WithMultipleConfigurations_ShouldReturnAllConfigurations()
    {
        // Arrange
        var config1 = CreateMockModuleConfiguration("Module1", "Section1");
        var config2 = CreateMockModuleConfiguration("Module2", "Section2");
        var config3 = CreateMockModuleConfiguration("Module3", "Section3");

        _sut.RegisterModuleConfiguration(config1.Object);
        _sut.RegisterModuleConfiguration(config2.Object);
        _sut.RegisterModuleConfiguration(config3.Object);

        // Act
        var result = _sut.GetAllConfigurations();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(config1.Object);
        result.Should().Contain(config2.Object);
        result.Should().Contain(config3.Object);
    }

    #endregion

    #region ValidateAllConfigurations Tests

    [Fact]
    public void ValidateAllConfigurations_WithNoConfigurations_ShouldReturnSuccess()
    {
        // Act
        var result = _sut.ValidateAllConfigurations();

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void ValidateAllConfigurations_WithValidConfigurations_ShouldReturnSuccess()
    {
        // Arrange
        var config1 = CreateMockModuleConfiguration("Module1", "Section1");
        var config2 = CreateMockModuleConfiguration("Module2", "Section2");

        _sut.RegisterModuleConfiguration(config1.Object);
        _sut.RegisterModuleConfiguration(config2.Object);

        // Act
        var result = _sut.ValidateAllConfigurations();

        // Assert
        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void ValidateAllConfigurations_WithInvalidConfigurations_ShouldReturnFailure()
    {
        // Arrange
        var validConfig = CreateMockModuleConfiguration("ValidModule", "ValidSection");
        
        // Register valid config first 
        _sut.RegisterModuleConfiguration(validConfig.Object);

        // Create invalid config that will fail during ValidateAllConfigurations, not during registration
        var invalidConfig = CreateMockModuleConfiguration("InvalidModule", "InvalidSection");
        invalidConfig.Setup(c => c.Validate())
            .Returns(ValidationResult.Success!) // Pass during registration
            .Callback(() => { /* First call for registration passes */ });

        _sut.RegisterModuleConfiguration(invalidConfig.Object);

        // Now setup the invalid response for the ValidateAllConfigurations call
        invalidConfig.Setup(c => c.Validate())
            .Returns(new ValidationResult("Configuration is invalid"));

        // Act
        var result = _sut.ValidateAllConfigurations();

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Contain("InvalidModule");
        result.ErrorMessage.Should().Contain("Configuration is invalid");
    }

    [Fact]
    public void ValidateAllConfigurations_WithExceptionDuringValidation_ShouldIncludeError()
    {
        // Arrange
        var validConfig = CreateMockModuleConfiguration("ValidModule", "ValidSection");
        
        // Register valid config first
        _sut.RegisterModuleConfiguration(validConfig.Object);

        // Create config that passes registration but throws during ValidateAllConfigurations
        var exceptionConfig = CreateMockModuleConfiguration("ExceptionModule", "ExceptionSection");
        exceptionConfig.Setup(c => c.Validate())
            .Returns(ValidationResult.Success!) // Pass during registration
            .Callback(() => { /* First call for registration passes */ });

        _sut.RegisterModuleConfiguration(exceptionConfig.Object);

        // Now setup exception for ValidateAllConfigurations call
        exceptionConfig.Setup(c => c.Validate())
            .Throws(new InvalidOperationException("Validation threw exception"));

        // Act
        var result = _sut.ValidateAllConfigurations();

        // Assert
        result.Should().NotBe(ValidationResult.Success);
        result.ErrorMessage.Should().Contain("ExceptionModule");
        result.ErrorMessage.Should().Contain("Validation error");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a mock IModuleConfiguration for testing
    /// </summary>
    private static Mock<IModuleConfiguration<object>> CreateMockModuleConfiguration(string moduleName, string sectionName)
    {
        var mock = new Mock<IModuleConfiguration<object>>();
        mock.Setup(c => c.ModuleName).Returns(moduleName);
        mock.Setup(c => c.SectionName).Returns(sectionName);
        mock.Setup(c => c.Validate()).Returns(ValidationResult.Success!);
        
        return mock;
    }

    /// <summary>
    /// Creates a strongly-typed mock IModuleConfiguration for testing
    /// </summary>
    private static Mock<IModuleConfiguration<T>> CreateMockModuleConfiguration<T>(string moduleName, string sectionName) where T : class
    {
        var mock = new Mock<IModuleConfiguration<T>>();
        mock.Setup(c => c.ModuleName).Returns(moduleName);
        mock.Setup(c => c.SectionName).Returns(sectionName);
        mock.Setup(c => c.Validate()).Returns(ValidationResult.Success!);
        
        return mock;
    }

    #endregion

    #region Test Helper Classes

    public class TestConfigType
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    #endregion
} 