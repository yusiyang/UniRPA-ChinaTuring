using ActiproSoftware.Windows.Controls.Ribbon;
using System.Windows;

namespace UniUpdater
{
    /// <summary>
    /// UpgradeInfo.xaml 的交互逻辑
    /// </summary>
    public partial class UpgradeInfo : RibbonWindow
    {
        public static DependencyProperty ProgressValueProperty;

        public int ProgressValue
        {
            get
            {
                return (int)GetValue(ProgressValueProperty);
            }
            set
            {
                SetValue(ProgressValueProperty, value);
            }
        }

        public static DependencyProperty DescriptionProperty;

        public string Description
        {
            get
            {
                return (string)GetValue(DescriptionProperty);
            }
            set
            {
                SetValue(DescriptionProperty, value);
            }
        }

        static UpgradeInfo()
        {
            ProgressValueProperty = DependencyProperty.Register("ProgressValue", typeof(int), typeof(UpgradeInfo), new FrameworkPropertyMetadata());
            DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(UpgradeInfo), new FrameworkPropertyMetadata("开始更新……"));
        }

        public UpgradeInfo()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
