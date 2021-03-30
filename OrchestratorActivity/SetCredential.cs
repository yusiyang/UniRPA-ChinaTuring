using System.Activities;
using System.ComponentModel;
using System;
using Plugins.Shared.Library;
using System.Security;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library.Exceptions;

namespace OrchestratorActivity
{
    [Designer(typeof(SetCredentialDesigner))]
    public sealed class SetCredential : CodeActivity
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

        public InArgument<Int32> _TimeoutMS = 30000;
        [Category("输入")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> TimeoutMS
        {
            get
            {
                return _TimeoutMS;
            }
            set
            {
                _TimeoutMS = value;
            }
        }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("用户名")]
        [Description("要设置的凭证的用户名。必须将文本放入引号中。")]
        public InArgument<string> UserName { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("密码")]
        [Description("要设置的凭证的密码。")]
        public InArgument<SecureString> PassWord { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("凭证名称")]
        [Description("要设置的凭证的名称。必须将文本放入引号中。")]
        public InArgument<string> CredentialName { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Orchestrator/set-credential.png"; } }

        #endregion


        public SetCredential()
        {
        }

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        //增加凭证 
        static extern bool CredWrite([In] ref NativeCredential userCredential, [In] UInt32 flags);

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
                Int32 _timeout = TimeoutMS.Get(context);
                Thread.Sleep(_timeout);
                latch = new CountdownEvent(1);
                Thread td = new Thread(() =>
                {
                    string credName = CredentialName.Get(context);
                    string userName = UserName.Get(context);
                    SecureString credentialName = PassWord.Get(context);
                    IntPtr inP = Marshal.SecureStringToBSTR(credentialName);
                    string passWord = Marshal.PtrToStringBSTR(inP);
                    System.Diagnostics.Debug.WriteLine(" secureStr.ToString() : " + passWord);
                    WriteCred(credName, userName, passWord, CRED_TYPE.GENERIC, CRED_PERSIST.LOCAL_MACHINE);

                    refreshData(latch);
                });
                td.TrySetApartmentState(ApartmentState.STA);
                td.IsBackground = true;
                td.Start();
                latch.Wait();
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

        public static int WriteCred(string key, string userName, string secret, CRED_TYPE type, CRED_PERSIST credPersist)
        {
            var byteArray = Encoding.Unicode.GetBytes(secret);
            if (byteArray.Length > 512)
                throw new ArgumentOutOfRangeException("The secret message has exceeded 512 bytes.");
            var cred = new Credential
            {
                TargetName = key,
                CredentialBlob = secret,
                CredentialBlobSize = (UInt32)Encoding.Unicode.GetBytes(secret).Length,
                AttributeCount = 0,
                Attributes = IntPtr.Zero,
                UserName = userName,
                Comment = null,
                TargetAlias = null,
                Type = type,
                Persist = credPersist
            };
            var ncred = NativeCredential.GetNativeCredential(cred);
            var written = CredWrite(ref ncred, 0);
            var lastError = Marshal.GetLastWin32Error();
            if (written)
            {
                return 0;
            }

            var message = "";
            if (lastError == 1312)
            {
                message = (string.Format("Failed to save " + key + " with error code {0}.", lastError)
                + "  This error typically occurrs on home editions of Windows XP and Vista.  Verify the version of Windows is Pro/Business or higher.");
            }
            else
            {
                message = string.Format("Failed to save " + key + " with error code {0}.", lastError);
            }
            System.Diagnostics.Debug.WriteLine("Error:" + message);
            return 1;
        }
    }
}
