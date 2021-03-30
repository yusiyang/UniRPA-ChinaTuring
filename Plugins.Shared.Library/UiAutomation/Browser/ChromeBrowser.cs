using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlaUI.UIA3;
using Newtonsoft.Json;

namespace Plugins.Shared.Library.UiAutomation.Browser
{
    public sealed class ChromeBrowser : IBrowser
    {
        private static readonly MessageClient Client = new MessageClient("NativeHost-Chrome");

        public static int TabCount => GetTabCount();

        public ChromeBrowser(UiNode node)
        {
            if (!Equals(node?.Parent?.WindowHandle, Client.BrowserHandle))
            {
                Available = false;
                return;
            }
            TabId = GetActiveTabId();
            UiNode = node;
            Available = true;
        }

        public ChromeBrowser()
        {
            if (Client.BrowserHandle.Equals(IntPtr.Zero))
            {
                Available = false;
                return;
            }

            TabId = GetActiveTabId();
            var panel = UIAUiNode.UIAAutomation.FromHandle(Client.BrowserHandle)
                .FindFirstChild(t => t.ByClassName("Chrome_RenderWidgetHostHWND"));
            if (panel != null)
            {
                UiNode = new UIAUiNode(panel);
            }

            Available = true;
        }

        public ChromeBrowser(string title)
        {
            var tabId = GetHtmlElemById(-1, "chrome.exe", title, null, null);
            if (int.TryParse(tabId, out var id))
            {
                TabId = id;

                var panel = UIAUiNode.UIAAutomation.FromHandle(Client.BrowserHandle)
                    .FindFirstChild(t => t.ByClassName("Chrome_RenderWidgetHostHWND"));
                if (panel != null)
                {
                    UiNode = new UIAUiNode(panel);
                }

                Available = true;
            }
            else
            {
                Available = false;
            }
        }

        public string Title => GetHtmlAttribute("pagetitle");
        public BrowserType Type => BrowserType.Chrome;
        public string Name => BrowserType.Chrome.ToString();
        public UiNode UiNode { get; private set; }
        public IntPtr Hwd => UiNode?.WindowHandle ?? Client.BrowserHandle;
        public bool Available { get; private set; }
        public bool Active => IsBrowserTabActive();
        public ReadyState ReadyState
        {
            get
            {
                var state = GetHtmlAttribute("readystate");
                if (state == "1")
                {
                    return ReadyState.Complete;
                }

                return ReadyState.Interactive;
            }
        }

        public int TabId { get; private set; }

        public int WindowId => GetWindowIdForTabId();

        public int Ratio => GetHtmlDevicePixelRatio();
        public void Navigate(Uri uri)
        {
            if (uri == null) return;

            var message = new NativeMessage
            {
                FunctionCall = nameof(Navigate),
                TabId = TabId,
                Url = uri.ToString()
            };
            Client.Message(message);
        }

        public void Close()
        {
            var message = new NativeMessage
            {
                FunctionCall = "CloseBrowser",
                TabId = TabId
            };
            var result = Client.Message(message);
        }

        public void Refresh()
        {
            NavigationCommand("reload");
        }

        public void Activate()
        {
            NavigationCommand("activate", TabId);
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void GoHome()
        {
            NavigationCommand("home");
        }

        public void GoForward()
        {
            NavigationCommand("forward");
        }

        public void GoBack()
        {
            NavigationCommand("back");
        }

        public void Open(Uri uri, string arguments, int timeOut = 30000)
        {
            var oldTabId = TabId;
            Process.Start("chrome.exe", $"{arguments} {uri}");
            var autoSet = new AutoResetEvent(false);
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;
            var task = new Task(() =>
            {
                while (Client.BrowserHandle == IntPtr.Zero || GetActiveTabId() == oldTabId)
                {
                    Thread.Sleep(50);
                }
                UiNode = new UIAUiNode(UIAUiNode.UIAAutomation.FromHandle(Client.BrowserHandle).FindFirstChild(t => t.ByClassName("Chrome_RenderWidgetHostHWND")));
                TabId = GetActiveTabId();
                while (!IsBrowserTabActive())
                {
                    Thread.Sleep(50);
                }
                Available = true;
                autoSet.Set();
            }, token);
            task.Start();
            autoSet.WaitOne(timeOut);
            tokenSource.Cancel();
        }

        public void WaitPage(int timeOut = 1000)
        {
            while (TabId < 0 && timeOut > 0)
            {
                timeOut -= 500;
                Thread.Sleep(500);
            }
        }
        public object ExecuteScript(string script, params object[] args)
        {
            var arg = "";
            if (args.Any(t => t is string))
            {
                arg = args.FirstOrDefault(t => t is string)?.ToString();
            }

            var target = args.FirstOrDefault(t => t is HtmlUiNode) as HtmlUiNode;
            return InjectAndRunJs(script, arg, target);
        }
        /// <summary>
        /// 注入并执行JS
        /// </summary>
        /// <param name="jsCode">Js代码</param>
        /// <param name="param">参数</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public string InjectAndRunJs(string jsCode, string param, UiNode target)
        {
            if (target != null && !(target is HtmlUiNode))
            {
                return null;
            }

            var node = (HtmlUiNode)target;
            var message = new NativeMessage
            {
                FunctionCall = nameof(InjectAndRunJs),
                TabId = node?.TabId ?? TabId,
                CustomId = node?.CustomId ?? "",
                JsCode = jsCode,
                Input = param
            };
            var result = Client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            return ret?.Result;
        }
        /// <summary>
        /// 获取元素属性值
        /// </summary>
        /// <param name="attrName"></param>
        /// <param name="tabId"></param>
        /// <param name="customId"></param>
        /// <returns></returns>
        public string GetHtmlAttribute(string attrName, int? tabId = null, string customId = null)
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(GetHtmlAttribute),
                TabId = tabId ?? TabId,
                CustomId = customId ?? "",
                AttrName = attrName
            };

            var result = Client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return ret.AttrValue;
            }
            return string.Empty;
        }

        /// <summary>
        /// 在页面上查找元素
        /// </summary>
        /// <param name="attrDic"></param>
        /// <returns></returns>
        public UiNode FindHtmlNode(Dictionary<string, string> attrDic)
        {
            string[] exceptName = { "Name", "AutomationId", "UserDefineId", "ClassName", "tag", "Description", "ProcessName", "Idx", "index", "ParentCustomId" };
            //var customId = GetHtmlElemById(-1, attrDic["ProcessName"], attrDic["Name"], null, null);
            //if (string.IsNullOrEmpty(customId))
            //{
            //    return null;
            //}
            //var tabId = int.Parse(customId);
            //NavigationCommand("activate", tabId);

            var needAttrDic = attrDic.Where(t => !exceptName.Contains(t.Key)).ToDictionary(key => key.Key, value => value.Value);
            var customId = GetHtmlElemById(TabId, "chrome.exe", Title, needAttrDic, attrDic["tag"], int.Parse(attrDic["index"]), attrDic["ParentCustomId"]);
            if (string.IsNullOrEmpty(customId))
            {
                return null;
            }
            var ids = customId.Split('|');
            return new HtmlUiNode(int.Parse(ids[0]), $"{ids[1]}|{ids[2]}", this);
        }

        public UiNode FindHtmlNode(string title, string selector)
        {
            return null;
        }

        public UiNode GetElementFromPoint(Point screenPoint)
        {
            var message = new NativeMessage
            {
                FunctionCall = "GetHtmlFromPoint",
                PageRenderOfsX = UiNode.BoundingRectangle.X,
                PageRenderOfsY = UiNode.BoundingRectangle.Y,
                ScreenX = screenPoint.X,
                ScreenY = screenPoint.Y,

                WindowLeft = -8,
                WindowTop = -8,
            };
            var result = Client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            var customId = "";
            if (ret?.RetCode == 1)
            {
                var ids = ret.CustomId.Split('|').ToList();
                ids.RemoveAt(0);
                customId = string.Join("|", ids);
            }
            return new HtmlUiNode(TabId, customId, this);
        }
        #region Private

        private bool IsBrowserTabActive()
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(IsBrowserTabActive),
                TabId = TabId
            };
            var result = Client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            return ret.RetCode == 1;
        }
        private int GetWindowIdForTabId()
        {
            var message = new NativeMessage
            {
                FunctionCall = "GetWindowIdForTabId",
                TabId = TabId
            };
            var result = Client.Message(message);
            var id = JsonConvert.DeserializeObject<NativeMessage>(result).WindowId;
            return id ?? 0;
        }
        private int GetActiveTabId()
        {
            var message = new NativeMessage
            {
                FunctionCall = "GetActiveTabId"
            };
            var result = Client.Message(message);
            result = JsonConvert.DeserializeObject<NativeMessage>(result).TabId.ToString();
            return int.TryParse(result, out int tabId) ? tabId : 0;
        }

        private static int GetTabCount()
        {
            var message = new NativeMessage
            {
                FunctionCall = "GetTabCount"
            };
            var result = Client.Message(message);
            var tabCount = JsonConvert.DeserializeObject<NativeMessage>(result).TabCount;
            return tabCount ?? 0;
        }

        /// <summary>
        /// 在页面中搜索元素
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="processName"></param>
        /// <param name="title"></param>
        /// <param name="attrDic"></param>
        /// <param name="tag"></param>
        /// <param name="index"></param>
        /// <param name="parentCustomId"></param>
        /// <returns></returns>
        private string GetHtmlElemById(int tabId, string processName, string title, Dictionary<string, string> attrDic, string tag, int? index = null, string parentCustomId = "")
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(GetHtmlElemById),
                ParentCustomId = "",
                TabId = tabId,
                Index = 1
            };
            if (tabId == -1)
            {
                message.AttrMap = new Dictionary<string, Map>
                {
                    {"instanceaffinity", new Map("1") },
                    {"processname",new Map(processName) },
                    {"title",new Map(title) }
                };
                message.TagName = new Map();
            }
            else
            {
                message.Index = index == 0 ? 1 : index;
                message.AttrMap = new Dictionary<string, Map>();
                foreach (var attr in attrDic)
                {
                    message.AttrMap.Add(attr.Key, new Map(attr.Value));
                }
                message.TagName = new Map(tag);
                message.ParentCustomId = parentCustomId ?? "";
            }

            var result = Client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return ret.CustomId;
            }

            return "";
        }
        private bool NavigationCommand(string command, int? tabId = null)
        {
            if (tabId < 0)
            {
                return false;
            }
            var message = new NativeMessage
            {
                FunctionCall = nameof(NavigationCommand),
                TabId = tabId ?? TabId,
                Command = command
            };
            var result = Client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 获取缩放值
        /// </summary>
        /// <param name="tabId"></param>
        /// <returns></returns>
        private int GetHtmlDevicePixelRatio()
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(GetHtmlDevicePixelRatio),
                TabId = TabId,
            };
            var result = Client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return ret.DevicePixelRatioPercentage ?? 100;
            }

            return 100;
        }
        #endregion
    }
}
