using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Editors;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;


namespace ExcelPlugins
{
    [Designer(typeof(RunVBADesigner))]
    public sealed class RunVBA : AsyncCodeActivity
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
        [DisplayName("VBA 名称")]
        [Description("指定的 VBA 名称。必须将文本放入引号中。")]
        public InArgument<string> VBAName { get; set; }

        private Dictionary<string, Argument> parameters;
        [Category("输入")]
        [DisplayName("参数")]
        [Browsable(true)]
        public Dictionary<string, Argument> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new Dictionary<string, Argument>();
                }
                return this.parameters;
            }
            set
            {
                parameters = value;
            }
        }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("返回值")]
        public OutArgument<string> ReturnValue
        {
            get; set;
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/vba.png"; } }

        [Browsable(false)]
        public string ClassName { get { return "RunVBA"; } }

        #endregion


        public RunVBA()
        {
            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(RunVBA), nameof(RunVBA.Parameters), new EditorAttribute(typeof(DictionaryArgumentEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return ClassName;
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            PropertyDescriptor property = context.DataContext.GetProperties()[ExcelCreate.GetExcelAppTag];
            Excel::Application excelApp = property.GetValue(context.DataContext) as Excel::Application;
            try
            {
                if (string.IsNullOrEmpty(VBAName.Get(context)))
                {
                    throw new System.Exception("请输入宏的名称");
                }

                object returnValue;
                string macroName = VBAName.Get(context);
                object[] parameters = null;
                if (Parameters != null)
                {
                    List<object> paraList = new List<object>();
                    foreach (var param in Parameters)
                    {
                        paraList.Add(param.Value.Get(context));
                    }
                    parameters = paraList.ToArray();
                }
                RunExcelMacro(excelApp, macroName, parameters, out returnValue);
                if (returnValue != null)
                    ReturnValue.Set(context, returnValue);
            }
            catch (Exception e)
            {
                new CommonVariable().realaseProcessExit(excelApp);
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }
            m_Delegate = new runDelegate(Run);

            return m_Delegate.BeginInvoke(callback, state);
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            Thread.Sleep(delayAfter);
        }

        public void RunExcelMacro(Excel::Application excelApp, string macroName, object[] parameters, out object rtnValue)
        {
            object oMissing = System.Reflection.Missing.Value;
            object[] paraObjects;
            if (parameters == null)
            {
                paraObjects = new object[] { macroName };
            }
            else
            {
                // 宏参数组长度  
                int paraLength = parameters.Length;

                paraObjects = new object[paraLength + 1];

                paraObjects[0] = macroName;
                for (int i = 0; i < paraLength; i++)
                {
                    paraObjects[i + 1] = parameters[i];
                }
            }

            rtnValue = "";
            try
            {
                rtnValue = excelApp.GetType().InvokeMember(
                    "Run",
                    System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    excelApp,
                    paraObjects
                );
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, "宏执行异常,请检查宏名称与参数是否匹配", e.Message);
                throw;
            }
            Excel._Workbook oBook = excelApp.ActiveWorkbook;
            oBook.Save();
        }
    }
}
