
using KidFit.Models;

namespace KidFit.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepo<T> Repo<T>() where T : ModelBase;
        Task<int> SaveChangesAsync();
    }
}
