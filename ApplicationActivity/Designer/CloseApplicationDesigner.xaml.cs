using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.UiAutomation;
using Plugins.Shared.Library.Window;
using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Windows;
using System.Windows.Controls.Primitives;


namespace ApplicationActivity
{
    // ActivityDesigner1.xaml 的交互逻辑
    public partial class CloseApplicationDesigner
    {
        public CloseApplicationDesigner()
        {
            InitializeComponent();
        }

        private void HyperlinkClick(object sender, RoutedEventArgs e)
        {
            UiElement.OnSelected = UiElement_OnSelected;//也可在程序安装始化时赋值
            UiElement.StartWindowHighlight();
        }

        private void UiElement_OnSelected(UiElement uiElement)
        {
            var screenshotsPath = uiElement.CaptureInformativeScreenshotToFile();
            setPropertyValue("SourceImgPath", screenshotsPath);
            setPropertyValue("Selector", new InArgument<string>(uiElement.Selector));
            setPropertyValue("SelectorOrigin", SerializeObj.Serialize(InExplorerOpen.BuildSelectorStatusModelFromStr(uiElement.Selector.ToString())));
            grid1.Visibility = System.Windows.Visibility.Hidden;
            setPropertyValue("ProcessName", new InArgument<string>(uiElement.ProcessName));
            setPropertyValue("visibility", System.Windows.Visibility.Visible);

            if (getPropertyValue("DisplayName").Equals(getPropertyValue("DefaultName")))
            {
                string displayName = getPropertyValue("DisplayName") + " \"" + uiElement.ProcessName + " " + uiElement.Name + "\"";
                setPropertyValue("DisplayName", displayName);
            }
        }

        private void setPropertyValue<T>(string propertyName, T value)
        {
            base.ModelItem.Properties[propertyName].SetValue(value);
        }

        private string getPropertyValue(string propertyName)
        {
            ModelProperty _property = base.ModelItem.Properties[propertyName];
            if (_property.Value == null)
                return "";
            return _property.Value.ToString();
        }

        private void HiddenNavigateTextBlock()
        {
            navigateTextBlock.Visibility = System.Windows.Visibility.Hidden;
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
        private void meauItemClickOne(object sender, RoutedEventArgs e)
        {
            UiElement.OnSelected = UiElement_OnSelected;//也可在程序安装始化时赋值
            UiElement.StartWindowHighlight();
        }

        /// <summary>
        /// 在 UI 探测器中打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void meauItemClickTwo(object sender, RoutedEventArgs e)
        {
            var selectorModelProperty = base.ModelItem.Properties["Selector"];
            var selectorOriginModelProperty = base.ModelItem.Properties["SelectorOrigin"];

            if (selectorModelProperty.Value != null)
            {
                InExplorerOpen.Execute(ref selectorModelProperty, ref selectorOriginModelProperty);
            }
            else
            {
                UniMessageBox.Show("没有数据！请先通过鼠标选取器选择元素。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Button_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string src = getPropertyValue("SourceImgPath");
            ShowImageWindow imgShow = new ShowImageWindow();
            imgShow.ShowImage(src);
        }

        private void ActivityDesigner_Loaded(object sender, RoutedEventArgs e)
        {
            string src = getPropertyValue("SourceImgPath");
            if (src != "")
                grid1.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
