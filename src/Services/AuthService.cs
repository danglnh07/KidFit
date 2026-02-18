using FluentValidation;
using KidFit.Dtos;
using KidFit.Models;
using KidFit.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace KidFit.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager,
                             SignInManager<ApplicationUser> signInManager,
                             IValidator<LoginRequestDto> loginValidator,
                             IConfiguration configuration,
                             ILogger<AuthService> logger)
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly IValidator<LoginRequestDto> _loginValidator = loginValidator;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<AuthService> _logger = logger;

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto req, bool generateToken = false)
        {
            // Validate login request DTO
            var validationResult = _loginValidator.Validate(req);
            if (!validationResult.IsValid)
            {
                var message = "Failed to login: model validation failed";
                List<string> errors = [.. validationResult.Errors.Select(e => e.ErrorMessage)];
                throw Shared.Exceptions.ValidationException.Create(message, errors);
            }

            // Find user by username or email
            ApplicationUser? user = null;
            if (req.Username is not null)
            {
                user = await _userManager.FindByNameAsync(req.Username);
            }
            else if (req.Email is not null)
            {
                user = await _userManager.FindByEmailAsync(req.Email);
            }
            else
            {
                // Though validator already check it, we still add a log just in case
                _logger.LogWarning($"Login failed: Username or email is required");
            }

            // Check if user exist
            if (user is null)
            {
                _logger.LogWarning($"Login failed: User {(req.Username is not null ? req.Username : req.Email)} not found");
                throw IdentityException.Create("Login failed: incorrect login credentials");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning($"Login failed: User {(req.Username is not null ? req.Username : req.Email)} is disabled");
                throw IdentityException.Create("Login failed: account is disabled");
            }

            // Check if password match
            var result = await _signInManager.PasswordSignInAsync(user, req.Password, false, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning($"Login failed: Invalid password for user {(req.Username is not null ? req.Username : req.Email)}");
                throw IdentityException.Create("Login failed: incorrect login credentials");
            }

            if (generateToken) return new();

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var jwtParam = new JwtParams()
            {
                Identifier = user.Id,
                Name = user.FullName,
                Email = user.Email!, // Email is required when create user 
                GivenName = user.UserName!, // Username is required when create user
                Role = roles.FirstOrDefault() ?? "",
                Issuer = GetJwtIssuer(),
                Audience = GetJwtAudience(),
                ExpiresInMinutes = GetTokenExpirationMinutes(),
            };
            var token = SecurityService.GenerateJWTToken(jwtParam, GetJwtSecret());

            // Return login response with token and some basic user information
            return new LoginResponseDto
            {
                Token = token,
                Username = user.UserName!,
                FullName = user.FullName,
                Role = roles.FirstOrDefault() ?? "",
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(GetTokenExpirationMinutes())
            };
        }

        public async Task ResetPasswordAsync(string id, string token, string newPassword)
        {
            // Find user by ID 
            var user = await _userManager.FindByIdAsync(id) ?? throw NotFoundException.Create(typeof(ApplicationUser).Name);

            // Reset password with token
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded || result.Errors.Count() != 0)
            {
                throw IdentityException.Create("Reset password failed", result.Errors);
            }
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
    }
}
