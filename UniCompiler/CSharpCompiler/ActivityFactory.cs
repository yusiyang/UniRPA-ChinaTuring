using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Annotations;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public class ActivityFactory
	{
		private readonly IdentifierNameSyntax _getImplementationMethodName = ObjectFactoryCache.IdentifiersCache.GetOrAdd("GetImplementation");

		private readonly IdentifierNameSyntax _activityImplementation = ObjectFactoryCache.IdentifiersCache.GetOrAdd("Implementation");

		private readonly Func<ObjectFactoryContext> _objectContextFactory;

		public HashSet<Assembly> Assemblies
		{
			get;
		} = new HashSet<Assembly>();


		public ActivityFactory(Func<ObjectFactoryContext> objectContextFactory)
		{
			_objectContextFactory = objectContextFactory;
		}

		public IEnumerable<CompilationUnitSyntax> Build(GenerateActivityRequest request, ObjectFactory.Hook hook = null)
		{
			List<MemberDeclarationSyntax> list = new List<MemberDeclarationSyntax>();
			list.AddRange(BuildActivityProperties(request.ActivityBuilder));
			list.Add(BuildConstructor(request.ClassName, request.ActivityBuilder, hook, request.CompileExpressions));
			list.Add(BuildActivityFactoryMethod(request.ActivityBuilder, hook));
			CompilationUnitSyntax compilationUnitSyntax = SyntaxFactory.CompilationUnit().WithExterns(new SyntaxList<ExternAliasDirectiveSyntax>(Assemblies.Select((Assembly x) => SyntaxFactory.ExternAliasDirective(AssemblyAlias.GetAliasFor(x)))));
			NamespaceDeclarationSyntax namespaceDeclarationSyntax = SyntaxFactory.NamespaceDeclaration(ObjectFactory.GetQualifiedName(null, request.ClassNamespace));
			ClassDeclarationSyntax classDeclarationSyntax = BuildClassDeclarationSyntax(request, list);
			namespaceDeclarationSyntax = namespaceDeclarationSyntax.AddMembers(classDeclarationSyntax);
			compilationUnitSyntax = compilationUnitSyntax.AddMembers(namespaceDeclarationSyntax);
			if (request.CompileExpressions)
			{
				(CompilationUnitSyntax, List<Assembly>) partialClassForExpressions = ExpressionsCompiler.GetPartialClassForExpressions(request.ActivityBuilder, request.ClassNamespace, request.ClassName);
				CompilationUnitSyntax expressionsActivitySyntax = partialClassForExpressions.Item1;
				foreach (Assembly item in partialClassForExpressions.Item2)
				{
					Assemblies.Add(item);
				}
				yield return compilationUnitSyntax;
				yield return expressionsActivitySyntax;
			}
			else
			{
				yield return compilationUnitSyntax;
			}
		}

		public void Clear()
		{
			Assemblies.Clear();
		}

		private ClassDeclarationSyntax BuildClassDeclarationSyntax(GenerateActivityRequest request, List<MemberDeclarationSyntax> classMembers)
		{
			ClassDeclarationSyntax classDeclarationSyntax = SyntaxFactory.ClassDeclaration(request.ClassName).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
			if (request.CompileExpressions)
			{
				classDeclarationSyntax = classDeclarationSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
			}
			ObjectFactoryContext objectFactoryContext = _objectContextFactory();
			Type typeFromHandle = typeof(Activity);
			SeparatedSyntaxList<AttributeSyntax> attributes = BuildClassAttributes(request, objectFactoryContext);
			classDeclarationSyntax = classDeclarationSyntax.AddBaseListTypes(SyntaxFactory.SimpleBaseType(typeFromHandle.GetGenericTypeName(objectFactoryContext))).AddMembers(classMembers.ToArray()).WithAttributeLists(new SyntaxList<AttributeListSyntax>(SyntaxFactory.AttributeList(attributes)));
			UpdateActivityFactory(objectFactoryContext);
			return classDeclarationSyntax;
		}

		private SeparatedSyntaxList<AttributeSyntax> BuildClassAttributes(GenerateActivityRequest request, ObjectFactoryContext context)
		{
			List<AttributeSyntax> list = new List<AttributeSyntax>();
			list.Add(BuildAttribute(typeof(BrowsableAttribute), SyntaxFactory.LiteralExpression(request.BrowsableAttribute ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression), context));
			if (!string.IsNullOrEmpty(request.CategoryAttribute))
			{
				list.Add(buildAttributeWithString(typeof(CategoryAttribute), request.CategoryAttribute, context));
			}
			if (!string.IsNullOrEmpty(request.DisplayNameAttribute))
			{
				list.Add(buildAttributeWithString(typeof(DisplayNameAttribute), request.DisplayNameAttribute, context));
			}
			if (!string.IsNullOrEmpty(request.DescriptionAttribute))
			{
				list.Add(buildAttributeWithString(typeof(DescriptionAttribute), request.DescriptionAttribute, context));
			}
			return SyntaxFactory.SeparatedList(list);
			AttributeSyntax buildAttributeWithString(Type attributeType, string parameter, ObjectFactoryContext _context)
			{
				return BuildAttribute(attributeType, SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(parameter)), _context);
			}
		}

		private ConstructorDeclarationSyntax BuildConstructor(string className, ActivityBuilder builder, ObjectFactory.Hook hook, bool requiresExpressionsCompilation)
		{
			List<string> namespaces = TextExpression.GetNamespacesForImplementation(builder).ToList();
			List<AssemblyReference> list = (from x in TextExpression.GetReferencesForImplementation(builder)
											where x?.AssemblyName != null
											select x).ToList();
			foreach (AssemblyReference item2 in list)
			{
				if (item2.Assembly == null)
				{
					item2.LoadAssembly();
					if (item2.Assembly == null)
					{
						continue;
					}
				}
				Assemblies.Add(item2.Assembly);
			}
			List<StatementSyntax> list2 = new List<StatementSyntax>();
			ObjectFactoryContext defaultValuesContext = _objectContextFactory();
			List<ExpressionStatementSyntax> collection = (from x in builder.Properties
														  where x.Value != null
														  select SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ObjectFactoryCache.IdentifiersCache.GetOrAdd(x.Name), ObjectFactory.GetFactorySyntax(x.Value, defaultValuesContext, hook, hasSetter: true)))).ToList();
			list2.AddRange(defaultValuesContext.VariablesStatements);
			list2.AddRange(defaultValuesContext.PropertiesAssignmentsStatements);
			list2.AddRange(collection);
			list2.Add(TextExpressionStatementsFactory.SetNamespaceForImplementation(namespaces, defaultValuesContext));
			list2.Add(TextExpressionStatementsFactory.SetReferencesForImplementation(list.Select((AssemblyReference x) => x.AssemblyName.FullName), defaultValuesContext));
			list2.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, _activityImplementation, _getImplementationMethodName)));
			if (requiresExpressionsCompilation)
			{
				ExpressionStatementSyntax item = CompiledExpressionInvokerStatementsFactory.SetCompiledExpressionRootForImplementation(defaultValuesContext);
				list2.Add(item);
			}
			UpdateActivityFactory(defaultValuesContext);
			return SyntaxFactory.ConstructorDeclaration(className).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).WithBody(SyntaxFactory.Block(list2));
		}

		private IEnumerable<PropertyDeclarationSyntax> BuildActivityProperties(ActivityBuilder builder)
		{
			ObjectFactoryContext context = _objectContextFactory();
			return builder.Properties.Select(delegate (DynamicActivityProperty prop)
			{
				PropertyDeclarationSyntax propertyDeclarationSyntax = BuildGetSetProperty(prop.Name, prop.Type);
				List<AttributeSyntax> list = new List<AttributeSyntax>();
				string annotationText = Annotation.GetAnnotationText(prop);
				AttributeSyntax categoryAttributeForProperty = PropertyTypeToCategoryAttributeMap.GetCategoryAttributeForProperty(prop.Type);
				list.Add(categoryAttributeForProperty);
				if (annotationText != null)
				{
					list.Add(BuildAttribute(parameter: SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(annotationText)), attributeType: typeof(DescriptionAttribute), context: context));
				}
				if (prop.Attributes.Count > 0)
				{
					list.AddRange(prop.Attributes.Select((Attribute a) => SyntaxFactory.Attribute((NameSyntax)a.GetType().GetGenericTypeName(context))));
				}
				return propertyDeclarationSyntax.WithAttributeLists(new SyntaxList<AttributeListSyntax>(SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(list))));
			});
		}

		private static IEnumerable<Type> GetImplementationDefaultNamespaces()
		{
			yield return typeof(Activity);
			yield return typeof(TypeDescriptor);
			yield return typeof(AssemblyName);
			yield return typeof(IDictionary<,>);
			yield return typeof(TextExpression);
		}

		private MethodDeclarationSyntax BuildActivityFactoryMethod(ActivityBuilder builder, ObjectFactory.Hook hook)
		{
			ObjectFactoryContext objectFactoryContext = _objectContextFactory();
			foreach (Type implementationDefaultNamespace in GetImplementationDefaultNamespaces())
			{
				objectFactoryContext.Assemblies.Add(implementationDefaultNamespace.Assembly);
			}
			ReturnStatementSyntax element = SyntaxFactory.ReturnStatement((builder.Implementation != null) ? ObjectFactory.GetFactorySyntax(builder.Implementation, objectFactoryContext, hook) : SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression));
			IEnumerable<StatementSyntax> statements = objectFactoryContext.VariablesStatements.Concat(objectFactoryContext.PropertiesAssignmentsStatements).Concat(Enumerable.Repeat(element, 1));
			MethodDeclarationSyntax result = SyntaxFactory.MethodDeclaration(typeof(Activity).GetGenericTypeName(objectFactoryContext), _getImplementationMethodName.Identifier).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)).WithBody(SyntaxFactory.Block(statements));
			UpdateActivityFactory(objectFactoryContext);
			return result;
		}

		private PropertyDeclarationSyntax BuildGetSetProperty(string propertyName, Type propertyType)
		{
			ObjectFactoryContext objectFactoryContext = _objectContextFactory();
			PropertyDeclarationSyntax result = SyntaxFactory.PropertyDeclaration(propertyType.GetGenericTypeName(objectFactoryContext), propertyName).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)).AddAccessorListAccessors(SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)), SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
			UpdateActivityFactory(objectFactoryContext);
			return result;
		}

		private static AttributeSyntax BuildAttribute(Type attributeType, LiteralExpressionSyntax parameter, ObjectFactoryContext context)
		{
			return SyntaxFactory.Attribute((NameSyntax)attributeType.GetGenericTypeName(context), SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(parameter))));
		}

		private void UpdateActivityFactory(ObjectFactoryContext objectContext)
		{
			foreach (Assembly assembly in objectContext.Assemblies)
			{
				Assemblies.Add(assembly);
			}
		}
	}
}
