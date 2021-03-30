using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Threading;
using MouseActivity;

namespace MailActivity.Outlook
{
    [Designer(typeof(DeleteOutlookMailItemDesigner))]
    public sealed class DeleteOutlookMailItemActivity : AsyncCodeActivity
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

        //[Category("常见")]
        //[DisplayName("出错时继续")]
        //[Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        //public bool ContinueOnError { get; set; }

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
        [RequiredArgument]
        [DisplayName("邮件对象")]
        [Description("指定邮件对象。")]
        public InArgument<MailMessage> MailMessage { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Mail/delete-mail.png";
            }
        }

        #endregion


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(state);
            CancellationTokenSource cts = new CancellationTokenSource();
            context.UserState = cts;
            MoveMessageAction(context, cts.Token).ContinueWith(delegate (Task<object> t)
            {
                if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception.InnerExceptions);
                }
                else if (t.IsCanceled || cts.IsCancellationRequested)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
                callback?.Invoke(tcs.Task);
            });

            Thread.Sleep(delayAfter);
            return tcs.Task;
        }

        private Task<object> MoveMessageAction(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            MailMessage message = MailMessage.Get(context);

            return Task.Factory.StartNew(((Func<object>)delegate
            {
                OutlookAPI.DeleteMessage(message);
                return null;
            }));
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            Task<object> task = (Task<object>)result;
            try
            {
                if (task.IsFaulted)
                {
                    throw task.Exception.InnerException;
                }
                if (task.IsCanceled || context.IsCancellationRequested)
                {
                    context.MarkCanceled();
                }
            }
            catch (OperationCanceledException)
            {
                context.MarkCanceled();
            }
        }
    }
}
