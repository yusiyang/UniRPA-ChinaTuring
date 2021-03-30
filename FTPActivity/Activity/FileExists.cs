using MouseActivity;
using Plugins.Shared.Library;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;


namespace FTPActivity
{
    [Designer(typeof(FTPActDesigner))]
    public class FileExists : ContinuableAsyncCodeActivity
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


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("路径")]
        [Description("指定要进行检查的文件路径。必须将文本放入引号中。")]
        public InArgument<string> RemotePath { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("是否存在")]
        public OutArgument<bool> Exists { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get { return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/FTP/exist-file.png"; }
        }

        #endregion


        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            PropertyDescriptor ftpSessionProperty = context.DataContext.GetProperties()[WithFtpSession.FtpSessionPropertyName];
            IFtpSession ftpSession = ftpSessionProperty?.GetValue(context.DataContext) as IFtpSession;

            if (ftpSession == null)
            {
                Thread.Sleep(delayAfter);
                throw new InvalidOperationException("FTPSessionNotFoundException");
            }

            bool exists = await ftpSession.FileExistsAsync(RemotePath.Get(context), cancellationToken);

            Thread.Sleep(delayAfter);
            return (asyncCodeActivityContext) =>
            {
                Exists.Set(asyncCodeActivityContext, exists);
            };
        }
    }
}
