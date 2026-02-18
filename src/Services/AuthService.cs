using KidFit.Models;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace KidFit.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             IConfiguration configuration,
                             ILogger<AuthService> logger)
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthService> _logger = logger;

        private async Task<ApplicationUser> GetAndValidateLoginCredentials(string? email, string? username)
        {
            // Find user by username or email
            ApplicationUser? user;
            if (!string.IsNullOrEmpty(username))
            {
                user = await _userManager.FindByNameAsync(username);
            }
            else if (!string.IsNullOrEmpty(email))
            {
                user = await _userManager.FindByEmailAsync(email);
            }
            else
            {
                _logger.LogWarning($"Login failed: Username or email is required");
                throw new ArgumentException("Either email or username must be provided");
            }

            // Check if user exist
            if (user is null)
            {
                throw IdentityException.Create("Login failed: incorrect login credentials");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                throw IdentityException.Create("Login failed: account is disabled");
            }

            return user;
        }

        private string GetJwtSecret()
        {
            return _configuration["AppSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
        }

        private string GetJwtIssuer()
        {
            return _configuration["AppSettings:Issuer"] ?? "KidFit";
        }

        private string GetJwtAudience()
        {
            return _configuration["AppSettings:Audience"] ?? "KidFit";
        }

        private int GetTokenExpirationMinutes()
        {
            return _configuration.GetValue("AppSettings:ExpirationMinutes", 24);
        }

        public async Task LoginWithCookieAsync(string? email, string? username, string password)
        {
            var user = await GetAndValidateLoginCredentials(email, username);

            // Check if password match
            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded)
            {
                throw IdentityException.Create("Login failed: incorrect login credentials");
            }
        }

        public async Task<string> LoginWithTokenAsync(string? email, string? username, string password)
        {
            var user = await GetAndValidateLoginCredentials(email, username);

            // Check if password match
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                throw IdentityException.Create("Login failed: incorrect login credentials");
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);
            if (roles is null || roles.Count() == 0)
            {
                throw IdentityException.Create("Generate token failed: user has no roles");
            }

            // Create JWT param 
            var param = new JwtParams()
            {
                Identifier = user.Id,
                Name = user.UserName!,
                Email = user.Email!,
                GivenName = user.FullName,
                Role = roles.FirstOrDefault()!,
                Issuer = GetJwtIssuer(),
                Audience = GetJwtAudience(),
                ExpiresInMinutes = GetTokenExpirationMinutes()
            };

            return SecurityService.GenerateJwtToken(param, GetJwtSecret());
        }

        // This method will logout user with Cookie based login method. 
        // For JWT, it should be the UI side to handle that, as we didn't 
        // have blacklist mechanism yet
        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task ResetPasswordAsync(string id, string token, string newPassword)
        {
            // Find user by ID 
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
            {
                throw NotFoundException.Create(typeof(ApplicationUser).Name);
            }
            else if (!user.IsActive)
            {
                throw ForbiddenException.Create("Reset password failed: account is disabled");
            }

            // Reset password with token
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded || result.Errors.Count() != 0)
            {
                throw IdentityException.Create("Reset password failed", result.Errors);
            }
        }
    }
}
