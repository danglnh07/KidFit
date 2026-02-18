using System.Linq.Expressions;
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
}
