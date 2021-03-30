using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DesignLibrary
{
    public class TextBoxAssist
    {
        #region PlaceHolder
        public static string GetPlaceHolder(DependencyObject obj)
        {
            return (string)obj.GetValue(PlaceHolderProperty);
        }

        public static void SetPlaceHolder(DependencyObject obj, string value)
        {
            obj.SetValue(PlaceHolderProperty, value);
        }

        // Using a DependencyProperty as the backing store for PlaceHolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceHolderProperty =
            DependencyProperty.RegisterAttached("PlaceHolder", typeof(string), typeof(TextBoxAssist), new PropertyMetadata(""));
        #endregion


        #region PlaceHolderBrush
        public static Brush GetPlaceHolderBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(PlaceHolderBrushProperty);
        }

        public static void SetPlaceHolderBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(PlaceHolderBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for PlaceHolderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaceHolderBrushProperty =
            DependencyProperty.RegisterAttached("PlaceHolderBrush", typeof(Brush), typeof(TextBoxAssist), new PropertyMetadata(new BrushConverter().ConvertFromString("#D7D7DB")));
        #endregion

    }
}
