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
                // If the type request is has specfic implementation
                if (typeof(T).IsAssignableTo(typeof(Card)))
                {
                    var cardRepo = new CardRepo(_context);
                    _repos.Add(type, cardRepo);
                    return (IGenericRepo<T>)cardRepo;
                }
                else if (typeof(T).IsAssignableTo(typeof(Lesson)))
                {
                    var lessonRepo = new LessonRepo(_context);
                    _repos.Add(type, lessonRepo);
                    return (IGenericRepo<T>)lessonRepo;
                }

                // If the model use GenericRepo
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
