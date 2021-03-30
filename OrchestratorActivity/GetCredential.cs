using System.Activities;
using System.ComponentModel;
using System;
using Plugins.Shared.Library;
using System.Security;
using System.Runtime.InteropServices;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library.Exceptions;

namespace OrchestratorActivity
{
    [Designer(typeof(GetCredentialDesigner))]
    public sealed class GetCredential : CodeActivity
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
        [DisplayName("凭证名称")]
        [Description("要检索的凭证的名称。必须将文本放入引号中。")]
        public InArgument<string> CredentialName { get; set; }

        [Category("输入")]
        [DisplayName("超时（毫秒）")]
        [Description("指定最长等待时间（以毫秒为单位），如果超出该时间活动未运行，系统就会报错。默认值为30000毫秒（30秒）。")]
        public InArgument<int> TimeoutMS { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("密码")]
        [Description("检索到的凭证的安全密码。")]
        public OutArgument<SecureString> PassWord { get; set; }

        [Category("输出")]
        [DisplayName("用户名")]
        [Description("检索到的凭证的用户名。")]
        public OutArgument<string> UserName { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Orchestrator/get-credential.png"; } }

        #endregion


        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        //读取凭证信息 
        static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag, out IntPtr CredentialPtr);

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
                    IntPtr credPtr = new IntPtr();
                    WReadCred(credName, CRED_TYPE.GENERIC, CRED_PERSIST.LOCAL_MACHINE, out credPtr);
                    Credential lRawCredential = (Credential)Marshal.PtrToStructure(credPtr, typeof(Credential));
                    SecureString securePassWord = new SecureString();
                    foreach (char c in lRawCredential.CredentialBlob)
                    {
                        securePassWord.AppendChar(c);
                    }
                    UserName.Set(context, lRawCredential.UserName);
                    PassWord.Set(context, securePassWord);

                    refreshData(latch);
                });
                td.TrySetApartmentState(ApartmentState.STA);
                td.IsBackground = true;
                td.Start();
                latch.Wait();
                //System.Diagnostics.Debug.WriteLine("UserName:" + lRawCredential.UserName);
                //System.Diagnostics.Debug.WriteLine("CredentialBlob:" + lRawCredential.CredentialBlob);
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

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CredEnumerate(string filter, uint flag, out uint count, out IntPtr pCredentials);

        public static bool WReadCred(string targetName, CRED_TYPE credType, CRED_PERSIST reservedFlag, out IntPtr intPtr)
        {
            return CredRead(targetName, credType, (int)reservedFlag, out intPtr);
        }
    }
}
