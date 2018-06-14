using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MK.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        List<T> AddRange(List<T> TEnities);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        void Delete(Expression<Func<T, bool>> where);
        void Delete(T entityToDelete);
        Task<bool> ExistsAsync(object primaryKey);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllWithIncludeAsync(List<string> includes);
        Task<IEnumerable<T>> GetAllWithIncludePage(List<string> includes, string filterKey = "", string value = "");
        Task<T> GetByIDAsync(object id);
        Task<T> GetFirstAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetWithIncludeAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includes);
        dynamic GroupByCount<Tkey>(Func<T, Tkey> predicate, Func<T, bool> filter, Func<T, object> AdditionalData = null, params string[] include);
        dynamic GroupBySum<Tkey>(Func<T, Tkey> predicate, Func<T, bool> filter, Func<T, decimal> Selector, params string[] include);
        T Insert(T entity);
        void setEntryState(object entities);
        Task<IEnumerable<T>> TakeWithIncludeAsync(Expression<Func<T, bool>> predicate, int page, params Expression<Func<T, object>>[] includes);
        void Update(T entityToUpdate, bool exclude = true, params string[] properties);
        void UpdateRange(List<T> TEnities);
    }
}