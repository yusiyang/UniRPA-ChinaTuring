using System.Activities;
using System.Activities.Expressions;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public static class ExpressionsCompiler
    {
        public static string GetPartialClassForActivity(Activity activity, string classNamespace, string className)
        {
            TextExpressionCompilerSettings textExpressionCompilerSettings = new TextExpressionCompilerSettings();
            textExpressionCompilerSettings.Activity = activity;
            textExpressionCompilerSettings.Language = "C#";
            textExpressionCompilerSettings.ActivityName = className;
            textExpressionCompilerSettings.ActivityNamespace = classNamespace;
            textExpressionCompilerSettings.RootNamespace = null;
            textExpressionCompilerSettings.GenerateAsPartialClass = true;
            textExpressionCompilerSettings.AlwaysGenerateSource = true;
            textExpressionCompilerSettings.ForImplementation = true;
            StringWriter stringWriter = new StringWriter();
            new TextExpressionCompiler(textExpressionCompilerSettings).GenerateSource(stringWriter);
            return stringWriter.ToString();
        }

        public static (CompilationUnitSyntax, List<Assembly>) GetPartialClassForExpressions(ActivityBuilder builder, string classNamespace, string className)
        {
            DynamicActivity dynamicActivity = new DynamicActivity
            {
                Implementation = (() => builder.Implementation)
            };
            foreach (DynamicActivityProperty property in builder.Properties)
            {
                dynamicActivity.Properties.Add(property);
            }
            TextExpression.SetNamespacesForImplementation(dynamicActivity, TextExpression.GetNamespacesForImplementation(builder));
            TextExpression.SetReferencesForImplementation(dynamicActivity, TextExpression.GetReferencesForImplementation(builder));
            CompilationUnitSyntax item = (CompilationUnitSyntax)SyntaxFactory.ParseSyntaxTree(GetPartialClassForActivity(dynamicActivity, classNamespace, className)).GetRoot();
            List<Assembly> list = new List<Assembly>();
            foreach (AssemblyReference item2 in TextExpression.GetReferencesForImplementation(builder))
            {
                list.Add(Assembly.Load(item2.AssemblyName));
            }
            return (item, list);
        }
    }
}
