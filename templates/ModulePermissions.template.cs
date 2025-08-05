using MicFx.Core.Permissions;

namespace {{ModuleName}}.Permissions;

/// <summary>
/// Central definition of all {{ModuleName}} module permissions
/// Auto-discovered by MICFX permission system
/// </summary>
[PermissionModule("{{modulename}}")]
public static class {{ModuleName}}Permissions
{
    // 👀 VIEW PERMISSIONS
    [Permission("View {{EntityName}}s", "Can view {{entityname}} list and details", Category = "{{EntityName}}s")]
    public const string VIEW_{{ENTITY_NAME}}S = "{{entityname}}s.view";

    // ✏️ CRUD PERMISSIONS
    [Permission("Create {{EntityName}}s", "Can create new {{entityname}}s", Category = "{{EntityName}}s")]
    public const string CREATE_{{ENTITY_NAME}}S = "{{entityname}}s.create";

    [Permission("Edit {{EntityName}}s", "Can modify existing {{entityname}}s", Category = "{{EntityName}}s")]
    public const string EDIT_{{ENTITY_NAME}}S = "{{entityname}}s.edit";

    [Permission("Delete {{EntityName}}s", "Can delete {{entityname}}s", Category = "{{EntityName}}s")]
    public const string DELETE_{{ENTITY_NAME}}S = "{{entityname}}s.delete";

    // ⚙️ MANAGEMENT PERMISSIONS
    [Permission("Manage {{ModuleName}} Settings", "Can configure {{modulename}} module settings", Category = "Configuration")]
    public const string MANAGE_SETTINGS = "settings.manage";

    [Permission("Export {{ModuleName}} Data", "Can export {{modulename}} data in various formats", Category = "Reports")]
    public const string EXPORT_DATA = "reports.export";

    // 🔄 BULK OPERATIONS (optional)
    [Permission("Bulk {{EntityName}} Operations", "Can perform bulk operations on {{entityname}}s", Category = "Bulk")]
    public const string BULK_OPERATIONS = "{{entityname}}s.bulk";

    // 📊 ADVANCED PERMISSIONS (add as needed)
    [Permission("View {{ModuleName}} Reports", "Can view {{modulename}} reports and analytics", Category = "Reports")]
    public const string VIEW_REPORTS = "reports.view";

    [Permission("Manage {{ModuleName}} Categories", "Can manage {{entityname}} categories", Category = "Categories")]
    public const string MANAGE_CATEGORIES = "categories.manage";
}

/// <summary>
/// Pre-defined permission groups for easy role assignment
/// Use these in your seeder for consistent role setup
/// </summary>
public static class {{ModuleName}}PermissionGroups
{
    /// <summary>
    /// Basic CRUD permissions (view, create, edit)
    /// Suitable for regular users/editors
    /// </summary>
    public static string[] BasicCrud => new[]
    {
        {{ModuleName}}Permissions.VIEW_{{ENTITY_NAME}}S,
        {{ModuleName}}Permissions.CREATE_{{ENTITY_NAME}}S,
        {{ModuleName}}Permissions.EDIT_{{ENTITY_NAME}}S
    };

    /// <summary>
    /// Full CRUD permissions including delete
    /// Suitable for administrators
    /// </summary>
    public static string[] FullCrud => new[]
    {
        {{ModuleName}}Permissions.VIEW_{{ENTITY_NAME}}S,
        {{ModuleName}}Permissions.CREATE_{{ENTITY_NAME}}S,
        {{ModuleName}}Permissions.EDIT_{{ENTITY_NAME}}S,
        {{ModuleName}}Permissions.DELETE_{{ENTITY_NAME}}S
    };

    /// <summary>
    /// Management permissions for advanced operations
    /// Suitable for module administrators
    /// </summary>
    public static string[] Management => new[]
    {
        {{ModuleName}}Permissions.MANAGE_SETTINGS,
        {{ModuleName}}Permissions.BULK_OPERATIONS,
        {{ModuleName}}Permissions.MANAGE_CATEGORIES
    };

    /// <summary>
    /// Reporting permissions
    /// Suitable for analysts and managers
    /// </summary>
    public static string[] Reporting => new[]
    {
        {{ModuleName}}Permissions.VIEW_REPORTS,
        {{ModuleName}}Permissions.EXPORT_DATA
    };

    /// <summary>
    /// Read-only access
    /// Suitable for viewers and auditors
    /// </summary>
    public static string[] ReadOnly => new[]
    {
        {{ModuleName}}Permissions.VIEW_{{ENTITY_NAME}}S,
        {{ModuleName}}Permissions.VIEW_REPORTS
    };

    /// <summary>
    /// Full access to everything in this module
    /// Suitable for SuperAdmin
    /// </summary>
    public static string[] FullAccess => BasicCrud
        .Concat(new[] { {{ModuleName}}Permissions.DELETE_{{ENTITY_NAME}}S })
        .Concat(Management)
        .Concat(Reporting)
        .ToArray();
}

/// <summary>
/// Example usage and documentation for developers
/// </summary>
public static class {{ModuleName}}PermissionExamples
{
    /// <summary>
    /// Example: How to use permissions in controllers
    /// </summary>
    public static void ControllerExamples()
    {
        /*
        [Area("Admin")]
        [Route("admin/{{modulename}}/{{entityname}}s")]
        public class {{EntityName}}Controller : Controller
        {
            [RequirePermission({{ModuleName}}Permissions.VIEW_{{ENTITY_NAME}}S)]
            public async Task<IActionResult> Index() { }

            [RequirePermission({{ModuleName}}Permissions.CREATE_{{ENTITY_NAME}}S)]
            public async Task<IActionResult> Create() { }

            [RequireAnyPermission({{ModuleName}}Permissions.EDIT_{{ENTITY_NAME}}S, {{ModuleName}}Permissions.DELETE_{{ENTITY_NAME}}S)]
            public async Task<IActionResult> Manage(int id) { }

            [RequireRole("SuperAdmin")]
            public async Task<IActionResult> SystemDiagnostics() { }
        }
        */
    }

    /// <summary>
    /// Example: How to use permissions in views
    /// </summary>
    public static void ViewExamples()
    {
        /*
        <!-- Create button -->
        <div requires-permission="@{{ModuleName}}Permissions.CREATE_{{ENTITY_NAME}}S">
            <a href="/admin/{{modulename}}/{{entityname}}s/create" class="btn btn-primary">
                <i class="fas fa-plus"></i> Add {{EntityName}}
            </a>
        </div>

        <!-- Edit/Delete actions -->
        <div requires-any-permission="@{{ModuleName}}Permissions.EDIT_{{ENTITY_NAME}}S,@{{ModuleName}}Permissions.DELETE_{{ENTITY_NAME}}S">
            <div class="dropdown">
                <button class="btn btn-sm btn-secondary dropdown-toggle">Actions</button>
                <div class="dropdown-menu">
                    <a requires-permission="@{{ModuleName}}Permissions.EDIT_{{ENTITY_NAME}}S" 
                       href="/admin/{{modulename}}/{{entityname}}s/edit/@item.Id">Edit</a>
                    <a requires-permission="@{{ModuleName}}Permissions.DELETE_{{ENTITY_NAME}}S" 
                       href="/admin/{{modulename}}/{{entityname}}s/delete/@item.Id">Delete</a>
                </div>
            </div>
        </div>

        <!-- Settings (admin only) -->
        <div requires-role="Admin">
            <a href="/admin/{{modulename}}/settings">Module Settings</a>
        </div>
        */
    }

    /// <summary>
    /// Example: How to configure seeder
    /// </summary>
    public static void SeederExamples()
    {
        /*
        public class {{ModuleName}}Seeder : IModuleSeeder
        {
            public async Task SeedAsync(IServiceProvider serviceProvider)
            {
                await serviceProvider.SeedPermissions()
                    .FromModule<{{ModuleName}}Permissions>()
                    .AssignToRole("SuperAdmin", role => role
                        .Include({{ModuleName}}PermissionGroups.FullAccess)
                    )
                    .AssignToRole("Admin", role => role
                        .Include({{ModuleName}}PermissionGroups.FullCrud)
                        .Include({{ModuleName}}PermissionGroups.Management)
                    )
                    .AssignToRole("Editor", role => role
                        .Include({{ModuleName}}PermissionGroups.BasicCrud)
                    )
                    .AssignToRole("Viewer", role => role
                        .Include({{ModuleName}}PermissionGroups.ReadOnly)
                    )
                    .ExecuteAsync();
            }
        }
        */
    }
}
