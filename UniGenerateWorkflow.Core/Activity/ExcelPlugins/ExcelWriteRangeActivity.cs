using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Uni.Core
{
    /// <summary>
    /// 后台写入区域内容活动
    /// </summary>
    public class ExcelWriteRangeActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "写入区域内容";

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
            var excelWriteRange = new ExcelPlugins.ExcelWriteRange
            {
                SheetIndex = 1
            };
            ((System.Activities.Activity)excelWriteRange).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    switch (key)
                    {
                        case nameof(excelWriteRange.DisplayName):
                            ((System.Activities.Activity)excelWriteRange).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(excelWriteRange.PathUrl):
                            excelWriteRange.PathUrl = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(excelWriteRange.CellBegin):
                            excelWriteRange.CellBegin = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(excelWriteRange.DataTable):
                            var test = activityVariableContext.Get(value.ToString()) ?? CreateVariable<string>(variables, key);
                            excelWriteRange.DataTable = new InArgument<System.Data.DataTable> { Expression = new VisualBasicValue<System.Data.DataTable>(activityVariableContext.Get(value.ToString()) ?? CreateVariable<string>(variables, key)) };
                            break;
                        case nameof(excelWriteRange.SheetIndex):
                            excelWriteRange.SheetIndex = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(excelWriteRange.SheetName):
                            excelWriteRange.SheetName = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(excelWriteRange.HasTitle):
                            excelWriteRange.HasTitle = (bool)value;
                            break;
                        default:
                            throw new NotImplementedException();

                    }
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetPathUrlProperty(activityVariableContext, excelWriteRange, unrecognizedParameters);

            return excelWriteRange;
        }

        /// <summary>
        /// 设置路径属性
        /// </summary>
        /// <param name="excelWriteRange"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetPathUrlProperty(ActivityVariableContext activityVariableContext, ExcelPlugins.ExcelWriteRange excelWriteRange, List<string> unrecognizedParameters)
        {
            if (excelWriteRange.PathUrl == null || unrecognizedParameters == null)
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
                excelWriteRange.PathUrl = match.Value ?? "";
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
                    excelWriteRange.PathUrl = match.Value ?? "";
                    unrecognizedParameters.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
