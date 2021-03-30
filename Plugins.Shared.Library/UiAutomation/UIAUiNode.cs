using System;
using System.Collections.Generic;
using System.Drawing;
using FlaUI.UIA3;
using WindowsAccessBridgeInterop;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA2;
using FlaUI.Core;
using FlaUI.Core.Definitions;
using FlaUI.Core.Conditions;
using static Plugins.Shared.Library.Win32Api;
using Plugins.Shared.Library.WindowsAPI;
using FlaUI.Core.Overlay;
using FlaUI.Core.Identifiers;
using WinApi.User32;

namespace Plugins.Shared.Library.UiAutomation
{
    class UIAUiNode : UiNode
    {
        private static ITreeWalker _treeWalker;

        internal static AutomationBase uia3Automation = new UIA3Automation();
        internal static AutomationBase uia2Automation = new UIA2Automation();

        internal AutomationElement automationElement;

        //优化读取速度
        private string cachedProcessName;
        private string cachedProcessFullPath;
        private UIAUiNode cachedParent;
        private UIAUiNode automationElementParent;

        public UIAUiNode(AutomationElement element, UIAUiNode parent = null)
        {
            this.automationElement = element;
            this.cachedParent = parent;
        }

        public static AutomationBase UIAAutomation
        {
            get
            {
                //此处根据需求来确定用UIA2还是UIA3

                //目前测试来看UIA2兼容性强些，腾讯的TIM用UIA2展示相对正常
                return uia2Automation;
            }
        }

        private static ITreeWalker TreeWalker
        {
            get
            {
                if (_treeWalker == null)
                {
                    _treeWalker = UIAAutomation.TreeWalkerFactory.GetControlViewWalker();
                }

                return _treeWalker;
            }
        }

        public string AutomationId
        {
            get
            {
                try
                {
                    return automationElement.AutomationId;
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                return automationElement.BoundingRectangle;
            }
        }

        public string ClassName
        {
            get
            {
                try
                {
                    return automationElement.ClassName;
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public string ControlType
        {
            get
            {
                try
                {
                    return automationElement.ControlType.ToString();
                }
                catch (Exception)
                {

                    return "";
                }
            }
        }


        public string Name
        {
            get
            {
                try
                {
                    return automationElement.Name;
                }
                catch (Exception)
                {

                    return "";
                }
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                try
                {
                    if (automationElement.Properties.NativeWindowHandle.IsSupported)
                    {
                        var windowHandle = automationElement.Properties.NativeWindowHandle.ValueOrDefault;
                        return windowHandle;
                    }
                    else
                    {
                        return IntPtr.Zero;
                    }
                }
                catch (Exception)
                {
                    return IntPtr.Zero;
                }

            }
        }

        //此处可能会出现从鼠标点获取的元素和从Desktop往下获取的元素不一致的情况，根据实际情况有可能需要修正
        private AutomationElement getCorrectParent(AutomationElement element)
        {
            return TreeWalker.GetParent(element);
        }

        public UiNode Parent
        {
            get
            {
                if (cachedParent == null)
                {
                    var realParent = getCorrectParent(automationElement);

                    if (realParent != null)
                    {
                        cachedParent = new UIAUiNode(realParent);
                    }
                }

                return cachedParent;
            }
        }

        public UiNode AutomationElementParent
        {
            get
            {
                automationElementParent = new UIAUiNode(automationElement.Parent);
                return automationElementParent;
            }
        }




        public string Role
        {
            get
            {
                return "";
            }
        }

        public string ProcessName
        {
            get
            {
                if (cachedProcessName == null)
                {
                    try
                    {
                        var processId = automationElement.Properties.ProcessId.Value;
                        var name = UiCommon.GetProcessName(processId);

                        cachedProcessName = name;
                    }
                    catch (Exception)
                    {
                        cachedProcessName = "";
                    }
                }

                return cachedProcessName;
            }
        }

        public string ProcessFullPath
        {
            get
            {
                if (cachedProcessFullPath == null)
                {
                    try
                    {
                        var processId = automationElement.Properties.ProcessId.Value;
                        var path = UiCommon.GetProcessFullPath(processId);

                        cachedProcessFullPath = path;
                    }
                    catch (Exception)
                    {
                        cachedProcessFullPath = "";
                    }
                }

                return cachedProcessFullPath;
            }
        }

        public List<UiNode> Children
        {
            get
            {
                var list = new List<UiNode>();
                var children = automationElement.FindAllChildren();
                foreach (var item in children)
                {
                    list.Add(new UIAUiNode(item, this));
                }


                if (JavaUiNode.AccessBridge.Functions.IsJavaWindow(this.WindowHandle))
                {
                    var contextNode = JavaUtils.GetContextNode(WindowHandle);
                    if (!contextNode.AccessibleContextHandle.IsNull)
                    {
                        var rootPanelNode = contextNode.FetchChildNode(0);
                        list.Add(new JavaUiNode(rootPanelNode));
                    }
                }
                return list;
            }
        }


        public bool IsTopLevelWindow
        {
            get
            {
                try
                {
                    return automationElement.Parent != null && automationElement.Parent.Parent == null;

                }
                catch (Exception)
                {
                    return false;
                }

            }
        }

        public string UserDefineId
        {
            get
            {
                return "";
            }
        }


        public string Description
        {
            get
            {

                try
                {
                    if (automationElement.Patterns.LegacyIAccessible.Pattern.Description.IsSupported)
                    {
                        return automationElement.Patterns.LegacyIAccessible.Pattern.Description.ValueOrDefault;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public string Idx
        {
            get
            {
                var realParent = getCorrectParent(automationElement);
                if (realParent == null)
                {
                    return "";
                }

                try
                {
                    var children = realParent.FindAllChildren();

                    int index = 0;

                    foreach (var item in children)
                    {
                        if (item.Equals(automationElement))
                        {
                            return index.ToString();
                        }

                        index++;
                    }
                }
                catch (Exception e)
                {
                }


                return "";
            }
        }

        /// <summary>
        /// 元素的UIA Patterns
        /// </summary>
        public FrameworkAutomationElementBase.IFrameworkPatterns Patterns => automationElement.Patterns;

        /// <summary>
        /// 查找所有符合条件的元素
        /// </summary>
        /// <param name="scope">查找范围</param>
        /// <param name="condition">查找条件</param>
        /// <returns>符合条件的元素集合</returns>
        public List<UiNode> FindAll(TreeScope scope, ConditionBase condition)
        {
            var list = new List<UiNode>();
            var elements = automationElement.FindAll(scope, condition);
            foreach (var item in elements)
            {
                list.Add(new UIAUiNode(item, this));
            }
            return list;
        }

        /// <summary>
        /// 通过索引查找子元素
        /// </summary>
        /// <param name="idx">索引号</param>
        /// <returns>找到的子元素</returns>
        public UiNode GetChildByIdx(int idx)
        {
            var item = automationElement.FindChildAt(idx);
            return new UIAUiNode(item, this);
        }


        //public void MouseClick(UiElementClickParams clickParams = null)
        //{
        //    if(clickParams == null)
        //    {
        //        clickParams = new UiElementClickParams();
        //    }

        //    try
        //    {
        //        automationElement.Click(clickParams.moveMouse);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //public void MouseDoubleClick(UiElementClickParams clickParams = null)
        //{
        //    if (clickParams == null)
        //    {
        //        clickParams = new UiElementClickParams();
        //    }

        //    try
        //    {
        //        automationElement.DoubleClick(clickParams.moveMouse);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //public void MouseRightClick(UiElementClickParams clickParams = null)
        //{
        //    if (clickParams == null)
        //    {
        //        clickParams = new UiElementClickParams();
        //    }

        //    try
        //    {
        //        automationElement.RightClick(clickParams.moveMouse);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        //public void MouseRightDoubleClick(UiElementClickParams clickParams = null)
        //{
        //    if (clickParams == null)
        //    {
        //        clickParams = new UiElementClickParams();
        //    }

        //    try
        //    {
        //        automationElement.RightDoubleClick(clickParams.moveMouse);
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        /// <summary>
        /// 将元素所在窗口置顶
        /// </summary>
        public void SetForeground()
        {
            this.automationElement.SetForeground();
        }

        /// <summary>
        /// 鼠标悬浮
        /// </summary>
        /// <param name="hoverParams">悬浮参数</param>
        public void MouseHover(UiElementHoverParams hoverParams = null)
        {
            if (hoverParams == null)
            {
                hoverParams = new UiElementHoverParams();
            }

            try
            {
                Point clickablePoint;
                switch (hoverParams.elementPosition)
                {
                    case ElementPosition.TopLeft:
                        clickablePoint = new Point(BoundingRectangle.X, BoundingRectangle.Y);
                        break;

                    case ElementPosition.TopRight:
                        clickablePoint = new Point(BoundingRectangle.X + BoundingRectangle.Width, BoundingRectangle.Y);
                        break;

                    case ElementPosition.BottomLeft:
                        clickablePoint = new Point(BoundingRectangle.X, BoundingRectangle.Y + BoundingRectangle.Height);
                        break;

                    case ElementPosition.BottomRight:
                        clickablePoint = new Point(BoundingRectangle.X + BoundingRectangle.Width, BoundingRectangle.Y + BoundingRectangle.Height);
                        break;

                    default:
                        clickablePoint = GetClickablePoint();
                        break;
                }

                clickablePoint.X += hoverParams.offset.x;
                clickablePoint.Y += hoverParams.offset.y;

                if (hoverParams.isMoveMouse)
                {
                    FlaUI.Core.Input.Mouse.MoveTo(clickablePoint);
                }

                FlaUI.Core.Input.Mouse.Position = clickablePoint;
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// 获取元素的可点击坐标点
        /// </summary>
        /// <returns>元素的可点击坐标点</returns>
        public Point GetClickablePoint()
        {
            try
            {
                return automationElement.GetClickablePoint();
            }
            catch (Exception)
            {
                //取矩形中心点
                return new Point(BoundingRectangle.Left + BoundingRectangle.Width / 2,
                                    BoundingRectangle.Top + BoundingRectangle.Height / 2);
            }
        }

        /// <summary>
        /// 聚焦
        /// </summary>
        public void Focus()
        {
            automationElement.Focus();
        }

        /// <summary>
        /// 键盘模拟输入
        /// </summary>
        /// <param name="text"></param>
        public void SimulateTypeText(string text)
        {
            var textBox = automationElement.AsTextBox();
            if (textBox == null)
            {
                throw new NotSupportedException("不支持此方式输入");
            }
            textBox.Text = text;
        }

        public void SimulateClick()
        {
            UiElement uiElement = new UiElement(this);
            var patterns = uiElement.Patterns;
            var invoke = patterns.Invoke;
            if (invoke != null && invoke.IsSupported)
            {
                invoke.Pattern.Invoke();
                return;
            }
            else
            {
                throw new NotSupportedException("此类元素不支持模拟点击");
            }
        }

        public void HighLight(Color? color = null, TimeSpan? duration = null, bool blocking = false)
        {
            var colorName = color ?? Color.Red;
            var rectangle = this.BoundingRectangle;
            if (!rectangle.IsEmpty)
            {
                var durationInMs = (int)(duration ?? TimeSpan.FromSeconds(2)).TotalMilliseconds;

                var overlayManager = new WinFormsOverlayManager();
                if (blocking)
                {
                    overlayManager.ShowBlocking(rectangle, colorName, durationInMs);
                }
                else
                {
                    overlayManager.Show(rectangle, colorName, durationInMs);
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public bool IsChecked()
        {
            var checkBox = automationElement.AsCheckBox();
            if (checkBox == null)
            {
                throw new NotSupportedException("该元素没有‘是否勾选’的属性");
            }
            return (bool)checkBox.IsChecked;
        }

        public void Check()
        {
            var checkBox = automationElement.AsCheckBox();
            if (checkBox == null || !checkBox.Patterns.Toggle.IsSupported)
            {
                var radioButton = automationElement.AsRadioButton();
                var selectionItemPattern = radioButton.Patterns.SelectionItem;
                if (selectionItemPattern != null && selectionItemPattern.IsSupported)
                {
                    selectionItemPattern.Pattern.Select();
                }
                else
                {
                    throw new NotSupportedException("该元素不支持勾选操作");
                }
            }
            else
            {
                while (!(bool)checkBox.IsChecked)
                {
                    checkBox.Toggle();
                }
            }
        }

        public void UnCheck()
        {
            var checkBox = automationElement.AsCheckBox();
            if (checkBox == null || !checkBox.Patterns.Toggle.IsSupported)
            {
                throw new NotSupportedException("该元素不支持反选操作");
            }
            else
            {
                while ((bool)checkBox.IsChecked)
                {
                    checkBox.Toggle();
                }
            }
        }

        public void Toggle()
        {
            var checkBox = automationElement.AsCheckBox();
            if (checkBox == null || !checkBox.Patterns.Toggle.IsSupported)
            {
                throw new NotSupportedException("该元素不支持变换勾选操作");
            }
            else
            {
                checkBox.Toggle();
            }
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <returns>获取到的文本</returns>
        public string GetText()
        {
            string text = "";
            var pattern = automationElement.Patterns;
            var value = pattern.Value;
            var textbox = automationElement.AsTextBox();

            if (textbox != null)
            {
                text = textbox.Text;
            }

            else if (value != null && value.IsSupported)
            {
                text = value.Pattern.Value;
            }

            else if (value == null && !string.IsNullOrEmpty(this.Name))
            {
                text = this.Name;
            }

            else if (!string.IsNullOrEmpty(this.ProcessFullPath))
            {
                text = this.ProcessFullPath;
            }

            return text;
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        public void SetText(string text)
        {
            var textbox = automationElement.AsTextBox();
            if (textbox == null)
            {
                throw new NotSupportedException("不支持此方式输入");
            }
            textbox.Text = text;
            //if (!textbox.IsReadOnly)
            //{
            //    textbox.Text = text;
            //}
            //else
            //{ 
            //    SharedObject.Instance.Output(SharedObject.enOutputType.Error, "有一个错误产生", "该编辑框无法设置值");
            //}
        }

        /// <summary>
        /// 是否为密码控件
        /// </summary>
        /// <returns></returns>
        public bool IsPassword()
        {
            return automationElement.Properties.IsPassword;
        }

        /// <summary>
        /// 清除所有选择项
        /// </summary>
        public void ClearSelection()
        {
            //支持SelectionItemPattern的控件清除选择
            var selectionItem = automationElement.Patterns.SelectionItem;
            if (selectionItem != null && selectionItem.IsSupported)
            {
                selectionItem.Pattern.RemoveFromSelection();
            }

            //不支持SelectionItemPattern
            else
            {
                //var windowHandle = automationElement.Properties.NativeWindowHandle;
                ////WinUser.h
                //SendMessage(windowHandle, 0x0185/*LB_SETSEL*/, (IntPtr)0, (IntPtr) (- 1));
                var listbox = automationElement.AsListBox();

                //listbox.Select(0);
                //listbox.RemoveFromSelection(0);

                for (int i = 0; i < listbox.Items.Length; i++)
                {
                    listbox.RemoveFromSelection(i);
                }
            }
        }

        /// <summary>
        /// 选择条目
        /// </summary>
        /// <param name="item">要选择的条目名称</param>
        public void SelectItem(string item)
        {
            var comboBox = automationElement.AsComboBox();
            var comboItem = comboBox.Select(item);
            if (comboItem.Text != item)
            {
                var lbox = automationElement.AsListBox();
                var listItem = lbox.Select(item);
                if (listItem.Text != item)
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, "SelectItem失败,请检查UI元素!");
                }
            }
        }

        /// <summary>
        /// 多选条目
        /// </summary>
        /// <param name="items">要选择的条目名称集合</param>
        public void SelectMultiItems(string[] items)
        {
            var comboBox = automationElement.AsComboBox();
            for (int i = 0; i < items.Length; i++)
            {
                //var comboItem = comboBox.Select(items[i]);
                //if (comboItem.Text != items[i])
                //{
                var lbox = automationElement.AsListBox();
                ListBoxItem[] listBoxItems = lbox.Items;

                for (int j = 0; j < items.Length; j++)
                {
                    int index = -1;
                    for (int k = 0; k < listBoxItems.Length; k++)
                    {
                        if (listBoxItems[k].Text == items[j])
                        {
                            index = k;
                        }
                    }
                    if (index != -1)
                    {
                        var listItem = lbox.AddToSelection(index);
                    }
                    else
                    {
                        SharedObject.Instance.Output(SharedObject.OutputType.Error, "SelectMutiItems失败,请检查UI元素!");
                        break;
                    }
                }
                break;
                //}
            }
        }

        /// <summary>
        /// 清除文本
        /// </summary>
        public void ClearText()
        {
            var textbox = automationElement.AsTextBox();
            if (!textbox.IsReadOnly)
            {
                textbox.Text = "";
            }
            else
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "有一个错误产生", "该编辑框无法设置值");
            }

        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="attributeName">属性名称</param>
        /// <returns></returns>
        public object GetAttributeValue(string attributeName)
        {
            object attrValue = null;
            FrameworkAutomationElementBase baseFrame = automationElement.FrameworkAutomationElement;
            PropertyId[] ids = automationElement.GetSupportedPropertiesDirect();
            for (int i = 0; i < ids.Length; i++)
            {
                if (String.Equals(ids[i].Name, attributeName, StringComparison.CurrentCultureIgnoreCase))
                {
                    attrValue = baseFrame.GetPropertyValue(ids[i]);
                    break;
                }
            }
            return attrValue;
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        /// <returns>是否启用</returns>
        public bool IsEnable()
        {
            return automationElement.IsEnabled;
        }

        /// <summary>
        /// 是否可见
        /// </summary>
        /// <returns>是否可见</returns>
        public bool IsVisible()
        {
            return !automationElement.IsOffscreen && automationElement.TryGetClickablePoint(out var point);
        }

        /// <summary>
        /// 是否为表格
        /// </summary>
        /// <returns>是否为表格</returns>
        public bool IsTable()
        {
            var gridPattern = automationElement.Patterns.Grid;
            return (gridPattern != null && gridPattern.IsSupported);
        }

        /// <summary>
        /// 鼠标集合操作
        /// </summary>
        /// <param name="clickParams">鼠标点击参数</param>
        public void MouseClick(UiElementClickParams clickParams = null)
        {
            //鼠标按键、点击类型（单击双击按下弹起）、操作方式（普通、模拟、窗口消息）、是否移动、点击位置、偏移坐标
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            #region 处理操作方式
            //普通点击
            if (clickParams.mouseActionType == MouseActionType.Normal)
            {

                #region 处理偏移坐标和点击位置            
                Point clickablePoint;
                switch (clickParams.elementPosition)
                {
                    case ElementPosition.Center:
                        clickablePoint = new Point(BoundingRectangle.X + BoundingRectangle.Width / 2, BoundingRectangle.Y + BoundingRectangle.Height / 2);
                        break;

                    case ElementPosition.TopLeft:
                        clickablePoint = new Point(BoundingRectangle.X, BoundingRectangle.Y);
                        break;

                    case ElementPosition.TopRight:
                        clickablePoint = new Point(BoundingRectangle.X + BoundingRectangle.Width, BoundingRectangle.Y);
                        break;

                    case ElementPosition.BottomLeft:
                        clickablePoint = new Point(BoundingRectangle.X, BoundingRectangle.Y + BoundingRectangle.Height);
                        break;

                    case ElementPosition.BottomRight:
                        clickablePoint = new Point(BoundingRectangle.X + BoundingRectangle.Width, BoundingRectangle.Y + BoundingRectangle.Height);
                        break;

                    default:
                        clickablePoint = GetClickablePoint();
                        break;
                }

                clickablePoint.X += clickParams.offset.x;
                clickablePoint.Y += clickParams.offset.y;

                #endregion

                #region 处理是否移动
                if (clickParams.isMoveMouse)
                {
                    FlaUI.Core.Input.Mouse.MoveTo(clickablePoint);
                }

                else
                {
                    FlaUI.Core.Input.Mouse.Position = clickablePoint;
                }
                #endregion

                #region 处理鼠标按键
                FlaUI.Core.Input.MouseButton mouseButton = new FlaUI.Core.Input.MouseButton();

                switch (clickParams.mouseButton)
                {
                    case MouseButton.Left:
                        mouseButton = FlaUI.Core.Input.MouseButton.Left;
                        break;

                    case MouseButton.Middle:
                        mouseButton = FlaUI.Core.Input.MouseButton.Middle;
                        break;

                    case MouseButton.Right:
                        mouseButton = FlaUI.Core.Input.MouseButton.Right;
                        break;

                    case MouseButton.XButton1:
                        mouseButton = FlaUI.Core.Input.MouseButton.XButton1;
                        break;

                    case MouseButton.XButton2:
                        mouseButton = FlaUI.Core.Input.MouseButton.XButton2;
                        break;

                    default:
                        mouseButton = FlaUI.Core.Input.MouseButton.Left;
                        break;
                }
                #endregion

                #region 处理点击类型


                switch (clickParams.clickType)
                {
                    case ClickType.Single:
                        FlaUI.Core.Input.Mouse.Click(mouseButton);
                        break;

                    case ClickType.Double:
                        FlaUI.Core.Input.Mouse.DoubleClick(mouseButton);
                        break;

                    case ClickType.Down:
                        FlaUI.Core.Input.Mouse.Down(mouseButton);
                        break;

                    case ClickType.Up:
                        FlaUI.Core.Input.Mouse.Up(mouseButton);
                        break;
                    default:
                        FlaUI.Core.Input.Mouse.Click(mouseButton);
                        break;
                }
                #endregion
            }

            //模拟点击
            else if (clickParams.mouseActionType == MouseActionType.Simulate)
            {
                SimulateClick();
            }

            //发送窗口消息
            else
            {
                UiElement uiElement = new UiElement(this);
                var windowHandle = uiElement.ClosestWindowElement.WindowHandle;
                LPPOINT lPPoint;
                lPPoint.X = BoundingRectangle.X + BoundingRectangle.Width / 2;
                lPPoint.Y = BoundingRectangle.Y + BoundingRectangle.Height / 2;
                Win32Api.ScreenToClient(windowHandle, out lPPoint);
                var clientPoint = new Point(lPPoint.X, lPPoint.Y);
                MouseMessageSender.MouseMessageAction(windowHandle, clientPoint, clickParams.mouseButton, clickParams.clickType);
            }
            #endregion
        }
    }
}
