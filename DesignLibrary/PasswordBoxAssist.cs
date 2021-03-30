using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace DesignLibrary
{
    public class PasswordBoxAssist
    {
        
    }

    public class PasswordBoxBehavior : Behavior<PasswordBox>
    {


        public static string GetPassWordText(DependencyObject obj)
        {
            return (string)obj.GetValue(PassWordTextProperty);
        }

        public static void SetPassWordText(DependencyObject obj, string value)
        {
            obj.SetValue(PassWordTextProperty, value);
        }

        // Using a DependencyProperty as the backing store for PassWordText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PassWordTextProperty =
            DependencyProperty.RegisterAttached("PassWordText", typeof(string), typeof(PasswordBoxBehavior), new PropertyMetadata(string.Empty));



        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.PasswordChanged -= AssociatedObject_PasswordChanged;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Password = GetPassWordText(AssociatedObject);
            AssociatedObject.PasswordChanged += AssociatedObject_PasswordChanged;
        }

        private void AssociatedObject_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            SetPassWordText(AssociatedObject, AssociatedObject.Password);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PasswordChanged -= AssociatedObject_PasswordChanged;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }
    }
}
