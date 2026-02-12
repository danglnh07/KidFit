using FluentValidation;

namespace KidFit.Models
{
    public class Card : ModelBase
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Image { get; set; } = "";
        public Guid CategoryId { get; set; }
        public CardCategory Category { get; set; } = new();
    }

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
