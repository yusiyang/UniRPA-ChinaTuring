using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.CodeCompletion
{
    public class VBCompilation:IDisposable
    {
        private VisualBasicCompilation _visualBasicCompilation;

        public VisualBasicCompilation VisualBasicCompilation => _visualBasicCompilation;

        private object _lockObj = new object();

        private Assembly _assembly;

        public VBCompilation(string assemblyName)
        {
            _visualBasicCompilation = VisualBasicCompilation.Create(assemblyName);
        }

        ~VBCompilation()
        {
            Dispose();
        }

        public async Task AddReferencesAsync(Assembly assembly)
        {
            await Task.Run(() => 
            {
                lock (_lockObj)
                {
                    if (_assembly == assembly)
                    {
                        return;
                    }

                    _visualBasicCompilation = _visualBasicCompilation.RemoveAllReferences();

                    var references = AssemblyHelper.GetAllDependencies(assembly);

                    foreach (var reference in references)
                    {
                        _visualBasicCompilation = _visualBasicCompilation.AddReferences(MetadataReference.CreateFromFile(reference.Location));
                    }
                    _assembly = assembly;
                }
                
            });
        }

        public void RemoveAllReferences()
        {
            lock (_lockObj)
            {
                _visualBasicCompilation = _visualBasicCompilation.RemoveAllReferences();
                _assembly = null;
            }
        }

        public async Task AddSyntaxTreesAsync(params SyntaxTree[] trees)
        {
            await Task.Run(() =>
            {
                AddSyntaxTrees(trees);
            });
        }

        public void AddSyntaxTrees(params SyntaxTree[] trees)
        {
            lock (_lockObj)
            {
                _visualBasicCompilation = _visualBasicCompilation.RemoveAllSyntaxTrees();
                _visualBasicCompilation = _visualBasicCompilation.AddSyntaxTrees(trees);
            }
        }

        public void RemoveAllSyntaxTrees()
        {
            lock (_lockObj)
            {
                _visualBasicCompilation = _visualBasicCompilation.RemoveAllSyntaxTrees();
            }
        }

        public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
        {
            lock (_lockObj)
            {
                return _visualBasicCompilation.GetSemanticModel(syntaxTree);
                }
        }

        public void Dispose()
        {
            RemoveAllReferences();
            RemoveAllSyntaxTrees();
            _visualBasicCompilation = null;
            GC.SuppressFinalize(this);
        }
    }
}
