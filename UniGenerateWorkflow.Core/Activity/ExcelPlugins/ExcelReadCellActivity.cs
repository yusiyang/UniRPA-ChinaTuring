using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Uni.Core
{
    /// <summary>
    /// 后台获取单元格内容活动
    /// </summary>
    public class ExcelReadCellActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "后台获取单元格内容";

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
            var excelReadCell = new ExcelPlugins.ExcelReadCell
            {
                SheetIndex = 1
            };
            ((System.Activities.Activity)excelReadCell).DisplayName = DisplayName;
            foreach (var key in activityDescription.Properties.Keys)
            {
                var value = activityDescription.Properties[key];
                if (value == null && nameof(excelReadCell.DisplayName) != key)
                {
                    continue;
                }
                switch (key)
                {
                    case nameof(excelReadCell.DisplayName):
                        ((System.Activities.Activity)excelReadCell).DisplayName = value == null ? DisplayName : value.ToString();
                        break;
                    case nameof(excelReadCell.PathUrl):
                        excelReadCell.PathUrl = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                        break;
                    case nameof(excelReadCell.Cell):
                        excelReadCell.Cell = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                        break;
                    case nameof(excelReadCell.CellContent):
                        excelReadCell.CellContent = new OutArgument<string> { Expression = new VisualBasicValue<Location<string>>(CreateOurArgument<string>(activityVariableContext, variables, key, value)) };
                        break;
                    case nameof(excelReadCell.SheetIndex):
                        excelReadCell.SheetIndex = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                        break;
                    case nameof(excelReadCell.SheetName):
                        excelReadCell.SheetName = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                        break;
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetPathUrlProperty(activityVariableContext, excelReadCell, unrecognizedParameters);

            return excelReadCell;
        }

        /// <summary>
        /// 设置路径属性
        /// </summary>
        /// <param name="excelReadCell"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetPathUrlProperty(ActivityVariableContext activityVariableContext, ExcelPlugins.ExcelReadCell excelReadCell, List<string> unrecognizedParameters)
        {
            if (excelReadCell.PathUrl != null || unrecognizedParameters == null)
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
                excelReadCell.PathUrl = match.Value ?? "";
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
                    excelReadCell.PathUrl = match.Value ?? "";
                    unrecognizedParameters.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
