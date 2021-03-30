using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace FileActivity
{
    [Designer(typeof(FileChangeTriggerActivityDesigner))]
    public sealed class FileChangeTriggerActivity : NativeActivity
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


        #region 属性分类：文件

        [Category("文件")]
        [DisplayName("文件名")]
        [Description("要监视更改的文件的名称。必须将文本放入引号中。")]
        public InArgument<string> FileName { get; set; }

        [Category("文件")]
        [DisplayName("路径")]
        [Description("要监视更改的文件的路径。必须将文本放入引号中。")]
        public InArgument<string> Path { get; set; }

        #endregion


        #region 属性分类：事件

        [Category("事件")]
        [DisplayName("更改类型")]
        [Description("要监控的更改的类型。")]
        public string ChangeType { get; set; }

        [Category("事件")]
        [DisplayName("通知筛选器")]
        [Description("要监控更改的筛选器的按位组合。")]
        public string _NotifyFilters { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("包含子目录")]
        [Description("指定是否包含指定位置的子目录。")]
        public bool IncludeSubdirectories  { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        private BookmarkResumptionHelper BookmarkResumptionHelper;

        [Browsable(false)]
        private Bookmark RuntimeBookmark;

        [Browsable(false)]
        private FileSystemWatcher FileSystemWatcher;

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/file/file-change-trigger.png";
            }
        }

        #endregion


        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.RequireExtension<BookmarkResumptionHelper>();
            metadata.AddDefaultExtensionProvider<BookmarkResumptionHelper>(() => new BookmarkResumptionHelper());
            metadata.AddArgument(new RuntimeArgument("Path", typeof(string), ArgumentDirection.In, true));
            if (this.ChangeType == "")
            {
                metadata.AddValidationError("Type 为空!");
            }
            if (this._NotifyFilters == "")
            {
                metadata.AddValidationError("Type 为空!");
            }
            base.CacheMetadata(metadata);
        }

        public FileChangeTriggerActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(FileChangeTriggerActivity), "ChangeType", new EditorAttribute(typeof(ChangeTypeEditor), typeof(PropertyValueEditor)));
            builder.AddCustomAttributes(typeof(FileChangeTriggerActivity), "_NotifyFilters", new EditorAttribute(typeof(NotifyFiltersEditor), typeof(PropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(NativeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                this.RuntimeBookmark = (context.Properties.Find("MonitorBookmark") as Bookmark);
                if (this.RuntimeBookmark == null)
                {
                    return;
                }
                this.BookmarkResumptionHelper = context.GetExtension<BookmarkResumptionHelper>();
                this.StartMonitor(context);
                context.CreateBookmark();
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

        private NotifyFilters getCboInfo()
        {
            NotifyFilters exp = new NotifyFilters();
            if(_NotifyFilters.Contains("LastAccess") && _NotifyFilters.Contains("LastWrite"))
                exp = (NotifyFilters)((NotifyFilters.FileName) | (NotifyFilters.DirectoryName) | (NotifyFilters.LastAccess) | (NotifyFilters.LastWrite));
            if (_NotifyFilters.Contains("LastWrite") &&!_NotifyFilters.Contains("LastAccess"))
                exp = (NotifyFilters)((NotifyFilters.FileName) | (NotifyFilters.DirectoryName) | (NotifyFilters.LastWrite));
            if (_NotifyFilters.Contains("LastAccess") && !_NotifyFilters.Contains("LastWrite"))
                exp = (NotifyFilters)((NotifyFilters.FileName) | (NotifyFilters.DirectoryName) | (NotifyFilters.LastAccess));
            if (!_NotifyFilters.Contains("LastAccess") && !_NotifyFilters.Contains("LastWrite"))
                exp = (NotifyFilters)((NotifyFilters.FileName) | (NotifyFilters.DirectoryName));
            return exp;
        }

        void StartMonitor(NativeActivityContext context)
        {
            string path = this.Path.Get(context);
            string text = this.FileName.Get(context);
            if ((File.GetAttributes(path) & FileAttributes.Directory) != FileAttributes.Directory && string.IsNullOrEmpty(text))
            {
                text = System.IO.Path.GetFileName(path);
                path = System.IO.Path.GetDirectoryName(path);
            }
            this.FileSystemWatcher = new FileSystemWatcher(path);
            this.FileSystemWatcher.Filter = text;
            this.FileSystemWatcher.IncludeSubdirectories = this.IncludeSubdirectories;
                FileSystemWatcher.NotifyFilter = getCboInfo();

            this.FileSystemWatcher.NotifyFilter = NotifyFilters.FileName|NotifyFilters.Size;
            if (this.ChangeType.Contains("All") || this.ChangeType.Contains("Changed"))
            {
                this.FileSystemWatcher.Changed += new FileSystemEventHandler(this.Event_Trigger);
            }
            if (this.ChangeType.Contains("All") || this.ChangeType.Contains("Created"))
            {
                this.FileSystemWatcher.Created += new FileSystemEventHandler(this.Event_Trigger);
            }
            if (this.ChangeType.Contains("All") || this.ChangeType.Contains("Renamed"))
            {
                this.FileSystemWatcher.Renamed += new RenamedEventHandler(this.Event_Trigger);
            }
            if (this.ChangeType.Contains("All") || this.ChangeType.Contains("Deleted"))
            {
                this.FileSystemWatcher.Deleted += new FileSystemEventHandler(this.Event_Trigger);
            }
            this.FileSystemWatcher.EnableRaisingEvents = true;
        }
        void StopMonitor(ActivityContext context)
        {
            try
            {
                if (this.FileSystemWatcher != null)
                {
                    this.FileSystemWatcher.EnableRaisingEvents = false;
                    this.FileSystemWatcher.Dispose();
                    this.FileSystemWatcher = null;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
            }
        }
        protected override void Cancel(NativeActivityContext context)
        {
            this.StopMonitor(context);
            base.Cancel(context);
        }
        protected override void Abort(NativeActivityAbortContext context)
        {
            this.StopMonitor(context);
            base.Abort(context);
        }
        void Event_Trigger(object sender, object args)
        {
            if (this.RuntimeBookmark != null)
            {
                this.BookmarkResumptionHelper.BeginResumeBookmark(RuntimeBookmark, args);
            }
        }
    }
}
