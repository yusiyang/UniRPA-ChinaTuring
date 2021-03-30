using ActiproSoftware.Windows.Controls.Ribbon;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UniExplorer.ViewModel;

namespace UniExplorer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 窗口所有内容都显示给用户之前马上执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 打开加载动画
            ViewModelLocator.instance.Main.IsBuildLoading = true;
        }

        /// <summary>
        /// 窗口所有内容都显示给用户之后马上执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(App.LaunchArgsStr))
            {
                // 有携带参数过来
                ViewModelLocator.instance.Main.OutputDataControlIsVisibily = Visibility.Visible;

                SelectorStatusModel selectorStatusModel = SerializeObj.Desrialize(new SelectorStatusModel(), App.LaunchArgsStr);
                ViewModelLocator.instance.MainDock.SelectorStatusModel = selectorStatusModel;
                ViewModelLocator.instance.Main.ValidateElementIsExist();
            }
            else
            {
                ViewModelLocator.instance.Main.OutputDataControlIsVisibily = Visibility.Hidden;
                // 关闭加载动画
                ViewModelLocator.instance.Main.IsBuildLoading = false;
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            // 关闭窗口时
        }
    }
}
