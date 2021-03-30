using DialogActivity.TypeEditor;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.Extensions;
using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace DialogActivity.Activity
{
    [Designer(typeof(DialogActivity.Designer.MessageBoxDesigner))]
    public sealed class MessageBoxActivity : CodeActivity
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


        #region 属性分类：输入

        [Category("输入")]
        [DisplayName("按钮")]
        [Description("指定要在消息框中显示的按钮。")]
        public Int32 Buttons { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("文本")]
        [Description("要在消息框中显示的文本。必须将文本放入引号中。")]
        public InArgument<string> Text { get; set; }

        [Category("输入")]
        [DisplayName("标题")]
        [Description("消息框对话框的标题。必须将文本放入引号中。")]
        public InArgument<string> Captions { get; set; }

        #endregion


        #region 属性分类：选项

        //[Category("选项")]
        //[DisplayName("排名最前")]
        //[Description("如果选中，则始终将消息置于前台。")]
        //public bool TopMost { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("所选按钮")]
        [Description("表示消息框对话框中已按下按钮的字符串。可能是：确定、是、否或取消。")]
        public OutArgument<string> ChosenButton { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Dialog/messagebox.png";
            }
        }

        #endregion


        static MessageBoxActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(MessageBoxActivity), "Buttons", new EditorAttribute(typeof(ButtonsClickTypeEditor), typeof(PropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string _Captions = Captions.Get(context);
                string _Text = Text.Get(context);

                if (string.IsNullOrEmpty(_Captions)) _Captions = "Uni Studio";

                if (Buttons > 1) Buttons++;

                //MessageBoxOptions messageBoxOptions;
                //if (TopMost)
                //{
                //    messageBoxOptions = MessageBoxOptions.DefaultDesktopOnly;
                //}  
                //else
                //{
                //    messageBoxOptions = MessageBoxOptions.None;
                //}

                MessageBoxImage messageBoxImage;
                if ((MessageBoxButton)Buttons == MessageBoxButton.OK)
                {
                    messageBoxImage = MessageBoxImage.Information;
                }
                else
                {
                    messageBoxImage = MessageBoxImage.Question;
                }

                //var result = MessageBox.Show(_Text, _Captions, (MessageBoxButton)Buttons, messageBoxImage, MessageBoxResult.None, messageBoxOptions);
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    var result = UniMessageBox.Show(_Text, _Captions, (MessageBoxButton)Buttons, messageBoxImage, MessageBoxResult.None);
                    ChosenButton.Set(context, result.ToString());
                }));

            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }


            Thread.Sleep(delayAfter);
        }
    }
}
