using System.Collections.Generic;
using System.Activities;
using System.ComponentModel;
using System.Activities.Presentation.Metadata;
using Plugins.Shared.Library.Editors;
using System.Activities.Presentation.PropertyEditing;
using Plugins.Shared.Library.Librarys;
using System.Linq;
using Python.Runtime;
using Plugins.Shared.Library;
using System;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace WorkflowUtils
{
    //待改动调用Ironpython执行
    [Designer(typeof(InvokePythonFileDesigner))]
    public sealed class InvokePythonFileActivity : CodeActivity
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
        [DisplayName("脚本路径")]
        [Description("Python 脚本文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> PythonFilePath { get; set; }

        [Category("输入")]
        [DisplayName("工作目录")]
        [Description("Python 脚本文件执行时的工作目录，默认为当前项目目录。必须将文本放入引号中。")]
        public InArgument<string> PythonWorkingDirectory { get; set; }

        [Category("输入")]
        [DisplayName("参数集")]
        public Dictionary<string, Argument> Arguments { get; set; } = new Dictionary<string, Argument>();

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/WorkflowUtils/python.png";
            }
        }

        #endregion


        private static string PythonHome;

        static InvokePythonFileActivity()
        {
            //if (!PythonEngine.IsInitialized)
            //{
            //    PythonHome = AppDomain.CurrentDomain.BaseDirectory + @"Python";
            //    Environment.SetEnvironmentVariable("PATH", PythonHome + ";" + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine), EnvironmentVariableTarget.Process);
            //    PythonEngine.PythonHome = PythonHome;

            //    PythonEngine.Initialize();
            //}
        }

        public InvokePythonFileActivity()
        {
            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(InvokePythonScriptActivity), "Arguments", new EditorAttribute(typeof(DictionaryArgumentEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = MouseActivity.Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = MouseActivity.Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);


            IntPtr ts = IntPtr.Zero;

            try
            {
                ts = PythonEngine.BeginAllowThreads();
                using (Py.GIL())
                {
                    using (var ps = Py.CreateScope())
                    {
                        //入参设置
                        Dictionary<string, object> inArguments = (from argument in Arguments
                                                                  where argument.Value.Direction != ArgumentDirection.Out
                                                                  select argument).ToDictionary((KeyValuePair<string, Argument> argument) => argument.Key, (KeyValuePair<string, Argument> argument) => argument.Value.Get(context));
                        foreach (var arg in inArguments)
                        {
                            ps.Set(arg.Key, arg.Value);
                        }

                        using (var scope = ps.NewScope())
                        {
                            PyObject pyObj = PythonPrintRedirectObject.instance.ToPython();
                            dynamic sys = Py.Import("sys");
                            sys.stdout = pyObj;

                            dynamic os = Py.Import("os");
                            string workDir = PythonWorkingDirectory.Get(context);
                            if (string.IsNullOrEmpty(workDir))
                            {
                                os.chdir(SharedObject.Instance.ProjectPath);//设置python运行时的默认当前目录为项目目录
                            }
                            else
                            {
                                sys.path.append(workDir);
                                // os.chdir(workDir);
                            }

                            //由于是32 bit的python，耗内存操作可能会报错(如aircv.find_sift(imsrc, imsch)会内存分配报错)
                            string pythonFilePath = PythonFilePath.Get(context);
                            scope.Exec(System.IO.File.ReadAllText(pythonFilePath));

                            //出参设置
                            Dictionary<string, object> outArguments = (from argument in Arguments
                                                                       where argument.Value.Direction != ArgumentDirection.In
                                                                       select argument).ToDictionary((KeyValuePair<string, Argument> argument) => argument.Key, (KeyValuePair<string, Argument> argument) => argument.Value.Get(context));

                            foreach (var arg in outArguments)
                            {
                                Type argumentType = Arguments[arg.Key].ArgumentType;

                                var argVal = scope.Get(arg.Key).AsManagedObject(argumentType);

                                if (arg.Value != null && !argumentType.IsAssignableFrom(argVal.GetType()))
                                {
                                    Arguments[arg.Key].Set(context, JsonParser.DeserializeArgument(argVal, argumentType));
                                }
                                else
                                {
                                    Arguments[arg.Key].Set(context, argVal);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }
            finally
            {
                if (ts != IntPtr.Zero)
                {
                    PythonEngine.EndAllowThreads(ts);
                }
            }

            Thread.Sleep(delayAfter);
        }
    }



}
