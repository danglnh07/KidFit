using System.Linq.Expressions;
using FluentValidation;
using KidFit.Dtos;

namespace KidFit.Shared.Queries
{
    public class QueryParam<T>
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public Expression<Func<T, object>>? OrderBy { get; set; }
        public bool? IsAsc { get; set; } = true;

        public QueryParam(QueryParamDto dto)
        {
            Page = dto.Page;
            Size = dto.Size;
            IsAsc = dto.IsAsc;

            foreach (var prop in typeof(T).GetProperties())
            {
                if (string.Equals(prop.Name, dto.OrderBy, StringComparison.OrdinalIgnoreCase))
                {
                    var parameter = Expression.Parameter(typeof(T), "t");
                    var propertyAccess = Expression.Property(parameter, prop);
                    var converted = Expression.Convert(propertyAccess, typeof(object));

                    OrderBy = Expression.Lambda<Func<T, object>>(converted, parameter);

                    return;
                }
            }
        }
    }

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
