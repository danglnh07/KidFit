using FluentValidation;
using KidFit.Models;

namespace KidFit.Validators
{
    public class ApplicationUserValidator : AbstractValidator<ApplicationUser>
    {
        public ApplicationUserValidator()
        {
            RuleFor(u => u.UserName).NotEmpty().WithMessage("Username is required");
            RuleFor(u => u.Email).NotEmpty().WithMessage("Email is required");

            // No need to check for password, since UserManager will do that 
            // based on what we configured
        }
    }
}
