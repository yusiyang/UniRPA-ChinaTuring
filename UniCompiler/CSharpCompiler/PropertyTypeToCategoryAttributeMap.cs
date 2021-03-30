using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public static class PropertyTypeToCategoryAttributeMap
	{
		private const string CategoryInput = "Input";

		private const string CategoryOutput = "Output";

		private const string CategoryProperties = "Properties";

		private static readonly NameSyntax CategoryAttributeName = (NameSyntax)typeof(CategoryAttribute).GetGenericTypeName(new ObjectFactoryContext());

		private static readonly AttributeSyntax PropertiesLiteralExpression = BuildAttribute(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Properties")));

		private static readonly Dictionary<string, AttributeSyntax> ArgTypeToCategory = new Dictionary<string, AttributeSyntax>
	{
		{
			typeof(InArgument).Name,
			BuildAttribute(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Input")))
		},
		{
			typeof(InArgument<>).Name,
			BuildAttribute(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Input")))
		},
		{
			typeof(OutArgument).Name,
			BuildAttribute(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Output")))
		},
		{
			typeof(OutArgument<>).Name,
			BuildAttribute(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Output")))
		},
		{
			typeof(InOutArgument).Name,
			BuildAttribute(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Input/Output")))
		},
		{
			typeof(InOutArgument<>).Name,
			BuildAttribute(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal("Input/Output")))
		}
	};

		private static AttributeSyntax BuildAttribute(LiteralExpressionSyntax parameter)
		{
			return SyntaxFactory.Attribute(CategoryAttributeName, SyntaxFactory.AttributeArgumentList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.AttributeArgument(parameter))));
		}

		public static AttributeSyntax GetCategoryAttributeForProperty(Type propertyType)
		{
			if (ArgTypeToCategory.TryGetValue(propertyType.Name, out AttributeSyntax value))
			{
				return value;
			}
			return PropertiesLiteralExpression;
		}
	}
}
