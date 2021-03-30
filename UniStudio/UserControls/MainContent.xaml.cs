using ActiproSoftware.Windows;
using ActiproSoftware.Windows.Controls.Ribbon;
using ActiproSoftware.Windows.Controls.Ribbon.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UniStudio.Windows;

namespace UniStudio.UserControls
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class MainContent : UserControl
    {
        public MainContent()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 当开始菜单打开状态改变时候发生
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIsApplicationMenuOpenChanged(object sender, BooleanPropertyChangedRoutedEventArgs e)
        {
            var ribbon = sender as Ribbon;

            var window = Application.Current.MainWindow;

            if (window.GetType().Name.Equals("MainWindow"))
            {
                if (ribbon.IsApplicationMenuOpen)
                {
                    (window as RibbonWindow).IsTitleBarVisible = true;
                }
                else
                {
                    (window as RibbonWindow).IsTitleBarVisible = false;
                }
            }
        }

        /// <summary>
        /// 最小化按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMinimizedClick(object sender, ExecuteRoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 最大化或还原按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowMaximizedOrNormalClick(object sender, ExecuteRoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState.Equals(WindowState.Maximized))
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
            else if (Application.Current.MainWindow.WindowState.Equals(WindowState.Normal))
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// 关闭按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WindowCloseClick(object sender, ExecuteRoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void OnHelpClick(object sender, ExecuteRoutedEventArgs e)
        {

        }
    }
}
