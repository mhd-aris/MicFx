using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using MicFx.SharedKernel.Common;

namespace MicFx.Infrastructure.Swagger;

/// <summary>
/// Simplified Swagger configuration for MicFx Framework
/// SIMPLIFIED: Removed over-engineered auto-discovery and routing type detection
/// </summary>
public static class SwaggerAutoDiscoveryExtensions
{
    /// <summary>
    /// Adds Swagger with simplified configuration
    /// SIMPLIFIED: Basic Swagger setup without complex auto-discovery
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
                Description = "Enterprise modular framework with conventional routing",
                Contact = new OpenApiContact
                {
                    Name = "MicFx Framework",
                    Email = "dev@micfx.io"
                }
            });

            // Simple grouping by controller name without complex routing type detection
            options.TagActionsBy(api =>
            {
                var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "Framework";
                var moduleName = ExtractModuleFromController(controllerName);
                
                return new[] { moduleName };
            });

            // Include all endpoints for main documentation
            options.DocInclusionPredicate((docName, apiDesc) => docName == "v1");

            // Simple XML comments inclusion
            IncludeXmlCommentsFromCurrentAssembly(options);

            // Configure JWT security scheme
            ConfigureSecurityScheme(options);

            // Simple operation IDs without complex routing detection
            options.CustomOperationIds(api =>
            {
                var controllerName = api.ActionDescriptor.RouteValues["controller"] ?? "Framework";
                var actionName = api.ActionDescriptor.RouteValues["action"] ?? "Unknown";
                var moduleName = ExtractModuleFromController(controllerName);
                
                return $"{moduleName}_{actionName}";
            });

            // Automatic response examples for ApiResponse<T> types only
            options.OperationFilter<AutoResponseExampleFilter>();
            
            // Simple schema IDs to avoid conflicts
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
    /// Uses Swagger UI with simplified MicFx configuration
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
            options.DocumentTitle = "MicFx Framework - API Documentation";
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
    /// Simple module name extraction from controller name
    /// SIMPLIFIED: Basic extraction without complex namespace scanning
    /// </summary>
    private static string ExtractModuleFromController(string? controllerName)
    {
        if (string.IsNullOrEmpty(controllerName))
            return "Framework";

        // Remove Controller suffix
        var cleanName = controllerName.Replace("Controller", "");

        // Simple module detection from assembly name pattern
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        
        foreach (var assembly in assemblies.Where(a => a.GetName().Name?.StartsWith("MicFx.Modules.") == true))
        {
            var assemblyName = assembly.GetName().Name;
            if (assemblyName != null && assemblyName.StartsWith("MicFx.Modules."))
            {
                var parts = assemblyName.Split('.');
                if (parts.Length >= 3)
                {
                    return parts[2]; // Extract module name (e.g., "Auth" from "MicFx.Modules.Auth")
                }
            }
        }

        // Fallback to controller name
        return cleanName;
    }

    /// <summary>
    /// Include XML comments from current assembly only
    /// SIMPLIFIED: Only current assembly, no complex auto-discovery
    /// </summary>
    private static void IncludeXmlCommentsFromCurrentAssembly(SwaggerGenOptions options)
    {
        try
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var xmlFile = $"{currentAssembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        }
        catch (Exception)
        {
            // Ignore XML comment loading errors
        }
    }

    /// <summary>
    /// Configure JWT security scheme for Swagger
    /// </summary>
    private static void ConfigureSecurityScheme(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
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