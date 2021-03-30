using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Xml;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Plugins.Shared.Library.Extensions;
using UniStudio.Community.Librarys;
using UniStudio.Community.Windows;

namespace UniStudio.Community.ViewModel
{
    public class TemplateItem : ViewModelBase
    {
        public const string TemplateNameProperty = "TemplateName";
        private string _templateName = "";
        public string TemplateName
        {
            get
            {
                return _templateName;
            }
            set
            {
                if (_templateName == value)
                {
                    return;
                }

                _templateName = value;
                RaisePropertyChanged(TemplateNameProperty);
            }
        }

        public const string TemplateDescriptionProperty = "TemplateDescription";
        private string _templateDescription = "";
        public string TemplateDescription
        {
            get
            {
                return _templateDescription;
            }
            set
            {
                if (_templateDescription == value)
                {
                    return;
                }

                _templateDescription = value;
                RaisePropertyChanged(TemplateDescriptionProperty);
            }
        }

        public const string DefaultProjectNameProperty = "DefaultProjectName";
        private string _defaultProjectName = "";
        public string DefaultProjectName
        {
            get
            {
                return _defaultProjectName;
            }
            set
            {
                if (_defaultProjectName == value)
                {
                    return;
                }

                _defaultProjectName = value;
                RaisePropertyChanged(DefaultProjectNameProperty);
            }
        }

        public const string DefaultProjectDescriptionProperty = "DefaultProjectDescription";
        private string _defaultProjectDescription = "";
        public string DefaultProjectDescription
        {
            get
            {
                return _defaultProjectDescription;
            }
            set
            {
                if (_defaultProjectDescription == value)
                {
                    return;
                }

                _defaultProjectDescription = value;
                RaisePropertyChanged(DefaultProjectDescription);
            }
        }

        public const string TemplateDirectoryPathProperty = "TemplateDirectoryPath";
        private string _templateDirectoryPath = "";
        public string TemplateDirectoryPath
        {
            get
            {
                return _templateDirectoryPath;
            }
            set
            {
                if (_templateDirectoryPath == value)
                {
                    return;
                }

                if (value.Substring(0, 2).Equals(".\\"))
                {
                    _templateDirectoryPath = Path.Combine(Environment.CurrentDirectory, value.Substring(2));
                }
                else
                {
                    _templateDirectoryPath = value;
                }

                RaisePropertyChanged(TemplateDirectoryPathProperty);
            }
        }

        #region 主题色配置
        public SolidColorBrush ItemTitleForeground { get => ViewModelLocator.instance.Main.ItemTitleForeground; }
        public SolidColorBrush ItemDescriptionForeground { get => ViewModelLocator.instance.Main.ItemDescriptionForeground; }
        public SolidColorBrush ItemMouseOverBackground { get => ViewModelLocator.instance.Main.ItemMouseOverBackground; }
        #endregion

        private RelayCommand _newProcessByDefaultTemplateCommand;
        public RelayCommand NewProcessByDefaultTemplateCommand
        {
            get
            {
                return _newProcessByDefaultTemplateCommand
                    ?? (_newProcessByDefaultTemplateCommand = new RelayCommand(
                    () =>
                    {
                        // 更新 ProjectConfig.xml 配置的 Template 项
                        XmlDocument doc = new XmlDocument();
                        var path = ViewModelLocator.instance.NewProject.ProjectConfigPath;
                        doc.Load(path);
                        var rootNode = doc.DocumentElement;
                        var templateElement = rootNode.SelectSingleNode("Template") as XmlElement;
                        if (templateElement == null)
                        {
                            templateElement = doc.CreateElement("Template");
                            templateElement.SetAttribute("Title", "新建 " + TemplateName);
                            templateElement.SetAttribute("Description", TemplateDescription);
                            templateElement.SetAttribute("ProjectName", DefaultProjectName);
                            templateElement.SetAttribute("ProjectDescription", DefaultProjectDescription);
                            templateElement.SetAttribute("TemplateDirectoryPath", TemplateDirectoryPath);
                            rootNode.AppendChild(templateElement);
                        }
                        else
                        {
                            templateElement.SetAttribute("Title", "新建 " + TemplateName);
                            templateElement.SetAttribute("Description", TemplateDescription);
                            templateElement.SetAttribute("ProjectName", DefaultProjectName);
                            templateElement.SetAttribute("ProjectDescription", DefaultProjectDescription);
                            templateElement.SetAttribute("TemplateDirectoryPath", TemplateDirectoryPath);
                        }
                        doc.Save(path);

                        //弹出新建项目对话框
                        var window = new NewProjectWindow();
                        window.Owner = Application.Current.MainWindow;
                        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        var vm = window.DataContext as NewProjectViewModel;
                        vm.ProjectType = NewProjectViewModel.enProjectType.Template;
                        window.ShowDialog();
                    }));
            }
        }

        private RelayCommand _newProcessByCustomTemplateCommand;
        public RelayCommand NewProcessByCustomTemplateCommand
        {
            get
            {
                return _newProcessByCustomTemplateCommand
                    ?? (_newProcessByCustomTemplateCommand = new RelayCommand(
                    () =>
                    {
                        if (!Directory.Exists(TemplateDirectoryPath))
                        {
                            MessageBoxResult result = UniMessageBox.Show("未能找到路径“" + TemplateDirectoryPath + "”的模板数据，是否从模板列表中移除该条目？", "询问", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                            if (result.Equals(MessageBoxResult.OK))
                            {
                                // 删除 CustomTemplate.xml 中的当前项，并更新列表
                                DeleteCustomTemplateXmlItem();
                            }
                            return;
                        }

                        // 更新 ProjectConfig.xml 配置的 Template 项
                        XmlDocument doc = new XmlDocument();
                        var path = ViewModelLocator.instance.NewProject.ProjectConfigPath;
                        doc.Load(path);
                        var rootNode = doc.DocumentElement;
                        var templateElement = rootNode.SelectSingleNode("Template") as XmlElement;
                        if (templateElement == null)
                        {
                            templateElement = doc.CreateElement("Template");
                            templateElement.SetAttribute("Title", "新建 " + TemplateName);
                            templateElement.SetAttribute("Description", TemplateDescription);
                            templateElement.SetAttribute("ProjectName", DefaultProjectName);
                            templateElement.SetAttribute("ProjectDescription", DefaultProjectDescription);
                            templateElement.SetAttribute("TemplateDirectoryPath", TemplateDirectoryPath);
                            rootNode.AppendChild(templateElement);
                        }
                        else
                        {
                            templateElement.SetAttribute("Title", "新建 " + TemplateName);
                            templateElement.SetAttribute("Description", TemplateDescription);
                            templateElement.SetAttribute("ProjectName", DefaultProjectName);
                            templateElement.SetAttribute("ProjectDescription", DefaultProjectDescription);
                            templateElement.SetAttribute("TemplateDirectoryPath", TemplateDirectoryPath);
                        }
                        doc.Save(path);

                        //弹出新建项目对话框
                        var window = new NewProjectWindow();
                        window.Owner = Application.Current.MainWindow;
                        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                        var vm = window.DataContext as NewProjectViewModel;
                        vm.ProjectType = NewProjectViewModel.enProjectType.Template;
                        window.ShowDialog();
                    }));
            }
        }

        private RelayCommand _showTemplateMenuCommand;
        public RelayCommand ShowTemplateMenuCommand
        {
            get
            {
                return _showTemplateMenuCommand
                    ?? (_showTemplateMenuCommand = new RelayCommand(
                    () =>
                    {
                        var view = App.Current.MainWindow;
                        var cm = view.FindResource("CustomTemplateItemContextMenu") as ContextMenu;
                        cm.DataContext = this;
                        cm.Placement = PlacementMode.MousePoint;
                        cm.IsOpen = true;
                    }));
            }
        }

        private RelayCommand _deleteTemplateCommand;
        public RelayCommand DeleteTemplateCommand
        {
            get
            {
                return _deleteTemplateCommand
                    ?? (_deleteTemplateCommand = new RelayCommand(
                    () =>
                    {
                        MessageBoxResult result = UniMessageBox.Show("是否要永久删除模板 " + TemplateName + "？", "删除模板", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result.Equals(MessageBoxResult.Yes))
                        {
                            // 删除 CustomTemplate.xml 中的当前项，并更新列表
                            DeleteCustomTemplateXmlItem();

                            try
                            {
                                // 删除磁盘实际数据
                                Directory.Delete(TemplateDirectoryPath, true);
                            }
                            catch (DirectoryNotFoundException e)
                            {

                            }
                        }
                    }));
            }
        }

        /// <summary>
        /// 删除 CustomTemplate.xml 中的当前项，并更新列表
        /// </summary>
        private void DeleteCustomTemplateXmlItem()
        {
            // 删除配置文件条目
            XmlDocument doc = new XmlDocument();
            var path = App.LocalRPAStudioDir + @"\Config\CustomTemplate.xml";
            doc.Load(path);
            var rootNode = doc.DocumentElement;
            var templateNodes = rootNode.SelectNodes("Template");
            foreach (XmlElement item in templateNodes)
            {
                var templateDirectoryPath = item.GetAttribute("TemplateDirectoryPath");
                if (TemplateDirectoryPath.Equals(templateDirectoryPath))
                {
                    rootNode.RemoveChild(item);
                    break;
                }
            }
            doc.Save(path);

            //广播 CustomTemplate.xml 改变的消息，以重刷自定义模板列表
            Messenger.Default.Send(new MessengerObjects.CustomTemplateModify());
        }
    }
}
