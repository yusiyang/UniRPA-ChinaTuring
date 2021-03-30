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
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library;
using UniExecutor.Tracking;
using UniExecutor.Enums;
using UniExecutor;
using UniExecutor.Executor;
using System.Threading;

namespace UniStudio.Executor.Debugger
{
    public class DebuggerManager
    {
        private WorkflowExecutor _workflowExecutor;

        public DebuggerManager()
        {
            _workflowExecutor = ExecutorContext.Current.WorkflowExecutor;
        }

        #region Speed
        /// <summary>
        /// 调试速度，毫秒为单位
        /// </summary>
        public int Speed { get;private set; }

        public DebugSpeed SpeedType { get;private set; }

        public void SetSpeed(DebugSpeed speedType)
        {
            SpeedType = speedType;
            switch (speedType)
            {
                case DebugSpeed.Off:
                    Speed = 0;
                    break;
                case DebugSpeed.One:
                    Speed = 2000;
                    break;
                case DebugSpeed.Two:
                    Speed = 1000;
                    break;
                case DebugSpeed.Three:
                    Speed = 500;
                    break;
                case DebugSpeed.Four:
                    Speed = 250;
                    break;
                default:
                    Speed = 0;
                    break;
            }
        }
        #endregion

        #region Operate
        public DebugOperate NextOperate { get; set; }

        public void Continue(DebugOperate operate = DebugOperate.Continue)
        {
            NextOperate = operate;
            ExecutorContext.Current.VisualTrackingParticipant.SlowStepEvent.Set();
            Thread.Sleep(5);
            ExecutorContext.Current.VisualTrackingParticipant.SlowStepEvent.Reset();
        }

        public void Break()
        {
            //中断调试
            NextOperate = DebugOperate.Break;
        }
        #endregion

        #region 生成Tracker
        private VisualTrackingParticipant GenerateTracker()
        {
            const String all = "*";

            VisualTrackingParticipant simTracker = new VisualTrackingParticipant()
            {
                TrackingProfile = new TrackingProfile()
                {
                    Name = "CustomTrackingProfile",
                    Queries =
                        {
                         new CustomTrackingQuery()
                            {
                                Name = all,
                                ActivityName = all
                            },
                            new WorkflowInstanceQuery()
                            {
                                // Limit workflow instance tracking records for started and completed workflow states
                                States = { WorkflowInstanceStates.Started, WorkflowInstanceStates.Completed },
                            },

                             new ActivityStateQuery()
                            {
                                // Subscribe for track records from all activities for all states
                                ActivityName = all,
                                States = { all },

                                // Extract workflow variables and arguments as a part of the activity tracking record
                                // VariableName = "*" allows for extraction of all variables in the scope
                                // of the activity
                                Variables =
                                {
                                     all
                                },

                                Arguments =
                                {
                                    all
                                },
                            },

                             new ActivityScheduledQuery()
                            {
                                ActivityName = all,
                                ChildActivityName = all
                            },
                        }
                }
            };

            TrackerVarsAdd(simTracker);

            ExecutorContext.Current.VisualTrackingParticipant = simTracker;
            return simTracker;
        }

        /// <summary>
        ///提前填充工作流用到的变量，以便在全局作用域里监视
        /// </summary>
        /// <param name="simTracker"></param>
        private void TrackerVarsAdd(VisualTrackingParticipant simTracker)
        {
            if (ExecutorContext.Current.VariableNames?.Count > 0)
            {
                foreach (var item in simTracker.TrackingProfile.Queries)
                {
                    if (item is ActivityStateQuery)
                    {
                        var query = item as ActivityStateQuery;

                        foreach (var name in ExecutorContext.Current.VariableNames)
                        {
                            query.Variables.Add(name);
                        }
                        break;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// 开始执行调试流程
        /// </summary>
        public void Debug()
        {
            if (ExecutorContext.Current.NeedStepInto)
            {
                _workflowExecutor.Run(ExecutorContext.Current.InputArguments, GenerateTracker());
            }
            else
            {
                _workflowExecutor.Run(ExecutorContext.Current.InputArguments);
            }
        }
    }
}
