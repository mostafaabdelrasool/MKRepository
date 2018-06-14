using MK.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MKRepository.Extentions
{
    public static class RepositorySync
    {
        public static void Xx<T>(this IGenericRepository<T> genericRepository)
             where T : class
        {
            if (genericRepository == null)
            {
                throw new ArgumentNullException(nameof(genericRepository));
            }
        }
    }
}
