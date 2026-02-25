using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KidFit.ViewModels
{
    public class LoginRequestViewModel
    {
        [Required(ErrorMessage = "Username is required for login")]
        public string Username { get; set; } = "";
        [Required(ErrorMessage = "Password is required for login")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
        [ValidateNever]
        public string? ReturnUrl { get; set; }
    }

    public class ResetPasswordRequestViewModel
    {
        [Required]
        public string Id { get; set; } = "";
        [Required]
        public string Token { get; set; } = "";
        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = "";
    }
}
