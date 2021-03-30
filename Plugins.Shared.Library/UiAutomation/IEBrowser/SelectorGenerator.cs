using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MSHTML;

namespace Plugins.Shared.Library.UiAutomation.IEBrowser
{
    public static class SelectorGenerator
    {
        /// <summary>
        /// 获取CSS Selector
        /// </summary>
        /// <param name="srcElement">IE元素</param>
        /// <param name="optimized">优化选项</param>
        /// <returns></returns>
        public static string CssPath(this IHTMLElement srcElement, bool optimized)
        {
            var node = srcElement as IHTMLDOMNode;
            if (node == null || node.nodeType != 1) return "";
            var steps = new List<SelectorStep>();
            var contextNode = node;
            //Debug.WriteLine("0 " + DateTime.Now + " " + DateTime.Now.Millisecond);
            while (contextNode != null)
            {
                var step = CssPathStep(contextNode, optimized, contextNode == node);
                if (step == null) break; // Error - bail out early.
                steps.Add(step);
                if (step.Optimized) break;
                contextNode = contextNode.parentNode;
            }
            //Debug.WriteLine("2 " + DateTime.Now + " " + DateTime.Now.Millisecond);
            steps.Reverse();
            var cssPath = steps.Aggregate("", (current, selectorStep) => current + " > " + selectorStep.Selector).Substring(3);
            //return "<css>" + cssPath + "</css>";
            return cssPath;
        }

        /// <summary>
        /// 获取每个节点的Selector
        /// </summary>
        /// <param name="node"></param>
        /// <param name="optimized"></param>
        /// <param name="isTargetNode"></param>
        /// <returns></returns>
        private static SelectorStep CssPathStep(IHTMLDOMNode node, bool optimized, bool isTargetNode)
        {
            var element = node as IHTMLElement;

            if (node.nodeType != 1) return null;
            var id = element.getAttribute("id") as string;
            var reg =  new Regex(@"^\d");
            if (optimized)
            {
                if (!string.IsNullOrEmpty(id)&&!reg.IsMatch(id)) return new SelectorStep() { Selector = IdSelector(id), Optimized = true };
                var nodeNameLower = node.nodeName.ToLower();
                if (nodeNameLower == "body" || nodeNameLower == "head" || nodeNameLower == "html")
                    return new SelectorStep() { Selector = node.nodeName.ToLower(), Optimized = true };
            }

            var nodeName = node.nodeName.ToLower();
            if (!string.IsNullOrEmpty(id) && !reg.IsMatch(id)) return new SelectorStep() { Selector = nodeName + IdSelector(id), Optimized = true };
            var parent = node.parentNode;
            if (parent == null || parent.nodeType == 9) return new SelectorStep() { Selector = nodeName, Optimized = true };
            var prefixedOwnClassNamesArray = PrefixedElementClassNames(element);
            var needsClassNames = false;
            var needsNthChild = false;
            var ownIndex = -1;
            var elementIndex = -1;
            var siblings = parent.childNodes as IHTMLDOMChildrenCollection;
            for (var i = 0; (ownIndex == -1 || !needsNthChild) && i < siblings.length; ++i)
            {
                var sibling = siblings.item(i) as IHTMLDOMNode;
                if (sibling?.nodeType != 1) continue;
                elementIndex += 1;
                if (sibling == node)
                {
                    ownIndex = elementIndex;
                    continue;
                }
                if (needsNthChild) continue;
                if (sibling.nodeName.ToLower() != nodeName) continue;

                needsClassNames = true;
                var ownClassNames = new HashSet<string>(prefixedOwnClassNamesArray);
                if (ownClassNames.Count == 0)
                {
                    needsNthChild = true;
                    continue;
                }
                List<string> siblingClassNamesArray = PrefixedElementClassNames(sibling as IHTMLElement);
                foreach (var siblingClass in siblingClassNamesArray)
                {
                    if (!ownClassNames.Contains(siblingClass)) continue;
                    ownClassNames.Remove(siblingClass);
                    if (ownClassNames.Count == 0)
                    {
                        needsNthChild = true;
                        break;
                    }
                }
            }
            var result = nodeName;
            if (isTargetNode && nodeName.ToLower() == "input" &&
                !string.IsNullOrEmpty(element.getAttribute("type")) &&
                string.IsNullOrEmpty(element.getAttribute("id")) &&
                string.IsNullOrEmpty(element.getAttribute("classname")))
                result += "[type=" + CSSEscape(element.getAttribute("type")) + "]";
            if (needsNthChild)
            {
                result += ":nth-child(" + (ownIndex + 1) + ")";
            }
            else if (needsClassNames)
            {
                foreach (var prefixedName in prefixedOwnClassNamesArray)
                {
                    result += "." + CSSEscape(prefixedName.Substring(1));
                }
            }
            return new SelectorStep() { Selector = result, Optimized = false };
        }

        /// <summary>
        /// 存在ID时的Selector
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static string IdSelector(string id)
        {
            return "#" + CSSEscape(id);
        }

        /// <summary>
        /// 获取元素的类名集合
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static List<string> PrefixedElementClassNames(IHTMLElement element)
        {
            var classAttributes = new List<string>();
            var classAttribute = element?.getAttribute("classname");
            if (string.IsNullOrEmpty(classAttribute)) return classAttributes;
            string[] substrings = Regex.Split(classAttribute, @"\s+");
            foreach (var substring in substrings)
            {
                if (!string.IsNullOrEmpty(substring))
                {
                    classAttributes.Add("$" + substring);
                }
            }

            return classAttributes;
        }

        /// <summary>
        /// 转义CSS字符串
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string CSSEscape(string value)
        {
            return value;

            // IE暂不支持以下处理
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            var length = value.Length;
            var index = -1;
            var result = "";
            var firstCodeUnit = value[0];
            while (++index < length)
            {
                var codeUnit = value[index];
                // Note: there’s no need to special-case astral symbols, surrogate
                // pairs, or lone surrogates.

                // If the character is NULL (U+0000), then the REPLACEMENT CHARACTER
                // (U+FFFD).
                if (codeUnit == 0x0000)
                {
                    result += '\uFFFD';
                    continue;
                }

                if (// If the character is in the range [\1-\1F] (U+0001 to U+001F) or is
                    // U+007F, […]
                    (codeUnit >= 0x0001 && codeUnit <= 0x001F) || codeUnit == 0x007F ||
                    // If the character is the first character and is in the range [0-9]
                    // (U+0030 to U+0039), […]
                    (index == 0 && codeUnit >= 0x0030 && codeUnit <= 0x0039) ||
                    // If the character is the second character and is in the range [0-9]
                    // (U+0030 to U+0039) and the first character is a `-` (U+002D), […]
                    (index == 1 && codeUnit >= 0x0030 && codeUnit <= 0x0039 && firstCodeUnit == 0x002D))
                {
                    // https://drafts.csswg.org/cssom/#escape-a-character-as-code-point
                    result += '\\' + ((int)codeUnit).ToString("X") + ' ';
                    continue;
                }

                if (// If the character is the first character and is a `-` (U+002D), and
                    // there is no second character, […]
                    index == 0 && length == 1 && codeUnit == 0x002D)
                {
                    result += '\\' + value[index];
                    continue;
                }
                // If the character is not handled by one of the above rules and is
                // greater than or equal to U+0080, is `-` (U+002D) or `_` (U+005F), or
                // is in one of the ranges [0-9] (U+0030 to U+0039), [A-Z] (U+0041 to
                // U+005A), or [a-z] (U+0061 to U+007A), […]
                if (codeUnit >= 0x0080 || codeUnit == 0x002D || codeUnit == 0x005F ||
                    codeUnit >= 0x0030 && codeUnit <= 0x0039 || codeUnit >= 0x0041 &&
                    codeUnit <= 0x005A || codeUnit >= 0x0061 && codeUnit <= 0x007A)
                {
                    // the character itself
                    result += value[index];
                    continue;
                }
                // Otherwise, the escaped character.
                // https://drafts.csswg.org/cssom/#escape-a-character
                result += '\\' + value[index];
            }
            return result;
        }
    }

    public class SelectorStep
    {
        public string Selector { get; set; }
        public bool Optimized { get; set; }
    }
}
