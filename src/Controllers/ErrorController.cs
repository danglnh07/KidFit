using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers
{
    public class ErrorController : Controller
    {
        public async Task<IActionResult> Error()
        {
            return View();
        }
    }
}
