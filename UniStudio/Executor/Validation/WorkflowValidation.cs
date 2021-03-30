using Plugins.Shared.Library;
using Plugins.Shared.Library.Extensions;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Validation;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UniStudio.ViewModel;

namespace UniStudio.Executor.Validation
{
    public class WorkflowValidation
    {
        public static bool Validate(WorkflowDesigner workflowDesigner)
        {
            if(workflowDesigner==null)
            {
                throw new NullReferenceException(nameof(workflowDesigner));
            }

            workflowDesigner.Flush();
            Activity workflow = ActivityXamlServices.Load(new StringReader(workflowDesigner.Text));
            workflow.DisplayName = ViewModelLocator.instance.Project.ProjectName;//让报错信息能报出项目名

            return Validate(workflow);
        }

        public static bool Validate(Activity workflow)
        {
            var result = ActivityValidationServices.Validate(workflow);
            if (result.Errors.Count > 0)
            {
                foreach (var err in result.Errors)
                {
                    SharedObject.Instance.Output(SharedObject.OutputType.Error, err.Message);
                }

                UniMessageBox.Show(App.Current.MainWindow, "工作流校验错误，请检查参数配置", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
