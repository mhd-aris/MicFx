using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MicFx.SharedKernel.Modularity;
using MicFx.SharedKernel.Common;

namespace MicFx.Infrastructure.Swagger;

/// <summary>
/// Auto-discovery Swagger configuration for MicFx Framework
/// Infrastructure layer yang independen dari Core
/// </summary>
public static class SwaggerAutoDiscoveryExtensions
{
    /// <summary>
    /// Adds Swagger with auto-discovery endpoints from all modules
    /// </summary>
    public static IServiceCollection AddMicFxSwaggerInfrastructure(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            // Main API documentation
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MicFx Modular Framework API",
                Version = "v1.0",
                Description = "Enterprise modular framework with smart auto-routing for API, MVC, and Admin endpoints",
                Contact = new OpenApiContact
                {
                    Name = "MicFx Framework",
                    Email = "dev@micfx.io"
                }
            });

            // Auto-discover and group by module with routing type
            options.TagActionsBy(api =>
            {
                var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "Framework";
                var routeTemplate = api.RelativePath ?? "";
                
                var moduleName = ExtractModuleFromController(controllerName);
                var routingType = DetermineRoutingType(routeTemplate);
                
                return new[] { $"{moduleName} ({routingType})" };
            });

            // Group endpoints by module for better organization  
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                // Include all endpoints for main documentation
                return docName == "v1";
            });

            // Auto-discovery all XML comments from MicFx assemblies
            IncludeXmlCommentsAutoDiscovery(options);

            // Configure JWT security scheme
            ConfigureSecurityScheme(options);

            // Custom operation IDs for better Swagger UI
            options.CustomOperationIds(api =>
            {
                var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "Framework";
                var actionName = api.ActionDescriptor.RouteValues["action"] ?? "Unknown";
                var moduleName = ExtractModuleFromController(controllerName);
                var routeTemplate = api.RelativePath ?? "";
                
                // Get controller type for better routing type detection
                Type? controllerType = null;
                if (api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerActionDescriptor)
                {
                    controllerType = controllerActionDescriptor.ControllerTypeInfo.AsType();
                }
                
                var routingType = DetermineRoutingType(routeTemplate, controllerType);
                
                return $"{moduleName}_{routingType}_{actionName}";
            });

            // Automatic response examples for all ApiResponse<T> types
            options.OperationFilter<AutoResponseExampleFilter>();
            
            // Custom schema IDs to avoid conflicts
            options.CustomSchemaIds(type => 
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ApiResponse<>))
                {
                    var innerType = type.GetGenericArguments()[0];
                    return $"ApiResponseOf{innerType.Name}";
                }
                return type.Name;
            });
        });

        return services;
    }

    /// <summary>
    /// Uses Swagger UI with MicFx configuration
    /// </summary>
    public static IApplicationBuilder UseMicFxSwaggerInfrastructure(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        // Only active in development environment
        if (!environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api/docs/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/api/docs/v1/swagger.json", "MicFx Framework v1.0");
            options.RoutePrefix = "api/docs";

            // UI customization
            options.DocumentTitle = "MicFx Framework - Smart Auto-Routing API";
            options.DefaultModelsExpandDepth(1);
            options.DefaultModelExpandDepth(1);
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.ShowExtensions();

            // Custom CSS for better appearance
            options.InjectStylesheet("/css/swagger-micfx.css");
        });

        return app;
    }

    /// <summary>
    /// Determines routing type based on route template and controller namespace/folder structure
    /// </summary>
    private static string DetermineRoutingType(string routeTemplate, Type? controllerType = null)
    {
        if (string.IsNullOrEmpty(routeTemplate))
            return "Unknown";

        routeTemplate = routeTemplate.ToLowerInvariant();

        // Check route patterns first
        if (routeTemplate.StartsWith("api/"))
            return "API";
        
        if (routeTemplate.StartsWith("admin/"))
            return "Admin";

        // Check namespace/folder structure if controller type is available
        if (controllerType != null)
        {
            var namespaceName = controllerType.Namespace ?? "";
            
            // Controllers in Api folder/namespace should be treated as API
            if (namespaceName.Contains(".Api"))
                return "API";
                
            // Controllers in Admin folder/namespace should be treated as Admin
            if (namespaceName.Contains(".Admin"))
                return "Admin";
        }
            
        return "MVC";
    }

            /// <summary>
        /// Extract module name from controller name
        /// Based on MicFx module naming conventions and folder structure patterns
        /// </summary>
        private static string ExtractModuleFromController(string? controllerName)
        {
            if (string.IsNullOrEmpty(controllerName))
                return "Framework";

            // Remove Controller suffix
            var cleanName = controllerName.Replace("Controller", "");

            // Check if from MicFx module based on assembly name
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            // Look for controller types in all MicFx modules
            foreach (var assembly in assemblies.Where(a => a.GetName().Name?.StartsWith("MicFx.Modules.") == true))
            {
                var controllerTypes = assembly.GetTypes()
                    .Where(t => t.Name.Equals($"{controllerName}Controller", StringComparison.OrdinalIgnoreCase) ||
                               t.Name.Equals($"{controllerName}", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var controllerType in controllerTypes)
                {
                    var namespaceName = controllerType.Namespace ?? "";
                    
                    // Extract module name from namespace
                    // Examples: 
                    // MicFx.Modules.Auth.Api -> Auth
                    // MicFx.Modules.HelloWorld.Controllers -> HelloWorld
                    var namespaceParts = namespaceName.Split('.');
                    if (namespaceParts.Length >= 3 && namespaceParts[0] == "MicFx" && namespaceParts[1] == "Modules")
                    {
                        return namespaceParts[2]; // MicFx.Modules.HelloWorld -> HelloWorld
                    }
                }
            }

            // Fallback: try assembly name extraction
            var moduleAssembly = assemblies.FirstOrDefault(a =>
                a.GetName().Name?.Contains($"MicFx.Modules.") == true);

            if (moduleAssembly != null)
            {
                var assemblyName = moduleAssembly.GetName().Name ?? "";
                var parts = assemblyName.Split('.');
                if (parts.Length >= 3 && parts[0] == "MicFx" && parts[1] == "Modules")
                {
                    return parts[2]; // MicFx.Modules.HelloWorld -> HelloWorld
                }
            }

            // Final fallback - clean the controller name
            return cleanName ?? "Unknown";
        }

    /// <summary>
    /// Auto-discovery XML comments from all MicFx assemblies
    /// </summary>
    private static void IncludeXmlCommentsAutoDiscovery(SwaggerGenOptions options)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("MicFx") == true);

        foreach (var assembly in assemblies)
        {
            var xmlFile = $"{assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        }
    }

    /// <summary>
    /// Configure JWT Bearer security scheme
    /// </summary>
    private static void ConfigureSecurityScheme(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    }
}