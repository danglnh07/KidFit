using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
