using DataTableActivity;
using DataTableActivity.Operators;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;

namespace Uni.Core
{
    /// <summary>
    /// 过滤数据表活动
    /// </summary>
    public class FilterDataTableActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "过滤数据表";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => false;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var filterDataTableActivity = new DataTableActivity.FilterDataTable();
            ((System.Activities.Activity)filterDataTableActivity).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(filterDataTableActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(filterDataTableActivity.DisplayName):
                            ((System.Activities.Activity)filterDataTableActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(filterDataTableActivity.ContinueOnError):
                            filterDataTableActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(filterDataTableActivity.DataTable):
                            filterDataTableActivity.DataTable = new InArgument<System.Data.DataTable> { Expression = new VisualBasicValue<System.Data.DataTable>(CreateVariable<System.Data.DataTable>(variables, key, value)) };
                            break;
                        case nameof(filterDataTableActivity.FilterRowsMode):
                            filterDataTableActivity.FilterRowsMode = (SelectMode)value;
                            break;
                        case nameof(filterDataTableActivity.Filters):
                            filterDataTableActivity.Filters = (List<FilterOperationArgument>)value;
                            break;
                        case nameof(filterDataTableActivity.OutDataTable):
                            filterDataTableActivity.OutDataTable = new OutArgument<System.Data.DataTable> { Expression = new VisualBasicValue<Location<System.Data.DataTable>>(CreateOurArgument<System.Data.DataTable>(activityVariableContext, variables, key, value)) };
                            break;
                        case nameof(filterDataTableActivity.SelectColumns):
                            filterDataTableActivity.SelectColumns = (List<InArgument>)value;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            return filterDataTableActivity;
        }
    }
}
