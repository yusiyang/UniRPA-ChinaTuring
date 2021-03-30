using System.Collections.Generic;
using System.Xml;
using System;
using System.Drawing;
using System.Linq;
using FlaUI.Core.WindowsAPI;
using FlaUI.Core.Input;
using FlaUI.Core.Overlay;
using System.Threading;
using FlaUI.Core.Definitions;
using FlaUI.Core.Conditions;
using Plugins.Shared.Library.WindowsAPI;
using static Plugins.Shared.Library.Win32Api;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using SAPFEWSELib;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.UiAutomation.IEBrowser;
using System.IO;
using Plugins.Shared.Library.Extensions;

/// <summary>
/// 注意事项
/// 1.Gma.UserActivityMonitor.dll中SetWindowsHookEx的参数有修改，否则新版.NET会抛异常，进行了源码修改
/// 2.JAVA相关的WindowsAccessBridgeInterop.dll等dll的showing值在中文是“可见”，所以进行了特殊处理，进行了源码修改
/// </summary>

namespace Plugins.Shared.Library.UiAutomation
{
    public class UiElement:IJavaScriptExecutor
    {
        public delegate void UiElementSelectedEventHandler(UiElement uiElement);
        public delegate void UiElementCanceledEventHandler();
        public static UiElementSelectedEventHandler OnSelected;
        public static UiElementCanceledEventHandler OnCancel;

        internal UiNode uiNode;

        public Point RelativeClickPos { get; set; }//鼠标点击的坐标

        private static UiElement cacheDesktop;
        private string cachedId;
        private UiElement cachedParent;
        private UiElement automationElementParent;
        private UiElement cachedDirectTopLevelWindow;
        private Rectangle boundingRectangle;

        public static OverlayForm overlayForm;
        internal Bitmap currentInformativeScreenshot;
        private Bitmap currentDesktopScreenshot;

        static UiElement()
        {
            overlayForm = new OverlayForm();
            cacheDesktop = new UiElement(new UIAUiNode(UIAUiNode.UIAAutomation.GetDesktop()));
        }

        /// <summary>
        /// 初始化，由于JAVA的accessBridge.Initialize()生效有延迟，所以提前使其生效
        /// </summary>
        public static void Init()
        {
            JavaUiNode.EnumJvms(true);
        }

        internal UiElement(UiNode node, UiElement parent = null)
        {
            this.uiNode = node;
            this.Parent = parent;
            this.boundingRectangle = node?.BoundingRectangle ?? Rectangle.Empty;
        }

        /// <summary>
        /// 父级元素
        /// </summary>
        public UiElement Parent
        {
            get
            {
                if (cachedParent == null)
                {
                    if (uiNode.Parent != null)
                    {
                        cachedParent = new UiElement(uiNode.Parent);
                    }
                }

                return cachedParent;
            }

            private set
            {
                cachedParent = value;
            }
        }

        /// <summary>
        /// AutomationElement父级元素
        /// </summary>
        public UiElement AutomationElementParent
        {
            get
            {
                automationElementParent = new UiElement(uiNode.AutomationElementParent);
                return automationElementParent;
            }
        }

        /// <summary>
        /// 直接父级窗口
        /// </summary>
        private UiElement _closestWindowElement;
        public UiElement ClosestWindowElement
        {
            get
            {
                if (_closestWindowElement == null)
                {
                    var element = this;
                    while (element.WindowHandle == IntPtr.Zero)
                    {
                        element = element.Parent;
                    }
                    _closestWindowElement = element;
                }
                return _closestWindowElement;
            }
        }

        /// <summary>
        /// 桌面元素，为本地元素的根节点
        /// </summary>
        public static UiElement Desktop
        {
            get
            {
                if (cacheDesktop == null)
                {
                    var _rootElement = UIAUiNode.UIAAutomation.GetDesktop();
                    cacheDesktop = new UiElement(new UIAUiNode(_rootElement));
                }

                return cacheDesktop;
            }
        }

        /// <summary>
        /// 所有子元素
        /// </summary>
        public List<UiElement> Children
        {
            get
            {
                var list = new List<UiElement>();
                var children = uiNode.Children;
                foreach (var item in children)
                {
                    if (item != null)
                    {
                        list.Add(new UiElement(item, this));
                    }
                }

                return list;
            }
        }

        /// <summary>
        /// 控件类型
        /// </summary>
        public string ControlType
        {
            get
            {
                if (string.IsNullOrEmpty(uiNode.ControlType))
                {
                    return "Node";
                }
                else
                {
                    return uiNode.ControlType;
                }

            }
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get
            {
                return uiNode.Name;
            }
        }

        /// <summary>
        /// AutomationId
        /// </summary>
        public string AutomationId
        {
            get
            {
                return uiNode.AutomationId;
            }
        }

        /// <summary>
        /// 用户自定义Id
        /// </summary>
        public string UserDefineId
        {
            get
            {
                return uiNode.UserDefineId;
            }
        }

        /// <summary>
        /// 类名
        /// </summary>
        public string ClassName
        {
            get
            {
                return uiNode.ClassName;
            }
        }

        /// <summary>
        /// 所属进程名
        /// </summary>
        public string ProcessName
        {
            get
            {
                return uiNode.ProcessName;
            }
        }

        /// <summary>
        /// 所属进程全路径
        /// </summary>
        public string ProcessFullPath
        {
            get
            {
                return uiNode.ProcessFullPath;
            }
        }

        /// <summary>
        /// 角色
        /// </summary>
        public string Role
        {
            get
            {
                return uiNode.Role;
            }
        }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get
            {
                return uiNode.Description;
            }
        }

        /// <summary>
        /// 索引
        /// </summary>
        public string Idx
        {
            get
            {
                return uiNode.Idx;
            }
        }

        /// <summary>
        /// 把选择器中的‘*’转换为正则pattern，并判断输入字符串是否正则匹配
        /// </summary>
        /// <param name="str1">字符串1</param>
        /// <param name="str2">字符串2</param>
        /// <returns>是否正则匹配</returns>
        // pattern may contain '*' and '?'
        // 严格匹配，str的首尾必须和pattern的首尾匹配
        static bool isRegExpStringMatch(String str, String pattern)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            int i = 0;
            int j = 0;
            int mark = -1; // 最近一个*的下一个字符的位置
            while (i < str.Length && j < pattern.Length)
            {
                if (pattern[j] == '?')
                {
                    i++;
                    j++;
                    continue;
                }
                if (pattern[j] == '*')
                {
                    j++;
                    mark = j;
                    continue;
                }
                if (str[i] != pattern[j])
                {
                    if (mark < 0)
                    {
                        // pattern中没有*,直接return false
                        return false;
                    }
                    // * 之后出现了不匹配的字符
                    // j 回退到mark, i 比j少回退一步， mark之前是已经匹配成功的，i比j少回退一步，是因为i如果和j回退的步数一样，则相当于又重新从上一个*之后开始匹配，这已经比较过一遍，失配了，继续重新从这里匹配就会死循环了，所以i少回退一步，继续比较
                    i -= j - mark - 1;
                    j = mark;
                    continue;
                }
                i++;
                j++;
            }
            if (j == pattern.Length)
            {
                if (i == str.Length)
                {
                    return true;
                }
                if (pattern[j - 1] == '*')
                {
                    // str还有剩余字符，但pattern最后一个字符是*,匹配成功
                    return true;
                }
                return false;
            }
            while (j < pattern.Length)
            {
                if (pattern[j] != '*')
                {
                    // pattern还有剩余字符，且字符中有非*字符,匹配失败
                    return false;
                }
                j++;
            }
            return true;
        }

        //// str may contain '?'
        //static int[] getNextArray(String str)
        //{
        //    if (string.IsNullOrEmpty(str))
        //    {
        //        return null;
        //    }
        //    int[] next = new int[str.Length];
        //    int k = -1;
        //    int j = 0;
        //    next[0] = -1;
        //    while (j < str.Length - 1)
        //    {
        //        if (k == -1 || str[k] == str[j] || str[k] == '?' || str[j] == '?')
        //        {
        //            k++;
        //            j++;
        //            next[j] = k;
        //        }
        //        else
        //        {
        //            k = next[k];
        //        }
        //    }
        //    return next;
        //}

        //// pattern may contain '?'
        //static int kmpFind(String str, String pattern, int start)
        //{
        //    if (string.IsNullOrEmpty(str))
        //    {
        //        return -1;
        //    }
        //    int[] next = getNextArray(pattern);
        //    if (next == null)
        //    {
        //        return -1;
        //    }
        //    int i = start;
        //    while (i < str.Length)
        //    {
        //        int j = 0;
        //        while (j < pattern.Length)
        //        {
        //            if (str[i] == pattern[j] || pattern[j] == '?')
        //            {
        //                i++;
        //                j++;
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }
        //        i -= j;
        //        if (j == pattern.Length)
        //        {
        //            return i;
        //        }
        //        int move = j - next[j];
        //        i += move;
        //    }
        //    return -1;
        //}

        //// pattern may contain '*' and '?'
        //// pattern按*分割后，子串里可能含有?,没法用String.find, 所以针对含?的字符串，结合KMP算法，实现了find函数，之后再将pattern按*分割，在输入字符串中按顺序查找子串，已实现find含有*和?的字符串
        //static int find(String str, String pattern)
        //{
        //    if (string.IsNullOrEmpty(str))
        //    {
        //        return -1;
        //    }
        //    if (string.IsNullOrEmpty(pattern))
        //    {
        //        return -1;
        //    }
        //    String[] items = pattern.Split('*');
        //    int i = 0;
        //    int ret = -1;
        //    foreach (String s in items)
        //    {
        //        int index = kmpFind(str, s, i);
        //        if (index < 0)
        //        {
        //            return -1;
        //        }
        //        if (i == 0)
        //        {
        //            ret = index;
        //        }
        //        i = index + s.Length;
        //    }
        //    return ret;
        //}

        /// <summary>
        /// 判断元素是否匹配
        /// </summary>
        /// <param name="uiElement">元素</param>
        /// <param name="xmlElement">元素的xml属性集合</param>
        /// <returns></returns>
        private static bool isUiElementMatch(UiElement uiElement, XmlElement xmlElement)
        {
            //System.Diagnostics.Debug.WriteLine(uiElement.Id+"$$$$$"+ xmlElement.OuterXml);
            //有可能ControlType也会改变，可用Node匹配任何的ControlType类型
            if (xmlElement.Name == "Node" || xmlElement.Name == uiElement.ControlType || xmlElement.Name == "Window")
            {
                bool isMatch = true;

                foreach (XmlAttribute attr in xmlElement.Attributes)
                {
                    //正则匹配 *
                    if (attr.Name == "Name")
                    {
                        if (!isRegExpStringMatch(uiElement.Name, attr.Value))
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "AutomationId")
                    {
                        if (!isRegExpStringMatch(uiElement.AutomationId, attr.Value))
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "UserDefineId")
                    {
                        if (!isRegExpStringMatch(uiElement.UserDefineId, attr.Value))

                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "ClassName")
                    {
                        if (!isRegExpStringMatch(uiElement.ClassName, attr.Value))

                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "Role")
                    {
                        if (!isRegExpStringMatch(uiElement.Role, attr.Value))

                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "Description")
                    {
                        if (!isRegExpStringMatch(uiElement.Description, attr.Value))

                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else if (attr.Name == "ProcessName")
                    {
                        if (!isRegExpStringMatch(uiElement.ProcessName, attr.Value))
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    else
                    {

                    }
                }

                return isMatch;
            }


            return false;
        }

        /// <summary>
        /// 所有相关属性的集合即为Id
        /// </summary>
        public string Id
        {
            get
            {
                if (cachedId == null)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    //此处有修改，判断逻辑isUiElementMatch也要对应修改
                    //{{
                    var itemName = ControlType;
                    var itemElement = xmlDoc.CreateElement(itemName);

                    if (!string.IsNullOrEmpty(Name))
                    {
                        itemElement.SetAttribute("Name", Name);
                    }

                    if (!string.IsNullOrEmpty(AutomationId))
                    {
                        Regex numReg = new Regex("^[1-9]\\d*$");
                        if (!numReg.IsMatch(AutomationId))
                        {
                            itemElement.SetAttribute("AutomationId", AutomationId);
                        }
                    }

                    if (!string.IsNullOrEmpty(UserDefineId))
                    {
                        itemElement.SetAttribute("UserDefineId", UserDefineId);
                    }

                    if (!string.IsNullOrEmpty(ClassName))
                    {
                        if (!ClassName.Contains(":"))
                        {
                            itemElement.SetAttribute("ClassName", ClassName);
                        }
                    }

                    if (!string.IsNullOrEmpty(Role))
                    {
                        itemElement.SetAttribute("Role", Role);
                    }


                    if (!string.IsNullOrEmpty(Description))
                    {
                        itemElement.SetAttribute("Description", Description);
                    }


                    if (uiNode.IsTopLevelWindow)
                    {
                        if (!string.IsNullOrEmpty(ProcessName))
                        {
                            itemElement.SetAttribute("ProcessName", ProcessName);
                        }
                    }

                    if (uiNode is HtmlUiNode node)
                    {
                        if (!string.IsNullOrEmpty(ProcessName))
                        {
                            itemElement.SetAttribute("ProcessName", ProcessName);
                        }
                        itemElement.SetAttribute("Title", node.Title);

                        var nodeInfos = node.NodeHierarchyInfo.Where(t => t.IsPresentInSelector == 1).ToList();
                        nodeInfos.Reverse();
                        foreach (NodeHierarchy nodeInfo in nodeInfos)
                        {
                            //itemElement.SetAttribute("Index", nodeInfo.SelectorInfo.Index.ToString());
                            //foreach (KeyValuePair<string, string> attribute in nodeInfo.SelectorInfo.Attributes)
                            //{
                            //    itemElement.SetAttribute(attribute.Key, attribute.Value);
                            //}
                            var attrNode= xmlDoc.CreateElement("NodeInfo");
                            attrNode.SetAttribute("index", nodeInfo.SelectorInfo.Index.ToString());
                            foreach (var attribute in nodeInfo.SelectorInfo.Attributes)
                            {
                                attrNode.SetAttribute(attribute.Key, attribute.Value);
                            }
                            itemElement.AppendChild(attrNode);
                        }
                    }

                    if (uiNode is IeNode ieNode)
                    {
                        itemElement.SetAttribute("CssSelector", ieNode.CssSelector);
                    }
                    //没有任何属性时，Idx
                    if (!itemElement.HasAttributes || (itemElement.Attributes.Count == 1 && itemElement.HasAttribute("ClassName")))
                    {
                        if (!string.IsNullOrEmpty(Idx) && !string.Equals(Idx, "0"))
                        {
                            itemElement.SetAttribute("Idx", Idx);
                        }
                    }
                    //}}

                    cachedId = itemElement.OuterXml;
                }


                return cachedId;
            }
        }

        /// <summary>
        /// 是否为顶层窗口
        /// </summary>
        public bool IsTopLevelWindow
        {
            get
            {
                return uiNode.IsTopLevelWindow;
            }
        }

        /// <summary>
        /// 直接顶层窗口
        /// </summary>
        public UiElement DirectTopLevelWindow
        {
            get
            {
                if (cachedDirectTopLevelWindow == null)
                {
                    if (this.IsTopLevelWindow)
                    {
                        cachedDirectTopLevelWindow = this;
                    }
                    else
                    {
                        UiElement topLevelWindowToFind = this.Parent;
                        while (true)
                        {
                            if (topLevelWindowToFind != null)
                            {
                                if (topLevelWindowToFind.IsTopLevelWindow)
                                {
                                    cachedDirectTopLevelWindow = topLevelWindowToFind;
                                    break;
                                }

                                topLevelWindowToFind = topLevelWindowToFind.Parent;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
                return cachedDirectTopLevelWindow;
            }
        }

        /// <summary>
        /// 桌面位图
        /// </summary>
        /// <returns>位图格式的桌面截图</returns>
        internal static Bitmap CaptureDesktop()
        {
            return UIAUiNode.UIAAutomation.GetDesktop().Capture();
        }


        /// <summary>
        /// 当前元素对应的Handle句柄
        /// </summary>
        public IntPtr WindowHandle
        {
            get
            {
                return uiNode.WindowHandle;
            }
        }

        public IntPtr MainWindowHandle
        {
            get
            {
                if (this.IsSAPUiNode)
                {
                    return ((SAPUiNode)this.uiNode).MainWindowHandle;
                }
                else
                {
                    return this.DirectTopLevelWindow.WindowHandle;
                }
            }
        }

        /// <summary>
        /// 全局Id，即元素与其所有父元素的Id组合
        /// </summary>
        public string GlobalId
        {
            get
            {
                //递归获取父节点的Id和自己的Id结合起来以组成全局ID
                return Parent == null ? this.Id : Parent.GlobalId + this.Id;
            }
        }

        /// <summary>
        /// 经过转义处理后的全局Id
        /// </summary>
        public string Selector
        {
            get
            {
                return GlobalId.Replace("\"", "\'").Replace("\t", "&#9;");//双引号变单引号，以便被""引用
            }
        }

        /// <summary>
        /// 有换行的全局Id
        /// </summary>
        public string GlobalIdStyled
        {
            get
            {
                //递归获取父节点的Id和自己的Id结合起来以组成全局ID
                return Parent == null ? this.Id : Parent.GlobalIdStyled + Environment.NewLine + this.Id;
            }
        }

        /// <summary>
        /// 元素对应的矩形框
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                return this.boundingRectangle;
            }
            set
            {
                this.boundingRectangle = value;
            }
        }


        /// <summary>
        /// 返回封装的Native对象
        /// </summary>
        public object NativeObject
        {
            get
            {
                if (uiNode is UIAUiNode)
                {
                    return (uiNode as UIAUiNode).automationElement;
                }

                if (uiNode is JavaUiNode)
                {
                    return (uiNode as JavaUiNode).accessibleNode;
                }

                if (uiNode is SAPUiNode)
                {
                    return (uiNode as SAPUiNode).SAPElement;
                }

                return null;
            }

        }

        /// <summary>
        /// 是否NativeObject是UIA的AutomationElement元素
        /// </summary>
        public bool IsNativeObjectAutomationElement
        {
            get
            {
                return uiNode is UIAUiNode;
            }
        }

        /// <summary>
        /// 是否NativeObject是JAVA的AccessibleNode元素
        /// </summary>
        public bool IsNativeObjectAccessibleNode
        {
            get
            {
                return uiNode is JavaUiNode;
            }
        }

        /// <summary>
        /// 是否NativeObject是SAP的元素 
        /// </summary>
        public bool IsSAPUiNode
        {
            get
            {
                return uiNode is SAPUiNode;
            }
        }

        /// <summary>
        /// 元素的UIA Patterns
        /// </summary>
        public FrameworkAutomationElementBase.IFrameworkPatterns Patterns => uiNode.Patterns;
        private static readonly object Lock = new object();
        /// <summary>
        /// 通过选择器查找元素
        /// </summary>
        /// <param name="selector">选择器</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>查找结果</returns>
        public static UiElement FromSelector(string selector, Int32 timeout)
        {
            if (!string.IsNullOrEmpty(selector))
            {
                UiElement ret = null;
                var globalId = selector.Replace("\'", "\"");
                lock (Lock)
                {
                    var autoSet = new AutoResetEvent(false);
                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    CancellationToken token = tokenSource.Token;
                    Task waitTask = new Task(() =>
                    {
                        while (!tokenSource.IsCancellationRequested)
                        {
                            //System.Windows.Application.Current.Dispatcher?.Invoke(() => { ret = FromGlobalId(globalId); });
                            ret = FromGlobalId(globalId);
                            if (ret == null)
                            {
                                Thread.Sleep(50);
                                continue;
                            }
                            autoSet.Set();
                            break;
                        }
                    }, token);
                    waitTask.Start();
                    autoSet.WaitOne(timeout);
                    tokenSource.Cancel();
                }
                return ret;
            }

            return null;
        }

        /// <summary>
        /// 通过全局Id查找元素
        /// </summary>
        /// <param name="globalId">全局Id</param>
        /// <returns>查找结果</returns>
        public static UiElement FromGlobalId(string globalId)
        {
            //根据GlobalId返回UiElement元素
            //若为桌面元素，解析XML，按顺序搜索，直到找到；若为sap元素，则直接用UserDefineId找，其中UserDefineId即为SAP接口中session.findById的参数
            globalId = globalId.Replace(Environment.NewLine, "");
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(rootXmlElement);
            rootXmlElement.InnerXml = globalId;
            UiElement elementToFind = null;

            //FindUiElement(Desktop, rootXmlElement, ref elementToFind);

            //SAP元素
            if ((rootXmlElement.LastChild as XmlElement)?.Name == "SAPNode")
            {
                FindSAPElement(globalId, ref elementToFind);
            }
            //浏览器（谷歌）元素
            else if ((rootXmlElement.LastChild as XmlElement)?.Name == "HtmlNode")
            {
                //var attrDic = new Dictionary<string, string>();
                //if (rootXmlElement.LastChild?.Attributes != null)
                //{
                //    foreach (XmlAttribute attribute in rootXmlElement.LastChild?.Attributes)
                //    {
                //        attrDic.Add(attribute.Name, attribute.Value);
                //    }
                //}
                //IBrowser browser;
                //if (attrDic["ProcessName"] == "chrome.exe")
                //{
                //    browser = new ChromeBrowser(attrDic["Name"]);
                //}
                //else
                //{
                //    browser = new FirefoxBrowser(attrDic["Name"]);
                //}
                //if (browser.Available)
                //{
                //    var ele = browser.FindHtmlNode(attrDic);
                //    if (ele != null)
                //    {
                //        elementToFind = new UiElement(ele);
                //    }
                //}

                var node = (XmlElement) rootXmlElement.LastChild;
                var processName = node?.Attributes["ProcessName"].Value;

                IBrowser browser;
                if (processName == "chrome.exe")
                {
                    browser = new ChromeBrowser(node.Attributes["Title"].Value);
                }
                else
                {
                    browser = new FirefoxBrowser(node.Attributes["Title"].Value);
                }

                if (browser.Available)
                {
                    UiNode ele = null;
                    foreach (XmlNode nodeInfo in node.ChildNodes)
                    {
                        var attrDic = new Dictionary<string, string>();
                        foreach (XmlAttribute attribute in nodeInfo.Attributes)
                        {
                            attrDic.Add(attribute.Name, attribute.Value);
                        }
                        attrDic.Add("ParentCustomId",((HtmlUiNode)ele)?.CustomId);
                        ele = browser.FindHtmlNode(attrDic);
                    }

                    
                    if (ele != null)
                    {
                        elementToFind = new UiElement(ele);
                    }
                }
            }
            //浏览器（IE）元素
            else if ((rootXmlElement.LastChild as XmlElement)?.Name == "IEHtmlNode")
            {
                var title = rootXmlElement.GetElementsByTagName("Window")?[0]?.Attributes?["Name"].Value;
                var cssSelector = (rootXmlElement.LastChild as XmlElement)?.Attributes["CssSelector"].Value;

                try
                {
                    var browser = new IeBrowser(title);
                    if (browser.Available)
                    {
                        var ele = browser.FindHtmlNode(title, cssSelector);
                        if (ele != null)
                        {
                            elementToFind = new UiElement(ele);
                        }
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception(message: "请尝试使用“工具->配置IE浏览器”后重新打开浏览器");
                }
            }

            //本地元素或Java元素
            else
            {
                //List<UiElement> parentWindows = FindElementParentWindow(globalId);

                //若GlobalId中元素节点仅包含进程信息时，将GlobalId中去除该进程前置所有节点信息
                //XmlNodeList nodes = rootXmlElement.ChildNodes;
                //int windowNodeIdx = 0;
                //string processName = "";
                //for (int i = 0; i < nodes.Count; i++)
                //{
                //    if ((nodes[i] as XmlElement).HasAttribute("ProcessName"))
                //    {
                //        windowNodeIdx = i;
                //        processName = (nodes[i] as XmlElement).GetAttribute("ProcessName");
                //        break;
                //    }
                //}
                //if (windowNodeIdx > 0 && !processName.Equals("explorer.exe"))
                //{
                //    for (int i = 0; i < windowNodeIdx; i++)
                //    {
                //        rootXmlElement.RemoveChild(rootXmlElement.FirstChild);
                //        //Console.WriteLine(rootXmlElement.InnerXml);
                //    }
                //}

                //foreach (UiElement parentWindow in parentWindows)
                //{
                //    FindUiElement(parentWindow, rootXmlElement, ref elementToFind);
                //    if (elementToFind != null)
                //    {
                //        break;
                //    }
                //}
                System.Windows.Application.Current.Dispatcher?.Invoke(() => {
                    FindUiElement(Desktop, rootXmlElement, ref elementToFind);
                });
            }
            return elementToFind;
        }

        /// <summary>
        /// 根据GlobalId查找其所在窗口元素
        /// </summary>
        /// <param name="GlobalId">全局ID</param>
        /// <returns>符合条件的窗口元素</returns>
        private static List<UiElement> FindElementParentWindow(string GlobalId)
        {
            string processName = "";

            //不从Desktop搜索，改从进程列表搜索
            //从GlobalId中筛选出processName
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(rootXmlElement);
            rootXmlElement.InnerXml = GlobalId;
            XmlNodeList nodes = rootXmlElement.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                XmlAttributeCollection attributes = node.Attributes;
                foreach (XmlAttribute attribute in attributes)
                {
                    if (attribute.Name.Equals("ProcessName", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        processName = attribute.Value;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(processName))
                {
                    break;
                }

            }

            List<UiElement> parentWindows = new List<UiElement>();

            //进程是桌面浏览器时特殊处理，直接返回Desktop，否则可能会定位错窗口
            if (processName.Equals("explorer.exe"))
            {
                parentWindows.Add(Desktop);
                return parentWindows;
            }
            //根据processName查询当前进程列表，筛选出所有processName相同的进程实例
            int dotIndex = processName.LastIndexOf(".");
            Process[] processes = Process.GetProcessesByName(processName.Substring(0, dotIndex));
            //根据进程实例获取进程当前窗口句柄

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    IntPtr windowHandle = process.MainWindowHandle;

                    //通过窗口句柄构造UiElement，为父元素
                    var _rootElement = UIAUiNode.UIAAutomation.FromHandle(windowHandle);
                    var cacheParentWindow = new UiElement(new UIAUiNode(_rootElement));
                    parentWindows.Add(cacheParentWindow);
                }
            }

            else
            {
                parentWindows.Add(Desktop);
            }
            return parentWindows;
        }

        /// <summary>
        /// 根据全局Id查找SAP元素
        /// </summary>
        /// <param name="Id">全局Id</param>
        /// <param name="elementToFind">查找到的元素</param>
        /// <returns>是否查找成功</returns>
        private static bool FindSAPElement(string Id, ref UiElement elementToFind)
        {
            try
            {
                if (SAPUiNode.SapGuiApp == null || SAPUiNode.SapSession == null || SAPUiNode.SapConnection == null)
                {
                    SAPUiNode.Initialize();
                }

                string UserDefineId = SAPGLobalIdToUserDefineId(Id);

                GuiComponent sapElement = SAPUiNode.SapSession.FindById(UserDefineId);

                Rectangle sapRect = GetSAPElementBoundingRectangle(sapElement);

                SAPUiNode sapUiNode = new SAPUiNode(sapElement, sapRect);
                elementToFind = new UiElement(sapUiNode);

                return elementToFind != null;
            }
            catch
            {
                SAPUiNode.Release();
                return false;
            }

        }

        /// <summary>
        /// 获取SAP元素的矩形框
        /// </summary>
        /// <param name="sapElement"></param>
        /// <returns></returns>
        private static Rectangle GetSAPElementBoundingRectangle(GuiComponent sapElement)
        {
            int left = (int)sapElement.GetType().InvokeMember("ScreenLeft", System.Reflection.BindingFlags.InvokeMethod, null, sapElement, null);
            int top = (int)sapElement.GetType().InvokeMember("ScreenTop", System.Reflection.BindingFlags.InvokeMethod, null, sapElement, null);
            int width = (int)sapElement.GetType().InvokeMember("Width", System.Reflection.BindingFlags.InvokeMethod, null, sapElement, null);
            int height = (int)sapElement.GetType().InvokeMember("Height", System.Reflection.BindingFlags.InvokeMethod, null, sapElement, null);

            Rectangle sapRect = new Rectangle(left, top, width, height);
            return sapRect;
        }

        /// <summary>
        /// 从SAP元素的全局Id中提取UserDefineId
        /// </summary>
        /// <param name="Id">全局Id</param>
        /// <returns>用户自定义Id</returns>
        private static string SAPGLobalIdToUserDefineId(string Id)
        {
            string userDefineId = "";
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(Id);
            userDefineId = xmlDocument.FirstChild.Attributes["UserDefineId"].Value;
            return userDefineId;
        }

        /// <summary>
        /// 从元素及其子节点中查找xmlElement对应的匹配元素
        /// </summary>
        /// <param name="uiElement">查找范围中的顶级元素</param>
        /// <param name="_xmlElement">要查找的元素的xml</param>
        /// <param name="elementToFind">查找到的元素</param>
        /// <returns></returns>
        private static bool FindUiElement(UiElement uiElement, XmlElement _xmlElement, ref UiElement elementToFind)
        {
            // PropertyCondition propertyCondition = UIAUiNode.UIAAutomation.ConditionFactory.ByAutomationId("System.PercentFull");
            // PropertyCondition propertyCondition1 = UIAUiNode.UIAAutomation.ConditionFactory.ByClassName("CabinetWClass");

            // AutomationElement desk= UIAUiNode.UIAAutomation.GetDesktop();
            //AutomationElement window = desk.FindFirst(TreeScope.Children, propertyCondition1);
            // AutomationElement[] automationElements= window.FindAll(TreeScope.Descendants, propertyCondition);

            // Console.WriteLine(automationElements.Length);


            var xmlElement = _xmlElement.CloneNode(true) as XmlElement;
            if (isUiElementMatch(uiElement, xmlElement.FirstChild as XmlElement))
            {
                //xmlElement减去第一个xml节点
                xmlElement.RemoveChild(xmlElement.FirstChild);

                if (!xmlElement.HasChildNodes)
                {
                    //XML所有节点已经搜索完了
                    elementToFind = uiElement;
                    return true;
                }

                //xmlElement如果有idx属性，则优先跳到Idx对应的节点进行搜索
                if ((xmlElement.FirstChild as XmlElement).HasAttribute("Idx"))
                {
                    var idxStr = (xmlElement.FirstChild as XmlElement).Attributes["Idx"].Value;
                    var idx = Convert.ToInt32(idxStr);


                    var element = uiElement.GetUiElementByIdx(idx);
                    if (element != null)
                    {
                        if (FindUiElement(element, xmlElement, ref elementToFind))
                        {
                            return elementToFind != null;
                        }
                    }
                }

                foreach (var item in uiElement.Children)
                {
                    if (FindUiElement(item, xmlElement, ref elementToFind))
                    {
                        break;
                    }
                }
            }

            return elementToFind != null;
        }

        /// <summary>
        /// 通过索引获取元素
        /// </summary>
        /// <param name="idx">索引</param>
        /// <returns>获取到的元素</returns>
        private UiElement GetUiElementByIdx(int idx)
        {
            var item = uiNode.GetChildByIdx(idx);
            return new UiElement(item, this);
        }

        /// <summary>
        /// 选取元素时的截图
        /// </summary>
        /// <returns>位图格式的截图</returns>
        public Bitmap CaptureInformativeScreenshot()
        {
            currentDesktopScreenshot = CaptureDesktop();

            if (uiNode.BoundingRectangle.IsEmpty)
            {
                //QQ截取时会出现为空的情况
                return null;
            }

            Bitmap target = drawInformativeOnUiNode(currentDesktopScreenshot);

            return target;
        }

        /// <summary>
        /// 获取UiElement的截图(原始尺寸)
        /// </summary>
        /// <returns></returns>
        public Bitmap CaptureScreenshot()
        {
            if (currentDesktopScreenshot == null)
            {
                currentDesktopScreenshot = CaptureDesktop();
            }

            if (uiNode.BoundingRectangle.IsEmpty)
            {
                //QQ截取时会出现为空的情况
                return null;
            }

            Rectangle cropRect = uiNode.BoundingRectangle;
            this.boundingRectangle = uiNode.BoundingRectangle;
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(currentDesktopScreenshot, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return target;
        }

        /// <summary>
        /// 在截图上画红色边框
        /// </summary>
        /// <param name="currentDesktopScreenshot"></param>
        /// <returns></returns>
        private Bitmap drawInformativeOnUiNode(Bitmap currentDesktopScreenshot)
        {
            using (Graphics g = Graphics.FromImage(currentDesktopScreenshot))
            {
                //最终截图的红色标记边框
                Pen pen = new Pen(Color.Red, 2);
                g.DrawRectangle(pen, uiNode.BoundingRectangle);
            }

            Rectangle cropRect = uiNode.BoundingRectangle;
            this.boundingRectangle = uiNode.BoundingRectangle;
            cropRect.Inflate(100, 50);
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using (Graphics g = Graphics.FromImage(target))
            {
                g.DrawImage(currentDesktopScreenshot, new Rectangle(0, 0, target.Width, target.Height),
                                 cropRect,
                                 GraphicsUnit.Pixel);
            }

            return target;
        }

        /// <summary>
        /// 截取提示性的信息，会扩大截取范围，并带有指示框，以方便观察
        /// </summary>
        /// <param name="filePath">不提供的话程序自动生成全局唯一名字，并在项目当前目录下的.screenshots目录下生成</param>
        public string CaptureInformativeScreenshotToFile()
        {

            var guid = Guid.NewGuid().ToString("N");

            var screenshotsPath = Path.Combine(SharedObject.Instance.ProjectPath, ".screenshots");
            if (!Directory.Exists(screenshotsPath))
            {
                Directory.CreateDirectory(screenshotsPath);
            }

            var filePath = Path.Combine(screenshotsPath, guid + ".png");


            if (currentInformativeScreenshot == null)
            {
                //之前生成框选元素图时出错，需要重新生成
                currentInformativeScreenshot = drawInformativeOnUiNode(currentDesktopScreenshot);
            }

            currentInformativeScreenshot.Save(filePath);

            return Path.GetFileName(filePath);
        }

        /// <summary>
        /// 元素方式高亮
        /// </summary>
        public static void StartElementHighlight()
        {
            StartHighlight(false);
        }

        /// <summary>
        /// 只高亮顶层窗口
        /// </summary>
        public static void StartWindowHighlight()
        {
            StartHighlight(true);
        }

        /// <summary>
        /// 开始高亮显示鼠标所在位置的元素或其所在窗口对应的元素
        /// </summary>
        /// <param name="isWindowHighlight"></param>
        private static void StartHighlight(bool isWindowHighlight)
        {
            System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;

            overlayForm.IsWindowHighlight = isWindowHighlight;
            overlayForm.StartHighlight();
        }

        #region 元素操作事件

        /// <summary>
        /// 高亮一个元素
        /// </summary>
        /// <param name="color">高亮的颜色</param>
        /// <param name="duration">延迟时间</param>
        /// <param name="blocking">是否堵塞</param>
        public void DrawHighlight(Color? color = null, TimeSpan? duration = null, bool blocking = false)
        {
            try
            {
                uiNode.HighLight(color, duration,true);
            }
            catch
            {
                UniMessageBox.Show("请检查是否相关程序已关闭或界面元素内容已改变，并重新选取元素。", "查找元素失败", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 将窗口置前
        /// </summary>
        public void SetForeground()
        {
            try
            {
                var directWindow = DirectTopLevelWindow;
                if (directWindow != null)
                {
                    UiCommon.ForceShow(directWindow.WindowHandle);
                    DirectTopLevelWindow.uiNode.SetForeground();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 根据过滤字符串来查找元素节点
        /// </summary>
        /// <param name="scope">查找范围</param>
        /// <param name="condition">查找条件</param>
        /// <param name="filterStr">过滤字符串</param>
        /// <returns>查找到的元素集合</returns>
        public List<UiElement> FindAllByFilter(TreeScope scope, ConditionBase condition, string filterStr)
        {
            List<UiElement> uiList = new List<UiElement>();
            List<UiElement> foundUiList = new List<UiElement>();

            filterStr = filterStr.Replace(Environment.NewLine, "");
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement rootXmlElement = xmlDoc.CreateElement("Root");
            xmlDoc.AppendChild(rootXmlElement);
            rootXmlElement.InnerXml = filterStr;

            var filterElement = rootXmlElement.CloneNode(true) as XmlElement;
            foundUiList = FindAll(scope, condition);

            foreach (var element in foundUiList)
            {
                if (isUiElementMatch(element as UiElement, filterElement))
                {
                    uiList.Add(element);
                }
            }
            return uiList;
        }

        /// <summary>
        /// 根据条件查找元素节点
        /// </summary>
        /// <param name="scope">查找范围</param>
        /// <param name="condition">查找条件</param>
        /// <returns>查找到的元素集合</returns>
        public List<UiElement> FindAll(TreeScope scope, ConditionBase condition)
        {
            List<UiElement> uiList = new List<UiElement>();

            var list = new List<UiElement>();
            var children = uiNode.FindAll(scope, condition);
            foreach (var item in children)
            {
                list.Add(new UiElement(item, this));
            }
            return list;
        }

        /// <summary>
        /// 获取元素可点击坐标点
        /// </summary>
        /// <returns></returns>
        public Point GetClickablePoint()
        {
            return uiNode.GetClickablePoint();
        }

        /// <summary>
        /// 聚焦
        /// </summary>
        public void Focus()
        {
            uiNode.Focus();
        }

        #endregion


        #region 键盘操作事件
        /// <summary>
        /// 按下
        /// </summary>
        /// <param name="virtualKey">按键码</param>
        public static void KeyboardPress(VirtualKey virtualKey)
        {
            Keyboard.Press((VirtualKeyShort)virtualKey);
        }

        /// <summary>
        /// 松开
        /// </summary>
        /// <param name="virtualKey">按键码</param>
        public static void KeyboardRelease(VirtualKey virtualKey)
        {
            Keyboard.Release((VirtualKeyShort)virtualKey);
        }

        /// <summary>
        /// 普通录入
        /// </summary>
        /// <param name="text">被录入的文本</param>
        public static void KeyboardType(string text)
        {
            Keyboard.Type(text);
        }

        /// <summary>
        /// 模拟录入
        /// </summary>
        /// <param name="text">被录入的文本</param>
        public void SimulateTypeText(string text)
        {
            uiNode.SimulateTypeText(text);
        }
        #endregion


        #region 鼠标操作事件

        /// <summary>
        /// 鼠标点击操作集合
        /// </summary>
        /// <param name="clickParams">点击参数</param>
        public void MouseClick(UiElementClickParams clickParams = null)
        {
            SetForeground();
            uiNode.MouseClick(clickParams);
        }

        /// <summary>
        /// 鼠标悬浮操作集合
        /// </summary>
        /// <param name="hoverParams">悬浮参数</param>
        public void MouseHover(UiElementHoverParams hoverParams = null)
        {
            SetForeground();
            uiNode.MouseHover(hoverParams);
        }

        /// <summary>
        /// 鼠标拖拽
        /// </summary>
        /// <param name="mouseButton">鼠标按键</param>
        /// <param name="startingPoint">起始点</param>
        /// <param name="endingPoint">结束点</param>
        public static void MouseDrag(MouseButton mouseButton, Point startingPoint, Point endingPoint)
        {
            Mouse.Drag(startingPoint, endingPoint.X - startingPoint.X, endingPoint.Y - startingPoint.Y, (FlaUI.Core.Input.MouseButton)mouseButton);
        }

        /// <summary>
        /// 弹起
        /// </summary>
        /// <param name="mouseButton">鼠标按键</param>
        public static void MouseUp(MouseButton mouseButton)
        {
            Mouse.Up((FlaUI.Core.Input.MouseButton)mouseButton);
        }

        /// <summary>
        /// 按下
        /// </summary>
        /// <param name="mouseButton">鼠标按键</param>
        public static void MouseDown(MouseButton mouseButton)
        {
            Mouse.Down((FlaUI.Core.Input.MouseButton)mouseButton);
        }

        /// <summary>
        /// 移动（有移动过程）
        /// </summary>
        /// <param name="newPosition">要移到的坐标点</param>
        public static void MouseMoveTo(Point newPosition)
        {
            Mouse.MoveTo(newPosition);
        }

        /// <summary>
        /// 移动（有移动过程）
        /// </summary>
        /// <param name="newX">要移到的坐标点的X坐标</param>
        /// <param name="newY">要移到的坐标点的Y坐标</param>
        public static void MouseMoveTo(int newX, int newY)
        {
            Mouse.MoveTo(newX, newY);
        }

        /// <summary>
        /// 移动（无移动过程）
        /// </summary>
        /// <param name="newPosition">要移到的坐标点</param>
        public static void MouseSetPostion(Point newPosition)
        {
            Mouse.Position = newPosition;
        }
        /// <summary>
        /// 移动（无移动过程）
        /// </summary>
        /// <param name="newX">要移到的坐标点的X坐标</param>
        /// <param name="newY">要移到的坐标点的Y坐标</param>
        public static void MouseSetPostion(int newX, int newY)
        {
            Mouse.Position = new Point(newX, newY);
        }


        /// <summary>
        /// 垂直滚动
        /// </summary>
        /// <param name="lines">滚动行数</param>
        public static void MouseVerticalScroll(double lines)
        {
            Mouse.Scroll(lines);
        }

        /// <summary>
        /// 水平滚动
        /// </summary>
        /// <param name="lines">滚动行数</param>
        public static void MouseHorizontalScroll(double lines)
        {
            Mouse.HorizontalScroll(lines);
        }

        /// <summary>
        /// 单击
        /// </summary>
        /// <param name="mouseButton">鼠标按键</param>
        public static void MouseClick(MouseButton mouseButton)
        {
            Mouse.Click((FlaUI.Core.Input.MouseButton)mouseButton);
        }

        /// <summary>
        /// 双击
        /// </summary>
        /// <param name="mouseButton">鼠标按键</param>
        public static void MouseDoubleClick(MouseButton mouseButton)
        {
            Mouse.DoubleClick((FlaUI.Core.Input.MouseButton)mouseButton);
        }

        /// <summary>
        /// 左键单击
        /// </summary>
        public static void MouseLeftClick()
        {
            Mouse.LeftClick();
        }

        /// <summary>
        /// 右键单击
        /// </summary>
        public static void MouseRightClick()
        {
            Mouse.RightClick();
        }

        /// <summary>
        /// 左键双击
        /// </summary>
        public static void MouseLeftDoubleClick()
        {
            Mouse.LeftDoubleClick();
        }

        /// <summary>
        /// 右键双击
        /// </summary>
        public static void MouseRightDoubleClick()
        {
            Mouse.RightDoubleClick();
        }

        /// <summary>
        /// 保留原始点击方式
        /// </summary>
        /// <param name="clickType">点击类型</param>
        /// <param name="mouseButton">鼠标按键</param>
        public static void MouseAction(ClickType clickType, MouseButton mouseButton)
        {
            switch (clickType)
            {
                case ClickType.Single:
                    MouseClick(mouseButton);
                    break;
                case ClickType.Double:
                    MouseDoubleClick(mouseButton);
                    break;
                case ClickType.Down:
                    MouseDown(mouseButton);
                    break;
                case ClickType.Up:
                    MouseUp(mouseButton);
                    break;
                default:
                    break;
            }
        }

        #endregion


        #region 控件操作
        /// <summary>
        /// 勾选复选框
        /// </summary>
        public void Check()
        {
            uiNode.Check();
        }

        /// <summary>
        /// 反选复选框
        /// </summary>
        public void UnCheck()
        {
            uiNode.UnCheck();
        }

        /// <summary>
        /// 变换复选框勾选状态
        /// </summary>
        public void Toggle()
        {
            uiNode.Toggle();
        }

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <returns>获取到的文本字符串</returns>
        public string GetText()
        {
            return uiNode.GetText();
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        public void SetText(string text)
        {
            uiNode.SetText(text);
        }

        /// <summary>
        /// 控件是否为密码框
        /// </summary>
        /// <returns>是否为密码框</returns>
        public bool IsPassword()
        {
            return uiNode.IsPassword();
        }

        /// <summary>
        /// 清除所有选择项
        /// </summary>
        public void ClearSelection()
        {
            uiNode.ClearSelection();
        }

        /// <summary>
        /// 选择条目
        /// </summary>
        /// <param name="item">条目名称</param>
        public void SelectItem(string item)
        {
            uiNode.SelectItem(item);
        }

        /// <summary>
        /// 多选条目
        /// </summary>
        /// <param name="items">条目名称数组</param>
        public void SelectMultiItems(string[] items)
        {
            uiNode.SelectMultiItems(items);
        }

        /// <summary>
        /// 清除文本
        /// </summary>
        public void ClearText()
        {
            uiNode.ClearText();
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="attributeName">属性名称</param>
        /// <returns>获取到的属性值</returns>
        public object GetAttributeValue(string attributeName)
        {
            return uiNode.GetAttributeValue(attributeName);
        }

        public void SetWebAttribute(string attrName, string attrValue)
        {
            if (uiNode is IeNode ieNode)
            {
                ieNode.SetAttribute(attrName, attrValue);
            }

            if (uiNode is HtmlUiNode node)
            {
                node.SetAttribute(attrName, attrValue);
            }
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <returns>可用或不可用</returns>
        public bool IsEnable()
        {
            return uiNode.IsEnable();
        }

        /// <summary>
        /// 是否可见
        /// </summary>
        /// <returns>可见或不可见</returns>
        public bool IsVisible()
        {
            return uiNode.IsVisible();
        }

        /// <summary>
        /// 是否为表格
        /// </summary>
        /// <returns>是或不是表格</returns>
        public bool IsTable()
        {
            return uiNode.IsTable();
        }

        #endregion

        public object ExecuteScript(string script, params object[] args)
        {
            if (uiNode is IJavaScriptExecutor jsExecutor)
            {
                return jsExecutor.ExecuteScript(script, args);
            }

            return "";
        }
    }
}

