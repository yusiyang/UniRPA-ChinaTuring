using GalaSoft.MvvmLight.Command;
using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.UserControls;
using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace UniStudio.ExpressionEditor
{
    public class VariablePopup
    {
        private ExpressionTextBox _expressionTextBox;

        private Popup _popup;

        private TextBox _varTextBox;

        /// <summary>
        /// 暴露出来一个public委托，这样的话就不用把接受变量的控件写死在这个组件的类里面了
        /// </summary>
        public Action<string> CreateVariableAction { get; set; }



        public VariablePopup(ExpressionTextBox expressionTextBox)
        {
            _expressionTextBox = expressionTextBox;
            CreatePopup();
        }

        public Popup Popup => _popup;

        public void Show()
        {
            Popup.IsOpen = true;
            _varTextBox.Focus();
        }

        public void Hide()
        {
            Popup.IsOpen = false;
        }

        public string Text => _varTextBox.Text;

        public void ClearText()
        {
            _varTextBox.Text = "";
        }

        private void CreatePopup()
        {
            var expressionParentGrid = _expressionTextBox.Parent as Grid;

            _popup = new Popup();

            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Height = _expressionTextBox.ActualHeight;
            stackPanel.Background = Brushes.White;
            var textBox = new TextBox
            {
                Text = "设置变量:",
                IsReadOnly = true,
                BorderThickness = new Thickness(1, 1, 0, 1),
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(2)
            };
            var varTextBox = new TextBox
            {
                Width = 100,
                BorderThickness = new Thickness(0, 1, 1, 1),
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(2)
            };
            _varTextBox = varTextBox;
            varTextBox.InputBindings.Add(new KeyBinding(new RelayCommand(CreateVariable), Key.Enter, ModifierKeys.None));

            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(varTextBox);
            _popup.Child = stackPanel;
            _popup.PlacementTarget = _expressionTextBox;
            _popup.Placement = PlacementMode.Left;
            _popup.StaysOpen = false;

            //expressionParentGrid.Children.Add(_popup);
        }

        /// <summary>
        /// 创建变量
        /// </summary>
        private void CreateVariable()
        {
            var varTextBox = Keyboard.FocusedElement as TextBox;

            if (string.IsNullOrEmpty(varTextBox?.Text))
            {
                VerifyVariableDialog verifyVariableDialog = new VerifyVariableDialog("变量的名称不能为空。");
                verifyVariableDialog.Show();
                _popup.IsOpen = false;
                this.ClearText();
                CreateVariableAction?.Invoke(Text);
                return;
            }
            var variableType = _expressionTextBox.ExpressionType ?? typeof(GenericValue);
            var selectedModelItem = DocumentContext.Current.WorkflowContext.Items.GetValue<Selection>().PrimarySelection;
            var variableScopeElement = selectedModelItem.GetVariableScopeElement();
            var variableCollection = variableScopeElement.GetVariableCollection();

            if (variableCollection.Any(t => (t.GetCurrentValue() as Variable)?.Name == varTextBox.Text))
            {
                VerifyVariableDialog verifyVariableDialog = new VerifyVariableDialog("此作用域中已有名为“" + varTextBox.Text + "”的变量。请选择其他名称。");
                verifyVariableDialog.Show();
                _popup.IsOpen = false;
                this.ClearText();
                CreateVariableAction?.Invoke(Text);
                return;
            }

            using (ModelEditingScope modelEditingScope = variableCollection.BeginEdit())
            {
                var variable = Variable.Create(varTextBox.Text, variableType, VariableModifiers.None);
                variableCollection.Add(variable);
                modelEditingScope.Complete();

                var activityBuilder = selectedModelItem.Root.GetCurrentValue() as ActivityBuilder;

                var types = variableType.GetUsedTypes();
                var namespaces = new List<string>(TextExpression.GetNamespacesForImplementation(activityBuilder));
                var references = new List<AssemblyReference>(TextExpression.GetReferencesForImplementation(activityBuilder));
                foreach (Type type in types)
                {
                    if (!namespaces.Contains(type.Namespace))
                    {
                        namespaces.Add(type.Namespace);
                    }
                    string assemblyName = type.Assembly.GetName().Name;
                    if (!references.Any(r => r.AssemblyName.Name == assemblyName))
                    {
                        references.Add(new AssemblyName(assemblyName));
                    }
                }
                TextExpression.SetReferencesForImplementation(activityBuilder, references);
                TextExpression.SetNamespacesForImplementation(activityBuilder, namespaces);
            }

            _popup.IsOpen = false;
            CreateVariableAction?.Invoke(Text);
            ClearText();
            ///用委托（或者事件去传值，不要写死，换个控件就没用了  这段方法）
            //var textBlock = VisualTreeHelperEx.FindDescendantByName(_expressionTextBox, "expresionTextBlock") as TextBlock;
            //if (textBlock == null)
            //{
            //    var textEditor = VisualTreeHelperEx.FindDescendantByType<TextEditor>(_expressionTextBox);
            //    if (textEditor != null)
            //    {
            //        textEditor.TextArea.Document.Replace(0, textEditor.Text.Length, this.Text);
            //        this.ClearText();
            //        textEditor.TextArea.Focus();
            //    }
            //}
            //else
            //{
            //    textBlock.Focus();
            //}
        }
    }
}
