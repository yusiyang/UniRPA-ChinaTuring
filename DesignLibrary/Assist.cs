using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DesignLibrary
{
    public class Assist
    {
        #region Position 图标位置
        public static Dock GetPosition(DependencyObject obj)
        {
            return (Dock)obj.GetValue(PositionProperty);
        }

        public static void SetPosition(DependencyObject obj, Dock value)
        {
            obj.SetValue(PositionProperty, value);
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.RegisterAttached("Position", typeof(Dock), typeof(Assist), new PropertyMetadata(Dock.Left));
        #endregion

        #region 图标类名

        public static EnumIcon GetIcon(DependencyObject obj)
        {
            return (EnumIcon)obj.GetValue(IconProperty);
        }

        public static void SetIcon(DependencyObject obj, EnumIcon value)
        {
            obj.SetValue(IconProperty, value);
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(EnumIcon), typeof(Assist), new PropertyMetadata(EnumIcon.NoIcon, OnIconChanged));

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Path element)
            {
                if (Enum.TryParse(e.NewValue.ToString(), out EnumIcon icon))
                {
                    var source = Geometry.Parse(IconDataFactory.IconDic[icon]);
                    element.Data = source;
                }
            }
        }

        #endregion

        #region 图标大小


        public static double GetIconWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(IconWidthProperty);
        }

        public static void SetIconWidth(DependencyObject obj, double value)
        {
            obj.SetValue(IconWidthProperty, value);
        }

        // Using a DependencyProperty as the backing store for IconWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.RegisterAttached("IconWidth", typeof(double), typeof(Assist), new PropertyMetadata(15.0));



        public static double GetIconHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(IconHeightProperty);
        }

        public static void SetIconHeight(DependencyObject obj, double value)
        {
            obj.SetValue(IconHeightProperty, value);
        }

        // Using a DependencyProperty as the backing store for IconHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.RegisterAttached("IconHeight", typeof(double), typeof(Assist), new PropertyMetadata(15.0));





        #endregion

        #region 图标颜色
        public static Brush GetIconBrush(DependencyObject obj)
        {
            return (Brush)obj.GetValue(IconBrushProperty);
        }

        public static void SetIconBrush(DependencyObject obj, Brush value)
        {
            obj.SetValue(IconBrushProperty, value);
        }

        // Using a DependencyProperty as the backing store for IconBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconBrushProperty =
            DependencyProperty.RegisterAttached("IconBrush", typeof(Brush), typeof(Assist), new PropertyMetadata(Brushes.Black));

        #endregion

        #region 圆角角度

        public static CornerRadius GetCornerRadius(DependencyObject obj)
        {
            return (CornerRadius)obj.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(Assist), new PropertyMetadata(new CornerRadius(0)));

        #endregion



        public static bool GetFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusProperty);
        }

        public static void SetFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusProperty, value);
        }

        // Using a DependencyProperty as the backing store for Focus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FocusProperty =
            DependencyProperty.RegisterAttached("Focus", typeof(bool), typeof(Assist), new PropertyMetadata(false,FocusPropertyChanged));

        private static void FocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool focus && focus && d is FrameworkElement element) {
                element.Focusable = true;
                element.Focus();
            }
        }
    }
}
