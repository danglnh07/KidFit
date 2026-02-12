using FluentValidation;

namespace KidFit.Models
{
    public class Module : ModelBase
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int CoreSlot { get; set; }
        public int TotalSlot { get; set; }
    }

    public class ModuleValidator : AbstractValidator<Module>
    {
        public ModuleValidator()
        {
            RuleFor(m => m.Name).NotEmpty().WithMessage("Module name must not be empty");
            RuleFor(m => m.Description).NotEmpty().WithMessage("Module description must not be empty");
            RuleFor(m => m.CoreSlot).GreaterThan(0).WithMessage("Core slot must be greater than 0");
            RuleFor(m => m.TotalSlot).GreaterThan(0).WithMessage("Total slot must be greater than 0");
            RuleFor(m => m.CoreSlot)
                .LessThanOrEqualTo(m => m.TotalSlot)
                .WithMessage("CoreSlot must be less than or equal to TotalSlot");
        }
    }
}
