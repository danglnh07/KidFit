using KidFit.Dtos;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Apis
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(AuthService authService, ILogger<AuthController> logger) : ControllerBase
    {
        private readonly AuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            throw new NotImplementedException();
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword()
        {
            throw new NotImplementedException();
        }
    }
}
