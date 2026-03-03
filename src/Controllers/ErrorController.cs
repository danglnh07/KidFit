using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public async Task<IActionResult> NotFoundPage()
        {
            Response.StatusCode = 404;
            return View();
        }

        [Route("Error/500")]
        public async Task<IActionResult> InternalServerErrorPage()
        {
            Response.StatusCode = 500;
            return View();
        }

        public async Task<IActionResult> Error()
        {
            return StatusCode(500);
        }
    }
}
