/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:UniStudio"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System.IO;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;

namespace UniStudio.Community.ViewModel
{
    public class ManagePackagesViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Window m_view;

        public ManagePackagesViewModel()
        {

        }


        public const string SourceNameTextBoxPropertyName = "SourceNameTextBox";
        private string _sourceNameTextBoxProperty = "";
        public string SourceNameTextBox
        {
            get
            {
                return _sourceNameTextBoxProperty;
            }
            set
            {
                if (_sourceNameTextBoxProperty == value)
                {
                    return;
                }

                _sourceNameTextBoxProperty = value;
                RaisePropertyChanged(SourceNameTextBoxPropertyName);
            }
        }


        public const string SourcePathTextBoxPropertyName = "SourcePathTextBox";
        private string _sourcePathTextBoxProperty = "";
        public string SourcePathTextBox
        {
            get
            {
                return _sourcePathTextBoxProperty;
            }
            set
            {
                if (_sourcePathTextBoxProperty == value)
                {
                    return;
                }

                _sourcePathTextBoxProperty = value;
                RaisePropertyChanged(SourcePathTextBoxPropertyName);
            }
        }


        private RelayCommand<RoutedEventArgs> _loadedCommand;
        /// <summary>
        /// Gets the LoadedCommand.
        /// </summary>
        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        m_view = (Window)p.Source;

                        Init();
                    }));
            }
        }

        private void Init()
        {
            //初始化相关信息
        }


        private RelayCommand _selectPackagePathCommand;
        /// <summary>
        /// 选择包源路径
        /// </summary>
        public RelayCommand SelectPackagePathCommand
        {
            get
            {
                return _selectPackagePathCommand
                    ?? (_selectPackagePathCommand = new RelayCommand(
                        () =>
                        {
                            var dialog = new System.Windows.Forms.FolderBrowserDialog();
                            var dialogResult = dialog.ShowDialog();

                            if (dialogResult == System.Windows.Forms.DialogResult.Cancel)
                            {
                                return;
                            }

                            var dirInfo = new DirectoryInfo(dialog.SelectedPath.Trim());
                            if (dialogResult == System.Windows.Forms.DialogResult.OK)
                            {
                                SourcePathTextBox = dirInfo.FullName;
                                return;
                            }
                        }));
            }
        }


    }
}