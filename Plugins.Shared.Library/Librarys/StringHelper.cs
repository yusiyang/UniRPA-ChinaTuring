using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugins.Shared.Library.Librarys
{
    public static class StringHelper
    {
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        public static string ToPercentNumber(double? v)
        {
            if (v == null) return "";
            return v.Value.ToString("0.00%");
        }

        public static List<T> ToList<T>(this string str)
        {
            if (str.IsNullOrWhiteSpace())
            {
                return new List<T>();
            }

            try
            {
                return str.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(s =>
                {
                    if (s.TryChangeType(typeof(T), out var value))
                    {
                        return (T) value;
                    }

                    throw new Exception($"{s}类型转换出错");
                }).ToList();
            }
            catch (Exception e)
            {
                return new List<T>();
            }
        }

        public static string ToDefaultWhiteSpace(this string str) {
            if (str == null) return "";
            return str;
        }
    }
}
