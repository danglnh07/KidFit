using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Views
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
