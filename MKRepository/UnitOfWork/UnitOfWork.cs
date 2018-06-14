using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using MK.Repository;
using System.Threading.Tasks;

namespace MK.UnitOfWork
{
    /// <summary>  
    /// Unit of Work class responsible for DB transactions  
    /// </summary>  
    public class UnitOfWork<TContext> : IDisposable, IUnitOfWork<TContext> where TContext : DbContext,new()
    {
        #region Private member variables...

        public DbContext _context = null;

        #endregion

        public UnitOfWork()
        {
            this._context = new TContext();
            _context.Configuration.LazyLoadingEnabled = false;
            _context.Configuration.ProxyCreationEnabled = false;
        }

        #region Public member methods...
        /// <summary>  
        /// Save method.  
        /// </summary>  
        public async Task<int> SaveAsync()
        {
            try
            {
              return  await  _context.SaveChangesAsync();
            }
          
            catch (DbEntityValidationException e)
            {

                throw e;
            }
            catch (Exception ex)
            {
                return 0;
            }

        }
        public void Refresh()
        {
            var refreshableObjects = _context.ChangeTracker.Entries();
            var context = ((IObjectContextAdapter)_context).ObjectContext;
            context.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, refreshableObjects);
        }
        #endregion

        #region Implementing IDiosposable...

        #region private dispose variable declaration...
        private bool disposed = false;
        #endregion

        /// <summary>  
        /// Protected Virtual Dispose method  
        /// </summary>  
        /// <param name="disposing"></param>  
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Debug.WriteLine("UnitOfWork is being disposed");
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        /// <summary>  
        /// Dispose method  
        /// </summary>  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public GenericRepository<T> Init<T>() where T : class
        {
            GenericRepository<T> gr = new GenericRepository<T>(_context);
            return gr;

        }
        public void ExecuteSqlCommand(string cmd)
        {
            this._context.Database.ExecuteSqlCommand(cmd);
        }

        public int save()
        {
            try
            {
                return  _context.SaveChanges();
            }

            catch (DbEntityValidationException e)
            {

                throw e;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
    }

}