using System;
using System.Windows.Forms;
using Uni.Core;

namespace Uni.GenerateWorkflow
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //数据库初始化
            DbContext.Initialize(false);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }
    }
}
