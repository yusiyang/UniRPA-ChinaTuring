using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Plugins.Shared.Library.Extensions
{
    public class UniMessageBox : Xceed.Wpf.Toolkit.MessageBox
    {
        protected override bool HasEffectiveKeyboardFocus => base.HasEffectiveKeyboardFocus;

        protected override bool IsEnabledCore => base.IsEnabledCore;

        protected override int VisualChildrenCount => base.VisualChildrenCount;

        protected override bool HandlesScrolling => base.HandlesScrolling;

        protected override IEnumerator LogicalChildren => base.LogicalChildren;

        public override void BeginInit()
        {
            base.BeginInit();
        }

        public override void EndInit()
        {
            base.EndInit();
        }

        #region 覆盖默认 Show 方法，并隐藏基类 Show 方法
        public static new MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, messageText, caption, button, icon, messageBoxStyle);
        }

        public static new MessageBoxResult Show(string messageText)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, messageText);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText);
        }

        public static new MessageBoxResult Show(string messageText, string caption)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, messageText, caption);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText, string caption)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText, caption);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText, string caption, Style messageBoxStyle)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText, caption, messageBoxStyle);
        }

        public static new MessageBoxResult Show(string messageText, string caption, MessageBoxButton button)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, messageText, caption, button);
        }

        public static new MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, Style messageBoxStyle)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, messageText, caption, button, messageBoxStyle);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText, string caption, MessageBoxButton button)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText, caption, button);
        }

        public static new MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, messageText, caption, button, icon);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText, caption, button, icon, defaultResult, messageBoxStyle);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText, caption, button, icon, defaultResult);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText, caption, button, icon, messageBoxStyle);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText, string caption)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText, caption);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, Style messageBoxStyle)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText, caption, button, messageBoxStyle);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText, caption, button);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText, caption, button, icon);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, Style messageBoxStyle)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText, caption, button, icon, messageBoxStyle);
        }

        public static new MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, messageText, caption, button, icon, defaultResult);
        }

        public static new MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(Application.Current.MainWindow, messageText, caption, button, icon, defaultResult, messageBoxStyle);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText, string caption, Style messageBoxStyle)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText, caption, messageBoxStyle);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText, caption, button, icon, defaultResult);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, Style messageBoxStyle)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText, caption, button, icon, defaultResult, messageBoxStyle);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText);
        }

        public static new MessageBoxResult Show(System.Windows.Window owner, string messageText, string caption, MessageBoxButton button, Style messageBoxStyle)
        {
            owner?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(owner, messageText, caption, button, messageBoxStyle);
        }

        public static new MessageBoxResult Show(IntPtr ownerWindowHandle, string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            Application.Current.MainWindow?.Activate();
            return Xceed.Wpf.Toolkit.MessageBox.Show(ownerWindowHandle, messageText, caption, button, icon);
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        public override bool ShouldSerializeContent()
        {
            return base.ShouldSerializeContent();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override void AddChild(object value)
        {
            base.AddChild(value);
        }

        protected override void AddText(string text)
        {
            base.AddText(text);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            return base.ArrangeOverride(arrangeBounds);
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return base.GetLayoutClip(layoutSlotSize);
        }

        protected override DependencyObject GetUIParentCore()
        {
            return base.GetUIParentCore();
        }

        protected override Visual GetVisualChild(int index)
        {
            return base.GetVisualChild(index);
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return base.HitTestCore(hitTestParameters);
        }

        protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters)
        {
            return base.HitTestCore(hitTestParameters);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            return base.MeasureOverride(constraint);
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            base.OnAccessKey(e);
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
        }

        protected override void OnCloseButtonClicked(RoutedEventArgs e)
        {
            base.OnCloseButtonClicked(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }

        protected override object OnCoerceCloseButtonVisibility(Visibility newValue)
        {
            return base.OnCoerceCloseButtonVisibility(newValue);
        }

        protected override object OnCoerceWindowStyle(WindowStyle newValue)
        {
            return base.OnCoerceWindowStyle(newValue);
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
        }

        protected override void OnContentStringFormatChanged(string oldContentStringFormat, string newContentStringFormat)
        {
            base.OnContentStringFormatChanged(oldContentStringFormat, newContentStringFormat);
        }

        protected override void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
        {
            base.OnContentTemplateChanged(oldContentTemplate, newContentTemplate);
        }

        protected override void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
        {
            base.OnContentTemplateSelectorChanged(oldContentTemplateSelector, newContentTemplateSelector);
        }

        protected override void OnContextMenuClosing(ContextMenuEventArgs e)
        {
            base.OnContextMenuClosing(e);
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return base.OnCreateAutomationPeer();
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
        }

        protected override void OnGotMouseCapture(MouseEventArgs e)
        {
            base.OnGotMouseCapture(e);
        }

        protected override void OnGotStylusCapture(StylusEventArgs e)
        {
            base.OnGotStylusCapture(e);
        }

        protected override void OnGotTouchCapture(TouchEventArgs e)
        {
            base.OnGotTouchCapture(e);
        }

        protected override void OnHeaderDragDelta(DragDeltaEventArgs e)
        {
            base.OnHeaderDragDelta(e);
        }

        protected override void OnHeaderIconDoubleClicked(MouseButtonEventArgs e)
        {
            base.OnHeaderIconDoubleClicked(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        protected override void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            base.OnIsActiveChanged(oldValue, newValue);
        }

        protected override void OnIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusedChanged(e);
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
        }

        protected override void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsMouseCapturedChanged(e);
        }

        protected override void OnIsMouseCaptureWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsMouseCaptureWithinChanged(e);
        }

        protected override void OnIsMouseDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsMouseDirectlyOverChanged(e);
        }

        protected override void OnIsStylusCapturedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsStylusCapturedChanged(e);
        }

        protected override void OnIsStylusCaptureWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsStylusCaptureWithinChanged(e);
        }

        protected override void OnIsStylusDirectlyOverChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsStylusDirectlyOverChanged(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
        }

        protected override void OnLeftPropertyChanged(double oldValue, double newValue)
        {
            base.OnLeftPropertyChanged(oldValue, newValue);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
        }

        protected override void OnLostStylusCapture(StylusEventArgs e)
        {
            base.OnLostStylusCapture(e);
        }

        protected override void OnLostTouchCapture(TouchEventArgs e)
        {
            base.OnLostTouchCapture(e);
        }

        protected override void OnManipulationBoundaryFeedback(ManipulationBoundaryFeedbackEventArgs e)
        {
            base.OnManipulationBoundaryFeedback(e);
        }

        protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
        {
            base.OnManipulationCompleted(e);
        }

        protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
        {
            base.OnManipulationDelta(e);
        }

        protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
        {
            base.OnManipulationInertiaStarting(e);
        }

        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            base.OnManipulationStarted(e);
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            base.OnManipulationStarting(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
        }

        protected override void OnPreviewDragEnter(DragEventArgs e)
        {
            base.OnPreviewDragEnter(e);
        }

        protected override void OnPreviewDragLeave(DragEventArgs e)
        {
            base.OnPreviewDragLeave(e);
        }

        protected override void OnPreviewDragOver(DragEventArgs e)
        {
            base.OnPreviewDragOver(e);
        }

        protected override void OnPreviewDrop(DragEventArgs e)
        {
            base.OnPreviewDrop(e);
        }

        protected override void OnPreviewGiveFeedback(GiveFeedbackEventArgs e)
        {
            base.OnPreviewGiveFeedback(e);
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewGotKeyboardFocus(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewLostKeyboardFocus(e);
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDoubleClick(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonUp(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);
        }

        protected override void OnPreviewQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            base.OnPreviewQueryContinueDrag(e);
        }

        protected override void OnPreviewStylusButtonDown(StylusButtonEventArgs e)
        {
            base.OnPreviewStylusButtonDown(e);
        }

        protected override void OnPreviewStylusButtonUp(StylusButtonEventArgs e)
        {
            base.OnPreviewStylusButtonUp(e);
        }

        protected override void OnPreviewStylusDown(StylusDownEventArgs e)
        {
            base.OnPreviewStylusDown(e);
        }

        protected override void OnPreviewStylusInAirMove(StylusEventArgs e)
        {
            base.OnPreviewStylusInAirMove(e);
        }

        protected override void OnPreviewStylusInRange(StylusEventArgs e)
        {
            base.OnPreviewStylusInRange(e);
        }

        protected override void OnPreviewStylusMove(StylusEventArgs e)
        {
            base.OnPreviewStylusMove(e);
        }

        protected override void OnPreviewStylusOutOfRange(StylusEventArgs e)
        {
            base.OnPreviewStylusOutOfRange(e);
        }

        protected override void OnPreviewStylusSystemGesture(StylusSystemGestureEventArgs e)
        {
            base.OnPreviewStylusSystemGesture(e);
        }

        protected override void OnPreviewStylusUp(StylusEventArgs e)
        {
            base.OnPreviewStylusUp(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            base.OnPreviewTouchDown(e);
        }

        protected override void OnPreviewTouchMove(TouchEventArgs e)
        {
            base.OnPreviewTouchMove(e);
        }

        protected override void OnPreviewTouchUp(TouchEventArgs e)
        {
            base.OnPreviewTouchUp(e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        protected override void OnQueryContinueDrag(QueryContinueDragEventArgs e)
        {
            base.OnQueryContinueDrag(e);
        }

        protected override void OnQueryCursor(QueryCursorEventArgs e)
        {
            base.OnQueryCursor(e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnStyleChanged(Style oldStyle, Style newStyle)
        {
            base.OnStyleChanged(oldStyle, newStyle);
        }

        protected override void OnStylusButtonDown(StylusButtonEventArgs e)
        {
            base.OnStylusButtonDown(e);
        }

        protected override void OnStylusButtonUp(StylusButtonEventArgs e)
        {
            base.OnStylusButtonUp(e);
        }

        protected override void OnStylusDown(StylusDownEventArgs e)
        {
            base.OnStylusDown(e);
        }

        protected override void OnStylusEnter(StylusEventArgs e)
        {
            base.OnStylusEnter(e);
        }

        protected override void OnStylusInAirMove(StylusEventArgs e)
        {
            base.OnStylusInAirMove(e);
        }

        protected override void OnStylusInRange(StylusEventArgs e)
        {
            base.OnStylusInRange(e);
        }

        protected override void OnStylusLeave(StylusEventArgs e)
        {
            base.OnStylusLeave(e);
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove(e);
        }

        protected override void OnStylusOutOfRange(StylusEventArgs e)
        {
            base.OnStylusOutOfRange(e);
        }

        protected override void OnStylusSystemGesture(StylusSystemGestureEventArgs e)
        {
            base.OnStylusSystemGesture(e);
        }

        protected override void OnStylusUp(StylusEventArgs e)
        {
            base.OnStylusUp(e);
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            base.OnTemplateChanged(oldTemplate, newTemplate);
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
        }

        protected override void OnToolTipClosing(ToolTipEventArgs e)
        {
            base.OnToolTipClosing(e);
        }

        protected override void OnToolTipOpening(ToolTipEventArgs e)
        {
            base.OnToolTipOpening(e);
        }

        protected override void OnTopPropertyChanged(double oldValue, double newValue)
        {
            base.OnTopPropertyChanged(oldValue, newValue);
        }

        protected override void OnTouchDown(TouchEventArgs e)
        {
            base.OnTouchDown(e);
        }

        protected override void OnTouchEnter(TouchEventArgs e)
        {
            base.OnTouchEnter(e);
        }

        protected override void OnTouchLeave(TouchEventArgs e)
        {
            base.OnTouchLeave(e);
        }

        protected override void OnTouchMove(TouchEventArgs e)
        {
            base.OnTouchMove(e);
        }

        protected override void OnTouchUp(TouchEventArgs e)
        {
            base.OnTouchUp(e);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
        }

        protected override void OnWindowStyleChanged(WindowStyle oldValue, WindowStyle newValue)
        {
            base.OnWindowStyleChanged(oldValue, newValue);
        }

        protected override void ParentLayoutInvalidated(UIElement child)
        {
            base.ParentLayoutInvalidated(child);
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            return base.ShouldSerializeProperty(dp);
        }
    }
}
