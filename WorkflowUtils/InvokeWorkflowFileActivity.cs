using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.ComponentModel;
using Plugins.Shared.Library.Editors;
using System.Collections.Generic;
using Plugins.Shared.Library;
using System.Activities.XamlIntegration;
using System.Linq;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library.Extensions;
using System;
using System.Threading;
using UniExecutor.Core.Events;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.UiAutomation;

namespace WorkflowUtils
{
    [Designer(typeof(InvokeWorkflowFileDesigner))]
    public sealed class InvokeWorkflowFileActivity : CodeActivity
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
        [DisplayName("文件路径")]
        [Description("工作流文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> WorkflowFilePath { get; set; }

        [Category("输入")]
        [DisplayName("参数集")]
        public Dictionary<string, Argument> Arguments { get; private set; } = new Dictionary<string, Argument>();

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/WorkflowUtils/invoke-xaml-file.png";
            }
        }

        #endregion


        private string ProjectPath { get; set; }

        private AutoResetEvent _autoResetEvent;

        private CodeActivityContext _context;

        public InvokeWorkflowFileActivity()
        {
            ProjectPath = SharedObject.Instance.ProjectPath;
            _autoResetEvent = new AutoResetEvent(false);

            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(InvokeWorkflowFileActivity), "Arguments", new EditorAttribute(typeof(DictionaryArgumentEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }


        public void SetWorkflowFilePath(string filePath)
        {
            if (filePath.StartsWith(SharedObject.Instance.ProjectPath, System.StringComparison.CurrentCultureIgnoreCase))
            {
                //如果在项目目录下，则使用相对路径保存
                filePath = Common.MakeRelativePath(SharedObject.Instance.ProjectPath, filePath);
            }

            WorkflowFilePath = filePath;
        }

        /// <summary>
        /// 创建并验证活动的参数、变量、子活动和活动委托的说明。
        /// </summary>
        /// <param name="metadata"></param>
        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            foreach (KeyValuePair<string, Argument> argument2 in Arguments)
            {
                Argument value = argument2.Value;
                RuntimeArgument argument = new RuntimeArgument(argument2.Key, value.ArgumentType, value.Direction);
                metadata.Bind(value, argument);
                metadata.AddArgument(argument);
            }

            base.CacheMetadata(metadata);
        }

        // 如果活动返回值，则从 CodeActivity<TResult>
        // 并从 Execute 方法返回该值。
        protected override void Execute(CodeActivityContext context)
        {
            _context = context;
            int delayAfter = MouseActivity.Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = MouseActivity.Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            // 获取 Text 输入参数的运行时值
            string workflowFilePath = context.GetValue(this.WorkflowFilePath);
            //如果workflowFilePath不是绝对路径，则转成绝对路径
            if(!System.IO.Path.IsPathRooted(workflowFilePath))
            {
                workflowFilePath = System.IO.Path.Combine(ProjectPath, workflowFilePath);
            }

            try
            {
                Dictionary<string, object> inArguments = (from argument in Arguments
                                                          where argument.Value.Direction != ArgumentDirection.Out
                                                          select argument).ToDictionary(argument => argument.Key+"|"+argument.Value.ArgumentType.AssemblyQualifiedName, argument => argument.Value.Get(context));

                foreach (var argument in inArguments.Where(t=>t.Value!=null&&t.Value.GetType()==typeof(UiElement)))
                {
                    inArguments[argument.Key] = (argument.Value as UiElement)?.GlobalId;
                }

                var viewOperateService = SharedObject.Instance.ViewOperateService;

                viewOperateService.InternalEnded += ViewOperateService_InternalEnded;

                viewOperateService.InvokeWorkflow(workflowFilePath, inArguments);

                _autoResetEvent.WaitOne();
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }

        private void ViewOperateService_InternalEnded(object sender, InternalEndRunEventArgs e)
        {
            SharedObject.Instance.ViewOperateService.InternalEnded -= ViewOperateService_InternalEnded;

            var outputArguments =new Dictionary<string,object>();
            foreach (var argument in e.Outputs)
            {
                if (argument.Key.Contains("|"))
                {
                    var typeName = argument.Key.Split('|').Last();
                    var type = Type.GetType(typeName);
                    if (type == null)
                    {
                        continue;
                    }
                    var argName = argument.Key.Split('|').First();
                    object value;
                    if (type==typeof(UiElement))
                    {
                        if (argument.Value==null)
                        {
                            value = null;
                        }
                        else
                        {
                            value = UiElement.FromGlobalId(argument.Value.ToString());
                        }
                    }
                    else
                    {
                        value = Convert.ChangeType(argument.Value, type);
                    }
                    outputArguments.Add(argName, value);
                }
                else
                {
                    outputArguments.Add(argument.Key, argument.Value);
                }
            }


            foreach (KeyValuePair<string, object> item in from argument in outputArguments
                                                          where Arguments.ContainsKey(argument.Key)
                                                          select argument)
            {
                Type argumentType = Arguments[item.Key].ArgumentType;
                if (item.Value != null && !argumentType.IsAssignableFrom(item.Value.GetType()))
                {
                    Arguments[item.Key].Set(_context, JsonParser.DeserializeArgument(item.Value, argumentType));
                }
                else
                {
                    Arguments[item.Key].Set(_context, item.Value);
                }
            }
            _autoResetEvent.Set();
        }
    }
}
