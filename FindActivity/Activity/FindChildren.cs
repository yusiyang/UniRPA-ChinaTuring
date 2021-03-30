using System.Collections.Generic;
using System.Activities;
using System.ComponentModel;
using System.Windows;
using System;
using Plugins.Shared.Library.UiAutomation;
using MouseActivity;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Conditions;
using Plugins.Shared.Library;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace FindActivity
{
    [Designer(typeof(FindChildrenDesigner))]
    public sealed class FindChildren : AsyncCodeActivity
    {
        public enum ScopeOption
        {
            Children,
            Descendants,
            Subtree
        }


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
        [OverloadGroup("Element")]
        [DisplayName("元素")]
        [Description("使用另一个活动返回的用户界面元素变量， 该属性不能与“选取器”属性一起使用。该字段仅支持用户界面元素变量。")]
        public InArgument<UiElement> Element { get; set; }

        [Category("目标")]
        [OverloadGroup("G1")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> Timeout { get; set; }

        [Category("目标")]
        [OverloadGroup("Selector")]
        [DisplayName("选取器")]
        [Description("用于在执行活动时查找特定用户界面元素的“文本”属性。它实际上是XML片段，用于指定您要查找的图形用户界面元素及其一些父元素的属性。必须将文本放入引号中。")]
        public InArgument<string> Selector { get; set; }
        [Browsable(false)]
        public string SelectorOrigin { get; set; }

        #endregion


        #region 属性分类：选项

        ScopeOption _Scope = ScopeOption.Children;
        [Category("选项")]
        [DisplayName("作用域")]
        [Description("使您能够设置集合中用户界面元素的范围。可用的选项如下：子项、后代、顶级、进程、线程。")]
        public ScopeOption Scope
        {
            get
            {
                return _Scope;
            }
            set
            {
                _Scope = value;
            }
        }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("FilterText")]
        [DisplayName("筛选")]
        [Description("XML 字符串，指定集合中所有用户界面对象应符合的条件。必须将文本放入引号中。")]
        public InArgument<string> FilterText { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("子项")]
        [Description("根据筛选器和范围设置查找到的所有用户界面子项。该字段仅支持枚举值<用户界面元素>变量。")]
        public OutArgument<List<UiElement>> UiList { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Find/FindChildren.png";
            }
        }

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
        public string ClassName { get { return "FindChildren"; } }


        [Browsable(false)]
        public string DefaultName { get { return "查找UI子元素"; } }

        #endregion


        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                m_Delegate = new runDelegate(Run);
                string filterText = FilterText.Get(context);
                TreeScope treeScope;
                if (Scope == ScopeOption.Children)
                    treeScope = TreeScope.Children;
                else if (Scope == ScopeOption.Descendants)
                    treeScope = TreeScope.Descendants;
                else
                    treeScope = TreeScope.Subtree;
                UiElement element = null;
                var selStr = Selector.Get(context);
                int timeout = Common.GetValueOrDefault(context, this.Timeout, 30000);

                element = Common.GetValueOrDefault(context, this.Element, null);
                if (element == null && selStr != null)
                {
                    element = UiElement.FromSelector(selStr,timeout);
                }
                else
                {
                    PropertyDescriptor property = context.DataContext.GetProperties()[EleScope.GetEleScope];
                    element = property.GetValue(context.DataContext) as UiElement;
                }

                List<UiElement> uiList = new List<UiElement>();
                uiList = element.FindAllByFilter(treeScope, TrueCondition.Default, filterText);
                UiList.Set(context, uiList);


                return m_Delegate.BeginInvoke(callback, state);
            }
            catch(Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    return m_Delegate.BeginInvoke(callback, state);
                }
                else
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
        }
    }
}
