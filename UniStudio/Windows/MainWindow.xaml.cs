using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library.CodeCompletion;
using UniStudio.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Plugins.Shared.Library.Librarys;
using ActiproSoftware.Windows.Controls.Ribbon;
using UniStudio.ViewModel;
using ActiproSoftware.Windows.Themes;
using System.Xml;

namespace UniStudio.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CopyDataStruct
        {
            public IntPtr dwData;//用户定义数据  
            public int cbData;//字符串长度
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;//字符串
        }

        public MainWindow()
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // 当所有视图资源加载完成时
            InitSomeViewSource();

            var hwndSource = PresentationSource.FromVisual(Application.Current.MainWindow) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(WndProc);
            }
        }

        /// <summary>
        /// 不知道为什么，在有些时候需要重置以下资源，才能够生效
        /// 这包括：👇
        /// - 程序启动时，当所有视图资源加载完成的时候
        /// - 重置主题的时候
        /// </summary>
        private void InitSomeViewSource()
        {
            // 重新将左滑菜单栏打开，以此覆盖在顶层
            ViewModelLocator.instance.Main.IsOpenStartScreen = false;
            ViewModelLocator.instance.Main.IsOpenStartScreen = true;
            // 重新将左侧工具栏容器图标开关置 true
            ViewModelLocator.instance.Dock.m_view._leftToolWindowContainer.HasTabImages = false;
            ViewModelLocator.instance.Dock.m_view._leftToolWindowContainer.HasTabImages = true;
            // 重新将右侧属性栏容器图标开关置 true
            ViewModelLocator.instance.Dock.m_view._rightToolWindowContainer.HasTabImages = false;
            ViewModelLocator.instance.Dock.m_view._rightToolWindowContainer.HasTabImages = true;
            // 重新将底部输出栏容器图标开关置 true
            ViewModelLocator.instance.Dock.m_view._bottomToolWindowContainer.HasTabImages = false;
            ViewModelLocator.instance.Dock.m_view._bottomToolWindowContainer.HasTabImages = true;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var message = (WindowMessage)msg;
            var subCode = (WindowMessageParameter)wParam.ToInt32();

            if (message == WindowMessage.WM_COPYDATA)
            {
                CopyDataStruct cds = (CopyDataStruct)Marshal.PtrToStructure(lParam, typeof(CopyDataStruct));//从发送方接收到的数据结构
                string param = cds.lpData;//获取发送方传过来的消息

                Messenger.Default.Send(new MessengerObjects.CopyData(param));//广播消息 //Messenger.Default.Register<对象的类型>(对象, TOKEN字符串, (trans) => { });//注册
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            return IntPtr.Zero;
        }

        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                //此处获取UniStudio引用的所有程序集的代码必须放在UniStudio模块中调用，不能放到其它DLL中调用，否则获取有误
                Assembly target = Assembly.GetExecutingAssembly();
                //排除掉NPinyinPro库，该库导致执行代码组件无法正常编译运行，原因不明
                //List<Assembly> references = (from assemblyName in target.GetReferencedAssemblies() where assemblyName.Name != "NPinyinPro"
                //                             select Assembly.Load(assemblyName)).ToList();
                var references = AssemblyHelper.GetAllDependencies(target);

                EditorUtil.init(references.ToList());
            });
        }

        private void RibbonWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void WindowStateChanged(object sender, EventArgs e)
        {
            if (Application.Current.MainWindow.WindowState.Equals(WindowState.Maximized))
            {
                ViewModelLocator.instance.Main.MaximizedOrNormalImage = "pack://application:,,,/Resource/Image/Ribbon/window-normal.png";
                ViewModelLocator.instance.Main.MaximizedOrNormalToolTip = "还原";
            }
            else if (Application.Current.MainWindow.WindowState.Equals(WindowState.Normal))
            {
                ViewModelLocator.instance.Main.MaximizedOrNormalImage = "pack://application:,,,/Resource/Image/Ribbon/window-maximized.png";
                ViewModelLocator.instance.Main.MaximizedOrNormalToolTip = "最大化";
            }
        }
    }
}
