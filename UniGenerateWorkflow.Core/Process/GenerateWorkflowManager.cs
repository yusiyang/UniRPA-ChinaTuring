using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Uni.Entity;

namespace Uni.Core
{
    /// <summary>
    /// 工作流生成管理类
    /// </summary>
    public class GenerateWorkflowManager
    {
        private static GenerateWorkflowManager _generateWorkflowManager = new GenerateWorkflowManager();

        private readonly TextInterpreter _interpreter;
        private readonly Convertor _convertor;
        private readonly ActivityXamlManager _activityXamlManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        private GenerateWorkflowManager()
        {
            InitKeywordData(out List<string> actionWords, out List<string> objectWords, out List<string> parameterWords, out Dictionary<string, KeywordTypeEnum> controlFlows);
            InitKeywordActivityMapData(out Dictionary<string, List<string>> akAcMap, out Dictionary<string, List<string>> okAcMap);
            InitActivityPropertyData(out Dictionary<string, List<string>> acPropertyMap);
            InitPropertyKeywordActivityPropertyData(out Dictionary<string, List<ActivityPropertyKeyValue>> pkApMap);

            BehaviorManager behaviorManager = new BehaviorManager();

            //解析器
            _interpreter = new TextInterpreter(actionWords, objectWords, parameterWords, controlFlows, behaviorManager);

            //转换器
            _convertor = new Convertor(akAcMap, okAcMap, acPropertyMap, pkApMap, behaviorManager);

            //活动Xaml管理
            _activityXamlManager = new ActivityXamlManager();
        }

        /// <summary>
        /// 获取生成工作流单例
        /// </summary>
        public static GenerateWorkflowManager Instance
        {
            get
            {
                return _generateWorkflowManager;
            }
        }

        private void InitKeywordData(out List<string> actionWords, out List<string> objectWords, out List<string> propertyWords, out Dictionary<string, KeywordTypeEnum> controlFlows)
        {
            actionWords = new List<string>();
            objectWords = new List<string>();
            propertyWords = new List<string>();
            controlFlows = new Dictionary<string, KeywordTypeEnum>();
            using (DbContext dbContext = new DbContext())
            {
                List<ActionKeyword> akWords = dbContext.ActionKeyword.GetList();
                if (akWords != null)
                {
                    foreach (ActionKeyword ak in akWords)
                    {
                        if (ak.KeywordType == KeywordTypeEnum.Word)
                        {
                            actionWords.Add(ak.Name);
                        }
                        if (ak.KeywordType == KeywordTypeEnum.Foreach ||
                            ak.KeywordType == KeywordTypeEnum.ForeachRow ||
                            ak.KeywordType == KeywordTypeEnum.While ||
                            ak.KeywordType == KeywordTypeEnum.IfElse)
                        {
                            if (!controlFlows.ContainsKey(ak.Name))
                            {
                                controlFlows.Add(ak.Name, ak.KeywordType);
                            }
                        }
                    }
                }

                List<ObjectKeyword> okWords = dbContext.ObjectKeyword.GetList();
                if (okWords != null)
                {
                    foreach (ObjectKeyword ok in okWords)
                    {
                        objectWords.Add(ok.Name);
                    }
                }

                //参数关键字
                List<ParameterKeyword> pkWords = dbContext.ParameterKeyword.GetList();
                if (pkWords != null)
                {
                    foreach (var pk in pkWords)
                    {
                        propertyWords.Add(pk.Name.ToLower());
                    }
                }
            }
        }

        private void InitKeywordActivityMapData(out Dictionary<string, List<string>> akAcMap, out Dictionary<string, List<string>> okAcMap)
        {
            akAcMap = new Dictionary<string, List<string>>();
            okAcMap = new Dictionary<string, List<string>>();
            using (DbContext dbContext = new DbContext())
            {
                string akAliasName = "akName";
                string acAliasName = "acName";
                string akSql = $"SELECT ak.{nameof(ActionKeyword.Name)} as {akAliasName},ac.{nameof(Activity.Name)} as {acAliasName} FROM {nameof(ActionKeywordActivityMapping)} as akam " +
                             $"LEFT JOIN {nameof(ActionKeyword)} as ak ON akam.{nameof(ActionKeywordActivityMapping.ActionKeywordId)} = ak.{nameof(ActionKeyword.Id)} " +
                             $"LEFT JOIN {nameof(Activity)} as ac ON akam.{nameof(ActionKeywordActivityMapping.ActivityId)} = ac.{nameof(Activity.Id)} ";

                DataTable akTable = dbContext.Client.Ado.GetDataTable(akSql);

                if (akTable != null)
                {
                    foreach (DataRow row in akTable.Rows)
                    {
                        string akName = row.Field<string>(akAliasName);
                        string acName = row.Field<string>(acAliasName);
                        if (!akAcMap.ContainsKey(akName))
                        {
                            akAcMap.Add(akName, new List<string>() { acName });
                        }
                        else
                        {
                            akAcMap[akName].Add(acName);
                        }
                    }
                }

                string okAliasName = "akName";
                string okSql = $"SELECT ok.{nameof(ObjectKeyword.Name)} as {okAliasName},ac.{nameof(Activity.Name)} as {acAliasName} FROM {nameof(ObjectKeywordActivityMapping)} as okam " +
                               $"LEFT JOIN {nameof(ObjectKeyword)} as ok ON okam.{nameof(ObjectKeywordActivityMapping.ObjectKeywordId)} = ok.{nameof(ObjectKeyword.Id)} " +
                               $"LEFT JOIN {nameof(Activity)} as ac ON okam.{nameof(ObjectKeywordActivityMapping.ActivityId)} = ac.{nameof(Activity.Id)} ";

                DataTable okTable = dbContext.Client.Ado.GetDataTable(okSql);
                if (okTable != null)
                {
                    foreach (DataRow row in okTable.Rows)
                    {
                        string okName = row.Field<string>(okAliasName);
                        string acName = row.Field<string>(acAliasName);
                        if (!okAcMap.ContainsKey(okName))
                        {
                            okAcMap.Add(okName, new List<string>() { acName });
                        }
                        else
                        {
                            okAcMap[okName].Add(acName);
                        }
                    }
                }
            }
        }

        private void InitActivityPropertyData(out Dictionary<string, List<string>> acPropertyMap)
        {
            acPropertyMap = new Dictionary<string, List<string>>();
            using (DbContext dbContext = new DbContext())
            {
                string acpAliasName = "acpName";
                string acAliasName = "acName";
                string akSql = $"SELECT acp.{nameof(ActivityProperty.Name)} as {acpAliasName},ac.{nameof(Activity.Name)} as {acAliasName} FROM {nameof(ActivityProperty)} as acp " +
                             $"LEFT JOIN {nameof(Activity)} as ac ON acp.{nameof(ActivityProperty.ActivityId)} = ac.{nameof(Activity.Id)} ";

                DataTable akTable = dbContext.Client.Ado.GetDataTable(akSql);

                if (akTable != null)
                {
                    foreach (DataRow row in akTable.Rows)
                    {
                        string acpName = row.Field<string>(acpAliasName);
                        string acName = row.Field<string>(acAliasName);
                        if (!acPropertyMap.ContainsKey(acName))
                        {
                            acPropertyMap.Add(acName, new List<string>() { acpName });
                        }
                        else
                        {
                            acPropertyMap[acName].Add(acpName);
                        }
                    }
                }
            }
        }

        private void InitPropertyKeywordActivityPropertyData(out Dictionary<string, List<ActivityPropertyKeyValue>> akApMap)
        {
            akApMap = new Dictionary<string, List<ActivityPropertyKeyValue>>();
            using (DbContext dbContext = new DbContext())
            {
                string pkAliasName = "pkName";
                string acAliasName = "acName";
                string apAliasName = "apName";
                string pkapSql = $"SELECT pk.{nameof(ParameterKeyword.Name)} as {pkAliasName},ac.{nameof(Activity.Name)} as {acAliasName},ap.{nameof(ActivityProperty.Name)} as {apAliasName} FROM {nameof(ParameterKeywordActivityPropertyMapping)} as pkap " +
                             $"LEFT JOIN {nameof(ParameterKeyword)} as pk ON pkap.{nameof(ParameterKeywordActivityPropertyMapping.ParameterKeywordId)} = pk.{nameof(ParameterKeyword.Id)} " +
                             $"LEFT JOIN {nameof(ActivityProperty)} as ap ON pkap.{nameof(ParameterKeywordActivityPropertyMapping.ActivityPropertyId)} = ap.{nameof(ActivityProperty.Id)} " +
                             $"LEFT JOIN {nameof(Activity)} as ac ON ap.{nameof(ActivityProperty.ActivityId)} = ac.{nameof(Activity.Id)}";

                DataTable pkapTable = dbContext.Client.Ado.GetDataTable(pkapSql);

                if (pkapTable != null)
                {
                    foreach (DataRow row in pkapTable.Rows)
                    {
                        string akName = row.Field<string>(pkAliasName);
                        string acName = row.Field<string>(acAliasName);
                        string apName = row.Field<string>(apAliasName);
                        if (!akApMap.ContainsKey(akName.ToLower()))
                        {
                            akApMap.Add(akName.ToLower(), new List<ActivityPropertyKeyValue> { new ActivityPropertyKeyValue { ActivityName = acName, PropertyName = apName } });
                        }
                        else
                        {
                            akApMap[akName.ToLower()].Add(new ActivityPropertyKeyValue { ActivityName = acName, PropertyName = apName });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 输入待解析文本内容
        /// </summary>
        /// <param name="text">待识别的文本</param>
        /// <param name="additionalResourcePath">额外需要的文件例如excel</param>
        /// <param name="error">错误信息</param>
        /// <returns>解析得到的活动集合</returns>
        public List<ActivityDescription> InputText(string text, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            List<ActivityDescription> adTotalList = new List<ActivityDescription>();
            List<Command> cuList = _interpreter.AnalysisText(text);
            foreach (Command cu in cuList)
            {
                if (cu.ErrorMessage != null)
                {
                    error = $"异常位置({cu.Index}){cu.Value} ：" + cu.ErrorMessage;
                    return adTotalList;
                }
                List<ActivityDescription> activityDescriptions = new List<ActivityDescription>();
                _convertor.AnlysisCommand(cu, activityDescriptions);
                adTotalList.AddRange(activityDescriptions);
            }
            //设置ActivityDescription.IsHasSelector的属性值
            SetIsHasSelector(adTotalList);
            return adTotalList;
        }

        /// <summary>
        /// 输入解析得到的活动集合
        /// </summary>
        /// <param name="ads">解析得到的活动集合</param>
        /// <param name="additionalResourceDir">附加资源文件目录</param> 
        /// <param name="saveXamlPath">xaml的保存路径</param> 
        public void GenerateXaml(List<ActivityDescription> ads, string additionalResourceDir, string saveXamlPath)
        {
            _activityXamlManager.GenerateXaml(ads, additionalResourceDir, saveXamlPath);
        }

        //private List<ActivityDescription> UnionRecordXaml(List<ActivityDescription> activityDescriptions)
        //{
        //    RecordXamlManager recordXamlManager = new RecordXamlManager();
        //    Dictionary<int, XamlActivityDescription> xadList = recordXamlManager.ReadXaml(Config.RecordXamlPath);
        //    if (xadList != null && xadList.Any() && activityDescriptions.Any())
        //    {
        //        List<int> handledIndex = new List<int>();
        //        UnionRecordXamlCore(activityDescriptions, xadList, handledIndex);
        //    }
        //    return activityDescriptions;
        //}

        //private void UnionRecordXamlCore(List<ActivityDescription> ads, Dictionary<int, XamlActivityDescription> xadList, List<int> handledIndex)
        //{
        //    foreach (ActivityDescription ad in ads)
        //    {
        //        if (ad.ActivityName == nameof(ForEachRowActivity) ||
        //            ad.ActivityName == nameof(ForEachActivity) ||
        //            ad.ActivityName == nameof(WhileActivity))
        //        {
        //            List<ActivityDescription> subAds = ad.Properties["Body"] as List<ActivityDescription>;
        //            HandledLoopLogic(subAds, xadList, handledIndex);
        //            continue;
        //        }
        //        //是否活动名称匹配
        //        bool result = xadList.Any(w => string.Compare(ad.ActivityName, w.Value.ActivityName, true) == 0);
        //        if (!result)
        //        {
        //            continue;
        //        }

        //        int max = handledIndex.Any() ? handledIndex.Max() + 1 : 0;

        //        for (int i = max; i < xadList.Count; i++)
        //        {
        //            foreach (string p in ad.AllParameters)
        //            {
        //                //LogTextActivity的message匹配到关键字并且LogTextActivity下是四个满足逻辑的组合，匹配GetText活动的逻辑
        //                if ((xadList[i].ActivityName == "LogMessageActivity" && !string.IsNullOrEmpty(xadList[i].Message) && xadList[i].Message.Contains(p) && i + 1 < xadList.Count) &&
        //                    (xadList[i + 1].ActivityName == nameof(ClickActivity)) &&
        //                    (xadList[i + 2].ActivityName == nameof(HotKeyActivity) && xadList[i + 1].SelectedKey != null && xadList[i + 1].SelectedKey.ToLower() == "key_c") &&
        //                    (xadList[i + 3].ActivityName == nameof(ClickActivity)) &&
        //                    (xadList[i + 4].ActivityName == nameof(HotKeyActivity) && xadList[i + 4].SelectedKey != null && xadList[i + 4].SelectedKey.ToLower() == "key_v"))
        //                {
        //                    handledIndex.Add(i);
        //                    handledIndex.Add(i + 1);
        //                    handledIndex.Add(i + 2);
        //                    handledIndex.Add(i + 3);
        //                    handledIndex.Add(i + 4);
        //                    if (!ad.Properties.ContainsKey("Selector"))
        //                    {
        //                        ad.Properties.Add("Selector", xadList[i + 1]?.Selector);
        //                    }
        //                    else
        //                    {
        //                        ad.Properties["Selector"] = xadList[i + 1]?.Selector;
        //                    }
        //                    break;
        //                }
        //                //LogTextActivity的message匹配到关键字
        //                else if (xadList[i].ActivityName == "LogMessageActivity" && !string.IsNullOrEmpty(xadList[i].Message) && xadList[i].Message.Contains(p) && i + 1 < xadList.Count)
        //                {
        //                    handledIndex.Add(i);
        //                    handledIndex.Add(i + 1);
        //                    if (!ad.Properties.ContainsKey("Selector"))
        //                    {
        //                        ad.Properties.Add("Selector", xadList[i + 1].Selector);
        //                    }
        //                    else
        //                    {
        //                        ad.Properties["Selector"] = xadList[i + 1].Selector;
        //                    }
        //                    break;
        //                }
        //                //是否录制的活动选择器里包含解析文本里的关键字信息以及或者text属性的文本是否匹配
        //                else if ((xadList[i].Selector != null && xadList[i].Selector.Contains(p)) || string.Compare(xadList[i].Text, p, true) == 0)
        //                {
        //                    handledIndex.Add(i);
        //                    if (!ad.Properties.ContainsKey("Selector"))
        //                    {
        //                        ad.Properties.Add("Selector", xadList[i].Selector);
        //                    }
        //                    else
        //                    {
        //                        ad.Properties["Selector"] = xadList[i].Selector;
        //                    }
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

        //private void HandledLoopLogic(List<ActivityDescription> ads, Dictionary<int, XamlActivityDescription> xadList, List<int> handledIndex)
        //{
        //    Dictionary<int, XamlActivityDescription> dic = new Dictionary<int, XamlActivityDescription>();
        //    var first = xadList.FirstOrDefault(f => f.Value.ActivityName == "LogMessageActivity" && !string.IsNullOrEmpty(f.Value.Message) && f.Value.Message.Contains("循环开始"));
        //    var last = xadList.LastOrDefault(f => f.Value.ActivityName == "LogMessageActivity" && !string.IsNullOrEmpty(f.Value.Message) && f.Value.Message.Contains("循环结束"));
        //    for (int i = first.Key + 1; i < last.Key; i++)
        //    {
        //        handledIndex.Add(i);
        //        dic.Add(i, xadList[i]);
        //    }
        //    UnionRecordXamlCore(ads, dic, handledIndex);
        //}

        private void SetIsHasSelector(List<ActivityDescription> activityDescriptions)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IActivity))));
            foreach (ActivityDescription activityDescription in activityDescriptions)
            {
                if (types.Any(a => a.Name == activityDescription.ActivityName))
                {
                    var type = types.First(a => a.Name == activityDescription.ActivityName);
                    var obj = (IActivity)Activator.CreateInstance(type);
                    activityDescription.IsHasSelector = (bool)type.GetProperty("IsHasSelector").GetValue(obj);
                }
                if (activityDescription.Properties == null || !activityDescription.Properties.Any())
                {
                    continue;
                }
                var value = activityDescription.Properties.Values.SingleOrDefault(a => a.GetType() == typeof(List<ActivityDescription>)) as List<ActivityDescription>;
                if (value == null)
                {
                    continue;
                }
                SetIsHasSelector(value);
            }
        }
    }
}
