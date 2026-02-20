using System.Linq.Expressions;
using KidFit.Data;
using KidFit.Models;
using KidFit.Shared.Queries;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace KidFit.Repositories
{
    public class GenericRepo<T>(AppDbContext context) : IGenericRepo<T> where T : ModelBase
    {
        protected readonly AppDbContext _context = context;

        public async Task<int> BulkSoftDeleteAsync(Expression<Func<T, bool>> predicate)
        {
            var now = DateTimeOffset.UtcNow; // Use variable so that SQLite can translate in unit test :v
            return await _context.Set<T>()
                .Where(predicate)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.TimeUpdated, now)
                );
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<T>().CountAsync();
        }

        public async Task<int> CountExistAsync(List<Guid> ids)
        {
            return await _context.Set<T>().CountAsync(x => ids.Contains(x.Id));
        }

        public async Task CreateAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task CreateBatchAsync(IList<T> entites)
        {
            await _context.Set<T>().AddRangeAsync(entites);
        }

        public async Task<IPagedList<T>> GetAllAsync(QueryParam<T> param)
        {
            IQueryable<T> query = _context.Set<T>();

            if (param.OrderBy is not null && param.IsAsc is not null)
            {
                query = param.IsAsc == true
                    ? query.OrderBy(param.OrderBy)
                    : query.OrderByDescending(param.OrderBy);
            }

            return await query.ToPagedListAsync(param.Page, param.Size);
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(item => item.Id == id);
        }

        public async Task<bool> IsExistAsync(Guid id)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(item => item.Id == id) is not null;
        }

        public async Task<bool> SoftDeleteAsync(Guid id)
        {
            // Find entity by ID
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.TimeUpdated = DateTimeOffset.UtcNow;
            return true;
        }

        public void Update(T entity)
        {
            entity.TimeUpdated = DateTimeOffset.UtcNow;
            _context.Update(entity);
        }
    }
}
