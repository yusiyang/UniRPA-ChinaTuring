using Plugins.Shared.Library.Extensions;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Tracking;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using UniExecutor;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniExecutor.Executor;
using UniExecutor.Tracking;
using UniNamedPipe.Attributes;

namespace UniStudio.Executor
{
    [PipeServer("Executor","Executor")]
    public class RunExecutor : IExecutor
    {
        private WorkflowExecutor WorkflowExecutor=>ExecutorContext.Current.WorkflowExecutor;

        /// <summary>
        /// 开始执行运行流程
        /// </summary>
        public void Start()
        {
            WorkflowExecutor.Run(null,GenerateTracker());
        }

        public void Stop()
        {
            WorkflowExecutor.Stop();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        private ActivityLogTrackingParticipant GenerateTracker()
        {
            var all = "*";
            var trackingParticipant = new ActivityLogTrackingParticipant
            {
                TrackingProfile = new TrackingProfile()
                {
                    Name = "CustomTrackingProfile",
                    Queries =
                        {
                             new ActivityStateQuery()
                            {
                                ActivityName = all,
                                States = { all },
                                Variables =
                                {
                                     all
                                },
                                Arguments =
                                {
                                    all
                                },
                            },
                        }
                }
            };
            return trackingParticipant;
        }

        public void InternalStart(JobModel jobModel, Dictionary<string, object> inputArguments)
        {
            new ExecutorContext(jobModel);
            
            WorkflowExecutor.Run(inputArguments, GenerateTracker());
        }
    }
}
