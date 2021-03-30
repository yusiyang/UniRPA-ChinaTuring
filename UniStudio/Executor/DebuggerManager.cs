using System;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Presentation;
using System.Activities.Tracking;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.Debug;
using System.Reflection;
using GalaSoft.MvvmLight.Messaging;
using UniStudio.ViewModel;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using UniStudio.Librarys;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library;
using UniStudio.Executor.Enums;
using log4net;

namespace UniStudio.Executor
{
    public class DebuggerManager
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private DocumentContext _context;

        public DebuggerManager(DocumentContext context)
        {
            _context = context;
            _activityIdLocationMapping = new Dictionary<string, SourceLocation>();
        }     
        
        public void Reset()
        {
            _activityIdLocationMapping.Clear();
            NextOperate = DebugOperate.Null;
        }

        private Dictionary<string, SourceLocation> _activityIdLocationMapping;

        public Dictionary<string,SourceLocation> GetActivityIdLocationMapping(bool forceUpdate=false)
        {
            if(!forceUpdate)
            {
                return _activityIdLocationMapping;
            }
            _activityIdLocationMapping.Clear();

            var debugView = _context.WorkflowDesigner.DebugManagerView;//不能保存m_workflowDesigner.DebugManagerView在以后使用，会是旧数据
            var modelService = _context.Services.GetService<ModelService>();

            var nonPublicInstance = BindingFlags.Instance | BindingFlags.NonPublic;
            var debuggerServiceType = typeof(DebuggerService);
            var ensureMappingMethodName = "EnsureSourceLocationUpdated";
            var mappingFieldName = "instanceToSourceLocationMapping";
            var ensureMappingMethod = debuggerServiceType.GetMethod(ensureMappingMethodName, nonPublicInstance);
            var mappingField = debuggerServiceType.GetField(mappingFieldName, nonPublicInstance);

            if (ensureMappingMethod == null)
                throw new MissingMethodException(debuggerServiceType.FullName, ensureMappingMethodName);
            if (mappingField == null)
                throw new MissingFieldException(debuggerServiceType.FullName, mappingFieldName);

            //var activity = (modelService.Root.GetCurrentValue() as ActivityBuilder).Implementation as Activity;
            //var rootActivity = activity.GetRoot();
            //try
            //{
            //    if (rootActivity != null)
            //        WorkflowInspectionServices.CacheMetadata(rootActivity);
            //}
            //catch(Exception ex)
            //{
            //    Logger.Error(ex, logger);
            //}

            ensureMappingMethod.Invoke(debugView, new object[0]);
            var mapping = mappingField.GetValue(debugView) as Dictionary<object, SourceLocation>;

            if(mapping==null||mapping.Count==0)
            {
                return _activityIdLocationMapping;
            }

            foreach (object instance in mapping.Keys)
            {
                var wfElement = instance as Activity;
                if (wfElement != null&&!wfElement.Id.IsNullOrWhiteSpace())
                {
                    _activityIdLocationMapping[wfElement.Id] = mapping[instance];
                }
            }
            return _activityIdLocationMapping;
        }

        public List<string> GetVariableNames()
        {
            List<string> varNameLsit = new List<string>();

            ModelService modelService = _context.Services.GetService<ModelService>();

            IEnumerable<ModelItem> flowcharts = modelService.Find(modelService.Root, typeof(Flowchart));
            IEnumerable<ModelItem> sequences = modelService.Find(modelService.Root, typeof(Sequence));

            foreach (var modelItem in flowcharts)
            {
                foreach (var varItem in modelItem.Properties["Variables"].Collection)
                {
                    var varName = varItem.Properties["Name"].ComputedValue as string;
                    varNameLsit.Add(varName);
                }
            }

            foreach (var modelItem in sequences)
            {
                foreach (var varItem in modelItem.Properties["Variables"].Collection)
                {
                    var varName = varItem.Properties["Name"].ComputedValue as string;
                    varNameLsit.Add(varName);
                }
            }

            return varNameLsit;
        }


        public DebugOperate NextOperate { get; private set; }

        public enum DebugOperate
        {
            Null,//无操作
            StepInto,//步入
            StepOver,//步过
            Continue,//继续
            Break,//中断
            Stop,//停止
        }

        public void Stop()
        {
            NextOperate=DebugOperate.Stop;//防止有事件卡住未往下走导致无法正常停止
        }
    }
}
