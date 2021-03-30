using MouseActivity;
using Plugins.Shared.Library;
using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace FTPActivity
{
    [Designer(typeof(SessionDesigner))]
    public class WithFtpSession : ContinuableAsyncNativeActivity
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
        //public new bool ContinueOnError { get; set; }

        [Category("常见")]
        [DisplayName("在此之前延迟")]
        [Description("活动开始执行任何操作之前的延迟时间（以毫秒为单位）。默认时间量为200毫秒。")]
        public InArgument<int> DelayBefore { get; set; }

        [Category("常见")]
        [DisplayName("在此之后延迟")]
        [Description("执行活动之后的延迟时间（以毫秒为单位）。默认时间量为300毫秒。")]
        public InArgument<int> DelayAfter { get; set; }

        #endregion


        #region 属性分类：服务器

        [Category("服务器")]
        [RequiredArgument]
        [DisplayName("主机")]
        public InArgument<string> Host { get; set; }

        [Category("服务器")]
        [DisplayName("端口")]
        public InArgument<int> Port { get; set; }

        #endregion


        #region 属性分类：凭据

        [Category("凭据")]
        [DisplayName("用户名")]
        public InArgument<string> Username { get; set; }

        [Category("凭据")]
        [DisplayName("密码")]
        public InArgument<string> Password { get; set; }

        [Category("凭据")]
        [DisplayName("匿名登录")]
        [DefaultValue(false)]
        public bool UseAnonymousLogin { get; set; }

        #endregion


        #region 属性分类：安全设置

        [Category("安全设置")]
        [DisplayName("FTPS 模式")]
        [DefaultValue(FtpsMode.None)]
        public FtpsMode FtpsMode { get; set; }

        [Category("安全设置")]
        [DisplayName("使用 SFTP")]
        [DefaultValue(false)]
        public bool UseSftp { get; set; }

        [Category("安全设置")]
        [DisplayName("客户端证书路径")]
        public InArgument<string> ClientCertificatePath { get; set; }

        [Category("安全设置")]
        [DisplayName("客户端证书密码")]
        public InArgument<string> ClientCertificatePassword { get; set; }

        [Category("安全设置")]
        [DisplayName("接受所有证书")]
        [DefaultValue(false)]
        public bool AcceptAllCertificates { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction<IFtpSession> Body { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get { return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/FTP/ftp-session.png"; }
        }

        #endregion


        private IFtpSession _ftpSession;

        public static readonly string FtpSessionPropertyName = "FtpSession";

        public WithFtpSession()
        {
            FtpsMode = FtpsMode.None;
            Body = new ActivityAction<IFtpSession>()
            {
                Argument = new DelegateInArgument<IFtpSession>(FtpSessionPropertyName),
                Handler = new Sequence() { DisplayName = "Do" }
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            // TODO: Validation code here.

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<NativeActivityContext>> ExecuteAsync(NativeActivityContext context, CancellationToken cancellationToken)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            IFtpSession ftpSession = null;
            FtpConfiguration ftpConfiguration = new FtpConfiguration(Host.Get(context));
            ftpConfiguration.Port = Port.Expression == null ? null : (int?)Port.Get(context);
            ftpConfiguration.UseAnonymousLogin = UseAnonymousLogin;
            ftpConfiguration.ClientCertificatePath = ClientCertificatePath.Get(context);
            ftpConfiguration.ClientCertificatePassword = ClientCertificatePassword.Get(context);
            ftpConfiguration.AcceptAllCertificates = AcceptAllCertificates;

            if (ftpConfiguration.UseAnonymousLogin == false)
            {
                ftpConfiguration.Username = Username.Get(context);
                ftpConfiguration.Password = Password.Get(context);

                if (string.IsNullOrWhiteSpace(ftpConfiguration.Username))
                {
                    Thread.Sleep(delayAfter);
                    throw new ArgumentNullException("EmptyUsernameException");
                }
            }

            if (UseSftp)
            {
                ftpSession = new SftpSession(ftpConfiguration);
            }
            else
            {
                ftpSession = new FtpSession(ftpConfiguration, FtpsMode);
            }

            await ftpSession.OpenAsync(cancellationToken);

            Thread.Sleep(delayAfter);
            return (nativeActivityContext) =>
            {
                if (Body != null)
                {
                    _ftpSession = ftpSession;
                    nativeActivityContext.ScheduleAction(Body, ftpSession, OnCompleted, OnFaulted);
                }
            };
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            if (_ftpSession == null)
            {
                throw new InvalidOperationException("FTPSessionNotFoundException");
            }

            _ftpSession.Close();
            _ftpSession.Dispose();
        }

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            PropertyDescriptor ftpSessionProperty = faultContext.DataContext.GetProperties()[WithFtpSession.FtpSessionPropertyName];
            IFtpSession ftpSession = ftpSessionProperty?.GetValue(faultContext.DataContext) as IFtpSession;

            ftpSession?.Close();
            ftpSession?.Dispose();
        }
    }
}
