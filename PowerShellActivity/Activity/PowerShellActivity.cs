using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using Plugins.Shared.Library.Editors;
using System.Activities.Presentation;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Management.Automation.Runspaces;
using System.Collections;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;

namespace PowerShellActivity
{
    [Designer(typeof(PowerShellDesigner))]
    public sealed class ShellActivity<T> : CodeActivity
    {
        #region 属性分类：常见

        public string _displayName;
        [Category("常见")]
        [DisplayName("显示名称")]
        public new string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    _displayName = base.DisplayName;
                }
                else
                {
                    base.DisplayName = _displayName;
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }
        [Browsable(false)]
        public string _DisplayName
        {
            get
            {
                return this.DisplayName;
            }
        }

        [Category("常见")]
        [DisplayName("出错时继续")]
        [Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        public bool ContinueOnError { get; set; }

        [Category("常见")]
        [DisplayName("在此之前延迟")]
        [Description("活动开始执行任何操作之前的延迟时间（以毫秒为单位）。默认时间量为200毫秒。")]
        public InArgument<int> DelayBefore { get; set; }

        [Category("常见")]
        [DisplayName("在此之后延迟")]
        [Description("执行活动之后的延迟时间（以毫秒为单位）。默认时间量为300毫秒。")]
        public InArgument<int> DelayAfter { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("命令文本")]
        [Description("要执行的 PowerShell 命令。必须将文本放入引号中。")]
        public InArgument<string> CommandText { get; set; }

        [Category("输入")]
        [DisplayName("输入")]
        [Description("PSObject 的集合，它们将被传递给用于执行命令的管道编写器。可以是另一个“调用 PowerShell”活动的输出。")]
        public InArgument<Collection<PSObject>> Input { get; set; }

        [Category("输入")]
        [DisplayName("参数")]
        [Description("PowerShell 命令参数的字典。")]
        public Dictionary<string, InArgument> parameters { get; private set; } = new Dictionary<string, InArgument>();

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("异步")]
        [Description("是否异步")]
        public bool IsAsync { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("输出")]
        [Description("执行命令后返回的类型参数对象的集合。可用于传递多个“调用 PowerShell”活动。")]
        public OutArgument<Collection<T>> Output { get; set; }

        #endregion


        #region 属性分类：杂项

        bool _isScript = false;
        [DisplayName("是脚本")]
        [Description("指定命令文本是否为脚本。")]
        public bool IsScript
        {
            get
            {
                return _isScript;
            }
            set
            {
                _isScript = value;
            }
        }

        [DisplayName("PowerShell 变量")]
        [Description("命名对象的字典，表示在命令的当前会话中使用的变量。PowerShell 命令可以从“输入”和“输入/输出”变量中检索信息，并可以设置“输出”变量。")]
        public Dictionary<string, Argument> PowerShellVariables { get; private set; } = new Dictionary<string, Argument>();

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/PowerShell/powershell.png";
            }
        }

        #endregion


        static ShellActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            Type attrType = Type.GetType("System.Activities.Presentation.FeatureAttribute, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            Type argType = Type.GetType("System.Activities.Presentation.UpdatableGenericArgumentsFeature, System.Activities.Presentation, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            Type psType = Type.GetType("");
            builder.AddCustomAttributes(typeof(ShellActivity<>), new Attribute[] { Activator.CreateInstance(attrType, new object[] { argType, }) as Attribute });
            builder.AddCustomAttributes(typeof(ShellActivity<>), new DefaultTypeArgumentAttribute(typeof(object)));
            builder.AddCustomAttributes(typeof(ShellActivity<>), "parameters", new EditorAttribute(typeof(DictionaryArgumentEditor), typeof(DialogPropertyValueEditor)));
            builder.AddCustomAttributes(typeof(ShellActivity<>), "PowerShellVariables", new EditorAttribute(typeof(DictionaryArgumentEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            Runspace runspace = null;
            Pipeline pipeline = null;
            try
            {
                runspace = RunspaceFactory.CreateRunspace();
                runspace.Open();
                pipeline = runspace.CreatePipeline();
                string _commandText = CommandText.Get(context);
                Command cmd = new Command(_commandText, this.IsScript);
                if (this.parameters != null)
                {
                    foreach (KeyValuePair<string, InArgument> parameter in this.parameters)
                    {
                        if (parameter.Value.Expression != null)
                        {
                            cmd.Parameters.Add(parameter.Key, parameter.Value.Get(context));
                        }
                        else
                        {
                            cmd.Parameters.Add(parameter.Key, true);
                        }
                    }
                }
                if (this.PowerShellVariables != null)
                {
                    foreach(KeyValuePair<string, Argument> powerShellVariable in this.PowerShellVariables)
                    {
                        if ((powerShellVariable.Value.Direction == ArgumentDirection.In) || (powerShellVariable.Value.Direction == ArgumentDirection.InOut))
                        {
                            runspace.SessionStateProxy.SetVariable(powerShellVariable.Key, powerShellVariable.Value.Get(context));
                        }
                    }
                }   

                pipeline.Commands.Add(cmd);
                IEnumerable pipelineInput = this.Input.Get(context);
                if (pipelineInput != null)
                {
                    foreach (object inputItem in pipelineInput)
                    {
                        pipeline.Input.Write(inputItem);
                    }
                }
                pipeline.Input.Close();
                Collection<T> _result= new Collection<T>();
                if (IsAsync)
                {
                    Output.Set(context, _result);
                    pipeline.InvokeAsync();
                }
                else
                {
                    T result_;
                    foreach (PSObject result in pipeline.Invoke())
                    {
                        result_ = (T)result.BaseObject;
                        _result.Add(result_);
                    }
                    Output.Set(context, _result);
                    pipeline.Dispose();
                    runspace.Close();
                }
            }
            catch (Exception e)
            {

                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (runspace != null)
                {
                    runspace.Dispose();
                }

                if (pipeline != null)
                {
                    pipeline.Dispose();
                }
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }
    }
}
