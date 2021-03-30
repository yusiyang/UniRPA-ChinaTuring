using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Plugins.Shared.Library.Librarys
{
    public static class EnumHelper
    {
        /// <summary>
        /// get enum description by name
        /// </summary>
        /// <typeparam name="T">enum type</typeparam>
        /// <param name="enumItemName">the enum name</param>
        /// <returns></returns>
        public static string GetDescription<T>(this T enumItem)
        {
            FieldInfo fi = enumItem.GetType().GetField(enumItem.ToString());

            if(fi == null)
            {
                return "";
            }

            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return enumItem.ToString();
            }
        }

        public static Dictionary<T,string> GetEnumInfos<T>(this T enumItem)
        {
            var enumType = typeof(T);
            if (!enumType.IsEnum)
            {
                return null;
            }

            var enumInfos = new Dictionary<T, string>();
            var fields = enumType.GetFields(BindingFlags.Static|BindingFlags.Public);
            foreach (var fieldInfo in fields)
            {
                string desText;
                var description= (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (description == null || description.Length == 0)
                {
                    desText = fieldInfo.Name;
                }
                else
                {
                    desText = description[0].Description;
                }
                enumInfos.Add((T)fieldInfo.GetValue(null), desText);
            }

            return enumInfos;
        }

        public static List<string> GetDescriptionsByEnum(Type enumType)
        {
            var resultList= new List<string>();
            if (!enumType.IsEnum)
            {
                return resultList;
            }

            var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var fieldInfo in fields)
            {
                var description = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (description == null || description.Length == 0)
                {
                    resultList.Add(fieldInfo.Name);
                }
                else
                {
                    resultList.Add(description[0].Description);
                }
            }

            return resultList;
        }


        public static TEnum GetEnumByDescription<TEnum>(this string description) where TEnum : struct
        {
            var result = default(TEnum);
            if (string.IsNullOrWhiteSpace(description))
            {
                return result;
            }

            var enumType = typeof(TEnum);
            var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var fieldInfo in fields)
            {
                var des = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (des.Length > 0 && des[0].Description == description)
                {
                    result = (TEnum)fieldInfo.GetValue(null);
                }
            }

            return result;
        }

        public static bool TryGetEnumByDescription<TEnum>(string description,out TEnum result) where TEnum : struct
        {
            result = default(TEnum);
            if (string.IsNullOrWhiteSpace(description))
            {
                return false;
            }

            var enumType = typeof(TEnum);
            var fields = enumType.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (var fieldInfo in fields)
            {
                var des = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (des.Length > 0 && des[0].Description == description)
                {
                    result = (TEnum) fieldInfo.GetValue(null);
                    return true;
                }
            }

            return false;
        }
    }
}