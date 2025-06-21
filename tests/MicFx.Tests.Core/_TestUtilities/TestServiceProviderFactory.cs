using Microsoft.Extensions.Logging;
using Moq;
using MicFx.Core.Modularity;

namespace MicFx.Tests.Core._TestUtilities;

/// <summary>
/// Factory untuk membuat service provider untuk testing
/// SIMPLIFIED: Only contains methods that are actually used in tests
/// </summary>
public static class TestServiceProviderFactory
{
    /// <summary>
    /// Membuat mock IServiceProvider untuk testing
    /// </summary>
    public static Mock<IServiceProvider> CreateMockServiceProvider()
    {
        var mockServiceProvider = new Mock<IServiceProvider>();
        
        // Setup basic services
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger<ModuleLifecycleManager>>();
        
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
        
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILoggerFactory)))
            .Returns(mockLoggerFactory.Object);
        
        mockServiceProvider.Setup(sp => sp.GetService(typeof(ILogger<ModuleLifecycleManager>)))
            .Returns(mockLogger.Object);
        
        return mockServiceProvider;
    }
} 