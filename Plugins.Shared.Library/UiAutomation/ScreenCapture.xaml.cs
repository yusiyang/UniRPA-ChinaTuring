using Plugins.Shared.Library.Extensions;
using Plugins.Shared.Library.UiAutomation.CaptureEvents;
using Plugins.Shared.Library.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinApi.User32;

namespace Plugins.Shared.Library.UiAutomation
{
    /// <summary>
    /// ScreenCapture.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenCapture : System.Windows.Window
    {
        private double _x;
        private double _y;
        private double _width;
        private double _height;
        private const double RectangeStrokeThickness = 1;

        private bool _isMouseDown = false;

        private System.Windows.Shapes.Rectangle _leftRect = new System.Windows.Shapes.Rectangle();

        private System.Windows.Shapes.Rectangle _rightRect = new System.Windows.Shapes.Rectangle();

        private System.Windows.Shapes.Rectangle _topRect = new System.Windows.Shapes.Rectangle();

        private System.Windows.Shapes.Rectangle _bottomRect = new System.Windows.Shapes.Rectangle();

        public event CapturedEventHandler Captured;

        public event CaptureCanceledEventHandler CaptureCanceled;

        public ScreenCapture(string screenshotDirectory, string screenshotFileName) : this()
        {
            ScreenshotDirectory = screenshotDirectory;
            ScreenshotFileName = screenshotFileName;
        }

        public ScreenCapture()
        {
            var rect = System.Windows.Forms.SystemInformation.VirtualScreen;
            Left = rect.X;
            Top = rect.Y;
            Width = rect.Width;
            Height = rect.Height;
            InitializeComponent();
            SetBackground();

            _leftRect.Width = Width;
            _leftRect.Height = Height;
            Canvas.SetLeft(_leftRect, 0);
            Canvas.SetTop(_leftRect, 0);
            _leftRect.Opacity = 0.3;
            _leftRect.Fill = System.Windows.Media.Brushes.LightGray;

            captureCanvas.Children.Add(_leftRect);

            InitRectFill();
        }

        private void InitRectFill()
        {
            _rightRect.Opacity = 0.3;
            _rightRect.Fill = System.Windows.Media.Brushes.LightGray;

            _topRect.Opacity = 0.3;
            _topRect.Fill = System.Windows.Media.Brushes.LightGray;

            _bottomRect.Opacity = 0.3;
            _bottomRect.Fill = System.Windows.Media.Brushes.LightGray;
        }

        private void SetBackground()
        {
            var imageSource = GetImageSource();
            imageSource.Freeze();
            ImageBrush brush = new ImageBrush(imageSource);
            this.Background = brush;
        }

        private BitmapSource GetImageSource()
        {
            Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
            {
                memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }

            var imageSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return imageSource;
        }

        private NetCoreEx.Geometry.Point _screenPoint;
        private int ScreenX
        {
            get
            {
                if (_screenPoint.IsEmpty)
                {
                    // 小于零时，特别处理
                    if (_x == 0 && _y == 0)
                    {
                        return 0;
                    }
                    var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                    _screenPoint = new NetCoreEx.Geometry.Point((int)_x, (int)_y);
                    User32Methods.ClientToScreen(hwndSource.Handle, ref _screenPoint);
                }
                return _screenPoint.X;
            }
        }

        private int ScreenY
        {
            get
            {
                if (_screenPoint.IsEmpty)
                {
                    // 小于零时，特别处理
                    if (_x == 0 && _y == 0)
                    {
                        return 0;
                    }
                    var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                    _screenPoint = new NetCoreEx.Geometry.Point((int)_x, (int)_y);
                    User32Methods.ClientToScreen(hwndSource.Handle, ref _screenPoint);
                }
                return _screenPoint.Y;
            }
        }

        private string _screenshotDirectory;
        public string ScreenshotDirectory
        {
            get
            {
                if (!_screenshotDirectory.IsNullOrWhiteSpace())
                {
                    return _screenshotDirectory;
                }
                _screenshotDirectory = SharedObject.Instance.ProjectPath + @"\.screenshots";
                if (!System.IO.Directory.Exists(_screenshotDirectory))
                {
                    System.IO.Directory.CreateDirectory(_screenshotDirectory);
                }
                return _screenshotDirectory;
            }
            private set
            {
                _screenshotDirectory = value;
            }
        }

        private string _screenshotFileName;
        public string ScreenshotFileName
        {
            get
            {
                if (!_screenshotFileName.IsNullOrWhiteSpace())
                {
                    return _screenshotFileName;
                }
                _screenshotFileName = $"{Guid.NewGuid().ToString("N")}.bmp";

                return _screenshotFileName;
            }
            private set
            {
                _screenshotFileName = value;
            }
        }

        public string ScreenshotFullName => System.IO.Path.Combine(ScreenshotDirectory, ScreenshotFileName);

        private void CaptureWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                this.Closed += OnCaptureCanceled;
                this.Close();
                return;
            }
            _isMouseDown = true;
            var cursorPosition = e.GetPosition(this);
            _x = cursorPosition.X;
            _y = cursorPosition.Y;
        }

        private void CaptureWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                var cursorPosition = e.GetPosition(null);

                double dx = cursorPosition.X;
                double dy = cursorPosition.Y;

                _width = Math.Abs(dx - _x);
                _height = Math.Abs(dy - _y);

                #region  通过一个矩形来表示目前截图区域
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                double rectWidth = _width + 2 * RectangeStrokeThickness;
                double rectHeight = _height + 2 * RectangeStrokeThickness;

                rect.Width = rectWidth;
                rect.Height = rectHeight;

                var strokeBrush = new SolidColorBrush(Colors.Red);
                rect.Stroke = strokeBrush;
                rect.StrokeThickness = RectangeStrokeThickness;
                rect.Fill = System.Windows.Media.Brushes.Transparent;

                if (dx < _x)
                {
                    Canvas.SetLeft(rect, dx - RectangeStrokeThickness);
                }
                else
                {
                    Canvas.SetLeft(rect, _x - RectangeStrokeThickness);
                }
                if (dy < _y)
                {
                    Canvas.SetTop(rect, dy - RectangeStrokeThickness);
                }
                else
                {
                    Canvas.SetTop(rect, _y - RectangeStrokeThickness);
                }

                SetFourRects(rect);

                captureCanvas.Children.Clear();
                captureCanvas.Children.Add(rect);
                captureCanvas.Children.Add(_leftRect);
                captureCanvas.Children.Add(_rightRect);
                captureCanvas.Children.Add(_topRect);
                captureCanvas.Children.Add(_bottomRect);
                #endregion

                if (e.LeftButton == MouseButtonState.Released)
                {
                    _x = dx < _x ? dx : _x;
                    _y = dy < _y ? dy : _y;
                    captureCanvas.Children.Clear();
                    CaptureScreen();

                    _isMouseDown = false;

                    this.Closed += OnCaptured;
                    this.Close();
                }
            }
        }

        private void SetFourRects(System.Windows.Shapes.Rectangle screenshotRect)
        {
            var screenshotLeft = Canvas.GetLeft(screenshotRect);
            var screenshotTop = Canvas.GetTop(screenshotRect);

            // 小于零时，特别处理
            screenshotLeft = screenshotLeft < 0 ? 0 : screenshotLeft;
            screenshotTop = screenshotTop < 0 ? 0 : screenshotTop;

            Canvas.SetLeft(_leftRect, 0);
            Canvas.SetTop(_leftRect, 0);
            _leftRect.Height = Height;
            _leftRect.Width = screenshotLeft - Left;

            Canvas.SetLeft(_topRect, screenshotLeft);
            Canvas.SetTop(_topRect, 0);
            _topRect.Height = screenshotTop;
            _topRect.Width = screenshotRect.Width;

            Canvas.SetLeft(_rightRect, screenshotLeft + screenshotRect.Width);
            Canvas.SetTop(_rightRect, 0);
            _rightRect.Height = Height;
            _rightRect.Width = Width - (screenshotLeft + screenshotRect.Width);

            Canvas.SetLeft(_bottomRect, screenshotLeft);
            Canvas.SetTop(_bottomRect, screenshotTop + screenshotRect.Height);
            _bottomRect.Height = Height - (screenshotTop + screenshotRect.Height);
            _bottomRect.Width = screenshotRect.Width;
        }

        private void CaptureScreen()
        {
            int iw = Convert.ToInt32(_width);
            int ih = Convert.ToInt32(_height);

            // 小于零时，特别处理
            Bitmap bitmap = new Bitmap(iw <= 0 ? 1 : iw, ih <= 0 ? 1 : ih);
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(ScreenX, ScreenY, 0, 0, new System.Drawing.Size(iw, ih));

                bitmap.Save(ScreenshotFullName, ImageFormat.Bmp);
            }
        }

        private void OnCaptured(object sender, EventArgs e)
        {
            var capturedEventArgs = new CapturedEventArgs(ScreenX, ScreenY, _width, _height, ScreenshotFileName);
            Captured?.Invoke(this, capturedEventArgs);
        }

        private void OnCaptureCanceled(object sender, EventArgs e)
        {
            var eventArgs = new CaptureCanceledEventArgs();
            CaptureCanceled?.Invoke(this, eventArgs);
        }

        private void CaptureWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Closed += OnCaptureCanceled;
                Close();
            }
            else if (e.Key == Key.F2)
            {
                this.Hide();
                var countDown = new CountDown(5);
                countDown.Closed += CountDown_Closed;
                countDown.Show();
            }
        }

        private void CountDown_Closed(object sender, EventArgs e)
        {
            SetBackground();
            this.Show();
            WindowsHelper.SetForeground(this);
        }
    }
}
