using Plugins.Shared.Library;
using Plugins.Shared.Library.Extensions;
using System;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniExecutor.Core.Models;
using UniExecutor.Enums;
using UniExecutor.Executor;
using UniExecutor.Output;
using UniExecutor.Service.Interface;
using UniExecutor.Services;
using UniExecutor.Tracking;
using UniStudio.Executor.Debugger;

namespace UniExecutor
{
    public class ExecutorContext
    {
        private static object _lockObj = new object();

        private static bool _isCreated = false;

        private static ExecutorContext _instance;

        private static Stack<ExecutorContext> _contextStack=new Stack<ExecutorContext>();

        public ExecutorContext LastExecutorContext { get; private set; }

        public bool NeedStepInto { get; set; } = true;

        public bool IsInternalExecutor { get; set; }

        public ExecutorContext(JobModel jobModel)
        {
            lock (_lockObj)
            {
                JobModel = jobModel;
                WorkflowExecutor = new WorkflowExecutor(jobModel.ProcessInfo);
                if (string.IsNullOrEmpty(SharedObject.Instance.ProjectPath))
                {
                    SharedObject.Instance.ProjectPath = jobModel.ProcessInfo.ProcessDir;
                }
                SharedObject.Instance.SetOutputFun(Output);
                SharedObject.Instance.IsInUi = false;
                SharedObject.Instance.ViewOperateService = ViewOperateService;
                 _instance = this;

                _contextStack.Push(this);
            }
        }

        public static ExecutorContext Current => _instance;

        #region 相关的实体
        public JobModel JobModel { get;}

        public WorkflowExecutor WorkflowExecutor { get; }

        private DebuggerManager _debuggerManager;

        public DebuggerManager DebuggerManager
        {
            get
            {
                if(_debuggerManager==null)
                {
                    _debuggerManager = new DebuggerManager();
                }
                return _debuggerManager;
            }
        }

        private IViewOperateService _viewOperateService;

        public IViewOperateService ViewOperateService
        {
            get
            {
                if(_viewOperateService==null)
                {
                    _viewOperateService = new ViewOperateService();
                }
                return _viewOperateService;
            }
        }

        public VisualTrackingParticipant VisualTrackingParticipant { get; set; }

        public Dictionary<string, SourceLocation> ActivityIdLocationDic { get; set; }

        public IDictionary<SourceLocation, BreakpointTypes> LocationBreakpointDic { get; set; }

        public List<string> VariableNames { get; set; }

        public Dictionary<string,object> InputArguments { get; set; }
        #endregion

        public void Remove()
        {
            var lastExecutorContext = _contextStack.Pop();
            if (_contextStack.Count > 0)
            {
                _instance = _contextStack.Peek();
                _instance.LastExecutorContext = lastExecutorContext;

                if (string.IsNullOrEmpty(SharedObject.Instance.ProjectPath))
                {
                    SharedObject.Instance.ProjectPath = _instance.JobModel.ProcessInfo.ProcessDir;
                }
                SharedObject.Instance.ViewOperateService = _instance.ViewOperateService;
                SharedObject.Instance.SetOutputFun(_instance.Output);
            }
            else
            {
                _instance = null;
            }
        }

        public void Output(SharedObject.OutputType outputType, string message, string messageDetail)
        {
            var outputMessageModel = new OutputMessageModel
            {
                OutputType = outputType.GetHashCode(),
                Message = message,
                MessageDetail = messageDetail
            };
            ViewOperateService.OutputMessage(outputMessageModel);
        }
    }
}
