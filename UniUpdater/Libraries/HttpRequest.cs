using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace UniUpdater.Libraries
{
    class HttpRequest
    {
        public static string Get(string url)
        {
            string result = "";
            Stream stream = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                stream = resp.GetResponseStream();

                //获取内容 
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return result;
            }
            finally
            {
                if(stream != null)
                {
                    stream.Close();
                }
            }
            return result;
        }


        public static string Get(string url, Dictionary<string, string> dic)
        {
            string result = "";
            StringBuilder builder = new StringBuilder();
            builder.Append(url);
            if (dic.Count > 0)
            {
                builder.Append("?");
                int i = 0;
                foreach (var item in dic)
                {
                    if (i > 0)
                        builder.Append("&");
                    builder.AppendFormat("{0}={1}", item.Key, item.Value);
                    i++;
                }
            }

            Stream stream = null;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(builder.ToString());
                //添加参数 
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                stream = resp.GetResponseStream();

                //获取内容 
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                return result;
            }
            finally
            {
                if(stream != null)
                {
                    stream.Close();
                }
            }
            return result;
        }



    }
}
