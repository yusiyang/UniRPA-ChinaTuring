using System.Windows;

namespace UniStudio.Community.Windows
{
    /// <summary>
    /// EditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class EditWindow : Window
    {
        public delegate void TextEventHandler(string strText);

        public TextEventHandler TextHandler;
        public EditWindow()
        {
            InitializeComponent();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TextHandler == null) return;
            TextHandler.Invoke(TextBox.Text);
            DialogResult = true;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
