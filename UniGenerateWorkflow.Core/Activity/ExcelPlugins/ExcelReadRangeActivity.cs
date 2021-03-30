using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Uni.Core
{
    /// <summary>
    /// 后台读取区域内容活动
    /// </summary>
    public class ExcelReadRangeActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "读取区域内容";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => false;

        /// <summary>
        /// 获取活动
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="activityDescription"></param>
        /// <param name="variables"></param>
        /// <param name="activityFactory"></param>
        /// <returns></returns>
        public Activity CreateInstance(ActivityVariableContext activityVariableContext, ActivityDescription activityDescription, out List<Variable> variables, ActivityFactory activityFactory = null)
        {
            variables = new List<Variable>();
            var excelReadRange = new ExcelPlugins.ExcelReadRange()
            {
                SheetIndex = 1,
                HasTitle = true
            };
            ((System.Activities.Activity)excelReadRange).DisplayName = DisplayName;
            var dic = new Dictionary<string, ArgumentTypeEnum>();
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(excelReadRange.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(excelReadRange.DisplayName):
                            ((System.Activities.Activity)excelReadRange).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(excelReadRange.PathUrl):
                            excelReadRange.PathUrl = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(excelReadRange.CellRange):
                            excelReadRange.CellRange = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(excelReadRange.DataTable):
                            excelReadRange.DataTable = new OutArgument<System.Data.DataTable> { Expression = new VisualBasicValue<Location<System.Data.DataTable>>(CreateOurArgument<System.Data.DataTable>(activityVariableContext, variables, key, value)) };
                            break;
                        case nameof(excelReadRange.SheetIndex):
                            excelReadRange.SheetIndex = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(excelReadRange.SheetName):
                            excelReadRange.SheetName = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(excelReadRange.HasTitle):
                            excelReadRange.HasTitle = (bool)value;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetPathUrlProperty(activityVariableContext, excelReadRange, unrecognizedParameters);

            //设置OutArgument输出变量
            SetDataTableProperty(activityVariableContext, excelReadRange, variables, dic);

            //添加到活动链表
            activityVariableContext.Activities.AddLast(new Uni.Core.ActivityContext { Activity = excelReadRange, Variables = dic });
            return excelReadRange;
        }

        /// <summary>
        /// 设置路径属性
        /// </summary>
        /// <param name="excelReadRange"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetPathUrlProperty(ActivityVariableContext activityVariableContext, ExcelPlugins.ExcelReadRange excelReadRange, List<string> unrecognizedParameters)
        {
            if (excelReadRange.PathUrl != null || unrecognizedParameters == null)
            {
                return;
            }
            bool isMatch = false;
            for (var i = unrecognizedParameters.Count - 1; i >= 0; i--)
            {
                var match = Regex.Match(unrecognizedParameters[i], FilePattern);
                if (!match.Success)
                {
                    continue;
                }
                excelReadRange.PathUrl = match.Value ?? "";
                unrecognizedParameters.RemoveAt(i);
                isMatch = true;
                break;
            }
            if (isMatch == false && !string.IsNullOrEmpty(activityVariableContext.AdditionalResourceDir))
            {
                for (var i = unrecognizedParameters.Count - 1; i >= 0; i--)
                {
                    var match = Regex.Match(Path.Combine(activityVariableContext.AdditionalResourceDir, unrecognizedParameters[i]), FilePattern);
                    if (!match.Success)
                    {
                        continue;
                    }
                    excelReadRange.PathUrl = match.Value ?? "";
                    unrecognizedParameters.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 设置DataTable属性
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="excelReadRange"></param>
        /// <param name="variables"></param>
        /// <param name="dic"></param>
        private void SetDataTableProperty(ActivityVariableContext activityVariableContext, ExcelPlugins.ExcelReadRange excelReadRange, List<Variable> variables, Dictionary<string, ArgumentTypeEnum> dic)
        {
            if (excelReadRange.DataTable != null)
            {
                return;
            }
            var variable = CreateOurArgument<System.Data.DataTable>(activityVariableContext, variables, DataTable, DataTable);
            dic.Add(variable, ArgumentTypeEnum.Out);
            excelReadRange.DataTable = new OutArgument<System.Data.DataTable>() { Expression = new VisualBasicValue<Location<System.Data.DataTable>>(variable) };
        }

    }
}
