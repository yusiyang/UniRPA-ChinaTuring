using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Plugins.Shared.Library;
using MouseActivity;

namespace MailActivity.Outlook
{
    [Designer(typeof(SaveOutlookAttachementsDesigner))]
    public sealed class SaveOutlookAttachementsActivity : AsyncCodeActivity
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
        [DisplayName("保存路径")]
        [Description("附件的保存路径。必须将文本放入引号中。")]
        public InArgument<string> FolderPath { get; set; }

        [Category("输入")]
        [DisplayName("邮件对象")]
        [Description("指定邮件对象。")]
        [RequiredArgument]
        public InArgument<MailMessage> Message { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("附件列表")]
        [Description("输出附件列表。")]
        public OutArgument<IEnumerable<string>> Attachments { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("过滤器")]
        [Description("正则表达式过滤器，只有符合过滤条件的附件名才会被保存。必须将文本放入引号中。")]
        [DefaultValue(null)]
        public InArgument<string> Filter { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Mail/save-outlook-mail.png";
            }
        }

        #endregion


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            CancellationTokenSource cts = new CancellationTokenSource();
            context.UserState = cts;
            Task<IEnumerable<string>> task = SaveAttachments(context, cts.Token);
            TaskCompletionSource<IEnumerable<string>> tcs = new TaskCompletionSource<IEnumerable<string>>(state);
            task.ContinueWith(delegate (Task<IEnumerable<string>> t)
            {
                if (cts.Token.IsCancellationRequested || t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception.InnerExceptions);
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

        Task<IEnumerable<string>> SaveAttachments(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            string folderPath = FolderPath.Get(context);
            MailMessage mail = Message.Get(context);
            string filter = Filter.Get(context);
            return Task.Factory.StartNew((Func<IEnumerable<string>>)delegate
            {
                List<string> list = new List<string>();
                List<Exception> list2 = new List<Exception>();
                if (mail.Attachments == null || mail.Attachments.Count == 0)
                {
                    return list;
                }
                if (string.IsNullOrEmpty(folderPath))
                {
                    folderPath = Directory.GetCurrentDirectory();
                }
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                foreach (Attachment attachment in mail.Attachments)
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return list;
                        }
                        string text = Path.GetFileName(attachment.Name);
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            text = Path.Combine(folderPath, text);
                        }
                        if (string.IsNullOrEmpty(filter) || Regex.IsMatch(text, filter))
                        {
                            try
                            {
                                FileStream fileStream = File.Open(text, FileMode.Create, FileAccess.Write);
                                attachment.ContentStream.Position = 0L;
                                attachment.ContentStream.CopyTo(fileStream);
                                fileStream.Close();
                                list.Add(text);
                            }
                            finally
                            {
                                attachment.ContentStream.Position = 0L;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedObject.Instance.Output(SharedObject.OutputType.Error, "有一个异常产生", ex.Message);
                        list2.Add(ex);
                    }
                }
                if (list2.Count > 0)
                {
                    throw new AggregateException(list2);
                }
                return list;
            });
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            Task<IEnumerable<string>> task = (Task<IEnumerable<string>>)result;
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
                    Attachments.Set(context, task.Result);
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
    }
}
