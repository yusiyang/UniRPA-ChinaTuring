using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System.Activities.Presentation;
using System.Activities.Presentation.View;
using System.Activities.Statements;
using System.Collections.ObjectModel;
using System;
using UniStudio.Executor;
using UniStudio.UserControls;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using Xceed.Wpf.AvalonDock.Layout;
using UniStudio.Librarys;
using ActiproSoftware.Windows.Controls.Docking;
using System.Windows.Media;
using System.Windows.Controls;
using UniStudio.Executor.Services;
using UniExecutor.Service.Interface;
using UniStudio.WorkflowOperation;
using System.Linq;
using System.Activities.Presentation.Services;
using Plugins.Shared.Library.Extensions;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace UniStudio.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class DockViewModel : ViewModelBase
    {
        public DockContent m_view { get; set; }

        private LayoutAnchorable m_layoutAnchorable { get; set; }
        private ToolWindow m_toolWindow { get; set; }

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
                        m_view = (DockContent)p.Source;
                    }));
            }
        }


        /// <summary>
        /// Initializes a new instance of the DockViewModel class.
        /// </summary>
        public DockViewModel()
        {
            Messenger.Default.Register<DocumentViewModel>(this, "IsSelected", (doc) =>
            {
                WorkflowPropertyView = doc.WorkflowDesignerInstance.PropertyInspectorView;
                WorkflowOutlineView = doc.WorkflowDesignerInstance.OutlineView;
                ActiveDocument = doc;
            });


            Messenger.Default.Register<DocumentViewModel>(this, "Close", (doc) =>
            {
                Documents.Remove(doc);

                if (Documents.Count == 0)
                {
                    //文档全关闭时，设置大纲视图为空
                    WorkflowOutlineView = null;
                }

                Messenger.Default.Send(this, "DocumentsCountChanged");
            });

            Messenger.Default.Register<IDebuggerService>(this, "BeginRun", BeginRun);
            Messenger.Default.Register<ViewOperate>(this, "EndRun", EndRun);
        }

        private void BeginRun(IDebuggerService obj)
        {
            //m_layoutAnchorable = new LayoutAnchorable();
            //m_layoutAnchorable.CanClose = false;
            //m_layoutAnchorable.CanHide = false;
            //m_layoutAnchorable.Title = "变量跟踪";
            //m_layoutAnchorable.IsActive = true;
            //m_layoutAnchorable.Content = new LocalsContent();
            //m_view.m_leftLayoutAnchorablePane.Children.Add(m_layoutAnchorable);

            m_toolWindow = new ToolWindow();
            m_toolWindow.CanClose = false;
            m_toolWindow.CanBecomeDocument = false;
            m_toolWindow.ContainerDockedSize = new Size(300, 0);
            m_toolWindow.Title = "变量跟踪";
            m_toolWindow.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resource/Image/Dock/var-track.png"));
            m_toolWindow.IsActive = true;
            m_toolWindow.Content = new LocalsContent();
            m_view._leftToolWindowContainer.Windows.Add(m_toolWindow);
            m_toolWindow.Activate();

            LastActiveDocumentStack.Clear();
        }

        private void EndRun(ViewOperate obj)
        {
            Common.RunInUI(() =>
            {
                //m_view.m_leftLayoutAnchorablePane.Children.Remove(m_layoutAnchorable);
                //m_layoutAnchorable = null;
                if (m_view._leftToolWindowContainer.Windows.Contains(m_toolWindow))
                {
                    m_view._leftToolWindowContainer.Windows.Remove(m_toolWindow);
                }
                m_toolWindow = null;
            });
        }


        /// <summary>
        /// The <see cref="Documents" /> property's name.
        /// </summary>
        public const string DocumentsPropertyName = "Documents";

        private ObservableCollection<DocumentViewModel> _documentsProperty = new ObservableCollection<DocumentViewModel>();

        /// <summary>
        /// Sets and gets the Documents property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public ObservableCollection<DocumentViewModel> Documents
        {
            get
            {
                return _documentsProperty;
            }

            set
            {
                if (_documentsProperty == value)
                {
                    return;
                }

                _documentsProperty = value;
                RaisePropertyChanged(DocumentsPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="LastActiveDocumentStack" /> property's name.
        /// </summary>
        public const string LastActiveDocumentStackPropertyName = "LastActiveDocumentStack";

        private Stack<DocumentViewModel> _lastActiveDocumentStackProperty = new Stack<DocumentViewModel>();

        /// <summary>
        /// Sets and gets the LastActiveDocumentStack property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Stack<DocumentViewModel> LastActiveDocumentStack
        {
            get
            {
                return _lastActiveDocumentStackProperty;
            }

            set
            {
                if (_lastActiveDocumentStackProperty == value)
                {
                    return;
                }

                _lastActiveDocumentStackProperty = value;
                RaisePropertyChanged(LastActiveDocumentStackPropertyName);
            }
        }

        /// <summary>
        /// The <see cref="ActiveDocument" /> property's name.
        /// </summary>
        public const string ActiveDocumentPropertyName = "ActiveDocument";

        private DocumentViewModel _activeDocumentProperty = null;

        /// <summary>
        /// Sets and gets the ActiveDocument property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public DocumentViewModel ActiveDocument
        {
            get
            {
                return _activeDocumentProperty;
            }

            set
            {
                if (_activeDocumentProperty == value)
                {
                    return;
                }
                _activeDocumentProperty = value;
                RaisePropertyChanged(ActiveDocumentPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="WorkflowPropertyView" /> property's name.
        /// </summary>
        public const string WorkflowPropertyViewPropertyName = "WorkflowPropertyView";

        private object _workflowPropertyViewProperty = null;

        /// <summary>
        /// Sets and gets the WorkflowPropertyView property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public object WorkflowPropertyView
        {
            get
            {
                return _workflowPropertyViewProperty;
            }

            set
            {
                if (_workflowPropertyViewProperty == value)
                {
                    return;
                }

                _workflowPropertyViewProperty = value;
                RaisePropertyChanged(WorkflowPropertyViewPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="CustomWorkflowPropertyView" /> property's name.
        /// </summary>
        public const string CustomWorkflowPropertyViewPropertyName = "CustomWorkflowPropertyView";

        private object _customWorkflowPropertyViewProperty = null;

        /// <summary>
        /// Sets and gets the WorkflowPropertyView property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public object CustomWorkflowPropertyView
        {
            get
            {
                return _customWorkflowPropertyViewProperty;
            }

            set
            {
                if (_customWorkflowPropertyViewProperty == value)
                {
                    return;
                }

                _customWorkflowPropertyViewProperty = value;
                RaisePropertyChanged(CustomWorkflowPropertyViewPropertyName);
            }
        }


        /// <summary>
        /// The <see cref="WorkflowOutlineView" /> property's name.
        /// </summary>
        public const string WorkflowOutlineViewPropertyName = "WorkflowOutlineView";

        private object _workflowOutlineViewProperty = null;

        /// <summary>
        /// Sets and gets the WorkflowOutlineView property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public object WorkflowOutlineView
        {
            get
            {
                return _workflowOutlineViewProperty;
            }

            set
            {
                if (_workflowOutlineViewProperty == value)
                {
                    return;
                }

                _workflowOutlineViewProperty = value;
                RaisePropertyChanged(WorkflowOutlineViewPropertyName);
            }
        }


        public void NewSequenceDocument(string title = "未命名文档", string xamlPath = "", bool isReadOnly = false, EventHandler validationCompleted = null)
        {
            DocumentViewModel doc = null;
            try
            {
                doc = new DocumentViewModel(title, xamlPath, isReadOnly, validationCompleted);
            }
            catch
            {
                UniMessageBox.Show("无效文档", "文档打开失败", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ViewModelLocator.instance.Dock.Documents.Add(doc);
            doc.IsSelected = true;
            Messenger.Default.Send(this, "DocumentsCountChanged");
        }
    }
}