using FluentFTP;
using MouseActivity;
using Plugins.Shared.Library;
using System;
using System.Activities;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FTPActivity
{
    [Designer(typeof(FTPActDesigner))]
    public class DownloadFiles : ContinuableAsyncCodeActivity
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
        public InArgument<string> RemotePath { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("本地路径")]
        public InArgument<string> LocalPath { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("创建文件夹")]
        public bool Recursive { get; set; }

        [Category("选项")]
        [DisplayName("包含子文件夹")]
        public bool Create { get; set; }

        [Category("选项")]
        [DisplayName("覆盖")]
        public bool Overwrite { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get { return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/FTP/file-download.png"; }
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

            string remotePath = RemotePath.Get(context);
            string localPath = LocalPath.Get(context);

            FtpObjectType objectType = await ftpSession.GetObjectTypeAsync(remotePath, cancellationToken);
            if (objectType == FtpObjectType.Directory)
            {
                if (string.IsNullOrWhiteSpace(Path.GetExtension(localPath)))
                {
                    if (!Directory.Exists(localPath))
                    {
                        if (Create)
                        {
                            Directory.CreateDirectory(localPath);
                        }
                        else
                        {
                            Thread.Sleep(delayAfter);
                            throw new ArgumentException("PathNotFoundException", localPath);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(delayAfter);
                    throw new InvalidOperationException("IncompatiblePathsException");
                }
            }
            else
            {
                if (objectType == FtpObjectType.File)
                {
                    if (string.IsNullOrWhiteSpace(Path.GetExtension(localPath)))
                    {
                        localPath = Path.Combine(localPath, Path.GetFileName(remotePath));
                    }

                    string directoryPath = Path.GetDirectoryName(localPath);

                    if (!Directory.Exists(directoryPath))
                    {
                        if (Create)
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        else
                        {
                            Thread.Sleep(delayAfter);
                            throw new InvalidOperationException("PathNotFoundException");
                        }
                    }
                }
                else
                {
                    Thread.Sleep(delayAfter);
                    throw new NotImplementedException("UnsupportedObjectTypeException");
                }
            }

            if(Overwrite) 
                await ftpSession.DownloadAsync(remotePath, localPath, FtpLocalExists.Overwrite, Recursive, cancellationToken);
            else
                await ftpSession.DownloadAsync(remotePath, localPath, FtpLocalExists.Append, Recursive, cancellationToken);

            Thread.Sleep(delayAfter);
            return (asyncCodeActivityContext) =>
            {
                
            };
        }
    }
}
