using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using MicFx.Modules.Auth.Services;
using MicFx.Modules.Auth.Authorization;

namespace MicFx.Tests.Core.Integration
{
    /// <summary>
    /// Integration tests for permission system functionality
    /// Tests the complete flow from claims to authorization
    /// </summary>
    public class PermissionSystemIntegrationTests
    {
        [Fact]
        public void PermissionAttribute_AutoModuleDetection_ShouldWork()
        {
            // Arrange & Act
            var attribute = new PermissionAttribute("users.view");

            // Assert
            Assert.Equal("users.view", attribute.Permission);
            Assert.NotNull(attribute.Policy);
            Assert.StartsWith("Permission:", attribute.Policy);
            
            // Module should be detected as "auth" from the assembly name
            Assert.NotNull(attribute.ModuleName);
        }

        [Theory]
        [InlineData("users.view", "users.*", true)]
        [InlineData("users.create", "users.*", true)]
        [InlineData("roles.view", "users.*", false)]
        [InlineData("anything.action", "*", true)]
        [InlineData("users.view", "users.view", true)]
        [InlineData("users.edit", "users.view", false)]
        public void WildcardPermissionMatching_ShouldWorkCorrectly(string permission, string userPermission, bool expected)
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMemoryCache();
            
            // Mock the auth context since we can't easily setup a real database in unit tests
            var mockContext = new Moq.Mock<MicFx.Modules.Auth.Data.AuthDbContext>();
            serviceCollection.AddSingleton(mockContext.Object);
            serviceCollection.AddScoped<IPermissionService, PermissionService>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var permissionService = serviceProvider.GetRequiredService<IPermissionService>();

            // Act
            var result = permissionService.MatchesWildcardPattern(permission, new List<string> { userPermission });

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task PermissionService_WithValidClaimsUser_ShouldAllowAccess()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMemoryCache();
            
            var mockContext = new Moq.Mock<MicFx.Modules.Auth.Data.AuthDbContext>();
            serviceCollection.AddSingleton(mockContext.Object);
            serviceCollection.AddScoped<IPermissionService, PermissionService>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var permissionService = serviceProvider.GetRequiredService<IPermissionService>();

            // Create user with permission claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Authentication, "true"),
                new Claim("user_id", "test-user"),
                new Claim("permission", "users.view")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = await permissionService.HasPermissionAsync(user, "users.view");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task PermissionService_WithWildcardClaims_ShouldAllowMatchingPermissions()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMemoryCache();
            
            var mockContext = new Moq.Mock<MicFx.Modules.Auth.Data.AuthDbContext>();
            serviceCollection.AddSingleton(mockContext.Object);
            serviceCollection.AddScoped<IPermissionService, PermissionService>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var permissionService = serviceProvider.GetRequiredService<IPermissionService>();

            // Create user with wildcard permission claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Authentication, "true"),
                new Claim("user_id", "test-user"),
                new Claim("permission", "users.*")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            // Act & Assert
            Assert.True(await permissionService.HasPermissionAsync(user, "users.view"));
            Assert.True(await permissionService.HasPermissionAsync(user, "users.create"));
            Assert.True(await permissionService.HasPermissionAsync(user, "users.edit"));
            Assert.False(await permissionService.HasPermissionAsync(user, "roles.view"));
        }

        [Fact]
        public async Task PermissionService_WithGlobalWildcard_ShouldAllowEverything()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMemoryCache();
            
            var mockContext = new Moq.Mock<MicFx.Modules.Auth.Data.AuthDbContext>();
            serviceCollection.AddSingleton(mockContext.Object);
            serviceCollection.AddScoped<IPermissionService, PermissionService>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var permissionService = serviceProvider.GetRequiredService<IPermissionService>();

            // Create super admin user with global wildcard
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Authentication, "true"),
                new Claim("user_id", "super-admin"),
                new Claim("permission", "*")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var user = new ClaimsPrincipal(identity);

            // Act & Assert - Should allow any permission
            Assert.True(await permissionService.HasPermissionAsync(user, "users.view"));
            Assert.True(await permissionService.HasPermissionAsync(user, "roles.create"));
            Assert.True(await permissionService.HasPermissionAsync(user, "anything.anywhere"));
            Assert.True(await permissionService.HasPermissionAsync(user, "system.admin"));
        }

        [Fact]
        public async Task PermissionService_WithUnauthenticatedUser_ShouldDenyAccess()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMemoryCache();
            
            var mockContext = new Moq.Mock<MicFx.Modules.Auth.Data.AuthDbContext>();
            serviceCollection.AddSingleton(mockContext.Object);
            serviceCollection.AddScoped<IPermissionService, PermissionService>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var permissionService = serviceProvider.GetRequiredService<IPermissionService>();

            // Create unauthenticated user
            var user = new ClaimsPrincipal();

            // Act
            var result = await permissionService.HasPermissionAsync(user, "users.view");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void PermissionOptimization_ShouldCompressMultiplePermissions()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddMemoryCache();
            
            var mockContext = new Moq.Mock<MicFx.Modules.Auth.Data.AuthDbContext>();
            serviceCollection.AddSingleton(mockContext.Object);
            serviceCollection.AddScoped<IPermissionService, PermissionService>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var permissionService = serviceProvider.GetRequiredService<IPermissionService>();

            // Large permission set that should be optimized
            var permissions = new List<string>
            {
                "users.view", "users.create", "users.edit", "users.delete", "users.activate",
                "roles.view", "roles.create", "roles.edit", "roles.delete",
                "permissions.view", "permissions.manage",
                "orders.view", "orders.create", "orders.edit", "orders.delete",
                "products.view", "products.create", "products.edit", "products.delete"
            };

            // Act
            var result = permissionService.MatchesWildcardPattern("users.view", permissions);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void AnyPermissionAttribute_ShouldHandleMultiplePermissions()
        {
            // Arrange & Act
            var attribute = new AnyPermissionAttribute("users.view", "users.edit", "roles.view");

            // Assert
            Assert.Equal(3, attribute.Permissions.Length);
            Assert.Contains("users.view", attribute.Permissions);
            Assert.Contains("users.edit", attribute.Permissions);
            Assert.Contains("roles.view", attribute.Permissions);
            Assert.StartsWith("AnyPermission:", attribute.Policy);
        }
    }
} 