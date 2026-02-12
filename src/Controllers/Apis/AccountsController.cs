using System.Security.Claims;
using KidFit.Dtos;
using KidFit.Services;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidFit.Controllers.Apis
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController(AccountService accountService,
                                    ILogger<AccountsController> logger) : ControllerBase
    {
        private readonly AccountService _accountService = accountService;
        private readonly ILogger<AccountsController> _logger = logger;

        [HttpPost("create")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateAccountDto request)
        {
            try
            {
                await _accountService.CreateAccountAsync(request);
                return Ok(new { success = true, message = "Account created successfully" });
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Create account failed: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create account error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during account creation" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return NotFound(new { message = "Account not found" });
                var account = await _accountService.GetAccountById(userId);
                if (account == null) return NotFound(new { message = "Account not found" });
                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting account: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AdminOrSelf")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var account = await _accountService.GetAccountById(id);
                if (account == null)
                {
                    return NotFound(new { message = $"Account {id} not found" });
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting account {id}: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateAccountDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return NotFound(new { message = "Account not found" });
                var account = await _accountService.UpdateAccount(userId, request);
                return Ok(new { success = true, message = "Account updated successfully", account });
            }
            catch (IdentityException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating account: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrSelf")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateAccountDto request)
        {
            try
            {
                var account = await _accountService.UpdateAccount(id, request);
                return Ok(new { success = true, message = "Account updated successfully", account });
            }
            catch (IdentityException ex)
            {
                _logger.LogWarning($"Update account failed: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update account error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during account update" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrSelf")]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                await _accountService.DeactivateAccount(id);
                return Ok(new { success = true, message = "Account deactivated successfully" });
            }
            catch (IdentityException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Deactivate account error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred during account deactivation" });
            }
        }
    }
}
