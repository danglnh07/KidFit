using FluentValidation;
using KidFit.Models;

namespace KidFit.Validators
{
    public class LessonValidator : AbstractValidator<Lesson>
    {
        public LessonValidator()
        {
            RuleFor(l => l.Name).NotEmpty().WithMessage("Lesson name must not be empty");
            RuleFor(l => l.Content).NotEmpty().WithMessage("Lesson content must not be empty");
            RuleFor(l => l.ModuleId).NotEmpty().WithMessage("Lesson module ID must not be empty");
            RuleFor(l => l.Year).NotEmpty().WithMessage("Lesson year must not be empty");
            RuleFor(l => l.Cards).NotEmpty().WithMessage("Lesson cards must not be empty");
        }
    }
}
