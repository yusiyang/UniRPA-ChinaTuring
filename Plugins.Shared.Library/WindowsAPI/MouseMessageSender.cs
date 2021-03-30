using FlaUI.Core.WindowsAPI;
using Plugins.Shared.Library.UiAutomation;
using Plugins.Shared.Library.WindowsAPI.WindowMessageParameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.WindowsAPI
{
    /// <summary>
    /// 鼠标消息发送
    /// </summary>
    public class MouseMessageSender
    {
        static Dictionary<ClickType, Action<IntPtr, Point, MouseButton>> clickTypeActionDic;

        static MouseMessageSender()
        {
            clickTypeActionDic = new Dictionary<ClickType, Action<IntPtr, Point, MouseButton>>
            {
                {ClickType.Single,SingeClick },
                {ClickType.Double,DoubleClick },
                {ClickType.Down,Down },
                {ClickType.Up,Up }
            };
        }

        #region 鼠标点击消息发送
        /// <summary>
        /// 发送鼠标消息
        /// </summary>
        /// <param name="windowHandle">接收消息的窗口句柄</param>
        /// <param name="wPram"></param>
        /// <param name="x">鼠标横坐标</param>
        /// <param name="y">鼠标纵坐标</param>
        public static void SendMessage(IntPtr windowHandle,uint windowsMessages, uint wParam, int x, int y)
        {
            var lParam = (IntPtr)((y << 16) | (x & 0xffff));
            User32.SendMessage(windowHandle, windowsMessages, new IntPtr(wParam), lParam);
        }

        /// <summary>
        /// 发送鼠标左键按下事件消息
        /// </summary>
        /// <param name="windowHandle">接收消息的窗口句柄</param>
        /// <param name="x">鼠标横坐标</param>
        /// <param name="y">鼠标纵坐标</param>
        public static void SendLeftDownMessage(IntPtr windowHandle,int x,int y)
        {
            var wParam = new IntPtr(MouseWParams.MK_LBUTTON);
            var lParam= (IntPtr)((y << 16) | (x & 0xffff));
            User32.SendMessage(windowHandle, WindowsMessages.WM_LBUTTONDOWN, wParam, lParam);
        }

        /// <summary>
        /// 发送鼠标左键按下事件消息
        /// </summary>
        /// <param name="windowHandle">接收消息的窗口句柄</param>
        /// <param name="point">鼠标坐标位置</param>
        public static void SendLeftDownMessage(IntPtr windowHandle, Point point)
        {
            SendLeftDownMessage(windowHandle,point.X, point.Y);
        }

        /// <summary>
        /// 发送鼠标单击事件消息
        /// </summary>
        /// <param name="windowHandle">接收消息的窗口句柄</param>
        /// <param name="x">鼠标横坐标</param>
        /// <param name="y">鼠标纵坐标</param>
        public static void SendSingleClickMessage(IntPtr windowHandle, int x,int y)
        {
            SendLeftDownMessage(windowHandle,x,y );
            var lParam = (IntPtr)((y << 16) | (x & 0xffff));
            User32.SendMessage(windowHandle, WindowsMessages.WM_LBUTTONDOWN, IntPtr.Zero, lParam);
        }

        #endregion


        #region UiElement各种点击汇总
        public static void Move(IntPtr windowHandle, Point point, MouseButton mouseButton)
        {
            var windowMessage = WindowsMessages.WM_MOUSEMOVE;
            uint wParam;
            switch (mouseButton)
            {
                case MouseButton.Left:
                    wParam = MouseWParams.MK_LBUTTON;
                    break;
                case MouseButton.Middle:
                    wParam = MouseWParams.MK_MBUTTON;
                    break;
                case MouseButton.Right:
                    wParam = MouseWParams.MK_RBUTTON;
                    break;
                case MouseButton.XButton1:
                    wParam = MouseWParams.MK_XBUTTON1;
                    break;
                case MouseButton.XButton2:
                    wParam = MouseWParams.MK_XBUTTON2;
                    break;
                default:
                    wParam = MouseWParams.MK_LBUTTON;
                    break;
            }

            var lParam = (IntPtr)((point.Y << 16) | (point.X & 0xffff));
            Win32Api.PostMessage(windowHandle, windowMessage, new IntPtr(wParam), lParam);
        }

        public static void Up(IntPtr windowHandle, Point point, MouseButton mouseButton)
        {
            uint windowMessage, wParam;
            switch (mouseButton)
            {
                case MouseButton.Left:
                    windowMessage = WindowsMessages.WM_LBUTTONUP;
                    wParam = MouseWParams.MK_LBUTTON;
                    break;
                case MouseButton.Middle:
                    windowMessage = WindowsMessages.WM_MBUTTONUP;
                    wParam = MouseWParams.MK_MBUTTON;
                    break;
                case MouseButton.Right:
                    windowMessage = WindowsMessages.WM_RBUTTONUP;
                    wParam = MouseWParams.MK_RBUTTON;
                    break;
                case MouseButton.XButton1:
                    windowMessage = WindowsMessages.WM_XBUTTONUP;
                    wParam = MouseWParams.MK_XBUTTON1;
                    break;
                case MouseButton.XButton2:
                    windowMessage = WindowsMessages.WM_XBUTTONUP;
                    wParam = MouseWParams.MK_XBUTTON2;
                    break;
                default:
                    windowMessage = WindowsMessages.WM_LBUTTONUP;
                    wParam = MouseWParams.MK_LBUTTON;
                    break;
            }

            var lParam = (IntPtr)((point.Y << 16) | (point.X & 0xffff));
            Win32Api.PostMessage(windowHandle, windowMessage, IntPtr.Zero, lParam);
        }

        public static void Down(IntPtr windowHandle, Point point, MouseButton mouseButton)
        {
            uint windowMessage, wParam;
            switch (mouseButton)
            {
                case MouseButton.Left:
                    windowMessage = WindowsMessages.WM_LBUTTONDOWN;
                    wParam = MouseWParams.MK_LBUTTON;
                    break;
                case MouseButton.Middle:
                    windowMessage = WindowsMessages.WM_MBUTTONDOWN;
                    wParam = MouseWParams.MK_MBUTTON;
                    break;
                case MouseButton.Right:
                    windowMessage = WindowsMessages.WM_RBUTTONDOWN;
                    wParam = MouseWParams.MK_RBUTTON;
                    break;
                case MouseButton.XButton1:
                    windowMessage = WindowsMessages.WM_XBUTTONDOWN;
                    wParam = MouseWParams.MK_XBUTTON1;
                    break;
                case MouseButton.XButton2:
                    windowMessage = WindowsMessages.WM_XBUTTONDOWN;
                    wParam = MouseWParams.MK_XBUTTON2;
                    break;
                default:
                    windowMessage = WindowsMessages.WM_LBUTTONUP;
                    wParam = MouseWParams.MK_LBUTTON;
                    break;
            }

            var lParam = (IntPtr)((point.Y << 16) | (point.X & 0xffff));
            Win32Api.PostMessage(windowHandle, windowMessage, new IntPtr(wParam), lParam);
        }

        public static void SingeClick(IntPtr windowHandle, Point point, MouseButton mouseButton)
        {
            //Down(windowHandle, point, mouseButton);
            Move(windowHandle, point, mouseButton);
            Down(windowHandle, point, mouseButton);
            //Thread.Sleep(500);
            Up(windowHandle, point, mouseButton);
        }

        public static void DoubleClick(IntPtr windowHandle, Point point, MouseButton mouseButton)
        {
            //Down(windowHandle, point, mouseButton);
            Move(windowHandle, point, mouseButton);
            Down(windowHandle, point, mouseButton);
            ////Thread.Sleep(500);
            Up(windowHandle, point, mouseButton);
            Down(windowHandle, point, mouseButton);
            ////Thread.Sleep(500);
            Up(windowHandle, point, mouseButton);
        }

        public static void Double(IntPtr windowHandle, Point point, MouseButton mouseButton)
        {
            uint windowMessage, wParam;
            switch (mouseButton)
            {
                case MouseButton.Left:
                    windowMessage = WindowsMessages.WM_LBUTTONDBLCLK;
                    wParam = MouseWParams.MK_LBUTTON;
                    break;
                case MouseButton.Middle:
                    windowMessage = WindowsMessages.WM_MBUTTONDBLCLK;
                    wParam = MouseWParams.MK_MBUTTON;
                    break;
                case MouseButton.Right:
                    windowMessage = WindowsMessages.WM_RBUTTONDBLCLK;
                    wParam = MouseWParams.MK_RBUTTON;
                    break;
                case MouseButton.XButton1:
                    windowMessage = WindowsMessages.WM_XBUTTONDBLCLK;
                    wParam = MouseWParams.MK_XBUTTON1;
                    break;
                case MouseButton.XButton2:
                    windowMessage = WindowsMessages.WM_XBUTTONDBLCLK;
                    wParam = MouseWParams.MK_XBUTTON2;
                    break;
                default:
                    windowMessage = WindowsMessages.WM_LBUTTONDBLCLK;
                    wParam = MouseWParams.MK_LBUTTON;
                    break;
            }

            var lParam = (IntPtr)((point.Y << 16) | (point.X & 0xffff));
            Win32Api.PostMessage(windowHandle, windowMessage, new IntPtr(wParam), lParam);
        }

        public static void MouseMessageAction(IntPtr windowHandle, Point point, MouseButton mouseButton,ClickType clickType)
        {
            clickTypeActionDic[clickType].Invoke(windowHandle, point, mouseButton);
        }
        #endregion
    }
}
