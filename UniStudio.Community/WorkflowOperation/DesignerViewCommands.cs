using System.Windows.Input;

namespace UniStudio.Community.WorkflowOperation
{
    public class DesignerViewCommands
    {
        public const string CommentOutCommandName = "CommentOutCommand";
        public static readonly ICommand CommentOutCommand = new RoutedCommand(CommentOutCommandName, typeof(DesignerViewWrapper), new InputGestureCollection
        {
            new KeyGesture(Key.D, ModifierKeys.Control)
        });

        public const string CommentOutDelCommandName = "CommentOutDelCommand";
        public static readonly ICommand CommentOutDelCommand = new RoutedCommand(CommentOutDelCommandName, typeof(DesignerViewWrapper), new InputGestureCollection
        {
            new KeyGesture(Key.E, ModifierKeys.Control)
        });
        
        public const string SurroundedWithTryCatchCommandName = "SurroundedWithTryCatchCommand";
        public static readonly ICommand SurroundedWithTryCatchCommand = new RoutedCommand(SurroundedWithTryCatchCommandName, typeof(DesignerViewWrapper), new InputGestureCollection
        {
            new KeyGesture(Key.T, ModifierKeys.Control)
        });

        public const string OpenWorkflowCommandName = "OpenWorkflowCommand";
        public static readonly ICommand OpenWorkflowCommand = new RoutedCommand(OpenWorkflowCommandName, typeof(DesignerViewWrapper), new InputGestureCollection
        {
            new KeyGesture(Key.O, ModifierKeys.Control)
        });
    }
}
