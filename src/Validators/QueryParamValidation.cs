using FluentValidation;
using KidFit.Dtos.Requests;

namespace KidFit.Validators
{
    public class QueryParamValidation<T> : AbstractValidator<QueryParamDto> where T : class
    {
        public QueryParamValidation()
        {
            RuleFor(q => q.Page).GreaterThan(0).WithMessage("Page must be greater than 0");
            RuleFor(q => q.Size).GreaterThan(0).LessThanOrEqualTo(50).WithMessage("Size must be integer in range [1, 50]");

            // Check if order by value match any field
            var fields = typeof(T).GetProperties();
            RuleFor(q => q.OrderBy)
                .Must(o => o is not null && fields.Any(f => string.Equals(f.Name, o, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Order by field not found");
        }

    }
}
