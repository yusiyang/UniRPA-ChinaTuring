using Newtonsoft.Json;
using System;
using System.Activities.Debugger;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor.Core.Models
{
    public class ActivityLocation
    {
        public string FileName { get; set; }

        public int StartLine { get; set; }

        public int StartColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public ActivityLocation() { }

        public SourceLocation ToSourceLocation()
        {
            return new SourceLocation(FileName, StartLine, StartColumn, EndLine, EndColumn);
        }

        public static ActivityLocation FromSourceLocation(SourceLocation sourceLocation)
        {
            var activityLocation = new ActivityLocation
            {
                FileName = sourceLocation.FileName,
                StartLine = sourceLocation.StartLine,
                StartColumn = sourceLocation.StartColumn,
                EndLine = sourceLocation.EndLine,
                EndColumn = sourceLocation.EndColumn
            };
            return activityLocation;
        }

        public override bool Equals(object obj)
        {
            ActivityLocation sourceLocation = obj as ActivityLocation;
            if (sourceLocation == null)
            {
                return false;
            }
            if (FileName != sourceLocation.FileName)
            {
                return false;
            }
            if (StartLine != sourceLocation.StartLine || StartColumn != sourceLocation.StartColumn || EndLine != sourceLocation.EndLine || EndColumn != sourceLocation.EndColumn)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return ((!string.IsNullOrEmpty(FileName)) ? FileName.GetHashCode() : 0) ^ StartLine.GetHashCode() ^ StartColumn.GetHashCode();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static explicit operator ActivityLocation(string jsonString)
        {
            return JsonConvert.DeserializeObject<ActivityLocation>(jsonString);
        }
    }
}
