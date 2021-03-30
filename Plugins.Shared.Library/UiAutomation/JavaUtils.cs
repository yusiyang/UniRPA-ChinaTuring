using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using WindowsAccessBridgeInterop;

namespace Plugins.Shared.Library.UiAutomation
{
    public static class JavaUtils
    {
        public static readonly AccessBridge AccessBridge = new AccessBridge();
        public static readonly HwndCache WindowCache = new HwndCache();
        public const int MAX_ACTION_INFO = 256;
        public const int MAX_ACTIONS_TO_DO = 32;
        public static List<AccessibleJvm> EnumJvms()
        {
            return AccessBridge.EnumJvms(hwnd =>
            {
                AccessBridge.CreateAccessibleWindow(hwnd);
                return WindowCache.Get(AccessBridge, hwnd);
            });
        }

        #region Extension

        public static void DoActions(this AccessibleNode node, params string[] actions)
        {
            var acNode = node as AccessibleContextNode;

            AccessibleActionsToDo todo = new AccessibleActionsToDo()
            {
                actions = new AccessibleActionInfo[MAX_ACTIONS_TO_DO],
                actionsCount = actions.Length,
            };
            for (int i = 0, n = Math.Min(actions.Length, MAX_ACTIONS_TO_DO); i < n; i++)
                todo.actions[i].name = actions[i];

            if (!AccessBridge.Functions.DoAccessibleActions(node.JvmId, acNode.AccessibleContextHandle, ref todo, out var failure))
                throw new Exception("Error performing action");
        }
        public static void SetText(this AccessibleNode node, string text)
        {
            var acNode = node as AccessibleContextNode;
            if (!AccessBridge.Functions.SetTextContents(node.JvmId, acNode?.AccessibleContextHandle, text))
                throw new Exception("Error performing action");
        }

        public static void ClearSelection(this AccessibleNode node)
        {
            var acNode = node as AccessibleContextNode;
            AccessBridge.Functions.ClearAccessibleSelectionFromContext(node.JvmId, acNode?.AccessibleContextHandle);
        }

        public static AccessibleContextInfo GetAccessibleContextInfo(this AccessibleNode node)
        {
            var acNode = node as AccessibleContextNode;
            AccessBridge.Functions.GetAccessibleContextInfo(node.JvmId, acNode?.AccessibleContextHandle, out var info);
            return info;
        }

        public static string GetText(this AccessibleNode node)
        {
            var sbText = new StringBuilder();
            var acNode = node as AccessibleContextNode;
            int caretIndex = 0;

            while (true)
            {
                if (!AccessBridge.Functions.GetAccessibleTextItems(node.JvmId, acNode.AccessibleContextHandle, out var ti, caretIndex))
                    throw new NotSupportedException("Error getting accessible text item information");

                if (!string.IsNullOrEmpty(ti.sentence))
                    sbText.Append(ti.sentence);
                else
                    break;

                caretIndex = sbText.Length;
            }

            return sbText.ToString().TrimEnd();
        }

        public static void SelectItems(this AccessibleNode node, string[] items)
        {
            var acNode = node as AccessibleContextNode;
            var selectItems = node.FindDescendents(t => items.Contains(t.GetAccessibleContextInfo().name));

            if (!selectItems.Any())
                throw new NotSupportedException("JAVA 未找到相应选择项");

            foreach (var item in selectItems)
            {
                AccessBridge.Functions.AddAccessibleSelectionFromContext(node.JvmId, acNode.AccessibleContextHandle, item.GetIndexInParent());
            }
        }
        public static void SelectItem(this AccessibleNode node, string item)
        {
            var acNode = node as AccessibleContextNode;
            var selectItem = node.FindDescendents(t => t.GetAccessibleContextInfo().name == item).FirstOrDefault();

            if (selectItem == null)
                throw new NotSupportedException("JAVA 未找到相应选择项");

            AccessBridge.Functions.AddAccessibleSelectionFromContext(node.JvmId, acNode.AccessibleContextHandle, selectItem.GetIndexInParent());
        }

        public static AccessibleTableInfo GetTableInfo(this AccessibleNode node)
        {
            var acNode = node as AccessibleContextNode;
            if (!AccessBridge.Functions.GetAccessibleTableInfo(node.JvmId, acNode.AccessibleContextHandle, out var tableInfo))
                return null;
            return tableInfo;
        }


        public static AccessibleNode FindDescendent(this AccessibleNode node, Func<AccessibleNode, bool> predicate)
        {
            var children = node?.GetChildren();

            if (children == null || !children.Any())
            {
                return null;
            }
            var find = children.FirstOrDefault(predicate);

            if (find != null)
            {
                return find;
            }
            foreach (var child in children)
            {
                find = child.FindDescendent(predicate);
                if (find != null)
                {
                    return find;
                }
            }
            return null;
        }

        public static List<AccessibleNode> FindDescendents(this AccessibleNode node, Func<AccessibleNode, bool> predicate)
        {
            var list = new List<AccessibleNode>();
            var children = node?.GetChildren();

            if (children == null || !children.Any())
            {
                return list;
            }

            IEnumerable<AccessibleNode> find = new List<AccessibleNode>();
            foreach (var child in children)
            {
                find = child.FindDescendents(predicate);
                if (find.Any())
                {
                    list.AddRange(find);
                }
            }

            find = children.Where(predicate);
            if (find.Any())
            {
                list.AddRange(find);
                return list;
            }

            return list;
        }


        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        #endregion


        public static AccessibleContextNode GetContextNode(IntPtr hwnd)
        {
            AccessBridge.Functions.GetAccessibleContextFromHWND(hwnd, out _, out var ac);
            return new AccessibleContextNode(AccessBridge, ac);
        }


    }
    public class HwndCache
    {
        private readonly ConcurrentDictionary<IntPtr, AccessibleWindow> _cache = new ConcurrentDictionary<IntPtr, AccessibleWindow>();

        public AccessibleWindow Get(AccessBridge accessBridge, IntPtr hwnd)
        {
            return _cache.GetOrAdd(hwnd, key => accessBridge.CreateAccessibleWindow(key));
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public IEnumerable<AccessibleWindow> Windows
        {
            get { return _cache.Values.Where(x => x != null); }
        }
    }
}
