using Plugins.Shared.Library;
using Plugins.Shared.Library.ActivityLog;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Activities.Tracking;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows.Threading;
using Newtonsoft.Json;
using UniExecutor.Core.Models;
using UniExecutor.Enums;
using UniExecutor.Service.Interface;
using UniExecutor.Services;
using UniStudio.Executor.Debugger;

namespace UniExecutor.Tracking
{
    public class VisualTrackingParticipant : TrackingParticipant
    {
        // 记录上一次的中断时候的位置（可能是断点中断，手动中断，单步调试等位置，主要供单步步过时记录步过前的位置）
        private ActivityInfo _lastDebugActivityInfo;

        // 主要用来记录中断时当前监视的变量信息
        private ActivityStateRecord _lastActivityStateRecord;

        private DebuggerManager _debuggerManager;

        private IViewOperateService _viewOperateService;

        private Dictionary<string, string> _activityIdParentMap = new Dictionary<string, string>();

        public VisualTrackingParticipant()
        {
            _debuggerManager = ExecutorContext.Current.DebuggerManager;
            _viewOperateService = ExecutorContext.Current.ViewOperateService;
            SlowStepEvent = new ManualResetEvent(false);
        }
        public ManualResetEvent SlowStepEvent { get; }

        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            OnTrackingRecordReceived(record, timeout);
        }

        private bool MeetingBreakpoint(ActivityInfo child)
        {
            var breakpointLocations = ExecutorContext.Current.LocationBreakpointDic;
            SourceLocation srcLoc = ExecutorContext.Current.ActivityIdLocationDic[child.Id];
            if (breakpointLocations.ContainsKey(srcLoc))
            {
                var types = breakpointLocations[srcLoc];
                if (types == (BreakpointTypes.Enabled | BreakpointTypes.Bounded))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsActivityAncestor(string id, string ancestorId)
        {
            if (id == ancestorId)
            {
                return true;
            }

            if (_activityIdParentMap.ContainsKey(id))
            {
                return IsActivityAncestor(_activityIdParentMap[id], ancestorId);
            }

            return false;
        }

        private void DoWaitThings(string id)
        {
            ShowCurrentLocation(id);
            ShowLocals();
        }

        private void ProcessSlowStep(string id)
        {
            if (_debuggerManager.Speed > 0)
            {
                DoWaitThings(id);
                SlowStepEvent.WaitOne(_debuggerManager.Speed);
            }
        }

        private void ProcessWait(string id)
        {
            bool isPaused = !SlowStepEvent.WaitOne(0);

            if (isPaused)
            {
                DoWaitThings(id);
            }
            _viewOperateService.SetDebuggingPaused(isPaused);

            SlowStepEvent.WaitOne();

            _viewOperateService.SetDebuggingPaused(false);

            HideCurrentLocation();           
        }

        private void ShowCurrentLocation(string id)
        {
            if (_debuggerManager.NextOperate == DebugOperate.Stop)
            {
                //停止时调用Dispatcher.Invoke会卡死，所以此处直接返回不往下走
                return;
            }

            _viewOperateService.ShowLocation(id);
        }


        private void HideCurrentLocation()
        {
            if (_debuggerManager.NextOperate == DebugOperate.Stop)
            {
                //停止时调用Dispatcher.Invoke会卡死，所以此处直接返回不往下走
                return;
            }

            _viewOperateService.HideLocation();
        }

        private void ShowLocals()
        {
            var locasModel = new LocalsModel
            {
                Variables = Format(_lastActivityStateRecord.Variables),
                Arguments = Format(_lastActivityStateRecord.Arguments)
            };
            _viewOperateService.ShowLocals(locasModel);
        }

        private IDictionary<string, string> Format(IDictionary<string, object> variables)
        {
            if (variables == null || variables.Count == 0)
            {
                return null;
            }
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (KeyValuePair<string, object> variable in variables)
            {
                var variableValue = variable.Value;
                if (variableValue != null&&(variableValue.GetType()==typeof(DataTable)||variableValue.GetType().IsSubclassOf(typeof(IEnumerable<>))|| variableValue.GetType().IsSubclassOf(typeof(IEnumerable))))
                {
                    variableValue = Newtonsoft.Json.JsonConvert.SerializeObject(variableValue,Formatting.Indented);
                }

                string value = (variableValue == null) ? string.Empty : variableValue.ToString().Display();
                dictionary.Add(variable.Key, value);
            }
            return dictionary;
        }

        //On Tracing Record Received call the TrackingRecordReceived with the record received information from the TrackingParticipant. 
        //We also do not worry about Expressions' tracking data
        protected void OnTrackingRecordReceived(TrackingRecord record, TimeSpan timeout)
        {
            if (record is WorkflowInstanceRecord)
            {

            }
            else if (record is ActivityScheduledRecord)
            {
                var activityScheduledRecord = record as ActivityScheduledRecord;

                if (activityScheduledRecord.Child != null && ExecutorContext.Current.ActivityIdLocationDic.ContainsKey(activityScheduledRecord.Child.Id))
                {
                    _activityIdParentMap[activityScheduledRecord.Child.Id] = activityScheduledRecord.Activity.Id;

                    if (MeetingBreakpoint(activityScheduledRecord.Child))
                    {
                        SlowStepEvent.Reset();
                        ProcessWait(activityScheduledRecord.Child.Id);
                        _lastDebugActivityInfo = activityScheduledRecord.Child;
                    }
                    else
                    {
                        if (_debuggerManager.NextOperate == DebugOperate.Null
                        || _debuggerManager.NextOperate == DebugOperate.Continue
                        )
                        {
                            ProcessSlowStep(activityScheduledRecord.Child.Id);
                        }
                        else if (_debuggerManager.NextOperate == DebugOperate.Break)
                        {
                            ProcessWait(activityScheduledRecord.Child.Id);
                            _lastDebugActivityInfo = activityScheduledRecord.Child;
                        }
                        else if (_debuggerManager.NextOperate == DebugOperate.StepInto)
                        {
                            ProcessWait(activityScheduledRecord.Child.Id);
                            _lastDebugActivityInfo = activityScheduledRecord.Child;
                        }
                        else if (_debuggerManager.NextOperate == DebugOperate.StepOver)
                        {
                            if (_lastDebugActivityInfo != null)
                            {
                                if (IsActivityAncestor(activityScheduledRecord.Activity.Id, _lastDebugActivityInfo.Id))
                                {
                                    _viewOperateService.SetDebuggingPaused(false);
                                }
                                else
                                {
                                    ProcessWait(activityScheduledRecord.Child.Id);
                                    _lastDebugActivityInfo = activityScheduledRecord.Child;
                                }
                            }
                            else if(!ExecutorContext.Current.IsInternalExecutor)
                            {
                                ProcessWait(activityScheduledRecord.Child.Id);
                                _lastDebugActivityInfo = activityScheduledRecord.Child;
                            }

                        }
                    }
                }
            }
            else if (record is ActivityStateRecord)
            {
                var activityStateRecord = record as ActivityStateRecord;

                if (activityStateRecord.State == ActivityStates.Closed
                     && (activityStateRecord.Activity.TypeName == "System.Activities.Statements.Sequence"
                         || activityStateRecord.Activity.TypeName == "System.Activities.Statements.Flowchart"
                         || activityStateRecord.Activity.TypeName == "WorkflowUtils.InvokeWorkflowFileActivity"
                        )
                    && (_debuggerManager.NextOperate == DebugOperate.Null
                        || _debuggerManager.NextOperate == DebugOperate.Continue
                        )
                    )
                {
                    ProcessSlowStep(activityStateRecord.Activity.Id);
                }
                else if (activityStateRecord.State == ActivityStates.Closed
                   && (
                   activityStateRecord.Activity.TypeName == "System.Activities.Statements.Sequence"
                   || activityStateRecord.Activity.TypeName == "System.Activities.Statements.Flowchart"
                   )
                   && (
                   _debuggerManager.NextOperate == DebugOperate.StepInto
                   || _debuggerManager.NextOperate == DebugOperate.StepOver
                        )
                   )
                {
                    if (!IsActivityAncestor(activityStateRecord.Activity.Id, _lastDebugActivityInfo.Id))
                    {
                        //此处需要判断下
                        ProcessWait(activityStateRecord.Activity.Id);
                    }
                }
                else if (activityStateRecord.State == ActivityStates.Closed
                    && activityStateRecord.Activity.TypeName == "WorkflowUtils.InvokeWorkflowFileActivity"
                    && (ExecutorContext.Current.LastExecutorContext?.NeedStepInto ?? false)
                    )
                {
                    ProcessWait(activityStateRecord.Activity.Id);
                    _lastDebugActivityInfo = activityStateRecord.Activity;
                }

                _lastActivityStateRecord = activityStateRecord;
                SetLastActivityLog();
            }
        }
        
        private void SetLastActivityLog()
        {
            var activityLog = new ActivityLog(_lastActivityStateRecord.Activity.Name, _lastActivityStateRecord.Activity.TypeName, _lastActivityStateRecord.Arguments);

            SharedObject.Instance.LastActivityLog = activityLog;
        }
    }
}