using FluentValidation;
using KidFit.Dtos;

namespace KidFit.Validators
{
    public class QueryParamValidator<T> : AbstractValidator<QueryParamDto>
    {
        public QueryParamValidator()
        {
            RuleFor(q => q.Page).GreaterThan(0).WithMessage("Page must be greater than 0");
            RuleFor(q => q.Size).GreaterThan(0)
                .LessThanOrEqualTo(50)
                .WithMessage("Size must be integer in range [1, 50]");

            var fields = typeof(T).GetProperties();
            RuleFor(q => q.OrderBy)
                .Must(o => o is null || fields.Any(f => string.Equals(f.Name, o, StringComparison.OrdinalIgnoreCase)))
                .WithMessage($"Order by field not found for this type: {typeof(T).Name}");
        }
    }
}
