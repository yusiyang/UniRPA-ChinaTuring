using ActiproSoftware.Windows.Controls.Docking;
using System.Linq;
using System.Windows.Controls;
using UniStudio.ViewModel;

namespace UniStudio.UserControls
{
    /// <summary>
    /// DockContent.xaml 的交互逻辑
    /// </summary>
    public partial class DockContent : UserControl
    {
        public DockContent()
        {
            InitializeComponent();
        }

        private void DockSite_WindowsClosing(object sender, DockingWindowsEventArgs e)
        {
            var documents = e.Windows.OfType<DocumentWindow>().ToArray();
            if (documents.Any())
            {
                foreach (var document in documents)
                {
                    var doc = document.DataContext as DocumentViewModel;
                    var docs = ViewModelLocator.instance.Dock.Documents.ToArray();
                    foreach (var currentDoc in docs)
                    {
                        if (doc.Equals(currentDoc))
                        {
                            if (!currentDoc.DoCloseDocument())
                            {
                                e.Cancel = true;
                                e.Handled = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
