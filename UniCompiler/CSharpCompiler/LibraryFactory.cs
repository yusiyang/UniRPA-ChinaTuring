using System;
using System.Activities;
using System.Activities.Expressions;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.VisualBasic.Activities;
using ReflectionMagic;

namespace UniCompiler.CSharpCompiler
{
	public class LibraryFactory
	{
		private readonly ConcurrentBag<ConcurrentDictionary<(string, Type), ObjectCreationExpressionSyntax>> _inArgumentsCachePool = new ConcurrentBag<ConcurrentDictionary<(string, Type), ObjectCreationExpressionSyntax>>();

		private GenerateLibraryRequest _generateLibraryRequest;

		public ConcurrentDictionary<Assembly, bool> Assemblies
		{
			get;
		} = new ConcurrentDictionary<Assembly, bool>();


		public ConcurrentBag<SyntaxTree> Build(GenerateLibraryRequest generateLibraryRequest)
		{
			_generateLibraryRequest = generateLibraryRequest;
			ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>
		{
			AssemblyInfoFactory.GenerateAssemblyInfo(generateLibraryRequest.AssemblyVersion)
		};
			ConcurrentBag<ActivityFactory> activityFactoryPool = new ConcurrentBag<ActivityFactory>();
			ConcurrentBag<ObjectFactoryContext> contextPool = new ConcurrentBag<ObjectFactoryContext>();
			ConcurrentBag<List<ObjectFactoryContext>> contextListPool = new ConcurrentBag<List<ObjectFactoryContext>>();
			Parallel.For(0, Math.Max(1, Environment.ProcessorCount - 1), (Action<int>)delegate
			{
				Lazy<GenerateActivityRequest> result2;
				List<ObjectFactoryContext> contextList2 = default(List<ObjectFactoryContext>);
				GenerateActivityRequest generateActivityRequest = default(GenerateActivityRequest);
				while (generateLibraryRequest.ActivitiesToCompile.TryDequeue(out result2))
				{
					contextList2 = (contextListPool.TryTake(out List<ObjectFactoryContext> result3) ? result3 : new List<ObjectFactoryContext>());
					ActivityFactory result4;
					ActivityFactory activityFactory = activityFactoryPool.TryTake(out result4) ? result4 : new ActivityFactory(() => objectContextFactory(contextList2));
					generateActivityRequest = result2.Value;
					foreach (CompilationUnitSyntax item in activityFactory.Build(generateActivityRequest, (object o, ObjectFactoryContext factoryContext) => Hook(generateActivityRequest.AbsolutePath, o, factoryContext)))
					{
						syntaxTrees.Add(item.SyntaxTree);
					}
					foreach (Assembly assembly in activityFactory.Assemblies)
					{
						Assemblies.TryAdd(assembly, value: true);
					}
					foreach (ObjectFactoryContext item2 in contextList2)
					{
						item2.ClearContext();
						contextPool.Add(item2);
						if (LibraryFactoryCache.InArgumentsCache.TryRemove(item2, out ConcurrentDictionary<(string, Type), ObjectCreationExpressionSyntax> value))
						{
							value.Clear();
							_inArgumentsCachePool.Add(value);
						}
					}
					contextList2.Clear();
					contextListPool.Add(contextList2);
					activityFactory.Clear();
					activityFactoryPool.Add(activityFactory);
					generateActivityRequest.ActivityBuilder = null;
				}
			});
			return syntaxTrees;
			ObjectFactoryContext objectContextFactory(List<ObjectFactoryContext> contextList)
			{
				ObjectFactoryContext result;
				ObjectFactoryContext objectFactoryContext = contextPool.TryTake(out result) ? result : new ObjectFactoryContext();
				contextList.Add(objectFactoryContext);
				return objectFactoryContext;
			}
		}

		public void Compile(ConcurrentBag<SyntaxTree> syntaxTrees, GenerateLibraryRequest generateLibraryRequest, MemoryStream targetMemoryStream)
		{
			IEnumerable<PortableExecutableReference> references = Assemblies.Keys.Select((Assembly x) => MetadataReference.CreateFromFile(x.Location).WithAliases(AssemblyAlias.GetAliases(x)));
			EmitResult emitResult = CSharpCompilation.Create(generateLibraryRequest.LibraryName, syntaxTrees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: false, null, null, null, null, OptimizationLevel.Release)).Emit(targetMemoryStream, null, null, null, generateLibraryRequest.AssemblyResources);
			if (emitResult.Success)
			{
				return;
			}
			IEnumerable<string> source = emitResult.Diagnostics.Select((Diagnostic s) => s.GetMessage());
			//throw new CSharpCompilationException(Logger.FormatErrorList(Resources.CodeCompiler_CompileCode_Compile_Errors, source.ToList()));
			throw new Exception(source.ToString());
		}

		private ExpressionSyntax Hook(string path, object @object, ObjectFactoryContext context)
		{
			Type type = @object.GetType();
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(InArgument<>))
			{
				Activity activity = (Activity)@object.AsDynamic().Expression;
				Type activityType = activity?.GetType();
				if (activityType != null && activityType.IsGenericType && activityType.GetGenericTypeDefinition() == typeof(VisualBasicValue<>))
				{
					IdentifierNameSyntax identifierNameSyntax = (IdentifierNameSyntax)ObjectFactory.GetFactorySyntax((string)activity.AsDynamic().ExpressionText, context, null).SyntaxTree.GetRoot();
					ConcurrentDictionary<(string, Type), ObjectCreationExpressionSyntax> result;
					return LibraryFactoryCache.InArgumentsCache.GetOrAdd(context, (ObjectFactoryContext _) => (!_inArgumentsCachePool.TryTake(out result)) ? new ConcurrentDictionary<(string, Type), ObjectCreationExpressionSyntax>() : result).GetOrAdd((identifierNameSyntax.Identifier.ValueText, type), delegate ((string, Type) newInArgument)
					{
						string item2 = newInArgument.Item1;
						ObjectCreationExpressionSyntax expression = SyntaxFactory.ObjectCreationExpression(activityType.GetGenericTypeName(context), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new ArgumentSyntax[1]
						{
						SyntaxFactory.Argument(ObjectFactoryCache.IdentifiersCache.GetOrAdd(item2))
						})), null);
						return SyntaxFactory.ObjectCreationExpression(type.GetGenericTypeName(context), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new ArgumentSyntax[1]
						{
						SyntaxFactory.Argument(expression).WithNameColon(SyntaxFactory.NameColon(ObjectFactoryCache.IdentifiersCache.GetOrAdd("expression")))
						})), null);
					});
				}
			}
			else if (type.Name == "InvokeWorkflowFile")
			{
				Literal<string> literal = ((InArgument<string>)@object.AsDynamic().WorkflowFileName).Expression as Literal<string>;
				string item = _generateLibraryRequest.PathsToClassNames[path].classNamespace;
				string fullPath = Path.GetFullPath(Path.Combine(_generateLibraryRequest.RootPath, literal.Value));
				(string, string) valueTuple = _generateLibraryRequest.PathsToClassNames[fullPath];
				IDictionary dictionary = (IDictionary)@object.AsDynamic().Arguments.RealObject;
				SeparatedSyntaxList<ExpressionSyntax> expressions = default(SeparatedSyntaxList<ExpressionSyntax>);
				foreach (string item3 in dictionary.Keys.OfType<string>())
				{
					ExpressionSyntax factorySyntax = ObjectFactory.GetFactorySyntax(dictionary[item3], context, (object o, ObjectFactoryContext c) => Hook(path, o, c), hasSetter: true);
					expressions = expressions.Add(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ObjectFactoryCache.IdentifiersCache.GetOrAdd(item3), factorySyntax));
				}
				InitializerExpressionSyntax initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, expressions);
				string namespaceToInvoke = GetNamespaceToInvoke(item, valueTuple.Item1);
				string typeName = string.IsNullOrEmpty(namespaceToInvoke) ? valueTuple.Item2 : (namespaceToInvoke + "." + valueTuple.Item2);
				return SyntaxFactory.ObjectCreationExpression(ObjectFactory.GetQualifiedName(null, typeName)).WithInitializer(initializer);
			}
			return null;
		}

		private static string GetNamespaceToInvoke(string fromNamespace, string toNamespace)
		{
			string[] first = fromNamespace.Split('.');
			string[] array = toNamespace.Split('.');
			int count = ((IEnumerable<string>)first).Zip((IEnumerable<string>)array, (Func<string, string, (string, string)>)((string from, string to) => (from, to))).TakeWhile((Func<(string, string), bool>)(((string from, string to) pair) => pair.from == pair.to)).Count();
			IEnumerable<string> values = array.Skip(count);
			return string.Join(".", values);
		}
	}
}
