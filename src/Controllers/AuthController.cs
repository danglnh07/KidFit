using KidFit.Services;
using KidFit.Shared.Constants;
using KidFit.Shared.Exceptions;
using KidFit.Shared.Util;
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
                    TempData[MessageLevel.LOG.ToString()] = Util.GetModelValidationError(ModelState);
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
                    return RedirectToAction(nameof(HomeController.Dashboard), nameof(HomeController).Replace("Controller", string.Empty));
                }
                else
                {
                    // Default case so that compiler won't complain about missing return
                    // All role should have a dedicated "Home" for them
                    return RedirectToAction(nameof(HomeController.Index), nameof(HomeController).Replace("Controller", string.Empty));
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
                _logger.LogError($"Login failed: unexpected error occurs: {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        public async Task<IActionResult> ResetPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestViewModel req)
        {
            try
            {
                // Check validation
                if (!ModelState.IsValid)
                {
                    // We should NOT give a specific error message here
                    TempData[MessageLevel.WARNING.ToString()] = "Invalid reset password credentials";
                    return View(req);
                }

                // Decode token
                req.Token = Base64UrlEncoder.Decode(req.Token);

                // Reset password
                await _authService.ResetPasswordAsync(req.Id, req.Token, req.NewPassword);
                return RedirectToAction(nameof(Login));
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Reset password failed: {ex.Message}");
                TempData[MessageLevel.ERROR.ToString()] = ex.Message;
                return View(req);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: unexpected error occurs {ex.Message}");
                return RedirectToAction(nameof(ErrorController.InternalServerErrorPage), nameof(ErrorController).Replace("Controller", string.Empty));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
