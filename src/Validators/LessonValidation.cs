using FluentValidation;
using KidFit.Models;

namespace KidFit.Validators
{

    public class LessonValidation : AbstractValidator<Lesson>
    {
        public LessonValidation()
        {
            RuleFor(l => l.Name).NotEmpty();
            RuleFor(l => l.Content).NotEmpty();
            RuleFor(l => l.ModuleId).NotEmpty();
            RuleFor(l => l.Year).NotEmpty();
            RuleFor(l => l.Cards).NotEmpty();
        }
    }

    public class LessonQueryParamValidation : QueryParamValidation<Lesson>
    {
    }
}
