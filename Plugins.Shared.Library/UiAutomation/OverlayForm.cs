using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;
using Gma.UserActivityMonitor;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsAccessBridgeInterop;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.UiAutomation.IEBrowser;
using SAPFEWSELib;
using SapROTWr;
using Plugins.Shared.Library.Extensions;

namespace Plugins.Shared.Library.UiAutomation
{
    public partial class OverlayForm : Form
    {
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        const int WM_NCHITTEST = 0x0084;
        const int HTTRANSPARENT = -1;

        public bool IsWindowHighlight { get; internal set; }

        private bool enablePassThrough { get; set; }

        private static DispatcherTimer dispatcherTimer = new DispatcherTimer();

        private static DispatcherTimer keyStateDispatcherTimer = new DispatcherTimer();

        /// <summary>
        /// 四个边框
        /// </summary>
        private ExtendedPanel panelBorderLeft, panelBorderTop, panelBorderRight, panelBorderBottom;

        /// <summary>
        /// 内部边框
        /// </summary>
        private ExtendedPanel panelInside;

        /// <summary>
        /// 当前正处于高亮状态的元素
        /// </summary>
        internal UiNode CurrentHighlightElement;

        private Point OldScreenPoint = new Point(0, 0);
        private string OldSapElementStr = "";

        public OverlayForm()
        {
            InitializeComponent();

            this.panelBorderLeft = createPanel(Color.FromArgb(232, 193, 116));
            this.panelBorderTop = createPanel(Color.FromArgb(232, 193, 116));
            this.panelBorderRight = createPanel(Color.FromArgb(232, 193, 116));
            this.panelBorderBottom = createPanel(Color.FromArgb(232, 193, 116));

            this.panelInside = createPanel(Color.FromArgb(123, 159, 212));

            this.Cursor = UiCommon.GetCursor(Properties.Resources.cursor);
        }

        private void keyStateDispatcherTimer_Tick(object sender, EventArgs e)
        {
            const int MBUTTON = 0x04;
            if (UiCommon.GetAsyncKeyState(MBUTTON) != 0)
            {
                doDelaySelect();
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW;//避免ATL+TAB时显示该窗体
                cp.ExStyle |= WS_EX_NOACTIVATE;//禁止激活，以便能够inspect菜单条目
                return cp;
            }
        }

        public Point CurrentHighlightElementRelativeClickPos { get; private set; }
        public bool hasDoSelect { get; private set; }

        private ExtendedPanel createPanel(Color color)
        {
            var panel = new ExtendedPanel();
            panel.Size = new Size(0, 0);
            panel.Parent = this;
            panel.BackColor = color;
            panel.Show();
            return panel;
        }



        protected override void WndProc(ref Message m)
        {
            if (enablePassThrough)
            {
                if (m.Msg == WM_NCHITTEST)
                {
                    m.Result = (IntPtr)HTTRANSPARENT;
                    return;
                }
            }

            base.WndProc(ref m);
        }

        private void OverlayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }


        private void doCancel()
        {
            StopHighlight();
            Task.Run(() =>
            {
                this.Invoke(new Action(() =>
                {
                    UiElement.OnCancel?.Invoke();
                }));
            });
        }

        private void doDelaySelect()
        {
            StopHighlight(false);
            var countDown = new CountDown(5);
            countDown.Closed += CountDown_Closed;
            countDown.Show();
        }

        private void CountDown_Closed(object sender, EventArgs e)
        {
            StartHighlight();
        }

        private void doSelect()
        {
            lock (this)
            {
                if (hasDoSelect)
                {
                    return;
                }

                hasDoSelect = true;

                if (CurrentHighlightElement != null)
                {
                    if (enableJavaUiNode(CurrentHighlightElement))
                    {
                        return;
                    }

                    if (InstallBrowserExtension(CurrentHighlightElement))
                    {
                        return;
                    }

                    var element = new UiElement(CurrentHighlightElement);
                    element.RelativeClickPos = CurrentHighlightElementRelativeClickPos;

                    //隐藏窗体并截全屏，以便后期自己绘制红色标记矩形
                    this.Hide();
                    element.currentInformativeScreenshot = element.CaptureInformativeScreenshot();

                    //System.Diagnostics.Debug.WriteLine(string.Format("CurrentHighlightElementRelativeClickPos = ({0},{1})", CurrentHighlightElementRelativeClickPos.X, CurrentHighlightElementRelativeClickPos.Y));
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        this.Invoke(new Action(() =>
                        {
                            UiElement.OnSelected?.Invoke(element);
                        }));
                    });

                    StopHighlight();
                }
            }

        }

        public void MoveRect(Rectangle rect)
        {
            int margin = 5;

            panelBorderLeft.Location = new Point(rect.Left, rect.Top);
            panelBorderLeft.Size = new Size(margin, rect.Height);

            panelBorderTop.Location = new Point(rect.Left, rect.Top);
            panelBorderTop.Size = new Size(rect.Width, margin);

            panelBorderRight.Location = new Point((rect.Left + rect.Width - margin), rect.Top);
            panelBorderRight.Size = new Size(margin, rect.Height);

            panelBorderBottom.Location = new Point(rect.Left, (rect.Top + rect.Height - margin));
            panelBorderBottom.Size = new Size(rect.Width, margin);

            panelInside.Location = new Point(rect.Left + 5, rect.Top + 5);
            panelInside.Size = new Size(rect.Width - 10, rect.Height - 10);

        }

        internal void installHook()
        {
            //使用 https://www.codeproject.com/Articles/7294/Processing-Global-Mouse-and-Keyboard-Hooks-in-C?msg=5642094#xx5642094xx
            uninstallHook();
            HookManager.MouseClickExt += HookManager_MouseClickExt;
            HookManager.KeyUp += HookManager_KeyUp;
        }

        internal void uninstallHook()
        {
            HookManager.MouseClickExt -= HookManager_MouseClickExt;
            HookManager.KeyUp -= HookManager_KeyUp;
        }


        private void HookManager_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    doCancel();
                    break;
                case Keys.F2:
                    doDelaySelect();
                    break;
                default:
                    break;
            }
        }

        private void HookManager_MouseClickExt(object sender, MouseEventExtArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    doSelect();
                    e.Handled = true;
                    break;
                case MouseButtons.Right:
                    doCancel();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }


        internal void StopHighlight(bool isNeedShowMainWindow = true)
        {
            uninstallHook();

            dispatcherTimer.Stop();
            keyStateDispatcherTimer.Stop();

            this.Hide();
            panelBorderLeft.Size = panelBorderTop.Size = panelBorderRight.Size = panelBorderBottom.Size = panelInside.Size = new Size(0, 0);

            if (isNeedShowMainWindow)
            {
                System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
            }
        }

        internal void StartHighlight()
        {
            hasDoSelect = false;

            installHook();

            JavaUiNode.EnumJvms(true);//重刷JVM列表
            this.Show();

            dispatcherTimer.Tick -= new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
            dispatcherTimer.Start();

            keyStateDispatcherTimer.Tick -= new EventHandler(keyStateDispatcherTimer_Tick);
            keyStateDispatcherTimer.Tick += new EventHandler(keyStateDispatcherTimer_Tick);

            keyStateDispatcherTimer.Interval = TimeSpan.FromMilliseconds(30);
            keyStateDispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                enablePassThrough = true;

                var screenPoint = System.Windows.Forms.Cursor.Position;

                AutomationElement element = null;

                if (IsWindowHighlight)
                {
                    IntPtr hWnd = UiCommon.GetRootWindow(screenPoint);

                    if (hWnd != IntPtr.Zero)
                    {
                        element = UIAUiNode.UIAAutomation.FromHandle(hWnd);
                    }
                }
                else
                {
                    if (InspectHtmlUiNode(screenPoint))
                    {
                        return;
                    }
                    try
                    {
                        element = UIAUiNode.UIAAutomation.FromPoint(screenPoint);
                    }
                    catch (Exception)
                    {
                        if (element == null)
                        {
                            IntPtr hWnd = UiCommon.WindowFromPoint(screenPoint);

                            if (hWnd != IntPtr.Zero)
                            {
                                element = UIAUiNode.UIAAutomation.FromHandle(hWnd);
                            }
                        }
                    }
                    if (inspectJavaUiNode(element, screenPoint))
                    {
                        return;
                    }

                    if (inspectSAPUiNode(element, screenPoint))
                    {
                        return;
                    }

                }

                CurrentHighlightElement = new UIAUiNode(element);

                if (element == null)
                {
                    return;
                }

                var rect = element.BoundingRectangle;

                //计算鼠标点击时的相对元素的偏移,以供后期有必要时使用
                CurrentHighlightElementRelativeClickPos = new Point(screenPoint.X - rect.Left, screenPoint.Y - rect.Top);

                this.MoveRect(rect);
            }
            catch (Exception exp)
            {

                Console.WriteLine(exp.StackTrace);
            }
            finally
            {
                enablePassThrough = false;

                this.TopMost = true;
            }
        }
        private bool InspectHtmlUiNode(Point screenPoint)
        {
            Stopwatch stopwatch=new Stopwatch();
            stopwatch.Start();
            var window = UIAUiNode.UIAAutomation.FromHandle(Win32Api.WindowFromPoint(new Win32Api.POINT
            {
                x = screenPoint.X,
                y = screenPoint.Y
            }));
            var winNode = new UIAUiNode(window);
            var className = winNode.ClassName;
            stopwatch.Stop();
            Debug.WriteLine($"Start :{stopwatch.ElapsedMilliseconds}");
            IBrowser browser;
            if (className == "Chrome_RenderWidgetHostHWND")
            { 
                browser = new ChromeBrowser(winNode);
                if (browser.Available)
                {
                    var htmlNode = browser.GetElementFromPoint(screenPoint);
                    if (htmlNode == null) return false;

                    var rect = htmlNode.BoundingRectangle;

                    CurrentHighlightElement = htmlNode;
                    CurrentHighlightElementRelativeClickPos = new Point(screenPoint.X - rect.Left, screenPoint.Y - rect.Top);
                    MoveRect(rect);
                    return true;
                }
            }

            if (className == "MozillaWindowClass")
            {
                browser = new FirefoxBrowser(winNode);
                if (browser.Available)
                {
                    var htmlNode = browser.GetElementFromPoint(screenPoint);
                    if (htmlNode == null) return false;

                    var rect = htmlNode.BoundingRectangle;

                    CurrentHighlightElement = htmlNode;
                    CurrentHighlightElementRelativeClickPos = new Point(screenPoint.X - rect.Left, screenPoint.Y - rect.Top);
                    MoveRect(rect);
                    return true;
                }
            }

            if (className == "Internet Explorer_Server")
            {
                browser =new IeBrowser(winNode);
                if (browser.Available)
                {
                    var ele = browser.GetElementFromPoint(screenPoint);
                    if (ele == null) return false;
                    var rect = ele.BoundingRectangle;

                    CurrentHighlightElement = ele;
                    CurrentHighlightElementRelativeClickPos = new Point(screenPoint.X - rect.Left, screenPoint.Y - rect.Top);

                    MoveRect(rect);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 启用浏览器插件
        /// </summary>
        /// <param name="currentHighlightElement">当前选中的元素</param>
        /// <returns></returns>
        private bool InstallBrowserExtension(UiNode currentHighlightElement)
        {
            if (currentHighlightElement.ControlType == "HtmlNode")
            {
                return false;
            }
            if (currentHighlightElement.ClassName != "Chrome_RenderWidgetHostHWND")
            {
                return false;
            }
            if (NativeMessageManager.NativeHostHandle != IntPtr.Zero)
            {
                return false;
            }
            StopHighlight();
            UniMessageBox.Show("请尝试使用“工具->安装Chrome插件”后重试", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            return true;
        }


        private bool enableJavaUiNode(UiNode node)
        {
            if (node.WindowHandle != IntPtr.Zero && node.ProcessName == "javaw.exe" && !JavaUiNode.AccessBridge.Functions.IsJavaWindow(node.WindowHandle))
            {
                StopHighlight();
                //提示用户是否启用JAVA自动化
                var ret = UniMessageBox.Show("是否启用Java Access Bridge？", "询问", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question, System.Windows.MessageBoxResult.Yes);
                if (ret == System.Windows.MessageBoxResult.Yes)
                {
                    //java8及以后的版本直接调用命令行，之前的版本需要手动安装
                    var jabswitchExe = System.IO.Path.GetDirectoryName(node.ProcessFullPath) + @"\jabswitch.exe";
                    if (System.IO.File.Exists(jabswitchExe))
                    {
                        //存在jabswitch.exe，则可直接调用jabswitch.exe -enable来启用
                        UiCommon.RunProcess(jabswitchExe, "-enable", true);
                    }
                    else
                    {
                        //需要主动安装accessbridge相关文件(根据32位或64位的JRE进行拷贝)
                        var javaExe = System.IO.Path.GetDirectoryName(node.ProcessFullPath) + @"\java.exe";

                        string windowsHome = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                        string javaHome = System.IO.Directory.GetParent(System.IO.Path.GetDirectoryName(node.ProcessFullPath)).FullName;

                        InstallJavaAccessBridge(Environment.Is64BitOperatingSystem, UiCommon.IsExe64Bit(javaExe), windowsHome, javaHome);
                    }

                    UniMessageBox.Show("操作完成，请重新运行Java程序以便操作生效", "提示", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }

                return true;
            }

            return false;
        }

        public static void InstallJavaAccessBridge(bool isOperatingSystem64Bit, bool isJava64Bit, string windowsHome, string javaHome)
        {
            var jabPath = System.Environment.CurrentDirectory + @"\JAB";
            if (isOperatingSystem64Bit)
            {
                //Windows64位系统安装JAB
                UiCommon.CopyFileToDir(jabPath + @"\WindowsAccessBridge-64.dll", windowsHome + @"\SYSTEM32");
                UiCommon.CopyFileToDir(jabPath + @"\WindowsAccessBridge-32.dll", windowsHome + @"\SYSWOW64");

                if (isJava64Bit)
                {
                    UiCommon.CopyFileToDir(jabPath + @"\JavaAccessBridge-64.dll", javaHome + @"\bin");
                    UiCommon.CopyFileToDir(jabPath + @"\JAWTAccessBridge-64.dll", javaHome + @"\bin");
                    UiCommon.CopyFileToDir(jabPath + @"\access-bridge-64.jar", javaHome + @"\lib\ext");
                }
                else
                {
                    UiCommon.CopyFileToDir(jabPath + @"\JavaAccessBridge-32.dll", javaHome + @"\bin");
                    UiCommon.CopyFileToDir(jabPath + @"\JAWTAccessBridge-32.dll", javaHome + @"\bin");
                    UiCommon.CopyFileToDir(jabPath + @"\access-bridge-32.jar", javaHome + @"\lib\ext");
                }

                UiCommon.CopyFileToDir(jabPath + @"\accessibility.properties", javaHome + @"\lib");
                UiCommon.CopyFileToDir(jabPath + @"\jaccess.jar", javaHome + @"\lib\ext");
            }
            else
            {
                //Windows32位系统安装JAB
                UiCommon.CopyFileToDir(jabPath + @"\WindowsAccessBridge.dll", windowsHome + @"\SYSTEM32");

                UiCommon.CopyFileToDir(jabPath + @"\JavaAccessBridge.dll", javaHome + @"\bin");
                UiCommon.CopyFileToDir(jabPath + @"\JAWTAccessBridge.dll", javaHome + @"\bin");
                UiCommon.CopyFileToDir(jabPath + @"\access-bridge.jar", javaHome + @"\lib\ext");

                UiCommon.CopyFileToDir(jabPath + @"\accessibility.properties", javaHome + @"\lib");
                UiCommon.CopyFileToDir(jabPath + @"\jaccess.jar", javaHome + @"\lib\ext");
            }

        }



        /// <summary>
        /// 若element是JAVA窗口，则里面的元素要按照JAVA节点方式获取
        /// </summary>
        /// <param name="element"></param>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        private bool inspectJavaUiNode(AutomationElement element, Point screenPoint)
        {

            if (element != null)
            {
                //判断是否是JAVA窗口
                var node = new UIAUiNode(element);
                if (JavaUiNode.AccessBridge.Functions.IsJavaWindow(node.WindowHandle))
                {
                    //是JAVA窗口，内部节点按JAVA方式选择

                    Path<AccessibleNode> javaNodePath = JavaUiNode.EnumJvms().Select(javaNode => javaNode.GetNodePathAt(screenPoint)).Where(x => x != null).FirstOrDefault(); ;
                    var currentJavaNode = javaNodePath == null ? null : javaNodePath.Leaf;

                    if (currentJavaNode == null)
                    {
                        return false;
                    }

                    CurrentHighlightElement = new JavaUiNode(currentJavaNode);

                    var rect = currentJavaNode.GetScreenRectangle();

                    //计算鼠标点击时的相对元素的偏移,以供后期有必要时使用
                    CurrentHighlightElementRelativeClickPos = new Point(screenPoint.X - (int)rect?.Left, screenPoint.Y - (int)rect?.Top);

                    this.MoveRect((Rectangle)currentJavaNode.GetScreenRectangle());
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 若element是SAP窗口，则里面的元素要按照SAP节点方式获取
        /// </summary>
        /// <param name="element"></param>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        private bool inspectSAPUiNode(AutomationElement element, Point screenPoint)
        {
            try
            {
                if (element != null)
                {
                    //判断是否是SAP窗口
                    var node = new UIAUiNode(element);
                    if (node.ProcessName.Equals("saplogon.exe"))
                    {
                        if (screenPoint == OldScreenPoint)
                        {
                            return true;
                        }
                        OldScreenPoint = screenPoint;
                        //是SAP窗口，内部节点按SAP方式选择

                        if (SAPUiNode.SapGuiApp == null || SAPUiNode.SapConnection==null || SAPUiNode.SapSession==null)
                        {
                            SAPUiNode.Initialize();
                        }
                        //遮罩层会导致FindByPosition失效，因此暂时隐藏
                        this.Hide();
                        GuiCollection guiCollection = SAPUiNode.SapSession.FindByPosition(Cursor.Position.X, Cursor.Position.Y);
                        this.Show();
                        if (guiCollection.Count > 0)
                        {
                            string sapElementStr = (string)guiCollection.ElementAt(0);

                            if (sapElementStr == OldSapElementStr)
                            {
                                return true;
                            }

                            //statusbar选不到，指定id
                            if (sapElementStr.EndsWith("wnd[0]"))
                            {
                                sapElementStr = sapElementStr + @"/sbar/pane[0]";
                            }

                            //okcd选不到，指定id
                            else if (sapElementStr.EndsWith("tbar[1]"))
                            {
                                sapElementStr = sapElementStr.Replace("tbar[1]", @"tbar[0]/okcd");
                            }

                            GuiComponent sapElement = SAPUiNode.SapSession.FindById((string)sapElementStr);

                            int left = (int)(sapElement.GetType().InvokeMember("ScreenLeft", System.Reflection.BindingFlags.InvokeMethod, null, sapElement, null));
                            int top = (int)(sapElement.GetType().InvokeMember("ScreenTop", System.Reflection.BindingFlags.InvokeMethod, null, sapElement, null));
                            int width = (int)(sapElement.GetType().InvokeMember("Width", System.Reflection.BindingFlags.InvokeMethod, null, sapElement, null));
                            int height = (int)(sapElement.GetType().InvokeMember("Height", System.Reflection.BindingFlags.InvokeMethod, null, sapElement, null));

                            if (sapElementStr.EndsWith("/sbar/pane[0]"))
                            {
                                //sbar元素接口返回的top坐标有问题，暂时没找到原因和规律，先根据鼠标位置确定top
                                top = screenPoint.Y - 20;
                            }

                            Rectangle sapRect = new Rectangle(left, top, width, height);

                            CurrentHighlightElement = new SAPUiNode(sapElement, sapRect);
                            //计算鼠标点击时的相对元素的偏移,以供后期有必要时使用
                            CurrentHighlightElementRelativeClickPos = new Point(screenPoint.X - sapRect.Left, screenPoint.Y - sapRect.Top);

                            this.MoveRect(sapRect);
                            return true;
                        }
                    }
                }

                return false;
            }

            catch (Exception e)
            {
                this.Show();
                SAPUiNode.Release();
                return false;
            }

        }

    }




    /// <summary>
    /// 设置可穿透的Panel，以便标记框上的鼠标能选择下方窗体
    /// </summary>
    public class ExtendedPanel : Panel
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HTTRANSPARENT = -1;

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == (int)WM_NCHITTEST)
                message.Result = (IntPtr)HTTRANSPARENT;
            else
                base.WndProc(ref message);
        }
    }


}
