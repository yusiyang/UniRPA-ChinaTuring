using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System;
using UniWorkforce.Librarys;
using System.Threading.Tasks;
using UniWorkforce.Services;
using UniWorkforce.Config;
using GalaSoft.MvvmLight.Messaging;
using UniWorkforce.Models.MessageModels;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.UserControls;
using log4net;
using Plugins.Shared.Library.Extensions;

namespace UniWorkforce.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ConnectToControllerViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Window View { get; set; }
        
        public RobotService RobotService => Context.Current.RobotService;

        /// <summary>
        /// Initializes a new instance of the RegisterViewModel class.
        /// </summary>
        public ConnectToControllerViewModel()
        {
        }

        /// <summary>
        /// The <see cref="RotbotUniqueNo" /> property's name.
        /// </summary>
        public const string RotbotUniqueNoPropertyName = "RotbotUniqueNo";

        private string _robotUniqueNo;

        /// <summary>
        /// Sets and gets the RotbotUniqueNo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string RotbotUniqueNo
        {
            get
            {
                return _robotUniqueNo;
            }

            set
            {
                if (_robotUniqueNo == value)
                {
                    return;
                }

                _robotUniqueNo = value;
                RaisePropertyChanged(RotbotUniqueNoPropertyName);
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
                        View = (Window)p.Source;
                        if(!string.IsNullOrWhiteSpace(ControllerSettings.Instance.RobotUniqueNo))
                        {
                            RotbotUniqueNo = ControllerSettings.Instance.RobotUniqueNo;
                        }
                    }));
            }
        }


        private RelayCommand _MouseLeftButtonDownCommand;

        /// <summary>
        /// Gets the MouseLeftButtonDownCommand.
        /// </summary>
        public RelayCommand MouseLeftButtonDownCommand
        {
            get
            {
                return _MouseLeftButtonDownCommand
                    ?? (_MouseLeftButtonDownCommand = new RelayCommand(
                    () =>
                    {
                        //点标题外的部分也能拖动，方便使用
                        View.DragMove();
                    }));
            }
        }

        private RelayCommand _connectCommand;

        /// <summary>
        /// Gets the ConnectCommand.
        /// </summary>
        public RelayCommand ConnectCommand
        {
            get
            {
                return _connectCommand
                    ?? (_connectCommand = new RelayCommand(
                    async () =>
                    {
                        using (var loadingWait = LoadingWait.Show())
                        {
                            try
                            {
                                if (ViewModelLocator.instance.Register.IsNotExpired())
                                {
                                    if (string.IsNullOrWhiteSpace(RotbotUniqueNo))
                                    {
                                        MessageBoxHelper.Show("请填写唯一码！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                    Context.Current.RobotUniqueNo = RotbotUniqueNo;

                                    var response = await RobotService.CheckRobotAsync();
                                    if (response.Code == ResultCode.SuccessCode)
                                    {
                                        Messenger.Default.Send(new ConnectedToControllerMessage(response.Result.RobotId), "ConnectedToController");
                                        CloseCommand.Execute(null);
                                        return;
                                    }
                                    if (response.Code == ResultCode.UnAuthorizedCode)
                                    {
                                        MessageBoxHelper.Show("未授权", "连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                    UniMessageBox.Show(App.Current.MainWindow, response.Code + " " + response.Message, "连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                                else
                                {
                                    MessageBoxHelper.Show("请先注册产品后再连接", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                            catch (Exception ex)
                            {
                                UniMessageBox.Show(App.Current.MainWindow, ex.Message, "连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                Logger.Error(ex, logger);
                            }
                        }
                    }));
            }
        }

        private RelayCommand _closeCommand;

        /// <summary>
        /// Gets the CloseCommand.
        /// </summary>
        public RelayCommand CloseCommand
        {
            get
            {
                return _closeCommand
                    ?? (_closeCommand = new RelayCommand(
                    () =>
                    {
                        View.Close();
                    }));
            }
        }       
    }
}