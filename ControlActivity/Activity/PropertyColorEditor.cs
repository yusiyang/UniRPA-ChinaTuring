using System.Activities.Presentation.PropertyEditing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using Xceed.Wpf.Toolkit;

namespace ControlActivity
{
    class PropertyColorEditor:PropertyValueEditor
    {
        public PropertyColorEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();
            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            FrameworkElementFactory picker = new FrameworkElementFactory(typeof(ColorPicker));
            Binding pickerBinding = new Binding("Value");
            pickerBinding.Mode = BindingMode.TwoWay;
            picker.SetBinding(ColorPicker.SelectedColorProperty,pickerBinding);
            picker.SetValue(ColorPicker.AvailableColorsHeaderProperty,"常用色");
            picker.SetValue(ColorPicker.StandardColorsHeaderProperty, "标准色");
            //picker.SetValue(ColorPicker.AdvancedButtonHeaderProperty, "自定义");
            picker.SetValue(ColorPicker.AdvancedTabHeaderProperty, "自定义");
            //picker.SetValue(ColorPicker.StandardButtonHeaderProperty, "基本色");
            picker.SetValue(ColorPicker.StandardTabHeaderProperty, "基本色");
            stack.AppendChild(picker);
            this.InlineEditorTemplate.VisualTree = stack;
        }
    }
}
