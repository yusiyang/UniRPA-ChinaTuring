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
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Http;
using Plugins.Shared.Library.Exceptions;

namespace AIActivity
{
    /// <summary>
    /// 票小秘通用发票识别
    /// </summary>
    [Designer(typeof(BaiduOCRDesigner))]
    public sealed class OCRInvoiceActivity : CodeActivity
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
        [OverloadGroup("ImageURL")]
        [DisplayName("图片URL")]
        [Browsable(true)]
        [Description("图片的URL地址。必须将文本放入引号中。")]
        public InArgument<string> ImageURL { get; set; }

        #endregion

        #region 属性分类：输出

        [Category("输出")]
        [Browsable(true)]
        [DisplayName("识别结果")]
        [Description("图片识别结果。")]
        public OutArgument<string> Result { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/AI/ocr-ticket.png";
            }
        }

        #endregion


        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input 
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string 
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string path = FileName.Get(context);
            string API_KEY = APIKey.Get(context);
            string SECRET_KEY = SecretKey.Get(context);
            string imgURL = ImageURL.Get(context);
            try
            {
                double timeStamp = ConvertToUnixTimestamp(DateTime.Now);
                string token = CalculateMD5Hash(API_KEY + '+' + timeStamp + '+' + SECRET_KEY);
                Dictionary<string, object> dic = new Dictionary<string, object>();
                dic.Add("app_key", API_KEY);
                dic.Add("timestamp", timeStamp.ToString());
                dic.Add("token", token);
                dic.Add("image_url", imgURL);
                string result;
                using (var client = new HttpClient())
                {
                    using (var multipartFormDataContent = new MultipartFormDataContent())
                    {
                        var values = new[]
                        {
                            new KeyValuePair<string, string>("app_key", API_KEY),
                            new KeyValuePair<string, string>("timestamp", timeStamp.ToString()),
                            new KeyValuePair<string, string>("token", token)
        };

                        foreach (var keyValuePair in values)
                        {
                            multipartFormDataContent.Add(new StringContent(keyValuePair.Value),
                                String.Format("\"{0}\"", keyValuePair.Key));
                        }

                        if (path != null)
                        {
                            string filename = System.IO.Path.GetFileName(path);
                            multipartFormDataContent.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(path)),
                           "\"image_file\"",
                           String.Format("\"{0}\"", filename));
                        }

                        var requestUri = "http://fapiao.glority.cn/v1/item/get_item_info";
                        result = client.PostAsync(requestUri, multipartFormDataContent).Result.Content.ReadAsStringAsync().Result;
                    }
                }

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