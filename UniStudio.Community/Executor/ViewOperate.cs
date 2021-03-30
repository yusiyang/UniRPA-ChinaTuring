using System;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Activities.Presentation.Validation;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using UniExecutor.Core.Interfaces;
using UniExecutor.Core.Models;
using UniNamedPipe.Attributes;
using UniStudio.Community.DataManager;
using UniStudio.Community.Executor.Enums;
using UniStudio.Community.ViewModel;

namespace UniStudio.Community.Executor
{
    [PipeServer("ViewOperate", "ViewOperate")]
    public class ViewOperate : IViewOperate
    {
        private DebuggerManager DebuggerManager => DocumentContext.Current.DebuggerManager;

        private Dictionary<string, object> _inputArguments;

        public void HideLocation()
        {
            if (DebuggerManager.NextOperate == DebuggerManager.DebugOperate.Stop)
            {
                //停止时调用Dispatcher.Invoke会卡死，所以此处直接返回不往下走
                return;
            }

            ViewModelLocator.instance.Main.View.Dispatcher.Invoke(DispatcherPriority.Render
               , (Action)(() =>
               {
                   var debugManagerView = DocumentContext.Current.Services.GetService<IDesignerDebugView>();
                   debugManagerView.CurrentLocation = null;
               }));
        }

        public void OutputMessage(OutputMessageModel outputMessageModel)
        {
            SharedObject.Instance.Output((SharedObject.OutputType)outputMessageModel.OutputType, outputMessageModel.Message, outputMessageModel.MessageDetail);
        }

        public void ShowLocals(LocalsModel localsModel)
        {
            Messenger.Default.Send(localsModel, "ShowLocals");
        }

        public void ShowLocation(string activityId)
        {
            if (DebuggerManager.NextOperate == DebuggerManager.DebugOperate.Stop)
            {
                //停止时调用Dispatcher.Invoke会卡死，所以此处直接返回不往下走
                return;
            }

            var activityIdLocationMapping = DebuggerManager.GetActivityIdLocationMapping();

            if (!activityIdLocationMapping.ContainsKey(activityId))
            {
                return;
            }

            SourceLocation srcLoc = activityIdLocationMapping[activityId];
            ViewModelLocator.instance.Main.View.Dispatcher.Invoke(DispatcherPriority.Render
                , (Action)(() =>
                {
                    var debugManagerView = DocumentContext.Current.Services.GetService<IDesignerDebugView>();
                    debugManagerView.CurrentLocation = srcLoc;
                }));
        }

        public void SetDebuggingPaused(bool paused)
        {
            Common.RunInUIAsync(() =>
            {
                ViewModelLocator.instance.Main.IsWorkflowDebuggingPaused = paused;
            });
        }

        public void EndRun(int stoppedType, Exception exception)
        {
            Messenger.Default.Send(this, "EndRun");
        }

        public void InvokeWorkflow(string filePath, Dictionary<string, object> inputArguments, int currentOperate)
        {
            var nameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            _inputArguments = inputArguments;
            ViewModelLocator.instance.Main.View.Dispatcher.Invoke(DispatcherPriority.Render
               , (Action)(() =>
               {
                   if (!NeedOpenWorkflow(filePath,currentOperate))
                   {
                       var processInfo = new ProcessInfo
                       {
                           ProcessDir = Path.GetDirectoryName(filePath),
                           Main = Path.GetFileName(filePath)
                       };
                       var jobModel = new JobModel
                       {
                           ProcessInfo = processInfo
                       }; 
                       var mainViewModel = ViewModelLocator.instance.Main;
                       if (mainViewModel.IsWorkflowRunning)
                       { 
                           mainViewModel.ExecutorService.InternalStart(inputArguments, jobModel); 
                       }
                       else
                       {
                           mainViewModel.DebuggerService.InternalStart(inputArguments, jobModel);
                       }
                       return;
                   }
                   var isExist = false;

                   ViewModelLocator.instance.Dock.LastActiveDocumentStack.Push(ViewModelLocator.instance.Dock.ActiveDocument);
                   foreach (var doc in ViewModelLocator.instance.Dock.Documents)
                   {
                       if (doc.XamlPath == filePath)
                       {
                           isExist = true;
                           doc.IsSelected = true;
                           break;
                       }
                   }
                   if (!isExist)
                   {
                       ViewModelLocator.instance.Dock.NewSequenceDocument(nameWithoutExt, filePath, false, ValidationCompleted);
                       ViewModelLocator.instance.Dock.ActiveDocument.IsReadOnly = true;
                   }
                   else
                   {
                       InternalStart();
                   }
               }));
        }

        private bool NeedOpenWorkflow(string filePath, int currentOperate)
        {
            if(ViewModelLocator.instance.Main.IsWorkflowRunning)
            {
                return false;
            }
            if(currentOperate==DebuggerManager.DebugOperate.StepInto.GetHashCode()|| ViewModelLocator.instance.Main.SlowStepSpeed!= DebugSpeed.Off)
            {
                return true;
            }
            var breakpointDic = ProjectSettingsDataManager.instance.m_projectBreakpointsDataManager.m_breakpointsDict;
            var relativePath = Common.MakeRelativePath(SharedObject.Instance.ProjectPath, filePath);
            if (breakpointDic!=null&&breakpointDic.ContainsKey(relativePath))
            {
                return true;
            }
            return false;
        }

        private void ValidationCompleted(object sender,EventArgs e)
        {
            ViewModelLocator.instance.Dock.ActiveDocument.ValidationCompleted = null;
            var validationService = DocumentContext.Current.Services.GetService<ValidationService>();
            validationService.RemoveEventHandler("ValidationCompleted");

            InternalStart();
        }

        private void InternalStart()
        {
            SharedObject.Instance.Output(SharedObject.OutputType.Trace, "InternalDebug Start");

            var mainViewModel = ViewModelLocator.instance.Main;
            mainViewModel.DebuggerService.InternalStart(_inputArguments);

            SharedObject.Instance.Output(SharedObject.OutputType.Trace, "InternalDebug End");
        }

        public void InternalEndRun(IDictionary<string, object> outputs)
        {
            ViewModelLocator.instance.Main.View.Dispatcher.Invoke(DispatcherPriority.Render
                  , (Action)(() =>
                  {
                      if (ViewModelLocator.instance.Main.IsWorkflowDebugging)
                      {
                          var debugManagerView = DocumentContext.Current.Services.GetService<IDesignerDebugView>();
                          debugManagerView.CurrentLocation = null;
                      }

                      DocumentViewModel lastDoc;
                      if (ViewModelLocator.instance.Dock.LastActiveDocumentStack.Count > 0)
                      {
                          lastDoc = ViewModelLocator.instance.Dock.LastActiveDocumentStack.Pop();
                      }
                      else
                      {
                          lastDoc = ViewModelLocator.instance.Dock.ActiveDocument;
                      }

                      foreach (var doc in ViewModelLocator.instance.Dock.Documents)
                      {
                          if (doc == lastDoc)
                          {
                              doc.IsSelected = true;
                              break;
                          }
                      }
                  }));
        }
    }
}
