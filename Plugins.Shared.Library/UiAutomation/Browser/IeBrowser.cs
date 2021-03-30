using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ActiproSoftware.Windows.Extensions;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using MSHTML;
using SHDocVw;
using Point = System.Drawing.Point;

namespace Plugins.Shared.Library.UiAutomation.Browser
{
    public sealed class IeBrowser : IBrowser
    {
        #region 静态
        private static readonly ShellWindows ShellWindows = new ShellWindows();
        private static readonly List<WebBrowser> CachedExplorers = new List<WebBrowser>();
        static IeBrowser()
        {
            CacheBrowser();
            ShellWindows.WindowRegistered += WindowRegistered;
            ShellWindows.WindowRevoked += WindowRevoked;
        }

        private static void WindowRevoked(int lCookie)
        {
            CacheBrowser();
        }
        private static void WindowRegistered(int lCookie)
        {
            CacheBrowser();
        }

        private static void CacheBrowser()
        {
            CachedExplorers.Clear();
            foreach (InternetExplorer ie in ShellWindows)
            {
                var filename = Path.GetFileNameWithoutExtension(ie.FullName) ?? "";
                if (!filename.Equals("iexplore", StringComparison.OrdinalIgnoreCase)) continue;
                try
                {
                    var browser = ie as WebBrowser;
                    if (browser != null && CachedExplorers.All(t => t.HWND != browser.HWND))
                    {
                        CachedExplorers.Add(browser);
                    }
                }
                catch (Exception e)
                {
                }
            }
        }
        private static HTMLDocument FindDocument(string pageTitle, out IntPtr panelHandle)
        {
            panelHandle = IntPtr.Zero;
            Object domObject = new Object();
            int tempInt = 0;
            Guid guidIEDocument2 = new Guid(); //应该是IHTMLDocument2的接口id
            int WM_Html_GETOBJECT = Win32Api.RegisterWindowMessage("WM_Html_GETOBJECT");
            //开始查找指定的ie窗体

            IntPtr parentHwnd = Win32Api.FindWindow("IEFrame", null);

            var hwnds = Win32Api.FindChildWindowByClassName(parentHwnd, "Frame Tab");
            IntPtr hwnd = IntPtr.Zero;
            foreach (var handle in hwnds)
            {
                hwnd = handle;
                hwnd = Win32Api.FindWindowEx(hwnd, IntPtr.Zero, "TabWindowClass", pageTitle);
                if (hwnd == IntPtr.Zero)
                {
                    continue;
                }

                panelHandle = hwnd;
                hwnd = Win32Api.FindWindowEx(hwnd, IntPtr.Zero, "Shell DocObject View", null);
                hwnd = Win32Api.FindWindowEx(hwnd, IntPtr.Zero, "Internet Explorer_Server", null);
                break;
            }
            int w = Win32Api.SendMessage(hwnd, WM_Html_GETOBJECT, 0, ref tempInt);
            int _ = Win32Api.ObjectFromLresult(w, ref guidIEDocument2, 0, ref domObject);
            HTMLDocument doc = (HTMLDocument)domObject;
            return doc;
        }
        private static HTMLDocument FindDocument(UiNode node, out IntPtr panelHandle)
        {
            panelHandle = IntPtr.Zero;
            var handle = node.WindowHandle;
            var domObject = new object();
            Guid IID_IHTMLDocument2 = typeof(IHTMLDocument2).GUID;
            var tempInt = 0;
            var wmHtmlGetObject = Win32Api.RegisterWindowMessage("WM_Html_GETOBJECT");
            var lResult = Win32Api.SendMessage(handle, wmHtmlGetObject, 0, ref tempInt);
            _ = Win32Api.ObjectFromLresult(lResult, ref IID_IHTMLDocument2, 0, ref domObject);
            var doc = (HTMLDocument)domObject;

            if (doc != null)
            {
                panelHandle = node.Parent.Parent.WindowHandle;
            }

            return doc;
        }

        private static WebBrowser FindBrowser(HTMLDocument doc)
        {
            object objIWebBrowser2 = new object();
            var pro = doc as IServiceProvider;

            Guid IID_IWebBrowser2 = typeof(InternetExplorer).GUID;
            Guid SID_SWebBrowserApp = typeof(IWebBrowserApp).GUID;
            pro?.QueryService(ref SID_SWebBrowserApp, ref IID_IWebBrowser2, out objIWebBrowser2);
            return objIWebBrowser2 as WebBrowser;
        }
        #endregion

        public IeBrowser()
        {
            WebBrowser = CachedExplorers.LastOrDefault();
            Title = (WebBrowser?.Document as HTMLDocument)?.title;
            if (WebBrowser == null)
            {
                Available = false;
                return;
            }
            Hwd = (IntPtr)WebBrowser.HWND;
            Available = true;
        }
        public IeBrowser(UiNode node)
        {
            var doc = FindDocument(node, out IntPtr panelHandle);
            Title = doc.title;
            WebBrowser = FindBrowser(doc);
            if (WebBrowser == null)
            {
                Available = false;
                return;
            }
            Hwd = (IntPtr)WebBrowser.HWND;
            PanelHwd = panelHandle;
            Available = true;
        }

        public IeBrowser(string title)
        {
            Title = title;
            var doc = FindDocument(title, out IntPtr panelHandle);
            if (doc == null)
            {
                Available = false;
                return;
            }
            WebBrowser = FindBrowser(doc);
            PanelHwd = panelHandle;
            Hwd = (IntPtr)WebBrowser.HWND;
            Available = true;
        }


        internal WebBrowser WebBrowser { get; private set; }
        private AutomationElement Panel
        {
            get
            {
                using (var automation = UIAUiNode.UIAAutomation)
                {
                    if (PanelHwd != IntPtr.Zero)
                    {
                        return automation.FromHandle(PanelHwd);
                    }
                    var ele = automation.FromHandle(Hwd);
                    AutomationElement panel = null;
                    panel = ele.FindFirst(TreeScope.Descendants,
                        new AndCondition(new PropertyCondition(automation.PropertyLibrary.Element.ControlType, ControlType.Pane),
                            new PropertyCondition(automation.PropertyLibrary.Element.ClassName, "TabWindowClass"))); // Frame Tab
                    PanelHwd = panel.Properties.NativeWindowHandle;
                    return panel;
                }
            }
        }
        public string Title { get; }
        public BrowserType Type => BrowserType.InternetExplorer;
        public string Name => Type.ToString();
        public UiNode UiNode => new UIAUiNode(Panel);
        public IntPtr Hwd { get; }
        public IntPtr PanelHwd { get; private set; } = IntPtr.Zero;
        //{
        //    get
        //    {
        //        if (PanelDic.ContainsKey(Hwd))
        //        {
        //            return PanelDic[Hwd];
        //        }
        //        return Panel.Properties.NativeWindowHandle;
        //    }
        //}

        public bool Available { get; private set; }

        public bool Active => Panel.IsAvailable;

        public int Ratio => 1;
        public ReadyState ReadyState =>
            WebBrowser.ReadyState == tagREADYSTATE.READYSTATE_COMPLETE ? ReadyState.Complete :
            WebBrowser.ReadyState == tagREADYSTATE.READYSTATE_INTERACTIVE ? ReadyState.Interactive :
            WebBrowser.ReadyState == tagREADYSTATE.READYSTATE_LOADED ? ReadyState.Loaded :
            WebBrowser.ReadyState == tagREADYSTATE.READYSTATE_LOADING ? ReadyState.Loading :
            WebBrowser.ReadyState == tagREADYSTATE.READYSTATE_UNINITIALIZED ? ReadyState.UnInitialized :
            ReadyState.None;

        public void Navigate(Uri uri)
        {
            WebBrowser.Navigate(uri.ToString());
        }

        public void Close()
        {
            //WebBrowser.Quit();
            WebBrowser.ExecWB(OLECMDID.OLECMDID_CLOSE, OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER);
        }

        public void Refresh()
        {
            WebBrowser.Refresh();
        }

        public void Activate()
        {
            const int WM_UPDATEUISTATE = 0x0128;

            var handle = Win32Api.FindWindow("TabThumbnailWindow", $"{Panel.Name}");
            Win32Api.SetForegroundWindow(handle);
            Win32Api.SetForegroundWindow(WebBrowser.HWND);
        }

        public void Stop()
        {
            WebBrowser.Stop();
        }

        public void GoHome()
        {
            WebBrowser.GoHome();
        }

        public void GoForward()
        {
            WebBrowser.GoForward();
        }

        public void GoBack()
        {
            WebBrowser.GoBack();
        }

        public void Open(Uri uri, string arguments, int timeOut)
        {
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.UseShellExecute = true;
            procInfo.FileName = "iexplore.exe";
            procInfo.Arguments = $"{arguments} {uri}";
            var proc = Process.Start(procInfo);

            var autoSet = new AutoResetEvent(false);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var task = new Task(() =>
            {
                while (!CachedExplorers.Any(t => (IntPtr)t.HWND == proc.MainWindowHandle))
                {
                    Thread.Sleep(50);
                }
                Available = true;
                WebBrowser = CachedExplorers.Last(t => (IntPtr)t.HWND == proc.MainWindowHandle);
                autoSet.Set();
            }, token);
            task.Start();
            autoSet.WaitOne(timeOut);
            tokenSource.Cancel();
        }

        public void WaitPage(int timeOut = 1000)
        {
            var autoReset = new AutoResetEvent(false);
            WebBrowser.CommandStateChange += delegate
            {
                autoReset.Set();
            };
            autoReset.WaitOne(timeOut);
        }

        public UiNode FindHtmlNode(Dictionary<string, string> attrDic)
        {
            return null;
        }

        public UiNode FindHtmlNode(string title, string selector)
        {
            var doc = WebBrowser.Document as HTMLDocument;
            var ele = doc?.querySelector(selector);

            if (ele != null) return new IeNode(ele, this);

            //doc = FindDocument(title,out _);
            //WebBrowser = FindBrowser(doc);
            //ele = doc?.querySelector(selector);
            //if (ele != null) return new IeNode(ele, this);
            return null;
        }

        public UiNode GetElementFromPoint(Point screenPoint)
        {
            Win32Api.ScreenToClient(PanelHwd, ref screenPoint);
            //var ele = (WebBrowser?.Document as HTMLDocument)?.elementFromPoint(screenPoint.X - Panel.BoundingRectangle.X, screenPoint.Y - Panel.BoundingRectangle.Y);
            var ele = (WebBrowser?.Document as HTMLDocument)?.elementFromPoint(screenPoint.X, screenPoint.Y);
            if (ele == null)
            {
                return null;
            }
            var node = new IeNode(ele, this);
            return node;
        }

        public string InjectAndRunJs(string jsCode, string param, UiNode target)
        {
            var result = "";
            Application.Current.Dispatcher?.Invoke(() =>
            {
                result = (WebBrowser.Document as HTMLDocument)?.parentWindow.execScript(jsCode);
            });
            return result;
        }

        public object ExecuteScript(string script, params object[] args)
        {
            var result = "";
            if (!args.Any(t=>t is string))
            {
                Application.Current.Dispatcher?.Invoke(() =>
                {
                    result = (WebBrowser.Document as HTMLDocument)?.parentWindow.execScript(script);
                });
                return result;
            }

            var method = args.First(t=>t is string).ToString();
            var argList = args.ToList();
            argList.Remove(args.First(t => t is string));
            args = argList.ToArray();
            InjectJs(WebBrowser.Document as IHTMLDocument2, script);
            Application.Current.Dispatcher?.Invoke(() =>
                {
                    result = RunJs((WebBrowser.Document as HTMLDocument)?.parentWindow, method, args);
                });
            return result;
        }


        private void InjectJs(IHTMLDocument2 document, string jsCode)
        {
            MSHTML.IHTMLElement jsElement = document.createElement("script");
            jsElement.setAttribute("type", "text/javascript");
            jsElement.setAttribute("text", jsCode);
            ((IHTMLDOMNode)document.body).appendChild((IHTMLDOMNode)jsElement);
        }

        private string RunJs(IHTMLWindow2 windowObject, string method, params object[] args)
        {
            return windowObject.GetType().InvokeMember(method, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, windowObject, args, CultureInfo.CurrentCulture)?.ToString();
        }
    }
    [ComImport, Guid("6d5140c1-7436-11ce-8034-00aa006009fa"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IServiceProvider
    {
        void QueryService(ref Guid guidService,
            ref Guid riid,
            [MarshalAs(UnmanagedType.Interface)] out object ppvObject);
    }
    public enum BrowserNavConstants
    {
        navOpenInNewWindow = 1,
        navNoHistory = 2,
        navNoReadFromCache = 4,
        navNoWriteToCache = 8,
        navAllowAutosearch = 16,
        navBrowserBar = 32,
        navHyperlink = 64,
        navEnforceRestricted = 128,
        navNewWindowsManaged = 256,
        navUntrustedForDownload = 512,
        navTrustedForActiveX = 1024,
        navOpenInNewTab = 2048,
        navOpenInBackgroundTab = 4096,
        navKeepWordWheelText = 8192,
        navVirtualTab = 16384,
        navBlockRedirectsXDomain = 32768,
        navOpenNewForegroundTab = 65536
    }

    [Flags]
    public enum UIS
    {
        UIS_SET = 1,
        UIS_CLEAR = 2,
        UIS_INITIALIZE = 3,
        UISF_HIDEFOCUS = 0x1,
        UISF_HIDEACCEL = 0x2,
        UISF_ACTIVE = 0x4
    }

    public enum UISF
    {
        //UISF_HIDEFOCUS=0x1,
        //UISF_HIDEACCEL=0x2,
        //UISF_ACTIVE=0x4
    }
}
