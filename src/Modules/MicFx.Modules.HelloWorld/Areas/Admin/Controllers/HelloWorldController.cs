using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MicFx.Modules.HelloWorld.Services;

namespace MicFx.Modules.HelloWorld.Areas.Admin.Controllers
{
    /// <summary>
    /// Admin controller for HelloWorld module
    /// Demonstrates how modules can contribute to the admin panel
    /// </summary>
    [Area("Admin")]
    [Route("admin/hello-world")]
    // [Authorize(Roles = "Admin")] // Temporarily disabled for testing
    public class HelloWorldController : Controller
    {
        private readonly IHelloWorldService _helloWorldService;

        public HelloWorldController(IHelloWorldService helloWorldService)
        {
            _helloWorldService = helloWorldService;
        }

        /// <summary>
        /// Main HelloWorld admin page
        /// </summary>
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Hello World Admin";
            
            var greeting = await _helloWorldService.GetGreetingAsync("admin");
            var statistics = await _helloWorldService.GetModuleStatisticsAsync();
            
            var model = new HelloWorldAdminViewModel
            {
                Message = greeting.Message,
                ModuleName = "HelloWorld",
                IsActive = true,
                Statistics = new HelloWorldStatistics
                {
                    TotalRequests = statistics.TotalGreetings,
                    ActiveSessions = statistics.TotalInteractions,
                    LastActivity = DateTime.Now
                }
            };

            return View(model);
        }

        /// <summary>
        /// HelloWorld settings page
        /// </summary>
        [HttpGet("settings")]
        public IActionResult Settings()
        {
            ViewData["Title"] = "Hello World Settings";
            
            var model = new HelloWorldSettingsViewModel
            {
                EnableGreeting = true,
                DefaultMessage = "Hello from MicFx!",
                MaxMessageLength = 100,
                AllowCustomMessages = true
            };

            return View(model);
        }

        /// <summary>
        /// Save HelloWorld settings
        /// </summary>
        [HttpPost("settings")]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(HelloWorldSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Here you would save the settings
            // For demo purposes, we'll just show a success message
            TempData["SuccessMessage"] = "Settings saved successfully!";
            
            return RedirectToAction(nameof(Settings));
        }
    }

    /// <summary>
    /// View model for HelloWorld admin page
    /// </summary>
    public class HelloWorldAdminViewModel
    {
        public string Message { get; set; } = string.Empty;
        public string ModuleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public HelloWorldStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// View model for HelloWorld settings
    /// </summary>
    public class HelloWorldSettingsViewModel
    {
        public bool EnableGreeting { get; set; }
        public string DefaultMessage { get; set; } = string.Empty;
        public int MaxMessageLength { get; set; }
        public bool AllowCustomMessages { get; set; }
    }

    /// <summary>
    /// Statistics for HelloWorld module
    /// </summary>
    public class HelloWorldStatistics
    {
        public int TotalRequests { get; set; }
        public int ActiveSessions { get; set; }
        public DateTime LastActivity { get; set; }
    }
} 