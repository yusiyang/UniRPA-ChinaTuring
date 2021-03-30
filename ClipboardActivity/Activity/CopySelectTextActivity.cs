using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace ClipboardActivity
{
    [Designer(typeof(CopySelectTextDesigner))]
    public sealed class CopySelectTextActivity : CodeActivity
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


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("结果")]
        [Description("从剪贴板中检索到的数据。")]
        public OutArgument<string> Result { get; set; }

        private InArgument<Int32> _Timeout = 3000;
        [Category("选项")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为 30000 毫秒（30 秒）。")]
        public InArgument<Int32> Timeout
        {
            get
            {
                return _Timeout;
            }
            set
            {
                if (_Timeout != value)
                    _Timeout = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Clipboard/copy-select-text.png";
            }
        }

        #endregion


        CountdownEvent latch;
        private void refreshData(CountdownEvent latch)
        {
            latch.Signal();
        }

        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        public const int KEYEVENTF_KEYUP = 2;

        public void KeyDown_Copy()
        {
            keybd_event(Keys.ControlKey, 0, 0, 0);
            keybd_event(Keys.C, 0, 0, 0);
            Thread.Sleep(500);
            keybd_event(Keys.ControlKey, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(Keys.C, 0, KEYEVENTF_KEYUP, 0);
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                Int32 _timeout = Timeout.Get(context);
                string data = "";
                Thread.Sleep(_timeout);
                latch = new CountdownEvent(1);
                Thread td = new Thread(() =>
                {
                    //System.Windows.Forms.IDataObject dataBefore = System.Windows.Forms.Clipboard.GetDataObject();
                    //SendKeys.SendWait("^{C}");
                    KeyDown_Copy();
                    System.Windows.IDataObject dataCurrent = System.Windows.Clipboard.GetDataObject();
                    if (dataCurrent.GetDataPresent(System.Windows.DataFormats.Text))
                    {
                        data = (string)dataCurrent.GetData(System.Windows.DataFormats.Text);
                    }
                    //System.Windows.Clipboard.Clear();
                    //System.Windows.Clipboard.SetDataObject(dataBefore);
                    refreshData(latch);
                });
                td.TrySetApartmentState(ApartmentState.STA);
                td.IsBackground = true;
                td.Start();
                latch.Wait();
                Result.Set(context, data);
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
