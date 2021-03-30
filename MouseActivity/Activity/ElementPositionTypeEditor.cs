using System;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MouseActivity
{
    public class ElementPositionTypeEditor : PropertyValueEditor
    {
        public ElementPositionTypeEditor()
        {
            this.InlineEditorTemplate = new DataTemplate();
            FrameworkElementFactory stack = new FrameworkElementFactory(typeof(StackPanel));
            FrameworkElementFactory comBox = new FrameworkElementFactory(typeof(ComboBox));
            Binding bindEnum = new Binding("Value");
            comBox.SetValue(ComboBox.ItemsSourceProperty, MouseClickTypes);
            comBox.SetValue(ComboBox.SelectedIndexProperty, bindEnum);
            stack.AppendChild(comBox);
            this.InlineEditorTemplate.VisualTree = stack;
        }

        public IList<ElementPositionType> MouseClickTypes
        {
            get
            {
                return Enum.GetValues(typeof(ElementPositionType)).Cast<ElementPositionType>().ToList<ElementPositionType>();
            }
        }
    }
}
