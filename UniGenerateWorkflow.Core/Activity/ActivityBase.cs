using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using Uni.Common;

namespace Uni.Core
{
    /// <summary>
    /// 活动基础类
    /// </summary>
    public abstract class ActivityBase
    {
        /// <summary>
        /// 随机数长度
        /// </summary>
        protected const int RandomNumberLength = 6;

        protected const string CurrentIndex = "CurrentIndex";

        protected const string Value = "Value";

        protected const string DataTable = "DataTable";

        protected const string To = "To";

        /// <summary>
        /// 匹配URL
        /// </summary>
        protected const string FilePattern = @"(?<path>(?:[a-zA-Z]:)?\\(?:[^\\\?\/\*\|<>:']+\\)+)(?<filename>(?<name>[^\\\?\/\*\|<>:']+?))\.(?:csv|xlsx|xls|exe)";

        /// <summary>
        /// 匹配URL
        /// </summary>
        protected const string UrlPattern = @"(((ht|f)tp(s?))\://)?(www.|[a-zA-Z].)[a-zA-Z0-9\-\.]+\.(com|edu|gov|mil|net|org|biz|info|name|museum|us|ca|uk)(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\;\?\'\\\+&%\$#\=~_\-]+))*";

        /// <summary>
        /// 匹配文件名
        /// </summary>
        protected const string FileNamePattern = @"[^/\\]+[/\\]*$";

        /// <summary>
        /// 创建活动所需变量
        /// </summary>
        /// <param name="variables"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string CreateVariable<T>(List<Variable> variables, string key, object value = null)
        {
            var number = Utility.GenerateRandomCode(RandomNumberLength);
            var variableKey = $"{key}_{number}";

            variables.Add(value == null ? new Variable<T>(variableKey) : new Variable<T>(variableKey, (T)value));
            return variableKey;
        }

        /// <summary>
        /// 创建输入变量
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activityVariableContext"></param>
        /// <param name="variables"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual string CreateOurArgument<T>(ActivityVariableContext activityVariableContext, List<Variable> variables, string key, object value = null)
        {
            var variable = CreateVariable<T>(variables, key);
            if (value != null)
            {
                activityVariableContext.Add(value.ToString(), variable);
            }
            return variable;
        }

        /// <summary>
        /// 获取Body
        /// </summary>
        /// <param name="activityFactory">活动工厂</param>
        /// <param name="currentVariables">当前变量集合</param>
        /// <param name="key">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public virtual ActivityAction<T> GetActivityAction<T>(ActivityVariableContext activityVariableContext, ActivityFactory activityFactory, List<Variable> currentVariables, string key, object value)
        {
            var row = CreateVariable<T>(currentVariables, key, null);
            var activityAction = new ActivityAction<T> { Argument = new DelegateInArgument<T>(row) };
            var sequence = GetSequence(activityVariableContext, activityFactory, currentVariables, key, value);
            activityAction.Handler = sequence;
            return activityAction;
        }

        /// <summary>
        /// 获取Body
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="currentActivity">当前活动</param>
        /// <param name="activityFactory">活动工厂</param>
        /// <param name="currentVariables">当前变量集合</param>
        /// <param name="key">属性名</param>
        /// <param name="value">属性值</param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual ActivityAction<T> GetActivityAction<T>(Activity currentActivity, ActivityVariableContext activityVariableContext, ActivityFactory activityFactory, List<Variable> currentVariables, string key, object value)
        {
            var dic = new Dictionary<string, ArgumentTypeEnum>();
            var row = CreateVariable<T>(currentVariables, key, null);
            dic.Add(row, ArgumentTypeEnum.DataRow);
            activityVariableContext.AddActivity(currentActivity, dic);
            var activityAction = new ActivityAction<T> { Argument = new DelegateInArgument<T>(row) };
            var sequence = GetSequence(activityVariableContext, activityFactory, currentVariables, key, value);
            activityAction.Handler = sequence;
            return activityAction;
        }

        /// <summary>
        /// 获取Sequence
        /// </summary>
        /// <param name="activityFactory">活动工厂</param>
        /// <param name="currentVariables">当前变量集合</param>
        /// <param name="key">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public virtual Sequence GetSequence(ActivityVariableContext activityVariableContext, ActivityFactory activityFactory, List<Variable> currentVariables, string key, object value)
        {
            var sequence = new Sequence();
            var activityDescriptions = (List<ActivityDescription>)value;
            foreach (var activityDescription in activityDescriptions)
            {
                var activity = activityFactory.GetActivity(activityVariableContext, activityDescription, out var otherVariables);
                if (activity == null)
                {
                    continue;
                }
                currentVariables.AddRange(otherVariables);
                sequence.Activities.Add(activity);
            }
            return sequence;
        }

        public abstract bool IsHasSelector { get; }
    }
}
