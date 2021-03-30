using MouseActivity.Activity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.UiAutomation;
using Plugins.Shared.Library.UiAutomation.CaptureEvents;
using Plugins.Shared.Library.Window;
using Plugins.Shared.Library.WindowsAPI;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MouseActivity
{
    public partial class ClickImageDesigner
    {
        public ClickImageDesigner()
        {
            InitializeComponent();
        }

        private void hyperlink_Click(object sender, RoutedEventArgs e)
        {            
            ShowScreenCapture();            
        }

        private void ScreenCaptured(object sender, CapturedEventArgs e)
        {
            SetPropertyValue("SourceImgPath", e.ImagePath);
            var processElement = GetProcessElement(e.CaptureRange);
            SetPropertyValue("Selector", new InArgument<string>(processElement.Selector.Equals("<Pane ClassName='#32769' /><Pane ClassName='WorkerW' ProcessName='explorer.exe' />") ? "<Pane ClassName='#32769' />" : processElement.Selector));  // 选择 Windows10 桌面元素时，会出现 “<Pane ClassName='WorkerW' ProcessName='explorer.exe' />” 节点，需特别处理
            grid1.Visibility = Visibility.Hidden;
            SetPropertyValue("Visibility", Visibility.Visible);

            InArgument<Int32> _offsetX = 0;
            InArgument<Int32> _offsetY = 0;
            setPropertyValue("offsetX", _offsetX);
            setPropertyValue("offsetY", _offsetY);

            if (getPropertyValue("DisplayName").Equals(getPropertyValue("DefaultName")))
            {
                string displayName = getPropertyValue("DisplayName") + " \"" + processElement.ProcessName + " " + processElement.Name + "\"";
                setPropertyValue("DisplayName", displayName);
            }
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }

        private string getPropertyValue(string propertyName)
        {
            ModelProperty _property = base.ModelItem.Properties[propertyName];
            if (_property.Value == null)
                return "";
            return _property.Value.ToString();
        }

        private void setPropertyValue<T>(string propertyName, T value)
        {
            base.ModelItem.Properties[propertyName].SetValue(value);
        }

        private void ScreenCaptureCanceled(object sender,CaptureCanceledEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Normal;
        }

        //菜单按钮点击
        private void NavigateButtonClick(object sender, RoutedEventArgs e)
        {
            contextMenu.PlacementTarget = this.navigateButton;
            contextMenu.Placement = PlacementMode.Top;
            contextMenu.IsOpen = true;
        }

        //菜单按钮初始化
        private void NavigateButtonInitialized(object sender, EventArgs e)
        {
            navigateButton.ContextMenu = null;
        }

        //菜单项点击测试
        private void reSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowScreenCapture();
        }

        private void showImageBtn_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string src = GetPropertyValue("SourceImgPath");
            ShowImageWindow imgShow = new ShowImageWindow();
            imgShow.ShowImage(src);
        }

        private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
        {
            string src = GetPropertyValue("SourceImgPath");
            if (src != "")
                grid1.Visibility = Visibility.Hidden;
        }


        private UiElement GetProcessElement(RectangleRange captureRange)
        {
            var centerPoint = captureRange.Center;
            var centerDrawPoint = new System.Drawing.Point((int)centerPoint.X, (int)centerPoint.Y);
            var uiElement = UiCommon.GetRootUiElement(centerDrawPoint);
            if(uiElement == null)
            {
                throw new Exception("进程元素找不到");
            }
            return uiElement;
        }

        private void SetPropertyValue<T>(string propertyName, T value)
        {
            base.ModelItem.Properties[propertyName].SetValue(value);
        }

        private string GetPropertyValue(string propertyName)
        {
            ModelProperty _property = base.ModelItem.Properties[propertyName];
            if (_property.Value == null)
                return "";
            return _property.Value.ToString();
        }

        private void ShowScreenCapture()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            Thread.Sleep(300);
            var screenCapture = new ScreenCapture();
            screenCapture.Captured += ScreenCaptured;
            screenCapture.CaptureCanceled += ScreenCaptureCanceled;
            screenCapture.Show();
            WindowsHelper.SetForeground(screenCapture);            
        }
    }
}
