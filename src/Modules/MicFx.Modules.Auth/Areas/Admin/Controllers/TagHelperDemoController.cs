using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicFx.Modules.Auth.Authorization;

namespace MicFx.Modules.Auth.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller untuk demo TagHelpers permission dan role-based visibility
    /// </summary>
    [Area("Admin")]
    [Route("admin/auth/taghelper-demo")]
    [Permission("auth.taghelper.view")]
    public class TagHelperDemoController : Controller
    {
        public TagHelperDemoController()
        {
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Title"] = "TagHelper Demo";
            return View();
        }
    }
} 