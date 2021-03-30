using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FlaUI.Core.Overlay;
using MSHTML;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.UiAutomation.IEBrowser;

namespace Plugins.Shared.Library.UiAutomation
{
    class IeNode : UiNode, IJavaScriptExecutor
    {
        //internal readonly IEBrowser.Browser _browser;
        internal readonly IeBrowser _browser;
        internal readonly IHTMLElement _element;
        private Rectangle? _rectangle;

        public IeNode(IHTMLElement element, IeBrowser browser)
        {
            _browser = browser;
            _element = element;
        }

        public string CssSelector => _element.CssPath(true);
        public string UserDefineId { get; set; }

        public string Idx
        {
            get
            {
                var idx = "-1";
                var uniqueId = (_element as IHTMLUniqueName)?.uniqueID;
                if (_element.parentElement != null && !string.IsNullOrEmpty(uniqueId))
                {
                    IHTMLElementCollection children = (IHTMLElementCollection)_element.parentElement.children;
                    for (int i = 0; i < children.length; i++)
                    {
                        if (children.item(i) is IHTMLUniqueName id && id.uniqueID == uniqueId)
                        {
                            idx = $"{i}";
                            break;
                        }
                    }
                }

                return idx;
            }
        }

        public string AutomationId { get; set; }
        public string ClassName => _browser.UiNode.ClassName;
        public string ControlType => "IEHtmlNode";
        public string Name => GetAttributeValue("Name").ToString();
        public string Role => _element.tagName.ToLower(CultureInfo.CurrentCulture);
        public string Description { get; set; }
        public string Value
        {
            get
            {
                if (_element.tagName.ToLower(CultureInfo.CurrentCulture) == "input")
                {
                    var ele = (IHTMLInputElement)_element;
                    return ele.value;
                }

                return _element.innerText;
                // return null;
            }
            set
            {
                if (_element.tagName.ToLower(CultureInfo.CurrentCulture) == "input")
                {
                    var ele = (IHTMLInputElement)_element;
                    ele.value = value;
                }
                if (_element.tagName.ToLower(CultureInfo.CurrentCulture) == "select")
                {
                    dynamic ele = (IHTMLSelectElement)_element;
                    foreach (IHTMLOptionElement e in ele.options)
                    {
                        if (e.value == value)
                        {
                            ele.value = value;
                        }
                        else if (e.text == value)
                        {
                            ele.value = e.value;
                        }
                    }
                }
            }
        }

        public string ProcessName => _browser.UiNode.ProcessName;
        public string ProcessFullPath => _browser.UiNode.ProcessFullPath;
        public IntPtr WindowHandle => _browser.Hwd;

        public UiNode Parent
        {
            get
            {
                var pEle = _element.parentElement;
                if (pEle == null)
                {
                    return AutomationElementParent;
                }
                return new IeNode(pEle,_browser );
            }
        }

        public UiNode AutomationElementParent => _browser.UiNode;

        public Rectangle BoundingRectangle
        {
            get
            {
                if (_rectangle != null) return _rectangle.Value;

                _rectangle = Rectangle.Empty;
                if (!(_element is IHTMLElement2 ele)) return _rectangle.Value;
                var frameOffsetX = 0;
                var frameOffsetY = 0;
                //var col = ele.getClientRects();
                //if (col == null) return _rectangle.Value;
                try
                {
                    //dynamic _rect = col.item(0);
                    //var rect = ele.getBoundingClientRect();
                    //var left = rect.left;
                    //var top = rect.top;
                    //var right = rect.right;
                    //var bottom = rect.bottom;
                    //int elementx = left;
                    //int elementy = top;
                    //int elementw = right - left;
                    //int elementh = bottom - top;


                    //elementx += frameOffsetX;
                    //elementy += frameOffsetY;

                    //elementx += _browser.Panel.BoundingRectangle.X;
                    //elementy += _browser.Panel.BoundingRectangle.Y;
                    //_rectangle = new Rectangle(elementx, elementy, elementw, elementh);
                    //return _rectangle.Value;

                    var rect= ele.getBoundingClientRect();
                    //var rectangle = new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
                    var point = new Point(rect.left, rect.top);
                    Win32Api.ClientToScreen(_browser.PanelHwd, ref point);
                    _rectangle=new Rectangle(point,new Size(rect.right - rect.left, rect.bottom - rect.top));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return _rectangle.Value;
            }
        }
        public List<UiNode> Children
        {
            get
            {
                var result = new List<IeNode>();
                IHTMLElementCollection children = (IHTMLElementCollection)_element.children;
                foreach (IHTMLElement c in children)
                {
                    try
                    {
                        result.Add(new IeNode(c,_browser));
                    }
                    catch (Exception)
                    {
                    }
                }
                return result.Select(t => t.TryCast<UiNode>()).ToList();
            }
        }

        public bool IsTopLevelWindow => false;
        public FrameworkAutomationElementBase.IFrameworkPatterns Patterns => _browser.UiNode.Patterns;
        public List<UiNode> FindAll(TreeScope scope, ConditionBase condition)
        {
            var result = new List<IeNode>();
            IHTMLElementCollection all = (_browser.WebBrowser.Document as HTMLDocument)?.all;
            foreach (IHTMLElement c in all)
            {
                try
                {
                    result.Add(new IeNode(c,_browser));
                }
                catch (Exception)
                {
                }
            }
            return result.Select(t => t.TryCast<UiNode>()).ToList();
        }

        public void SetForeground()
        {
            _browser.UiNode.SetForeground();
        }

        public Point GetClickablePoint()
        {
            var point = new Point(BoundingRectangle.Left + BoundingRectangle.Width / 2, BoundingRectangle.Top + BoundingRectangle.Height / 2);
            return point;
        }

        public UiNode GetChildByIdx(int idx)
        {
            return null;
        }

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

            Focus();
            if (clickParams.isMoveMouse)
            {
                Mouse.MoveTo(GetClickablePoint());
            }
            else
            {
                Mouse.Position = GetClickablePoint();
            }

            Mouse.Click();
        }

        public void SimulateClick()
        {
            _element.click();
        }

        public void MouseHover(UiElementHoverParams hoverParams = null)
        {
            if (hoverParams == null)
            {
                hoverParams = new UiElementHoverParams();
            }

            Focus();
            if (hoverParams.isMoveMouse)
            {
                Mouse.MoveTo(GetClickablePoint());
            }
            else
            {
                Mouse.Position = GetClickablePoint();
            }
        }

        public void SimulateTypeText(string text)
        {
            ((HTMLInputElement)_element).value = text;
        }

        public void HighLight(Color? color = null, TimeSpan? duration = null, bool blocking = false)
        {
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

        public void Focus()
        {
            _browser.Activate();
            (_element as IHTMLElement2)?.focus();
        }

        public bool IsChecked()
        {
            return ((IHTMLInputElement) _element).@checked;
        }

        public void Check()
        {
            ((IHTMLInputElement) _element).@checked = true;
        }

        public void UnCheck()
        {
            ((IHTMLInputElement) _element).@checked = false;
        }

        public void Toggle()
        {
            _element.click();
        }

        public string GetText()
        {
            return _element.innerText;
        }

        public void SetText(string text)
        {
            ((HTMLInputElement)_element).value = text;
        }

        public bool IsPassword()
        {
            return (_element as IHTMLInputElement)?.type == "password";
        }

        public void ClearSelection()
        {
            ((HTMLSelectElement)_element).value = "";
        }

        public void SelectItem(string item)
        {
            ((HTMLSelectElement)_element).value = item;
        }

        public void SelectMultiItems(string[] items)
        {
            foreach (IHTMLOptionElement option in (IHTMLElementCollection) _element.children)
            {
                if (items.Contains(option.text))
                {
                    option.selected = true;
                }
                option.selected = false;
            }
        }

        public void ClearText()
        {
            ((HTMLInputElement)_element).value = "";
        }

        public object GetAttributeValue(string attributeName)
        {
            return _element.GetAttribute(attributeName);
        }

        public bool IsEnable()
        {
            return _browser.ReadyState==ReadyState.Complete;
        }

        public bool IsVisible()
        {
            return _browser.WebBrowser.Visible;
        }

        public bool IsTable()
        {
            return Role.Equals("table", StringComparison.OrdinalIgnoreCase);
        }

        public void SetAttribute(string attrName, string attrValue)
        {
            _element.setAttribute(attrName, attrValue);
        }

        public object ExecuteScript(string script, params object[] args)
        {
            return _browser.ExecuteScript(script, args,_element);
        }
    }
}
