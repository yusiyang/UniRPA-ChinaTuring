using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugins.Shared.Library;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.IO;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.Exceptions;

namespace NetWorkActivity
{
    [Designer(typeof(PostDesigner))]
    public sealed class Post : CodeActivity
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

        InArgument<Int32> _GetTimeOut = 10000;
        [Category("输入")]
        [DisplayName("请求超时")]
        [Description("请求超时时间（毫秒）。")]
        public InArgument<Int32> GetTimeOut
        {
            get
            {
                return _GetTimeOut;
            }
            set
            {
                _GetTimeOut = value;
            }
        }

        [Category("输入")]
        [DisplayName("URL")]
        [Description("HTTP请求访问的网页地址。必须将文本放入引号中。")]
        public InArgument<string> URL { get; set; }

        [Category("输入")]
        [DisplayName("发送数据")]
        [Description("HTTP请求向服务器发送的数据。支持XML字符串。必须将文本放入引号中。")]
        public InArgument<string> String { get; set; }

        public enum PostTypeEnums
        {
            application_json,
            text_xml,
            application_x_www_form_urlencoded
        }
        [Category("输入")]
        [DisplayName("数据类型")]
        [Description("HTTP 请求向服务器发送的数据类型。可用的选项包括：application_json、text_xml、application_x_www_form_urlencoded。")]
        public PostTypeEnums PostType{ get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("响应数据")]
        [Description("发送数据后服务器响应的数据。")]
        public OutArgument<string> OutString { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string ClassName { get { return "Post"; } }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/NetWork/post-request.png";
            }
        }

        #endregion


        private string classID = Guid.NewGuid().ToString("N");

        static Post()
        {
        }

        //protected override async IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        //{
        //    string url = URL.Get(context);
        //    Int32 timeOut = GetTimeOut.Get(context);
        //    string xmlString = XmlString.Get(context);
        //    //string xmlString = "\"imageData\":{\"rowSetArray\":[{\"dataMap\":{\"SA_SW_TOKEN\":\"ff79c907-86fc-4446-9456-c53844571c24\",\"F_FPDM\":\"031001800204\",\"F_FPHM\":\"93324375\",\"F_KPRQ\":\"20190507\",\"F_JE\":\"12621.36\",\"F_JYM\":\"922801\"}}]}";

        //    HttpContent httpContent = new StringContent(xmlString);
        //    if (PostType == PostTypeEnums.application_josn)
        //        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/josn");
        //    if (PostType == PostTypeEnums.text_xml)
        //        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");
        //    if (PostType == PostTypeEnums.application_x_www_form_urlencoded)
        //        httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

        //    HttpClient httpClient = new HttpClient();
        //    httpClient.Timeout = TimeSpan.FromMilliseconds(timeOut);
        //    HttpResponseMessage res = await httpClient.PostAsync(url, httpContent);
        //    if (res.IsSuccessStatusCode)
        //    {
        //        Task<string> t = res.Content.ReadAsStringAsync();
        //        OutString.Set(context, t.Result);
        //        Debug.WriteLine("Result : " + t.Result);
        //    }
        //    else
        //    {
        //        SharedObject.RunInUI(() =>
        //        {
        //            SharedObject.Instance.Output(SharedObject.enOutputType.Error, "POST远程服务器无响应或超时");
        //        });
        //    }

        //    var tcs = new TaskCompletionSource<string>(state);
        //    return tcs.Task;
        //}
        //protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        //{
        //    //throw new NotImplementedException();
        //}

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            string url = URL.Get(context);
            Int32 timeOut = GetTimeOut.Get(context);
            string xmlString = String.Get(context);
            //string xmlString = "{\"imageData\":{\"rowSetArray\":[{\"dataMap\":{\"SA_SW_TOKEN\":\"ff79c907-86fc-4446-9456-c53844571c24\",\"F_FPDM\":\"031001800204\",\"F_FPHM\":\"933\",\"F_KPRQ\":\"20190507\",\"F_JE\":\"12621.36\",\"F_JYM\":\"59946528961555922801\"}}]}}";

            //HttpContent httpContent = new StringContent(xmlString);
            //if (PostType == PostTypeEnums.application_josn)
            //    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/josn");
            //if (PostType == PostTypeEnums.text_xml)
            //    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/xml");
            //if (PostType == PostTypeEnums.application_x_www_form_urlencoded)
            //    httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/x-www-form-urlencoded");

            //HttpClient httpClient = new HttpClient();
            //httpClient.Timeout = TimeSpan.FromMilliseconds(timeOut);
            //HttpResponseMessage res = httpClient.PostAsync(url, httpContent).Result;

            //Debug.WriteLine("Content : " + res.Content);
            //Debug.WriteLine("Content : " + res.ToString());
            //Debug.WriteLine("Content : " + res.StatusCode);

            //if (res.IsSuccessStatusCode)
            //{
            //    Task<string> t = res.Content.ReadAsStringAsync();
            //    OutString.Set(context, t.Result);
            //    Debug.WriteLine("Result : " + t.Result);
            //}
            //else
            //{
            //    SharedObject.RunInUI(() =>
            //    {
            //        SharedObject.Instance.Output(SharedObject.enOutputType.Error, "POST远程服务器无响应或超时");
            //    });
            //    Task<string> t = res.Content.ReadAsStringAsync();
            //    OutString.Set(context, t.Result);
            //    Debug.WriteLine("Result : " + t.Result);
            //}

            try
            {
                string Url = url;
                //string postDataStr = "{\"imageData\":{\"rowSetArray\":[{\"dataMap\":{\"SA_SW_TOKEN\":\"ff79c907-86fc-4446-9456-c53844571c24\",\"F_FPDM\":\"031001800204\",\"F_FPHM\":\"933\",\"F_KPRQ\":\"20190507\",\"F_JE\":\"12621.36\",\"F_JYM\":\"59946528961555922801\"}}]}}";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = "POST";
                request.Timeout = timeOut;

                if (PostType == PostTypeEnums.application_json)
                    request.ContentType = "application/json";
                if (PostType == PostTypeEnums.text_xml)
                    request.ContentType = "text/xml";
                if (PostType == PostTypeEnums.application_x_www_form_urlencoded)
                    request.ContentType = "application/x-www-form-urlencoded";

                request.ContentLength = Encoding.UTF8.GetByteCount(xmlString);
                Stream myRequestStream = request.GetRequestStream();
                StreamWriter myStreamWriter = new StreamWriter(myRequestStream, Encoding.GetEncoding("gb2312"));
                myStreamWriter.Write(xmlString);
                myStreamWriter.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                OutString.Set(context, retString);
                Debug.WriteLine("retString : " + retString);
                myStreamReader.Close();
                myResponseStream.Close();
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
