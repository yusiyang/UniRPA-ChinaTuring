using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using WinApi.User32;
using static Plugins.Shared.Library.Win32Api;

namespace Plugins.Shared.Library.WindowsAPI
{
    public class WindowsHelper
    {
        public static bool IsTopMost(IntPtr hwnd)
        {
            var style = GetWindowLong(hwnd, GWL_EXSTYLE);
            style = style & WS_EX_TOPMOST;
            return style == WS_EX_TOPMOST;           
        }

        public static bool IsModal(IntPtr hwnd)
        {
            var style = GetWindowLong(hwnd, GWL_EXSTYLE);
            style = style & WS_EX_DLGMODALFRAME;
            return style == WS_EX_DLGMODALFRAME;
        }

        public static void CloseWindowsExcludeWindow(IntPtr excludeHwnd, int? processId=null)
        {
            if(processId==null)
            {
                processId = Process.GetCurrentProcess().Id;
            }

            EnumWindowsProcDelegate enumWindowsProc = (hWnd, lParam) =>
            {
                GetWindowThreadProcessId(hWnd, out int windowProcessId);
                if (windowProcessId == processId && excludeHwnd != hWnd && IsModal(hWnd) &&IsWindowVisible(new HandleRef(null, hWnd)))
                {
                    SendMessage(hWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                };
                return true;
            };
            EnumWindows(enumWindowsProc, IntPtr.Zero);
        }

        public static void SetForeground(System.Windows.Window window)
        {
            var hwndSource = PresentationSource.FromVisual(window) as HwndSource;

            User32Methods.SetForegroundWindow(hwndSource.Handle);
        }
    }
}
