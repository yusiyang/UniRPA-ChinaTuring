using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Plugins.Shared.Library.Extensions;
using System.IO;
using Plugins.Shared.Library;
using System.Activities.Presentation.Metadata;
using Plugins.Shared.Library.Editors;
using System.Activities.Presentation.PropertyEditing;
using MouseActivity;

namespace MailActivity.Outlook
{
    [Designer(typeof(SendOutlookMailDesigner))]
    public sealed class SendOutlookMailActivity : AsyncCodeActivity
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
        [DisplayName("邮件对象")]
        [OverloadGroup("Forward")]
        [Description("要转发的消息，该字段仅支持 MailMessage 对象。")]
        public InArgument<MailMessage> MailMessage { get; set; }

        [Category("输入")]
        [DisplayName("账户")]
        [Description("Outlook账户名。必须将文本放入引号中。")]
        public InArgument<string> Account { get; set; }

        [Category("输入")]
        [DisplayName("显示名")]
        [Description("预期的显示名称，邮件系统必须支持该功能才能使用。必须将文本放入引号中。")]
        [DefaultValue(null)]
        public InArgument<string> SentOnBehalfOfName { get; set; }


        #endregion


        #region 属性分类：接收人

        [Category("接收人")]
        [RequiredArgument]
        [DisplayName("收件人")]
        [Description("电子邮件的主要收件人。必须将文本放入引号中。")]
        public InArgument<string> To { get; set; }

        [Category("接收人")]
        [DisplayName("抄送")]
        [Description("电子邮件的次要收件人。必须将文本放入引号中。")]
        public InArgument<string> Cc { get; set; }

        [Category("接收人")]
        [DisplayName("密送")]
        [Description("电子邮件的隐藏收件人。必须将文本放入引号中。")]
        public InArgument<string> Bcc { get; set; }

        #endregion


        #region 属性分类：邮件

        [Category("邮件")]
        [DisplayName("主题")]
        [Description("电子邮件的主题。必须将文本放入引号中。")]
        public InArgument<string> Subject { get; set; }

        [Category("邮件")]
        [DisplayName("正文")]
        [Description("电子邮件的正文。必须将文本放入引号中。")]
        public InArgument<string> Body { get; set; }

        #endregion


        #region 属性分类：附件

        [Category("附件")]
        [DisplayName("附件文件")]
        public List<InArgument<string>> Files { get; set; } = new List<InArgument<string>>();

        [Category("附件")]
        [DisplayName("附件")]
        [DefaultValue(null)]
        public List<string> Attachments { get; set; }

        [Category("附件")]
        [DisplayName("附件列表")]
        [DefaultValue(null)]
        public InArgument<IEnumerable<string>> AttachmentsCollection { get; set; }

        #endregion


        #region 属性分类： 选项

        [Category("选项")]
        [DisplayName("HTML 格式")]
        [Description("指定邮件正文是否以 HTML 格式编写。")]
        public bool IsBodyHtml { get; set; }

        [Category("选项")]
        [DisplayName("是否保存成草稿")]
        public bool IsDraft { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Mail/send-outlook-mail.png";
            }
        }

        #endregion


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            if (Attachments != null)
            {
                if (Attachments.Count == 0)
                {
                    Attachments = null;
                }
                else
                {
                    if (Files == null)
                    {
                        Files = new List<InArgument<string>>();
                    }
                    foreach (string attachment in Attachments)
                    {
                        Files.Add(new InArgument<string>(attachment));
                    }
                    Attachments = null;
                }
            }

            int num = 1;
            foreach (InArgument<string> file in Files)
            {
                RuntimeArgument argument = new RuntimeArgument("attachmentArg" + ++num, typeof(string), ArgumentDirection.In);
                metadata.Bind(file, argument);
                metadata.AddArgument(argument);
            }
        }

        public SendOutlookMailActivity()
        {
            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(SendOutlookMailActivity), "Files", new EditorAttribute(typeof(ArgumentCollectionEditor), typeof(DialogPropertyValueEditor)));

            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            CancellationTokenSource cancellationTokenSource2 = (CancellationTokenSource)(context.UserState = new CancellationTokenSource());
            Receiver reciever = new Receiver(To.Get(context), Cc.Get(context), Bcc.Get(context));
            bool isNewMessage = false;
            MailMessage mailMessage = MailMessage.Get(context);
            string body = Body.Get(context);
            if (mailMessage == null)
            {
                mailMessage = new MailMessage();
                mailMessage.Subject = Subject.Get(context);
                mailMessage.Body = body;
                isNewMessage = true;
            }
            mailMessage.IsBodyHtml = IsBodyHtml;
            int timeout = Timeout.Get(context);
            Task task = SendMailTask(context, reciever, mailMessage, cancellationTokenSource2.Token, isNewMessage, body);
            TaskCompletionSource<object> taskCompletionSource = new TaskCompletionSource<object>(state);
            TaskHandler(callback, task, taskCompletionSource, cancellationTokenSource2.Token, mailMessage, isNewMessage, timeout);

            Thread.Sleep(delayAfter);
            return taskCompletionSource.Task;
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            Task task = (Task)result;
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

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            ((CancellationTokenSource)context.UserState).Cancel();
        }



        public Task SendMailTask(AsyncCodeActivityContext context, Receiver reciever, MailMessage mailMessage, CancellationToken cancellationToken, bool isNewMessage, string body)
        {
            List<string> attachments = Attachments ?? new List<string>();
            foreach (InArgument<string> file in Files)
            {
                AddAttachments(attachments, file.Get(context));
            }
            foreach (string item in AttachmentsCollection.Get(context).EmptyIfNull())
            {
                AddAttachments(attachments, item);
            }
            string to = To.Get(context);
            string cc = Cc.Get(context);
            string bcc = Bcc.Get(context);
            string sentOnBehalfOfName = SentOnBehalfOfName.Get(context);
            string account = Account.Get(context);
            return Task.Factory.StartNew(delegate
            {
                OutlookAPI.SendMail(mailMessage, account, sentOnBehalfOfName, to, cc, bcc, attachments, IsDraft, IsBodyHtml);
            });
        }

        public static void TaskHandler(AsyncCallback callback, Task task, TaskCompletionSource<object> tcs, CancellationToken token, MailMessage msg, bool isNewMessage, int timeout)
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
                    else
                    {
                        if (isNewMessage)
                        {
                            msg.Dispose();
                        }
                        if (token.IsCancellationRequested || task.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else if (task.IsFaulted)
                        {
                            tcs.TrySetException(task.Exception.InnerExceptions);
                        }
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex.InnerException);
                }
                callback?.Invoke(tcs.Task);
            });
        }

        private static void AddAttachments(List<string> attachments, string attPath)
        {
            try
            {
                string text = null;
                if (!Path.IsPathRooted(attPath))
                {
                    text = Path.Combine(Environment.CurrentDirectory, attPath);
                }
                if (File.Exists(text))
                {
                    attachments.Add(text);
                }
                else
                {
                    attachments.Add(attPath);
                }
            }
            catch (Exception ex)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "有一个异常产生", ex.Message);
            }
        }




    }







}
