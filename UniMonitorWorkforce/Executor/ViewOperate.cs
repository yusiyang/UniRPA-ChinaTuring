using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniNamedPipe.Attributes;
using UniWorkforce.Executor.Enums;
using UniWorkforce.Services;
using UniWorkforce.ViewModel;

namespace UniWorkforce.Executor
{
    [PipeServer("ViewOperate", "ViewOperate")]
    public class ViewOperate : IViewOperate
    {
        public void OutputMessage(OutputMessageModel outputMessageModel)
        {
            SharedObject.Instance.Output((SharedObject.OutputType)outputMessageModel.OutputType, outputMessageModel.Message, outputMessageModel.MessageDetail);
        }

        public void EndRun(int stoppedType, Exception exception)
        {
            Messenger.Default.Send(this, "EndRun");
            switch((StoppedType)stoppedType)
            {
                case StoppedType.Force:
                    Context.Current.CurrentTaskContext.TaskStatus = TaskStatusEnum.Stopped;
                    break;
                case StoppedType.Exception:
                    Context.Current.CurrentTaskContext.TaskStatus = TaskStatusEnum.Fault;
                    break;
                case StoppedType.Normal:
                default:
                    Context.Current.CurrentTaskContext.TaskStatus = TaskStatusEnum.Finished;
                    break;
            }
        }

        public void ShowLocation(string activityId)
        {
            throw new NotImplementedException();
        }

        public void HideLocation()
        {
            throw new NotImplementedException();
        }

        public void ShowLocals(LocalsModel localsModel)
        {
            throw new NotImplementedException();
        }

        public void SetDebuggingPaused(bool paused)
        {
            throw new NotImplementedException();
        }

        public void InvokeWorkflow(string filePath, Dictionary<string, object> inputArguments, int currentOperate)
        {
            var processInfo = new ProcessInfo
            {
                ProcessDir = Path.GetDirectoryName(filePath),
                Main = Path.GetFileName(filePath),
            };
            var jobModel = new JobModel
            {
                ProcessInfo = processInfo
            };
            ViewModelLocator.instance.Main.ExecutorService.InternalStart(inputArguments, jobModel);
        }

        public void InternalEndRun(IDictionary<string, object> outputs)
        {
            
        }
    }
}
