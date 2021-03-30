using System;

namespace UniCompiler.Logging
{
    public class LoggingManager
    {
        public static void ExecuteCSharpCompiler(Action action)
        {
            action();
        }

        public static void ExecuteCSharpAssemblyCompiler(Action action)
        {
            action();
        }

        public static void ExecuteCSharpAssemblyWriter(Action action, string outputFilePath)
        {
            action();
        }

        public static void ExecuteCSharpDocumentsCompiler(Action action)
        {
            action();
        }

        public static void ExecuteXamlDocumentLoader(Action action, string file, int index, int count)
        {
            //InternalExecuteInScope(new LoggingScopeOptions
            //{
            //    Category = Categories.Preprocessing,
            //    BeginMessage = string.Format(Resources.WorkflowDocumentPreprocessor_Load_Loading_xaml, file),
            //    CompletedMessage = Resources.WorkflowDocumentPreprocessor_Load_Loading_xaml_completed,
            //    CompletedMessageType = TraceEventType.Verbose,
            //    ScopeIndex = index,
            //    ScopeCount = count
            //}, action);
            action();
        }
    }
}