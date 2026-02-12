using FluentValidation;
using KidFit.Shared.Constants;

namespace KidFit.Models
{
    public class Lesson : ModelBase
    {
        public string Name { get; set; } = "";
        public string Content { get; set; } = "";
        public Guid ModuleId { get; set; }
        public Module Module { get; set; } = new();
        public List<Card> Cards { get; set; } = [];
        public Year Year { get; set; }
    }

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
