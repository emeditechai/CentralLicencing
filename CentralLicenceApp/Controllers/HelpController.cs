using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CentralLicenceApp.Controllers
{
    [Authorize]
    public class HelpController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Help & Guide";
            return View();
        }
    }
}
