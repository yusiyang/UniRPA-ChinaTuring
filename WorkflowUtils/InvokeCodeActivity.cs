using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Activities.Expressions;
using System.Activities.Presentation.Metadata;
using Plugins.Shared.Library.Editors;
using System.Activities.Presentation.PropertyEditing;
using Plugins.Shared.Library.Extensions;
using System.Threading;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;

namespace WorkflowUtils
{
    [Designer(typeof(InvokeCodeDesigner))]
    public sealed class InvokeCodeActivity : CodeActivity
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

        [RequiredArgument]
        [Category("输入")]
        [DisplayName("代码")]
        public string Code { get; set; }

        [Category("输入")]
        [DisplayName("参数集")]
        public Dictionary<string, Argument> Arguments { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string CompilationError { get; set; }
        
        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/WorkflowUtils/invoke-code.png";
            }
        }

        #endregion


        private static Dictionary<string, CompilerRunner> codeRunnerCache = new Dictionary<string, CompilerRunner>(25);

        private static object codeRunnerCacheLock = new object();

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (!string.IsNullOrWhiteSpace(CompilationError))
            {
                metadata.AddValidationError(CompilationError);
            }
        }

        public InvokeCodeActivity()
        {
            Arguments = new Dictionary<string, Argument>();

            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(InvokeCodeActivity), "Arguments", new EditorAttribute(typeof(DictionaryArgumentEditor), typeof(DialogPropertyValueEditor)));
            builder.AddCustomAttributes(typeof(InvokeCodeActivity), "Code", new EditorAttribute(typeof(VBNetCodeEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        public static string GetImports(IEnumerable<string> imports)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string import in imports)
            {
                if (!string.IsNullOrWhiteSpace(import))
                {
                    stringBuilder.AppendLine($"Imports {import}");
                }
            }
            return stringBuilder.ToString();
        }

        public static string GetVbNetArguments(List<Tuple<string, Type, ArgumentDirection>> inArgs)
        {
            string text = "";
            foreach (Tuple<string, Type, ArgumentDirection> inArg in inArgs)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    text += ", ";
                }
                string arg = "";
                switch (inArg.Item3)
                {
                    case ArgumentDirection.In:
                        arg = "ByVal";
                        break;
                    case ArgumentDirection.Out:
                    case ArgumentDirection.InOut:
                        arg = "ByRef";
                        break;
                }
                text += $"{arg} {inArg.Item1} As {GetVbNetTypeName(inArg.Item2)}";
            }
            return text;
        }

        private static CompilerRunner GetCompilerRunner(string userCode, List<Tuple<string, Type, ArgumentDirection>> args, string imps)
        {
            CompilerRunner value = null;
            string vbFunctionCode = GetVbFunctionCode(userCode, args);
            lock (codeRunnerCacheLock)
            {
                if (codeRunnerCache.TryGetValue(vbFunctionCode, out value))
                {
                    return value;
                }
                Tuple<string, string, int> vbModuleCode = GetVbModuleCode(vbFunctionCode, imps);
                value = new CompilerRunner(vbModuleCode.Item1, vbModuleCode.Item2, "Run", vbModuleCode.Item3);
                codeRunnerCache.Add(vbFunctionCode, value);
                return value;
            }
        }

        public static CompilerRunner CreateCompilerRunner(string userCode, string imps, List<Tuple<string, Type, ArgumentDirection>> args)
        {
            Tuple<string, string, int> vbModuleCode = GetVbModuleCode(GetVbFunctionCode(userCode, args), imps);
            return new CompilerRunner(vbModuleCode.Item1, vbModuleCode.Item2, "Run", vbModuleCode.Item3, generateInMemory: false);
        }

        private IList<string> GetImports(Activity workflow)
        {
            return TextExpression.GetNamespacesForImplementation(workflow) ?? new string[0];
        }


        private Activity GetRootActivity(CodeActivityContext context)
        {
            var ext = context.GetExtension<WorkflowRuntime>();
            return ext?.GetRootActivity();
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            IList<string> imports = GetImports(GetRootActivity(context));

            string code = Code;
            List<Tuple<string, Type, ArgumentDirection>> list = new List<Tuple<string, Type, ArgumentDirection>>(Arguments.Count);
            object[] array = new object[Arguments.Count];
            int num = 0;
            foreach (KeyValuePair<string, Argument> argument2 in Arguments)
            {
                list.Add(new Tuple<string, Type, ArgumentDirection>(argument2.Key, argument2.Value.ArgumentType, argument2.Value.Direction));
                array[num++] = argument2.Value.Get(context);
            }
            try
            {
                string imports2 = GetImports(imports);
                GetCompilerRunner(code, list, imports2).Run(array);
                int num3 = 0;
                foreach (Tuple<string, Type, ArgumentDirection> item in list)
                {
                    Argument argument = Arguments[item.Item1];
                    if (argument.Direction != 0)
                    {
                        argument.Set(context, array[num3]);
                    }
                    num3++;
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

            Thread.Sleep(delayAfter);
        }


        private static Tuple<string, string, int> GetVbModuleCode(string funcCode, string imps)
        {
            int num = imps.Count((char c) => c == '\n');
            string text = GenerateRandomSufix();
            return new Tuple<string, string, int>($"Option Explicit\r\nOption Strict\r\n{imps}Module RPACodeRunner_{text}\r\n{funcCode}\r\nEnd Module", "RPACodeRunner_" + text, 4 + num);
        }

        private static string GetVbFunctionCode(string userCode, List<Tuple<string, Type, ArgumentDirection>> inArgs)
        {
            string vbNetArguments = GetVbNetArguments(inArgs);
            return $"Sub Run({vbNetArguments})\r\n{userCode}\r\nEnd Sub";
        }

        private static string GenerateRandomSufix()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        private static string GetVbNetTypeName(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.FullName.Replace("[]", "()");
            }
            if (t.IsNested && t.DeclaringType.IsGenericType)
            {
                throw new NotImplementedException();
            }
            string str = t.FullName.Substring(0, t.FullName.IndexOf('`')) + "(Of ";
            int num = 0;
            Type[] genericArguments = t.GetGenericArguments();
            foreach (Type t2 in genericArguments)
            {
                if (num > 0)
                {
                    str += ", ";
                }
                str += GetVbNetTypeName(t2);
                num++;
            }
            return str + ")";
        }

        public void SetSuccessfulCompilation()
        {
            CompilationError = "";
        }

        public void SetCompilationError(string errorMessage)
        {
            CompilationError = errorMessage;
        }
    }

}
