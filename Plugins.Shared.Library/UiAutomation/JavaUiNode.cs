using FlaUI.Core.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using WindowsAccessBridgeInterop;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core;
using static Plugins.Shared.Library.Win32Api;
using Plugins.Shared.Library.WindowsAPI;
using FlaUI.Core.Overlay;

namespace Plugins.Shared.Library.UiAutomation
{
    class JavaUiNode : UiNode
    {
        internal static readonly AccessBridge AccessBridge = JavaUtils.AccessBridge;
        private static List<AccessibleJvm> cachedJvms;
        internal AccessibleNode accessibleNode;


        private Dictionary<string, string> propDict = new Dictionary<string, string>();
        private Dictionary<string, object> propObjectDict = new Dictionary<string, object>();

        private UiNode cachedParent;

        static JavaUiNode()
        {
            try
            {
                AccessBridge.Initialize();
            }
            catch (Exception)
            {

            }
        }

        public JavaUiNode(AccessibleNode accessibleNode)
        {
            this.accessibleNode = accessibleNode;

            var propertyList = accessibleNode.GetProperties(PropertyOptions.AccessibleContextInfo | PropertyOptions.ObjectDepth);
            foreach (var item in propertyList)
            {
                if (item.Name != null)
                {
                    //Console.WriteLine(item.Name);
                    //Console.WriteLine(item.Value.ToString());
                    propObjectDict[item.Name] = item.Value;
                    propDict[item.Name] = item.Value == null ? "" : item.Value.ToString();
                }
            }
        }


        public string AutomationId
        {
            get
            {
                return "";
            }
        }

        public string UserDefineId
        {
            get
            {
                return propDict["Object Depth"] + "-" + propDict["Index in parent"] + "-" + propDict["Children count"];
            }
        }

        public string Idx
        {
            get
            {
                return "";
            }
        }


        //JAVA因为不需要，所以暂时不实现该接口
        public UiNode GetChildByIdx(int idx)
        {
            return Children[idx];
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                return accessibleNode.GetScreenRectangle() ?? new Rectangle();
            }
        }

        public List<UiNode> Children
        {
            get
            {
                var list = new List<UiNode>();
                var children = accessibleNode.GetChildren();
                foreach (var child in children)
                {
                    list.Add(new JavaUiNode(child));
                }

                return list;
            }
        }

        public string ClassName
        {
            get
            {
                return "";
            }
        }

        public string ControlType
        {
            get
            {
                return "JavaNode";
            }
        }

        public bool IsTopLevelWindow
        {
            get
            {
                return WindowHandle != IntPtr.Zero;
            }
        }

        public string Name
        {
            get
            {
                if (propDict.ContainsKey("Name"))
                {
                    return propDict["Name"];
                }

                return "";
            }
        }

        public UiNode Parent
        {
            get
            {
                if (cachedParent == null)
                {
                    var parent = accessibleNode.GetParent();
                    if (parent != null)
                    {
                        //如果parent含有WindowHandle属性，则说明到了顶层窗口，改用UIA窗口结点
                        UiNode parentUiNode = new JavaUiNode(accessibleNode.GetParent());
                        if (parentUiNode.IsTopLevelWindow)
                        {
                            parentUiNode = new UIAUiNode(UIAUiNode.UIAAutomation.FromHandle(parentUiNode.WindowHandle));
                        }

                        cachedParent = parentUiNode;
                    }
                }

                return cachedParent;
            }
        }

        public string ProcessName
        {
            get
            {
                return "";
            }
        }

        public string ProcessFullPath
        {
            get
            {
                return "";
            }
        }

        public string Role
        {
            get
            {
                if (propDict.ContainsKey("Role"))
                {
                    return propDict["Role"];
                }

                return "";
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                if (propDict.ContainsKey("WindowHandle"))
                {
                    return (IntPtr)Convert.ToInt32(propDict["WindowHandle"]);
                }

                return IntPtr.Zero;
            }
        }

        public string Description
        {
            get
            {
                if (propDict.ContainsKey("Description"))
                {
                    return propDict["Description"];
                }

                return "";
            }
        }

        public UiNode AutomationElementParent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FrameworkAutomationElementBase.IFrameworkPatterns Patterns => throw new NotImplementedException();

        internal static List<AccessibleJvm> EnumJvms(bool bRefresh = false)
        {
            if (bRefresh)
            {
                cachedJvms = null;
            }

            if (cachedJvms == null || cachedJvms.Count == 0)
            {
                cachedJvms = JavaUtils.EnumJvms();
            }

            return cachedJvms;
        }

        public void SetForeground()
        {
            //此处其实不会走进来，因为JAVA元素的直接父窗口为UIAUiNode
            if (WindowHandle != IntPtr.Zero)
            {
                UiCommon.SetForegroundWindow(WindowHandle);
            }
        }

        //public void MouseClick(UiElementClickParams clickParams = null)
        //{
        //    if (clickParams == null)
        //    {
        //        clickParams = new UiElementClickParams();
        //    }

        //    var clickablePoint = GetClickablePoint();
        //    if (clickParams.isMoveMouse)
        //    {
        //        Mouse.MoveTo(clickablePoint);
        //    }
        //    else
        //    {
        //        Mouse.Position = clickablePoint;
        //    }
        //    Mouse.LeftClick();
        //}

        //public void MouseDoubleClick(UiElementClickParams clickParams = null)
        //{
        //    if (clickParams == null)
        //    {
        //        clickParams = new UiElementClickParams();
        //    }

        //    var clickablePoint = GetClickablePoint();
        //    if (clickParams.isMoveMouse)
        //    {
        //        Mouse.MoveTo(clickablePoint);
        //    }
        //    else
        //    {
        //        Mouse.Position = clickablePoint;
        //    }

        //    Mouse.LeftDoubleClick();
        //}

        //public void MouseRightClick(UiElementClickParams clickParams = null)
        //{
        //    if (clickParams == null)
        //    {
        //        clickParams = new UiElementClickParams();
        //    }

        //    var clickablePoint = GetClickablePoint();
        //    if (clickParams.isMoveMouse)
        //    {
        //        Mouse.MoveTo(clickablePoint);
        //    }
        //    else
        //    {
        //        Mouse.Position = clickablePoint;
        //    }

        //    Mouse.RightClick();
        //}

        //public void MouseRightDoubleClick(UiElementClickParams clickParams = null)
        //{
        //    if (clickParams == null)
        //    {
        //        clickParams = new UiElementClickParams();
        //    }

        //    var clickablePoint = GetClickablePoint();
        //    if (clickParams.isMoveMouse)
        //    {
        //        Mouse.MoveTo(clickablePoint);
        //    }
        //    else
        //    {
        //        Mouse.Position = clickablePoint;
        //    }

        //    Mouse.RightDoubleClick();
        //}

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

        public Point GetClickablePoint()
        {
            //TODO WJF JAVA窗口最小化时，程序恢复显示时，取坐标为负数，原因不明
            var point = new Point(BoundingRectangle.Left + BoundingRectangle.Width / 2, BoundingRectangle.Top + BoundingRectangle.Height / 2);
            return point;
        }

        public void Focus()
        {

        }

        public List<UiNode> FindAll(TreeScope scope, ConditionBase condition)
        {
            throw new NotImplementedException();
        }

        public void SimulateTypeText(string text)
        {
            SetText(text);
        }

        public void SimulateClick()
        {
            DoActions("单击");
        }

        public void HighLight(Color? color = null, TimeSpan? duration = null, bool blocking = false)
        {
            SetForeground();
            Focus();
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
            return accessibleNode.GetAccessibleContextInfo().states_en_US.Split(',').Contains("checked");
        }

        public void Check()
        {
            if (!IsChecked())
            {
                Toggle();
            }
        }

        public void UnCheck()
        {
            if (IsChecked())
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            DoActions("单击");
        }

        public string GetText()
        {
            return accessibleNode.GetText();
        }

        public void SetText(string text)
        {
            accessibleNode.SetText(text);
        }

        public bool IsPassword()
        {
            return false;
        }

        public void ClearSelection()
        {
            accessibleNode.ClearSelection();
        }

        public void SelectItem(string item)
        {
            accessibleNode.SelectItem(item);
        }

        public void SelectMultiItems(string[] items)
        {
            accessibleNode.SelectItems(items);
        }

        public void ClearText()
        {
            accessibleNode.SetText("");
        }

        public object GetAttributeValue(string attributeName)
        {
            return propDict[attributeName];
        }

        public bool IsEnable()
        {
            return accessibleNode.GetAccessibleContextInfo().states_en_US.Split(',').Contains("enabled");
        }

        public bool IsVisible()
        {
            return accessibleNode.GetAccessibleContextInfo().states.Split(',').Contains("visible");
        }

        public bool IsTable()
        {
            if (accessibleNode.GetTableInfo()!=null)
            {
                return true;
            }

            return false;
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

        private void DoActions(params string[] actions)
        {
            accessibleNode.DoActions(actions);
        }
    }
}
