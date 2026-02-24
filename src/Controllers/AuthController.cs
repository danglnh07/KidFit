using KidFit.Services;
using KidFit.Shared.Constants;
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

        public async Task<IActionResult> Login(string? returnUrl)
        {
            _logger.LogInformation($"ReturnUrl {returnUrl}");
            var resp = new LoginRequestViewModel()
            {
                Username = "",
                Password = "",
                ReturnUrl = returnUrl
            };
            return View(resp);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestViewModel req)
        {
            try
            {
                // Check validation
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(x => x.Value?.Errors.Count > 0).ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                    foreach (var error in errors)
                    {
                        _logger.LogWarning($"Field: {error.Key}, Error: {string.Join(", ", error.Value)}");
                    }
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid login credentials";
                    return View(req);
                }

                // Login
                await _authService.LoginWithCookieAsync(null, req.Username, req.Password);

                // Redirect based on return URL and role
                if (req.ReturnUrl is not null && Url.IsLocalUrl(req.ReturnUrl)) // Prevent XSS
                {
                    return Redirect(req.ReturnUrl);
                }
                else if (User.IsInRole(Role.ADMIN.ToString()))
                {
                    return RedirectToAction("Dashboard", "Home");
                }
                else
                {
                    // Default case
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Login failed: {ex.Message}");
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return View(req);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return RedirectToAction("Error", "Error");
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
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid reset password credentials";
                    return View();
                }

                // Decode token
                req.Token = Base64UrlEncoder.Decode(req.Token);

                // Reset password
                await _authService.ResetPasswordAsync(req.Id, req.Token, req.NewPassword);
                return RedirectToAction("Login", "Auth");
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Reset password failed: {ex.Message}");
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return View(req);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: {ex.Message}");
                return RedirectToAction("Error", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Login", "Auth");
        }
    }
}
