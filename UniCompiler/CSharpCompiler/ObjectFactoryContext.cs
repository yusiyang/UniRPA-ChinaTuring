using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UniCompiler.CSharpCompiler
{
	public class ObjectFactoryContext
    {
        public Dictionary<Type, int> VariablesCount
        {
            get;
        } = new Dictionary<Type, int>();


        public Dictionary<object, IdentifierNameSyntax> VariablesMap
        {
            get;
        } = new Dictionary<object, IdentifierNameSyntax>();


        public List<StatementSyntax> VariablesStatements
        {
            get;
        } = new List<StatementSyntax>();


        public List<StatementSyntax> PropertiesAssignmentsStatements
        {
            get;
        } = new List<StatementSyntax>();


        public HashSet<Assembly> Assemblies
        {
            get;
        } = new HashSet<Assembly>();


        public void ClearContext()
        {
            VariablesCount.Clear();
            VariablesMap.Clear();
            VariablesStatements.Clear();
            PropertiesAssignmentsStatements.Clear();
            Assemblies.Clear();
        }
    }
}
