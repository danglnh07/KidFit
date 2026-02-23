namespace KidFit.ViewModels
{
    public class CreateAccountViewModel
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string? AvatarUrl { get; set; }
    }

    public class UpdateAccountViewModel
    {
        public string? UserName { get; set; } = null;
        public string? Email { get; set; } = null;
        public string? AvatarUrl { get; set; } = null;
    }

    public class AccountViewModel
    {
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class AccountViewModelWithRole : AccountViewModel
    {
        public string Role { get; set; } = "";
    }
}
