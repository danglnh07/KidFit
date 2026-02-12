using KidFit.Shared.Constants;

namespace KidFit.Dtos
{
    /*
     * Since we relied on UserManager completely, we don't need a validator here
     */

    public class CreateAccountDto
    {
        public string Email { get; set; } = "";
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string? AvatarUrl { get; set; } = null;
        public Role Role { get; set; }
    }

    public class ViewAccountDto
    {
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string? AvatarUrl { get; set; } = null;
    }

    public class UpdateAccountDto
    {
        public string? FullName { get; set; } = null;
        public string? Username { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? AvatarUrl { get; set; } = null;
    }

    public class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
