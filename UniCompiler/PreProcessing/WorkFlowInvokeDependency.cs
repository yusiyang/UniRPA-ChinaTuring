namespace UniCompiler.PreProcessing
{
    public class WorkFlowInvokeDependency
    {
        public string DocumentAbsolutePath
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        internal XamlWorkflowArgument[] Arguments
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Name;
        }
	}
}
