using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.Xml;
using NuGet;
using System.IO;
using UniWorkforce.Librarys;
using Plugins.Shared.Library.Librarys;
using System.Collections.Generic;
using UniWorkforce.Services;
using Google.Protobuf;
using UniWorkforce.Windows;
using System.Linq;
using Plugins.Shared.Library.UserControls;
using Plugins.Shared.Library.Extensions;

namespace UniWorkforce.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class PublishMonitorProcessViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Window _view;

        /// <summary>
        /// Initializes a new instance of the PublishProjectViewModel class.
        /// </summary>
        public PublishMonitorProcessViewModel()
        {
        }


        private string _tempFileDir = null;

        public string TempFileDir
        {
            get
            {
                if (_tempFileDir == null)
                {
                    _tempFileDir = Path.Combine(Settings.Instance.LocalRPAStudioDir, "tempFiles");
                    if (!Directory.Exists(_tempFileDir))
                    {
                        Directory.CreateDirectory(_tempFileDir);
                    }
                }
                return _tempFileDir;
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
                        _view = (Window)p.Source;
                    }));
            }
        }

        private bool _isReadyToController = false;

        /// <summary>
        /// Sets and gets the IsPublishToController property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsReadyToController
        {
            get
            {
                return _isReadyToController;
            }

            set
            {
                if (_isReadyToController != value)
                {
                    _isReadyToController = value;
                    OkCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// The <see cref="ReleaseNotes" /> property's name.
        /// </summary>
        public const string ReleaseNotesPropertyName = "ReleaseNotes";

        private string _releaseNotesProperty = "";

        /// <summary>
        /// Sets and gets the ReleaseNotes property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ReleaseNotes
        {
            get
            {
                return _releaseNotesProperty;
            }

            set
            {
                if (_releaseNotesProperty == value)
                {
                    return;
                }

                _releaseNotesProperty = value;
                RaisePropertyChanged(ReleaseNotesPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="ProcessFile" /> property's name.
        /// </summary>
        public const string ProcessFilePropertyName = "ProcessFile";

        private string _processFileProperty = "";

        /// <summary>
        /// Sets and gets the ProcessFile property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProcessFile
        {
            get
            {
                return _processFileProperty;
            }

            set
            {
                if (_processFileProperty == value)
                {
                    return;
                }

                _processFileProperty = value;
                RaisePropertyChanged(ProcessFilePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FileName" /> property's name.
        /// </summary>
        public const string FileNamePropertyName = "FileName";

        private string _fileNameProperty = "";

        /// <summary>
        /// Sets and gets the FileName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileNameProperty;
            }

            set
            {
                if (_fileNameProperty == value)
                {
                    return;
                }

                _fileNameProperty = value;
                RaisePropertyChanged(FileNamePropertyName);

                CheckProcessCommand.Execute(null);
            }
        }

        /// <summary>
        /// The <see cref="LoadingWaitVisible" /> property's name.
        /// </summary>
        public const string LoadingWaitVisiblePropertyName = "LoadingWaitVisible";

        private bool _loadingWaitVisible = false;

        /// <summary>
        /// Sets and gets the LoadingWaitVisible property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool LoadingWaitVisible
        {
            get
            {
                return _loadingWaitVisible;
            }

            set
            {
                if (_loadingWaitVisible == value)
                {
                    return;
                }

                _loadingWaitVisible = value;
                RaisePropertyChanged(LoadingWaitVisiblePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="RobotTypes" /> property's name.
        /// </summary>
        public const string RobotTypesPropertyName = "RobotTypes";

        private ObservableCollection<RobotTypeModel> _robotTypesProperty = new ObservableCollection<RobotTypeModel>();

        /// <summary>
        /// Sets and gets the RobotTypes property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<RobotTypeModel> RobotTypes
        {
            get
            {
                return _robotTypesProperty;
            }

            set
            {
                if (_robotTypesProperty == value)
                {
                    return;
                }

                _robotTypesProperty = value;
                RaisePropertyChanged(RobotTypesPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="SelectedRobotType" /> property's name.
        /// </summary>
        public const string SelectedRobotTypePropertyName = "SelectedRobotType";

        private RobotTypeModel _selectedRobotTypeProperty;

        /// <summary>
        /// Sets and gets the SelectedRobotType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public RobotTypeModel SelectedRobotType
        {
            get
            {
                return _selectedRobotTypeProperty;
            }

            set
            {
                if (_selectedRobotTypeProperty == value)
                {
                    return;
                }

                _selectedRobotTypeProperty = value;
                RaisePropertyChanged(SelectedRobotTypePropertyName);
            }
        }

        private RelayCommand _browserProcessCommand;

        /// <summary>
        /// Gets the BrowserFolderCommand.
        /// </summary>
        public RelayCommand BrowserProcessCommand
        {
            get
            {
                return _browserProcessCommand
                    ?? (_browserProcessCommand = new RelayCommand(
                    () =>
                    {
                        ProcessFile = Librarys.Common.ShowSelectSingleFileDialog(null, "选择流程");
                        OnPublishingToController();
                    }));
            }
        }

        /// <summary>
        /// The <see cref="NewProjectVersion" /> property's name.
        /// </summary>
        public const string NewProjectVersionPropertyName = "NewProjectVersion";

        private string _newProjectVersionProperty = "";

        /// <summary>
        /// Sets and gets the NewProjectVersion property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string NewProjectVersion
        {
            get
            {
                return _newProjectVersionProperty;
            }

            set
            {
                if (_newProjectVersionProperty == value)
                {
                    return;
                }

                _newProjectVersionProperty = value;
                RaisePropertyChanged(NewProjectVersionPropertyName);

                IsNewProjectVersionCorrect = !string.IsNullOrWhiteSpace(value);
                if (!IsNewProjectVersionCorrect)
                {
                    NewProjectVersionValidatedWrongTip = "版本号不能为空";
                    return;
                }

                try
                {
                    var ver = new Version(value);
                    if (ver.Major >= 0 && ver.Minor >= 0 && ver.Build >= 0 && ver.Revision < 0)
                    {
                        IsNewProjectVersionCorrect = true;
                    }
                    else
                    {
                        IsNewProjectVersionCorrect = false;
                        NewProjectVersionValidatedWrongTip = "版本号须为a.b.c形式";
                    }
                }
                catch (Exception)
                {
                    IsNewProjectVersionCorrect = false;
                    NewProjectVersionValidatedWrongTip = "版本号非法";
                }

            }
        }


        /// <summary>
        /// The <see cref="NewProjectVersionValidatedWrongTip" /> property's name.
        /// </summary>
        public const string NewProjectVersionValidatedWrongTipPropertyName = "NewProjectVersionValidatedWrongTip";

        private string _newProjectVersionValidatedWrongTipProperty = "";

        /// <summary>
        /// Sets and gets the NewProjectVersionValidatedWrongTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string NewProjectVersionValidatedWrongTip
        {
            get
            {
                return _newProjectVersionValidatedWrongTipProperty;
            }

            set
            {
                if (_newProjectVersionValidatedWrongTipProperty == value)
                {
                    return;
                }

                _newProjectVersionValidatedWrongTipProperty = value;
                RaisePropertyChanged(NewProjectVersionValidatedWrongTipPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="NewProjectVersionPadding" /> property's name.
        /// </summary>
        public const string NewProjectVersionPaddingPropertyName = "NewProjectVersionPadding";

        private Thickness _newProjectVersionPaddingProperty = new Thickness();

        /// <summary>
        /// Sets and gets the NewProjectVersionPadding property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Thickness NewProjectVersionPadding
        {
            get
            {
                return _newProjectVersionPaddingProperty;
            }

            set
            {
                if (_newProjectVersionPaddingProperty == value)
                {
                    return;
                }

                _newProjectVersionPaddingProperty = value;
                RaisePropertyChanged(NewProjectVersionPaddingPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsNewProjectVersionCorrect" /> property's name.
        /// </summary>
        public const string IsNewProjectVersionCorrectPropertyName = "IsNewProjectVersionCorrect";

        private bool _isNewProjectVersionCorrectProperty = false;

        /// <summary>
        /// Sets and gets the IsNewProjectVersionCorrect property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsNewProjectVersionCorrect
        {
            get
            {
                return _isNewProjectVersionCorrectProperty;
            }

            set
            {
                if (_isNewProjectVersionCorrectProperty == value)
                {
                    return;
                }

                _isNewProjectVersionCorrectProperty = value;
                RaisePropertyChanged(IsNewProjectVersionCorrectPropertyName);

                if (value)
                {
                    NewProjectVersionPadding = new Thickness();
                }
                else
                {
                    NewProjectVersionPadding = new Thickness(0, 0, 20, 0);
                }

                OkCommand.RaiseCanExecuteChanged();

            }
        }

        /// <summary>
        /// The <see cref="IsProcessValid" /> property's name.
        /// </summary>
        public const string IsProcessValidPropertyName = "IsProcessValid";

        private bool _isProcessValidProperty = false;

        /// <summary>
        /// Sets and gets the IsProcessValid property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsProcessValid
        {
            get
            {
                return _isProcessValidProperty;
            }

            set
            {
                if (_isProcessValidProperty == value)
                {
                    return;
                }

                _isProcessValidProperty = value;
                RaisePropertyChanged(IsProcessValidPropertyName);

                if (value)
                {
                    NewProjectVersionPadding = new Thickness();
                }
                else
                {
                    NewProjectVersionPadding = new Thickness(0, 0, 20, 0);
                }

                OkCommand.RaiseCanExecuteChanged();

            }
        }


        /// <summary>
        /// The <see cref="ProcessCheckErrorInfo" /> property's name.
        /// </summary>
        public const string ProcessCheckErrorInfoPropertyName = "ProcessCheckErrorInfo";

        private string _processCheckErrorInfoProperty;

        /// <summary>
        /// Sets and gets the ProcessCheckErrorInfo property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProcessCheckErrorInfo
        {
            get
            {
                return _processCheckErrorInfoProperty;
            }

            set
            {
                if (_processCheckErrorInfoProperty == value)
                {
                    return;
                }

                _processCheckErrorInfoProperty = value;
                RaisePropertyChanged(ProcessCheckErrorInfoPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ProcessNamePadding" /> property's name.
        /// </summary>
        public const string ProcessNamePaddingPropertyName = "ProcessNamePadding";

        private Thickness _processNamePaddingProperty = new Thickness();

        /// <summary>
        /// Sets and gets the ProcessNamePadding property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Thickness ProcessNamePadding
        {
            get
            {
                return _processNamePaddingProperty;
            }

            set
            {
                if (_processNamePaddingProperty == value)
                {
                    return;
                }

                _processNamePaddingProperty = value;
                RaisePropertyChanged(ProcessNamePaddingPropertyName);
            }
        }

        private RelayCommand _checkProcessCommand;

        /// <summary>
        /// Gets the CheckProcessCommand.
        /// </summary>
        public RelayCommand CheckProcessCommand
        {
            get
            {
                return _checkProcessCommand
                    ?? (_checkProcessCommand = new RelayCommand(
                    () =>
                    {
                        IsProcessValid = false;

                        var result = Context.Current.RobotService.CheckProcess(FileName);

                        if (result.Code == ResultCode.LoginOutTimeCode)
                        {
                            ProcessCheckErrorInfo = "登陆已过期";
                            UniMessageBox.Show(App.Current.MainWindow, "登陆已过期", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        if (result.Code == ResultCode.UnAuthorizedCode)
                        {
                            ProcessCheckErrorInfo = "未授权";
                            UniMessageBox.Show(App.Current.MainWindow, "未授权", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                        if (result.Code == ResultCode.SuccessCode)
                        {
                            Version version = null;
                            if(result.Result!=null&&result.Result.IsExist)
                            {
                                version = new Version(result.Result.Version);
                            }
                            else
                            {
                                version = new Version(0, 0, 0);
                            }
                            var newVersion = new Version(version.Major, version.Minor, version.Build + 1);
                            NewProjectVersion = newVersion.ToString();

                            var robotType = RobotTypes.FirstOrDefault(r => r.ID == result.Result.RobotTypeID);
                            SelectedRobotType = robotType;

                            IsProcessValid = true;
                        }
                        else
                        {
                            ProcessCheckErrorInfo = result.Message;
                            UniMessageBox.Show(App.Current.MainWindow, $"检查流程信息失败！原因是：{result.Message}", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }));
            }
        }

        private RelayCommand _okCommand;

        /// <summary>
        /// Gets the OkCommand.
        /// </summary>
        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand
                    ?? (_okCommand = new RelayCommand(
                    async () =>
                    {
                        using (var loadingWait = LoadingWait.Show())
                        {
                            try
                            {
                                #region 数据校验
                                if (string.IsNullOrWhiteSpace(FileName))
                                {
                                    UniMessageBox.Show(App.Current.MainWindow, "流程名不能为空！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }

                                if (string.IsNullOrWhiteSpace(ReleaseNotes))
                                {
                                    UniMessageBox.Show(App.Current.MainWindow, "流程说明不能为空！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }

                                if (!IsProcessValid)
                                {
                                    CheckProcessCommand.Execute(null);
                                    if (!IsProcessValid)
                                    {
                                        UniMessageBox.Show(App.Current.MainWindow, "流程检测不通过！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }
                                }

                                if (string.IsNullOrWhiteSpace(NewProjectVersion))
                                {
                                    UniMessageBox.Show(App.Current.MainWindow, "新版本不能为空！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }
                                #endregion

                                var tempZipFile = Path.Combine(TempFileDir, Guid.NewGuid().ToString() + ".zip");

                                var monitorProcessFile = BuildMonitorProcess();
                                var filesToZip = new List<string> { monitorProcessFile, ProcessFile };
                                var flag = ZipHelper.ZipManyFilesOrDictorys(filesToZip, tempZipFile, null);
                                if (!flag)
                                {
                                    UniMessageBox.Show(App.Current.MainWindow, "压缩文件失败！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    return;
                                }

                                using (FileStream stream = File.Open(tempZipFile, FileMode.Open))
                                {
                                    var request = new PublishRequest
                                    {
                                        SessionId = Context.Current.SessionId,
                                        ProcessName = FileName,
                                        ProcessDescription = ReleaseNotes,
                                        FileStream = ByteString.FromStream(stream),
                                        Version = NewProjectVersion,
                                        PublishTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                        RobotTypeID = SelectedRobotType.ID
                                    };
                                    var result = await Context.Current.RobotService.PublishProcessAsync(request);

                                    if (result.Code == ResultCode.LoginOutTimeCode)
                                    {
                                        UniMessageBox.Show(App.Current.MainWindow, "登陆已过期", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                    if (result.Code == ResultCode.UnAuthorizedCode)
                                    {
                                        UniMessageBox.Show(App.Current.MainWindow, "未授权", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                    if (result.Code == ResultCode.SuccessCode)
                                    {
                                        UniMessageBox.Show(App.Current.MainWindow, $"项目发布成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                        CancelCommand.Execute(null);
                                    }
                                    else
                                    {
                                        UniMessageBox.Show(App.Current.MainWindow, $"发布项目失败！原因是：{result.Message}", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                Logger.Error(err, logger);
                                UniMessageBox.Show(App.Current.MainWindow, "发布项目失败！", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    },
                    () => IsReadyToController && IsNewProjectVersionCorrect&& IsProcessValid));
            }
        }

        private RelayCommand _cancelCommand;

        /// <summary>
        /// Gets the CancelCommand.
        /// </summary>
        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand
                    ?? (_cancelCommand = new RelayCommand(
                    () =>
                    {
                        _view.Close();
                    }));
            }
        }

        private string BuildMonitorProcess()
        {
            var publishAuthors = Environment.UserName;
            var publishVersion = NewProjectVersion;
            var projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Processes\\监控流程");

            var tempMoniterPrjDir = Path.Combine(TempFileDir, FileName);
            if (!Directory.Exists(tempMoniterPrjDir))
            {
                Directory.CreateDirectory(tempMoniterPrjDir);
            }

            ManifestMetadata metadata = new ManifestMetadata()
            {
                Authors = publishAuthors,
                Owners = publishAuthors,
                Version = publishVersion,
                Id = FileName,//项目名称
                Description = ReleaseNotes,//项目描述
                ReleaseNotes = ReleaseNotes,
                Tags="Monitor"
            };

            PackageBuilder builder = new PackageBuilder();
            builder.PopulateFiles(projectPath, new[] { new ManifestFile() { Source = @"**", Target = @"lib/net452" } });
            builder.Populate(metadata);

            var outputPath = Path.Combine(tempMoniterPrjDir, FileName) + "." + publishVersion + ".nupkg";
            if(File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            using (FileStream stream = File.Open(outputPath, FileMode.Create))
            {
                builder.Save(stream);
            }

            return outputPath;
        }

        private void OnPublishingToController()
        {
            try
            {
                LoadingWaitVisible = true;
                IsReadyToController = false;

                var isConnectedToController = Context.Current.ConnectedToController;
                if (!isConnectedToController)
                {
                    UniMessageBox.Show(App.Current.MainWindow, "未能连接到控制器,请先连接到控制器", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var isLogined = !string.IsNullOrWhiteSpace(Context.Current.SessionId);
                if (!isLogined)
                {
                    var loginWindow = new LoginWindow();
                    loginWindow.Owner = _view;
                    loginWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    var loginViewModel = loginWindow.DataContext as LoginViewModel;

                    loginWindow.ShowDialog();
                    if (!loginViewModel.IsLogined)
                    {
                        return;
                    }
                }

                var robotTypeListResponse = Context.Current.RobotService.GetRobotTypeList();
                if (robotTypeListResponse.Code == ResultCode.LoginOutTimeCode)
                {
                    UniMessageBox.Show(App.Current.MainWindow, "登陆已过期", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (robotTypeListResponse.Code == ResultCode.UnAuthorizedCode)
                {
                    UniMessageBox.Show(App.Current.MainWindow, "未授权", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (robotTypeListResponse.Code == ResultCode.SuccessCode)
                {
                    RobotTypes = new ObservableCollection<RobotTypeModel>(robotTypeListResponse.Result);
                }

                IsReadyToController = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, logger);
                UniMessageBox.Show(App.Current.MainWindow, ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingWaitVisible = false;
            }
        }
    }
}