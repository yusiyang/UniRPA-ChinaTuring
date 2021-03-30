using Plugins.Shared.Library.UiAutomation;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace UniStudio.Community.Librarys
{
    public class DataGridAssist
    {


        public static bool GetNeedChooseFirst(DependencyObject obj)
        {
            return (bool)obj.GetValue(NeedChooseFirstProperty);
        }

        public static void SetNeedChooseFirst(DependencyObject obj, bool value)
        {
            obj.SetValue(NeedChooseFirstProperty, value);
        }

        // Using a DependencyProperty as the backing store for NeedChooseFirst.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NeedChooseFirstProperty =
            DependencyProperty.RegisterAttached("NeedChooseFirst", typeof(bool), typeof(DataGridAssist), new PropertyMetadata(false, ValueChanged));

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (
                d is DataGridRow row &&
                e.NewValue is bool needChooseFirst)
            {

                if (needChooseFirst)
                {
                    row.PreviewMouseLeftButtonDown += Row_PreviewMouseLeftButtonDown;
                    row.PreviewMouseLeftButtonUp += Row_PreviewMouseLeftButtonUp;
                    row.PreviewMouseRightButtonDown += Row_PreviewMouseRightButtonDown;
                    //row.PreviewMouseRightButtonUp += Row_PreviewMouseRightButtonUp;
                }
                else
                {
                    row.PreviewMouseLeftButtonDown -= Row_PreviewMouseLeftButtonDown;
                    row.PreviewMouseLeftButtonUp -= Row_PreviewMouseLeftButtonUp;
                    row.PreviewMouseRightButtonDown -= Row_PreviewMouseRightButtonDown;
                    //row.PreviewMouseRightButtonUp -= Row_PreviewMouseRightButtonUp;
                }


            }
        }

        private static void Row_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = VisualTreeHelperEx.FindAncestorByType<DataGrid>(sender as DataGridRow);
            grid.Columns.ForEach((i) =>
            {
                i.IsReadOnly = false;
            });
        }

        private static void Row_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                var grid = VisualTreeHelperEx.FindAncestorByType<DataGrid>(row);
                grid.SelectedItem = row.Item;
            }
        }

        private static void Row_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var grid = VisualTreeHelperEx.FindAncestorByType<DataGrid>(sender as DataGridRow);
            grid.Columns.ForEach((i) =>
            {
                i.IsReadOnly = false;
            });
        }

        private static void Row_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && !row.IsSelected)
            {
                var grid = VisualTreeHelperEx.FindAncestorByType<DataGrid>(row);
                grid.Columns.ForEach((i) =>
                {
                    i.IsReadOnly = true;
                });
            }
        }
    }
}
