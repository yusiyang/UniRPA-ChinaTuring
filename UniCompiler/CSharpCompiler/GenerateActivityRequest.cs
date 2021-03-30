using System.Activities;

namespace UniCompiler.CSharpCompiler
{
    public class GenerateActivityRequest
    {
        public string AbsolutePath
        {
            get;
            set;
        }

        public ActivityBuilder ActivityBuilder
        {
            get;
            set;
        }

        public string ClassName
        {
            get;
            set;
        }

        public string ClassNamespace
        {
            get;
            set;
        }

        public bool CompileExpressions
        {
            get;
            set;
        }

        public string CategoryAttribute
        {
            get;
            set;
        }

        public string DisplayNameAttribute
        {
            get;
            set;
        }

        public bool BrowsableAttribute
        {
            get;
            set;
        }

        public string DescriptionAttribute
        {
            get;
            set;
        }
	}
}
