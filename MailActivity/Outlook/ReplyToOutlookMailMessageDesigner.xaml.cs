using Plugins.Shared.Library.Editors;
using System.Windows.Input;

namespace MailActivity.Outlook
{
    // ReplyToOutlookMailMessageDesigner.xaml 的交互逻辑
    public partial class ReplyToOutlookMailMessageDesigner
    {
        public ReplyToOutlookMailMessageDesigner()
        {
            InitializeComponent();
        }

        private void AttachFiles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ArgumentCollectionEditor.ShowDialog("附件", base.ModelItem);
        }

    }
}
