using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xaml;

namespace UniCompiler
{
	public class XamlTypeResolver : XamlSchemaContext
    {
        private readonly List<Assembly> _fakeAssemblies = new List<Assembly>();

        public void AddFakeAssembly(Assembly assembly)
        {
            _fakeAssemblies.Add(assembly);
        }

        protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            XamlType xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);
            if (xamlType != null)
            {
                return xamlType;
            }
            Type type = FindTypeInFakeAssemblies(xamlNamespace, name);
            if (type != null)
            {
                xamlType = GetXamlType(type);
                if (xamlType != null)
                {
                    return xamlType;
                }
            }
            if (xamlNamespace != "clr-namespace:")
            {
                //Logger.WriteLine(string.Format(Resources.XamlTypeResolver_GetXamlType_Could_not_resolve_type_from_namespace_, name, xamlNamespace), null, TraceEventType.Warning);
            }
            return base.GetXamlType(xamlNamespace, name, typeArguments);
        }

        private Type FindTypeInFakeAssemblies(string xamlNamespace, string name)
        {
            string nameSpace = xamlNamespace.Replace("clr-namespace:", string.Empty);
            int num = nameSpace.IndexOf(";assembly");
            if (num > 0)
            {
                nameSpace = nameSpace.Substring(0, num);
            }
            foreach (Assembly fakeAssembly in _fakeAssemblies)
            {
                TypeInfo typeInfo = fakeAssembly.DefinedTypes.FirstOrDefault((TypeInfo s) => s.Name == name && s.Namespace == nameSpace);
                if (typeInfo != null)
                {
                    return typeInfo;
                }
            }
            return null;
        }
    }
}
