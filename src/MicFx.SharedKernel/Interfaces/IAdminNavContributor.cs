using System.Collections.Generic;

namespace MicFx.SharedKernel.Interfaces;

/// <summary>
/// Interface for modules that want to contribute navigation items to the admin panel
/// </summary>
public interface IAdminNavContributor
{
    /// <summary>
    /// Gets navigation items to be displayed in the admin panel
    /// </summary>
    /// <returns>Collection of admin navigation items</returns>
    IEnumerable<AdminNavItem> GetNavItems();
}

/// <summary>
/// Admin navigation item
/// </summary>
public class AdminNavItem
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Order { get; set; } = 100;
    public string Category { get; set; } = "General";
    public string[] RequiredRoles { get; set; } = new[] { "Admin" };
    public bool IsActive { get; set; } = true;
}