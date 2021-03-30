using System;
using System.Collections.Generic;
using System.Linq;
using System.Activities;
using System.ComponentModel;
using System.Activities.Presentation.Metadata;
using Plugins.Shared.Library.Editors;
using System.Activities.Presentation.PropertyEditing;
using Python.Runtime;
using Plugins.Shared.Library.Librarys;
using Plugins.Shared.Library;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace WorkflowUtils
{
    /// <summary>
    /// 注意pythonnet库(Python.Runtime.dll)由于UTF8的问题在加载文件内容并执行时报错，做了修改)
    /// 修改代码如下
    /// [DllImport(_PythonDll, CallingConvention = CallingConvention.Cdecl)]
    /// internal static extern int PyRun_SimpleString([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))] string code);//我修改为UTF8编码
    /// [DllImport(_PythonDll, CallingConvention = CallingConvention.Cdecl)]
    /// internal static extern IntPtr PyRun_String([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))] string code, IntPtr st, IntPtr globals, IntPtr locals);//我修改为UTF8编码
    /// [DllImport(_PythonDll, CallingConvention = CallingConvention.Cdecl)]
    /// internal static extern IntPtr Py_CompileString([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Utf8Marshaler))] string code, string file, IntPtr tok);//我修改为UTF8编码
    /// </summary>

        //待改动调用Ironpython执行

    [Designer(typeof(InvokePythonScriptDesigner))]
    public sealed class InvokePythonScriptActivity : CodeActivity
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
        [DisplayName("代码")]
        public string Code { get; set; } = "";

        [Category("输入")]
        [DisplayName("工作目录")]
        [Description("Python 脚本代码执行时的工作目录，默认为当前项目目录。必须将文本放入引号中。")]
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

        static InvokePythonScriptActivity()
        {

            //if (!PythonEngine.IsInitialized)
            //{
            //    PythonHome = AppDomain.CurrentDomain.BaseDirectory + @"Python";
            //    Environment.SetEnvironmentVariable("PATH", PythonHome + ";" + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine), EnvironmentVariableTarget.Process);
            //    PythonEngine.PythonHome = PythonHome;

            //    PythonEngine.Initialize();
            //}
        }

        public InvokePythonScriptActivity()
        {
            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(InvokePythonScriptActivity), "Arguments", new EditorAttribute(typeof(DictionaryArgumentEditor), typeof(DialogPropertyValueEditor)));
            builder.AddCustomAttributes(typeof(InvokePythonScriptActivity), "Code", new EditorAttribute(typeof(PythonScriptEditor), typeof(DialogPropertyValueEditor)));
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
                                os.chdir(workDir);
                            }


                            //由于是32 bit的python，耗内存操作可能会报错(如aircv.find_sift(imsrc, imsch)会内存分配报错)
                            scope.Exec(Code);

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
