using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Extensions
{
    public static class RectangleExtensions
    {
        /// <summary>
        /// 获取矩形的中心点
        /// </summary>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static Point Center(this Rectangle rectangle)
        {
            var center = new Point
            {
                X = rectangle.X + rectangle.Width / 2,
                Y = rectangle.Y + rectangle.Height / 2
            };
            return center;
        }
    }
}
