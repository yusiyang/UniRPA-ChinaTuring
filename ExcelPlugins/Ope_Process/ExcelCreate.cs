using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.Extensions;
using System;
using System.Activities;
using System.Activities.Statements;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelPlugins
{
    [Designer(typeof(ExcelCreateDesigner))]
    public sealed class ExcelCreate : NativeActivity
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


        #region 属性分类：新建/打开文档选项

        InArgument<string> _PathUrl;
        [Category("新建/打开文档选项")]
        [DisplayName("文件路径")]
        [Description("要打开的Excel文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> PathUrl
        {
            get
            {
                return _PathUrl;
            }
            set
            {
                _PathUrl = value;
            }
        }

        bool _NewDoc;
        [Category("新建/打开文档选项")]
        [DisplayName("是否创建新文档")]
        public bool NewDoc
        {
            get
            {
                return _NewDoc;
            }
            set
            {
                _NewDoc = value;
            }
        }

        bool _IsVisible = true;
        [Category("新建/打开文档选项")]
        [DisplayName("流程是否可见")]
        public bool IsVisible
        {
            get { return _IsVisible; }
            set { _IsVisible = value; }
        }

        #endregion


        #region 属性分类：保存选项

        InArgument<string> _SavePathUrl;
        [Category("保存选项")]
        [DisplayName("文件路径")]
        [Description("要保存的Excel文件的全路径。必须将文本放入引号中。")]
        public InArgument<string> SavePathUrl
        {
            get
            {
                return _SavePathUrl;
            }
            set
            {
                _SavePathUrl = value;
            }
        }

        bool _IsExit = true;
        [Category("保存选项")]
        [DisplayName("程序是否退出")]
        public bool IsExit
        {
            get
            {
                return _IsExit;
            }
            set
            {
                _IsExit = value;
            }
        }

        bool _Save = true;
        [Category("保存选项")]
        [DisplayName("保存")]
        public bool Save
        {
            get
            {
                return _Save;
            }
            set
            {
                _Save = value;
            }
        }

        bool _SaveAs;
        [Category("保存选项")]
        [DisplayName("另存为")]
        public bool SaveAs
        {
            get
            {
                return _SaveAs;
            }
            set
            {
                _SaveAs = value;
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public ActivityAction<object> Body { get; set; }

        [Browsable(false)]
        public static string GetExcelAppTag { get { return "GetMail"; } }

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/excel/create.png"; } }

        #endregion


        public ExcelCreate()
        {
            Body = new ActivityAction<object>
            {
                Argument = new DelegateInArgument<object>(GetExcelAppTag),
                Handler = new Sequence()
                {
                    DisplayName = "Do"
                }
            };
        }

        private bool isPathAvailable(string path)
        {
            if (path == null)
                return false;
            string[] sArray = path.Split('\\');
            string dict = sArray[0] + '\\';
            System.Diagnostics.Debug.WriteLine("dict : " + dict);
            if (!Directory.Exists(dict))
                return false;
            else
                return true;
        }


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (this.NewDoc == false && this.PathUrl == null)
            {
                metadata.AddValidationError("非创建新文档需要添加文件的有效路径");
            }
            if ((this.SaveAs == true || this.NewDoc == true) && this.SavePathUrl == null)
            {
                metadata.AddValidationError("另存为或保存新文档需要添加有效的保存路径");
            }
        }

        private Excel::Application excelApp;
        protected override void Execute(NativeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string filePath = PathUrl.Get(context);

                excelApp = new Excel::Application();
                excelApp.Visible = IsVisible;
                excelApp.DisplayAlerts = false;

                if (_NewDoc == true)
                {
                    excelApp.Workbooks.Add(true);
                }
                else
                {
                    if (!File.Exists(filePath))
                    {
                        //SharedObject.Instance.Output(SharedObject.enOutputType.Error, "文件不存在，请检查路径有效性");
                        //new CommonVariable().realaseProcessExit(excelApp);
                        throw new Exception("文件不存在，请检查路径有效性");
                        //return;
                    }
                    else
                    {
                        //可用Open或Add函数打开文件，但对于执行VBA，Add无保存权限
                        excelApp.Workbooks.Open(filePath);
                    }
                }

                context.ScheduleAction(Body, excelApp, OnCompleted, OnFaulted);
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                try
                {
                    new CommonVariable().realaseProcessExit(excelApp);
                }
                catch
                {

                }
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }
        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            //TODO
        }
        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            Excel::_Workbook book = excelApp.ActiveWorkbook;
            string saveFilePath = SavePathUrl.Get(context);
            if (_Save)
            {
                if ((!isPathAvailable(saveFilePath)) && (_NewDoc))
                {
                    string messageBoxText = "此文档为新建文件,请输入正确保存路径!";
                    string caption = "提示";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        UniMessageBox.Show(messageBoxText, caption, button, icon);
                    }));
                }
                else if (!_NewDoc)
                {
                    book.Save();
                }
                else
                {
                    book.SaveAs(saveFilePath);
                }
            }
            if (_SaveAs)
            {
                if (!isPathAvailable(saveFilePath))
                {
                    string messageBoxText = "另存为应输入正确保存路径!";
                    string caption = "提示";
                    MessageBoxButton button = MessageBoxButton.OK;
                    MessageBoxImage icon = MessageBoxImage.Warning;
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        UniMessageBox.Show(messageBoxText, caption, button, icon);
                    }));
                }
                else
                {
                    book.SaveAs(saveFilePath);
                }
            }
            if (IsExit)
                new CommonVariable().realaseProcessExit(excelApp);
            else
                new CommonVariable().realaseProcess(excelApp);

            Thread.Sleep(DelayAfter.Get(context));
        }
    }
}
