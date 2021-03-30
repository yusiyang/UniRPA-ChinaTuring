using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Xml;
using System.IO;
using UniStudio.Librarys;
using log4net;
using System.Windows;
using Newtonsoft.Json;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Controls;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Text;
using Plugins.Shared.Library.Extensions;

namespace UniStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class NewProjectViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Window m_view;


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
                    }));
            }
        }

        private RelayCommand<RoutedEventArgs> _projectNameLoadedCommand;

        /// <summary>
        /// Gets the ProjectNameLoadedCommand.
        /// </summary>
        public RelayCommand<RoutedEventArgs> ProjectNameLoadedCommand
        {
            get
            {
                return _projectNameLoadedCommand
                    ?? (_projectNameLoadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        var textBox = (TextBox)p.Source;
                        textBox.Focus();
                        textBox.SelectAll();
                    }));
            }
        }



        public enum enProjectType
        {
            Null = 0,
            Process,
            Template
        }

        /// <summary>
        /// The <see cref="ProjectType" /> property's name.
        /// </summary>
        public const string ProjectTypePropertyName = "ProjectType";

        private enProjectType _projectTypeProperty = enProjectType.Null;

        /// <summary>
        /// Sets and gets the ProjectType property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public enProjectType ProjectType
        {
            get
            {
                return _projectTypeProperty;
            }

            set
            {
                if (_projectTypeProperty == value)
                {
                    return;
                }

                _projectTypeProperty = value;
                RaisePropertyChanged(ProjectTypePropertyName);

                initInfoByProjectType(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the NewProjectViewModel class.
        /// </summary>
        public NewProjectViewModel()
        {
        }

        /// <summary>
        /// The <see cref="Title" /> property's name.
        /// </summary>
        public const string TitlePropertyName = "Title";

        private string _titleProperty = "";

        /// <summary>
        /// Sets and gets the Title property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Title
        {
            get
            {
                return _titleProperty;
            }

            set
            {
                if (_titleProperty == value)
                {
                    return;
                }

                _titleProperty = value;
                RaisePropertyChanged(TitlePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="Description" /> property's name.
        /// </summary>
        public const string DescriptionPropertyName = "Description";

        private string _descriptionProperty = "";

        /// <summary>
        /// Sets and gets the Description property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string Description
        {
            get
            {
                return _descriptionProperty;
            }

            set
            {
                if (_descriptionProperty == value)
                {
                    return;
                }

                _descriptionProperty = value;
                RaisePropertyChanged(DescriptionPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="IsProjectNameCorrect" /> property's name.
        /// </summary>
        public const string IsProjectNameCorrectPropertyName = "IsProjectNameCorrect";

        private bool _isProjectNameCorrectProperty = false;

        /// <summary>
        /// Sets and gets the IsProjectNameCorrect property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsProjectNameCorrect
        {
            get
            {
                return _isProjectNameCorrectProperty;
            }

            set
            {
                if (_isProjectNameCorrectProperty == value)
                {
                    return;
                }

                _isProjectNameCorrectProperty = value;
                RaisePropertyChanged(IsProjectNameCorrectPropertyName);
            }
        }

        public const string LeftImageSourceProperty = "LeftImageSource";
        private string _leftImageSource = "";
        public string LeftImageSource
        {
            get
            {
                return _leftImageSource;
            }
            set
            {
                if (_leftImageSource == value)
                {
                    return;
                }

                _leftImageSource = value;
                RaisePropertyChanged(LeftImageSourceProperty);
            }
        }

        private string TemplateDirectoryPath { get; set; }

        /// <summary>
        /// The <see cref="ProjectName" /> property's name.
        /// </summary>
        public const string ProjectNamePropertyName = "ProjectName";

        private string _projectNameProperty = "";

        /// <summary>
        /// Sets and gets the ProjectName property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProjectName
        {
            get
            {
                return _projectNameProperty;
            }

            set
            {
                if (_projectNameProperty == value)
                {
                    return;
                }

                _projectNameProperty = value;
                RaisePropertyChanged(ProjectNamePropertyName);

                projectNameValidate(value);
            }
        }


        /// <summary>
        /// The <see cref="ProjectNameValidatedWrongTip" /> property's name.
        /// </summary>
        public const string ProjectNameValidatedWrongTipPropertyName = "ProjectNameValidatedWrongTip";

        private string _projectNameValidatedWrongTipProperty = "";

        /// <summary>
        /// Sets and gets the ProjectNameValidatedWrongTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProjectNameValidatedWrongTip
        {
            get
            {
                return _projectNameValidatedWrongTipProperty;
            }

            set
            {
                if (_projectNameValidatedWrongTipProperty == value)
                {
                    return;
                }

                _projectNameValidatedWrongTipProperty = value;
                RaisePropertyChanged(ProjectNameValidatedWrongTipPropertyName);
            }
        }

        private void projectNameValidate(string value)
        {
            IsProjectNameCorrect = true;
            if (string.IsNullOrEmpty(value))
            {
                IsProjectNameCorrect = false;
                ProjectNameValidatedWrongTip = "名称不能为空";
            }
            else
            {
                if (value.Contains(@"\") || value.Contains(@"/"))
                {
                    IsProjectNameCorrect = false;
                    ProjectNameValidatedWrongTip = "名称不能有非法字符";
                }
                else
                {
                    System.IO.FileInfo fi = null;
                    try
                    {
                        fi = new System.IO.FileInfo(value);
                    }
                    catch (ArgumentException) { }
                    catch (System.IO.PathTooLongException) { }
                    catch (NotSupportedException) { }
                    if (ReferenceEquals(fi, null))
                    {
                        // file name is not valid
                        IsProjectNameCorrect = false;
                        ProjectNameValidatedWrongTip = "名称不能有非法字符";
                    }
                    else
                    {
                        // file name is valid... May check for existence by calling fi.Exists.
                    }
                }
            }

            if (Directory.Exists(ProjectPath + @"\" + ProjectName))
            {
                IsProjectNameCorrect = false;
                ProjectNameValidatedWrongTip = "已经存在同名称的项目";
            }

            CreateProjectCommand.RaiseCanExecuteChanged();
        }




        /// <summary>
        /// The <see cref="IsProjectPathCorrect" /> property's name.
        /// </summary>
        public const string IsProjectPathCorrectPropertyName = "IsProjectPathCorrect";

        private bool _isProjectPathCorrectProperty = false;

        /// <summary>
        /// Sets and gets the IsProjectPathCorrect property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public bool IsProjectPathCorrect
        {
            get
            {
                return _isProjectPathCorrectProperty;
            }

            set
            {
                if (_isProjectPathCorrectProperty == value)
                {
                    return;
                }

                _isProjectPathCorrectProperty = value;
                RaisePropertyChanged(IsProjectPathCorrectPropertyName);
            }
        }



        /// <summary>
        /// 此路径为项目创建时所在的目录
        /// </summary>
        public const string ProjectPathPropertyName = "ProjectPath";

        private string _projectPathProperty = "";

        /// <summary>
        /// Sets and gets the ProjectPath property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProjectPath
        {
            get
            {
                return _projectPathProperty;
            }

            set
            {
                if (_projectPathProperty == value)
                {
                    return;
                }

                _projectPathProperty = value;
                RaisePropertyChanged(ProjectPathPropertyName);

                projectPathValidate(value);
                projectNameValidate(ProjectName);//路径改变了同样要检查名称
            }
        }

        private void projectPathValidate(string value)
        {
            IsProjectPathCorrect = true;
            if (string.IsNullOrEmpty(value))
            {
                IsProjectPathCorrect = false;
                ProjectPathValidatedWrongTip = "位置不能为空";
            }
            else
            {
                if (!Directory.Exists(value))
                {
                    IsProjectPathCorrect = false;
                    ProjectPathValidatedWrongTip = "指定的位置不存在";
                }
            }

            CreateProjectCommand.RaiseCanExecuteChanged();
        }



        /// <summary>
        /// The <see cref="ProjectPathValidatedWrongTip" /> property's name.
        /// </summary>
        public const string ProjectPathValidatedWrongTipPropertyName = "ProjectPathValidatedWrongTip";

        private string _projectPathValidatedWrongTipProperty = "";

        /// <summary>
        /// Sets and gets the ProjectPathValidatedWrongTip property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProjectPathValidatedWrongTip
        {
            get
            {
                return _projectPathValidatedWrongTipProperty;
            }

            set
            {
                if (_projectPathValidatedWrongTipProperty == value)
                {
                    return;
                }

                _projectPathValidatedWrongTipProperty = value;
                RaisePropertyChanged(ProjectPathValidatedWrongTipPropertyName);
            }
        }



        private RelayCommand _selectProjectPathCommand;

        /// <summary>
        /// Gets the SelectProjectPathCommand.
        /// </summary>
        public RelayCommand SelectProjectPathCommand
        {
            get
            {
                return _selectProjectPathCommand
                    ?? (_selectProjectPathCommand = new RelayCommand(
                    () =>
                    {
                        string dst_dir = "";
                        if (Common.ShowSelectDirDialog("请选择一个位置来新建项目", ref dst_dir))
                        {
                            ProjectPath = dst_dir;
                        }
                    }));
            }
        }



        /// <summary>
        /// The <see cref="ProjectDescription" /> property's name.
        /// </summary>
        public const string ProjectDescriptionPropertyName = "ProjectDescription";

        private string _projectDescriptionProperty = "";

        /// <summary>
        /// Sets and gets the ProjectDescription property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ProjectDescription
        {
            get
            {
                return _projectDescriptionProperty;
            }

            set
            {
                if (_projectDescriptionProperty == value)
                {
                    return;
                }

                _projectDescriptionProperty = value;
                RaisePropertyChanged(ProjectDescriptionPropertyName);
            }
        }


        private RelayCommand _createProjectCommand;

        /// <summary>
        /// Gets the CreateProjectCommand.
        /// </summary>
        public RelayCommand CreateProjectCommand
        {
            get
            {
                return _createProjectCommand
                    ?? (_createProjectCommand = new RelayCommand(
                    () =>
                    {
                        if (!ViewModelLocator.instance.Main.DoCloseProject())
                        {
                            return;//用户取消了关闭，直接返回
                        }

                        //开始创建项目
                        //1.首先保存创建项目的位置到ProjectUserConfig.xml中去
                        SaveDefaultProjectPath(ProjectPath);

                        //2.创建项目目录
                        var currentProjectPath = Path.Combine(ProjectPath, ProjectName);
                        try
                        {
                            Directory.CreateDirectory(currentProjectPath);
                        }
                        catch (Exception e)
                        {
                            //创建项目失败
                            Logger.Error(e, logger);

                            UniMessageBox.Show(App.Current.MainWindow, "创建项目目录失败，请检查", "错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        //3.根据创建项目方式不同，进行不同的操作
                        switch (ProjectType)
                        {
                            case enProjectType.Process:
                                // 创建项目配置文件project.json
                                initProjectJson(ProjectName, ProjectDescription, "Main.xaml", "Workflow", currentProjectPath);
                                // 创建项目主流程文件Main.xaml
                                initMainXaml();

                                break;

                            case enProjectType.Template:
                                // 拷贝模板数据到项目目录
                                Plugins.Shared.Library.Librarys.DirectoryOperation.Copy(TemplateDirectoryPath, currentProjectPath);
                                // 更新project.json文件
                                string jsonString = File.ReadAllText(Path.Combine(currentProjectPath, "project.json"), Encoding.UTF8);
                                JObject jObject = JObject.Parse(jsonString);
                                jObject["name"] = ProjectName;
                                jObject["description"] = ProjectDescription;
                                string convertString = Convert.ToString(jObject);
                                File.WriteAllText(Path.Combine(currentProjectPath, "project.json"), convertString, Encoding.UTF8);

                                break;
                        }

                        //4.保存到最近项目列表中
                        addToRecentProjects(ProjectName, ProjectDescription, currentProjectPath);

                        //5.切换到项目DOCKER中，并自动打开Main.xaml
                        var msg = new MessengerObjects.ProjectOpen();
                        msg.ProjectJsonFile = Path.Combine(currentProjectPath, "project.json");
                        Messenger.Default.Send(msg);


                        ViewModelLocator.instance.Main.IsOpenStartScreen = false;
                        ViewModelLocator.instance.Main.IsBackButtonVisible = true;
                        m_view.Close();
                    },
                    () => IsProjectNameCorrect && IsProjectPathCorrect));
            }
        }

        public string ProjectConfigPath
        {
            get
            {
                var path = App.LocalRPAStudioDir + @"\config\ProjectConfig.xml";
                if (!File.Exists(path))
                {
                    XmlDocument doc = new XmlDocument();
                    using (var ms = new MemoryStream(Properties.Resources.ProjectConfig))
                    {
                        ms.Flush();
                        ms.Position = 0;
                        doc.Load(ms);
                        ms.Close();
                    }
                    doc.Save(path);
                }

                return path;
            }
        }

        public void addToRecentProjects(string name, string description, string projectPath)
        {
            XmlDocument doc = new XmlDocument();
            var path = App.LocalRPAStudioDir + @"\Config\RecentProjects.xml";
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            var projectNodes = rootNode.SelectNodes("Project");

            //最多记录100条，默认显示个数由XML的MaxShowCount限制
            if (projectNodes.Count > 100)
            {
                rootNode.RemoveChild(rootNode.LastChild);
            }

            XmlElement projectElement = doc.CreateElement("Project");
            projectElement.SetAttribute("FilePath", projectPath + @"\project.json");
            projectElement.SetAttribute("Name", name);
            projectElement.SetAttribute("Description", description);

            rootNode.PrependChild(projectElement);

            doc.Save(path);

            //广播RecentProjects.xml改变的消息，以重刷最近项目列表
            Messenger.Default.Send(new MessengerObjects.RecentProjectsModify());
        }

        private void initMainXaml()
        {
            byte[] data = Properties.Resources.Main;
            FileStream fileStream = new FileStream(ProjectPath + @"\" + ProjectName + @"\Main.xaml", FileMode.CreateNew);
            fileStream.Write(data, 0, (int)(data.Length));
            fileStream.Close();
        }

        public void initProjectJson(string name, string description, string main, string projectType, string projectPath)
        {
            var config = new ProjectJsonConfig();
            config.Init();
            config.studioVersion = Common.GetProgramVersion();
            config.name = name;
            config.description = description;
            config.main = main;
            config.projectType = projectType;
            config.dependencies = new Dictionary<string, string>()
            {
                // TODO:【🐵🐵 LPY 🐵🐵】 这里暂时写几个测试用的活动包，后期会将平台默认包放在这里
                //{ "ActivitiesDownload", "[1.0.0]" },
                //{ "ActivitiesGetJsonByUrl", "[1.0.0]" },
                //{ "ActivitiesUnzip", "[0.1.0]" }
            };

            string json = JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);

            using (FileStream fs = new FileStream(projectPath + @"\project.json", FileMode.Create))
            {
                //写入 
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }

        }

        private void SaveDefaultProjectPath(string projectPath)
        {
            XmlDocument doc = new XmlDocument();
            var path = App.LocalRPAStudioDir + @"\Config\ProjectUserConfig.xml";
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            rootNode.SetAttribute("DefaultCreatePath", ProjectPath);

            doc.Save(path);
        }

        private void initInfoByProjectType(enProjectType value)
        {
            switch (value)
            {
                case enProjectType.Process:
                    initProjectUserConfig();
                    initProjectByProcessConfig();
                    break;

                case enProjectType.Template:
                    initProjectUserConfig();
                    initProjectByTemplateConfig();
                    break;
            }
        }

        private void initProjectByProcessConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ProjectConfigPath);

            var rootNode = doc.DocumentElement;
            var processElement = rootNode.SelectSingleNode("Process") as XmlElement;

            Title = processElement.GetAttribute("Title");
            Description = processElement.GetAttribute("Description");
            var projectName = processElement.GetAttribute("ProjectName");
            ProjectName = Common.GetValidDirectoryName(ProjectPath, projectName, "{0}", 1);
            ProjectDescription = processElement.GetAttribute("ProjectDescription");
            LeftImageSource = "pack://application:,,,/Resource/Image/Windows/NewProject/process.png";
        }

        private void initProjectByTemplateConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ProjectConfigPath);

            var rootNode = doc.DocumentElement;
            var templateElement = rootNode.SelectSingleNode("Template") as XmlElement;

            Title = templateElement.GetAttribute("Title");
            Description = templateElement.GetAttribute("Description");
            var projectName = templateElement.GetAttribute("ProjectName");
            ProjectName = Common.GetValidDirectoryName(ProjectPath, projectName, "{0}", 1);
            ProjectDescription = templateElement.GetAttribute("ProjectDescription");
            LeftImageSource = "pack://application:,,,/Resource/Image/Windows/NewTemplate/template.png";

            TemplateDirectoryPath = templateElement.GetAttribute("TemplateDirectoryPath");
        }

        private void initProjectUserConfig()
        {
            XmlDocument doc = new XmlDocument();
            var path = App.LocalRPAStudioDir + @"\Config\ProjectUserConfig.xml";
            doc.Load(path);
            var rootNode = doc.DocumentElement;

            var defaultCreatePath = rootNode.GetAttribute("DefaultCreatePath");
            if (string.IsNullOrEmpty(defaultCreatePath) || !Directory.Exists(defaultCreatePath))
            {
                defaultCreatePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\UniStudio\Projects";

                if (!Directory.Exists(defaultCreatePath))
                {
                    Directory.CreateDirectory(defaultCreatePath);
                }
            }

            ProjectPath = defaultCreatePath;

        }


    }






    class ProjectJsonConfig
    {
        private static readonly string initial_schema_version = "1.0.0";//新建项目时的的project.json文件版本
        private static readonly string initial_project_version = "1.0.0";//新建项目时的的项目版本，发布项目时会显示出来

        public string schemaVersion { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string studioVersion { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string projectType { get; set; }

        public string projectVersion { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string name { get; set; }

        public string description { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string main { get; set; }

        public Dictionary<string, string> dependencies { get; set; }

        public bool Upgrade()
        {
            var schemaVersionTmp = schemaVersion;

            Upgrade(ref schemaVersionTmp, initial_schema_version);

            if (schemaVersion == schemaVersionTmp)
            {
                return false;
            }
            else
            {
                schemaVersion = schemaVersionTmp;
                return true;
            }
        }

        private void Upgrade(ref string schemaVersion, string newSchemaVersion)
        {
            if (schemaVersion == newSchemaVersion)
            {
                return;
            }

            if (string.IsNullOrEmpty(schemaVersion))
            {
                //TODO WJF 后期可以考虑删除，并强制schemaVersion及projectVersion字段在JSON中存在
                //从空schemaVersion=>1.0.0
                projectVersion = "1.0.0";

                schemaVersion = "1.0.0";
            }
            else if (schemaVersion == "1.0.0")
            {
                //TODO WJF 1.0.0=>1.0.1升级
                schemaVersion = "1.0.1";
            }
            else if (schemaVersion == "1.0.1")
            {
                //TODO WJF 1.0.1=>1.0.2升级
                schemaVersion = "1.0.2";
            }

            Upgrade(ref schemaVersion, initial_schema_version);
        }

        public void Init()
        {
            schemaVersion = initial_schema_version;
            projectVersion = initial_project_version;
        }
    }







}