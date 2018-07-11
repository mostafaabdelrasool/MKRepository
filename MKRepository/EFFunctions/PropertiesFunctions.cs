using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MKRepository.EFFunctions
{
    internal class PropertiesFunctions
    {
        public static void ExcludeProperty<T>(DbContext Context, string[] excluded)
        {
            var objectContext = ((IObjectContextAdapter)Context).ObjectContext;
            foreach (var entry in objectContext.ObjectStateManager.GetObjectStateEntries(System.Data.Entity.EntityState.Modified)
                .Where(entity => entity.Entity.GetType() == typeof(T)))
            {
                foreach (var item in excluded)
                {
                    entry.RejectPropertyChanges(item);
                }

            }
        }
        public static bool getEleminatedPro(PropertyInfo property, params string[] properties)
        {

            if ((!property.PropertyType.IsInterface && !property.PropertyType.IsClass
                || property.PropertyType.Name == "String") && !properties.Contains(property.Name))
            {
                return true;
            }
            return false;
        }
    }
}
