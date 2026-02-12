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
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning($"Login failed at validation: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Login failed: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            try
            {
                await _authService.ResetPasswordAsync(request.Id, request.Token, request.NewPassword);
                return Ok(new { success = true, message = "Password reset successfully" });
            }
            catch (IdentityException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reset password error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while resetting password" });
            }
        }
    }
}
