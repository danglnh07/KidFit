using FluentValidation;
using KidFit.Models;

namespace KidFit.Validators
{

    public class CardValidator : AbstractValidator<Card>
    {
        public CardValidator()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("Card name must not be empty");
            RuleFor(c => c.Description).NotEmpty().WithMessage("Card description must not be empty");
            RuleFor(c => c.Image).NotEmpty().WithMessage("Card image must not be empty");
            RuleFor(c => c.CategoryId).NotEmpty().WithMessage("Card category ID must not be empty");
        }
    }
}
