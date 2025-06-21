using Microsoft.AspNetCore.Mvc;
using MicFx.Modules.HelloWorld.Domain;
using MicFx.Modules.HelloWorld.Services;
using MicFx.Modules.HelloWorld.ViewModels;

namespace MicFx.Modules.HelloWorld.Controllers;

/// <summary>
/// HelloWorld MVC Controller - Web pages with Views
/// Uses conventional routing: /helloworld/{action} from framework routing convention
/// Routes handled by: /{module}/{controller}/{action}/{id?}
/// </summary>
public class HelloWorldController : Controller
{
    private readonly IHelloWorldService _helloWorldService;

    public HelloWorldController(IHelloWorldService helloWorldService)
    {
        _helloWorldService = helloWorldService;
    }

    /// <summary>
    /// HelloWorld home page showing module information
    /// ROUTE: GET /helloworld/helloworld (Index action)
    /// </summary>
    /// <returns>Home view with module information</returns>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var greeting = await _helloWorldService.GetGreetingAsync("welcome");
        var statistics = await _helloWorldService.GetModuleStatisticsAsync();
        var manifest = await _helloWorldService.GetModuleManifestAsync();

        var viewModel = new
        {
            Greeting = greeting,
            Statistics = statistics,
            Manifest = manifest,
            Title = "HelloWorld Module - MicFx Framework Demo"
        };

        return View(viewModel);
    }

    /// <summary>
    /// Interactive demo page showing framework capabilities
    /// ROUTE: GET /helloworld/helloworld/demo
    /// </summary>
    /// <returns>Demo view with interactive features</returns>
    [HttpGet]
    public async Task<IActionResult> Demo()
    {
        var greetings = await _helloWorldService.GetAllGreetingsAsync();
        var statistics = await _helloWorldService.GetModuleStatisticsAsync();

        var viewModel = new
        {
            Greetings = greetings,
            Statistics = statistics,
            Title = "HelloWorld Demo - Interactive Framework Features"
        };

        return View(viewModel);
    }

    /// <summary>
    /// About page showing module information
    /// ROUTE: GET /helloworld/helloworld/about
    /// </summary>
    /// <returns>About view with module details</returns>
    [HttpGet]
    public async Task<IActionResult> About()
    {
        var manifest = await _helloWorldService.GetModuleManifestAsync();
        var health = await _helloWorldService.ValidateFrameworkIntegrationAsync();

        var viewModel = new AboutViewModel
        {
            Title = "About HelloWorld Module",
            Manifest = manifest,
            Health = health
        };

        return View(viewModel);
    }

    /// <summary>
    /// Create personalized greeting (MVC form handling)
    /// ROUTE: POST /helloworld/helloworld/creategreeting
    /// </summary>
    /// <param name="userName">User name from form</param>
    /// <returns>Personalized greeting view</returns>
    [HttpPost]
    public async Task<IActionResult> CreateGreeting(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            TempData["Error"] = "Please enter a valid name";
            return RedirectToAction("Demo");
        }

        try
        {
            var interaction = await _helloWorldService.CreatePersonalizedGreetingAsync(userName, "mvc");
            
            var viewModel = new
            {
                Interaction = interaction,
                Title = $"Hello {userName}!"
            };

            return View("Greeting", viewModel);
        }
        catch (Exception)
        {
            TempData["Error"] = "An error occurred while creating your greeting";
            return RedirectToAction("Demo");
        }
    }
}