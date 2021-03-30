using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.Core.Input;
using FlaUI.Core.Overlay;
using FlaUI.UIA2;
using Newtonsoft.Json;
using Plugins.Shared.Library.UiAutomation.Browser;

namespace Plugins.Shared.Library.UiAutomation
{
    class HtmlUiNode : UiNode,IJavaScriptExecutor
    {
        public List<NodeHierarchy> NodeHierarchyInfo => GetHtmlAncestryInfo();

        internal readonly IBrowser _browser;
        private readonly MessageClient _client;
        private int WindowId => GetWindowIdForTabId();

        public HtmlUiNode(int tabId, string customId, IBrowser browser)
        {
            TabId = tabId;
            CustomId = customId;
            _browser = browser;
            _client = new MessageClient($"NativeHost-{_browser.Type.ToString()}");

            Name = GetText().Trim();
            Title = GetHtmlAttribute("pagetitle");
            Role = GetHtmlAttribute("tag");
            ClassName = GetHtmlAttribute("class");
        }
        public string Title { get; }
        public UiNode Panel
        {
            get
            {
                if (_browser.UiNode != null)
                {
                    return _browser.UiNode;
                }

                var panel = UIAUiNode.UIAAutomation.FromHandle(_browser.Hwd);
                if (panel == null)
                {
                    return null;
                }
                return new UIAUiNode(panel);
            }
        }

        public string UserDefineId { get; set; }
        public string CustomId { get; }
        public int TabId { get; }
        public string Idx { get; set; }

        public string AutomationId => Panel?.AutomationId;

        public string ClassName { get; }

        public string ControlType => "HtmlNode";

        public string Name { get; set; }

        public string Role { get; }
        public string Description { get; set; }

        public string ProcessName => Panel.ProcessName;

        public string ProcessFullPath => Panel.ProcessFullPath;

        public IntPtr WindowHandle => Panel.WindowHandle;

        public UiNode Parent
        {
            get
            {
                if (string.Equals(Role, "body", StringComparison.OrdinalIgnoreCase))
                {
                    return Panel;
                }
                var parentCustomId = GetHtmlAttribute("parentcustomid");
                if (string.IsNullOrEmpty(parentCustomId))
                {
                    return null;
                }

                var ids = parentCustomId.Split('|');
                return new HtmlUiNode(TabId, ids[1] + "|" + ids[2], _browser);
            }
        }

        public UiNode AutomationElementParent => Panel;

        public Rectangle BoundingRectangle => IsHtmlElemValid() ? GetHtmlRectangle(TabId, CustomId) : Rectangle.Empty;

        public List<UiNode> Children => new List<UiNode>();

        public bool IsTopLevelWindow => Panel?.IsTopLevelWindow ?? false;

        public FrameworkAutomationElementBase.IFrameworkPatterns Patterns => Panel.Patterns;

        public void MouseClick(UiElementClickParams clickParams = null)
        {
            if (clickParams == null)
            {
                clickParams = new UiElementClickParams();
            }

            if (clickParams.mouseActionType == MouseActionType.Simulate)
            {
                SimulateClick();
                return;
            }
            if (clickParams.isMoveMouse)
            {
                Mouse.MoveTo(GetClickablePoint());
            }
            else
            {
                Mouse.Position = GetClickablePoint();
            }

            Mouse.LeftClick();
        }


        public void MouseHover(UiElementHoverParams hoverParams = null)
        {
            if (hoverParams == null)
            {
                hoverParams = new UiElementHoverParams();
            }

            if (hoverParams.isMoveMouse)
            {
                Mouse.MoveTo(GetClickablePoint());
            }
            else
            {
                Mouse.Position = GetClickablePoint();
            }
        }

        public void Focus()
        {
            _browser.Activate();
        }

        public void SetForeground()
        {
            Panel?.SetForeground();
        }

        public Point GetClickablePoint()
        {
            var point = new Point(BoundingRectangle.Left + BoundingRectangle.Width / 2, BoundingRectangle.Top + BoundingRectangle.Height / 2);
            return point;
        }

        public UiNode GetChildByIdx(int idx)
        {
            return Children[idx];
        }

        public List<UiNode> FindAll(FlaUI.Core.Definitions.TreeScope scope, ConditionBase condition)
        {
            return new List<UiNode>();
        }

        public void SimulateClick()
        {
            ClickHtmlElem();
        }

        public void SimulateTypeText(string text)
        {
            WriteTextHtmlElem(text);
        }

        public void HighLight(Color? color = null, TimeSpan? duration = null, bool blocking = false)
        {
            SetForeground();
            Focus();
            var colorName = color ?? Color.Red;
            var rectangle = this.BoundingRectangle;
            if (!rectangle.IsEmpty)
            {
                var durationInMs = (int)(duration ?? TimeSpan.FromSeconds(2)).TotalMilliseconds;

                var overlayManager = new WinFormsOverlayManager();
                if (blocking)
                {
                    overlayManager.ShowBlocking(rectangle, colorName, durationInMs);
                }
                else
                {
                    overlayManager.Show(rectangle, colorName, durationInMs);
                }
            }
            else
            {
                throw new Exception();
            }
        }

        public bool IsChecked()
        {
            return GetHtmlAttribute("checked") == "1";
        }

        public void Check()
        {
            CheckHtmlElem(true);
        }

        public void UnCheck()
        {
            CheckHtmlElem(false);
        }

        public void Toggle()
        {
            CheckHtmlElem(!IsChecked());
        }

        public string GetText()
        {
            return GetHtmlAttribute("aaname");
        }

        public void SetText(string text)
        {
            WriteTextHtmlElem(text);
        }

        public bool IsPassword()
        {
            throw new NotImplementedException();
        }

        public void ClearSelection()
        {
            HtmlSelectedItems(new List<string>().ToArray());
        }

        public void SelectItem(string item)
        {
            HtmlSelectedItems(new[] { item });
        }

        public void SelectMultiItems(string[] items)
        {
            HtmlSelectedItems(items);
        }

        public void ClearText()
        {
            SetText("");
        }

        public object GetAttributeValue(string attributeName)
        {
            return GetHtmlAttribute(attributeName);
        }

        public bool IsEnable()
        {
            return GetAttributeValue("readystate").ToString() == "1";
        }

        public bool IsVisible()
        {
            var browserHwnd = _client.BrowserHandle;
            if (!Win32Api.IsWindowVisible(browserHwnd))
            {
                return false;
            }
            if (Win32Api.IsOnTop(browserHwnd))
            {
                return _browser.Active && IsHtmlElemValid();
            }
            var browserVisible=false;
            Librarys.TimeoutHelper timeHelper = new Librarys.TimeoutHelper
            {
                Do = () =>
                {
                    var browserWindow = UIAUiNode.UIAAutomation.FromHandle(browserHwnd);
                    if (!browserWindow.IsOffscreen && browserWindow.TryGetClickablePoint(out _))
                    {
                        browserVisible = true;
                    }
                }
            };
            if (timeHelper.DoWithTimeout(TimeSpan.FromSeconds(5)) == false)
            {
                return browserVisible && _browser.Active && IsHtmlElemValid();
            }
            else
            {
                return false;
            }
        }

        public bool IsTable()
        {
            return string.Equals(Role, "table", StringComparison.OrdinalIgnoreCase);
        }

        public void SetAttribute(string attrName, string attrValue)
        {
            var message = new NativeMessage
            {
                FunctionCall = "SetHtmlAttribute",
                TabId = TabId,
                CustomId = CustomId,
                AttrName = attrName,
                AttrValue = attrValue
            };
            var result = _client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
        }
        #region Private

        private string[] HtmlSelectedItems(string[] items, bool getAll = false)
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(HtmlSelectedItems),
                TabId = TabId,
                CustomId = CustomId,
                ItemsToSelect = items.Any() ? items.ToList() : null
            };
            if (getAll)
            {
                message.GetAllItems = 1;
            }
            var result = _client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret.RetCode == 1)
            {
                return ret.SelectedItems.ToArray();
            }
            return new List<string>().ToArray();
        }
        private void SetFocusedHtmlElement()
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(SetFocusedHtmlElement),
                TabId = TabId,
                CustomId = CustomId,
            };
            var result = _client.Message(message);
        }
        private void CheckHtmlElem(bool check)
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(CheckHtmlElem),
                TabId = TabId,
                CustomId = CustomId,
                DoCheck = check ? 1 : 0
            };
            var result = _client.Message(message);
        }
        private void ClickHtmlElem()
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(ClickHtmlElem),
                TabId = TabId,
                CustomId = CustomId,
            };
            var result = _client.Message(message);
        }
        /// <summary>
        /// 获取元素的查找信息
        /// </summary>
        /// <returns></returns>
        private List<NodeHierarchy> GetHtmlAncestryInfo()
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(GetHtmlAncestryInfo),
                TabId = TabId,
                CustomId = CustomId,
                GetFlags = 4
            };

            var result = _client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return ret.NodeHierarchyInfo;
            }
            return new List<NodeHierarchy>();
        }
        /// <summary>
        /// 获取元素属性值
        /// </summary>
        /// <param name="attrName"></param>
        /// <returns></returns>
        private string GetHtmlAttribute(string attrName)
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(GetHtmlAttribute),
                TabId = TabId,
                CustomId = CustomId,
                AttrName = attrName
            };

            var result = _client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return ret.AttrValue;
            }
            return string.Empty;
        }
        /// <summary>
        /// 模拟键入
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool WriteTextHtmlElem(string text)
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(WriteTextHtmlElem),
                TabId = TabId,
                CustomId = CustomId,
                Text = text
            };
            var result = _client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 元素是否可用
        /// </summary>
        /// <param name="tabId"></param>
        /// <param name="customId"></param>
        /// <returns></returns>
        public bool IsHtmlElemValid()
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(IsHtmlElemValid),
                TabId = TabId,
                CustomId = CustomId
            };
            var result = _client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// 获取元素矩形
        /// </summary>
        /// <param name="parentRect"></param>
        /// <param name="tabId"></param>
        /// <param name="customId"></param>
        /// <returns></returns>
        public Rectangle GetHtmlRectangle(int? tabId, string customId = null)
        {
            //var scale = Win32Api.DpiX;
            //Point offset = new Point();
            //switch (scale)
            //{
            //    case 96:
            //        offset = new Point(8, 111);
            //        break;
            //    case 120:
            //        offset = new Point(9, 137);
            //        break;
            //    case 144:
            //        offset = new Point(11, 165);
            //        break;
            //    case 192:
            //        offset = new Point(13, 218);
            //        break;
            //}

            var offset = new Point(Panel.BoundingRectangle.Left - Panel.Parent.BoundingRectangle.Left,
                Panel.BoundingRectangle.Top - Panel.Parent.BoundingRectangle.Top);
            var message = new NativeMessage
            {
                FunctionCall = nameof(GetHtmlRectangle),
                PageRenderOfsX = offset.X,
                PageRenderOfsY = offset.Y,

                UseClientCoordinates = 0,

                WindowLeft = Panel.Parent.BoundingRectangle.Left,
                WindowTop = Panel.Parent.BoundingRectangle.Top,

                TabId = TabId,
                WindowId = WindowId,
                CustomId = customId ?? ""
            };
            var result = _client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                var width = ret.Right - ret.Left;
                var height = ret.Bottom - ret.Top;
                var rect = new Rectangle
                {
                    X = ret.Left ?? 0,
                    Y = ret.Top ?? 0,
                    Width = width ?? 0,
                    Height = height ?? 0
                };
                return rect;

            }
            return Rectangle.Empty;
        }

        /// <summary>
        /// 获取标签页所属窗体的Id
        /// </summary>
        /// <returns></returns>
        private int GetWindowIdForTabId()
        {
            var message = new NativeMessage
            {
                FunctionCall = nameof(GetWindowIdForTabId),
                TabId = TabId
            };

            var result = _client.Message(message);
            var ret = JsonConvert.DeserializeObject<NativeMessage>(result);
            if (ret?.RetCode == 1)
            {
                return ret.WindowId ?? 1;
            }

            return 0;
        }
        #endregion

        public object ExecuteScript(string script, params object[] args)
        {
            return _browser.ExecuteScript(script, args,this);
        }
    }
}
