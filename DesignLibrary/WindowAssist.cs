using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DesignLibrary
{
    public class WindowAssist
    {


        public static bool GetIsClosed(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsClosedProperty);
        }

        public static void SetIsClosed(DependencyObject obj, bool value)
        {
            obj.SetValue(IsClosedProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsClosed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsClosedProperty =
            DependencyProperty.RegisterAttached("IsClosed", typeof(bool), typeof(WindowAssist), new PropertyMetadata(false,WindowClosedChanged));

        private static void WindowClosedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window win && e.NewValue is bool isclose && isclose) {
                win.Close();
            }
        }
    }
}
