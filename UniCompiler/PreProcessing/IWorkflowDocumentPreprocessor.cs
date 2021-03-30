namespace UniCompiler.PreProcessing
{
    public interface IWorkflowDocumentPreprocessor
    {
        WorkflowDocument Load(string xamlFilePath);
    }
}
