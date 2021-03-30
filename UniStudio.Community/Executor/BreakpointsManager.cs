using System;
using System.Activities;
using System.Activities.Debugger;
using System.Activities.Presentation.Debug;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.Services;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Plugins.Shared.Library.Extensions;
using UniStudio.Community.DataManager;
using UniStudio.Community.Librarys;
using UniStudio.Community.ViewModel;
using WorkflowUtils;

namespace UniStudio.Community.Executor
{
    class BreakpointsManager
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public static void SetBreakpoint(DocumentViewModel activeDocument,string activityId,bool IsEnabled)
        {
            var context = DocumentContext.GetContext(activeDocument);
            var workflowDesigner = context.WorkflowDesigner;
            var debuggerManager = context.DebuggerManager;

            var activityIdLocationMapping = debuggerManager.GetActivityIdLocationMapping(true);
            if (activityIdLocationMapping.ContainsKey(activityId))
            {
                SourceLocation srcLoc = activityIdLocationMapping[activityId];

                if (IsEnabled)
                {
                    workflowDesigner.DebugManagerView.InsertBreakpoint(srcLoc, BreakpointTypes.Enabled | BreakpointTypes.Bounded);
                }
                else
                {
                    workflowDesigner.DebugManagerView.DeleteBreakpoint(srcLoc);
                }
            }
            else
            {
                //找不到断点位置，说明文件有修改，则该断点信息删除
                ProjectSettingsDataManager.instance.m_projectBreakpointsDataManager.RemoveBreakpointLocation(activeDocument.RelativeXamlPath, activityId);
            }
            
        }

        public static IDictionary<SourceLocation, BreakpointTypes> GetBreakpointLocations(DocumentViewModel activeDocument = null)
        {
            activeDocument = activeDocument?? ViewModelLocator.instance.Dock.ActiveDocument;
            var workflowDesigner = activeDocument.WorkflowDesignerInstance;
            return workflowDesigner.DebugManagerView.GetBreakpointLocations();
        }

        /// <summary>
        /// 移除选中的活动的断点
        /// </summary>
        /// <param name="activeDocument"></param>
        public static void RemoveBreakpoint(DocumentViewModel activeDocument=null,bool recursive=true)
        {
            try
            {
                if(activeDocument==null)
                {
                    activeDocument = ViewModelLocator.instance.Dock.ActiveDocument;
                }
                var context = DocumentContext.GetContext(activeDocument);
                var workflowDesigner = context.WorkflowDesigner;

                var modelItemList = new List<ModelItem>();
                var selectedModelItem = workflowDesigner.Context.Items.GetValue<Selection>().PrimarySelection;
                if (selectedModelItem == null)
                {
                    return;
                }
                modelItemList.Add(selectedModelItem);

                if(recursive)
                {
                    var modelService = workflowDesigner.Context.Services.GetService<ModelService>();
                    var selectedModelItems= modelService.Find(selectedModelItem, typeof(Activity));
                    if(selectedModelItems.Any())
                    {
                        modelItemList.AddRange(selectedModelItems);
                    }
                }

                var debuggerManager = context.DebuggerManager;

                var activityIdLocationMapping = debuggerManager.GetActivityIdLocationMapping(true);
                if (activityIdLocationMapping.Count == 0)
                {
                    return;
                }

                var breakpointLocations = workflowDesigner.DebugManagerView.GetBreakpointLocations();

                foreach (var modelItem in modelItemList)
                {
                    var activity = modelItem.GetCurrentValue() as Activity;
                    if (activity == null || activity is CommentOutActivity || activity.Id.IsNullOrWhiteSpace())
                    {
                        continue;
                    }
                    SourceLocation srcLoc = activityIdLocationMapping[activity.Id];
                    //TODO WJF srcLoc在只有一个组件时为null，需要特殊处理下

                    if (breakpointLocations.ContainsKey(srcLoc))
                    {
                        workflowDesigner.DebugManagerView.DeleteBreakpoint(srcLoc);
                    }
                }
            }
            catch (Exception err)
            {
                //特殊情况时触发，目前在flowchart中如果不连接start node,也会报错
                Logger.Debug(err, logger);
            }
        }

        public static void ToggleBreakpoint(DocumentViewModel activeDocument)
        {
            try
            {
                var context = DocumentContext.GetContext(activeDocument);
                var workflowDesigner = context.WorkflowDesigner;
                Activity activity = workflowDesigner.Context.Items.GetValue<Selection>().
                        PrimarySelection.GetCurrentValue() as Activity;
                if (activity == null || activity is CommentOutActivity)
                {
                    return;
                }

                //切换该活动的断点
                var debuggerManager = context.DebuggerManager;

                var activityIdLocationMapping = debuggerManager.GetActivityIdLocationMapping(true);
                if (activityIdLocationMapping.Count == 0)
                {
                    return;
                }

                if (activity.Id.IsNullOrWhiteSpace())
                {
                    return;
                }
                SourceLocation srcLoc = activityIdLocationMapping[activity.Id];
                //TODO WJF srcLoc在只有一个组件时为null，需要特殊处理下

                bool bInsertBreakpoint = false;
                var breakpointLocations = workflowDesigner.DebugManagerView.GetBreakpointLocations();
                if (breakpointLocations.ContainsKey(srcLoc))
                {
                    var types = breakpointLocations[srcLoc];
                    if (types != (BreakpointTypes.Enabled | BreakpointTypes.Bounded))
                    {
                        bInsertBreakpoint = true;
                    }
                }
                else
                {
                    bInsertBreakpoint = true;
                }

                if (bInsertBreakpoint)
                {
                    workflowDesigner.DebugManagerView.InsertBreakpoint(srcLoc, BreakpointTypes.Enabled | BreakpointTypes.Bounded);
                    //ProjectSettingsDataManager.instance.m_projectBreakpointsDataManager.AddBreakpointLocation(activeDocument.RelativeXamlPath, activity.Id, true);
                }
                else
                {
                    workflowDesigner.DebugManagerView.DeleteBreakpoint(srcLoc);
                    //ProjectSettingsDataManager.instance.m_projectBreakpointsDataManager.RemoveBreakpointLocation(activeDocument.RelativeXamlPath, activity.Id);
                }
            }
            catch (Exception err)
            {
                //特殊情况时触发，目前在flowchart中如果不连接start node,也会报错
                Logger.Debug(err, logger);
            }
            
        }

        public static void RemoveAllBreakpoints(DocumentViewModel activeDocument)
        {
            var workflowDesigner = activeDocument.WorkflowDesignerInstance;
            workflowDesigner.DebugManagerView.ResetBreakpoints();

            ProjectSettingsDataManager.instance.m_projectBreakpointsDataManager.RemoveAllBreakpointsLocation(activeDocument.RelativeXamlPath);
        }
    }
}
