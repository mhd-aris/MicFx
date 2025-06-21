using Microsoft.AspNetCore.Mvc;
using MicFx.SharedKernel.Common;
using MicFx.SharedKernel.Modularity;
using MicFx.Modules.HelloWorld.Domain;
using MicFx.Modules.HelloWorld.Services;

namespace MicFx.Modules.HelloWorld.Api;

/// <summary>
/// HelloWorld API Controller - Pure JSON API endpoints
/// Demonstrates API-first approach with structured responses
/// Uses auto-routing: /api/hello-world/* (detected from Api folder)
/// </summary>
[ApiController]
[Route("api/hello-world")]
[Produces("application/json")]
public class HelloWorldController : ControllerBase
{
    private readonly IHelloWorldService _helloWorldService;

    public HelloWorldController(IHelloWorldService helloWorldService)
    {
        _helloWorldService = helloWorldService;
    }

    /// <summary>
    /// Gets a basic greeting message from the HelloWorld module
    /// AUTO-ROUTE: GET /api/hello-world/greeting
    /// </summary>
    /// <param name="context">Optional context for the greeting</param>
    /// <returns>Greeting wrapped in ApiResponse</returns>
    [HttpGet("greeting")]
    [ProducesResponseType(typeof(ApiResponse<Greeting>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<Greeting>>> GetGreeting([FromQuery] string? context = null)
    {
        var greeting = await _helloWorldService.GetGreetingAsync(context);
        return Ok(ApiResponse<Greeting>.Ok(greeting, "Greeting retrieved successfully"));
    }

    /// <summary>
    /// Creates a personalized greeting for a specific user
    /// AUTO-ROUTE: POST /api/hello-world/greet/{userName}
    /// </summary>
    /// <param name="userName">Name of the user to greet</param>
    /// <param name="context">Optional context for personalization</param>
    /// <returns>Personalized user interaction</returns>
    [HttpPost("greet/{userName}")]
    [ProducesResponseType(typeof(ApiResponse<UserInteraction>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<UserInteraction>>> CreatePersonalizedGreeting(
        string userName, 
        [FromQuery] string? context = null)
    {
        var interaction = await _helloWorldService.CreatePersonalizedGreetingAsync(userName, context);
        return Ok(ApiResponse<UserInteraction>.Ok(interaction, 
            $"Personalized greeting created for {userName}"));
    }

    /// <summary>
    /// Gets all available greetings
    /// AUTO-ROUTE: GET /api/hello-world/greetings
    /// </summary>
    /// <returns>List of all active greetings</returns>
    [HttpGet("greetings")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Greeting>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Greeting>>>> GetAllGreetings()
    {
        var greetings = await _helloWorldService.GetAllGreetingsAsync();
        return Ok(ApiResponse<IEnumerable<Greeting>>.Ok(greetings, 
            $"Retrieved {greetings.Count()} active greetings"));
    }

    /// <summary>
    /// Gets module statistics and performance metrics
    /// AUTO-ROUTE: GET /api/hello-world/statistics
    /// </summary>
    /// <returns>Module statistics</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<ModuleStatistics>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<ModuleStatistics>>> GetModuleStatistics()
    {
        var statistics = await _helloWorldService.GetModuleStatisticsAsync();
        return Ok(ApiResponse<ModuleStatistics>.Ok(statistics, 
            "Module statistics calculated successfully"));
    }

    /// <summary>
    /// Gets module manifest information
    /// AUTO-ROUTE: GET /api/hello-world/manifest
    /// </summary>
    /// <returns>Module manifest</returns>
    [HttpGet("manifest")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<object>>> GetModuleManifest()
    {
        var manifest = await _helloWorldService.GetModuleManifestAsync();
        var manifestData = new
        {
            manifest.Name,
            manifest.Version,
            manifest.Description,
            manifest.Author,
            manifest.Dependencies
        };

        return Ok(ApiResponse<object>.Ok(manifestData, 
            "Module manifest retrieved successfully"));
    }

    /// <summary>
    /// Validates MicFx framework integration health
    /// AUTO-ROUTE: GET /api/hello-world/health
    /// </summary>
    /// <returns>Framework integration validation results</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, object>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> ValidateFrameworkIntegration()
    {
        var validationResults = await _helloWorldService.ValidateFrameworkIntegrationAsync();
        return Ok(ApiResponse<Dictionary<string, object>>.Ok(validationResults, 
            "Framework integration validation completed"));
    }

    /// <summary>
    /// Gets comprehensive module information
    /// AUTO-ROUTE: GET /api/hello-world/info
    /// </summary>
    /// <returns>Combined module information</returns>
    [HttpGet("info")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<object>>> GetModuleInfo()
    {
        var manifest = await _helloWorldService.GetModuleManifestAsync();
        var statistics = await _helloWorldService.GetModuleStatisticsAsync();
        var health = await _helloWorldService.ValidateFrameworkIntegrationAsync();

        var moduleInfo = new
        {
            Module = new
            {
                manifest.Name,
                manifest.Version,
                manifest.Description,
                manifest.Author
            },
            Statistics = statistics,
            Health = health,
            Endpoints = new
            {
                Api = new[]
                {
                    "GET /api/hello-world/greeting - Get basic greeting",
                    "POST /api/hello-world/greet/{userName} - Create personalized greeting", 
                    "GET /api/hello-world/greetings - Get all greetings",
                    "GET /api/hello-world/statistics - Get module statistics",
                    "GET /api/hello-world/manifest - Get module manifest",
                    "GET /api/hello-world/health - Validate framework integration",
                    "GET /api/hello-world/info - Get comprehensive module info"
                },
                Mvc = new[]
                {
                    "GET /hello-world - HelloWorld home page",
                    "GET /hello-world/demo - Interactive demo page", 
                    "GET /hello-world/about - About the module"
                },
                Admin = new[]
                {
                    "GET /admin/hello-world - Admin dashboard",
                    "GET /admin/hello-world/settings - Module settings",
                    "GET /admin/hello-world/logs - System logs"
                }
            }
        };

        return Ok(ApiResponse<object>.Ok(moduleInfo, 
            "Complete module information retrieved successfully"));
    }
} 