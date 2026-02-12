using System.Linq.Expressions;
using KidFit.Models;
using KidFit.Shared.Queries;
using X.PagedList;

namespace KidFit.Repositories
{
    public interface IGenericRepo<T> where T : ModelBase
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IPagedList<T>> GetAllAsync(QueryParam<T> param);
        Task CreateAsync(T entity);
        Task CreateBatch(IList<T> entites);
        void Update(T entity);
        Task<bool> SoftDeleteAsync(Guid id);
        Task<int> BulkSoftDeleteAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountExists(List<Guid> ids);
    }
}
