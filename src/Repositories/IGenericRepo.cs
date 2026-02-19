using System.Linq.Expressions;
using KidFit.Models;
using KidFit.Shared.Queries;
using X.PagedList;

namespace KidFit.Repositories
{
    public interface IGenericRepo<T> where T : ModelBase
    {
        // Get entity by ID
        Task<T?> GetByIdAsync(Guid id);

        // Return true if entity exist and not deleted 
        Task<bool> IsExistAsync(Guid id);

        // Count how many of the provided ids exist and not deleted 
        Task<int> CountExistAsync(List<Guid> ids);

        // Get paged list of entities
        Task<IPagedList<T>> GetAllAsync(QueryParam<T> param);

        // Create entity
        Task CreateAsync(T entity);

        // Create a batch of entities
        Task CreateBatchAsync(IList<T> entites);

        // Update entity
        void Update(T entity);

        // Delete entity
        Task<bool> SoftDeleteAsync(Guid id);

        // Delete a list of entities based on a predicate
        // Use for delete all associated entities where the current entity get deleted
        Task<int> BulkSoftDeleteAsync(Expression<Func<T, bool>> predicate);

        // Count how many entities exist and not deleted
        Task<int> CountAsync();
    }
}
