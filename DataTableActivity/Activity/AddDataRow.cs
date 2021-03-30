using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;

namespace DataTableActivity
{
    [Designer(typeof(AddDataRowDesigner))]
    public sealed class AddDataRow : AsyncCodeActivity
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
        [OverloadGroup("DataRow")]
        [DisplayName("数据行")]
        [Description("要添加到 DataTable 的 DataRow 对象，如果设置了此属性，则忽略 ArrayRow 属性。")]
        public InArgument<DataRow> DataRow { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [OverloadGroup("ArrayRow")]
        [DisplayName("数组行")]
        [Description("要添加到 DataTable 的对象数组。每个对象的类型应映射到 DataTable 中其对应列的类型。")]
        public InArgument<object[]> ArrayRow { get; set; }

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("数据表")]
        [Description("要添加行数据的 DataTable 对象。")]
        public InArgument<DataTable> DataTable { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/DataTable/add-data-row.png";
            }
        }

        #endregion


        public AddDataRow()
        {

        }

        private delegate string runDelegate();
        private runDelegate m_Delegate;
        public string Run()
        {
            return DisplayName;
        }


        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            DataTable dataTable = DataTable.Get(context);
            DataRow dataRow = DataRow.Get(context);
            object[] objs = ArrayRow.Get(context);
            try
            {
                if (dataRow != null)
                {
                    dataTable.Rows.Add(dataRow);
                }
                else
                {
                    object[] newObjs = null;
                    if (objs.Length >= dataTable.Columns.Count)
                    {
                        List<object> list = new List<object>();
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            list.Add(objs[i]);
                        }
                        newObjs = list.ToArray();
                    }
                    else
                    {
                        List<object> list = new List<object>();
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            if (i < objs.Length)
                            {
                                list.Add(objs[i]);
                            }
                            else
                            {
                                list.Add(null);
                            }
                        }
                        newObjs = list.ToArray();
                    }
                    dataTable.Rows.Add(newObjs);
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

            m_Delegate = new runDelegate(Run);

            return m_Delegate.BeginInvoke(callback, state);
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {

        }
    }
}