using ActiproSoftware.Text.Languages.DotNet.Reflection;
using ActiproSoftware.Text.Languages.VB.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using ActiproSoftware.Windows.Extensions;
using Plugins.Shared.Library.CodeCompletion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Editors
{
    /// <summary>
    /// SyntaxEditor的全局通用配置
    /// </summary>
    public static class GlobalEditorConfig
    {
        /// <summary>
        /// 这个地方添加常用变量
        /// </summary>
        public static string[] VBKeyWords { get; } = new string[] { "new", "As", "String", "Sub", "End", "Dim","Console"};

        /// <summary>
        /// 获取代码提示信息
        /// </summary>
        /// <param name="exprFullNameString">用户输入字符</param>
        /// <param name="variableDeclarations">参数列表</param>
        /// <param name="m_namespaceNodeRoot">命名控件根</param>
        /// <returns></returns>

        static CompletionSession GetCompletionSession(string exprFullNameString,
            List<VariableNameType> variableDeclarations,
            ExpressionNode namespaceNodeRoot)
        {

            CompletionSession session = new CompletionSession();


            List<ExpressionNode> rootNodes =
                ExpressionNode.SubsetAutoCompletionList(namespaceNodeRoot, exprFullNameString);

            List<CompletionItem> items = new List<CompletionItem>();


            var queryVarList = from s in variableDeclarations
                               where s.VariableName.StartsWith(exprFullNameString, StringComparison.CurrentCultureIgnoreCase)
                               select new
                               {
                                   Name = s.VariableName,
                                   Type = s.VariableType
                               };

            if (queryVarList != null && queryVarList.Any())
            {
                foreach (var name in queryVarList)
                {
                    items.Add(new CompletionItem(name.Name,
                        new CommonImageSourceProvider(CommonImageKind.PropertyPublic),
                        new HtmlContentProvider($"<span style=\"color:#8686AF;\">（局部变量）</span><span style=\"color:#AA551E;\">{name.Name}</span> <span style=\"color:#2D85FF;\">As {name.Type}</span>")
                        ));
                }
            }


            if (rootNodes.Count > 0)
            {
                foreach (var key in VBKeyWords)
                {
                    items.Add(new CompletionItem(key,
                       new CommonImageSourceProvider(CommonImageKind.Keyword),
                       new HtmlContentProvider($"<span style=\"color:#8686AF;\">（VB关键字）</span><span style=\"color:#AA551E;\">{key}</span>")
                       ));
                }

                var kind = CommonImageKind.PropertyPublic;
                foreach (var item in rootNodes)
                {
                    if (item.ItemType == "namespace")
                    {
                        kind = CommonImageKind.Namespace;
                    }
                    else if (item.ItemType == "class")
                    {
                        kind = CommonImageKind.ClassPublic;
                    }
                    items.Add(new CompletionItem(item.Name,
                       new CommonImageSourceProvider(kind),
                       new HtmlContentProvider($"<span style=\"color:#2D85FF;\">{item.ItemType}</span> <span style=\"color:#AA551E;\">{item.Path.Trim('.')}</span>")
                    ));
                }
            }
            if (items.Count > 0)
            {
                session.Items.AddRange(items.Distinct(x=>x.Text).OrderBy(x => x.Text));
            }
            return session;
        }


        public static void SyntaxEditorDocumentTextChanged(object sender, EditorSnapshotChangedEventArgs e, 
            List<VariableNameType> variableDeclarations,
            ExpressionNode namespaceNodeRoot) {
            if (sender is SyntaxEditor editor) {
                if (e.NewSnapshot.Text == e.OldSnapshot.Text)
                {
                    e.Handled = true;
                }
                ///非删除动作切控件自己不会提示的
                ///只要空格的时候就弹出(弹出所有的)
                else if (e.ChangedSnapshotRange.AbsoluteLength > 0
                    && editor.IntelliPrompt.Sessions.Count == 0
                    )
                {

                    ///如果在插入的位置前面又奇数个“ 则认为客户想输入的是字符串，就不提示了
                    ///当用户是想定义一个变量（变量前是dim的  也不提示）
                    var idx = e.ChangedSnapshotRange.StartOffset;
                    var count = editor.Text.Substring(0, idx).Count(x => x == '\"');

                    //var dimIdx = 0;// editor.Text.Substring(0,idx).LastIndexOf("Dim ");

                    //if (dimIdx == idx - 4)
                    //{ 
                    
                    //}
                    //else 
                    if (count % 2 == 0)
                    {
                        if (e.TypedText == " " || e.ChangedSnapshotRange.Text == Environment.NewLine)
                        {
                            ShowCompletionSession(editor, string.Empty, variableDeclarations, namespaceNodeRoot);
                        }
                        else
                        {
                            var txt = e.NewSnapshot.Text?.Trim();
                            ShowCompletionSession(editor, txt, variableDeclarations, namespaceNodeRoot);
                        }
                    }
                }
            }
        }

        static void ShowCompletionSession(SyntaxEditor textEditor, 
            string exprFullNameString, List<VariableNameType> variableDeclarations,
            ExpressionNode namespaceNodeRoot)
        {

            var session = GlobalEditorConfig.GetCompletionSession(exprFullNameString, variableDeclarations, namespaceNodeRoot);
            if (session.Items.Count > 0)
            {
                session.Open(textEditor.ActiveView);
            }
            else
            {
                textEditor.IntelliPrompt.CloseAllSessions();
            }
        }


        public static void InitVBLanguage(this SyntaxEditor syntaxEditor, IEnumerable<string> _imports) {
            new Task(() =>
            {
                var language = new VBSyntaxLanguage();
                var project = language.GetService<IProjectAssembly>();
                project.AssemblyReferences.AddMsCorLib();// MsCorLib should always be added at a minimum
                foreach (var ns in _imports)
                {
                    project.AssemblyReferences.Add(ns);
                }
                Librarys.Common.RunInUI(() =>
                {
                    syntaxEditor.Document.Language = language;  //设置为VB
                });
            }).Start();
        }



        
    }


    public class VariableNameType
    {
        public string VariableName { get; set; }

        public Type VariableType { get; set; }
    }

    public class DistinctEqualityComparer<T, V> : IEqualityComparer<T>
    {
        private Func<T, V> keySelector;

        public DistinctEqualityComparer(Func<T, V> keySelector)
        {
            this.keySelector = keySelector;
        }

        public bool Equals(T x, T y)
        {
            return EqualityComparer<V>.Default.Equals(keySelector(x), keySelector(y));
        }

        public int GetHashCode(T obj)
        {
            return EqualityComparer<V>.Default.GetHashCode(keySelector(obj));
        }
    }

    public static class DistinctExtensions
    {
        public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector)
        {
            return source.Distinct(new DistinctEqualityComparer<T, V>(keySelector));
        }
    }


}
