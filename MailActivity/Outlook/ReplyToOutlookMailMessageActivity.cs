using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Threading;
using Plugins.Shared.Library.Extensions;
using System.IO;
using Plugins.Shared.Library.Editors;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using MouseActivity;

namespace MailActivity.Outlook
{
    [Designer(typeof(ReplyToOutlookMailMessageDesigner))]
    public sealed class ReplyToOutlookMailMessageActivity : AsyncTaskCodeActivity
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
        [RequiredArgument]
        [DisplayName("邮件对象")]
        [Description("邮件对象。必须将文本放入引号中。")]
        public InArgument<MailMessage> MailMessage { get; set; }

        [Category("输入")]
        [DisplayName("回复所有人")]
        [Description("不勾选的话只回复给发送者，勾选的话不仅给发送者，还会给所有抄送者回复邮件。")]
        public bool ReplyAll { get; set; }

        [Category("输入")]
        [DisplayName("邮件正文")]
        [Description("邮件正文。必须将文本放入引号中。")]
        public InArgument<string> Body { get; set; }

        [Category("输入")]
        [DisplayName("附件")]
        public List<InArgument<string>> Files { get; set; } = new List<InArgument<string>>();

        [Category("输入")]
        [DisplayName("附件列表")]
        [DefaultValue(null)]
        public InArgument<IEnumerable<string>> AttachmentsCollection { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Mail/reply-outlook-mail.png";
            }
        }

        #endregion


        private const int DefaultTimeoutMS = 30000;

        public ReplyToOutlookMailMessageActivity()
        {
            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(ReplyToOutlookMailMessageActivity), "Files", new EditorAttribute(typeof(ArgumentCollectionEditor), typeof(DialogPropertyValueEditor)));

            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            int num = 1;
            foreach (InArgument<string> file in Files)
            {
                RuntimeArgument argument = new RuntimeArgument("attachmentArg" + ++num, typeof(string), ArgumentDirection.In);
                metadata.Bind(file, argument);
                metadata.AddArgument(argument);
            }
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            int num = Timeout.Get(context);
            string body = Body.Get(context);
            MailMessage mailMessage = MailMessage.Get(context);
            if (mailMessage == null)
            {
                Thread.Sleep(delayAfter);
                throw new ArgumentNullException("MailMessage对象不能为空！");
            }
            MailMessage message = mailMessage;
            List<string> attachments = new List<string>();
            foreach (InArgument<string> file in Files)
            {
                AddAttachments(attachments, file.Get(context));
            }
            foreach (string item in AttachmentsCollection.Get(context).EmptyIfNull())
            {
                AddAttachments(attachments, item);
            }
            num = ((num <= 0) ? DefaultTimeoutMS : num);
            using (CancellationTokenSource timeoutCts = new CancellationTokenSource(num))
            {
                using (CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken))
                {
                    try
                    {
                        await OutlookAPI.ReplyToAsync(message, body, attachments, ReplyAll, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        if (timeoutCts.IsCancellationRequested)
                        {
                            Thread.Sleep(delayAfter);
                            throw new TimeoutException();
                        }

                        Thread.Sleep(delayAfter);
                        throw;
                    }
                }
            }

            Thread.Sleep(delayAfter);
            return delegate
            {
            };
        }

        private static void AddAttachments(List<string> attachments, string attPath)
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
    }
}
