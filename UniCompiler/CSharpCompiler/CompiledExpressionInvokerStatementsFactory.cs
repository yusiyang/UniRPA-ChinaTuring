using System.Activities.Expressions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public static class CompiledExpressionInvokerStatementsFactory
    {
        public static ExpressionStatementSyntax SetCompiledExpressionRootForImplementation(ObjectFactoryContext context)
        {
            return SyntaxFactory.ExpressionStatement(SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, typeof(CompiledExpressionInvoker).GetGenericTypeName(context), ObjectFactoryCache.IdentifiersCache.GetOrAdd("SetCompiledExpressionRootForImplementation")), SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(new ArgumentSyntax[2]
            {
                SyntaxFactory.Argument(SyntaxFactory.ThisExpression()),
                SyntaxFactory.Argument(SyntaxFactory.ThisExpression())
            }))));
        }
    }
}
