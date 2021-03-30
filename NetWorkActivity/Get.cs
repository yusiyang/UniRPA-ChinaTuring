using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.Librarys;

namespace NetWorkActivity
{
    [Designer(typeof(GetDesigner))]
    public sealed class Get : AsyncCodeActivity
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


        #region 属性分类：请求

        InArgument<Int32> _GetTimeOut = 10000;
        [Category("请求")]
        [DisplayName("请求超时")]
        [Description("请求超时时间（毫秒）。")]
        public InArgument<Int32> GetTimeOut
        {
            get
            {
                return _GetTimeOut;
            }
            set
            {
                _GetTimeOut = value;
            }
        }

        [Category("请求")]
        [DisplayName("URL")]
        [Description("HTTP请求访问的网页地址。必须将文本放入引号中。")]
        public InArgument<string> URL { get; set; }

        #endregion


        #region 属性分类：响应

        [Category("响应")]
        [DisplayName("响应格式")]
        [Description("响应结果格式。默认为Any，可选Xml和Json两种格式。")]
        public Enums.AcceptTypeEnum AcceptType { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("响应数据")]
        [Description("发送数据后服务器响应的数据。必须将文本放入引号中。")]
        public OutArgument<string> OutString { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string ClassName { get { return "Get"; } }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/NetWork/get-request.png";
            }
        }

        #endregion        

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string url = URL.Get(context);
            var timeout = GetTimeOut.Get(context);


            var cancellationTokenSource = (CancellationTokenSource)(context.UserState = new CancellationTokenSource());
            var response = HttpClientHelper.GetAsync(url, cancellationTokenSource.Token, acceptType: AcceptType.GetDescription(), timout: timeout);
            
            TaskCompletionSource<string> taskCompletionSource = new TaskCompletionSource<string>(state);
            response.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    taskCompletionSource.TrySetException(task.Exception.InnerExceptions);
                }
                else if (task.IsCanceled)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                else
                {
                    taskCompletionSource.TrySetResult(task.Result);
                }
                callback?.Invoke(taskCompletionSource.Task);
            });
            return taskCompletionSource.Task;
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {

            var task = (Task<string>)result;

            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
            try
            {
                if (task.IsFaulted)
                {
                    throw task.Exception.InnerException;
                }
                if (task.IsCanceled || context.IsCancellationRequested)
                {
                    context.MarkCanceled();
                }
                else
                {
                    OutString.Set(context, task.Result);
                }
            }
            catch (OperationCanceledException)
            {
                context.MarkCanceled();
            }
            catch (Exception e)
            {
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            ((CancellationTokenSource)context.UserState).Cancel();
            base.Cancel(context);
        }

    }
}
