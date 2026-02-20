using System.Linq.Expressions;

namespace KidFit.Shared.Queries
{
    public class QueryParam<T>
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public Expression<Func<T, object>>? OrderBy { get; set; }
        public bool? IsAsc { get; set; } = true;

        private Expression<Func<T, object>>? FromStringToExpression(string? orderBy)
        {
            if (orderBy is null) return null;

            foreach (var prop in typeof(T).GetProperties())
            {
                if (string.Equals(prop.Name, orderBy, StringComparison.OrdinalIgnoreCase))
                {
                    var parameter = Expression.Parameter(typeof(T), "t");
                    var propertyAccess = Expression.Property(parameter, prop);
                    var converted = Expression.Convert(propertyAccess, typeof(object));
                    return Expression.Lambda<Func<T, object>>(converted, parameter);
                }
            }

            throw new ArgumentException($"OrderBy property {orderBy} not found in type {typeof(T).Name}");
        }

        public QueryParam(int page, int size, string? orderBy, bool? isAsc)
        {
            Page = page < 1 ? 1 : page;
            Size = size < 0 ? 5 : size;
            IsAsc = isAsc;
            OrderBy = FromStringToExpression(orderBy);
        }

        public QueryParam(int page, int size)
        {
            Page = page < 1 ? 1 : page;
            Size = size < 0 ? 5 : size;
            IsAsc = null;
            OrderBy = null;
        }
    }
}
