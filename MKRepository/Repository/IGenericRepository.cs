using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MK.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        List<TEntity> AddRange(List<TEntity> TEnities);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
        void Delete(Expression<Func<TEntity, bool>> where);
        void Delete(TEntity entityToDelete);
        Task<bool> ExistsAsync(object primaryKey);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> GetAllWithIncludeAsync(List<string> includes);
        Task<IEnumerable<TEntity>> GetAllWithIncludePage(List<string> includes, string filterKey = "", string value = "");
        Task<TEntity> GetByIDAsync(object id);
        Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
        Task<IEnumerable<TEntity>> GetWithIncludeAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
        dynamic GroupByCount<Tkey>(Func<TEntity, Tkey> predicate, Func<TEntity, bool> filter, Func<TEntity, object> AdditionalData = null, params string[] include);
        dynamic GroupBySum<Tkey>(Func<TEntity, Tkey> predicate, Func<TEntity, bool> filter, Func<TEntity, decimal> Selector, params string[] include);
        TEntity Insert(TEntity entity);
        void setEntryState(object entities);
        Task<IEnumerable<TEntity>> TakeWithIncludeAsync(Expression<Func<TEntity, bool>> predicate, int page, params Expression<Func<TEntity, object>>[] includes);
        void Update(TEntity entityToUpdate, bool exclude = true, params string[] properties);
        void UpdateRange(List<TEntity> TEnities);
    }
}