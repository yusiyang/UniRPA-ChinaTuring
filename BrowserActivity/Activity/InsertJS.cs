using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using System.Threading;
using System.Windows;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections;
using Plugins.Shared.Library.UiAutomation;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Windows.Forms;
using MouseActivity;
using MSHTML;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.Exceptions;

[assembly: InternalsVisibleTo("BrowserActivity.Microsoft.CSharp")]

namespace BrowserActivity
{
    [Designer(typeof(InsertJSDesigner))]
    public sealed class InsertJS : AsyncCodeActivity
    {
        #region 属性分类：常见

        public string _displayName;
        [Category("常见")]
        [DisplayName("显示名称")]
        public new string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    _displayName = base.DisplayName;
                }
                else
                {
                    base.DisplayName = _displayName;
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }
        [Browsable(false)]
        public string _DisplayName
        {
            get
            {
                return this.DisplayName;
            }
        }

        [Category("常见")]
        [DisplayName("出错时继续")]
        [Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        public bool ContinueOnError { get; set; }

        [Category("常见")]
        [DisplayName("在此之前延迟")]
        [Description("活动开始执行任何操作之前的延迟时间（以毫秒为单位）。默认时间量为200毫秒。")]
        public InArgument<int> DelayBefore { get; set; }

        [Category("常见")]
        [DisplayName("在此之后延迟")]
        [Description("执行活动之后的延迟时间（以毫秒为单位）。默认时间量为300毫秒。")]
        public InArgument<int> DelayAfter { get; set; }

        #endregion


        #region 属性分类：目标

        [Category("目标")]
        [DisplayName("元素")]
        [Description("使用另一个活动返回的用户界面元素变量， 该属性不能与“选取器”属性一起使用。该字段仅支持用户界面元素变量。")]
        public InArgument<UiElement> Element { get; set; }

        [Category("目标")]
        [OverloadGroup("G1")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> Timeout { get; set; }

        [Category("目标")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        #endregion


        #region 属性分类：输入

        InArgument<string> _JSCode;
        [Category("输入")]
        [RequiredArgument]
        [DisplayName("脚本代码")]
        [Description("要运行的JavaScript代码。可在这里填写脚本代码或脚本文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> JSCode
        {
            get
            {
                return _JSCode;
            }
            set
            {
                _JSCode = value;
            }
        }

        InArgument<string> _JSPara;
        [Category("输入")]
        [DisplayName("脚本数据")]
        [Description("输入JavaScript代码数据。必须将文本放入引号中。")]
        public InArgument<string> JSPara
        {
            get
            {
                return _JSPara;
            }
            set
            {
                _JSPara = value;
            }
        }

        #endregion


        #region 属性分类：输出

        OutArgument<string> _JSOut;
        [Category("输出")]
        [DisplayName("脚本输出")]
        [Description("JavaScript 代码返回的字符串结果。必须将文本放入引号中。")]
        public OutArgument<string> JSOut
        {
            get
            {
                return _JSOut;
            }
            set
            {
                _JSOut = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        private System.Windows.Visibility visi = System.Windows.Visibility.Hidden;
        [Browsable(false)]
        public System.Windows.Visibility visibility
        {
            get
            {
                return visi;
            }
            set
            {
                visi = value;
            }
        }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Browser/InsertJS.png";
            }
        }

        [Browsable(false)]
        public string DefaultName { get { return "注入JS脚本"; } }

        #endregion


        CountdownEvent latch;
        private void refreshData(CountdownEvent latch)
        {
            latch.Signal();
        }

        static object InvokeScript(object callee, string method, params object[] args)
        {
            return callee.GetType().InvokeMember(method, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, callee, args);
        }

        public static T GetValueOrDefault<T>(ActivityContext context, InArgument<T> source, T defaultValue)
        {
            T result = defaultValue;
            if (source != null && source.Expression != null)
            {
                result = source.Get(context);
            }
            return result;
        }

        
        bool isErrorFlag = false;

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
           
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            //System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            var selStr = Selector.Get(context);
            string jsCode = JSCode.Get(context);
            string jsPara = JSPara.Get(context);
            if (File.Exists(jsCode))
            {
                string jsContent = File.ReadAllText(jsCode);
                jsCode = jsContent;
            }
            m_Delegate = new runDelegate(Run);
            //流程运行 只允许一个输入参数
            //chrome运行JS与IE不同，首先命名函数，而后执行。IE命名后可直接执行
            //返回值必须调用return func(),参数传递必须使用func(arguments[i])

            var returnStr = "";
            try
            {
                if (selStr == null || selStr == "")
                {
                    PropertyDescriptor property = context.DataContext.GetProperties()[OpenBrowser.OpenBrowsersPropertyTag];
                    if (property == null)
                        property = context.DataContext.GetProperties()[AttachBrowser.OpenBrowsersPropertyTag];
                    if (property == null)
                    {
                        m_Delegate = new runDelegate(Run);
                        return m_Delegate.BeginInvoke(callback, state);
                    }
                    IBrowser browser = property.GetValue(context.DataContext) as IBrowser;
                    //流程运Chrome和火狐浏览器
                    returnStr=browser?.InjectAndRunJs(jsCode, jsCode, null);

                }
                //桌面选取运行 允许三个参数
                else
                {
                    latch = new CountdownEvent(1);
                    Thread td = new Thread(() =>
                    {

                        UiElement element = GetValueOrDefault(context, this.Element, null);
                        int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);
                        if (element == null && selStr != null)
                        {
                            element = UiElement.FromSelector(selStr,timeout);
                        }
                        if (element != null)
                        {
                            //int windowHandle = (int)element.WindowHandle;
                            //if ((int)element.WindowHandle == 0)
                            //{
                            //    windowHandle = (int)element.Parent.WindowHandle;
                            //}
                            //MSHTML.IHTMLDocument2 currDoc = null;
                            //SHDocVw.InternetExplorer ieBrowser = GetIEFromHWndClass.GetIEFromHWnd(windowHandle, out currDoc);
                            //MSHTML.IHTMLElement currEle = null;
                            //if (currDoc != null)
                            //    currEle = GetIEFromHWndClass.GetEleFromDoc(element.GetClickablePoint(), windowHandle, currDoc);
                            //runIEJS(ieBrowser, currDoc, currEle, jsCode, jsPara, context, 1);
                            returnStr=element.ExecuteScript(jsCode, jsPara)?.ToString();
                        }
                        else
                        {
                            SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", "查找不到元素");
                            if (!ContinueOnError)
                            {
                                throw new NotImplementedException("查找不到元素");
                            }
                        }
                        refreshData(latch);
                    });
                    td.TrySetApartmentState(ApartmentState.STA);
                    td.IsBackground = true;
                    td.Start();
                    latch.Wait();
                }
            }
            catch(Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (ContinueOnError)
                {
                    return m_Delegate.BeginInvoke(callback, state);
                }
                else
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            if (!string.IsNullOrEmpty(returnStr))
            {
                JSOut.Set(context, returnStr);
            }
            if (isErrorFlag==true && !ContinueOnError)
            {
                throw new ActivityRuntimeException(this.DisplayName, new NotImplementedException("执行JS过程出错！"));
            }
            else
            {
                return m_Delegate.BeginInvoke(callback, state);
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
        }

        private void runIEJS(SHDocVw.InternetExplorer ieBrowser,
            MSHTML.IHTMLDocument2 currDoc,
            MSHTML.IHTMLElement currEle,
            string jsCode,
            string jsPara,
            AsyncCodeActivityContext context,
            int jsFlag
        )
        {
            try
            {
                /*WEB页面中添加JS*/
                MSHTML.IHTMLElement JSele = ((HTMLDocument)ieBrowser.Document).createElement("script");
                JSele.setAttribute("type", "text/javascript");
                JSele.setAttribute("text", jsCode);
                ((IHTMLDOMNode)((HTMLDocument)ieBrowser.Document).body).appendChild((IHTMLDOMNode)JSele);
                
                //设置属性
                //currDoc.parentWindow.execScript("document.body.setAttribute('PSResult','PSResult')", "JavaScript");
                //移除属性
                //currDoc.parentWindow.execScript("document.body.removeAttribute('PSResult')", "JavaScript");

                //若为Function类型的JS函数体
                string FuncName = "";
                string ArgStr = "";
                bool flag = false;
                jsCode = jsCode.Trim();
                if (jsCode.Contains("function") || jsCode.Contains("Function"))
                {
                    foreach (char myChar in jsCode)
                    {
                        if (myChar == ' ')
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            if (myChar == '(')
                                break;
                            FuncName += myChar;
                        }
                    }

                    flag = false;
                    foreach (char myChar in jsCode)
                    {
                        if (myChar == '(')
                        {
                            flag = true;
                            continue;
                        }
                        if (flag)
                        {
                            if (myChar == ')')
                                break;
                            ArgStr += myChar;
                        }
                    }
                }
                //execScript方式无法获取返回值
                //object c = currDoc.parentWindow.execScript("function aaa(){return \"aaa\"};aaa();");
                FuncName = FuncName.Trim();
                string[] ArgArray = ArgStr.Split(',');
                object htmlWindowObject = currDoc.parentWindow;

                object returnValue = null;
                if (ArgArray.Length > 2 && jsFlag == 1)
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, "JS函数参数个数超出范围");
                    isErrorFlag = true;
                }
                else if (ArgArray.Length > 1 && jsFlag == 0)
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, "JS函数参数个数超出范围");
                    isErrorFlag = true;
                }
                else
                {
                    //两个参数 第一个为element 第二个为输入参数值
                    if (ArgArray.Length == 2 && jsFlag == 1)
                        returnValue = InvokeScript(htmlWindowObject, FuncName, new object[] { currEle, jsPara });
                    //一个参数 默认为element
                    if (ArgArray.Length == 1 && jsFlag == 1)
                        returnValue = InvokeScript(htmlWindowObject, FuncName, new object[] { currEle });
                    //无参
                    if (ArgArray.Length == 0 && jsFlag == 1)
                        returnValue = InvokeScript(htmlWindowObject, FuncName, new object[] { });
                    //流程化IE 无参
                    if (ArgArray.Length == 0 && jsFlag == 0)
                        returnValue = InvokeScript(htmlWindowObject, FuncName, new object[] { });
                    //流程化IE 一个参数
                    if (ArgArray.Length == 1 && jsFlag == 0)
                        returnValue = InvokeScript(htmlWindowObject, FuncName, new object[] { jsPara });
                }

                string returnStr = "";
                if (returnValue != null)
                {
                    returnStr = returnValue.ToString();
                    JSOut.Set(context, returnStr);
                }

                ArrayList list = new ArrayList();
                var allShellWindows = new SHDocVw.ShellWindows();
                foreach (SHDocVw.InternetExplorer browser in allShellWindows)
                {
                    list.Add(browser);
                }
                CommonVariable.BrowsersList = list;

                //确保页面是否已加载完成
                while (ieBrowser.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
                {
                    Thread.Sleep(500);
                }
                /*使用此种方式获取返回值会返回Microsoft.CSharp.RuntimeBinder.RuntimeBinderException*/
                //int a = ieBrowser.Document.Script.value;
                //int a = currDoc.Script.value;
            }
            catch(Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "JS子线程运行失败");
                isErrorFlag = true;
            }
        }


        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }
        [Browsable(false)]
        public string ClassName { get { return "InsertJS"; } }
    }
}
