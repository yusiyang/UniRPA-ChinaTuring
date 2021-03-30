using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace UniStudio.Community.UserControls
{
    /// <summary>
    /// SearchTextBoxControl.xaml 的交互逻辑
    /// </summary>
    public partial class SearchTextBoxControl : UserControl
    {


        public string HintText
        {
            get { return (string)GetValue(HintTextProperty); }
            set { SetValue(HintTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HintText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HintTextProperty =
            DependencyProperty.Register("HintText", typeof(string), typeof(SearchTextBoxControl), new PropertyMetadata("请输入搜索内容"));




        public string ClearToolTipText
        {
            get { return (string)GetValue(ClearToolTipTextProperty); }
            set { SetValue(ClearToolTipTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClearToolTipText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClearToolTipTextProperty =
            DependencyProperty.Register("ClearToolTipText", typeof(string), typeof(SearchTextBoxControl), new PropertyMetadata("清除搜索内容"));




        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty,value);}
        }

        // Using a DependencyProperty as the backing store for SearchText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SearchTextBoxControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ICommand SearchClearCommand
        {
            get { return (ICommand)GetValue(SearchClearCommandProperty); }
            set { SetValue(SearchClearCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchClearCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchClearCommandProperty =
            DependencyProperty.Register("SearchClearCommand", typeof(ICommand), typeof(SearchTextBoxControl), new PropertyMetadata(null));



        #region 全局主题配色
        private static SolidColorBrush _searchTextBoxBackground;
        public static SolidColorBrush SearchTextBoxBackground
        {
            get
            {
                return _searchTextBoxBackground;
            }
            set
            {
                if (_searchTextBoxBackground == value)
                {
                    return;
                }

                _searchTextBoxBackground = value;
            }
        }
        #endregion



        public SearchTextBoxControl()
        {
            InitializeComponent();
        }

        private void SearchClearBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchText = "";
            if(SearchClearCommand != null)
            {
                SearchClearCommand.Execute(null);
            }
        }

        private void WatermarkTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(sender, e);
            if (string.IsNullOrEmpty(SearchText))
            {
                searchClearBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                searchClearBtn.Visibility = Visibility.Visible;
            }
        }

        public static event TextChangedEventHandler TextChanged;
    }
}
