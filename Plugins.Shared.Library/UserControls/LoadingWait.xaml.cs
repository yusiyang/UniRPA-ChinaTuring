using Plugins.Shared.Library.Librarys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Plugins.Shared.Library.UserControls
{
    /// <summary>
    /// LoadingWait.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingWait : UserControl,IDisposable
    {
        #region Data
        private readonly DispatcherTimer _animationTimer;
        #endregion

        #region Constructor
        public LoadingWait()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer(
                DispatcherPriority.ContextIdle, Dispatcher);
            _animationTimer.Interval = new TimeSpan(0, 0, 0, 0, 90);
        }
        #endregion

        #region Private Methods
        private void Start()
        {
            _animationTimer.Tick += HandleAnimationTick;
            _animationTimer.Start();
        }

        private void Stop()
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= HandleAnimationTick;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            const double offset = Math.PI;
            const double step = Math.PI * 2 / 10.0;

            SetPosition(C0, offset, 0.0, step);
            SetPosition(C1, offset, 1.0, step);
            SetPosition(C2, offset, 2.0, step);
            SetPosition(C3, offset, 3.0, step);
            SetPosition(C4, offset, 4.0, step);
            SetPosition(C5, offset, 5.0, step);
            SetPosition(C6, offset, 6.0, step);
            SetPosition(C7, offset, 7.0, step);
            SetPosition(C8, offset, 8.0, step);
        }

        private void SetPosition(Ellipse ellipse, double offset,
            double posOffSet, double step)
        {
            ellipse.SetValue(Canvas.LeftProperty, 50.0
                + Math.Sin(offset + posOffSet * step) * 50.0);

            ellipse.SetValue(Canvas.TopProperty, 50
                + Math.Cos(offset + posOffSet * step) * 50.0);
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void HandleVisibleChanged(object sender,
            DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;

            if (isVisible)
                Start();
            else
                Stop();
        }

        #endregion

        #region Public Methods

        public static LoadingWait Show(Action action=null,Thickness margin=default)
        {
            var owerWindow = Common.GetOwnerWindow();
            if (owerWindow == null)
            {
                throw new InvalidOperationException("没有父窗口");
            }

            var container = owerWindow.Content as UIElement;
            var grid = container as Grid;
            UIElement parent;
            if (grid == null || grid.RowDefinitions.Count > 0)
            {
                var parentGrid = new Grid();
                owerWindow.Content = parentGrid;
                parentGrid.Children.Add(container);
                parent = parentGrid;
            }
            else
            {
                parent = container;
            }
            return Show(parent, action,margin);
        }

        public static LoadingWait Show(UIElement parent, Action action=null, Thickness margin = default)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            var addChild = parent as IAddChild;
            if (addChild == null)
            {
                throw new ArgumentException($"parent不是IAddChild类型");
            }

            var loadingWait = VisualTreeHelperEx.FindDescendantByType<LoadingWait>(parent);
            if (loadingWait == null)
            {
                loadingWait = new LoadingWait();
                loadingWait.Margin = margin;
                addChild.AddChild(loadingWait);
            }
            loadingWait.Visibility = Visibility.Visible;

            action?.BeginInvoke(ar =>
            {
                Common.RunInUI(() =>
                {
                    loadingWait.Visibility = Visibility.Collapsed;
                });
            }, null);

            return loadingWait;
        }

        public void Dispose()
        {
            if(this.Visibility==Visibility.Visible)
            {
                this.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}
