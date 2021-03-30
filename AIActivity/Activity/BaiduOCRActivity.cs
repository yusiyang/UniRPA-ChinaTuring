﻿using Plugins.Shared.Library;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Baidu.Aip.Ocr;
using System.Drawing;
using MouseActivity;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace AIActivity
{
    /// <summary>
    /// 百度云OCR通用文字识别
    /// </summary>
    [Designer(typeof(BaiduOCRDesigner))]
    public sealed class BaiduOCRActivity : CodeActivity
    {
        #region 属性分类：常见

        public string _displayName;
        [Category("常见")]
        [DisplayName("显示名称")]
        public new string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    _displayName = base.DisplayName;
                }
                else
                {
                    base.DisplayName = _displayName;
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }
        [Browsable(false)]
        public string _DisplayName
        {
            get
            {
                return this.DisplayName;
            }
        }

        [Category("常见")]
        [DisplayName("出错时继续")]
        [Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        public bool ContinueOnError { get; set; }

        [Category("常见")]
        [DisplayName("在此之前延迟")]
        [Description("活动开始执行任何操作之前的延迟时间（以毫秒为单位）。默认时间量为200毫秒。")]
        public InArgument<int> DelayBefore { get; set; }

        [Category("常见")]
        [DisplayName("在此之后延迟")]
        [Description("执行活动之后的延迟时间（以毫秒为单位）。默认时间量为300毫秒。")]
        public InArgument<int> DelayAfter { get; set; }

        #endregion

        #region 属性分类：输入
        [Category("输入")]
        [RequiredArgument]
        [Browsable(true)]
        [Description("您的APIKey。必须将文本放入引号中。")]
        public InArgument<string> APIKey { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [Browsable(true)]
        [Description("您的SecretKey。必须将文本放入引号中。")]
        public InArgument<string> SecretKey { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("FileName")]
        [DisplayName("文件路径")]
        [Browsable(true)]
        [Description("图片文件的全路径。如果设置了此属性，则忽略输入项中Image属性。必须将文本放入引号中。")]
        public InArgument<string> FileName { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("image")]
        [DisplayName("图像")]
        [Browsable(true)]
        [Description("要进行文本识别的图像，仅支持Image变量。如果设置了此属性，则忽略输入项中FileName属性。")]
        public InArgument<System.Drawing.Image> image { get; set; }

        #endregion

        #region 属性分类：输出

        [Category("输出")]
        [Browsable(true)]
        [DisplayName("识别结果")]
        [Description("图片识别结果。")]
        public OutArgument<string> Result { get; set; }

        #endregion

        #region 属性分类：选项

        public enum LanguageType
        {
            CHN_ENG,//中英文混合；默认值     
            ENG,//英文
            POR,//葡萄牙语
            FRE,//法语
            GER,//德语
            ITA,//意大利语
            SPA,//西班牙语
            RUS,//俄语
            JAP,//日语
            KOR //韩语
        }
        LanguageType Language_type = LanguageType.CHN_ENG;
        [Category("选项")]
        [DisplayName("语言类型")]
        [Description("识别语言类型，默认为CHN_ENG。")]
        [Browsable(true)]
        public LanguageType language_type
        {
            get { return Language_type; }
            set { Language_type = value; }
        }

        bool Detect_direction = false;
        [Category("选项")]
        [DisplayName("检测图像朝向")]
        [Browsable(true)]
        [Description("是否检测图像朝向，默认不检测。朝向是指输入图像是正常方向还是逆时针旋转90/180/270度。")]
        public bool detect_direction
        {
            get { return Detect_direction; }
            set { Detect_direction = value; }
        }

        bool Detect_language = false;
        [Category("选项")]
        [DisplayName("检测语言")]
        [Browsable(true)]
        [Description("是否检测语言，默认不检测。当前支持中文、英语、日语、韩语。")]
        public bool detect_language
        {
            get { return Detect_language; }
            set { Detect_language = value; }
        }

        bool Probability = false;
        [Category("选项")]
        [DisplayName("返回置信度")]
        [Browsable(true)]
        [Description("是否返回识别结果中每一行的置信度。")]
        public bool probability
        {
            get { return Probability; }
            set { Probability = value; }
        }

        #endregion

        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/AI/baidu-ocr.png";
            }
        }

        #endregion

        //将图片转换成字节数组
        public byte[] SaveImage(String path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read); //将图片以文件流的形式进行保存
            BinaryReader br = new BinaryReader(fs);
            byte[] imgBytesIn = br.ReadBytes((int)fs.Length); //将流读入到字节数组中
            return imgBytesIn;
        }

        //将Image变量转换成字节数组
        public byte[] ConvertImageToByte(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        System.Drawing.Image img;
        byte[] by;
        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string path = FileName.Get(context);
            string API_KEY = APIKey.Get(context);
            string SECRET_KEY = SecretKey.Get(context);
            img = image.Get(context);
            try
            {
                if (path != null)
                {
                    by = SaveImage(path);
                }
                else
                {
                    by = ConvertImageToByte(img);
                }
                var client = new Ocr(API_KEY, SECRET_KEY);
                //修改超时时间  
                client.Timeout = 60000;
                //参数设置                     
                var options = new Dictionary<string, object>
                {
                    {"language_type", language_type},
                    {"detect_direction",detect_direction.ToString().ToLower()},
                    {"detect_language", detect_language.ToString().ToLower()},
                    {"probability", probability.ToString().ToLower()}
                };
                //带参数调用通用文字识别
                string result = client.GeneralBasic(by, options).ToString();
                Result.Set(context, result);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }
    }
}