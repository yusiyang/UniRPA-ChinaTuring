using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace Uni.Core
{
    /// <summary>
    /// 单行操作活动
    /// </summary>
    public class ForEachRowActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "单行操作";

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
            var forEachRowActivity = new DataTableActivity.ForEachRow();
            ((System.Activities.Activity)forEachRowActivity).DisplayName = DisplayName;
            var dic = new Dictionary<string, ArgumentTypeEnum>();

            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(forEachRowActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(forEachRowActivity.DisplayName):
                            ((System.Activities.Activity)forEachRowActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(forEachRowActivity.ContinueOnError):
                            forEachRowActivity.ContinueOnError = (bool)value;
                            break;
                        //case nameof(forEachRowActivity.body):
                        //    foreachrowactivity.body = getactivityaction<datarow>(foreachrowactivity, activityvariablecontext, activityfactory, variables, key, value);
                        //    break;
                        case nameof(forEachRowActivity.CurrentIndex):
                            var variable = CreateOurArgument<int>(activityVariableContext, variables, key, value);
                            dic.Add(variable, ArgumentTypeEnum.Out);
                            forEachRowActivity.CurrentIndex = new OutArgument<int>() { Expression = new VisualBasicValue<Location<int>>(variable) };
                            break;
                        case nameof(forEachRowActivity.DataTable):
                            forEachRowActivity.DataTable = new InArgument<DataTable> { Expression = new VisualBasicValue<DataTable>(activityVariableContext.Get(value.ToString()) ?? CreateVariable<DataTable>(variables, key)) };
                            break;
                    }
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetCurrentIndexProperty(activityVariableContext, forEachRowActivity, variables, dic);
            SetDataTableProperty(activityVariableContext, forEachRowActivity);
            activityVariableContext.AddActivity(forEachRowActivity, dic);

            //检查特定属性
            if (activityDescription.Properties == null || !activityDescription.Properties.ContainsKey(nameof(forEachRowActivity.Body)))
            {
                return forEachRowActivity;
            }
            var body = activityDescription.Properties[nameof(forEachRowActivity.Body)];
            forEachRowActivity.Body = GetActivityAction<DataRow>(forEachRowActivity, activityVariableContext, activityFactory, variables, nameof(forEachRowActivity.Body), body);
            return forEachRowActivity;
        }

        /// <summary>
        /// 设置当前行索引属性
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="forEachRowActivity"></param>
        /// <param name="variables"></param>
        /// <param name="dic"></param>
        private void SetCurrentIndexProperty(ActivityVariableContext activityVariableContext, DataTableActivity.ForEachRow forEachRowActivity, List<Variable> variables, Dictionary<string, ArgumentTypeEnum> dic)
        {
            if (forEachRowActivity.CurrentIndex != null)
            {
                return;
            }
            var variable = CreateOurArgument<int>(activityVariableContext, variables, CurrentIndex, CurrentIndex);
            dic.Add(variable, ArgumentTypeEnum.Out);
            forEachRowActivity.CurrentIndex = new OutArgument<int>() { Expression = new VisualBasicValue<Location<int>>(variable) };
        }

        /// <summary>
        /// 设置DataTable属性
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="forEachRowActivity"></param>
        private void SetDataTableProperty(ActivityVariableContext activityVariableContext, DataTableActivity.ForEachRow forEachRowActivity)
        {
            if (forEachRowActivity.DataTable != null || activityVariableContext.Activities.Last == null)
            {
                return;
            }
            var activity = activityVariableContext.Activities.Last.Value;

            if (!activity.Variables.ContainsValue(ArgumentTypeEnum.Out))
            {
                return;
            }
            var variable = activity.Variables.First(a => a.Value == ArgumentTypeEnum.Out);
            forEachRowActivity.DataTable = new InArgument<DataTable> { Expression = new VisualBasicValue<DataTable>(variable.Key) };
        }
    }
}
