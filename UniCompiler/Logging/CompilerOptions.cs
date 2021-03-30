using System.Collections.Generic;

namespace UniCompiler.Logging
{
    public class CompilerOptions
    {
        //[Option('l', "libraryName", Required = true, HelpText = "The name of the library that will be built.")]
        /// <summary>
        /// Required = true, HelpText = "The name of the library that will be built."
        /// </summary>
        public string LibraryName
        {
            get;
            set;
        }

        //[Option('p', "pathToCompile", Required = true, HelpText = "Path to the root of the UiPath project folder.")]
        /// <summary>
        /// Required = true, HelpText = "Path to the root of the UiPath project folder."
        /// </summary>
        public string PathToCompile
        {
            get;
            set;
        }

        //[Option('o', "outputFolder", Required = false, HelpText = "Path to the output folder (needs to exist).")]
        /// <summary>
        /// Required = false, HelpText = "Path to the output folder (needs to exist)."
        /// </summary>
        public string OutputFolder
        {
            get;
            set;
        }

        //[Option('s', "studioFolder", Required = false, Default = null, HelpText = "UiPath Studio install folder to look for some dependencies not found in packages.")]
        /// <summary>
        /// Required = false, Default = null, HelpText = "UiPath Studio install folder to look for some dependencies not found in packages."
        /// </summary>
        public string StudioFolder
        {
            get;
            set;
        }

        //[Option('d', "dependencies", Required = false, Default = new string[]
        //{

        //}, HelpText = "Full path to dependencies assemblies separated by space.")]
        /// <summary>
        /// Required = false,HelpText = "Full path to dependencies assemblies separated by space."
        /// </summary>
        public IEnumerable<string> Dependencies
        {
            get;
            set;
        }

        //[Option('e', "privateActivities", Required = false, Default = new string[]
        //{

        //}, HelpText = "List of relative file paths of activities to be published.")]
        /// <summary>
        /// Required = false, HelpText = "List of relative file paths of activities to be published."
        /// </summary>
        public IEnumerable<string> PrivateActivities
        {
            get;
            set;
        }

        //[Option('w', "outputVersion", Required = true, HelpText = "Version of the output library.")]
        /// <summary>
        /// Required = true, HelpText = "Version of the output library."
        /// </summary>
        public string Version
        {
            get;
            set;
        }

        //[Option('x', "includeOriginalXaml", Default = false, Required = false, HelpText = "Use this to include the original xaml in the resulting assembly.")]
        /// <summary>
        /// Required = false, HelpText = "Use this to include the original xaml in the resulting assembly."
        /// </summary>
        public bool IncludeOriginalXaml
        {
            get;
            set;
        }

        //[Option('n', "needsCwdResolve", Default = true, Required = false, HelpText = "Set this property to true if your project includes external resource files, such as .csv, .png, etc, to resolve the directory of these files at runtime.")]
        /// <summary>
        /// Default = true, Required = false, HelpText = "Set this property to true if your project includes external resource files, such as .csv, .png, etc, to resolve the directory of these files at runtime."
        /// </summary>
        public bool NeedsCurrentWorkingDirectoryResolve
        {
            get;
            set;
        }

        //[Option('u', "uiCultureCode", Default = "", Required = false, HelpText = "UI culture code used to localize compiler messages.")]
        /// <summary>
        /// Default = "", Required = false, HelpText = "UI culture code used to localize compiler messages."
        /// </summary>
        public string UICultureCode
        {
            get;
            set;
        }

        //[Option('r', "rootCategory", Default = null, Required = false, HelpText = "The root category of all the activities.")]
        /// <summary>
        /// Default = null, Required = false, HelpText = "The root category of all the activities."
        /// </summary>
        public string ActivitiesRootCategory
        {
            get;
            set;
        }

        //[Option('L', "expressionLanguage", Default = "VB", Required = false, HelpText = "The language in which the expressions are written. Supported values: C#, VB")]
        /// <summary>
        /// Default = "VB", Required = false, HelpText = "The language in which the expressions are written. Supported values: C#, VB"
        /// </summary>
        public string ExpressionLanguage
        {
            get;
            set;
        }

        //[Option('c', "useCSharpCompiler", Default = false, Required = false, HelpText = "The compiler to use. True for using C# compiler. Xaml compiler is the default")]
        /// <summary>
        /// Default = false, Required = false, HelpText = "The compiler to use. True for using C# compiler. Xaml compiler is the default"
        /// </summary>
        public bool UseCSharpCompiler
        {
            get;
            set;
        }
    }
}