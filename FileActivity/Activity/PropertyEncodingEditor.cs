using System;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ActiproSoftware.Windows.Controls.Editors;

namespace FileActivity.Activity
{
    class PropertyEncodingEditor : PropertyValueEditor
    {
        public PropertyEncodingEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();
            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            FrameworkElementFactory encodingBox = new FrameworkElementFactory(typeof(AutoCompleteBox));
            Binding boxBinding = new Binding("Value");
            boxBinding.Mode = BindingMode.TwoWay;
            encodingBox.SetBinding(AutoCompleteBox.TextProperty,boxBinding);
            encodingBox.SetValue(AutoCompleteBox.ItemsSourceProperty,new List<string>
            {
                Encoding.ASCII.WebName,
                Encoding.BigEndianUnicode.WebName,
                Encoding.Default.WebName,
                Encoding.UTF32.WebName,
                Encoding.UTF7.WebName,
                Encoding.UTF8.WebName,
                Encoding.Unicode.WebName,
                
            });
            //encodingBox.SetValue(AutoCompleteBox.DataFilterProperty, new AutoCompleteBoxStringFilter());
            stack.AppendChild(encodingBox);
            this.InlineEditorTemplate.VisualTree = stack;
        }
    }
}
