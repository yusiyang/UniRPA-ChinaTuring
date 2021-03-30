using Plugins.Shared.Library;
using Plugins.Shared.Library.ActivityLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniWorkforce.Services;

namespace UniWorkforce
{
    public class TaskContext
    {
        public string TaskId { get; set; }

        public string TaskNo { get; set; }

        public string ProcessName { get; set; }

        private TaskStatusEnum _taskStatusEnum;

        public TaskStatusEnum TaskStatus
        {
            get
            {
                return _taskStatusEnum;
            }
            set
            {
                _taskStatusEnum = value;
                Context.Current.RobotService.SendTaskStatus();
            }
        }

        public List<JobLogParams> TaskLogs { get; set; } = new List<JobLogParams>();

        public TaskContext()
        { }

        public TaskContext(string taskId, string taskNo, string processName = null)
        {
            TaskId = taskId;
            TaskNo = taskNo;
            ProcessName = processName;
        }

        public void AddLog(SharedObject.OutputType type, string msg, string msgDetails)
        {
            var jobLogParams = new JobLogParams
            {
                Description = string.Format("活动日志：msg={0},msgDetails={1}", msg, msgDetails),
                TaskNo = TaskNo,
                ProcessName = ProcessName,
                CreatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                TaskID = TaskId,
                RobotID = Context.Current.RobotId ?? "",
                UniqueNo = Context.Current.RobotUniqueNo,
                Type = (LogType)type
            };
            if (ActivityLogFormat.TryParse(msg, out var activityLog))
            {
                jobLogParams.Description = "活动信息";
                jobLogParams.ActivityName = activityLog.ActivityName;
                jobLogParams.ActivityType = activityLog.ActivityType;
                jobLogParams.Parameters = activityLog.ParameterStr.Replace(ActivityLogFormat.ParameterSeparator, "||");
            }

            TaskLogs.Add(jobLogParams);

            //for (var i = 0; i < 333; i++)
            //{
            //    TaskLogs.Add(jobLogParams);
            //}
        }

        public void Close()
        {
            Context.Current.RobotService.InsertTaskLogList();
            Context.Current.CurrentTaskContext = null;
            Context.Current.StartGetTask();
        }
    }
}
