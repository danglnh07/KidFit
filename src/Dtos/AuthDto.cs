using FluentValidation;

namespace KidFit.Dtos
{
    /*
     * Since we relied on UserManager completely, we don't need a fully validator here
     */

    public class LoginRequestDto
    {
        public string? Username { get; set; } = "";
        public string? Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Username) || !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Username or email is required.");
            // No need to validate password => UserManager will handle that
        }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = "";
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTimeOffset ExpiresAt { get; set; }
    }

    public class ResetPasswordRequestDto
    {
        public string Id { get; set; } = "";
        public string Token { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }


}
