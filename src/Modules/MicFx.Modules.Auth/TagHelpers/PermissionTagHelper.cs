using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using MicFx.Modules.Auth.Services;
using System.Security.Claims;

namespace MicFx.Modules.Auth.TagHelpers
{
    /// <summary>
    /// TagHelper untuk hide/show elements berdasarkan permissions
    /// Usage: <div requires-permission="users.edit">Content</div>
    /// </summary>
    [HtmlTargetElement(Attributes = "requires-permission")]
    public class PermissionTagHelper : TagHelper
    {
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionTagHelper(IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
        {
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Permission yang diperlukan untuk menampilkan element
        /// </summary>
        [HtmlAttributeName("requires-permission")]
        public string RequiresPermission { get; set; } = string.Empty;

        /// <summary>
        /// Jika true, hide element jika user TIDAK punya permission
        /// Jika false, hide element jika user PUNYA permission (inverse logic)
        /// </summary>
        [HtmlAttributeName("hide-if-no-permission")]
        public bool HideIfNoPermission { get; set; } = true;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(RequiresPermission))
            {
                return; // No permission check, show element
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                output.SuppressOutput();
                return;
            }

            var hasPermission = await _permissionService.HasPermissionAsync(user, RequiresPermission);

            // Apply logic based on HideIfNoPermission setting
            if ((HideIfNoPermission && !hasPermission) || (!HideIfNoPermission && hasPermission))
            {
                output.SuppressOutput();
            }

            // Remove the custom attributes from final HTML
            output.Attributes.RemoveAll("requires-permission");
            output.Attributes.RemoveAll("hide-if-no-permission");
        }
    }

    /// <summary>
    /// TagHelper untuk multiple permissions (ANY logic)
    /// Usage: <div requires-any-permission="users.edit,users.view">Content</div>
    /// </summary>
    [HtmlTargetElement(Attributes = "requires-any-permission")]
    public class AnyPermissionTagHelper : TagHelper
    {
        private readonly IPermissionService _permissionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnyPermissionTagHelper(IPermissionService permissionService, IHttpContextAccessor httpContextAccessor)
        {
            _permissionService = permissionService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Comma-separated list of permissions (user needs ANY one of them)
        /// </summary>
        [HtmlAttributeName("requires-any-permission")]
        public string RequiresAnyPermission { get; set; } = string.Empty;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(RequiresAnyPermission))
            {
                return; // No permission check, show element
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                output.SuppressOutput();
                return;
            }

            var permissions = RequiresAnyPermission.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                  .Select(p => p.Trim())
                                                  .ToArray();

            var hasAnyPermission = false;
            foreach (var permission in permissions)
            {
                if (await _permissionService.HasPermissionAsync(user, permission))
                {
                    hasAnyPermission = true;
                    break;
                }
            }

            if (!hasAnyPermission)
            {
                output.SuppressOutput();
            }

            // Remove the custom attribute from final HTML
            output.Attributes.RemoveAll("requires-any-permission");
        }
    }

    /// <summary>
    /// TagHelper untuk role-based show/hide
    /// Usage: <div requires-role="Admin">Content</div>
    /// </summary>
    [HtmlTargetElement(Attributes = "requires-role")]
    public class RoleTagHelper : TagHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleTagHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Role yang diperlukan untuk menampilkan element
        /// </summary>
        [HtmlAttributeName("requires-role")]
        public string RequiresRole { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (string.IsNullOrEmpty(RequiresRole))
            {
                return; // No role check, show element
            }

            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
            {
                output.SuppressOutput();
                return;
            }

            if (!user.IsInRole(RequiresRole))
            {
                output.SuppressOutput();
            }

            // Remove the custom attribute from final HTML
            output.Attributes.RemoveAll("requires-role");
        }
    }
} 