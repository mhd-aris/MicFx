using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MicFx.Modules.Auth.Domain.DTOs;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Services;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MicFx.Modules.Auth.Controllers
{
    /// <summary>
    /// Controller untuk authentication - login, register, logout
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, SignInManager<User> signInManager, ILogger<AuthController> logger)
        {
            _authService = authService;
            _signInManager = signInManager;
            _logger = logger;
        }

        // GET: /auth/login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Login";

            // Jika user sudah login, redirect ke return URL atau home
            if (_signInManager.IsSignedIn(User))
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            return View();
        }

        // POST: /auth/login
        [HttpPost]
        // [ValidateAntiForgeryToken] // Temporarily disabled for debugging
        public async Task<IActionResult> Login(LoginRequest? model, string? returnUrl = null)
        {
            _logger.LogDebug("Processing login request for {Email}", model?.Email ?? "unknown");

            if (model == null)
            {
                _logger.LogWarning("Login attempt with null model, creating new model");
                model = new LoginRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login form validation failed for {Email}", model.Email);
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            _logger.LogDebug("Calling AuthService.LoginAsync for {Email}", model.Email);
            var result = await _authService.LoginAsync(model);

            _logger.LogInformation("Login result for {Email}: {IsSuccess}", model.Email, result.IsSuccess);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Login successful for {Email}, redirecting", model.Email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }

                // Use LocalRedirect instead of RedirectToAction to avoid POST method issues
                return LocalRedirect("/");
            }

            _logger.LogWarning("❌ Login failed for {Email}: {Message}", model.Email, result.Message);
            ModelState.AddModelError(string.Empty, result.Message);
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // GET: /auth/register
        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Register";

            // Jika user sudah login, redirect ke home
            if (_signInManager.IsSignedIn(User))
            {
                return LocalRedirect("/");
            }

            return View();
        }

        // POST: /auth/register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            ViewData["Title"] = "Register";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.RegisterAsync(model);

            if (result.IsSuccess)
            {
                // Auto login setelah register
                var loginResult = await _authService.LoginAsync(new LoginRequest
                {
                    Email = model.Email,
                    Password = model.Password,
                    RememberMe = false
                });

                if (loginResult.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Registration successful! Welcome to MicFx.";
                    return LocalRedirect("/");
                }

                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction(nameof(Login));
            }

            // Add errors to ModelState
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        // GET & POST: /auth/logout (support both methods)
        [HttpGet]
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.Identity?.Name ?? "Unknown";
            _logger.LogInformation("🚪 User {UserId} logging out", userId);
            
            await _authService.LogoutAsync(userId);

            TempData["InfoMessage"] = "You have been logged out successfully.";
            _logger.LogInformation("✅ User {UserId} logged out successfully", userId);
            
            return LocalRedirect("/");
        }

        // GET: /auth/access-denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Access Denied";
            return View();
        }

        // GET: /auth/profile (untuk authenticated users)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            ViewData["Title"] = "My Profile";

            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var userInfo = await _authService.GetUserInfoAsync(userId);
            if (userInfo == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(userInfo);
        }

        // GET: /auth/change-password
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Change Password";
            return View();
        }

        // POST: /auth/change-password
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
        {
            ViewData["Title"] = "Change Password";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _authService.ChangePasswordAsync(userId, model);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (!string.IsNullOrEmpty(result.Message))
            {
                ModelState.AddModelError(string.Empty, result.Message);
            }

            return View(model);
        }

        // API endpoint untuk checking login status
        [HttpGet]
        public IActionResult Status()
        {
            return Json(new
            {
                isAuthenticated = User.Identity?.IsAuthenticated ?? false,
                userName = User.Identity?.Name,
                roles = User.Claims
                    .Where(c => c.Type == "role")
                    .Select(c => c.Value)
                    .ToArray()
            });
        }
    }
}