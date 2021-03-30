using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library.Extensions;
using System.Windows;
using UniWorkforce.Models;
using UniWorkforce.Models.MessageModels;
using UniWorkforce.Services;
using UniWorkforce.Windows;

namespace UniWorkforce.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        public Window View { get; set; }
        public RobotService RobotService => Context.Current.RobotService;

        /// <summary>
        /// Initializes a new instance of the RegisterViewModel class.
        /// </summary>
        public LoginViewModel()
        {
        }

        /// <summary>
        /// The <see cref="LoginName" /> property's name.
        /// </summary>
        public const string LoginNamePropertyName = "LoginName";

        private string _loginName;

        /// <summary>
        /// Sets and gets the LoginName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string LoginName
        {
            get
            {
                return _loginName;
            }

            set
            {
                if (_loginName == value)
                {
                    return;
                }

                _loginName = value;
                RaisePropertyChanged(LoginNamePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Password" /> property's name.
        /// </summary>
        public const string PasswordPropertyName = "Password";

        private string _password;

        /// <summary>
        /// Sets and gets the Password property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                if (_password == value)
                {
                    return;
                }

                _password = value;
                RaisePropertyChanged(PasswordPropertyName);
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
                        Init();
                    }));
            }
        }

        private void Init()
        {
            LoginName = string.Empty;
            Password = string.Empty;
            IsLogined = false;
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

        public bool IsLogined { get; private set; }

        private RelayCommand _loginCommand;

        /// <summary>
        /// Gets the ConnectCommand.
        /// </summary>
        public RelayCommand LoginCommand
        {
            get
            {
                return _loginCommand
                    ?? (_loginCommand = new RelayCommand(
                    () =>
                    {
                        if(!Context.Current.ConnectedToController)
                        {
                            UniMessageBox.Show(App.Current.MainWindow, "请先连接控制器", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        #region 数据验证
                        if (string.IsNullOrWhiteSpace(LoginName))
                        {
                            UniMessageBox.Show(App.Current.MainWindow, "用户名不能为空", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        var loginWindow = View as LoginWindow;
                        if (string.IsNullOrWhiteSpace(loginWindow.PasswordBox.Password))
                        {
                            UniMessageBox.Show(App.Current.MainWindow, "密码不能为空", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        #endregion

                        //var request = new LoginRequest
                        //{
                        //    Code = "4r11aa2q9texzfx9omqcysu2",
                        //    LoginName = LoginName,
                        //    Password = loginWindow.PasswordBox.Password
                        //};
                        //var response= RobotService.DesignerLogin(request);

                        var request = new LoginRequestModel
                        {
                            UserName = LoginName,
                            Password = loginWindow.PasswordBox.Password
                        };
                        LoginResponseModel response;
                        try
                        {
                           response = RobotService.Login(request);
                            if (response.Code != 200)
                            {
                                UniMessageBox.Show(App.Current.MainWindow, response.Message, "连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                        }
                        catch(System.Exception ex)
                        {
                            UniMessageBox.Show(App.Current.MainWindow, ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        IsLogined = true;

                        var loggedInMessage = new LoggedInMessage(response.Data);
                        Messenger.Default.Send(loggedInMessage, "LoggedIn");

                        CloseCommand.Execute(null);
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