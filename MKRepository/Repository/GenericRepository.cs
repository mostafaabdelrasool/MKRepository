using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace MK.Repository
{
    /// <summary>  
    /// Generic Repository class for Entity Operations  
    /// </summary>  
    /// <typeparam name="TEntity"></typeparam>  
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> 
        where TEntity : class
    {
        #region Private member variables...
        internal DbContext Context;
        internal DbSet<TEntity> DbSet;
        #endregion

        #region Public Constructor...
        /// <summary>  
        /// Public Constructor,initializes privately declared local variables.  
        /// </summary>  
        /// <param name="context"></param>  
        public GenericRepository(DbContext context)
        {
            this.Context = context;
            this.DbSet = context.Set<TEntity>();
        }
        #endregion

        #region Public member methods...

        /// Generic get method on the basis of id for Entities.  
        /// </summary>  
        /// <param name="id"></param>  
        /// <returns></returns>  
        public async virtual Task<TEntity> GetByIDAsync(object id)
        {
            return await DbSet.FindAsync(id);
        }

        /// <summary>  
        /// generic Insert method for the entities  
        /// </summary>  
        /// <param name="entity"></param>  
        public virtual TEntity Insert(TEntity entity)
        {
            return DbSet.Add(entity);
        }

        /// <summary>  
        /// Generic Delete method for the entities  
        /// </summary>  
        /// <param name="entityToDelete"></param>  
        public virtual void Delete(TEntity entityToDelete)
        {
            DbSet.Remove(entityToDelete);
        }
        public virtual void Delete(Expression<Func<TEntity, bool>> where)
        {
            var query = DbSet.Where(where);
            DbSet.RemoveRange(query);
        }
        /// <summary>  
        /// Generic update method for the entities  
        /// </summary>  
        /// <param name="entityToUpdate"></param>  
        public virtual void Update(TEntity entityToUpdate, bool exclude = true, params string[] properties)
        {
            //to check if entity exist in current context
            var attahcedObj = DbSet.Local.FirstOrDefault(x => ((dynamic)x).Id == ((dynamic)entityToUpdate).Id);

            if (attahcedObj != null)
            {

                Context.Entry(attahcedObj).CurrentValues.SetValues(entityToUpdate);
                if (exclude)
                {
                    ExcludeProperty(attahcedObj, properties);
                }
                else
                {
                    var allExcluded = entityToUpdate.GetType().GetProperties()
                            .Where(x => getEleminatedPro(x, properties)).Select(x => x.Name);
                    ExcludeProperty(attahcedObj, allExcluded.ToArray());
                }

            }
            else
            {
                DbSet.Attach(entityToUpdate);
                Context.Entry(entityToUpdate).State = System.Data.Entity.EntityState.Modified;
                if (exclude)
                {
                    ExcludeProperty(attahcedObj, properties);
                }
                else
                {
                    var allExcluded = entityToUpdate.GetType().GetProperties()
                            .Where(x => getEleminatedPro(x, properties)).Select(x => x.Name);
                    ExcludeProperty(attahcedObj, allExcluded.ToArray());
                }
            }
        }

        private bool getEleminatedPro(PropertyInfo x, params string[] properties)
        {

            if ((!x.PropertyType.IsInterface && !x.PropertyType.IsClass
                || x.PropertyType.Name == "String") && !properties.Contains(x.Name))
            {
                return true;
            }
            return false;
        }

        private void ExcludeProperty(TEntity entityToUpdate, string[] excluded)
        {
            var objectContext = ((IObjectContextAdapter)Context).ObjectContext;
            foreach (var entry in objectContext.ObjectStateManager.GetObjectStateEntries(System.Data.Entity.EntityState.Modified).Where(entity => entity.Entity.GetType() == typeof(TEntity)))
            {
                foreach (var item in excluded)
                {
                    entry.RejectPropertyChanges(item);
                }

            }
        }

        /// <summary>  
        /// generic method to fetch all the records from db  
        /// </summary>  
        /// <returns></returns>  
        public async virtual Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        /// <summary>  
        /// Inclue multiple  
        /// </summary>  
        /// <param name="predicate"></param>  
        /// <param name="include"></param>  
        /// <returns></returns>  
        public async Task<IEnumerable<TEntity>> GetWithIncludeAsync(Expression<Func<TEntity, bool>> predicate, 
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = DbSet.Where(predicate);
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
            return await query.ToListAsync();
        }
        /// <summary>  
        /// Generic method to check if entity exists  
        /// </summary>  
        /// <param name="primaryKey"></param>  
        /// <returns></returns>  
        public async Task<bool> ExistsAsync(object primaryKey)
        {
            return await DbSet.FindAsync(primaryKey) != null;
        }

        /// <summary>  
        /// Gets a single record by the specified criteria (usually the unique identifier)  
        /// </summary>  
        /// <param name="predicate">Criteria to match on</param>  
        /// <returns>A single record that matches the specified criteria</returns>  
        public async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = DbSet.Where(predicate);
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
            return await DbSet.FirstOrDefaultAsync<TEntity>(predicate);
        }

        public List<TEntity> AddRange(List<TEntity> TEnities)
        {
            this.DbSet.AddRange(TEnities);
            return TEnities;
        }
        public void UpdateRange(List<TEntity> TEnities)
        {
            TEnities.ForEach(x =>
            {
                this.Update(x);
            });

        }
        /// <summary>  
        /// generic method to get count record on the basis of a condition but query able.  
        /// </summary>  
        /// <param name="where"></param>  
        /// <returns></returns>  
        public async virtual Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate,
            params Expression<Func<TEntity, object>>[] includes)
        {
            var query = DbSet.Where(predicate);
            foreach (var item in includes)
            {
                query = query.Include(item);
            }
            return await query.CountAsync();
        }
        public async virtual Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }
        public void setEntryState(object entities)
        {

            foreach (var item in (IEnumerable)entities)
            {
                Context.Entry(item).State = System.Data.Entity.EntityState.Unchanged;
            }

        }
        public async Task<IEnumerable<TEntity>> TakeWithIncludeAsync(Expression<Func<TEntity, bool>> predicate
           , int page, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = DbSet.Where(predicate);
            foreach (var item in includes)
            {
                query = query.Include(item);
            }

            return await query.Take(page).ToListAsync();
        }
        public dynamic GroupByCount<Tkey>(Func<TEntity, Tkey> predicate, Func<TEntity, bool> filter, 
            Func<TEntity, object> AdditionalData = null, params string[] include)
        {
            IQueryable<TEntity> query = this.DbSet;
            if (include != null)
            {
                query = include.Aggregate(query, (current, inc) => current.Include(inc));
                //query = query.FilterIsDelete(include);
            }
            return query.Where(filter).GroupBy(predicate).Select(x => new { key = x.Key, count = x.Count(),
                Data = AdditionalData != null ? x.Select(AdditionalData).Distinct().ToList() : null });
        }
        public dynamic GroupBySum<Tkey>(Func<TEntity, Tkey> predicate, Func<TEntity, bool> filter, Func<TEntity, decimal> Selector, params string[] include)
        {
            IQueryable<TEntity> query = this.DbSet;
            if (include != null)
            {
                query = include.Aggregate(query, (current, inc) => current.Include(inc));
                //query = query.FilterIsDelete(include);
            }
            return query.Where(filter).GroupBy(predicate).Select(x => new { key = x.Key, sum = x.Sum(Selector) }).ToList();
        }
        public async virtual Task<IEnumerable<TEntity>> GetAllWithIncludePage(List<string> includes, string filterKey = "", string value = "")
        {
            IQueryable<TEntity> query = this.DbSet;
            //install library that convert to lamda expression
            //if (includes != null)
            //{
            //    query = includes.Aggregate(query, (current, inc) => current.Include(inc));
            //    //query = query.FilterIsDelete(include);
            //}
            //if (!string.IsNullOrEmpty(filterKey) && !string.IsNullOrEmpty(value))
            //{
            //    query = query.Where(filterKey + value);
            //}
            return await query.ToListAsync();
        }
        public async virtual Task<IEnumerable<TEntity>> GetAllWithIncludeAsync(List<string> includes)
        {
            foreach (var item in includes)
            {
                DbSet.Include(item);
            }
            return await DbSet.ToListAsync();
        }
        #endregion


    }
}