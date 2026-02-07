using FluentValidation;
using KidFit.Models;

namespace KidFit.Validators
{

    public class ModuleValidation : AbstractValidator<Module>
    {
        public ModuleValidation()
        {
            RuleFor(m => m.Name).NotEmpty();
            RuleFor(m => m.Description).NotEmpty();
            RuleFor(m => m.CoreSlot).GreaterThan(0);
            RuleFor(m => m.TotalSlot).GreaterThan(0);
            RuleFor(m => m.CoreSlot).LessThanOrEqualTo(m => m.TotalSlot).WithMessage("CoreSlot must be less than or equal to TotalSlot");
        }
    }

    public class ModuleQueryParamValidation : QueryParamValidation<Module>
    {
    }
}
