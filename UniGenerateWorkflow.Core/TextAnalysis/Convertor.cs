using System;
using System.Collections.Generic;
using System.Linq;

namespace Uni.Core
{
    /// <summary>
    /// 转换器，负责识别命令单元，查找到对应的活动
    /// </summary>
    public class Convertor
    {
        /// <summary>
        /// 行为关键字和活动映射集合 [行为关键字/活动名称集合]
        /// </summary>
        private readonly Dictionary<string, List<string>> _akAcDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// 对象关键字和活动映射集合 [对象关键字/活动名称集合]
        /// </summary>
        private readonly Dictionary<string, List<string>> _okAcDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// 活动和活动属性名称映射集合 [活动名称/活动属性名称集合]
        /// </summary>
        private readonly Dictionary<string, List<string>> _acPropertyDic = new Dictionary<string, List<string>>();

        /// <summary>
        /// 属性关键字和活动活动属性属性值映射集合 [属性关键字/活动活动属性属性值映射集合]
        /// </summary>
        private readonly Dictionary<string, List<ActivityPropertyKeyValue>> _pkApDic = new Dictionary<string, List<ActivityPropertyKeyValue>>();

        private readonly BehaviorManager _behaviorManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="akAcDic">行为关键字和活动映射集合</param>
        /// <param name="okAcDic">对象关键字和活动映射集合</param>
        /// <param name="acPropertyDic">活动和活动属性名称映射集合</param>
        /// <param name="pkApDic">属性关键字和活动活动属性属性值映射集合</param>  
        public Convertor(
            Dictionary<string, List<string>> akAcDic,
            Dictionary<string, List<string>> okAcDic,
            Dictionary<string, List<string>> acPropertyDic,
            Dictionary<string, List<ActivityPropertyKeyValue>> pkApDic,
            BehaviorManager behaviorManager
            )
        {
            _akAcDic = akAcDic;
            _okAcDic = okAcDic;
            _acPropertyDic = acPropertyDic;
            _pkApDic = pkApDic;
            _behaviorManager = behaviorManager;

        }

        /// <summary>
        /// 解析命令单元
        /// </summary>
        /// <param name="command">命令单元</param>
        /// <param name="activityDescriptions">活动描述集合</param>
        /// <returns></returns>
        public void AnlysisCommand(Command command, List<ActivityDescription> activityDescriptions)
        {
            if (command == null || !string.IsNullOrEmpty(command.ErrorMessage))
            {
                return;
            }
            //处理控制流方式
            if (command.ControlFlow != null)
            {
                ActivityDescription ad = HandleControlFlowKey(command.ControlFlow);
                //原始命令文本内容
                ad.OiginalCommandText = command.Value;
                activityDescriptions.Add(ad);
                return;
            }

            //非控制流处理方式
            if (command.Units == null || !command.Units.Any())
            {
                return;
            }
            List<int> handledIndexs = new List<int>();
            List<string> allParams = new List<string>();
            foreach (IUnit unit in command.Units)
            {
                if (unit is Parameter)
                {
                    allParams.Add(unit.Value);
                }

                //跳过已经处理过的单元 
                if (handledIndexs.Contains(unit.Index))
                {
                    continue;
                }
                handledIndexs.Add(unit.Index);
                ActivityDescription ad = null;
                switch (unit.Type)
                {
                    case UnitType.ActionKey:
                        ad = HandleActionKey(unit, handledIndexs);
                        break;
                    case UnitType.ObjectKey:
                        // ad = HandleObjectKey(unit);   //暂时不考虑单个对象关键词识别的逻辑
                        break;
                    case UnitType.Parameter:
                        ad = HandleParameterKey(unit, activityDescriptions, handledIndexs);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (ad != null)
                {
                    ad.AllParameters = allParams;
                    //原始命令文本内容
                    ad.OiginalCommandText = command.Value;
                    activityDescriptions.Add(ad);
                }
            }
        }

        /// <summary>
        /// 处理行为单元
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="handledIndexs"></param>
        /// <returns></returns>
        private ActivityDescription HandleActionKey(IUnit unit, List<int> handledIndexs)
        {
            ActionKey actionKey = unit as ActionKey;
            if (actionKey == null)
            {
                return null;
            }
            List<string> afterOklist = null;
            IUnit nextUnit = actionKey.Next;
            //如果actionKey下一个单元对象是objectKey类型
            if (nextUnit != null && nextUnit.Type == UnitType.ObjectKey)
            {
                handledIndexs.Add(nextUnit.Index);
                afterOklist = GetActivityByObjectKey((ObjectKey)nextUnit);
            }
            //如果actionKey下一个单元对象是参数类型的则再往后找一个object单元对象的
            else if (nextUnit != null && nextUnit.Type == UnitType.Parameter)
            {
                IUnit p = nextUnit;
                if (p.Next != null && p.Next.Type == UnitType.ObjectKey)
                {
                    nextUnit = p.Next;
                    handledIndexs.Add(p.Next.Index);
                    afterOklist = GetActivityByObjectKey((ObjectKey)p.Next);
                }
            }
            //如果actionKey的前一个单元和后一个单元都是参数
            if (actionKey.Previous != null &&
                actionKey.Previous.Type == UnitType.Parameter &&
                actionKey.Next != null &&
                actionKey.Next.Type == UnitType.Parameter)
            {
                handledIndexs.Add(actionKey.Index);
            }

            //取action和object的交集
            List<string> list = GetActivityByActionKey(actionKey);
            var reusltList = list == null ? null : afterOklist?.Intersect(list);
            ActivityDescription ad = null;
            if (reusltList != null && reusltList.Any())
            {
                ad = new ActivityDescription();
                ad.ActivityName = reusltList.First();
                ad.RelatedUnits = new List<string>() { actionKey.Value, nextUnit.Value };
                ad.RelatedIndexes = new List<int>() { actionKey.Index, nextUnit.Index };
            }
            else if (afterOklist != null)
            {
                ad = new ActivityDescription();
                ad.ActivityName = afterOklist.First();
                ad.RelatedUnits = new List<string>() { nextUnit.Value };
                ad.RelatedIndexes = new List<int>() { nextUnit.Index };
            }
            else if (list != null)
            {
                ad = new ActivityDescription();
                ad.ActivityName = list.First();
                ad.RelatedUnits = new List<string>() { actionKey.Value };
                ad.RelatedIndexes = new List<int>() { actionKey.Index };
            }
            return ad;
        }

        /// <summary>
        /// 处理对象单元
        /// </summary>
        /// <param name="unit">对象单元</param>
        /// <returns></returns>
        private ActivityDescription HandleObjectKey(IUnit unit)
        {
            ObjectKey objectKey = unit as ObjectKey;
            List<string> list = GetActivityByObjectKey((ObjectKey)objectKey);
            if (list != null && list.Any())
            {
                ActivityDescription ad = new ActivityDescription();
                ad.ActivityName = list.First();
                ad.RelatedUnits = new List<string>() { objectKey.Value };
                ad.RelatedIndexes = new List<int>() { objectKey.Index };
                return ad;
            }
            return null;
        }

        /// <summary>
        /// 处理参数单元
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="activityDescriptions">活动描述</param>
        private ActivityDescription HandleParameterKey(IUnit unit, List<ActivityDescription> activityDescriptions, List<int> handledIndexs)
        {
            Parameter p = unit as Parameter;
            if (p == null)
            {
                return null;
            }

            //参数位于ObjectKey之后
            IUnit preUnit = p.Previous;
            while (preUnit != null && preUnit.Type != UnitType.ObjectKey)
            {
                preUnit = preUnit.Previous;
            }
            ActivityDescription result = null;
            ActivityDescription activityInfo = null;
            //参数位于ObjectKey和ActionKey之间
            if (p.Previous != null &&
                p.Previous.Type == UnitType.ActionKey &&
                p.Next != null &&
                p.Next.Type == UnitType.ObjectKey)
            {
                activityInfo = activityDescriptions?.SingleOrDefault(s => s.RelatedIndexes.Contains(p.Previous.Index) && s.RelatedIndexes.Contains(p.Next.Index));
            }
            //参数位于ActionKey之后且，且参数之后没有任何单元
            else if (p.Previous != null && p.Previous.Type == UnitType.ActionKey)
            {
                activityInfo = activityDescriptions?.SingleOrDefault(s => s.RelatedIndexes.Contains(p.Previous.Index));
            }
            //参数位于ActionKey之前
            else if (p.Next != null && p.Next.Type == UnitType.ActionKey)
            {
                result = HandleActionKey(p.Next, handledIndexs);
                activityInfo = result;
            }
            else if (p.Previous != null)
            {
                activityInfo = activityDescriptions?.SingleOrDefault(s => s.RelatedIndexes.Contains(p.Previous.Index));
                if (activityInfo == null)
                {
                    //找到前面第一个不属于参数的节点
                    IUnit previousUnit = p.Previous;
                    while (previousUnit.Previous != null && previousUnit.Previous.Type == UnitType.Parameter)
                    {
                        previousUnit.Previous = previousUnit.Previous.Previous;
                    }
                    activityInfo = activityDescriptions?.SingleOrDefault(s => s.RelatedIndexes.Contains(previousUnit.Previous.Index));
                }
            }

            if (activityInfo != null)
            {
                activityInfo.Properties = activityInfo.Properties ?? new Dictionary<string, object>();
                if (_acPropertyDic.ContainsKey(activityInfo.ActivityName))
                {
                    List<string> properties = _acPropertyDic[activityInfo.ActivityName];
                    string value = _behaviorManager.GetValue(unit.Value);
                    bool isAnalysised = false;
                    //如果匹配到符合的selector内容，直接使用该selector 
                    string selectorValue = _behaviorManager.GetSelectorValue(unit.Value);
                    if (selectorValue != null && properties.Contains("Selector"))
                    {
                        isAnalysised = true;
                        activityInfo.Properties.Add("Selector", selectorValue);
                    }
                    else if (properties.Count == 1) //如果只有一个属性值
                    {
                        isAnalysised = true;
                        activityInfo.Properties.Add(properties.First(), value ?? unit.Value);
                    }
                    else if (properties.Count > 1 && p.Keywords.Any())  //如果有多个属性值
                    {
                        foreach (string word in p.Keywords)
                        {
                            var propertyValue = _pkApDic[word.ToLower()].FirstOrDefault(a => a.ActivityName == activityInfo.ActivityName);
                            if (propertyValue == null)
                            {
                                continue;
                            }
                            isAnalysised = true;
                            activityInfo.Properties.Add(propertyValue.PropertyName, word);
                        }
                    }
                    if (!isAnalysised) //参数无法被识别
                    {
                        activityInfo.UnrecognizedParameters = activityInfo.UnrecognizedParameters ?? new List<string>();
                        activityInfo.UnrecognizedParameters.Add(value ?? unit.Value);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 处理控制流单元
        /// </summary>
        /// <param name="controlFlow"></param>
        /// <returns></returns>
        private ActivityDescription HandleControlFlowKey(ControlFlow controlFlow)
        {
            ActivityDescription ad = new ActivityDescription();
            if (_akAcDic.ContainsKey(controlFlow.Value))
            {
                List<string> list = _akAcDic[controlFlow.Value];
                if (list != null && list.Any())
                {
                    ad.ActivityName = list[0];
                }
                else
                {
                    return null;
                }
            }

            ad.Properties = new Dictionary<string, object>();
            switch (controlFlow.Type)
            {
                case UnitType.Foreach:
                    Foreach f = controlFlow as Foreach;
                    ad.Properties.Add(nameof(f.Values), f.Values);
                    ad.Properties.Add(nameof(f.Body), f.Body);
                    break;
                case UnitType.ForeachRow:
                    ForeachRow fr = controlFlow as ForeachRow;
                    //ad.Properties.Add(nameof(fr.DataTable), fr.DataTable);
                    MappingPropertiesToActivities(nameof(fr.Body), fr.Body, ad);
                    break;
                case UnitType.While:
                    While w = controlFlow as While;
                    ad.Properties.Add(nameof(w.Condition), w.Condition);
                    MappingPropertyToActivity(nameof(w.Body), w.Body, w.BodyCommand, ad);
                    break;
                case UnitType.IfElse:
                    IfElse i = controlFlow as IfElse;
                    ad.Properties.Add(nameof(i.Condition), i.Condition);
                    MappingPropertyToActivity(nameof(i.Then), i.Then, i.ThenCommand, ad);
                    MappingPropertyToActivity(nameof(i.Else), i.Else, i.ElseCommand, ad);
                    break;
                default:
                    throw new NotImplementedException();
            }
            return ad;
        }

        /// <summary>
        /// 将属性进步一解析成活动 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="command"></param>
        /// <param name="ad"></param>
        private void MappingPropertyToActivity(string propertyName, object propertyValue, Command command, ActivityDescription ad)
        {
            if (command != null)
            {
                List<ActivityDescription> childAd = new List<ActivityDescription>();
                AnlysisCommand(command, childAd);
                if (childAd.Count == 1)
                {
                    ad.Properties.Add(propertyName, childAd.First());
                }
            }
            else
            {
                ad.Properties.Add(propertyName, propertyValue);
            }
        }

        private void MappingPropertiesToActivities(string propertyName, List<Command> commands, ActivityDescription ad)
        {
            if (commands != null && commands.Any())
            {
                List<ActivityDescription> adTemp = new List<ActivityDescription>();
                foreach (Command command in commands)
                {
                    List<ActivityDescription> childAd = new List<ActivityDescription>();
                    AnlysisCommand(command, childAd);
                    adTemp.AddRange(childAd);
                }
                ad.Properties.Add(propertyName, adTemp);
            }
        }

        private List<string> GetActivityByActionKey(ActionKey actionKey)
        {
            if (actionKey != null)
            {
                if (_akAcDic.ContainsKey(actionKey.Value))
                {
                    return _akAcDic[actionKey.Value];
                }
            }
            return null;
        }

        private List<string> GetActivityByObjectKey(ObjectKey objectKey)
        {
            if (objectKey != null)
            {
                if (_okAcDic.ContainsKey(objectKey.Value))
                {
                    return _okAcDic[objectKey.Value];
                }
            }
            return null;
        }
    }
}
