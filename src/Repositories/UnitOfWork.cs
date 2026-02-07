using KidFit.Data;
using KidFit.Models;

namespace KidFit.Repositories
{
    public class UnitOfWork(AppDbContext context) : IUnitOfWork
    {
        private readonly AppDbContext _context = context;
        private readonly Dictionary<string, object> _repos = [];

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public IGenericRepo<T> Repo<T>() where T : ModelBase
        {
            var type = typeof(T).Name;

            if (!_repos.ContainsKey(type))
            {
                var repo = new GenericRepo<T>(_context);
                _repos.Add(type, repo);
                return repo;
            }

            return (IGenericRepo<T>)_repos[type];
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
