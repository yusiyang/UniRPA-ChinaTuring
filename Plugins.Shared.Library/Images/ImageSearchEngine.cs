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
    public static class ImageSearchEngine
    {
        private static Mat _sourceMat;

        private static Mat _temlateMat;

        private static TemplateMatchModes _matchMode = TemplateMatchModes.CCoeffNormed;

        public static void Initialize(Image sourceImage, Image matchTemplate)
        {
            _sourceMat = Mat.FromImageData(ImageHelper.ImageToBytes(sourceImage), ImreadModes.AnyColor);
            _temlateMat = Mat.FromImageData(ImageHelper.ImageToBytes(matchTemplate), ImreadModes.AnyColor);
        }

        public static Rectangle Search()
        {
            var reslut = new Mat();
            //copy 到imageDisplay 然后又不用，还会提示异常，删除这段代码；
            //var imageDisplay = new Mat();
            //_sourceMat.CopyTo(imageDisplay);

            var reslutCols = _sourceMat.Cols - _temlateMat.Cols + 1;
            var reslutRows = _sourceMat.Rows - _temlateMat.Rows + 1;
            reslut.Create(reslutRows, reslutCols, MatType.CV_32FC1);

            Cv2.MatchTemplate(_sourceMat, _temlateMat, reslut, _matchMode);
            //Cv2.Normalize(reslut, reslut, 1, 0, NormTypes.MinMax, -1,new Mat());

            Point minLocation, maxLocation, matchLoc;
            double minVal = -1;
            double maxVal = 0;
            Cv2.MinMaxLoc(reslut, out minVal, out maxVal, out minLocation, out maxLocation);

            matchLoc = maxLocation;
            if (maxVal < 0.95)
            {
                return Rectangle.Empty;
            }

            //if (_matchMode== TemplateMatchModes.SqDiff|| _matchMode == TemplateMatchModes.SqDiffNormed)
            //{
            //    matchLoc = minLocation;
            //    similarity = 1 - minVal;
            //}
            //else
            //{
            //    matchLoc = maxLocation;
            //    similarity = maxVal;
            //}

            var rectangle = new Rectangle(matchLoc.X, matchLoc.Y, _temlateMat.Cols, _temlateMat.Rows);
            return rectangle;
        }
    }
}
