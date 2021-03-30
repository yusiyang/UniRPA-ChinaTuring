using System;
using System.Activities;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Plugins.Shared.Library
{
    public abstract class ContinuableAsyncCodeActivity : AsyncTaskCodeActivity
    {
        [Category("常见")]
        [DisplayName("出错时继续")]
        [Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        public bool ContinueOnError { get; set; } = false;

        protected sealed override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            try
            {
                return base.BeginExecute(context, callback, state);
            }
            catch (Exception e)
            {
                if (ContinueOnError)
                {
                    Trace.TraceError(e.ToString());

                    var taskCompletionSource = new TaskCompletionSource<AsyncCodeActivityContext>(state);
                    taskCompletionSource.TrySetResult(null);
                    callback?.Invoke(taskCompletionSource.Task);

                    return taskCompletionSource.Task;
                }
                else
                {
                    throw;
                }
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            try
            {
                base.EndExecute(context, result);
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
