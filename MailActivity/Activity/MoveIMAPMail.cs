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
using MouseActivity;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace MailActivity
{
    [Designer(typeof(MoveIMAPMailDesigner))]
    public sealed class MoveIMAPMail : CodeActivity
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


        #region 属性分类：登录

        [Category("登录")]
        [RequiredArgument]
        [DisplayName("邮件账户")]
        [Description("用于发送邮件的电子邮件帐户。必须将文本放入引号中。")]
        public InArgument<string> Email { get; set; }

        [Category("登录")]
        [RequiredArgument]
        [DisplayName("密码")]
        [Description("用于发送邮件的电子邮件帐户的密码。必须将文本放入引号中。")]
        public InArgument<string> Password { get; set; }

        #endregion


        #region 属性分类：主机

        [Category("主机")]
        [RequiredArgument]
        [DisplayName("服务器")]
        [Description("使用的电子邮件服务器主机。")]
        public InArgument<string> Server { get; set; }

        [Category("主机")]
        [DisplayName("端口")]
        [Description("电子邮件将通过的端口。")]
        public InArgument<Int32> Port { get; set; }

        [Category("主机")]
        [DisplayName("SSL")]
        [Description("指定是否应使用 SSL 发送消息。")]
        public bool EnableSSL { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("安全连接")]
        [Description("指定用于连接的 SSL 和/或 TLS 加密。")]
        public SecureSocketOptions SecureConnection { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("目标文件夹")]
        [Description("要将消息移动到的邮件文件夹。必须将文本放入引号中。")]
        public InArgument<string> MailFolderTo { get; set; }

        public InArgument<string> _MailFolderFrom = "INBOX";
        [Category("输入")]
        [RequiredArgument]
        [DisplayName("源文件夹")]
        [Description("可以找到邮件消息的文件夹。必须将文本放入引号中。")]
        public InArgument<string> MailFolderFrom
        {
            get
            {
                return _MailFolderFrom;
            }
            set
            {
                _MailFolderFrom = value;
            }
        }

        [Category("输入")]
        [DisplayName("邮件消息")]
        [Description("要移动的MailMessage对象。")]
        public InArgument<MimeMessage> MailMoveMessage { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Mail/GetMail.png";
            }
        }

        #endregion


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string username = Email.Get(context);               //发送端账号   
            string password = Password.Get(context);            //发送端密码(这个客户端重置后的密码)
            string server = Server.Get(context);                //邮件服务器
            Int32 port = Port.Get(context);                     //端口号
            string mailFolderTo = MailFolderTo.Get(context);    //目标文件夹
            string mailFolderFrom = MailFolderFrom.Get(context);//源文件夹
            MimeMessage mailMoveMessage = MailMoveMessage.Get(context);

            ImapClient client = new ImapClient();
            SearchQuery query;
            try
            {
                client.Connect(server, port, SecureConnection);
                client.Authenticate(username, password);

                if (EnableSSL)
                    client.SslProtocols = System.Security.Authentication.SslProtocols.Ssl3;

                query = SearchQuery.All;
                List<IMailFolder> mailFolderList = client.GetFolders(client.PersonalNamespaces[0]).ToList();
                IMailFolder fromFolder = client.GetFolder(mailFolderFrom);
                IMailFolder toFolder = client.GetFolder(mailFolderTo);
                fromFolder.Open(FolderAccess.ReadWrite);
                IList<UniqueId> uidss = fromFolder.Search(query);
                List<MailMessage> emails = new List<MailMessage>();
                for (int i = uidss.Count - 1; i >= 0; i--)
                {
                    MimeMessage message = fromFolder.GetMessage(new UniqueId(uidss[i].Id));
                    if (message.Date == mailMoveMessage.Date &&
                        message.MessageId == mailMoveMessage.MessageId &&
                        message.Subject == mailMoveMessage.Subject)
                    {
                        fromFolder.MoveTo(new UniqueId(uidss[i].Id), toFolder);
                        break;
                    }
                }
                client.Disconnect(true);
            }
            catch (Exception e)
            {
                client.Disconnect(true);
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }

        private void onComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
        }
    }
}