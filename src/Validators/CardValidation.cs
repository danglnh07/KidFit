using FluentValidation;
using KidFit.Models;

namespace KidFit.Validators
{
    public class CardValidation : AbstractValidator<Card>
    {
        public CardValidation()
        {
            RuleFor(c => c.Name).NotEmpty();
            RuleFor(c => c.Description).NotEmpty();
            RuleFor(c => c.Image).NotEmpty();
            RuleFor(c => c.CategoryId).NotEmpty();
        }
    }

    public class CardQueryParamValidation : QueryParamValidation<Card>
    {
    }
}
