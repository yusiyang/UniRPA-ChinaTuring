using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public static class ObjectFactoryCache
	{
		private static long TypeCount;

		public static ConcurrentDictionary<Type, ObjectCreationExpressionSyntax> ObjectCreationExpressionCache
		{
			get;
		} = new ConcurrentDictionary<Type, ObjectCreationExpressionSyntax>();


		public static ConcurrentDictionary<Type, TypeSyntax> GenericNamesCache
		{
			get;
		} = new ConcurrentDictionary<Type, TypeSyntax>();


		public static ConcurrentDictionary<(string assemblyAlias, string name), NameSyntax> QualifiedNamesCache
		{
			get;
		} = new ConcurrentDictionary<(string, string), NameSyntax>();


		public static ConcurrentDictionary<Type, List<PropertyInfo>> TypePropertiesCache
		{
			get;
		} = new ConcurrentDictionary<Type, List<PropertyInfo>>();


		public static ConcurrentDictionary<Enum, ExpressionSyntax> EnumsCache
		{
			get;
		} = new ConcurrentDictionary<Enum, ExpressionSyntax>();


		public static ConcurrentDictionary<string, ExpressionSyntax> NumbersCache
		{
			get;
		} = new ConcurrentDictionary<string, ExpressionSyntax>();


		public static ConcurrentDictionary<string, IdentifierNameSyntax> IdentifiersCache
		{
			get;
		} = new ConcurrentDictionary<string, IdentifierNameSyntax>();


		public static ConcurrentDictionary<Type, long> ObjectTypeIdGenerator
		{
			get;
		} = new ConcurrentDictionary<Type, long>();


		public static ConcurrentDictionary<(string variableName, string propertyName), MemberAccessExpressionSyntax> VariablePropertiesCache
		{
			get;
		} = new ConcurrentDictionary<(string, string), MemberAccessExpressionSyntax>();


		public static ConcurrentDictionary<string, LocalDeclarationStatementSyntax> LocalVariablesStatements
		{
			get;
		} = new ConcurrentDictionary<string, LocalDeclarationStatementSyntax>();


		public static long GetOrAdd(this ConcurrentDictionary<Type, long> typeObjectMap, Type key)
		{
			return typeObjectMap.GetOrAdd(key, (Type newType) => Interlocked.Increment(ref TypeCount));
		}

		public static IdentifierNameSyntax GetOrAdd(this ConcurrentDictionary<string, IdentifierNameSyntax> cache, string key)
		{
			return cache.GetOrAdd(key, SyntaxFactory.IdentifierName);
		}

		public static LocalDeclarationStatementSyntax GetOrAdd(this ConcurrentDictionary<string, LocalDeclarationStatementSyntax> variableStatementMap, string variableName, Type variableType, ObjectFactoryContext context)
		{
			ObjectCreationExpressionSyntax newObjectExpression = ObjectCreationExpressionCache.GetOrAdd(variableType, (Type typeToBeAdded) => SyntaxFactory.ObjectCreationExpression(typeToBeAdded.GetGenericTypeName(context), null, null));
			return variableStatementMap.GetOrAdd(variableName, (string variableToBeAdded) => ObjectFactory.Variable(IdentifiersCache.GetOrAdd(variableToBeAdded), newObjectExpression));
		}

		public static void ClearCache()
		{
			TypePropertiesCache.Clear();
			ObjectCreationExpressionCache.Clear();
			GenericNamesCache.Clear();
			QualifiedNamesCache.Clear();
			EnumsCache.Clear();
			NumbersCache.Clear();
			IdentifiersCache.Clear();
			ObjectTypeIdGenerator.Clear();
			VariablePropertiesCache.Clear();
			LocalVariablesStatements.Clear();
		}
	}

}
