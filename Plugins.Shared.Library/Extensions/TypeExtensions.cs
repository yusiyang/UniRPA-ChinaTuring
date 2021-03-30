using Plugins.Shared.Library.Librarys;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Plugins.Shared.Library.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsStatic(this Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }

        public static void SetPropValue(this object obj,string propName,object value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            var type = obj.GetType();

            if (string.IsNullOrWhiteSpace(propName))
            {
                throw new ArgumentException($"参数{propName}有误");
            }
            var dotIndex = propName.IndexOf(".");
            if (dotIndex == -1)
            {
                var prop = type.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public);
                if (prop == null)
                {
                    throw new ArgumentOutOfRangeException(propName);
                }

                object objVal;
                if (value.TryChangeType(prop.PropertyType, out objVal))
                {
                    prop.SetValue(obj, objVal);
                    return;
                }
                throw new ArgumentException("value数据类型有误");
            }

            var tempPropName = propName.Substring(0, dotIndex);
            var tempProp= type.GetProperty(tempPropName, BindingFlags.Instance | BindingFlags.Public);
            if (tempProp == null)
            {
                throw new ArgumentOutOfRangeException(tempPropName);
            }

            var tempPropObj = tempProp.GetValue(obj);
            if (tempPropObj == null)
            {
                tempPropObj = Activator.CreateInstance(tempProp.PropertyType);
                tempProp.SetValue(obj, tempPropObj);
            }

            tempPropObj.SetPropValue(propName.Substring(dotIndex+1),value);
        }

        public static void RemoveEventHandler(this object obj,string eventName,MethodBase methodInfo=null)
        {
            var type = obj.GetType();
            if(methodInfo==null)
            {
                var trace = new StackTrace(true);
                var frame = trace.GetFrame(1);
                methodInfo = frame.GetMethod();
            }
            var eventInfo = type.GetEvent(eventName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            var eventField = type.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic|BindingFlags.Public);
            var delegates = (Delegate)eventField.GetValue(obj);
            var removeMethod = eventInfo.GetRemoveMethod(true);

            foreach (var item in delegates.GetInvocationList())
            {
                if (item.Method == methodInfo)
                {
                    removeMethod.Invoke(obj, new[] { item });
                    break;
                }
            }
        }

        public static Type GetAsGenericTypeOrDefault(this Type type)
        {
            if (!type.IsGenericType || type.IsGenericTypeDefinition)
            {
                return type;
            }
            return type.GetGenericTypeDefinition();
        }

        public static IEnumerable<Type> GetUsedTypes(this Type targetType)
        {
            return internalGetUsedTypes(targetType).Distinct();
            IEnumerable<Type> internalGetUsedTypes(Type type)
            {
                Type typeAsGeneric = type.GetAsGenericTypeOrDefault();
                yield return typeAsGeneric;
                Type[] genericTypeArguments = type.GenericTypeArguments;
                foreach (Type targetType2 in genericTypeArguments)
                {
                    foreach (Type usedType in targetType2.GetUsedTypes())
                    {
                        Type asGenericTypeOrDefault = usedType.GetAsGenericTypeOrDefault();
                        if (!usedType.IsGenericParameter && typeAsGeneric != asGenericTypeOrDefault)
                        {
                            yield return asGenericTypeOrDefault;
                        }
                    }
                }
            }
        }

        public static string GetDisplayName(this Type type)
        {
            if (type.IsGenericType)
            {
                string text = type.Name;
                int num = text.IndexOf('`');
                if (num > 0)
                {
                    text = text.Substring(0, num);
                }
                Type[] genericArguments = type.GetGenericArguments();
                StringBuilder stringBuilder = new StringBuilder(text);
                stringBuilder.Append("<");
                for (int i = 0; i < genericArguments.Length - 1; i++)
                {
                    stringBuilder.AppendFormat("{0},", GetDisplayName(genericArguments[i]));
                }
                stringBuilder.AppendFormat("{0}>", GetDisplayName(genericArguments[genericArguments.Length - 1]));
                return stringBuilder.ToString();
            }
            return type.Name;
        }

        public static string GetFriendlyTypeName(this Type type)
        {
            var codeDomProvider = CodeDomProvider.CreateProvider("C#");
            var typeReferenceExpression = new CodeTypeReferenceExpression(new CodeTypeReference(type));
            using (var writer = new StringWriter())
            {
                codeDomProvider.GenerateCodeFromExpression(typeReferenceExpression, writer, new CodeGeneratorOptions());
                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
