using ActiproSoftware.Windows.Controls.Ribbon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UniExecutor.View
{
    /// <summary>
    /// RuntimeErrorDialogs.xaml 的交互逻辑
    /// </summary>
    public partial class RuntimeErrorDialogs : RibbonWindow
    {
        public RuntimeErrorDialogs(string exceptionSource, string exceptionMessage, string exceptionType, string exceptionDetails)
        {
            InitializeComponent();
            _exceptionSource.Text = exceptionSource;
            _exceptionMessage.Text = exceptionMessage;
            _exceptionType.Text = exceptionType;
            _exceptionDetails.Text = exceptionDetails;
        }

        private void OnDetailsBtnClick(object sender, MouseButtonEventArgs e)
        {
            if (_detailsBtn.IsExpanded)
            {
                _exceptionDetailsContainer.Visibility = Visibility.Collapsed;
            }
            else
            {
                _exceptionDetailsContainer.Visibility = Visibility.Visible;
            }
        }

        private void OnOpenLogsClick(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\UniStudio\Logs");
        }

        private void OnCopyClipBoardClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject("来源: " + _exceptionSource.Text
                + "\r\n\r\n消息: " + _exceptionMessage.Text
                + "\r\n\r\n异常类型：" + _exceptionType.Text
                + "\r\n\r\n" + _exceptionDetails.Text);
        }

        private void OnOkBtnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
