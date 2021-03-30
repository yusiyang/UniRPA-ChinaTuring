using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ReflectionMagic;

namespace UniCompiler.CSharpCompiler
{
	public static class ObjectFactory
	{
		public delegate ExpressionSyntax Hook(object @object, ObjectFactoryContext context);

		private static readonly IdentifierNameSyntax VarIdentifier = SyntaxFactory.IdentifierName("var");

		private static readonly SyntaxList<ArrayRankSpecifierSyntax> OmittedArraySize = SyntaxFactory.SingletonList(SyntaxFactory.ArrayRankSpecifier(SyntaxFactory.SingletonSeparatedList((ExpressionSyntax)SyntaxFactory.OmittedArraySizeExpression())));

		private static readonly LiteralExpressionSyntax Default = SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression, SyntaxFactory.Token(SyntaxKind.DefaultKeyword));

		public static LocalDeclarationStatementSyntax Variable(IdentifierNameSyntax name, ExpressionSyntax value)
		{
			return SyntaxFactory.LocalDeclarationStatement(SyntaxFactory.VariableDeclaration(VarIdentifier).WithVariables(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.VariableDeclarator(name.Identifier).WithInitializer(SyntaxFactory.EqualsValueClause(value)))));
		}

		public static void AppendVariable(IdentifierNameSyntax varName, Type typeofValue, ExpressionSyntax varValue, ObjectFactoryContext context)
		{
			AppendVariable(Variable(varName, varValue), typeofValue, context);
		}

		public static void AppendVariable(LocalDeclarationStatementSyntax localVarStatement, Type typeofValue, ObjectFactoryContext context)
		{
			context.VariablesStatements.Add(localVarStatement);
			context.Assemblies.Add(typeofValue.Assembly);
		}

		public static ExpressionSyntax ToCollectionLiteral(this IEnumerable enumerable, ObjectFactoryContext context, Hook hook, bool hasSetter)
		{
			if (!hasSetter && !enumerable.GetEnumerator().MoveNext())
			{
				return null;
			}
			SeparatedSyntaxList<ExpressionSyntax> expressions = default(SeparatedSyntaxList<ExpressionSyntax>);
			foreach (object item in enumerable)
			{
				expressions = expressions.Add(GetFactorySyntaxOrDefault(item, context, hook));
			}
			InitializerExpressionSyntax initializerExpressionSyntax = SyntaxFactory.InitializerExpression(SyntaxKind.CollectionInitializerExpression, expressions);
			if (!hasSetter)
			{
				return initializerExpressionSyntax;
			}
			return SyntaxFactory.ObjectCreationExpression(enumerable.GetType().GetGenericTypeName(context), null, initializerExpressionSyntax);
		}

		public static ExpressionSyntax ToArrayLiteral(this IEnumerable enumerable, ObjectFactoryContext context, Hook hook)
		{
			SeparatedSyntaxList<ExpressionSyntax> expressions = default(SeparatedSyntaxList<ExpressionSyntax>);
			foreach (object item in enumerable)
			{
				expressions = expressions.Add(GetFactorySyntaxOrDefault(item, context, hook));
			}
			InitializerExpressionSyntax initializer = SyntaxFactory.InitializerExpression(SyntaxKind.ArrayInitializerExpression, expressions);
			return SyntaxFactory.ArrayCreationExpression((ArrayTypeSyntax)enumerable.GetType().GetGenericTypeName(context)).WithInitializer(initializer);
		}

		public static ExpressionSyntax ToDictionaryLiteral(this IEnumerable dictionary, ObjectFactoryContext context, Hook hook, bool hasSetter)
		{
			if (!hasSetter && !dictionary.GetEnumerator().MoveNext())
			{
				return null;
			}
			List<ExpressionSyntax> list = new List<ExpressionSyntax>();
			foreach (object item in dictionary)
			{
				List<ExpressionSyntax> list2 = new List<ExpressionSyntax>();
				dynamic val = item.AsDynamic();
				list2.Add(GetFactorySyntax(((object)val.Key).Unwrap(), context, hook));
				list2.Add(GetFactorySyntaxOrDefault(((object)val.Value).Unwrap(), context, hook));
				list.Add(SyntaxFactory.InitializerExpression(SyntaxKind.ComplexElementInitializerExpression, SyntaxFactory.SeparatedList(list2)));
			}
			InitializerExpressionSyntax initializerExpressionSyntax = SyntaxFactory.InitializerExpression(SyntaxKind.CollectionInitializerExpression, SyntaxFactory.SeparatedList(list));
			if (!hasSetter)
			{
				return initializerExpressionSyntax;
			}
			return SyntaxFactory.ObjectCreationExpression(dictionary.GetType().GetGenericTypeName(context), null, initializerExpressionSyntax);
		}

		public static ExpressionSyntax ToEnumLiteral(Enum value, ObjectFactoryContext context)
		{
			context.Assemblies.Add(value.GetType().Assembly);
			return ObjectFactoryCache.EnumsCache.GetOrAdd(value, delegate (Enum newValue)
			{
				Type type = newValue.GetType();
				TypeSyntax genericTypeName = type.GetGenericTypeName(context);
				return type.GetCustomAttributes<FlagsAttribute>().Any() ? ToFlagEnumLiteral(newValue, type, genericTypeName, context) : SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, genericTypeName, ObjectFactoryCache.IdentifiersCache.GetOrAdd(Enum.GetName(type, newValue)));
			});
		}

		public static ExpressionSyntax ToFlagEnumLiteral(Enum value, Type typeofValue, TypeSyntax enumTypeName, ObjectFactoryContext context)
		{
			List<MemberAccessExpressionSyntax> list = new List<MemberAccessExpressionSyntax>();
			foreach (Enum value2 in Enum.GetValues(typeofValue))
			{
				if (value.HasFlag(value2))
				{
					list.Add(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, enumTypeName, ObjectFactoryCache.IdentifiersCache.GetOrAdd(Enum.GetName(typeofValue, value2))));
				}
			}
			if (list.Count > 1)
			{
				return BuildBitwiseOrBinaryExpression(list, 0);
			}
			return list[0];
			ExpressionSyntax BuildBitwiseOrBinaryExpression(List<MemberAccessExpressionSyntax> members, int index)
			{
				if (index == members.Count - 1)
				{
					return members[index];
				}
				return SyntaxFactory.BinaryExpression(SyntaxKind.BitwiseOrExpression, members[index], BuildBitwiseOrBinaryExpression(members, index + 1));
			}
		}

		public static ExpressionSyntax ToObjectLiteral(this object obj, ObjectFactoryContext context, Hook hook)
		{
			Type type = obj.GetType();
			if (!context.VariablesCount.TryGetValue(type, out int value))
			{
				value = 0;
				context.VariablesCount.Add(type, value);
			}
			string text = $"v_{ObjectFactoryCache.ObjectTypeIdGenerator.GetOrAdd(type)}_{value++}";
			context.VariablesCount[type] = value;
			IdentifierNameSyntax objectVariable = ObjectFactoryCache.IdentifiersCache.GetOrAdd(text);
			context.VariablesMap[obj] = objectVariable;
			(IEnumerable<PropertyInfo> propertiesForCollectionInitialization, IEnumerable<PropertyInfo> restOfProperties) typeProperties = GetTypeProperties(type);
			IEnumerable<PropertyInfo> item = typeProperties.propertiesForCollectionInitialization;
			IEnumerable<PropertyInfo> item2 = typeProperties.restOfProperties;
			List<ExpressionSyntax> list = new List<ExpressionSyntax>();
			foreach (PropertyInfo item4 in item)
			{
				object obj2 = getPropertyValue(item4, obj);
				if (obj2 != null)
				{
					ExpressionSyntax factorySyntax = GetFactorySyntax(obj2, context, hook);
					if (factorySyntax != null)
					{
						list.Add(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ObjectFactoryCache.IdentifiersCache.GetOrAdd(item4.Name), factorySyntax));
					}
				}
			}
			ObjectCreationExpressionSyntax objectCreationExpressionSyntax = null;
			if (list.Count == 0)
			{
				AppendVariable(ObjectFactoryCache.LocalVariablesStatements.GetOrAdd(text, type, context), type, context);
			}
			else
			{
				objectCreationExpressionSyntax = SyntaxFactory.ObjectCreationExpression(type.GetGenericTypeName(context), null, SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SeparatedList(list)));
				AppendVariable(objectVariable, type, objectCreationExpressionSyntax, context);
			}
			foreach (PropertyInfo property in item2)
			{
				object value2 = property.GetValue(obj, BindingFlags.Public, null, null, null);
				if (value2 != null)
				{
					ExpressionSyntax factorySyntax2 = GetFactorySyntax(value2, context, hook, hasSetter: true);
					if (factorySyntax2 != null)
					{
						MemberAccessExpressionSyntax orAdd = ObjectFactoryCache.VariablePropertiesCache.GetOrAdd((text, property.Name), ((string variableName, string propertyName) _) => SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, objectVariable, ObjectFactoryCache.IdentifiersCache.GetOrAdd(property.Name)));
						ExpressionStatementSyntax item3 = SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, orAdd, factorySyntax2));
						context.PropertiesAssignmentsStatements.Add(item3);
					}
				}
			}
			return objectVariable;
			object getPropertyValue(PropertyInfo info, object target)
			{
				return info.GetValue(target, BindingFlags.Public, null, null, null);
			}
		}

		public static ExpressionSyntax ToInlineObjectLiteral(this object obj, ObjectFactoryContext context, Hook hook)
		{
			Type type = obj.GetType();
			(IEnumerable<PropertyInfo> propertiesForCollectionInitialization, IEnumerable<PropertyInfo> restOfProperties) typeProperties = GetTypeProperties(type);
			IEnumerable<PropertyInfo> item = typeProperties.propertiesForCollectionInitialization;
			IEnumerable<PropertyInfo> item2 = typeProperties.restOfProperties;
			List<ExpressionSyntax> initializerExpressions = new List<ExpressionSyntax>();
			foreach (PropertyInfo item3 in item)
			{
				addInitializerExpressionForProperty(item3, hasSetter: false);
			}
			foreach (PropertyInfo item4 in item2)
			{
				addInitializerExpressionForProperty(item4, hasSetter: true);
			}
			return SyntaxFactory.ObjectCreationExpression(type.GetGenericTypeName(context), null, SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression, SyntaxFactory.SeparatedList(initializerExpressions)));
			void addInitializerExpressionForProperty(PropertyInfo property, bool hasSetter)
			{
				object obj2 = getPropertyValue(property, obj);
				if (obj2 != null)
				{
					ExpressionSyntax factorySyntax = GetFactorySyntax(obj2, context, hook, hasSetter);
					if (factorySyntax != null)
					{
						initializerExpressions.Add(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, ObjectFactoryCache.IdentifiersCache.GetOrAdd(property.Name), factorySyntax));
					}
				}
			}
			object getPropertyValue(PropertyInfo info, object target)
			{
				return info.GetValue(target, BindingFlags.Public, null, null, null);
			}
		}

		public static (IEnumerable<PropertyInfo> propertiesForCollectionInitialization, IEnumerable<PropertyInfo> restOfProperties) GetTypeProperties(Type type)
		{
			List<PropertyInfo> orAdd = ObjectFactoryCache.TypePropertiesCache.GetOrAdd(type, (Type newType) => (from p in newType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
																												group p by p.Name into g
																												select g.First()).ToList());
			IEnumerable<PropertyInfo> item = orAdd.Where(delegate (PropertyInfo x)
			{
				MethodInfo setMethod = x.GetSetMethod();
				return ((object)setMethod == null || !setMethod.IsPublic) && x.PropertyType.IsCollectionOrDictionary();
			});
			IEnumerable<PropertyInfo> item2 = orAdd.Where((PropertyInfo x) => x.GetSetMethod()?.IsPublic ?? false);
			return (item, item2);
		}

		public static bool TryGetStringConverter(this Type typeOfvalue, out TypeConverter stringConverter)
		{
			TypeConverter converter = TypeDescriptor.GetConverter(typeOfvalue);
			if (converter != null && converter.CanConvertTo(typeof(string)) && converter != null && converter.CanConvertFrom(typeof(string)))
			{
				stringConverter = converter;
				return true;
			}
			stringConverter = null;
			return false;
		}

		public static NameSyntax GetQualifiedName(string assemblyAlias, string typeName)
		{
			return ObjectFactoryCache.QualifiedNamesCache.GetOrAdd((assemblyAlias, typeName), delegate ((string assemblyAlias, string name) tuple)
			{
				(string assemblyAlias, string name) valueTuple = tuple;
				string item = valueTuple.assemblyAlias;
				string item2 = valueTuple.name;
				int num = item2.LastIndexOf('.');
				if (num >= 0)
				{
					return SyntaxFactory.QualifiedName(GetQualifiedName(item, item2.Substring(0, num)), ObjectFactoryCache.IdentifiersCache.GetOrAdd(item2.Substring(num + 1)));
				}
				return (item == null) ? ((NameSyntax)ObjectFactoryCache.IdentifiersCache.GetOrAdd(item2)) : ((NameSyntax)SyntaxFactory.AliasQualifiedName(ObjectFactoryCache.IdentifiersCache.GetOrAdd(item), ObjectFactoryCache.IdentifiersCache.GetOrAdd(item2)));
			});
		}

		public static TypeSyntax GetGenericTypeName(this Type type, ObjectFactoryContext context)
		{
			context.Assemblies.Add(type.Assembly);
			return ObjectFactoryCache.GenericNamesCache.GetOrAdd(type, delegate (Type typeToBeAdded)
			{
				if (typeToBeAdded.IsGenericType)
				{
					NameSyntax qualifiedName = GetQualifiedName(AssemblyAlias.GetAliasFor(type.Assembly), typeToBeAdded.Namespace);
					List<TypeSyntax> list = (from x in typeToBeAdded.GetGenericArguments()
											 select x.GetGenericTypeName(context)).ToList();
					NameSyntax left = (typeToBeAdded.DeclaringType != null) ? AppendDeclaringTypeNames(typeToBeAdded, qualifiedName, list) : qualifiedName;
					int num2 = typeToBeAdded.Name.IndexOf('`');
					string text2 = (num2 > 0) ? typeToBeAdded.Name.Substring(0, num2) : typeToBeAdded.Name;
					if (num2 <= 0)
					{
						return SyntaxFactory.QualifiedName(left, (SimpleNameSyntax)GetQualifiedName(null, text2));
					}
					return SyntaxFactory.QualifiedName(left, SyntaxFactory.GenericName(ObjectFactoryCache.IdentifiersCache.GetOrAdd(text2).Identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(list))));
				}
				if (typeToBeAdded.IsArray)
				{
					return SyntaxFactory.ArrayType(typeToBeAdded.GetElementType().GetGenericTypeName(context), OmittedArraySize);
				}
				NameSyntax qualifiedName2 = GetQualifiedName(AssemblyAlias.GetAliasFor(type.Assembly), typeToBeAdded.Namespace);
                return SyntaxFactory.QualifiedName((typeToBeAdded.DeclaringType != null) ? AppendDeclaringTypeNames(typeToBeAdded, qualifiedName2, null) : qualifiedName2, (SimpleNameSyntax)GetQualifiedName(null, typeToBeAdded.Name));
			});
			NameSyntax AppendDeclaringTypeNames(Type typeToBeAdded, NameSyntax @namespace, List<TypeSyntax> genericTypeArguments)
			{
				Stack<Type> stack = new Stack<Type>();
				Type declaringType = typeToBeAdded.DeclaringType;
				while (declaringType != null)
				{
					stack.Push(declaringType);
					declaringType = declaringType.DeclaringType;
				}
				NameSyntax nameSyntax = @namespace;
				while (stack.Count > 0)
				{
					Type type2 = stack.Pop();
					int num = type2.Name.IndexOf('`');
					string text = (num > 0) ? type2.Name.Substring(0, num) : type2.Name;
					if (num > 0)
					{
						int count = int.Parse(type2.Name.Substring(num + 1, 1));
						List<TypeSyntax> nodes = genericTypeArguments.Take(count).ToList();
						genericTypeArguments.RemoveRange(0, count);
						nameSyntax = SyntaxFactory.QualifiedName(nameSyntax, SyntaxFactory.GenericName(ObjectFactoryCache.IdentifiersCache.GetOrAdd(text).Identifier, SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(nodes))));
					}
					else
					{
						nameSyntax = SyntaxFactory.QualifiedName(nameSyntax, (SimpleNameSyntax)GetQualifiedName(null, text));
					}
				}
				return nameSyntax;
			}
		}

		public static ExpressionSyntax GetFactorySyntaxOrDefault(object value, ObjectFactoryContext context, Hook hook, bool hasSetter = false)
		{
			return GetFactorySyntax(value, context, hook, hasSetter) ?? Default;
		}

		public static ExpressionSyntax GetFactorySyntax(object value, ObjectFactoryContext context, Hook hook, bool hasSetter = false)
		{
			if (value == null)
			{
				return null;
			}
			Type type = (value is Type) ? typeof(Type) : value.GetType();
			Type type2 = value as Type;
			if ((object)type2 == null)
			{
				string text = value as string;
				if (text == null)
				{
					if (!(value is bool))
					{
						if (value != null)
						{
							if (type.IsNumeric())
							{
								return ObjectFactoryCache.NumbersCache.GetOrAdd(value.ToString(), (string newValue) => SyntaxFactory.ParseExpression(newValue.ToString()));
							}
							if (type.IsEnum)
							{
								return ToEnumLiteral((Enum)value, context);
							}
							if (type.ImplementsGenericType(typeof(IDictionary<,>)))
							{
								return ((IEnumerable)value).ToDictionaryLiteral(context, hook, hasSetter);
							}
							if (type.IsArray)
							{
								return ((IEnumerable)value).ToArrayLiteral(context, hook);
							}
							if (type.ImplementsGenericType(typeof(ICollection<>)))
							{
								return ((IEnumerable)value).ToCollectionLiteral(context, hook, hasSetter);
							}
							if (type.TryGetStringConverter(out TypeConverter stringConverter))
							{
								object obj = stringConverter.ConvertTo(value, typeof(string));
								string aliasFor = AssemblyAlias.GetAliasFor(type.Assembly);
								string aliasFor2 = AssemblyAlias.GetAliasFor(typeof(TypeDescriptor).Assembly);
								context.Assemblies.Add(type.Assembly);
								return SyntaxFactory.ParseExpression($"({aliasFor}::{type.Namespace}.{type.Name}){aliasFor2}::System.ComponentModel.TypeDescriptor.GetConverter(typeof({value.GetType().GetGenericTypeName(context)})).ConvertFrom(\"{obj}\")");
							}
						}
						ExpressionSyntax expressionSyntax = hook?.Invoke(value, context);
						if (expressionSyntax != null)
						{
							return expressionSyntax;
						}
						if (context.VariablesMap.TryGetValue(value, out IdentifierNameSyntax value2))
						{
							return value2;
						}
						if (!typeof(Activity).IsAssignableFrom(type) && !typeof(Argument).IsAssignableFrom(type))
						{
							return value.ToObjectLiteral(context, hook);
						}
						return value.ToInlineObjectLiteral(context, hook);
					}
					return SyntaxFactory.LiteralExpression(((bool)value) ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression);
				}
				string text2 = text;
				if (context.VariablesMap.TryGetValue(value, out IdentifierNameSyntax value3))
				{
					return value3;
				}
				string key = $"v{context.VariablesMap.Count}";
				IdentifierNameSyntax orAdd = ObjectFactoryCache.IdentifiersCache.GetOrAdd(key);
				context.VariablesMap[text2] = orAdd;
				AppendVariable(orAdd, type, SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(text2)), context);
				return context.VariablesMap[text2];
			}
			return SyntaxFactory.TypeOfExpression(type2.GetGenericTypeName(context));
		}
	}
}
