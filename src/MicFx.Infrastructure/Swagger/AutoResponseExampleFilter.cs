using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicFx.Infrastructure.Swagger;

/// <summary>
/// Operation filter that adds basic standard response examples
/// </summary>
public class AutoResponseExampleFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add basic standard response descriptions only
        AddBasicResponseDescriptions(operation);
    }

    /// <summary>
    /// Adds basic standard HTTP response descriptions
    /// </summary>
    private static void AddBasicResponseDescriptions(OpenApiOperation operation)
    {
        // 200 - Success
        if (!operation.Responses.ContainsKey("200"))
        {
            operation.Responses.Add("200", new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(true),
                            ["message"] = new OpenApiString("Operation completed successfully"),
                            ["data"] = new OpenApiObject(),
                            ["timestamp"] = new OpenApiString("2024-01-01T00:00:00Z")
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
                Description = "Bad request",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Example = new OpenApiObject
                        {
                            ["success"] = new OpenApiBoolean(false),
                            ["message"] = new OpenApiString("Invalid request data"),
                            ["errors"] = new OpenApiArray
                            {
                                new OpenApiString("Validation error example")
                            },
                            ["timestamp"] = new OpenApiString("2024-01-01T00:00:00Z")
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
                            ["timestamp"] = new OpenApiString("2024-01-01T00:00:00Z")
                        }
                    }
                }
            });
        }
    }
}