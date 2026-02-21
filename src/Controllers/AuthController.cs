using KidFit.Services;
using KidFit.Shared.Exceptions;
using KidFit.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace KidFit.Controllers
{
    public class AuthController(AuthService authService, ILogger<AuthController> logger) : Controller
    {
        private readonly AuthService _authService = authService;
        private readonly ILogger<AuthController> _logger = logger;

        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestViewModel req)
        {
            try
            {
                // Check validation
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Invalid login credentials";
                    return View();
                }

                // Login
                await _authService.LoginWithCookieAsync(null, req.Username, req.Password);
                return RedirectToAction("Dashboard", "Home");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Login failed: {ex.Message}");
                TempData["Message"] = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                TempData["Message"] = "An error occurred";
                return View();
            }
        }

        public async Task<IActionResult> ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestViewModel req)
        {
            try
            {
                // Check validation
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Invalid reset password credentials";
                    return View();
                }


                // Decode token
                req.Token = Base64UrlEncoder.Decode(req.Token);

                // Reset password
                await _authService.ResetPasswordAsync(req.Id, req.Token, req.NewPassword);
                return Redirect("/auth/login");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Reset password failed: {ex.Message}");
                TempData["Message"] = ex.Message;
                return View(req);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: {ex.Message}");
                TempData["Message"] = "An error occurred";
                return View(req);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.Logout();
            _logger.LogInformation("User logged out");
            return Redirect("/auth/login");
        }
    }
}
