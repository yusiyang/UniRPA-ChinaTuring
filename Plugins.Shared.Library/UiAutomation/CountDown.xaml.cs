using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.UiAutomation.CaptureEvents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using WinApi.User32;

namespace Plugins.Shared.Library.UiAutomation
{
    /// <summary>
    /// CountDown.xaml 的交互逻辑
    /// </summary>
    public partial class CountDown : System.Windows.Window
    {
        public static DependencyProperty CountDownInfoProperty;

        private DispatcherTimer _timer;

        private int _countDownSeconds;

        static CountDown()
        {
            CountDownInfoProperty = DependencyProperty.Register("CountDownInfo", typeof(string), typeof(CountDown), new FrameworkPropertyMetadata());
        }

        public string CountDownInfo
        {
            get
            {
                return (string)GetValue(CountDownInfoProperty);
            }
            set
            {
                SetValue(CountDownInfoProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countDownSeconds">总共倒计时多少秒</param>
        public CountDown(int countDownSeconds)
        {
            InitializeComponent();
            var rect = System.Windows.Forms.SystemInformation.VirtualScreen;
            Left = rect.X;
            Top = rect.Y;

            this.DataContext = this;
            _countDownSeconds = countDownSeconds;
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval= TimeSpan.FromSeconds(1);
            _timer.Start();
            CountDownInfo= (_countDownSeconds--).ToString();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CountDownInfo = (_countDownSeconds).ToString();
            if(_countDownSeconds<=0)
            {
                this.Close();
                _timer.Stop();
            }
            _countDownSeconds--;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var rect = System.Windows.Forms.SystemInformation.VirtualScreen;
            if (Left == rect.X)
            {
                Left = rect.Width - this.ActualWidth;
                Top = rect.Height - this.ActualHeight;
            }
            else
            {
                Left = rect.X;
                Top = rect.Y;
            }
        }
    }
}
