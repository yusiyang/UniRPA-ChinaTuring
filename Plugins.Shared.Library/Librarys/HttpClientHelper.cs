using FlaUI.Core.Logging;
using FlaUI.Core.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public enum AcceptTypeEnum
    {
        Json,
        Xml
    }

    public class HttpClientHelper
    {
        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="queryDic"></param>
        /// <param name="acceptType"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(string url, Dictionary<string, string> queryDic = null, string acceptType = "application/json", string referer = null)
        {
            var responseString = await GetAsync(url, queryDic, acceptType, referer);

            if (typeof(T) == typeof(string))
            {
                return (T)Convert.ChangeType(responseString, typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(responseString);

        }

        public static async Task<string> GetAsync(string url,Dictionary<string, string> queryDic = null, string acceptType = "application/json", string referer = null, int timout = 30000)
        {
            return await GetAsync(url, CancellationToken.None, queryDic, acceptType, referer, timout);
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="queryDic"></param>
        /// <param name="acceptType"></param>
        /// <param name="referer"></param>
        /// <param name="timout">超时，以毫秒为单位</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string url, CancellationToken cancellationToken, Dictionary<string, string> queryDic = null, string acceptType = "application/json", string referer = null,int timout=30000)
        {
            var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None };
            using (var httpClient = new HttpClient(handler))
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(timout);
                #region
                var urlParams = new StringBuilder();
                if (queryDic != null && queryDic.Count > 0)
                {
                    foreach (var item in queryDic)
                    {
                        urlParams.Append($"{item.Key}={item.Value}&");
                    }
                }
                if (urlParams.Length > 0)
                {
                    urlParams = urlParams.Remove(urlParams.Length - 1, 1);
                    url = $"{url}?{urlParams.ToString()}";
                }
                #endregion

                httpClient.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
                if (!referer.IsNullOrWhiteSpace())
                {
                    httpClient.DefaultRequestHeaders.Referrer = new Uri(referer);
                }
                var response = await httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("请求失败");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                return responseString;
            }
        }


        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="queryDic"></param>
        /// <param name="acceptType"></param>
        /// <param name="referer"></param>
        /// <returns></returns>
        public static T Post<T>(string url, dynamic postData, Dictionary<string, string> queryDic = null, string acceptType = "application/json", string referer = null)
        {
            try
            {
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.None };
                using (var httpClient = new HttpClient(handler))
                {
                    #region
                    var urlParams = new StringBuilder();
                    if (queryDic != null && queryDic.Count > 0)
                    {
                        foreach (var item in queryDic)
                        {
                            urlParams.Append($"{item.Key}={item.Value}&");
                        }
                    }
                    if (urlParams.Length > 0)
                    {
                        urlParams = urlParams.Remove(urlParams.Length - 1, 1);
                        url = $"{url}?{urlParams.ToString()}";
                    }
                    #endregion
                    httpClient.DefaultRequestHeaders.Add("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/77.0.3865.90 Safari/537.36");
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
                    var httpContent = new StringContent(JsonConvert.SerializeObject(postData));
                    httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    if (!referer.IsNullOrWhiteSpace())
                    {
                        httpClient.DefaultRequestHeaders.Referrer = new Uri(referer);
                    }

                    var response = httpClient.PostAsync(url, httpContent).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("请求失败：" + (int)response.StatusCode+" "+response.ReasonPhrase);
                    }


                    if (typeof(T) == typeof(string))
                    {
                        return (T)Convert.ChangeType(responseString, typeof(T));
                    }

                    var domain = JsonConvert.DeserializeObject<T>(responseString);
                    return domain;
                }
            }
            catch
            {
                throw;
            }

        }
    }
}
