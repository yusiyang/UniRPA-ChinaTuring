using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace HttpServerActivity
{
    [Designer(typeof(HttpServerDesigner))]
    public class HttpServerActivity : NativeActivity
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


        #region 属性分类：杂项

        [Browsable(false)]
        private BookmarkResumptionHelper BookmarkResumptionHelper;

        [Browsable(false)]
        private Bookmark RuntimeBookmark;

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/HttpServer/HttpServer.png";
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
            metadata.AddArgument(new RuntimeArgument("HttpServer", typeof(string), ArgumentDirection.In, true));
            base.CacheMetadata(metadata);
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
                    Thread.Sleep(delayAfter);
                    return;
                }
                this.BookmarkResumptionHelper = context.GetExtension<BookmarkResumptionHelper>();
                this.StartHttpServer(context);
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

        void StartHttpServer(NativeActivityContext context)
        {
            httpBase = new HttpServerBase();
            httpBase.StartServer(12306);
            httpBase.RevMessageEvent += HttpBase_RevMessageEvent;
        }

        private void HttpBase_RevMessageEvent(object sender, HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                using (var streamReader = new StreamReader(request.InputStream))
                {
                    var data = streamReader.ReadToEnd();
                    this.BookmarkResumptionHelper.BeginResumeBookmark(RuntimeBookmark, data);
                    using (var stream = response.OutputStream)
                    {
                        var bytes = Encoding.UTF8.GetBytes("Handle End");
                        stream.Write(bytes, 0, bytes.Length);
                    }
                    response.StatusCode = 200;
                }
            }
            catch (Exception ex)
            {
                using (var stream = response.OutputStream)
                {
                    var bytes = Encoding.UTF8.GetBytes("Handle Error");
                    stream.Write(bytes, 0, bytes.Length);
                }
                response.StatusCode = 404;
            }
        }

        void StopMonitor(ActivityContext context)
        {

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


        private static HttpServerBase httpBase;
    }
}
