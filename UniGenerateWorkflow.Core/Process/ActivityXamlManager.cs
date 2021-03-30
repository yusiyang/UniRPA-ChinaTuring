using System;
using System.Activities;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.IO;
using System.Xaml;

namespace Uni.Core
{

    /// <summary>
    /// 活动Xaml管理
    /// </summary>
    public class ActivityXamlManager
    {
        /// <summary>
        /// 活动工厂
        /// </summary>
        private readonly ActivityFactory _activityFactory;

        /// <summary>
        /// 生成的xmal默认名称
        /// </summary>
        public const string SaveXamlDefaultName = "WorkFlow.xaml";

        /// <summary>
        /// 占位符标识
        /// </summary>
        private const string PlaceholderForActivitiesName = "[PlaceholderForActivities]";

        /// <summary>
        /// 前缀标识
        /// </summary>
        private const string PrefixName = "<Sequence>";

        /// <summary>
        /// 后缀标识
        /// </summary>
        private const string SuffixName = "</Sequence></Activity>";

        /// <summary>
        /// 构造函数
        /// </summary>
        public ActivityXamlManager()
        {
            _activityFactory = new ActivityFactory();
        }

        /// <summary>
        /// 生成Xaml
        /// </summary>
        /// <param name="activityDescriptions"></param>  
        /// <param name="saveXamlDirectory">保存xaml的目录地址</param>  
        public void GenerateXaml(List<ActivityDescription> activityDescriptions, string additionalResourceDir, string saveXamlDirectory)
        {
            var activityBuilder = GetActivityBuilder(activityDescriptions, additionalResourceDir);
            SaveToXaml(activityBuilder, saveXamlDirectory);
        }

        /// <summary>
        /// 创建 ActivityBuilder
        /// </summary>
        /// <param name="activityDescriptions"></param>
        /// <returns></returns>
        private ActivityBuilder<object> GetActivityBuilder(List<ActivityDescription> activityDescriptions, string additionalResourceDir)
        {
            ActivityVariableContext activityContext = new ActivityVariableContext();
            activityContext.AdditionalResourceDir = additionalResourceDir;
            ActivityBuilder<object> activityBuilder = new ActivityBuilder<object>();
            var sequence = new Sequence();

            foreach (var item in activityDescriptions)
            {
                var activityDescription = item.Clone();
                if (activityDescription == null)
                {
                    continue;
                }
                var activity = _activityFactory.GetActivity(activityContext, activityDescription, out var variables);
                if (activity == null)
                {
                    continue;
                }
                foreach (var variable in variables)
                {
                    sequence.Variables.Add(variable);
                }
                sequence.Activities.Add(activity);
            }

            activityBuilder.Implementation = sequence;
            return activityBuilder;
        }

        /// <summary>
        /// 保存Xaml
        /// </summary>
        /// <param name="activityBuilder"></param>   
        /// <param name="saveXamlDirectory">保存xaml的目录地址</param>  
        private void SaveToXaml(ActivityBuilder<object> activityBuilder, string saveXamlDirectory)
        {
            if (!Directory.Exists(saveXamlDirectory))
            {
                Directory.CreateDirectory(saveXamlDirectory);
            }
            var path = Path.Combine(saveXamlDirectory, SaveXamlDefaultName);
            using (StreamWriter sw = File.CreateText(path))
            {
                using (XamlWriter xw = ActivityXamlServices.CreateBuilderWriter(new XamlXmlWriter(sw, new XamlSchemaContext())))
                {
                    XamlServices.Save(xw, activityBuilder);
                }
            }
            var replaceContent = string.Empty;
            if (File.Exists(path))
            {
                string strContent = File.ReadAllText(path);
                var length = strContent.IndexOf(SuffixName) - strContent.IndexOf(PrefixName) - PrefixName.Length;
                replaceContent = strContent.Substring(strContent.IndexOf(PrefixName) + PrefixName.Length, length > 0 ? length : 0);
            }
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "Template.txt"))
            {
                string strContent = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Template.txt");
                strContent = strContent.Replace(PlaceholderForActivitiesName, replaceContent);
                File.WriteAllText(path, strContent);
            }
        }
    }
}
