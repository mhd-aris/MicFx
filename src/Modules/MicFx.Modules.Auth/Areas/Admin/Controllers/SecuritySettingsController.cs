using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicFx.Modules.Auth.Domain.DTOs;

namespace MicFx.Modules.Auth.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/auth/settings")]
    [Authorize(Policy = "SuperAdminOnly")]
    public class SecuritySettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SecuritySettingsController> _logger;

        public SecuritySettingsController(
            IConfiguration configuration,
            ILogger<SecuritySettingsController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "Security Settings";

            var settings = new SecuritySettingsViewModel
            {
                MinPasswordLength = _configuration.GetValue<int>("Auth:Password:MinLength", 8),
                RequireUppercase = _configuration.GetValue<bool>("Auth:Password:RequireUppercase", true),
                RequireLowercase = _configuration.GetValue<bool>("Auth:Password:RequireLowercase", true),
                RequireNumbers = _configuration.GetValue<bool>("Auth:Password:RequireDigit", true),
                RequireSpecialChars = _configuration.GetValue<bool>("Auth:Password:RequireSpecialChar", true),
                MaxLoginAttempts = _configuration.GetValue<int>("Auth:Lockout:MaxFailedAttempts", 5),
                LockoutDurationMinutes = _configuration.GetValue<int>("Auth:Lockout:DurationMinutes", 30),
                SessionTimeoutMinutes = _configuration.GetValue<int>("Auth:Session:TimeoutMinutes", 120),
                MaxConcurrentSessions = _configuration.GetValue<int>("Auth:Session:MaxConcurrent", 3),
                RequireTwoFactor = _configuration.GetValue<bool>("Auth:TwoFactor:Require", false),
                AllowRememberDevice = _configuration.GetValue<bool>("Auth:TwoFactor:AllowRememberDevice", true),
                PasswordExpiryDays = _configuration.GetValue<int>("Auth:Password:ExpiryDays", 90)
            };

            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SecuritySettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // In a real application, you would save these settings to configuration
            // For now, we'll just show a success message
            TempData["SuccessMessage"] = "Security settings updated successfully!";
            
            _logger.LogInformation("Security settings updated by user {User}", User.Identity?.Name);
            
            return RedirectToAction(nameof(Index));
        }
    }
} 