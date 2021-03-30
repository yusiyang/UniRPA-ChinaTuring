using Plugins.Shared.Library;
using Plugins.Shared.Library.ActivityLog;
using System;
using System.Activities.Tracking;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Tracking
{
    public class ActivityLogTrackingParticipant : TrackingParticipant
    {
        protected override void Track(TrackingRecord record, TimeSpan timeout)
        {
            if (record is ActivityStateRecord)
            {
                var activityStateRecord = record as ActivityStateRecord;

                var activityLog = new ActivityLog(activityStateRecord.Activity.Name, activityStateRecord.Activity.TypeName, activityStateRecord.Arguments);

                SharedObject.Instance.LastActivityLog = activityLog;
            }
        }
    }
}
