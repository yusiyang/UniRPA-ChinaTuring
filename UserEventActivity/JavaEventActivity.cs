using System;
using System.Activities;
using System.ComponentModel;
using Plugins.Shared.Library;
using System.Diagnostics;

namespace UserEventActivity
{
    [Designer(typeof(JavaEventDesigner))]
    public sealed class JavaEventActivity : NativeActivity
    {
        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/UserEvent/monitor.png";
            }
        }

        public new string DisplayName
        {
            get
            {
                return "Java Event";
            }
        }

        [Browsable(false)]
        private BookmarkResumptionHelper BookmarkResumptionHelper;
        [Browsable(false)]
        private Bookmark RuntimeBookmark;

        [Category("Input")]
        public InArgument<string> TitleName { get; set; }
        [Category("Input")]
        public InArgument<IntPtr> JavaWindow { get; set; }

        [Browsable(false)]
        AccessBridge ab;

        public JavaEventActivity()
        {
            ab = new AccessBridge();
            ab.Initialize();
        }
        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        [Browsable(false)]
        string _TitleName;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.RequireExtension<BookmarkResumptionHelper>();
            metadata.AddDefaultExtensionProvider<BookmarkResumptionHelper>(() => new BookmarkResumptionHelper());
            metadata.AddArgument(new RuntimeArgument("TitleName", typeof(string), ArgumentDirection.In, true));
            //if (this.ChangeType == "")
            //{
            //    metadata.AddValidationError("Type 为空!");
            //}
            //if (this._NotifyFilters == "")
            //{
            //    metadata.AddValidationError("Type 为空!");
            //}
            base.CacheMetadata(metadata);
        }

        protected override void Execute(NativeActivityContext context)
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

        void StartMonitor(NativeActivityContext context)
        {
            _TitleName = TitleName.Get(context);
            ab.Events.FocusGained += Event_Trigger;
        }
        void StopMonitor(ActivityContext context)
        {
            try
            {
                if (ab != null)
                {
                    ab.Events.FocusGained -= Event_Trigger;
                   // ab.Dispose();
                   // ab = null;
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
        void Event_Trigger(int vmid, JavaObjectHandle evt, JavaObjectHandle source)
        {
            AccessibleContextInfo info;
            ab.Functions.GetAccessibleContextInfo(vmid, source, out info);
            Console.WriteLine(info.name);
            //if (this.RuntimeBookmark != null)
            //{
            //    this.BookmarkResumptionHelper.BeginResumeBookmark(RuntimeBookmark, "");
            //}
            if (Equals(info.name,_TitleName))
            {
                if (this.RuntimeBookmark != null)
                {
                    this.BookmarkResumptionHelper.BeginResumeBookmark(RuntimeBookmark, "");
                }
            }
        }

        //protected override void Execute(CodeActivityContext context)
        //{
        //    _TitleName = TitleName.Get(context);
        //    ab.Events.FocusGained += Events_FocusGained;
        //    latch = new CountdownEvent(1);
        //    latch.Wait();
        //    //if (_TitleName == "生成凭证")
        //    //    Thread.Sleep(8000);
        //    //else
        //    //    Thread.Sleep(20000);
        //}

        //private void Events_FocusGained(int vmid, JavaObjectHandle evt, JavaObjectHandle source)
        //{
        //      AccessibleContextInfo info;
        //      ab.Functions.GetAccessibleContextInfo(vmid, source, out info);
        //    Thread.Sleep(200);
        //    Console.WriteLine(info.name);
        //    if (Equals(_TitleName,info.name))
        //      {
        //        Task.Run(() => {
        //            refreshData(latch);
        //        });

        //    }
        //}
    }
}
