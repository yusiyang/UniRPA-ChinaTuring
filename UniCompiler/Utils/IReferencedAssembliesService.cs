using System.Activities.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace UniCompiler.Utils
{
	internal interface IReferencedAssembliesService
    {
        void AddAssemblies(IEnumerable<AssemblyReference> assemblyReferences);

        Assembly[] GetAllReferences();
    }
}
