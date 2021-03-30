using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;

namespace UniStudio.Community.ViewModel
{
    public class ExtractDataTableViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Window m_view;

        public ExtractDataTableViewModel()
        {

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


    }
}
