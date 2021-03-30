using Plugins.Shared.Library;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace WorkflowUtils
{
    // InvokeWorkflowFileDesigner.xaml 的交互逻辑
    public partial class InvokeWorkflowFileDesigner
    {
        public InvokeWorkflowFileDesigner()
        {
            InitializeComponent();
        }

        private string GetInArgumentStringValue(InArgument<string> arg)
        {
            if (arg == null)
            {
                return "";
            }

            return ((System.Activities.Expressions.Literal<string>)arg.Expression).Value;
        }

        //private void EditArgumentsBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    ModelItem mi = this.ModelItem.Properties["Arguments"].Dictionary;
        //    var options = new DynamicArgumentDesignerOptions()
        //    {
        //        Title = "编辑工作流参数"
        //    };

        //    DynamicArgumentDialog.ShowDialog(this.ModelItem, mi, Context, this.ModelItem.View, options);
        //}
        public static ArgumentDirection DirectionFromType(Type argumentType)
        {
            if (typeof(InArgument).IsAssignableFrom(argumentType))
            {
                return ArgumentDirection.In;
            }
            if (typeof(OutArgument).IsAssignableFrom(argumentType))
            {
                return ArgumentDirection.Out;
            }
            if (typeof(InOutArgument).IsAssignableFrom(argumentType))
            {
                return ArgumentDirection.InOut;
            }
            return ArgumentDirection.In;
        }
        private void EditArgumentsBtn_Click(object sender, RoutedEventArgs e)
        {
            var workflowFilePathArg = ModelItem.Properties["WorkflowFilePath"].ComputedValue as InArgument<string>;

            var workflowFilePath = "";
            if (workflowFilePathArg != null)
            {
                //TODO WJF 此处该转换不确定是否正确，准确用法需要Get(Context)这种用法
                workflowFilePath = GetInArgumentStringValue(workflowFilePathArg);

                //转成绝对路径
                //如果workflowFilePath不是绝对路径，则转成绝对路径
                if (!System.IO.Path.IsPathRooted(workflowFilePath))
                {
                    workflowFilePath = System.IO.Path.Combine(SharedObject.Instance.ProjectPath, workflowFilePath);
                }
            }

            ModelItemDictionary mi = this.ModelItem.Properties["Arguments"].Dictionary;

            Dictionary<string, Argument> argDict = new Dictionary<string, Argument>();

            DynamicActivity activity = null;
            if (!string.IsNullOrEmpty(workflowFilePath))
            {
                activity = ActivityXamlServices.Load(workflowFilePath) as DynamicActivity;
                if (activity == null)
                {
                    return;
                }
                foreach (var prop in activity.Properties)
                {
                    if (!argDict.ContainsKey(prop.Name))
                    {
                        if (mi == null || !mi.ContainsKey(prop.Name) || mi[prop.Name]?.GetCurrentValue() == null)
                        {
                            argDict.Add(prop.Name, (Argument)Activator.CreateInstance(prop.Type));
                        }
                        else
                        {
                            var arg = (Argument) mi[prop.Name].GetCurrentValue();
                            if (DirectionFromType(prop.Type)==ArgumentDirection.In)
                            {
                                var newArg = Argument.Create(arg.ArgumentType, ArgumentDirection.In);
                                newArg.Expression = arg.Expression;
                                argDict.Add(prop.Name, newArg);
                            }
                            else if (DirectionFromType(prop.Type) == ArgumentDirection.Out)
                            {
                                var newArg = Argument.Create(arg.ArgumentType, ArgumentDirection.Out);
                                newArg.Expression = arg.Expression;
                                argDict.Add(prop.Name, newArg);
                            }
                            else if (DirectionFromType(prop.Type) == ArgumentDirection.InOut)
                            {
                                var newArg = Argument.Create(arg.ArgumentType, ArgumentDirection.InOut);
                                newArg.Expression = arg.Expression;
                                argDict.Add(prop.Name, newArg);
                            }
                            else
                            {
                                argDict.Add(prop.Name, arg);
                            }
                        }
                    }
                }
            }
            var options = new DynamicArgumentDesignerOptions()
            {
                Title = "导入工作流参数"
            };

            ModelTreeManager mtm = new ModelTreeManager(new EditingContext());
            mtm.Load(argDict);

            if (DynamicArgumentDialog.ShowDialog(this.ModelItem, mtm.Root, Context, this.ModelItem.View, options))
            {
                var saveArgDict = this.ModelItem.Properties["Arguments"].Dictionary;
                saveArgDict.Clear();
                foreach (var item in argDict)
                {
                    if (activity.Properties.Contains(item.Key))
                    {
                        activity.Properties[item.Key].Value = item.Value;
                    }
                    else
                    {
                        activity.Properties.Add(new DynamicActivityProperty
                        {
                            Name = item.Key,
                            Type = item.Value.GetType(),
                            Value = item.Value
                        });
                    }
                    saveArgDict.Add(item.Key, item.Value);
                }
            }
        }

        private void BrowserBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "工作流文件 (*.xaml)|*.xaml|所有文件 (*.*)|*.*";
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = SharedObject.Instance.ProjectPath;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                if (filePath.StartsWith(SharedObject.Instance.ProjectPath, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    //如果在项目目录下，则使用相对路径保存
                    filePath = Common.MakeRelativePath(SharedObject.Instance.ProjectPath, filePath);
                }

                ModelItem.Properties["WorkflowFilePath"].SetValue(new InArgument<string>(filePath));
            }
        }

    }
}
