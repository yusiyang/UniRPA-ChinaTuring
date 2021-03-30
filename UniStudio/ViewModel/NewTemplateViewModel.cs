using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using log4net;
using Plugins.Shared.Library.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using UniStudio.Librarys;

namespace UniStudio.ViewModel
{
    public class NewTemplateViewModel : ViewModelBase
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Window m_view;

        public string ProjectTemplatesPath
        {
            get
            {
                string path = Path.Combine(App.LocalRPAStudioDir, "ProjectTemplates");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

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
                RaisePropertyChanged(DefaultProjectDescriptionProperty);
            }
        }



        public const string IsTemplateNameCorrectProperty = "IsTemplateNameCorrect";
        private bool _isTemplateNameCorrect;
        public bool IsTemplateNameCorrect
        {
            get
            {
                return _isTemplateNameCorrect;
            }
            set
            {
                if (_isTemplateNameCorrect == value)
                {
                    return;
                }

                _isTemplateNameCorrect = value;
                RaisePropertyChanged(IsTemplateNameCorrectProperty);
            }
        }

        public const string IsTemplateNameEmptyProperty = "IsTemplateNameEmpty";
        private bool _isTemplateNameEmpty;
        public bool IsTemplateNameEmpty
        {
            get
            {
                return _isTemplateNameEmpty;
            }
            set
            {
                if (_isTemplateNameEmpty == value)
                {
                    return;
                }

                _isTemplateNameEmpty = value;
                RaisePropertyChanged(IsTemplateNameEmptyProperty);
            }
        }

        public const string IsTemplateNameRepeatedProperty = "IsTemplateNameRepeated";
        private bool _isTemplateNameRepeated;
        public bool IsTemplateNameRepeated
        {
            get
            {
                return _isTemplateNameRepeated;
            }
            set
            {
                if (_isTemplateNameRepeated == value)
                {
                    return;
                }

                _isTemplateNameRepeated = value;
                RaisePropertyChanged(IsTemplateNameRepeatedProperty);
            }
        }



        public const string IsDefaultProjectNameCorrectProperty = "IsDefaultProjectNameCorrect";
        private bool _isDefaultProjectNameCorrect;
        public bool IsDefaultProjectNameCorrect
        {
            get
            {
                return _isDefaultProjectNameCorrect;
            }
            set
            {
                if (_isDefaultProjectNameCorrect == value)
                {
                    return;
                }

                _isDefaultProjectNameCorrect = value;
                RaisePropertyChanged(IsDefaultProjectNameCorrectProperty);
            }
        }

        public const string IsDefaultProjectNameEmptyProperty = "IsDefaultProjectNameEmpty";
        private bool _isDefaultProjectNameEmpty;
        public bool IsDefaultProjectNameEmpty
        {
            get
            {
                return _isDefaultProjectNameEmpty;
            }
            set
            {
                if (_isDefaultProjectNameEmpty == value)
                {
                    return;
                }

                _isDefaultProjectNameEmpty = value;
                RaisePropertyChanged(IsDefaultProjectNameEmptyProperty);
            }
        }



        public const string CanCreateProperty = "CanCreate";
        private bool _canCreate;
        public bool CanCreate
        {
            get
            {
                return _canCreate;
            }
            set
            {
                if (_canCreate == value)
                {
                    return;
                }

                _canCreate = value;
                RaisePropertyChanged(CanCreateProperty);
            }
        }



        private RelayCommand<RoutedEventArgs> _loadedCommand;
        public RelayCommand<RoutedEventArgs> LoadedCommand
        {
            get
            {
                return _loadedCommand
                    ?? (_loadedCommand = new RelayCommand<RoutedEventArgs>(
                    p =>
                    {
                        m_view = (Window)p.Source;

                        this.TemplateName = ViewModelLocator.instance.Project.ProjectName;
                        this.TemplateDescription = ViewModelLocator.instance.Project.ProjectDescription;
                        this.DefaultProjectName = ViewModelLocator.instance.Project.ProjectName;
                        this.DefaultProjectDescription = ViewModelLocator.instance.Project.ProjectDescription;

                        IsTemplateNameCorrect = true;
                        IsTemplateNameEmpty = false;
                        IsTemplateNameRepeated = false;

                        IsDefaultProjectNameCorrect = true;
                        IsDefaultProjectNameEmpty = false;

                        CanCreate = true;

                        if (Directory.Exists(Path.Combine(ProjectTemplatesPath, TemplateName)))
                        {
                            IsTemplateNameCorrect = false;
                            IsTemplateNameEmpty = false;
                            IsTemplateNameRepeated = true;
                        }
                    }));
            }
        }

        private RelayCommand<RoutedEventArgs> _templateNameChangedCommand;
        public RelayCommand<RoutedEventArgs> TemplateNameChangedCommand
        {
            get
            {
                return _templateNameChangedCommand
                    ?? (_templateNameChangedCommand = new RelayCommand<RoutedEventArgs>(
                        p =>
                        {
                            var textBox = (TextBox)p.Source;

                            if (string.IsNullOrEmpty(textBox.Text))
                            {
                                IsTemplateNameCorrect = false;
                                IsTemplateNameEmpty = true;
                                IsTemplateNameRepeated = false;
                            }
                            else if (Directory.Exists(Path.Combine(ProjectTemplatesPath, textBox.Text)))
                            {
                                IsTemplateNameCorrect = false;
                                IsTemplateNameEmpty = false;
                                IsTemplateNameRepeated = true;
                            }
                            else
                            {
                                IsTemplateNameCorrect = true;
                                IsTemplateNameEmpty = false;
                                IsTemplateNameRepeated = false;
                            }

                            if ((IsTemplateNameCorrect || IsTemplateNameRepeated) && IsDefaultProjectNameCorrect)
                            {
                                CanCreate = true;
                            }
                            else
                            {
                                CanCreate = false;
                            }
                        }));
            }
        }

        private RelayCommand<RoutedEventArgs> _defaultProjectNameChangedCommand;
        public RelayCommand<RoutedEventArgs> DefaultProjectNameChangedCommand
        {
            get
            {
                return _defaultProjectNameChangedCommand
                    ?? (_defaultProjectNameChangedCommand = new RelayCommand<RoutedEventArgs>(
                        p =>
                        {
                            var textBox = (TextBox)p.Source;

                            if (string.IsNullOrEmpty(textBox.Text))
                            {
                                IsDefaultProjectNameCorrect = false;
                                IsDefaultProjectNameEmpty = true;
                            }
                            else
                            {
                                IsDefaultProjectNameCorrect = true;
                                IsDefaultProjectNameEmpty = false;
                            }

                            if ((IsTemplateNameCorrect || IsTemplateNameRepeated) && IsDefaultProjectNameCorrect)
                            {
                                CanCreate = true;
                            }
                            else
                            {
                                CanCreate = false;
                            }
                        }));
            }
        }

        private RelayCommand _createTemplateCommand;
        public RelayCommand CreateTemplateCommand
        {
            get
            {
                return _createTemplateCommand
                    ?? (_createTemplateCommand = new RelayCommand(
                        () =>
                        {
                            XmlDocument doc = new XmlDocument();
                            var path = App.LocalRPAStudioDir + @"\Config\CustomTemplate.xml";
                            doc.Load(path);
                            var rootNode = doc.DocumentElement;

                            if (IsTemplateNameRepeated)
                            {
                                MessageBoxResult result = UniMessageBox.Show("已经存在相同名称的模板。保存当前模板将覆盖旧模板。是否要继续？", "新建模板", MessageBoxButton.YesNo, MessageBoxImage.Question);
                                if (result.Equals(MessageBoxResult.Yes))
                                {
                                    // 删除当前模板文件夹数据
                                    Directory.Delete(Path.Combine(ProjectTemplatesPath, TemplateName), true);

                                    // 在 CustomTemplate.xml 配置列表中删除当前项
                                    var templateNodes = rootNode.SelectNodes("Template");
                                    foreach (XmlElement template in templateNodes)
                                    {
                                        if (template.GetAttribute("TemplateName").Equals(TemplateName))
                                        {
                                            rootNode.RemoveChild(template);
                                        }
                                    }
                                }
                                else
                                {
                                    return;
                                }
                            }

                            // 在 CustomTemplate.xml 配置列表中追加
                            XmlElement templateElement = doc.CreateElement("Template");
                            templateElement.SetAttribute("TemplateName", TemplateName);
                            templateElement.SetAttribute("TemplateDescription", TemplateDescription);
                            templateElement.SetAttribute("DefaultProjectName", DefaultProjectName);
                            templateElement.SetAttribute("DefaultProjectDescription", DefaultProjectDescription);
                            templateElement.SetAttribute("TemplateDirectoryPath", Path.Combine(ProjectTemplatesPath, TemplateName));
                            rootNode.AppendChild(templateElement);
                            doc.Save(path);

                            // 拷贝当前项目到模板文件夹下
                            Plugins.Shared.Library.Librarys.DirectoryOperation.Copy(ViewModelLocator.instance.Project.ProjectPath, Path.Combine(ProjectTemplatesPath, TemplateName));

                            // 广播 CustomTemplate.xml 改变的消息，以重刷最近项目列表
                            Messenger.Default.Send(new MessengerObjects.CustomTemplateModify());

                            m_view.Close();
                            UniMessageBox.Show("当前项目已成功另存为模板。检查起始页面以查看它并将其用作其他项目的起点", "另存为模板", MessageBoxButton.OK, MessageBoxImage.Information);
                        }));
            }
        }
    }
}
