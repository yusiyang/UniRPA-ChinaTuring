using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
using System.Drawing;
using SAPFEWSELib;
using SapROTWr;
using FlaUI.Core.Input;
using System.Reflection;
using FlaUI.Core.Overlay;
using static Plugins.Shared.Library.Win32Api;
using Plugins.Shared.Library.WindowsAPI;

namespace Plugins.Shared.Library.UiAutomation
{
    class SAPUiNode : UiNode
    {
        internal GuiComponent SAPElement;

        internal Rectangle rectangle=new Rectangle(0,0,0,0);
        public static GuiApplication SapGuiApp { get; set; }
        public static GuiConnection SapConnection { get; set; }
        public static GuiSession SapSession { get; set; }

        /// <summary>
        /// 初始化SAP选择器，目前仅支持1个连接和1个会话
        /// </summary>
        public static void Initialize()
        {
            Release();
            SapROTWr.CSapROTWrapper sapROTWrapper = new SapROTWr.CSapROTWrapper();
            object SapGuilRot = sapROTWrapper.GetROTEntry("SAPGUI");
            object engine = SapGuilRot.GetType().InvokeMember("GetSCriptingEngine", System.Reflection.BindingFlags.InvokeMethod, null, SapGuilRot, null);
            SapGuiApp = engine as GuiApplication;
            var connectionCount = SapGuiApp.Connections.Count;
            var connectionLength = SapGuiApp.Connections.Length;
            if (connectionCount > 0)
            {
                SapConnection = SapGuiApp.Connections.ElementAt(0) as GuiConnection;
                SapSession = SapConnection.Children.ElementAt(0) as GuiSession;
            }
        }

        public static void Release()
        {
            SAPUiNode.SapConnection = null;
            SAPUiNode.SapGuiApp = null;
            SAPUiNode.SapSession = null;
        }
        //private UiNode cachedParent;

        public SAPUiNode(GuiComponent guiComponent,Rectangle rectangle)
        {
            SAPElement = guiComponent;
            this.rectangle = rectangle;
        }

        public SAPUiNode(GuiComponent guiComponent)
        {
            SAPElement = guiComponent;
        }

        public string UserDefineId
        {
            get
            {
                return SAPElement.Id;
            }
        }

        public string Idx
        {
            get
            {
                return "";
            }
        }

        public string AutomationId
        {
            get
            {
                return "";
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
                return "SAPNode";
            }
        }
        public string Name
        {
            get
            {
                return SAPElement.Name;
                //return (string)SAPElement.GetType().InvokeMember("ScreenLeft", System.Reflection.BindingFlags.InvokeMethod, null, SAPElement, null);
            }
        }

        public string Role
        {
            get
            {
                return SAPElement.Type;
            }
        }

        public string Description
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
                return "saplogon.exe";
            }
        }

        public string ProcessFullPath
        {
            get
            {
                return "";
            }
        }

        public IntPtr WindowHandle
        {
            get
            {
                return IntPtr.Zero;
            }
        }

        public IntPtr MainWindowHandle
        {
            get
            {
                return (IntPtr)SapSession.ActiveWindow.Handle;
            }
        }

        public UiNode Parent
        {
            get
            {
                return null;
            }
        }

        public UiNode AutomationElementParent
        {
            get
            {
                return null;
            }
        }

        public Rectangle BoundingRectangle
        {
            get
            {
                return rectangle;
            }
        }

        public List<UiNode> Children
        {
            get
            {
                var list = new List<UiNode>();
                GuiComponentCollection children = (GuiComponentCollection)SAPElement.GetType().InvokeMember("Children", System.Reflection.BindingFlags.InvokeMethod, null, SAPElement, null);
                int size = children.Length;
                for (int i = 0; i < size; i++)
                {
                    SAPUiNode child = new SAPUiNode(children.ElementAt(i));
                    list.Add(child);
                }
                return list;
            }
        }

        public bool IsTopLevelWindow
        {
            get
            {
                return WindowHandle != IntPtr.Zero;
            }
        }

        public FrameworkAutomationElementBase.IFrameworkPatterns Patterns
        {
            get
            {
                return null;
            }
        }


        public List<UiNode> FindAll(TreeScope scope, ConditionBase condition)
        {
            throw new NotImplementedException();
        }

        public void Focus()
        {
            ((GuiVComponent)SAPElement).SetFocus();
        }

        public UiNode GetChildByIdx(int idx)
        {
            return this.Children[idx];
        }

        public Point GetClickablePoint()
        {
            Point point = new Point(BoundingRectangle.Left + BoundingRectangle.Width / 2, BoundingRectangle.Top + BoundingRectangle.Height / 2);
            return point;
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

        public void SetForeground()
        {
            if (WindowHandle != IntPtr.Zero)
            {
                UiCommon.SetForegroundWindow(WindowHandle);
            }
        }

        public void SimulateTypeText(string text)
        {
            SetText(text);
        }

        public void SimulateClick()
        {
            string type = SAPElement.Type;
            if (type.Equals("GuiButton"))
            {
                ((GuiButton)SAPElement).Press();
            }

            else if (type.Equals("GuiCheckBox"))
            {
                Toggle();
            }

            else if (type.Equals("GuiRadioButton"))
            {
                ((GuiRadioButton)SAPElement).Select();
            }

            else
            {
                Focus();
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
            return (bool)SAPElement.GetType().InvokeMember("Selected", System.Reflection.BindingFlags.InvokeMethod, null, SAPElement, null);
        }

        public void Check()
        {
            object[] args = new object[1];
            args[0] = true;
            SAPElement.GetType().InvokeMember("Selected", System.Reflection.BindingFlags.InvokeMethod, null, SAPElement,args);
        }

        public void UnCheck()
        {
            object[] args = new object[1];
            args[0] = false;
            SAPElement.GetType().InvokeMember("Selected", System.Reflection.BindingFlags.InvokeMethod, null, SAPElement, args);
        }

        public void Toggle()
        {
            if (IsChecked())
            {
                UnCheck();
            }

            else
            {
                Check();
            }
        }

        public string GetText()
        {
            return ((GuiVComponent)SAPElement).Text;
        }

        public void SetText(string text)
        {
            ((GuiVComponent)SAPElement).Text = text;
        }

        public bool IsPassword()
        {
            string type=SAPElement.Type;
            return string.Equals(type, "GuiPasswordField", StringComparison.CurrentCultureIgnoreCase);
        }

        public void ClearSelection()
        {
            ((GuiGridView)SAPElement).ClearSelection();
        }

        public void SelectItem(string item)
        {
            ((GuiGridView)SAPElement).SelectContextMenuItemByText(item);
        }

        public void SelectMultiItems(string[] items)
        {
            throw new NotImplementedException();
        }

        public void ClearText()
        {
            ((GuiVComponent)SAPElement).Text = "";
        }

        public object GetAttributeValue(string attributeName)
        {
            return SAPElement.GetType().InvokeMember(attributeName, System.Reflection.BindingFlags.InvokeMethod, null, SAPElement, null);
        }

        public bool IsEnable()
        {
            throw new NotImplementedException();
        }

        public bool IsVisible()
        {
            GuiComponent element=SapSession.FindById(SAPElement.Id);
            int x = (int)element.GetType().InvokeMember("ScreenLeft", System.Reflection.BindingFlags.InvokeMethod, null, element, null);
            int y = (int)element.GetType().InvokeMember("ScreenTop", System.Reflection.BindingFlags.InvokeMethod, null, element, null);
            return (x > 0 && y > 0);
        }

        public bool IsTable()
        {
            return SAPElement.Type.Equals("GuiGridView");
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
