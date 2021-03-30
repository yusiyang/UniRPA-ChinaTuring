using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    /// <summary>
    /// 图片帮助类
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// 图片转成字节
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static byte[] ImageToBytes(Image img)
        {
            MemoryStream ms = new MemoryStream();
            byte[] imagedata = null;
            img.Save(ms,ImageFormat.Bmp);
            imagedata = ms.GetBuffer();
            return imagedata;
        }

        /// <summary>
        /// 字节转换成图片
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static Image BytesToImage(byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            Image img = Image.FromStream(ms);
            return img;
        }
    }
}
