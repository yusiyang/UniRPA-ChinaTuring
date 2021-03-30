using System.Activities;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System;
using Microsoft.Office.Interop.Word;
using Plugins.Shared.Library;
using System.Threading;
using MouseActivity;
using Application = System.Windows.Application;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.Extensions;

namespace WordPlugins
{
    [Designer(typeof(WordSaveDesigner))]
    public sealed class WordSave : CodeActivity
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

        InArgument<string> _PathUrl;
        [Category("输入")]
        [DisplayName("文件路径")]
        [Description("要保存的Word文档的全路径。必须将文本放入引号中。")]
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

        #endregion


        #region 属性分类：选项

        bool _Save = true;
        [Category("选项")]
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

        bool _IsExit = true;
        [Category("选项")]
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

        bool _SaveAs;
        [Category("选项")]
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
        private bool isPathAvailable(string path)
        {
            if (path == null)
            {
                return false;
            }
            string[] sArray = path.Split('\\');
            string dict = sArray[0] + '\\';
            System.Diagnostics.Debug.WriteLine("dict : " + dict);

            if (!Directory.Exists(dict))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/word/save.png"; } }

        #endregion


        public WordSave()
        {
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            if (this.SaveAs == true && this.PathUrl == null)
            {
                metadata.AddValidationError("另存为需要添加有效路径");
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string filePath = PathUrl.Get(context);
                //CommonVariable.doc = CommonVariable.app.ActiveDocument;
                if (_Save)
                {
                    if ((!isPathAvailable(filePath)) && (CommonVariable.isNewFile))
                    {
                        string messageBoxText = "此文档为新建文件,请输入正确保存路径!";
                        string caption = "提示";
                        MessageBoxButton button = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Warning;
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            UniMessageBox.Show(messageBoxText, caption, button, icon);
                        }));
                        CommonVariable.realaseProcessExit();
                        return;
                    }
                    else if (!CommonVariable.isNewFile)
                    {
                        CommonVariable.docs.Save(true, WdOriginalFormat.wdOriginalDocumentFormat);
                    }
                    else
                    {
                        CommonVariable.doc.SaveAs2(filePath);
                    }
                }
                if (_SaveAs)
                {
                    if (!isPathAvailable(filePath))
                    {
                        string messageBoxText = "另存为应输入正确保存路径!";
                        string caption = "提示";
                        MessageBoxButton button = MessageBoxButton.OK;
                        MessageBoxImage icon = MessageBoxImage.Warning;
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            UniMessageBox.Show(messageBoxText, caption, button, icon);
                        }));
                        CommonVariable.realaseProcessExit();

                        Thread.Sleep(delayAfter);
                        return;
                    }
                    else
                    {
                        CommonVariable.doc.SaveAs2(filePath);
                    }
                }
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                CommonVariable.realaseProcessExit();
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            if (IsExit)
                CommonVariable.realaseProcessExit();

            Thread.Sleep(delayAfter);
        }
    }
}
