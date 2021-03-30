using System.Windows;
using UniExecutor.Core.Models;
using UniNamedPipe;

namespace UniExecutor
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        public void Init(JobModel jobModel)
        {
            var context = new ExecutorContext(jobModel);
            Configure.ConfigureServer();
        }
    }
}
