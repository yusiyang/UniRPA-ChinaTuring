using Microsoft.VisualBasic.Activities;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Uni.Common;

namespace Uni.Core
{
    /// <summary>
    /// 后台设置单元格内容活动
    /// </summary>
    public class ExcelWriteCellActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "后台设置单元格内容";

        /// <summary>
        /// 是否含有属性
        /// </summary>
        public override bool IsHasSelector => false;

        /// <summary>
        /// 单元格偏移
        /// </summary>
        private const int CurrentIndexOffset = 2;

        private string _pathUrl = string.Empty;

        private bool _isSameFile = false;

        private Dictionary<string, string> _columnNamesByCellContent;

        private Dictionary<string, string> GetColumnNamesByCellContent(string path)
        {
            if (string.IsNullOrEmpty(_pathUrl))
            {
                return null;
            }
            if (_isSameFile)
            {
                if (_columnNamesByCellContent != null)
                {
                    return _columnNamesByCellContent;
                }
            }
            else
            {
                _columnNamesByCellContent = NPOIUtility.GetColumnNamesByCellContent(path);
            }
            return _columnNamesByCellContent;
        }

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
            var excelWriteCell = new ExcelPlugins.ExcelWriteCell()
            {
                SheetIndex = 1
            };
            ((System.Activities.Activity)excelWriteCell).DisplayName = DisplayName;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(excelWriteCell.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(excelWriteCell.DisplayName):
                            ((System.Activities.Activity)excelWriteCell).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(excelWriteCell.PathUrl):
                            excelWriteCell.PathUrl = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(excelWriteCell.Cell):
                            var currentIndex = activityVariableContext.Get(CurrentIndex);
                            excelWriteCell.Cell = new InArgument<string> { Expression = new VisualBasicValue<string>("\"" + value.ToString() + "\" + (" + currentIndex + "+2).ToString()") };
                            break;
                        case nameof(excelWriteCell.CellContent):
                            excelWriteCell.CellContent = new InArgument<string> { Expression = new VisualBasicValue<string>(activityVariableContext.Get(value.ToString()) ?? CreateVariable<string>(variables, key)) };
                            break;
                        case nameof(excelWriteCell.SheetIndex):
                            excelWriteCell.SheetIndex = new InArgument<int> { Expression = new VisualBasicValue<int>(CreateVariable<int>(variables, key, value)) };
                            break;
                        case nameof(excelWriteCell.SheetName):
                            excelWriteCell.SheetName = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                    }
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetPathUrlProperty(activityVariableContext, excelWriteCell, unrecognizedParameters);
            SetCellProperty(activityVariableContext, excelWriteCell, unrecognizedParameters);
            SetCellContentProperty(activityVariableContext, excelWriteCell);
            activityVariableContext.Activities.AddLast(new Uni.Core.ActivityContext { Activity = excelWriteCell, Variables = null });
            return excelWriteCell;
        }

        /// <summary>
        /// 设置路径属性
        /// </summary>
        /// <param name="excelWriteCell"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetPathUrlProperty(ActivityVariableContext activityVariableContext, ExcelPlugins.ExcelWriteCell excelWriteCell, List<string> unrecognizedParameters)
        {
            if (excelWriteCell.PathUrl != null || unrecognizedParameters == null)
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
                excelWriteCell.PathUrl = match.Value ?? "";
                _isSameFile = _pathUrl == match.Value;
                _pathUrl = match.Value ?? "";
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
                    excelWriteCell.PathUrl = match.Value ?? "";
                    unrecognizedParameters.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 设置单元格属性
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="excelWriteCell"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetCellProperty(ActivityVariableContext activityVariableContext, ExcelPlugins.ExcelWriteCell excelWriteCell, List<string> unrecognizedParameters)
        {
            if (excelWriteCell.Cell != null || unrecognizedParameters == null)
            {
                return;
            }
            var columnNamesByCellContent = GetColumnNamesByCellContent(_pathUrl);
            for (var i = unrecognizedParameters.Count - 1; i >= 0; i--)
            {
                var currentIndex = activityVariableContext.Get(CurrentIndex);
                if (columnNamesByCellContent == null || !columnNamesByCellContent.ContainsValue(unrecognizedParameters[i]))
                {
                    continue;
                }
                var cell = columnNamesByCellContent.First(a => a.Value == unrecognizedParameters[i]);
                excelWriteCell.Cell = new InArgument<string> { Expression = new VisualBasicValue<string>("\"" + cell.Key + "\" + (" + currentIndex + "+" + CurrentIndexOffset + ").ToString()") };
                unrecognizedParameters.RemoveAt(i);
                break;
            }
        }

        /// <summary>
        /// 设置单元格内容属性
        /// </summary>
        /// <param name="activityVariableContext"></param>
        /// <param name="excelWriteCell"></param>
        private void SetCellContentProperty(ActivityVariableContext activityVariableContext, ExcelPlugins.ExcelWriteCell excelWriteCell)
        {
            if (excelWriteCell.CellContent != null || activityVariableContext.Activities.Last == null)
            {
                return;
            }
            var activity = activityVariableContext.Activities.Last.Value;
            if (!activity.Variables.ContainsValue(ArgumentTypeEnum.Out))
            {
                return;
            }
            var variable = activity.Variables.First(a => a.Value == ArgumentTypeEnum.Out);
            excelWriteCell.CellContent = new InArgument<string> { Expression = new VisualBasicValue<string>(variable.Key) };
        }
    }
}
