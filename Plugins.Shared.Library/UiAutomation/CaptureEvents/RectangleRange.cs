using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.UiAutomation.CaptureEvents
{
    public struct RectangleRange
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Height { get; set; }

        public double Width { get; set; }
        
        public RectangleRange(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Point Center
        {
            get
            {
                var center = new Point();
                center.X = X + Width / 2;
                center.Y = Y + Height / 2;
                return center;
            }
        }
    }
}
