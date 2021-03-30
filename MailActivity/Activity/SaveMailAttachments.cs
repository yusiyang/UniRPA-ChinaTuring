using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using System.Diagnostics;
using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit;
using System.Collections;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.IO;
using MimeKit.Text;
using MailKit.Net.Pop3;
using System.Net.Mail;
using MailKit.Net.Imap;
using MailKit;
using MailKit.Search;
using System.Linq;
using System.Reflection;
using System.Globalization;
using MouseActivity;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace MailActivity
{
    [Designer(typeof(SaveMailAttachmentsDesigner))]
    public sealed class SaveMailAttachments : CodeActivity
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
        [RequiredArgument]
        [DisplayName("路径")]
        [Description("要保存的MimeMessage附件的全路径。必须将文本放入引号中。")]
        public InArgument<string> PathName { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("邮件消息")]
        [Description("要保存的MimeMessage附件。")]
        public InArgument<MimeMessage> MimeMessageAttachs { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("附件列表")]
        [Description("检索到的附件。")]
        public OutArgument<string[]> AttachFiles { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Mail/SaveMailAttachments.png";
            }
        }

        #endregion


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string pathName = PathName.Get(context);
            MimeMessage mailMessage = MimeMessageAttachs.Get(context);
            try
            {
                if (!Directory.Exists(pathName))
                    Directory.CreateDirectory(pathName);
                List<string> list = new List<string>();
                foreach (MimePart attachment in mailMessage.Attachments)
                {
                    using (var cancel = new System.Threading.CancellationTokenSource())
                    {
                        string fileName = attachment.FileName;
                        string filePath = pathName + "\\" + fileName;
                        using (var stream = File.Create(filePath))
                        {
                            attachment.Content.DecodeTo(stream, cancel.Token);
                            list.Add(filePath);
                        }
                    }
                }
                AttachFiles.Set(context, list.ToArray());
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
