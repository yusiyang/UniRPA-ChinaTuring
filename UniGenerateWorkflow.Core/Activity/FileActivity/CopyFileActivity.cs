using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Uni.Core
{
    /// <summary>
    /// 复制文件活动
    /// </summary>
    public class CopyFileActivity : ActivityBase, IActivity
    {
        /// <summary>
        /// 默认名称
        /// </summary>
        private const string DisplayName = "复制文件";

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
            var copyFileActivity = new FileActivity.CopyFileActivity();
            ((System.Activities.Activity)copyFileActivity).DisplayName = DisplayName;
            var parameters = activityDescription.UnrecognizedParameters;
            if (activityDescription.Properties != null)
            {
                foreach (var key in activityDescription.Properties.Keys)
                {
                    var value = activityDescription.Properties[key];
                    if (value == null && nameof(copyFileActivity.DisplayName) != key)
                    {
                        continue;
                    }
                    switch (key)
                    {
                        case nameof(copyFileActivity.DisplayName):
                            ((System.Activities.Activity)copyFileActivity).DisplayName = value == null ? DisplayName : value.ToString();
                            break;
                        case nameof(copyFileActivity.ContinueOnError):
                            copyFileActivity.ContinueOnError = (bool)value;
                            break;
                        case nameof(copyFileActivity.Destination):
                            copyFileActivity.Destination = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(copyFileActivity.NewFileName):
                            copyFileActivity.NewFileName = new InArgument<string>() { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        case nameof(copyFileActivity.Overwrite):
                            copyFileActivity.Overwrite = (bool)value;
                            break;
                        case nameof(copyFileActivity.Path):
                            copyFileActivity.Path = new InArgument<string> { Expression = new VisualBasicValue<string>(CreateVariable<string>(variables, key, value)) };
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }

            //设置未赋值但需要赋值的属性
            var unrecognizedParameters = activityDescription.UnrecognizedParameters;
            SetPathProperty(activityVariableContext, copyFileActivity, unrecognizedParameters);
            SetDestinationProperty(copyFileActivity, unrecognizedParameters);

            return copyFileActivity;
        }

        /// <summary>
        /// 设置路径属性
        /// </summary>
        /// <param name="copyFileActivity"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetPathProperty(ActivityVariableContext activityVariableContext, FileActivity.CopyFileActivity copyFileActivity, List<string> unrecognizedParameters)
        {
            if (copyFileActivity.Path != null || unrecognizedParameters == null)
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
                copyFileActivity.Path = match.Value ?? "";
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
                    copyFileActivity.Path = match.Value ?? "";
                    unrecognizedParameters.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 设置目标文件夹属性
        /// </summary>
        /// <param name="copyFileActivity"></param>
        /// <param name="unrecognizedParameters"></param>
        private void SetDestinationProperty(FileActivity.CopyFileActivity copyFileActivity, List<string> unrecognizedParameters)
        {
            if (copyFileActivity.Destination != null || unrecognizedParameters == null)
            {
                return;
            }

            for (int i = unrecognizedParameters.Count - 1; i >= 0; i--)
            {
                copyFileActivity.Destination = unrecognizedParameters[i];
                unrecognizedParameters.RemoveAt(i);
                break;
            }
        }

    }
}
