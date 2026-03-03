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
            RuleFor(l => l.CardIds).NotEmpty().WithMessage("Lesson card must not be empty");
        }
    }
}
