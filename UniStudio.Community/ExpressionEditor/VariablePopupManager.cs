using System.Activities.Presentation.View;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace UniStudio.Community.ExpressionEditor
{
    public class VariablePopupManager
    {
        private static VariablePopupManager _instance;

        private static object _lockObj = new object();

        private static ConcurrentDictionary<ExpressionTextBox, VariablePopup> _expressionTexPopupDic = new ConcurrentDictionary<ExpressionTextBox, VariablePopup>();

        private VariablePopupManager()
        { }

        public static VariablePopupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObj)
                    {
                        if (_instance == null)
                        {
                            _instance = new VariablePopupManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public void CreateVariablePopup(ExpressionTextBox expressionTextBox)
        {
            if (expressionTextBox == null)
            {
                expressionTextBox = VisualTreeHelperEx.FindAncestorByType<ExpressionTextBox>(Keyboard.FocusedElement as DependencyObject);
            }

            if (!_expressionTexPopupDic.TryGetValue(expressionTextBox, out VariablePopup variablePopup))
            {
                variablePopup = new VariablePopup(expressionTextBox);
                _expressionTexPopupDic[expressionTextBox] = variablePopup;
            }
            variablePopup.Show();
        }

        public void ClearPopup(ExpressionTextBox expressionTextBox)
        {
            if (_expressionTexPopupDic.TryGetValue(expressionTextBox, out VariablePopup variablePopup))
            {
                variablePopup.CreateVariableAction = null;
            }
        }

        public VariablePopup GetVariablePopup(ExpressionTextBox expressionTextBox)
        {
            if (_expressionTexPopupDic.TryGetValue(expressionTextBox, out VariablePopup variablePopup))
            {
                return variablePopup;
            }
            return null;
        }
    }
}
