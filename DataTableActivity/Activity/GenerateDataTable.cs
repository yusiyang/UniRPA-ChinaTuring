using DataTableActivity.Operators;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.Librarys;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace DataTableActivity
{
    [Designer(typeof(GenerateDataTableDesigner))]
    public sealed class GenerateDataTable : TaskAsyncCodeActivity<DataTable>
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

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("数据源")]
        [Description("指定要转换为表的非结构化数据的来源。必须将文本放入引号中。")]
        public InArgument<string> InputString { get; set; }

        [Category("输入")]
        [Browsable(false)]
        [DisplayName("位置")]
        [Description("如果使用OCR进行抓取，并且选中了“获取单词信息”复选框，则此字段包含 IEnumerable<KeyValuePair<Rectangle,String>> 具有 WordsInfo 值的变量。")]
        public InArgument<IEnumerable<KeyValuePair<Rectangle, String>>> Positions { get; set; }

        #endregion


        #region 属性分类：固定格式选项

        [Category("固定格式选项")]
        [DisplayName("列宽")]
        [Description("指定要在表中创建的列的大小。该字段仅支持 IEnumerable<Int32> 变量。")]
        public InArgument<IEnumerable<Int32>> ColumnSizes { get; set; }

        #endregion


        #region 属性分类：格式化选项

        [Category("格式化选项")]
        [DisplayName("列分隔符")]
        [Description("指定要用作列分隔符的字符。必须将文本放入引号中。")]
        public InArgument<string> ColumnSeparators { get; set; }

        [Category("格式化选项")]
        [DisplayName("行分隔符")]
        [Description("指定要用作换行符分隔符的字符。必须将文本放入引号中。")]
        public InArgument<string> NewLineSeparator { get; set; }

        [Category("格式化选项")]
        [DisplayName("CSV 解析")]
        [Description("是否 CSV 解析。")]
        public bool CSVParse { get; set; }

        #endregion


        #region 属性分类：选项

        [Category("选项")]
        [DisplayName("类型自动检测")]
        [Description("选中后，自动检测列或行类型，无论是 String，Int32 等。")]
        public bool AutoDetectTypes { get; set; }

        [Category("选项")]
        [DisplayName("列标题")]
        [Description("如果选中，则使用第一个标识的列作为列标题。")]
        public bool UseColumnHeader { get; set; }

        [Category("选项")]
        [DisplayName("行标题")]
        [Description("如果选中，则使用第一个标识的行作为行标题。")]
        public bool UseRowHeader { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("数据表")]
        [Description("生成的 DataTable 变量。")]
        public OutArgument<DataTable> DataTable { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        [DefaultValue(false)]
        public bool PreserveStrings { get; set; }

        [Browsable(false)]
        [DefaultValue(false)]
        public bool PreserveNewLines { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/generate-data-table.png";
            }
        }

        #endregion


        public GenerateDataTable()
        {

        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return DisplayName;
        }

        protected override Task<DataTable> ExecuteAsyncWithResult(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            IEnumerable<KeyValuePair<Rectangle, string>> positions = this.Positions.Get(context);
            string input = this.InputString.Get(context);
            ITableOptions tableOptions = this.GetTableOptions();
            IFormatOptions formatOptions = this.GetFormatOptions(context);
            //TODO DelayAfter

            return Task.Factory.StartNew<DataTable>(delegate
            {
                TableFormatter tableFormatter = new TableFormatter();
                if (positions == null)
                {
                    return tableFormatter.Format(input, formatOptions, tableOptions);
                }
                return tableFormatter.Format(positions, tableOptions);
            });
        }

        protected override void OutputResult(AsyncCodeActivityContext context, DataTable result)
        {
            this.DataTable.Set(context, result);
        }

        private ITableOptions GetTableOptions()
        {
            return new TableOptions
            {
                AutoDetectTypes = this.AutoDetectTypes,
                UseColumnHeader = this.UseColumnHeader,
                UseRowHeader = this.UseRowHeader
            };
        }

        private IFormatOptions GetFormatOptions(AsyncCodeActivityContext context)
        {
            return new FormatOptions
            {
                ColumnSeparators = this.ColumnSeparators.Get(context),
                NewLineSeparator = this.NewLineSeparator.Get(context),
                CSVParsing = this.CSVParse,
                ColumnSizes = this.ColumnSizes.Get(context),
                PreserveNewLines = this.PreserveNewLines,
                PreserveStrings = this.PreserveStrings
            };
        }
    }


    public abstract class TaskAsyncCodeActivity<T> : AsyncCodeActivity
    {
        [DefaultValue(null)]
        public bool ContinueOnError { get; set; }

        protected sealed override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            context.UserState = cancellationTokenSource;
            Task<T> arg_24_0 = this.ExecuteAsyncWithResult(context, cancellationTokenSource.Token);
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>(state);
            arg_24_0.Unwrap(taskCompletionSource, callback, cancellationTokenSource);
            return taskCompletionSource.Task;
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            Task<T> task = (Task<T>)result;
            if (task == null)
            {
                this.OutputResult(context, default(T));
            }
            try
            {
                if (task.IsFaulted)
                {
                    throw task.Exception.InnerException ?? task.Exception;
                }
                if (task.IsCanceled || context.IsCancellationRequested)
                {
                    context.MarkCanceled();
                }
                else
                {
                    this.OutputResult(context, task.Result);
                }
            }
            catch (OperationCanceledException)
            {
                context.MarkCanceled();
            }
            catch (Exception e)
            {
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            ((CancellationTokenSource)context.UserState).Cancel();
        }

        protected abstract Task<T> ExecuteAsyncWithResult(AsyncCodeActivityContext context, CancellationToken cancellationToken);

        protected abstract void OutputResult(AsyncCodeActivityContext context, T result);
    }
}