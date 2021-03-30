using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System;
using UniWorkforce.Librarys;
using System.Threading.Tasks;
using UniWorkforce.Services;
using UniWorkforce.Config;
using UniStudio.Community.Windows;
using UniStudio.Community.ProcessOperation;
using Plugins.Shared.Library.Extensions;

namespace UniStudio.Community.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class LoginViewModel : ViewModelBase
    {
        private string _userName;
        public string UserName { get { return _userName; } set { Set(ref _userName, value); } }


        private string _password;
        public string PassWord { get { return _password; } set { Set(ref _password, value); } }


        private bool _isClosed = false;
        public bool IsClosed { get { return _isClosed; } set { Set(ref _isClosed, value); } }

        private bool _isLogined = false;
        public bool IsLogined { get { return _isLogined; } set { Set(ref _isLogined, value); } }

        public LoginViewModel()
        {
        }



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
                        //var a = PassWord;
                        //IProcessService processService = new ProcessService();
                        //#region 数据验证
                        //if (string.IsNullOrWhiteSpace(LoginName))
                        //{
                        //    UniMessageBox.Show(App.Current.MainWindow, "用户名不能为空", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        //    return;
                        //}
                        //var loginWindow = View as LoginWindow;
                        //if (string.IsNullOrWhiteSpace(loginWindow.PasswordBox.Password))
                        //{
                        //    UniMessageBox.Show(App.Current.MainWindow, "密码不能为空", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        //    return;
                        //}
                        //#endregion

                        //var result= processService.Login(LoginName, loginWindow.PasswordBox.Password);
                        //if(!result.IsSucess)
                        //{
                        //    UniMessageBox.Show(App.Current.MainWindow, result.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                        //    return;
                        //}
                        //IsLogined = true;
                        //CloseCommand.Execute(null);
                    }));
            }
        }
    }
}