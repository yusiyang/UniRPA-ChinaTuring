using System;
using System.Collections.Generic;
using System.Linq;
using ReflectionMagic;

namespace UniCompiler.CSharpCompiler
{
	public static class TypeHelper
    {
        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(double),
            typeof(decimal),
            typeof(long),
            typeof(short),
            typeof(sbyte),
            typeof(byte),
            typeof(ulong),
            typeof(ushort),
            typeof(uint),
            typeof(float)
        };

        public static bool IsNumeric(this Type type)
        {
            return NumericTypes.Contains(Nullable.GetUnderlyingType(type) ?? type);
        }

        public static bool ImplementsGenericType(this Type type, Type genericType)
        {
            return type.GetInterfaces().Any((Type i) => i.IsGenericType && i.GetGenericTypeDefinition() == genericType);
        }

        public static bool IsCollectionOrDictionary(this Type type)
        {
            if (!ImplementsGenericType(type, typeof(ICollection<>)))
            {
                return ImplementsGenericType(type, typeof(IDictionary<,>));
            }
            return true;
        }

        public static object Unwrap(this object o)
        {
            PrivateReflectionDynamicObjectBase privateReflectionDynamicObjectBase = o as PrivateReflectionDynamicObjectBase;
            if (privateReflectionDynamicObjectBase != null)
            {
                return privateReflectionDynamicObjectBase.RealObject;
            }
            return o;
        }
    }
}
