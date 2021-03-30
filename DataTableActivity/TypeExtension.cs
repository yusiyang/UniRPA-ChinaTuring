using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTableActivity
{
    internal static class TypeExtension
    {
        public static bool IsNumericType(this Type o)
        {
            return !o.IsClass && !o.IsInterface && o.GetInterfaces().Any(q => q == typeof(IFormattable));
        }
    }
}
