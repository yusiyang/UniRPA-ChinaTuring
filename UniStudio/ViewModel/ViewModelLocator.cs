/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:UniStudio"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace UniStudio.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        public static ViewModelLocator instance;
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            instance = this;
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}
            SimpleIoc.Default.Register<SplashScreenViewModel>();

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<ActivitiesViewModel>();
            SimpleIoc.Default.Register<DockViewModel>();
            SimpleIoc.Default.Register<StartViewModel>();
            SimpleIoc.Default.Register<ToolViewModel>();
            SimpleIoc.Default.Register<SettingViewModel>();
            SimpleIoc.Default.Register<HelpViewModel>();
            SimpleIoc.Default.Register<SnippetsViewModel>();
            SimpleIoc.Default.Register<NewProjectViewModel>();
            SimpleIoc.Default.Register<NewTemplateViewModel>();
            SimpleIoc.Default.Register<ProjectViewModel>();
            SimpleIoc.Default.Register<ProjectSettingsViewModel>();
            SimpleIoc.Default.Register<NewFolderViewModel>();
            SimpleIoc.Default.Register<RenameViewModel>();
            SimpleIoc.Default.Register<NewXamlFileViewModel>();
            SimpleIoc.Default.Register<OutputViewModel>();
            SimpleIoc.Default.Register<MessageDetailsViewModel>();
            SimpleIoc.Default.Register<LocalsViewModel>();
            SimpleIoc.Default.Register<PublishProjectViewModel>();
            SimpleIoc.Default.Register<PublishLibraryViewModel>();
            SimpleIoc.Default.Register<ManagePackagesViewModel>();
            SimpleIoc.Default.Register<PackageManagerViewModel>();
            SimpleIoc.Default.Register<ExtractDataTableViewModel>();
            SimpleIoc.Default.Register<RegisterViewModel>();
            SimpleIoc.Default.Register<StartupViewModel>();
            SimpleIoc.Default.Register<SearchViewModel>();
            SimpleIoc.Default.Register<SearchResultViewModel>();
            SimpleIoc.Default.Register<MyAppsViewModel>();
        }

        public SplashScreenViewModel SplashScreen
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SplashScreenViewModel>();
            }
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public ActivitiesViewModel Activities
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ActivitiesViewModel>();
            }
        }

        public SnippetsViewModel Snippets
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SnippetsViewModel>();
            }
        }



        public DockViewModel Dock
        {
            get
            {
                return ServiceLocator.Current.GetInstance<DockViewModel>();
            }
        }

        public StartViewModel Start
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StartViewModel>();
            }
        }

        public ToolViewModel Tool
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ToolViewModel>();
            }
        }

        public SettingViewModel Setting
        {
            get
            {
                return ServiceLocator.Current.GetInstance<SettingViewModel>();
            }
        }
        public HelpViewModel Help
        {
            get
            {
                return ServiceLocator.Current.GetInstance<HelpViewModel>();
            }
        }

        public ProjectViewModel Project
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ProjectViewModel>();
            }
        }

        public OutputViewModel Output
        {
            get
            {
                return ServiceLocator.Current.GetInstance<OutputViewModel>();
            }
        }

        public StartupViewModel Startup
        {
            get
            {
                return ServiceLocator.Current.GetInstance<StartupViewModel>();
            }
        }
        public RegisterViewModel Register
        {
            get
            {
                return ServiceLocator.Current.GetInstance<RegisterViewModel>();
            }
        }

        public MyAppsViewModel MyApps
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MyAppsViewModel>();
            }
        }


        //注意，这些窗口非长久存在，每次打开后要重建DataContext，所以要传唯一KEY System.Guid.NewGuid().ToString(){{{{{
        //请使用SimpleIoc.Default.GetInstanceWithoutCaching<T>()替代ServiceLocator.Current.GetInstance<T>(System.Guid.NewGuid().ToString())
        //后者每次生成新的都会保留在IOC容器中，不会自动释放；

        public NewProjectViewModel NewProject
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<NewProjectViewModel>();//ServiceLocator.Current.GetInstance<NewProjectViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public NewTemplateViewModel NewTemplate
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<NewTemplateViewModel>();//ServiceLocator.Current.GetInstance<NewTemplateViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public ProjectSettingsViewModel ProjectSettings
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<ProjectSettingsViewModel>();//ServiceLocator.Current.GetInstance<ProjectSettingsViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public NewFolderViewModel NewFolder
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<NewFolderViewModel>();//ServiceLocator.Current.GetInstance<NewFolderViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public RenameViewModel Rename
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<RenameViewModel>();//ServiceLocator.Current.GetInstance<RenameViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public NewXamlFileViewModel NewXamlFile
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<NewXamlFileViewModel>();//ServiceLocator.Current.GetInstance<NewXamlFileViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public MessageDetailsViewModel MessageDetails
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<MessageDetailsViewModel>();//ServiceLocator.Current.GetInstance<MessageDetailsViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public LocalsViewModel Locals
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<LocalsViewModel>();//ServiceLocator.Current.GetInstance<LocalsViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public PublishProjectViewModel PublishProject
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<PublishProjectViewModel>();//ServiceLocator.Current.GetInstance<PublishProjectViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public PublishLibraryViewModel PublishLibrary
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<PublishLibraryViewModel>();//ServiceLocator.Current.GetInstance<PublishLibraryViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public ManagePackagesViewModel ManagePackages
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<ManagePackagesViewModel>();//ServiceLocator.Current.GetInstance<ManagePackagesViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public PackageManagerViewModel PackageManager
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<PackageManagerViewModel>();//ServiceLocator.Current.GetInstance<PackageManagerViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public ExtractDataTableViewModel ExtractDataTable
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<ExtractDataTableViewModel>();//ServiceLocator.Current.GetInstance<ExtractDataTableViewModel>(System.Guid.NewGuid().ToString());
            }
        }


        public SearchViewModel Search
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<SearchViewModel>();//ServiceLocator.Current.GetInstance<SearchViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public SearchResultViewModel SearchResult
        {
            get
            {
                return SimpleIoc.Default.GetInstanceWithoutCaching<SearchResultViewModel>();//ServiceLocator.Current.GetInstance<SearchResultViewModel>(System.Guid.NewGuid().ToString());
            }
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}