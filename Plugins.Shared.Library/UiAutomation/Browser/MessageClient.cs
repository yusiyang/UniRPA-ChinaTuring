using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuGet.Versioning;
using Plugins.Shared.Library.UiAutomation.Browser.NamedPipe;

namespace Plugins.Shared.Library.UiAutomation.Browser
{
    public class MessageClient
    {
        private IntPtr _browserHandle;
        public IntPtr BrowserHandle => GetBrowserHandle();
        private IntPtr _hostHandle;
        public IntPtr HostHandle => GetHostHandle();
        private string PipeName { get; }
        private static readonly JsonSerializerSettings JsonSetting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };
        public MessageClient(string pipeName)
        {
            PipeName = pipeName;
        }

        public string Message(NativeMessage message, int timeOut = 1000)
        {
            var result = "";
            try
            {
                using (var pipeClient = new NamedPipeClientStream("localhost", PipeName, PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None))
                {
                    if (timeOut <= 0)
                    {
                        pipeClient.Connect();
                    }
                    else
                    {
                        pipeClient.Connect(timeOut);//连接服务端
                    }

                    StreamWriter sw = new StreamWriter(pipeClient);
                    StreamReader sr = new StreamReader(pipeClient);

                    sw.WriteLine(JsonConvert.SerializeObject(message, JsonSetting));
                    sw.Flush();

                    result = sr.ReadLine();
                    pipeClient.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("管道报错：" + ex.Message + "\r\n" + ex.StackTrace);
                //throw ex;
            }

            return result;
        }
        private IntPtr GetBrowserHandle()
        {
            if (_browserHandle != IntPtr.Zero)
            {
                return _browserHandle;
            }
            var msg = new NativeMessage
            {
                FunctionCall = nameof(GetBrowserHandle)
            };
            var result = Message(msg);
            _ = int.TryParse(result, out var handle);
            return _browserHandle = (IntPtr)handle;
        }
        private IntPtr GetHostHandle()
        {
            if (_hostHandle != IntPtr.Zero)
            {
                return _hostHandle;
            }
            var msg = new NativeMessage
            {
                FunctionCall = nameof(GetHostHandle)
            };
            var result = Message(msg);
            _ = int.TryParse(result, out var handle);
            return _hostHandle = (IntPtr)handle;
        }
    }
}
