using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicFx.Infrastructure.Swagger;

/// <summary>
/// Operation filter to add response examples automatically
/// Independent of module-specific configuration
/// </summary>
public class AutoResponseExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add standard response descriptions
        AddStandardResponseDescriptions(operation);

        // Add examples based on HTTP methods
        AddResponseExamplesByMethod(operation, context);
    }

    private void AddStandardResponseDescriptions(OpenApiOperation operation)
    {
        // 200 - Success
        if (!operation.Responses.ContainsKey("200"))
        {
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Operation successful",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(true),
                            ["message"] = new OpenApiString("Operation completed successfully"),
                            ["data"] = new OpenApiObject(),
                            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                        }
                    }
                }
            });
        }

        // 400 - Bad Request
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad request - invalid data",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(false),
                            ["message"] = new OpenApiString("Validation failed"),
                            ["errors"] = new OpenApiArray
                            {
                                new OpenApiString("Field 'name' is required")
                            },
                            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                        }
                    }
                }
            });
        }

        // 401 - Unauthorized
        if (!operation.Responses.ContainsKey("401"))
        {
            operation.Responses.Add("401", new OpenApiResponse
            {
                Description = "Unauthorized - authentication required",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(false),
                            ["message"] = new OpenApiString("Authentication required"),
                            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                        }
                    }
                }
            });
        }

        // 404 - Not Found
        if (!operation.Responses.ContainsKey("404"))
        {
            operation.Responses.Add("404", new OpenApiResponse
            {
                Description = "Resource not found",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(false),
                            ["message"] = new OpenApiString("Resource not found"),
                            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                        }
                    }
                }
            });
        }

        // 500 - Internal Server Error
        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal server error",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(false),
                            ["message"] = new OpenApiString("An internal error occurred"),
                            ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                        }
                    }
                }
            });
        }
    }

    private void AddResponseExamplesByMethod(OpenApiOperation operation, OperationFilterContext context)
    {
        var httpMethod = context.ApiDescription.HttpMethod?.ToUpper();
        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
        var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];

        // Customize examples based on HTTP method
        switch (httpMethod)
        {
            case "GET":
                AddGetExamples(operation, controllerName, actionName);
                break;
            case "POST":
                AddPostExamples(operation, controllerName, actionName);
                break;
            case "PUT":
                AddPutExamples(operation, controllerName, actionName);
                break;
            case "DELETE":
                AddDeleteExamples(operation, controllerName, actionName);
                break;
        }
    }

    private void AddGetExamples(OpenApiOperation operation, string? controller, string? action)
    {
        // Override 200 response for GET requests
        if (operation.Responses.ContainsKey("200"))
        {
            var response = operation.Responses["200"];
            if (response.Content != null && response.Content.ContainsKey("application/json"))
            {
                response.Content["application/json"].Example = new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString($"Data retrieved from {controller}"),
                    ["data"] = CreateSampleDataForController(controller),
                    ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                };
            }
        }
    }

    private void AddPostExamples(OpenApiOperation operation, string? controller, string? action)
    {
        // Override 200/201 response for POST requests
        var responseKey = operation.Responses.ContainsKey("201") ? "201" : "200";
        if (operation.Responses.ContainsKey(responseKey))
        {
            var response = operation.Responses[responseKey];
            if (response.Content != null && response.Content.ContainsKey("application/json"))
            {
                response.Content["application/json"].Example = new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString($"Resource created in {controller}"),
                    ["data"] = new OpenApiObject
                    {
                        ["id"] = new OpenApiInteger(123),
                        ["createdAt"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                    },
                    ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                };
            }
        }
    }

    private void AddPutExamples(OpenApiOperation operation, string? controller, string? action)
    {
        // Override 200 response for PUT requests
        if (operation.Responses.ContainsKey("200"))
        {
            var response = operation.Responses["200"];
            if (response.Content != null && response.Content.ContainsKey("application/json"))
            {
                response.Content["application/json"].Example = new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString($"Resource updated in {controller}"),
                    ["data"] = new OpenApiObject
                    {
                        ["id"] = new OpenApiInteger(123),
                        ["updatedAt"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                    },
                    ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                };
            }
        }
    }

    private void AddDeleteExamples(OpenApiOperation operation, string? controller, string? action)
    {
        // Override 200 response for DELETE requests
        if (operation.Responses.ContainsKey("200"))
        {
            var response = operation.Responses["200"];
            if (response.Content != null && response.Content.ContainsKey("application/json"))
            {
                response.Content["application/json"].Example = new OpenApiObject
                {
                    ["success"] = new OpenApiBoolean(true),
                    ["message"] = new OpenApiString($"Resource deleted from {controller}"),
                    ["timestamp"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
                };
            }
        }
    }

    private OpenApiObject CreateSampleDataForController(string? controller)
    {
        // Create sample data based on controller name
        return controller?.ToLower() switch
        {
            "helloworld" => new OpenApiObject
            {
                ["message"] = new OpenApiString("Hello from MicFx Framework!"),
                ["version"] = new OpenApiString("1.0.0"),
                ["features"] = new OpenApiArray
                {
                    new OpenApiString("Auto-mapping"),
                    new OpenApiString("Modular architecture"),
                    new OpenApiString("Convention-based routing")
                }
            },
            "auth" => new OpenApiObject
            {
                ["token"] = new OpenApiString("jwt.token.here"),
                ["expiresIn"] = new OpenApiInteger(3600),
                ["user"] = new OpenApiObject
                {
                    ["id"] = new OpenApiInteger(1),
                    ["username"] = new OpenApiString("johndoe"),
                    ["email"] = new OpenApiString("john@example.com")
                }
            },
            _ => new OpenApiObject
            {
                ["id"] = new OpenApiInteger(1),
                ["name"] = new OpenApiString("Sample Item"),
                ["description"] = new OpenApiString($"Sample data from {controller} module"),
                ["createdAt"] = new OpenApiString(DateTime.UtcNow.ToString("O"))
            }
        };
    }
}