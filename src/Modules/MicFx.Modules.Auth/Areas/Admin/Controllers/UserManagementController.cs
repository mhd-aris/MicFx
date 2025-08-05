using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Services;
using MicFx.Modules.Auth.Domain.DTOs;
using MicFx.Modules.Auth.Authorization;

namespace MicFx.Modules.Auth.Areas.Admin.Controllers
{
    /// <summary>
    /// Enhanced admin controller for managing users with permission-based authorization
    /// Uses permission system with wildcard support
    /// </summary>
    [Area("Admin")]
    [Route("admin/auth/users")]
    [Permission("users.view")] // Auto-detected as "auth.users.view"
    public class UserManagementController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: /admin/auth/users
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20, string search = "")
        {
            ViewData["Title"] = "User Management";

            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => 
                    (u.Email != null && u.Email.Contains(search)) || 
                    (u.FirstName != null && u.FirstName.Contains(search)) || 
                    (u.LastName != null && u.LastName.Contains(search)));
            }

            var totalUsers = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Get roles for each user
            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FirstName = user.FirstName ?? "",
                    LastName = user.LastName ?? "",
                    FullName = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = roles.ToList()
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
            ViewBag.Search = search;

            return View(userViewModels);
        }

        // GET: /admin/auth/users/create
        [HttpGet("create")]
        [Permission("users.create")] // Requires create permission
        public async Task<IActionResult> Create()
        {
            var allRoles = await _roleManager.Roles.ToListAsync();

            var viewModel = new CreateUserViewModel
            {
                AvailableRoles = allRoles.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Description = r.Description ?? "",
                    IsSelected = false
                }).ToList()
            };

            ViewData["Title"] = "Create New User";
            return View(viewModel);
        }

        // POST: /admin/auth/users/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        [Permission("users.create")] // Requires create permission
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload available roles
                var allRoles = await _roleManager.Roles.ToListAsync();
                model.AvailableRoles = allRoles.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Description = r.Description ?? "",
                    IsSelected = model.SelectedRoles.Contains(r.Name ?? "")
                }).ToList();

                return View(model);
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Add selected roles
                if (model.SelectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                _logger.LogInformation($"✅ Created new user {user.Email} with {model.SelectedRoles.Count} roles");
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // Reload available roles on error
            var allRolesOnError = await _roleManager.Roles.ToListAsync();
            model.AvailableRoles = allRolesOnError.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name ?? "",
                Description = r.Description ?? "",
                IsSelected = model.SelectedRoles.Contains(r.Name ?? "")
            }).ToList();

            return View(model);
        }

        // GET: /admin/auth/users/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            var viewModel = new UserDetailsViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList(),
                Claims = claims.Select(c => new { c.Type, c.Value }).Cast<object>().ToList()
            };

            ViewData["Title"] = $"User Details - {user.Email}";
            return View(viewModel);
        }

        // GET: /admin/auth/users/edit/{id}
        [HttpGet("edit/{id}")]
        [Permission("users.edit")] // Requires specific edit permission
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                IsActive = user.IsActive,
                SelectedRoles = userRoles.ToList(),
                AvailableRoles = allRoles.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Description = r.Description ?? "",
                    IsSelected = userRoles.Contains(r.Name ?? "")
                }).ToList()
            };

            ViewData["Title"] = $"Edit User - {user.Email}";
            return View(viewModel);
        }

        // POST: /admin/auth/users/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        [Permission("users.edit")] // Requires edit permission for POST
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                // Reload available roles
                var allRoles = await _roleManager.Roles.ToListAsync();
                model.AvailableRoles = allRoles.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name ?? "",
                    Description = r.Description ?? "",
                    IsSelected = model.SelectedRoles.Contains(r.Name ?? "")
                }).ToList();

                return View(model);
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = User.Identity?.Name ?? "System";

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Update user roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();
            var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            }

            if (rolesToAdd.Any())
            {
                await _userManager.AddToRolesAsync(user, rolesToAdd);
            }

            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /admin/auth/users/toggle-status/{id}
        [HttpPost("toggle-status/{id}")]
        [ValidateAntiForgeryToken]
        [AnyPermission("users.edit")] // Requires either activate or edit permission
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedBy = User.Identity?.Name ?? "System";

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {(user.IsActive ? "activated" : "deactivated")} successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update user status.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 