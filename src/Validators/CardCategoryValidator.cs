using FluentValidation;
using KidFit.Models;

namespace KidFit.Validators
{
    public class CardCategoryValidator : AbstractValidator<CardCategory>
    {
        public CardCategoryValidator()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("Card category name must not be empty");
            RuleFor(c => c.Description).NotEmpty().WithMessage("Card category description must not be empty");
            RuleFor(c => c.BorderColor).NotEmpty().WithMessage("Card category border color must not be empty");
        }
    }

}
