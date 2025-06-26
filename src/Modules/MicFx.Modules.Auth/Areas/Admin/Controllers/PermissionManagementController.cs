using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicFx.Modules.Auth.Data;
using MicFx.Modules.Auth.Authorization;
using MicFx.Modules.Auth.Domain.DTOs;

namespace MicFx.Modules.Auth.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("admin/auth/permissions")]
    [Permission("permissions.view")]
    public class PermissionManagementController : Controller
    {
        private readonly AuthDbContext _context;

        public PermissionManagementController(AuthDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Permission Management";
            
            var permissions = await _context.Permissions
                .Include(p => p.RolePermissions)
                    .ThenInclude(rp => rp.Role)
                .OrderBy(p => p.Name)
                .ToListAsync();

            var permissionViewModels = permissions.Select(p => new PermissionViewModel
            {
                Id = p.Id.ToString(),
                Name = p.Name ?? "",
                DisplayName = p.DisplayName ?? "",
                Description = p.Description ?? "",
                Module = p.Module ?? "",
                Category = p.Category ?? "",
                IsSystemPermission = p.IsSystemPermission,
                CreatedAt = p.CreatedAt,
                Roles = p.RolePermissions
                    .Where(rp => rp.IsActive)
                    .Select(rp => rp.Role?.Name ?? "")
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList()
            }).ToList();

            return View(permissionViewModels);
        }

        [HttpGet("create")]
        [Permission("permissions.create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Create Permission";
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        [Permission("permissions.create")]
        public async Task<IActionResult> Create(string name, string description)
        {
            if (ModelState.IsValid)
            {
                var permission = new Domain.Entities.Permission
                {
                    Name = name,
                    Description = description
                };

                _context.Permissions.Add(permission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Create Permission";
            return View();
        }

        [HttpGet("edit/{id}")]
        [Permission("permissions.edit")]
        public async Task<IActionResult> Edit(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            ViewData["Title"] = "Edit Permission";
            return View(permission);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        [Permission("permissions.edit")]
        public async Task<IActionResult> Edit(int id, Domain.Entities.Permission permission)
        {
            if (id != permission.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _context.Update(permission);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Edit Permission";
            return View(permission);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        [Permission("permissions.delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _context.Permissions.Remove(permission);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
} 