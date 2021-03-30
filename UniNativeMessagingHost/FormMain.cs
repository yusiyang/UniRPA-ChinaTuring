using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using UniNativeMessagingHost.Model;

namespace UniNativeMessagingHost
{
    public partial class FormMain : Form
    {
        private const int CodeVersion = 7243;
        private Thread _listenThread;

        private readonly JsonSerializerSettings _jsonSetting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        };
        private int _currentWindowId;

        public FormMain()
        {
            KillOther();
            InitializeComponent();
            this.WindowState = WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            ListenMessage();

            RegisterNewlyCreatedWindowId();
            do
            {
                OnProductVersionChanged();
            }
            while (!IsTraceEnabled());
        }



        private void KillOther()
        {
            var currentProcess = Process.GetCurrentProcess();
            var runningProcesses = Process.GetProcessesByName(currentProcess.ProcessName);
            foreach (var process in runningProcesses)
            {
                if (process.Id == currentProcess.Id)
                {
                    continue;
                }
                process.Kill();
            }
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            //SendNativeMessage(GetActiveTabId());
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _listenThread?.Abort();
            this.Dispose(true);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //SendNativeMessage(GetWindowIdForTabId());
        }

        #region 主动调用插件的方法

        //private string GetActiveTabId(NativeMessage ret = null)
        //{
        //    if (ret?.ReturnId != null)
        //    {
        //        int.TryParse(ret.TabId.ToString(), out _currentTabId);
        //        return "";
        //    }

        //    var message = new NativeMessage
        //    {
        //        CodeVersion = CodeVersion,
        //        WindowId = 1,
        //        FunctionCall = nameof(GetActiveTabId),
        //        RequestId = NativeMessage.CurrentRequestId,
        //    };

        //    _funcReturnDic.Add((int)message.RequestId, message.FunctionCall);

        //    return JsonConvert.SerializeObject(message, _jsonSetting);
        //}

        //private string GetWindowIdForTabId(NativeMessage ret = null)
        //{
        //    if (ret?.ReturnId != null)
        //    {
        //        _currentWindowId = ret.WindowId ?? 1;

        //        return "";
        //    }

        //    if (_currentTabId == 0)
        //    {
        //        return "";
        //    }
        //    var message = new NativeMessage
        //    {
        //        CodeVersion = CodeVersion,
        //        TabId = _currentTabId,
        //        FunctionCall = nameof(GetWindowIdForTabId),
        //        RequestId = NativeMessage.CurrentRequestId,
        //    };
        //    _funcReturnDic.Add((int)message.RequestId, message.FunctionCall);

        //    return JsonConvert.SerializeObject(message, _jsonSetting);
        //}

        //private string GetHtmlFromPoint(NativeMessage message)
        //{
        //    if (message.ReturnId != null)
        //    {
        //        if (message.RetCode == 0)
        //        {
        //            _currentCustomId = "";
        //            return "";
        //        }
        //        _currentCustomId = message.CustomId.Remove(0, 2);
        //        return "";
        //    }

        //    if (_currentWindowId == 0)
        //    {
        //        return "";
        //    }

        //    message.CodeVersion = CodeVersion;
        //    message.RequestId = NativeMessage.CurrentRequestId;
        //    message.WindowId = _currentWindowId;

        //    _funcReturnDic.Add((int)message.RequestId, message.FunctionCall);
        //    return JsonConvert.SerializeObject(message, _jsonSetting);
        //}

        //private string GetHtmlRectangle(NativeMessage message)
        //{
        //    message.CodeVersion = CodeVersion;
        //    message.RequestId = NativeMessage.CurrentRequestId;
        //    message.TabId = _currentTabId;
        //    message.WindowId = _currentWindowId;
        //    message.CustomId = _currentCustomId;

        //    _funcReturnDic.Add((int)message.RequestId, message.FunctionCall);
        //    var messageStr = JsonConvert.SerializeObject(message, _jsonSetting);
        //    SendNativeMessage(messageStr);

        //    return ReadNativeMessage();
        //}

        //private string GetHtmlAttribute(NativeMessage message)
        //{
        //    if (message.ReturnId != null)
        //    {
        //        return "";
        //    }
        //    message.CodeVersion = CodeVersion;
        //    message.RequestId = NativeMessage.CurrentRequestId;

        //    _funcReturnDic.Add((int)message.RequestId, message.FunctionCall);
        //    return JsonConvert.SerializeObject(message, _jsonSetting);
        //}

        //private string GetHtmlAncestryInfo(NativeMessage message)
        //{
        //    if (message.ReturnId != null)
        //    {

        //        return "";
        //    }
        //    message.CodeVersion = CodeVersion;
        //    message.RequestId = NativeMessage.CurrentRequestId;

        //    _funcReturnDic.Add((int)message.RequestId, message.FunctionCall);
        //    return JsonConvert.SerializeObject(message, _jsonSetting);
        //}


        #endregion

        #region 插件初始化方法
        private void RegisterNewlyCreatedWindowId()
        {
            var message = JsonConvert.DeserializeObject<NativeMessage>(ReadNativeMessage());

            if (message.FunctionCall == nameof(RegisterNewlyCreatedWindowId))
            {
                _currentWindowId = message.WindowId ?? 0;
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
            var message = JsonConvert.DeserializeObject<NativeMessage>(ReadNativeMessage());
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
                stdout.Write(Encoding.UTF8.GetBytes(str),0,length);
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


        private void ListenMessage()
        {
            _listenThread = new Thread(() =>
            {
                using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("NativeHost", PipeDirection.InOut, 1))
                {
                    while (true)
                    {
                        try
                        {
                            
                            pipeServer.WaitForConnection();
                            pipeServer.ReadMode = PipeTransmissionMode.Byte;
                            StreamWriter writer = new StreamWriter(pipeServer);
                            StreamReader reader = new StreamReader(pipeServer);
                            var input = reader.ReadLine();
                            if (string.IsNullOrEmpty(input))
                            {
                                pipeServer.Disconnect();
                                continue;
                            }

                            //txtBox_display.AppendText(input + "\r\n");

                            var message = JsonConvert.DeserializeObject<NativeMessage>(input);

                            message.CodeVersion = CodeVersion;
                            message.RequestId = NativeMessage.CurrentRequestId;
                            input = JsonConvert.SerializeObject(message, _jsonSetting);

                            SendNativeMessage(input);

                            var result = ReadNativeMessage();

                            //txtBox_display.AppendText(result+"\r\n");

                            writer.WriteLine(result);
                            writer.Flush();
                        }
                        catch (Exception e)
                        {
                            pipeServer.Disconnect();
                            //txtBox_display.AppendText(e.Message + "\r\n");
                            continue;
                        }
                        pipeServer.Disconnect();

                    }
                }
            })
            {
                IsBackground = true
            };
            _listenThread.Start();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            //var message = new NativeMessage
            //{
            //    CodeVersion = 7243,
            //    FunctionCall = nameof(GetHtmlFromPoint),
            //    PageRenderOfsX = 8,
            //    PageRenderOfsY = 111,

            //    ScreenX = 1057,
            //    ScreenY = 564,
            //    WindowId = _currentWindowId,
            //    WindowLeft = -8,
            //    WindowTop = -8
            //};
            //SendNativeMessage(GetHtmlFromPoint(message));



            //var message = new NativeMessage
            //{
            //    CodeVersion = 7243,
            //    FunctionCall = nameof(GetHtmlAttribute),
            //    TabId = _currentTabId,
            //    CustomId = "0|1",
            //    AttrName = "tag"
            //};

            //SendNativeMessage(GetHtmlAttribute(message));

            //var message = new NativeMessage
            //{
            //    CodeVersion = CodeVersion,
            //    FunctionCall = nameof(GetHtmlAncestryInfo),
            //    TabId = _currentTabId,
            //    CustomId = _currentCustomId,
            //    GetFlags = 4
            //};
            //SendNativeMessage(GetHtmlAncestryInfo(message));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //    var message = new NativeMessage
            //    {
            //        CodeVersion = 7243,
            //        FunctionCall = nameof(GetHtmlRectangle),
            //        CustomId = _currentCustomId,

            //        TabId = _currentTabId,
            //        WindowId = _currentWindowId,

            //        PageRenderOfsX = 8,
            //        PageRenderOfsY = 111,

            //        ScreenX = 1057,
            //        ScreenY = 564,

            //        WindowLeft = -8,
            //        WindowTop = -8,
            //        UseClientCoordinates = 0
            //    };
            //    SendNativeMessage(GetHtmlRectangle(message));
        }

        private void txtBox_display_TextChanged(object sender, EventArgs e)
        {
            this.txtBox_display.SelectionStart = this.txtBox_display.Text.Length;
            this.txtBox_display.SelectionLength = 0;
            this.txtBox_display.ScrollToCaret();
        }
    }
}
