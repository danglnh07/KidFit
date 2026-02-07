using FluentValidation;
using KidFit.Models;

namespace KidFit.Validators
{
    public class CardCategoryValidation : AbstractValidator<CardCategory>
    {
        public CardCategoryValidation()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("Card category name is empty");
            RuleFor(c => c.Description).NotEmpty().WithMessage("Card category description is empty");
            RuleFor(c => c.BorderColor).NotEmpty().WithMessage("Card category border color is empty");
        }
    }

    public class CardCategoryQueryParamValidation : QueryParamValidation<CardCategory>
    {
    }
}
