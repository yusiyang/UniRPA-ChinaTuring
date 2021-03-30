using ActiproSoftware.Text;
using ActiproSoftware.Text.Languages.DotNet.Reflection;
using ActiproSoftware.Text.Languages.VB.Implementation;
using ActiproSoftware.Windows.Controls.SyntaxEditor;
using ActiproSoftware.Windows.Controls.SyntaxEditor.IntelliPrompt.Implementation;
using ActiproSoftware.Windows.Extensions;
using log4net;
using Microsoft.CodeAnalysis;
using NPOI.SS.Formula.Functions;
using Plugins.Shared.Library.CodeCompletion;
using Plugins.Shared.Library.Editors;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities;
using System.Activities.Presentation.Hosting;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using UniStudio.Community.Librarys;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace UniStudio.Community.ExpressionEditor
{
    /// <summary>
    /// MDZZ  调试了俩天才发现每次修改表达式后都会重新生成实例RoslynExpressionEditorInstance
    /// </summary>
    public class RoslynExpressionEditorInstance : IExpressionEditorInstance
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private SyntaxEditor textEditor = new SyntaxEditor() { Tag = Guid.NewGuid() };

        private List<VariableNameType> variableDeclarations = new List<VariableNameType>();//变量和参数列表
        internal ExpressionNode m_namespaceNodeRoot = new ExpressionNode();
        private Collection<string> ImportedNamespaces { get; set; }

        bool canCommand = true;
        bool isModify = false;
        /// <summary>
        /// 是否打开过创建变量的popup
        /// </summary>
        int hasCommand = -1;

        private ExpressionTextBox _expressionTextBox;
        /// <summary>
        /// 如果是属性弹出框则为true，如果直接在设计流程中编辑的则为false
        /// </summary>
        private bool _isWindowContent;

        public RoslynExpressionEditorInstance()
        {
            textEditor.FontSize = 12;
            textEditor.Foreground = Brushes.Black;
            textEditor.Padding = new Thickness(0, 0, 15, 0);//以便让最后的光标CARET显示出来，并且防止错误符号遮挡
            textEditor.BorderThickness = new Thickness(0);
            textEditor.IsAutoCorrectEnabled = false;
        }


        private void TextEditor_DocumentTextChanged(object sender, EditorSnapshotChangedEventArgs e)
        {
            try
            {
                isModify = true;
                GlobalEditorConfig.SyntaxEditorDocumentTextChanged(sender, e, variableDeclarations, m_namespaceNodeRoot);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, logger);
            }
        }

        string GetHeaderText()
        {
            StringBuilder importsNamespace = new StringBuilder();//导入命名空间以实现省略命名空间的功能
            foreach (var ns in ImportedNamespaces)
            {
                importsNamespace.Append($"Imports {ns}");
                importsNamespace.AppendLine();
            }
            importsNamespace.Append("Module Module1\r\nSub Main()");
            importsNamespace.AppendLine();
            importsNamespace.Append($"{string.Join(Environment.NewLine, variableDeclarations.Select(v => $"Dim {v.VariableName} As {v.VariableType.FullName}"))}");
            importsNamespace.AppendLine();
            //Dim _tmp_ = ");
            return importsNamespace.ToString();
        }


        /// <summary>
        /// 注册键盘事件
        /// </summary>
        /// <param name="keyBinding"></param>
        public void RegisterKeyCommand(KeyBinding keyBinding)
        {
            keyBinding.CommandParameter = this._expressionTextBox;
            textEditor.InputBindings.Add(keyBinding);
        }

        public void ExcuteKeyCommand()
        {
            hasCommand = 1;
            canCommand = false;
            VariablePopupManager.Instance.CreateVariablePopup(this._expressionTextBox);
            var popup = VariablePopupManager.Instance.GetVariablePopup(_expressionTextBox);
            if (popup != null && popup.CreateVariableAction == null)
            {
                popup.CreateVariableAction = (str) =>
                {
                    ///如果创建变量失败 直接获取焦点
                    if (!string.IsNullOrEmpty(str))
                    {
                        variableDeclarations.Add(new VariableNameType()
                        {
                            VariableType = typeof(string),
                            VariableName = str,
                        });
                        textEditor.Document.SetHeaderAndFooterText(GetHeaderText(), @"\r\nEnd Sub\r\nEnd Module");
                        textEditor.Document.AppendText(TextChangeTypes.Custom, str);
                    }
                    textEditor.Focus();
                    canCommand = true;
                };
            }
        }

        protected void MouseRightUp(object sender, MouseButtonEventArgs e)
        {
            var contextMenu = new ContextMenu();
            var menuItem = new MenuItem();
            menuItem.Header = "创建变量      Ctrl+K";
            menuItem.Click += MenuClick;
            contextMenu.Items.Add(menuItem);
            contextMenu.Placement = PlacementMode.MousePoint;
            textEditor.ContextMenu = contextMenu;
            contextMenu.IsOpen = true;
        }

        private void MenuClick(object sender, RoutedEventArgs e)
        {
            textEditor.ContextMenu.IsOpen = false;
            ExcuteKeyCommand();
        }

        public void UpdateInstance(List<ModelItem> variables, string text, ImportedNamespaceContextItem importedNamespaces)
        {
            try
            {
                variableDeclarations = variables.Select(v =>
                {
                    var c = v.GetCurrentValue() as LocationReference;

                    return new VariableNameType
                    {
                        VariableName = c?.Name,
                        VariableType = c?.Type
                    };
                }).Where(x => !string.IsNullOrWhiteSpace(x.VariableName)).ToList();
            }
            catch (Exception err)
            {
                Logger.Debug(err, logger);
                variableDeclarations = new List<VariableNameType>();
            }

            this.ImportedNamespaces = importedNamespaces.ImportedNamespaces;



            //new Task(() =>
            //{
            //    var language = new VBSyntaxLanguage();
            //    var project = language.GetService<IProjectAssembly>();
            //    project.AssemblyReferences.AddMsCorLib();// MsCorLib should always be added at a minimum
            //    foreach (var ns in ImportedNamespaces)
            //    {
            //        project.AssemblyReferences.Add(ns);
            //    }
            //    UniWorkforce.Librarys.Common.RunInUI(() =>
            //    {
            //        textEditor.Document.Language = language;  //设置为VB
            //    });
            //}).Start();

            textEditor.InitVBLanguage(ImportedNamespaces);


            //设置代码前后文本（用户只需要编辑显示中间部分就好了）
            textEditor.Document.SetHeaderAndFooterText(GetHeaderText(), @"\r\nEnd Sub\r\nEnd Module");
            textEditor.Document.AppendText(TextChangeTypes.AutoComplete, text);

            if (_expressionTextBox == null)
            {
                textEditor.Unloaded += TextEditor_Unloaded;
                textEditor.DocumentTextChanged += TextEditor_DocumentTextChanged;
                textEditor.GotFocus += TextEditor_GotFocus;
                textEditor.LostFocus += TextEditor_LostFocus;
                textEditor.MouseRightButtonUp += MouseRightUp;
                textEditor.Document.FileName = $"Document{Guid.NewGuid()}.vb";
            }
        }


        public void Focus()
        {
            _expressionTextBox = VisualTreeHelperEx.FindAncestorByType<ExpressionTextBox>(textEditor);
            _isWindowContent = _expressionTextBox.ActualHeight > 30;
            #region 让文本框和窗口一起放大缩小
            textEditor.IsLineNumberMarginVisible = _isWindowContent;
            textEditor.IsSelectionMarginVisible = _isWindowContent;
            textEditor.HorizontalScrollBarVisibility = _isWindowContent ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            textEditor.VerticalScrollBarVisibility = _isWindowContent ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            textEditor.IsOutliningMarginVisible = false;
            textEditor.CanSplitHorizontally = false;
            #endregion

            new Task(() =>
            {
                ///需要间隔1ms,不然的话获取到焦点后无法编辑
                Thread.Sleep(1);
                UniWorkforce.Librarys.Common.RunInUI(() =>
                {
                    textEditor.Focusable = true;
                    textEditor.Focus();
                    hasCommand = 0;
                });
            }).Start();
        }

        private void TextEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            GotAggregateFocus?.Invoke(sender, e);
        }



        private void TextEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!_isWindowContent && ((canCommand && isModify) || hasCommand == 0))
            {
                ///LostAggregateFocus 后再次选取 会重新生成textEditor  所以就释放textEditor
                LostAggregateFocus?.Invoke(sender, e);
                textEditor.Unloaded -= TextEditor_Unloaded;
                textEditor.DocumentTextChanged -= TextEditor_DocumentTextChanged;
                textEditor.GotFocus -= TextEditor_GotFocus;
                textEditor.LostFocus -= TextEditor_LostFocus;
                textEditor.MouseRightButtonDown -= MouseRightUp;
                textEditor.InputBindings.Clear();
                textEditor.ContextMenu = null;
                VariablePopupManager.Instance.ClearPopup(_expressionTextBox);
                m_namespaceNodeRoot = null;
                _expressionTextBox = null;
            }
        }

        #region IExpressionEditorInstance implicit

        public bool AcceptsReturn { get; set; }

        public bool AcceptsTab { get; set; }

        public bool HasAggregateFocus
        {
            get
            {
                return true;
            }
        }

        public Control HostControl
        {
            get
            {
                return textEditor;
            }
        }

        public int MaxLines { get; set; }

        public int MinLines { get; set; }

        public event EventHandler Closing;
        public event EventHandler GotAggregateFocus;
        public event EventHandler LostAggregateFocus;
        public event EventHandler TextChanged;

        public bool CanCompleteWord()
        {
            return true;
        }

        public bool CanCopy()
        {
            return true;
        }

        public bool CanCut()
        {
            return true;
        }

        public bool CanDecreaseFilterLevel()
        {
            return true;
        }

        public bool CanGlobalIntellisense()
        {
            return true;
        }

        public bool CanIncreaseFilterLevel()
        {
            return true;
        }

        public bool CanParameterInfo()
        {
            return true;
        }

        public bool CanPaste()
        {
            return true;
        }

        public bool CanQuickInfo()
        {
            return true;
        }

        public void ClearSelection()
        {

        }

        public void Close()
        {

        }

        public bool CompleteWord()
        {
            return true;
        }

        public string GetCommittedText()
        {
            return textEditor.Text;
        }

        public bool GlobalIntellisense()
        {
            return true;
        }
        public bool DecreaseFilterLevel()
        {
            return true;
        }

        public bool IncreaseFilterLevel()
        {
            return true;
        }

        public bool ParameterInfo()
        {
            return true;
        }

        public bool QuickInfo()
        {
            return true;
        }

        #endregion

        #region IExpressionEditorInstance explicit

        //void IExpressionEditorInstance.Focus()
        //{

        //    _expressionTextBox = VisualTreeHelperEx.FindAncestorByType<ExpressionTextBox>(textEditor);

        //    _isWindowContent = _expressionTextBox.ActualHeight > 30;

        //    #region 让文本框和窗口一起放大缩小
        //    textEditor.IsLineNumberMarginVisible = _isWindowContent;
        //    textEditor.IsSelectionMarginVisible = _isWindowContent;
        //    textEditor.HorizontalScrollBarVisibility = _isWindowContent ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        //    textEditor.VerticalScrollBarVisibility = _isWindowContent ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        //    textEditor.IsOutliningMarginVisible = false;
        //    textEditor.CanSplitHorizontally = false;
        //    #endregion





        //    new Task(() =>
        //    {
        //        ///需要间隔1ms,不然的话获取到焦点后无法编辑
        //        Thread.Sleep(1);
        //        UniWorkforce.Librarys.Common.RunInUI(() =>
        //        {
        //            textEditor.Focus();
        //        });
        //    }).Start();
        //    Console.WriteLine("IExpressionEditorInstance.Focus()");
        //}


        private void TextEditor_Unloaded(object sender, RoutedEventArgs e)
        {
            Closing?.Invoke(sender, EventArgs.Empty);
        }

        bool IExpressionEditorInstance.Cut()
        {
            //textEditor.Cut();
            return true;
        }

        bool IExpressionEditorInstance.Copy()
        {
            //textEditor.Copy();
            return true;
        }

        bool IExpressionEditorInstance.Paste()
        {
            //textEditor.Paste();
            return true;
        }

        bool IExpressionEditorInstance.Undo()
        {
            return true;
            //return textEditor.Undo();
        }

        bool IExpressionEditorInstance.Redo()
        {
            return true;
            //return textEditor.Redo();
        }

        bool IExpressionEditorInstance.CanUndo()
        {
            return true;
            //return textEditor.CanUndo;
        }

        bool IExpressionEditorInstance.CanRedo()
        {
            return true;
            //return textEditor.CanRedo;
        }

        string IExpressionEditorInstance.Text
        {
            get
            {
                return textEditor.Text;
            }

            set
            {
                textEditor.Text = value;
            }
        }

        ScrollBarVisibility IExpressionEditorInstance.VerticalScrollBarVisibility
        {
            get
            {
                return textEditor.VerticalScrollBarVisibility;
            }

            set
            {
                textEditor.VerticalScrollBarVisibility = value;
            }
        }

        ScrollBarVisibility IExpressionEditorInstance.HorizontalScrollBarVisibility
        {
            get
            {
                return textEditor.HorizontalScrollBarVisibility;
            }

            set
            {
                textEditor.HorizontalScrollBarVisibility = value;
            }
        }




        #endregion
    }
}
