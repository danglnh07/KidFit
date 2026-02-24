using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace KidFit.ViewModels
{
    public class LoginRequestViewModel
    {
        [Required]
        public string Username { get; set; } = "";
        [Required]
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
        [Required]
        public string NewPassword { get; set; } = "";
    }
}
