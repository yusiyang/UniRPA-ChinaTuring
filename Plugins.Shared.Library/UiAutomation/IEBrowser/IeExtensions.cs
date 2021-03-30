using System;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Media.Imaging;
using MSHTML;
using SHDocVw;

namespace Plugins.Shared.Library.UiAutomation.IEBrowser
{
    public static class IeExtensions
    {
        public static BitmapFrame GetImageSourceFromResource(string resourceName)
        {
            string[] names = typeof(IeExtensions).Assembly.GetManifestResourceNames();
            foreach (var name in names)
            {
                if (name.EndsWith(resourceName,StringComparison.Ordinal))
                {
                    return BitmapFrame.Create(typeof(IeExtensions).Assembly.GetManifestResourceStream(name) ?? throw new InvalidOperationException());
                }
            }
            return null;
        }
        public static IHTMLElement GetXPath(this WebBrowser wb, string xpath)
        {
            return (IHTMLElement)wb.CallScript("xpath",xpath);
        }

        // https://stackoverflow.com/questions/15273311/how-to-invoke-scripts-work-in-mshtml
        public static object CallScript(this WebBrowser axWebBrowser,string method, params object[] args)
        {
            try
            {
                object htmlWindowObject = axWebBrowser?.Document.GetProperty("parentWindow");

                // call a global JavaScript function
                return htmlWindowObject.InvokeScript(method, args);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            

        }

        private static object GetProperty(this object doc, string property)
        {
            return doc.GetType().InvokeMember(property,
                BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.Public,
                null, doc, new object[] { },CultureInfo.CurrentCulture);
        }

        private static object InvokeScript(this object doc, string method, params object[] args)
        {
            return doc.GetType().InvokeMember(method,
                BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public,
                null, doc, args, CultureInfo.CurrentCulture);
        }

        // https://stackoverflow.com/questions/28009093/how-to-using-xpath-in-webbrowser-control

        // https://github.com/mradosta/thousandpass/blob/master/addons/msie/1000Pass_com/1000pass_com/XPath.cs
        public static string GetXPath(this IHTMLElement element)
        {
            if (element == null)
                return "";
            IHTMLElement currentNode = element;
            ArrayList path = new ArrayList();

            while (currentNode != null)
            {
                string pe = GetNode(currentNode);
                if (pe != null)
                {
                    path.Add(pe);
                    //if (pe.IndexOf("@id") != -1)
                    //    break;  // Found an ID, no need to go upper, absolute path is OK
                }
                currentNode = currentNode.parentElement;
            }
            path.Reverse();
            return Join(path, "/");
        }
        #region CssSelector
        public static string GetCssPath(this IHTMLElement el)
        {
            var names = new ArrayList();
            while (el != null)
            {
                var selector = el.tagName.ToLower(CultureInfo.CurrentCulture);
                if (!string.IsNullOrEmpty(el.id))
                {
                    try
                    {
                        if ((el.document as HTMLDocument)?.querySelector(selector + '#' + el.id) == el)
                        {
                            selector += '#' + el.id;
                            names.Insert(0, selector);
                            break;
                        }
                    }
                    catch { }
                }
                else
                {
                    var sib = el as IHTMLDOMNode;
                    var nth = 1;
                    while (sib?.previousSibling != null)
                    {
                        sib = sib?.previousSibling;
                        if (sib.nodeName.ToLower(CultureInfo.CurrentCulture) == selector)
                            nth++;
                    }
                    if (nth != 1)
                        selector += ":nth-of-type(" + nth + ")";
                }
                names.Insert(0, selector);
                el = el.parentElement;
            }
            return string.Join(" > ", names.ToArray());
        }
        #endregion

        private static string Join(ArrayList items, string delimiter)
        {
            StringBuilder sb = new StringBuilder();
            foreach (object item in items)
            {
                if (item == null)
                    continue;

                sb.Append(delimiter);
                sb.Append(item);
            }
            return sb.ToString();
        }

        private static string GetNode(IHTMLElement node)
        {
            string nodeExpr = node.tagName;
            if (nodeExpr == null)  // Eg. node = #text
                return null;
            if (!string.IsNullOrEmpty(node.id))
            {
                nodeExpr += "[@id='" + node.id + "']";
                // We don't really need to go back up to //HTML, since IDs are supposed
                // to be unique, so they are a good starting point.
                return "/" + nodeExpr;
            }

            // Find rank of node among its type in the parent
            int rank = 1;
            IHTMLDOMNode nodeDom = node as IHTMLDOMNode;
            IHTMLDOMNode psDom = nodeDom.previousSibling;
            IHTMLElement ps = psDom as IHTMLElement;
            while (ps != null)
            {
                if (ps.tagName == node.tagName)
                {
                    rank++;
                }
                psDom = psDom.previousSibling;
                ps = psDom as IHTMLElement;
            }
            if (rank > 1)
            {
                nodeExpr += "[" + rank + "]";
            }
            else
            { // First node of its kind at this level. Are there any others?
                IHTMLDOMNode nsDom = nodeDom.nextSibling;
                IHTMLElement ns = nsDom as IHTMLElement;
                while (ns != null)
                {
                    if (ns.tagName == node.tagName)
                    { // Yes, mark it as being the first one
                        nodeExpr += "[1]";
                        break;
                    }
                    nsDom = nsDom.nextSibling;
                    ns = nsDom as IHTMLElement;
                }
            }
            return nodeExpr;
        }




        public static bool TryCast<T>(this object obj, out T result)
        {
            if (obj is T)
            {
                result = (T)obj;
                return true;
            }
            if (obj is Literal<T>)
            {
                result = ((Literal<T>)obj).Value;
                return true;
            }

            result = default;
            return false;
        }
        public static T TryCast<T>(this object obj)
        {
            if (TryCast<T>(obj, out var result))
                return result;
            return result;
        }
        public static T GetValue<T>(this ModelItem model, string name)
        {
            T result = default;
            if (model?.Properties[name] != null)
            {
                if (model.Properties[name].Value == null) return result;
                if (model.Properties[name].Value.Properties["Expression"] != null)
                {
                    result = model.Properties[name].Value.Properties["Expression"].ComputedValue.TryCast<T>();
                    return result;
                }
                result = model.Properties[name].ComputedValue.TryCast<T>();
                return result;
            }
            return result;
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="e"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAttribute(this IHTMLElement e, string name)
        {
            dynamic value = e.getAttribute(name);
            return value is DBNull ? "" : value + "";
        }

    }
}
