using System;
using System.Activities;
using System.ComponentModel;
using System.Diagnostics;

namespace Plugins.Shared.Library
{
    public abstract class ContinuableAsyncNativeActivity : AsyncTaskNativeActivity
    {
        [Category("常见")]
        [DisplayName("出错时继续")]
        [Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        public bool ContinueOnError { get; set; } = false;

        protected override void Execute(NativeActivityContext context)
        {
            try
            {
                base.Execute(context);
            }
            catch (Exception e)
            {
                if (ContinueOnError)
                {
                    Trace.TraceError(e.ToString());
                }
                else
                {
                    throw;
                }
            }
        }

        protected override void BookmarkResumptionCallback(NativeActivityContext context, Bookmark bookmark, object value)
        {
            try
            {
                base.BookmarkResumptionCallback(context, bookmark, value);
            }
            catch (Exception e)
            {
                if (ContinueOnError)
                {
                    Trace.TraceError(e.ToString());
                }
                else
                {
                    throw;
                }
            }
        }
    }

    public abstract class AsyncTaskNativeActivityContinue<T> : AsyncTaskNativeActivity<T>
    {
        public bool ContinueOnError { get; set; } = false;

        protected override void Execute(NativeActivityContext context)
        {
            try
            {
                base.Execute(context);
            }
            catch (Exception e)
            {
                if (ContinueOnError)
                {
                    Trace.TraceError(e.ToString());
                }
                else
                {
                    throw;
                }
            }
        }

        protected override void BookmarkResumptionCallback(NativeActivityContext context, Bookmark bookmark, object value)
        {
            try
            {
                base.BookmarkResumptionCallback(context, bookmark, value);
            }
            catch (Exception e)
            {
                if (ContinueOnError)
                {
                    Trace.TraceError(e.ToString());
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
