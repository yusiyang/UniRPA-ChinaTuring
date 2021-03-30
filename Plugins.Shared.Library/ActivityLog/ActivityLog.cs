using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.ActivityLog
{
    public class ActivityLog : IFormattable
    {
        public string ActivityName { get; set; }

        public string ActivityType { get; set; }

        public IDictionary<string,object> Parameters { get; set; }

        public string ParameterStr { get; set; }

        public ActivityLog()
        { }

        public ActivityLog(string activityName,string activityType,IDictionary<string,object> parameters)
        {
            ActivityName = activityName;
            ActivityType = activityType;
            Parameters = parameters;
        }

        public string ToString(string format)
        {
            string formattedString = null;
            if (!TryFormatter(format, new ActivityLogFormat(), out formattedString))
            {
                formattedString = ToString();
            }
            return formattedString;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            string formattedString = null;
            if (formatProvider == null || !TryFormatter(format, formatProvider, out formattedString))
            {
                formattedString = ToString();
            }
            return formattedString;
        }

        private bool TryFormatter(string format, IFormatProvider formatProvider, out string formattedString)
        {
            bool result = false;
            formattedString = null;
            if (formatProvider != null)
            {
                ICustomFormatter customFormatter = formatProvider.GetFormat(GetType()) as ICustomFormatter;
                if (customFormatter != null)
                {
                    result = true;
                    formattedString = customFormatter.Format(format, this, formatProvider);
                }
            }
            return result;
        }

        public override string ToString()
        {
            return ToString("N;T;P", new ActivityLogFormat());
        }
    }
}
