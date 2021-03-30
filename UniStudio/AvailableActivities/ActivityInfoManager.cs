using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStudio.AvailableActivities
{
    public class ActivityInfoManager
    {
        private static ActivityInfoManager _instance;

        private static object _lockObj = new object();

        public List<ActivityInfo> AvailableActivityInfos { get; private set; }

        public static ActivityInfoManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new ActivityInfoManager();
                        }
                    }
                }
                return _instance;
            }
        }

        private ActivityInfoManager()
        { }

        public void InitActivityNames()
        {
            AvailableActivityInfos = new List<ActivityInfo>();


        }
    }
}
