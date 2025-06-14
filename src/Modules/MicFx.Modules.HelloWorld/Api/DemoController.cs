using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MicFx.SharedKernel.Common;
using MicFx.Modules.HelloWorld.Services;

namespace MicFx.Modules.HelloWorld.Api
{
    /// <summary>
    /// Demo controller showcasing framework routing and convention features
    /// Focused on demonstrating framework capabilities, not business logic
    /// </summary>
    [ApiController]
    [Route("api/hello-world/demo")]
    public class DemoController : ControllerBase
    {
        private readonly ILogger<DemoController> _logger;

        public DemoController(ILogger<DemoController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Demonstrates basic auto-mapping capabilities
        /// </summary>
        [HttpGet("routing")]
        public async Task<ActionResult<ApiResponse<object>>> GetRoutingDemo()
        {
            _logger.LogInformation("Processing routing demo request");

            await Task.Delay(10); // Minimal delay for async demo

            var result = new
            {
                Message = "This endpoint demonstrates MicFx auto-routing capabilities",
                Route = "/api/hello-world/demo/routing",
                Controller = "Demo",
                AutoMapped = true,
                FrameworkFeature = "Convention-based routing",
                Timestamp = DateTime.UtcNow
            };

            return Ok(ApiResponse<object>.Ok(result, "Routing demo executed successfully"));
        }

        /// <summary>
        /// Demonstrates parameter binding conventions
        /// </summary>
        [HttpGet("parameters/{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetParameterDemo(
            int id, 
            [FromQuery] string? filter = null,
            [FromQuery] int limit = 10)
        {
            _logger.LogInformation("Processing parameter demo request with ID {Id}, Filter: {Filter}, Limit: {Limit}", 
                id, filter, limit);

            await Task.Delay(10);

            var result = new
            {
                Message = "Parameter binding demonstration",
                Parameters = new
                {
                    RouteParameter = id,
                    QueryFilter = filter,
                    QueryLimit = limit
                },
                Route = "/api/hello-world/demo/parameters/{id}",
                FrameworkFeature = "Parameter binding and validation",
                Timestamp = DateTime.UtcNow
            };

            return Ok(ApiResponse<object>.Ok(result, "Parameter demo executed successfully"));
        }

        /// <summary>
        /// Demonstrates structured request/response patterns
        /// </summary>
        [HttpPost("request-response")]
        public async Task<ActionResult<ApiResponse<object>>> PostRequestResponseDemo([FromBody] DemoRequest request)
        {
            _logger.LogInformation("Processing request-response demo with data: {@Request}", request);

            await Task.Delay(10);

            if (request == null)
            {
                return BadRequest(ApiResponse<object>.Error("Request body is required"));
            }

            var result = new
            {
                Message = "Request-response pattern demonstration",
                ReceivedData = request,
                ProcessedAt = DateTime.UtcNow,
                Route = "/api/hello-world/demo/request-response",
                FrameworkFeature = "Structured API responses",
                ValidationPassed = !string.IsNullOrEmpty(request.Name)
            };

            return Ok(ApiResponse<object>.Ok(result, "Request-response demo executed successfully"));
        }

        /// <summary>
        /// Demonstrates HTTP status code handling
        /// </summary>
        [HttpGet("status-codes/{code:int}")]
        public async Task<ActionResult<ApiResponse<object>>> GetStatusCodeDemo(int code)
        {
            _logger.LogInformation("Processing status code demo for code {StatusCode}", code);

            await Task.Delay(10);

            var result = new
            {
                Message = "Status code demonstration",
                RequestedCode = code,
                FrameworkFeature = "HTTP status code handling",
                Timestamp = DateTime.UtcNow
            };

            return code switch
            {
                200 => Ok(ApiResponse<object>.Ok(result, "Success response")),
                400 => BadRequest(ApiResponse<object>.Error("Bad request demonstration")),
                404 => NotFound(ApiResponse<object>.Error("Not found demonstration")),
                500 => StatusCode(500, ApiResponse<object>.Error("Server error demonstration")),
                _ => Ok(ApiResponse<object>.Ok(result, $"Status code {code} demonstration"))
            };
        }

        /// <summary>
        /// Demonstrates framework conventions and naming
        /// </summary>
        [HttpGet("conventions")]
        public async Task<ActionResult<ApiResponse<object>>> GetConventionsDemo()
        {
            _logger.LogInformation("Processing conventions demo request");

            await Task.Delay(10);

            var result = new
            {
                Message = "Framework conventions demonstration",
                Conventions = new
                {
                    Routing = "Convention-based auto-routing",
                    Naming = "Kebab-case URL conversion",
                    Structure = "Folder-based organization",
                    ResponseFormat = "Standardized ApiResponse<T>",
                    Logging = "Structured logging with module context"
                },
                ExampleRoutes = new[]
                {
                    "/api/hello-world/demo/routing",
                    "/api/hello-world/demo/parameters/{id}",
                    "/api/hello-world/demo/conventions"
                },
                FrameworkFeature = "MicFx conventions",
                Timestamp = DateTime.UtcNow
            };

            return Ok(ApiResponse<object>.Ok(result, "Conventions demo executed successfully"));
        }
    }

    /// <summary>
    /// Demo request model for POST endpoint
    /// </summary>
    public class DemoRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? Value { get; set; }
        public DateTime? Date { get; set; }
    }
}