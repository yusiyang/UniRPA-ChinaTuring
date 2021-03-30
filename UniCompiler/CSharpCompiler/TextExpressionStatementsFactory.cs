using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public static class TextExpressionStatementsFactory
	{
		private static readonly SyntaxList<ArrayRankSpecifierSyntax> OmittedArraySize = SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList((ExpressionSyntax)SyntaxFactory.OmittedArraySizeExpression())));

		private static readonly ThisExpressionSyntax This = SyntaxFactory.ThisExpression();

		private static readonly ArrayCreationExpressionSyntax StringArrayExpression = SyntaxFactory.ArrayCreationExpression(SyntaxFactory.ArrayType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword))).WithRankSpecifiers(OmittedArraySize));

		private static readonly ArrayCreationExpressionSyntax AssemblyReferenceArrayExpression = SyntaxFactory.ArrayCreationExpression(SyntaxFactory.ArrayType(typeof(AssemblyReference).GetGenericTypeName(new ObjectFactoryContext())).WithRankSpecifiers(OmittedArraySize));

		public static ExpressionStatementSyntax SetNamespaceForImplementation(IEnumerable<string> namespaces, ObjectFactoryContext context)
		{
			IEnumerable<LiteralExpressionSyntax> nodes = namespaces.Select((string x) => ActivityFactoryCache.NamespacesForImplementationCache.GetOrAdd(x, (string newNamespace) => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(newNamespace))));
			ArrayCreationExpressionSyntax expression = StringArrayExpression.WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList((IEnumerable<ExpressionSyntax>)nodes)));
			return SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, typeof(TextExpression).GetGenericTypeName(context), ObjectFactoryCache.IdentifiersCache.GetOrAdd("SetNamespacesForImplementation")), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new ArgumentSyntax[2]
			{
			SyntaxFactory.Argument(This),
			SyntaxFactory.Argument(expression)
			}))));
		}

		public static ExpressionStatementSyntax SetReferencesForImplementation(IEnumerable<string> assemblyNames, ObjectFactoryContext context)
		{
			IEnumerable<ObjectCreationExpressionSyntax> nodes = assemblyNames.Select((string x) => ActivityFactoryCache.AssemblyNamesForImplementationCache.GetOrAdd(x, (string newAssemblyName) => SyntaxFactory.ObjectCreationExpression(typeof(AssemblyReference).GetGenericTypeName(context)).WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SingletonSeparatedList((ExpressionSyntax)SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ObjectFactoryCache.IdentifiersCache.GetOrAdd("AssemblyName"), SyntaxFactory.ObjectCreationExpression(typeof(AssemblyName).GetGenericTypeName(context)).WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(newAssemblyName))))))))))));
			ArrayCreationExpressionSyntax expression = AssemblyReferenceArrayExpression.WithInitializer(SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, SyntaxFactory.SeparatedList((IEnumerable<ExpressionSyntax>)nodes)));
			return SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, typeof(TextExpression).GetGenericTypeName(context), ObjectFactoryCache.IdentifiersCache.GetOrAdd("SetReferencesForImplementation")), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new ArgumentSyntax[2]
			{
			SyntaxFactory.Argument(This),
			SyntaxFactory.Argument(expression)
			}))));
		}
	}
}
