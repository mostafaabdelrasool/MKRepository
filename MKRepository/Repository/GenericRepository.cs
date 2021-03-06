﻿using MKRepository.EFFunctions;
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
    /// <typeparam name="T"></typeparam>  
    public class GenericRepository<T> : IGenericRepository<T>
        where T : class
    {
        #region Private member variables...
        internal DbContext Context;
        internal DbSet<T> DbSet;
        #endregion

        #region Public Constructor...
        /// <summary>  
        /// Public Constructor,initializes privately declared local variables.  
        /// </summary>  
        /// <param name="context"></param>  
        public GenericRepository(DbContext context)
        {
            this.Context = context;
            this.DbSet = context.Set<T>();
        }
        #endregion

        #region CUD
        /// <summary>  
        /// generic Insert method for the entities  
        /// </summary>  
        /// <param name="entity"></param>  
        public virtual T Insert(T entity)
        {
            return DbSet.Add(entity);
        }

        /// <summary>  
        /// Generic Delete method for the entities  
        /// </summary>  
        /// <param name="entityToDelete"></param>  
        public virtual void Delete(T entityToDelete)
        {
            DbSet.Remove(entityToDelete);
        }
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            var query = DbSet.Where(where);
            DbSet.RemoveRange(query);
        }
        /// <summary>  
        /// Generic update method for the entities  
        /// </summary>  
        /// <param name="entityToUpdate"></param>  
        public virtual void Update(T entityToUpdate, bool exclude = true, params string[] properties)
        {
            //to check if entity exist in current context
            var attahcedObj = DbSet.Local.FirstOrDefault(x => ((dynamic)x).Id == ((dynamic)entityToUpdate).Id);
            if (attahcedObj != null)
            {
                Context.Entry(attahcedObj).CurrentValues.SetValues(entityToUpdate);
            }
            else
            {
                DbSet.Attach(entityToUpdate);
                Context.Entry(entityToUpdate).State = System.Data.Entity.EntityState.Modified;
            }
            if (exclude)
            {
                PropertiesFunctions.ExcludeProperty<T>(Context, properties);
            }
            else
            {
                var allExcluded = entityToUpdate.GetType().GetProperties()
                        .Where(x => PropertiesFunctions.getEleminatedPro(x, properties)).Select(x => x.Name);
                PropertiesFunctions.ExcludeProperty<T>(Context, allExcluded.ToArray());
            }
        }
        public List<T> AddRange(List<T> TEnities)
        {
            this.DbSet.AddRange(TEnities);
            return TEnities;
        }
        public void UpdateRange(List<T> TEnities)
        {
            TEnities.ForEach(x =>
            {
                this.Update(x);
            });

        }
        public void setEntryState(object entities)
        {

            foreach (var item in (IEnumerable)entities)
            {
                Context.Entry(item).State = System.Data.Entity.EntityState.Unchanged;
            }

        }
        #endregion
        #region Getting Functions
        /// Generic get method on the basis of id for Entities.  
        /// </summary>  
        /// <param name="id"></param>  
        /// <returns></returns>  
        public async virtual Task<T> GetByIDAsync(object id)
        {
            return await DbSet.FindAsync(id);
        }
        /// <summary>  
        /// generic method to fetch all the records from db  
        /// </summary>  
        /// <returns></returns>  
        public async virtual Task<IEnumerable<T>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        /// <summary>  
        /// Inclue multiple  
        /// </summary>  
        /// <param name="predicate"></param>  
        /// <param name="include"></param>  
        /// <returns></returns>  
        public async Task<IEnumerable<T>> GetWithIncludeAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = ImplementQueryInclude(predicate, includes);
            return await query.ToListAsync();
        }

        private IQueryable<T> ImplementQueryInclude(Expression<Func<T, bool>> predicate, Expression<Func<T, object>>[] includes)
        {
            var query = DbSet.Where(predicate);
            foreach (var item in includes)
            {
                query = query.Include(item);
            }

            return query;
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
        public async Task<T> GetFirstAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = ImplementQueryInclude(predicate, includes);
            return await DbSet.FirstOrDefaultAsync<T>(predicate);
        }


        /// <summary>  
        /// generic method to get count record on the basis of a condition but query able.  
        /// </summary>  
        /// <param name="where"></param>  
        /// <returns></returns>  
        public async virtual Task<int> CountAsync(Expression<Func<T, bool>> predicate,
            params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = ImplementQueryInclude(predicate, includes);
            return await query.CountAsync();
        }
        public async virtual Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }

        public async Task<IEnumerable<T>> TakeWithIncludeAsync(Expression<Func<T, bool>> predicate
           , int page, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = ImplementQueryInclude(predicate, includes);
            return await query.Take(page).ToListAsync();
        }
        public dynamic GroupByCount<Tkey>(Func<T, Tkey> predicate, Func<T, bool> filter,
            Func<T, object> AdditionalData = null, params string[] include)
        {
            IQueryable<T> query = this.DbSet;
            if (include != null)
            {
                query = include.Aggregate(query, (current, inc) => current.Include(inc));
                //query = query.FilterIsDelete(include);
            }
            return query.Where(filter).GroupBy(predicate).Select(x => new
            {
                key = x.Key,
                count = x.Count(),
                Data = AdditionalData != null ? x.Select(AdditionalData).Distinct().ToList() : null
            });
        }
        public dynamic GroupBySum<Tkey>(Func<T, Tkey> predicate, Func<T, bool> filter, Func<T, decimal> Selector, params string[] include)
        {
            IQueryable<T> query = this.DbSet;
            if (include != null)
            {
                query = include.Aggregate(query, (current, inc) => current.Include(inc));
                //query = query.FilterIsDelete(include);
            }
            return query.Where(filter).GroupBy(predicate).Select(x => new { key = x.Key, sum = x.Sum(Selector) }).ToList();
        }
        public async virtual Task<IEnumerable<T>> GetAllWithIncludePage(List<string> includes, string filterKey = "", string value = "")
        {
            IQueryable<T> query = this.DbSet;
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
        public async virtual Task<IEnumerable<T>> GetAllWithIncludeAsync(List<string> includes)
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