using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;

namespace DialogActivity.Activity
{
    [Designer(typeof(DialogActivity.Designer.InputDialogDesigner))]
    public sealed class InputDialogActivity : CodeActivity
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
        [DisplayName("是密码")]
        [Description("指定是否应将输入值视为密码。")]
        public bool IsPassword { get; set; }

        [Category("输入")]
        [DisplayName("标签")]
        [Description("表单字段的标签。必须将文本放入引号中。")]
        public InArgument<string> LabelTextBox { get; set; }

        [Category("输入")]
        [DisplayName("标题")]
        [Description("输入对话框的标题。必须将文本放入引号中。")]
        public InArgument<string> TitleTextBox { get; set; }

        [Category("输入")]
        [DisplayName("选项")]
        [Description("一系列可供选择的选项。如果设置为仅包含一个元素，则会显示一个文本框用于填写文本。如果设置为包含 2 或 3 个元素，则它们会显示为单选按钮以供选择。如果设置为包含 3 个以上的项目，则它们会显示为下拉框以供选择。此字段仅支持“字符串数组”变量。")]
        public InArgument<string[]> OptionsTextBox { get; set; }

        #endregion


        #region 属性分类；输出

        [Category("输出")]
        [DisplayName("结果")]
        [Description("用户在输入对话框中插入的值。")]
        public OutArgument<string> Result { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Dialog/inputdialog.png";
            }
        }

        #endregion


        CountdownEvent latch;
        private void refreshData(CountdownEvent latch)
        {
            latch.Signal();
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string _title = TitleTextBox.Get(context);
                string _textValue = LabelTextBox.Get(context);
                string[] _listItem = OptionsTextBox.Get(context);
                string _resultValue = null;
                latch = new CountdownEvent(1);
                Thread td = new Thread(() =>
                {
                    DialogActivity.Windows.InputDialogWindow dlg = new DialogActivity.Windows.InputDialogWindow();

                    if (_title != null)
                        dlg.Title = _title;

                    if (_textValue != null)
                        dlg.setTextContent(_textValue);

                    if(_listItem != null)
                    {
                        if (_listItem.Length <= 1)
                            dlg.CreateEditBox(IsPassword);
                        else if (_listItem.Length > 1 && _listItem.Length < 4)
                            dlg.CreateCheckBox(_listItem);
                        else
                            dlg.CreateCombobox(_listItem);
                    }
                    else
                    {
                        dlg.CreateEditBox(IsPassword);
                    }
                    dlg.ShowDialog();
                    _resultValue = dlg.getValue();
                    dlg.Close();
                    refreshData(latch);
                });
                td.TrySetApartmentState(ApartmentState.STA);
                td.IsBackground = true;
                td.Start();
                latch.Wait();
                Result.Set(context, _resultValue);
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
