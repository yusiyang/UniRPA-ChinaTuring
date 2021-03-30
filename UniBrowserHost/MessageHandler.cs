using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniBrowserHost.Model;
using System.Diagnostics;
using ThreadState = System.Threading.ThreadState;

namespace UniBrowserHost
{
    public class MessageHandler
    {
        public event Action<NativeMessage> OnRequest;
        public event Action<string, string> OnReturn;

        public event Action<NativeMessage> OnMessage;

        private Thread _listenThread;
        private AutoResetEvent _autoReset = null;
        private const int CodeVersion = 7243;
        private static readonly string[] IgnoreList = { "GetHostHandle", "GetBrowserHandle", "" };

        private static readonly List<int> _windows = new List<int>();
        private static int _lastWindow;
        private static IntPtr Handle;
        private static Process BrowserProcess;
        private static IntPtr BrowserHandle;

        private bool _requesting;
        private object _lockObj = new object();

        private readonly JsonSerializerSettings _jsonSetting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };
        public MessageHandler()
        {
            Handle = Process.GetCurrentProcess().Handle;
            BrowserProcess = Process.GetCurrentProcess().Parent();
            if (BrowserProcess.ProcessName != "firefox")
            {
                BrowserProcess = BrowserProcess.Parent();
            }
            BrowserProcess.EnableRaisingEvents = true;
            BrowserHandle = BrowserProcess.MainWindowHandle;
        }

        private void BrowserExited(object sender, EventArgs e)
        {
            _autoReset?.Set();
        }

        public void WaitForExit()
        {
            _autoReset = new AutoResetEvent(false);
            _autoReset.WaitOne();
            try
            {
                if (_listenThread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    _listenThread.Interrupt();
                }
                _listenThread.Abort();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
        public void Init()
        {
            BrowserProcess.Exited += BrowserExited;
            try
            {
                RegisterNewlyCreatedWindowId();
                do
                {
                    OnProductVersionChanged();
                }
                while (!IsTraceEnabled());
            }
            catch (Exception e)
            {
                return;
            }
            

            #region 收发消息
            _listenThread = new Thread(start: async () =>
            {
                var pipeName = BrowserProcess.ProcessName == "chrome" ? "NativeHost-Chrome" 
                    : BrowserProcess.ProcessName == "firefox"? "NativeHost-Firefox":"";
                if (string.IsNullOrEmpty(pipeName))
                {
                    return;
                }
                using (var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 4, PipeTransmissionMode.Message, PipeOptions.WriteThrough | PipeOptions.Asynchronous))
                {
                    while (true)
                    {
                        try
                        {
                            //pipeServer.WaitForConnection();
                            await pipeServer.WaitForConnectionAsync();
                            _requesting = true;
                            //pipeServer.ReadMode = PipeTransmissionMode.Byte;
                            StreamWriter writer = new StreamWriter(pipeServer);
                            StreamReader reader = new StreamReader(pipeServer);
                            var input = reader.ReadLine();
                            if (string.IsNullOrEmpty(input))
                            {
                                continue;
                            }

                            var message = JsonConvert.DeserializeObject<NativeMessage>(input);
                            if (message==null)
                            {
                                continue;
                            }
                            await Task.Run(() => { OnRequest?.Invoke(message); });
                            if (FilterRequest(message, writer))
                            {
                                continue;
                            }

                            message.CodeVersion = CodeVersion;
                            message.RequestId = NativeMessage.CurrentRequestId;
                            input = JsonConvert.SerializeObject(message, _jsonSetting);

                            SendNativeMessage(input);

                            var result = ReadNativeMessage();
                            result = FilterReturn(result);
                            _requesting = false;
                            await Task.Run(() => { OnReturn?.Invoke(input, result); });

                            writer.WriteLine(result);
                            writer.Flush();

                        }
                        catch (Exception e)
                        {
                            //txtBox_display.AppendText(e.Message + "\r\n");
                            continue;
                        }
                        finally
                        {
                            pipeServer.Disconnect();
                        }
                    }

                }
            })
            {
                IsBackground = true
            };
            _listenThread.Start();
            #endregion
        }

        private string FilterReturn(string result)
        {
            if (string.IsNullOrEmpty(result))
            {
                return "";
            }
            var message = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (string.IsNullOrEmpty(message?.FunctionCall))
            {
                return result;
            }
            switch (message.FunctionCall)
            {
                case "":
                    _lastWindow = message.WindowId ?? 0;
                    if (!_windows.Contains(_lastWindow))
                    {
                        _windows.Add(_lastWindow);
                    }
                    break;
                default:
                    break;
            }

            return FilterReturn(ReadNativeMessage());
        }

        private bool FilterRequest(NativeMessage message, StreamWriter writer)
        {
            if (!IgnoreList.Contains(message.FunctionCall))
            {
                return false;
            }

            switch (message.FunctionCall)
            {
                case "GetHostHandle":
                    writer.Write(Handle.ToInt32());
                    writer.Flush();
                    break;
                case "GetBrowserHandle":
                    writer.Write(BrowserHandle.ToInt32());
                    writer.Flush();
                    break;
                default:
                    break;
            }
            return true;
        }

        #region 插件初始化方法
        private void RegisterNewlyCreatedWindowId()
        {
            var msg = ReadNativeMessage();
            if (string.IsNullOrEmpty(msg))
            {
                return;
            }
            var message = JsonConvert.DeserializeObject<NativeMessage>(msg);

            if (message.FunctionCall == nameof(RegisterNewlyCreatedWindowId))
            {
                _lastWindow = message.WindowId ?? 0;
                if (!_windows.Contains(_lastWindow))
                {
                    _windows.Add(_lastWindow);
                }
            }
        }
        private void OnProductVersionChanged()
        {
            var message = new NativeMessage
            {
                FunctionCall = "OnProductVersionChanged",
                RequestId = NativeMessage.CurrentRequestId,
                WindowId = 1
            };

            SendNativeMessage(JsonConvert.SerializeObject(message, _jsonSetting));
            LoadScripts();
        }
        private void LoadScripts()
        {
            var msg = ReadNativeMessage();
            if (string.IsNullOrEmpty(msg))
            {
                return;
            }
            var message = JsonConvert.DeserializeObject<NativeMessage>(msg);
            if (message.FunctionCall != nameof(LoadScripts)) return;
            var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "Scripts", "*.js");
            Dictionary<string, object> scriptDic =
                files.ToDictionary<string, string, object>(file => file.Split('\\').Last(), File.ReadAllText); //key-js脚本文件名，value-脚本内容
            scriptDic.Add("returnId", 1);
            scriptDic.Add("codeVersion", CodeVersion);
            scriptDic.Add("version", CodeVersion);
            scriptDic.Add("content", scriptDic["content.js"]);
            scriptDic.Remove("content.js");
            scriptDic.Add("background", scriptDic["background.js"]);
            scriptDic.Remove("background.js");

            SendNativeMessage(JsonConvert.SerializeObject(scriptDic));
        }
        private bool IsTraceEnabled()
        {
            var message = JsonConvert.DeserializeObject<NativeMessage>(ReadNativeMessage());
            if (message.FunctionCall != nameof(IsTraceEnabled)) return false;

            NativeMessage response = new NativeMessage
            {
                IsTraceEnabled = true,
                ReturnId = message.RequestId,
                RequestId = message.RequestId,
            };
            SendNativeMessage(JsonConvert.SerializeObject(response, _jsonSetting));

            var result = JsonConvert.DeserializeObject<NativeMessage>(ReadNativeMessage());
            if (result.ReturnId == response.RequestId)
            {
                if (result.CodeVersion < CodeVersion)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
        #region 与插件交互
        /// <summary>
        /// 向Chrome插件发送消息
        /// </summary>
        /// <param name="str"></param>
        private static void SendNativeMessage(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            //var length = str.Length;
            var length = Encoding.UTF8.GetByteCount(str);

            using (var stdout = Console.OpenStandardOutput())
            {
                stdout.Write(BitConverter.GetBytes(length), 0, 4);
                stdout.Write(Encoding.UTF8.GetBytes(str), 0, length);
                stdout.Flush();
            }
        }
        /// <summary>
        /// 读取Message
        /// </summary>
        /// <returns></returns>
        private static string ReadNativeMessage()
        {
            string input = "";
            using (var messageStream = Console.OpenStandardInput())
            {
                byte[] bytes = new byte[4];
                messageStream.Read(bytes, 0, 4);
                var length = BitConverter.ToInt32(bytes, 0);

                var messageBytes = new byte[length];
                messageStream.Read(messageBytes, 0, length);
                input = Encoding.UTF8.GetString(messageBytes);
            }
            return input;
        }
        #endregion
    }
}
