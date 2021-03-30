using Plugins.Shared.Library.Extensions;
using System;

namespace Plugins.Shared.Library.Librarys
{
    public static class ConvertHelper
    {
        public static double ToDouble(this object obj, double defaultVal = 0.0)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return defaultVal;
            }

            try
            {
                double val;
                if(double.TryParse(obj.ToString(),out val))
                {
                    return val;
                }

                return defaultVal;
            }
            catch (Exception e)
            {
                return defaultVal;
            }
        }


        public static int ToInt(this object obj, int defaultVal = 0)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return defaultVal;
            }

            try
            {
                int val;
                if (int.TryParse(obj.ToString(), out val))
                {
                    return val;
                }

                return defaultVal;
            }
            catch (Exception e)
            {
                return defaultVal;
            }
        }

        public static string ToStringEx(this object obj, string defalutStr = "")
        {
            if (obj == null || obj == DBNull.Value)
            {
                return defalutStr;
            }

            return obj.ToString();
        }

        public static bool TryChangeType(this object value, Type targetType,out object newValue)
        {
            if (targetType.IsNullableType())
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }

            newValue = null;
            try
            {
                newValue = Convert.ChangeType(value, targetType);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
