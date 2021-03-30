using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.UiAutomation.CaptureEvents
{
    public class CapturedEventArgs:EventArgs
    {
        public RectangleRange CaptureRange { get; private set; }

        public string ImagePath { get; set; }

        public CapturedEventArgs(double x, double y, double width, double height,string imagePath=null)
        {
            CaptureRange = new RectangleRange(x, y, width, height);
            ImagePath = imagePath;
        }
    }
}
