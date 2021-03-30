using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using System;
using UniWorkforce.Librarys;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.IO;
using log4net;
using Plugins.Shared.Library.Librarys;
using System.Security.Cryptography;
using CSharp_easy_RSA_PEM;
using Plugins.Shared.Library.Extensions;

namespace UniWorkforce.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class RegisterViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window _view;

        /// <summary>
        /// Initializes a new instance of the RegisterViewModel class.
        /// </summary>
        public RegisterViewModel()
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
                        _view = (Window)p.Source;
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
                        _view.DragMove();
                    }));
            }
        }



        private RelayCommand<System.ComponentModel.CancelEventArgs> _closingCommand;

        /// <summary>
        /// Gets the ClosingCommand.
        /// </summary>
        public RelayCommand<System.ComponentModel.CancelEventArgs> ClosingCommand
        {
            get
            {
                return _closingCommand
                    ?? (_closingCommand = new RelayCommand<System.ComponentModel.CancelEventArgs>(
                    e =>
                    {
                        e.Cancel = true;//不关闭窗口
                        _view.Hide();
                    }));
            }
        }

        public void LoadRegisterInfo()
        {
            //加载注册信息
            Task.Run(()=> {
                //异步加载，避免卡顿
                bool isRegistered = false;
                string expiresDate = "";
                GetRegisterInfo(ref isRegistered, ref expiresDate);

                Librarys.Common.RunInUI(()=> {
                    IsRegistered = isRegistered;
                    ExpiresDate = expiresDate;

                    IsNeverExpires = expiresDate == "forever" ? true : false;

                    ViewModelLocator.instance.Startup.RefreshProgramStatus(IsRegistered);
                });
            });
                      
        }



        /// <summary>
        /// The <see cref="IsRegistered" /> property's name.
        /// </summary>
        public const string IsRegisteredPropertyName = "IsRegistered";

        private bool _isRegisteredProperty = false;

        /// <summary>
        /// Sets and gets the IsRegistered property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsRegistered
        {
            get
            {
                return _isRegisteredProperty;
            }

            set
            {
                if (_isRegisteredProperty == value)
                {
                    return;
                }

                _isRegisteredProperty = value;
                RaisePropertyChanged(IsRegisteredPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsNeverExpires" /> property's name.
        /// </summary>
        public const string IsNeverExpiresPropertyName = "IsNeverExpires";

        private bool _isNeverExpiresProperty = false;

        /// <summary>
        /// Sets and gets the IsNeverExpires property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNeverExpires
        {
            get
            {
                return _isNeverExpiresProperty;
            }

            set
            {
                if (_isNeverExpiresProperty == value)
                {
                    return;
                }

                _isNeverExpiresProperty = value;
                RaisePropertyChanged(IsNeverExpiresPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ExpiresDate" /> property's name.
        /// </summary>
        public const string ExpiresDatePropertyName = "ExpiresDate";

        private string _expiresDateProperty = "";

        /// <summary>
        /// Sets and gets the ExpiresDate property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ExpiresDate
        {
            get
            {
                return _expiresDateProperty;
            }

            set
            {
                if (_expiresDateProperty == value)
                {
                    return;
                }

                _expiresDateProperty = value;
                RaisePropertyChanged(ExpiresDatePropertyName);
            }
        }


        private RelayCommand _exportMachineCodeFileCommand;

        /// <summary>
        /// Gets the ExportMachineCodeFileCommand.
        /// </summary>
        public RelayCommand ExportMachineCodeFileCommand
        {
            get
            {
                return _exportMachineCodeFileCommand
                    ?? (_exportMachineCodeFileCommand = new RelayCommand(
                    () =>
                    {
                        //导出机器码
                        var d = DateTime.Now;
                        var fileName = $"机器码({d.Year}-{d.Month:D2}-{d.Day:D2} {d.Hour:D2}：{d.Minute:D2}：{d.Second:D2})";

                        //选择待保存的目录
                        string userSelPath;
                        bool ret = Librarys.Common.ShowSaveAsFileDialog(out userSelPath, fileName, ".machine", "机器码文件");

                        if (ret == true)
                        {
                            //生成机器信息JSON格式数据
                            try
                            {
                                var secretStr = Librarys.Common.GetComputerSecret();
                                //保存文件
                                System.IO.File.WriteAllText(userSelPath, secretStr);

                                var result = UniMessageBox.Show(_view, "导出机器码成功，是否定位到该机器码文件？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                                if(result == MessageBoxResult.Yes)
                                {
                                    Librarys.Common.LocateFileInExplorer(userSelPath);
                                }
                            }
                            catch (Exception)
                            {
                                UniMessageBox.Show(_view, "导出机器码成功失败，请检查", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            
                        }
                        
                    }));
            }
        }

        public void GetRegisterInfo(ref bool isRegistered, ref string expiresDate)
        {
            if (IsNotExpired(ref expiresDate))
            {
                isRegistered = true;
            }
            else
            {
                isRegistered = false;
                expiresDate = "";
            }
        }

        public bool IsNotExpired()
        {
            string expiresDate = "";
            return IsNotExpired(ref expiresDate);
        }

        public bool IsNotExpired(ref string expiresDate)
        {
            var fileFullPath = Settings.Instance.LocalRPAStudioDir + @"\Authorization\license.authorization";

            return IsNotExpired(fileFullPath,ref expiresDate);
        }


        public bool IsNotExpired(string fileFullPath,ref string expiresDate)
        {
            bool isNotExpired = false;

            if (!System.IO.File.Exists(fileFullPath))
            {
                return isNotExpired;
            }

            if (Plugins.Shared.Library.Librarys.Common.CheckAuthorization(fileFullPath, Properties.Resources.verify_public_rsa, ref expiresDate))
            {
                //授权合法，检查下有效期
                if (expiresDate == "forever")
                {
                    isNotExpired = true;
                }
                else
                {
                    DateTime current = DateTime.Now;
                    DateTime deadline = Convert.ToDateTime(expiresDate).AddDays(1);//截止日期得再加上一天，因为从当天00:00:00截止
                    if (current.CompareTo(deadline) < 0)
                    {
                        isNotExpired = true;
                    }
                }
            }

            return isNotExpired;
        }

        private RelayCommand _registerCommand;

        /// <summary>
        /// Gets the RegisterCommand.
        /// </summary>
        public RelayCommand RegisterCommand
        {
            get
            {
                return _registerCommand
                    ?? (_registerCommand = new RelayCommand(
                    () =>
                    {
                        LisenceHelper.Instance.GenerateLicense();
                        LoadRegisterInfo();
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
                        _view.Close();
                    }));
            }
        }        

    }
}