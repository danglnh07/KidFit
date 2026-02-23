using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace KidFit.ViewModels
{
    public class CreateAccountViewModel
    {
        [Required]
        [NotNull]
        public string UserName { get; set; } = "";
        [Required]
        [NotNull]
        [EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        [NotNull]
        public string Role { get; set; } = "";
        public string? AvatarUrl { get; set; }
    }

    public class AccountViewModel
    {
        [Required]
        [NotNull]
        public string Id { get; set; } = "";
        [Required]
        [NotNull]
        public string UserName { get; set; } = "";
        [Required]
        [NotNull]
        [EmailAddress]
        public string Email { get; set; } = "";
        [Required]
        [NotNull]
        public bool IsActive { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class AccountViewModelWithRole : AccountViewModel
    {
        [Required]
        [NotNull]
        public string Role { get; set; } = "";
    }
}
