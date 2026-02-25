using System.ComponentModel.DataAnnotations;

namespace KidFit.ViewModels
{
    public class CreateAccountViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = "";
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email is not valid")]
        public string Email { get; set; } = "";
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "";
        public string? AvatarUrl { get; set; }
    }

    public class AccountViewModel
    {
        [Required(ErrorMessage = "Id is required")]
        public string Id { get; set; } = "";
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = "";
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = "";
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class AccountViewModelWithRole : AccountViewModel
    {
        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "";
    }
}
