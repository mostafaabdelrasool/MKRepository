using MK.Repository;
using System.Data.Entity;
using System.Threading.Tasks;
namespace MK.UnitOfWork
{
    public interface IUnitOfWork<TContext> where TContext : DbContext, new()
    {
        /// <summary>
        /// Save method.
        /// </summary>
        Task<int> SaveAsync();
        int save();
        GenericRepository<T> Init<T>() where T : class;
        void Refresh();
    }
}