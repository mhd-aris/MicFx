using Microsoft.AspNetCore.Mvc;

namespace MicFx.Mvc.Web.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}