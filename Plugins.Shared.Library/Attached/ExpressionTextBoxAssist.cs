using System;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.View;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Plugins.Shared.Library.Attached
{
    public class ExpressionTextBoxAssist
    {
        private const string source = "ModelItem";

        public static PropertyInfo GetBindProperty(DependencyObject obj)
        {
            return (PropertyInfo)obj.GetValue(BindPropertyProperty);
        }

        public static void SetBindProperty(DependencyObject obj, PropertyInfo value)
        {
            obj.SetValue(BindPropertyProperty, value);
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindPropertyProperty =
            DependencyProperty.RegisterAttached("BindProperty", typeof(PropertyInfo), typeof(ExpressionTextBoxAssist), new PropertyMetadata(null,BindPropertyChanged));

        private static void BindPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is PropertyInfo propertyInfo && d is ExpressionTextBox textBox)
            {
                var des = propertyInfo.GetCustomAttribute(typeof(DescriptionAttribute),false);
                textBox.HintText = (des as DescriptionAttribute)?.Description;
                textBox.ExpressionType = propertyInfo.PropertyType.IsGenericType ? propertyInfo.PropertyType.GenericTypeArguments[0] : propertyInfo.PropertyType;
                textBox.SetBinding(ExpressionTextBox.OwnerActivityProperty, source);
                textBox.SetBinding(ExpressionTextBox.ExpressionProperty, new System.Windows.Data.Binding($"{source}.{propertyInfo.Name}")
                {
                    Mode = System.Windows.Data.BindingMode.TwoWay,
                    Converter = new ArgumentToExpressionConverter(),
                    ConverterParameter = "In",
                });

            }
            else {
                throw new ArgumentException("必须绑定到ExpressionTextBox控件上！");
            }
        }
    }

    public class PropertyInfoExtension : MarkupExtension
    {
        public Type Type { get; set; }

        public string PropertyName { get; set; }

        public PropertyInfoExtension() { 
            
        }

        public PropertyInfoExtension(string propertyName) {
            PropertyName = propertyName;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Type.GetProperty(PropertyName);
        }
    }
}

