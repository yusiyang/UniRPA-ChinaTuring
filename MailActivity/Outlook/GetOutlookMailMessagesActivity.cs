using System.Collections.Generic;
using System.Activities;
using System.ComponentModel;
using System.Net.Mail;
using Microsoft.Office.Interop.Outlook;
using System;
using System.Threading.Tasks;
using System.Threading;
using MouseActivity;

namespace MailActivity.Outlook
{
    [Designer(typeof(GetOutlookMailMessagesDesigner))]
    public sealed class GetOutlookMailMessagesActivity : AsyncCodeActivity
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
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> Timeout { get; set; }

        [Category("输入")]
        [DisplayName("邮件目录")]
        [Description("从哪个邮件目录去获取邮件。必须将文本放入引号中。")]
        public InArgument<string> MailFolder { get; set; }

        [Category("输入")]
        [DisplayName("账户")]
        [Description("Outlook账户名。必须将文本放入引号中。")]
        public InArgument<string> Account { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("输出邮件列表")]
        public OutArgument<List<MailMessage>> Messages { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("获取邮件个数")]
        public InArgument<int> Top { get; set; }

        [Category("选项")]
        [DisplayName("过滤器")]
        [Description("只有符合过滤条件的邮件才会被获取。必须将文本放入引号中。")]
        public InArgument<string> Filter { get; set; }

        [Category("选项")]
        [DisplayName("获取附件")]
        public bool GetAttachements { get; set; }

        [Category("选项")]
        [DisplayName("只获取未读邮件")]
        [Description("是否只获取未读邮件，默认打勾。")]
        public bool OnlyUnreadMessages { get; set; }

        [Category("选项")]
        [DisplayName("标记为已读")]
        [Description("是否将接收到的邮件自动标记成已读。")]
        public bool MarkAsRead { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Mail/outlook.png";
            }
        }

        #endregion


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        public GetOutlookMailMessagesActivity()
        {
            MailFolder = "收件箱";
            OnlyUnreadMessages = true;
            Top = new InArgument<int>(30);
        }


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            CancellationTokenSource cancellationTokenSource2 = (CancellationTokenSource)(context.UserState = new CancellationTokenSource());
            int timeout = Timeout.Get(context);
            Task<List<MailMessage>> messages = GetMessages(context, cancellationTokenSource2.Token);
            TaskCompletionSource<List<MailMessage>> taskCompletionSource = new TaskCompletionSource<List<MailMessage>>(state);
            TaskHandler(callback, messages, taskCompletionSource, cancellationTokenSource2.Token, timeout);

            Thread.Sleep(delayAfter);
            return taskCompletionSource.Task;
        }

        public static void TaskHandler(AsyncCallback callback, Task<List<MailMessage>> task, TaskCompletionSource<List<MailMessage>> tcs, CancellationToken token, int timeout)
        {
            timeout = ((timeout <= 0) ? 30000 : timeout);
            Task.Run(delegate
            {
                try
                {
                    if (!task.Wait(timeout, token))
                    {
                        tcs.TrySetException(new TimeoutException());
                    }
                    else if (token.IsCancellationRequested || task.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else if (task.IsFaulted)
                    {
                        tcs.TrySetException(task.Exception.InnerExceptions);
                    }
                    else
                    {
                        tcs.TrySetResult(task.Result);
                    }
                }
                catch (System.Exception ex)
                {
                    tcs.TrySetException(ex.InnerException);
                }
                callback?.Invoke(tcs.Task);
            });
        }


        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            Task<List<MailMessage>> task = (Task<List<MailMessage>>)result;
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
                else
                {
                    Messages.Set(context, task.Result);
                }
            }
            catch (OperationCanceledException)
            {
                context.MarkCanceled();
            }
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            ((CancellationTokenSource)context.UserState).Cancel();
        }

        public Task<List<MailMessage>> GetMessages(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            string account = Account.Get(context);
            string filter = Filter.Get(context);
            string folderpath = MailFolder.Get(context);
            int top = Top.Get(context);

            return Task.Factory.StartNew(() => OutlookAPI.GetMessages(account, folderpath, top, filter, OnlyUnreadMessages, MarkAsRead, true, cancellationToken));
        }




    }
}
