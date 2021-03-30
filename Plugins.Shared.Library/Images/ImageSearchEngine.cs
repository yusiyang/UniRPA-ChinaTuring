using OpenCvSharp;
using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = OpenCvSharp.Point;

namespace Plugins.Shared.Library.Images
{
    /// <summary>
    /// 图片搜索引擎
    /// </summary>
    public class ImageSearchEngine
    {
        private Mat _sourceMat;

        private Mat _temlateMat;

        private TemplateMatchModes _matchMode;

        public ImageSearchEngine(Image sourceImage, Image matchTemplate,TemplateMatchModes matchMode= TemplateMatchModes.SqDiff)
        {
            _sourceMat = Mat.FromImageData(ImageHelper.ImageToBytes(sourceImage), ImreadModes.AnyColor);
            _temlateMat = Mat.FromImageData(ImageHelper.ImageToBytes(matchTemplate), ImreadModes.AnyColor);
            _matchMode = matchMode;
        }

        public Rectangle Search()
        {
            var reslut = new Mat();
            var imageDisplay = new Mat();
            _sourceMat.CopyTo(imageDisplay);

            var reslutCols = _sourceMat.Cols - _temlateMat.Cols + 1;
            var reslutRows = _sourceMat.Rows - _temlateMat.Rows + 1;
            reslut.Create(reslutRows, reslutCols, MatType.CV_32FC1);

            Cv2.MatchTemplate(_sourceMat, _temlateMat, reslut, _matchMode);
            Cv2.Normalize(reslut, reslut, 1, 0, NormTypes.MinMax, -1);

            Point minLocation, maxLocation,matchLoc;
            Cv2.MinMaxLoc(reslut, out minLocation, out maxLocation);
            if(_matchMode== TemplateMatchModes.SqDiff|| _matchMode == TemplateMatchModes.SqDiffNormed)
            {
                matchLoc = minLocation;
            }
            else
            {
                matchLoc = maxLocation;
            }

            var rectangle = new Rectangle(matchLoc.X, matchLoc.Y, _temlateMat.Cols, _temlateMat.Rows);
            return rectangle;
        }
    }
}
