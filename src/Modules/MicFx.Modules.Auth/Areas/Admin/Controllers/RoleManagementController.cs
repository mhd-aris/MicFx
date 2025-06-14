using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicFx.Modules.Auth.Domain.Entities;
using MicFx.Modules.Auth.Services;
using MicFx.Modules.Auth.Domain.DTOs;
using MicFx.Modules.Auth.Data;

namespace MicFx.Modules.Auth.Areas.Admin.Controllers
{
    /// <summary>
    /// Admin controller untuk mengelola roles
    /// Hanya dapat diakses oleh Admin dan SuperAdmin
    /// </summary>
    [Area("Admin")]
    [Route("admin/auth/roles")]
    [Authorize(Policy = AuthorizationPolicyService.AdminAreaPolicy)]
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly AuthDbContext _context;
        private readonly ILogger<RoleManagementController> _logger;

        public RoleManagementController(
            RoleManager<Role> roleManager,
            UserManager<User> userManager,
            AuthDbContext context,
            ILogger<RoleManagementController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        // GET: /admin/auth/roles
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Role Management";

            var roles = await _roleManager.Roles
                .OrderBy(r => r.Priority)
                .ThenBy(r => r.Name)
                .ToListAsync();

            var roleViewModels = new List<RoleViewModel>();
            foreach (var role in roles)
            {
                var userCount = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
                roleViewModels.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name ?? "",
                    Description = role.Description ?? "",
                    IsSystemRole = role.IsSystemRole,
                    Priority = role.Priority,
                    UserCount = userCount.Count
                });
            }

            return View(roleViewModels);
        }

        // GET: /admin/auth/roles/details/{id}
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id && rp.IsActive)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync();

            var viewModel = new RoleDetailsViewModel
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = role.Description ?? "",
                IsSystemRole = role.IsSystemRole,
                Priority = role.Priority,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt,
                UserCount = usersInRole.Count,
                Users = usersInRole.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FirstName = u.FirstName ?? "",
                    LastName = u.LastName ?? "",
                    FullName = $"{u.FirstName ?? ""} {u.LastName ?? ""}".Trim(),
                    IsActive = u.IsActive
                }).ToList(),
                Permissions = permissions.Select(p => new PermissionViewModel
                {
                    Id = p.Id.ToString(),
                    Name = p.Name ?? "",
                    DisplayName = p.DisplayName ?? "",
                    Description = p.Description ?? "",
                    Module = p.Module ?? "",
                    Category = p.Category ?? ""
                }).ToList()
            };

            ViewData["Title"] = $"Role Details - {role.Name}";
            return View(viewModel);
        }

        // GET: /admin/auth/roles/create
        [HttpGet("create")]
        [Authorize(Policy = AuthorizationPolicyService.SuperAdminPolicy)]
        public async Task<IActionResult> Create()
        {
            var permissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Category)
                .ThenBy(p => p.DisplayName)
                .ToListAsync();

            var viewModel = new EditRoleViewModel
            {
                IsActive = true,
                Priority = 10,
                AvailablePermissions = permissions.Select(p => new PermissionViewModel
                {
                    Id = p.Id.ToString(),
                    Name = p.Name ?? "",
                    DisplayName = p.DisplayName ?? "",
                    Description = p.Description ?? "",
                    Module = p.Module ?? "",
                    Category = p.Category ?? "",
                    IsSystemPermission = p.IsSystemPermission
                }).ToList()
            };

            ViewData["Title"] = "Create Role";
            return View(viewModel);
        }

        // POST: /admin/auth/roles/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthorizationPolicyService.SuperAdminPolicy)]
        public async Task<IActionResult> Create(EditRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload permissions
                var permissions = await _context.Permissions
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.Category)
                    .ThenBy(p => p.DisplayName)
                    .ToListAsync();

                model.AvailablePermissions = permissions.Select(p => new PermissionViewModel
                {
                    Id = p.Id.ToString(),
                    Name = p.Name ?? "",
                    DisplayName = p.DisplayName ?? "",
                    Description = p.Description ?? "",
                    Module = p.Module ?? "",
                    Category = p.Category ?? "",
                    IsSystemPermission = p.IsSystemPermission,
                    IsSelected = model.SelectedPermissions.Contains(p.Id.ToString())
                }).ToList();

                return View(model);
            }

            // Check if role already exists
            if (await _roleManager.RoleExistsAsync(model.Name))
            {
                ModelState.AddModelError("Name", "Role with this name already exists.");
                return View(model);
            }

            var role = new Role(model.Name ?? "", model.Description ?? "", false)
            {
                Priority = model.Priority,
                IsActive = model.IsActive,
                CreatedBy = User.Identity?.Name ?? "System"
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Add permissions to role
            if (model.SelectedPermissions.Any())
            {
                var permissionIds = model.SelectedPermissions.Select(int.Parse).ToList();
                var permissions = await _context.Permissions
                    .Where(p => permissionIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var permission in permissions)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permission.Id,
                        GrantedBy = User.Identity?.Name ?? "System",
                        IsActive = true
                    };
                    _context.RolePermissions.Add(rolePermission);
                }

                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Role created successfully!";
            return RedirectToAction(nameof(Details), new { id = role.Id });
        }

        // GET: /admin/auth/roles/edit/{id}
        [HttpGet("edit/{id}")]
        [Authorize(Policy = AuthorizationPolicyService.SuperAdminPolicy)]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent editing system roles by non-SuperAdmin
            if (role.IsSystemRole && !User.IsInRole("SuperAdmin"))
            {
                TempData["ErrorMessage"] = "You cannot edit system roles.";
                return RedirectToAction(nameof(Index));
            }

            var rolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id && rp.IsActive)
                .Select(rp => rp.PermissionId.ToString())
                .ToListAsync();

            var allPermissions = await _context.Permissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Category)
                .ThenBy(p => p.DisplayName)
                .ToListAsync();

            var viewModel = new EditRoleViewModel
            {
                Id = role.Id,
                Name = role.Name ?? "",
                Description = role.Description ?? "",
                Priority = role.Priority,
                IsActive = role.IsActive,
                IsSystemRole = role.IsSystemRole,
                SelectedPermissions = rolePermissions,
                AvailablePermissions = allPermissions.Select(p => new PermissionViewModel
                {
                    Id = p.Id.ToString(),
                    Name = p.Name ?? "",
                    DisplayName = p.DisplayName ?? "",
                    Description = p.Description ?? "",
                    Module = p.Module ?? "",
                    Category = p.Category ?? "",
                    IsSystemPermission = p.IsSystemPermission,
                    IsSelected = rolePermissions.Contains(p.Id.ToString())
                }).ToList()
            };

            ViewData["Title"] = $"Edit Role - {role.Name}";
            return View(viewModel);
        }

        // POST: /admin/auth/roles/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthorizationPolicyService.SuperAdminPolicy)]
        public async Task<IActionResult> Edit(string id, EditRoleViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent editing system roles by non-SuperAdmin
            if (role.IsSystemRole && !User.IsInRole("SuperAdmin"))
            {
                TempData["ErrorMessage"] = "You cannot edit system roles.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                // Reload permissions
                var allPermissions = await _context.Permissions
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Module)
                    .ThenBy(p => p.Category)
                    .ThenBy(p => p.DisplayName)
                    .ToListAsync();

                model.AvailablePermissions = allPermissions.Select(p => new PermissionViewModel
                {
                    Id = p.Id.ToString(),
                    Name = p.Name ?? "",
                    DisplayName = p.DisplayName ?? "",
                    Description = p.Description ?? "",
                    Module = p.Module ?? "",
                    Category = p.Category ?? "",
                    IsSystemPermission = p.IsSystemPermission,
                    IsSelected = model.SelectedPermissions.Contains(p.Id.ToString())
                }).ToList();

                return View(model);
            }

            // Update role properties
            role.Description = model.Description ?? "";
            role.Priority = model.Priority;
            role.IsActive = model.IsActive;
            role.UpdatedAt = DateTime.UtcNow;
            role.UpdatedBy = User.Identity?.Name ?? "System";

            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            // Update role permissions
            var currentPermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .ToListAsync();

            // Remove old permissions
            _context.RolePermissions.RemoveRange(currentPermissions);

            // Add new permissions
            if (model.SelectedPermissions.Any())
            {
                var permissionIds = model.SelectedPermissions.Select(int.Parse).ToList();
                foreach (var permissionId in permissionIds)
                {
                    var rolePermission = new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissionId,
                        GrantedBy = User.Identity?.Name ?? "System",
                        IsActive = true
                    };
                    _context.RolePermissions.Add(rolePermission);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Role updated successfully!";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
} 