using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Uni.Entity;

namespace Uni.Core
{
    /// <summary>
    /// 控制流文本解析管理
    /// </summary>
    public class ControlFlowAnalysisManager
    {
        private readonly List<ControlFlow> _controlFlows = new List<ControlFlow>();
        private readonly TextInterpreter _textInterpreter;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="controlFlows"></param>
        public ControlFlowAnalysisManager(Dictionary<string, KeywordTypeEnum> controlFlows, TextInterpreter textInterpreter)
        {
            _textInterpreter = textInterpreter;
            InitControlFlowCache(controlFlows);
        }

        private void InitControlFlowCache(Dictionary<string, KeywordTypeEnum> controlFlows)
        {
            //提取控制流中的关键字
            if (controlFlows != null && controlFlows.Any())
            {
                foreach (var cfWords in controlFlows)
                {
                    //获取正则
                    string[] kwArray = cfWords.Key.Split(new string[] { ControlFlow.PlaceholderFlag }, StringSplitOptions.RemoveEmptyEntries);
                    string reg = $".*{cfWords.Key.Replace(ControlFlow.PlaceholderFlag, ".+")}";
                    if (!reg.EndsWith(".*"))
                    {
                        reg += ".*";
                    }
                    List<ActionKey> akList = new List<ActionKey>();
                    int index = 0;
                    foreach (string kw in kwArray)
                    {
                        akList.Add(new ActionKey() { Index = index, Value = kw });
                        index++;
                    }

                    ControlFlow cf = null;
                    if (cfWords.Value == KeywordTypeEnum.IfElse)
                    {
                        cf = new IfElse();
                    }
                    if (cfWords.Value == KeywordTypeEnum.While)
                    {
                        cf = new While();
                    }
                    if (cfWords.Value == KeywordTypeEnum.Foreach)
                    {
                        cf = new Foreach();
                    }
                    if (cfWords.Value == KeywordTypeEnum.ForeachRow)
                    {
                        cf = new ForeachRow();
                    }
                    cf.Regex = reg;
                    cf.Value = cfWords.Key;
                    cf.ActionKeys = akList;
                    _controlFlows.Add(cf);
                }
            }
        }

        /// <summary>
        /// 获取控制流列表
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <returns>控制流解析列表</returns>
        public ControlFlow GetContorlFlow(string text, int index, string[] commnadunitStrs, List<int> handledIndexs)
        {
            foreach (ControlFlow cf in _controlFlows)
            {
                bool isMatched = Regex.IsMatch(text, cf.Regex);
                if (!isMatched)
                {
                    continue;
                }
                List<string> tempList = new List<string>();
                List<string> segmentList = new List<string>();
                for (int i = 0; i < cf.ActionKeys.Count + 1; i++)
                {
                    if (i != cf.ActionKeys.Count)
                    {
                        tempList.Add(cf.ActionKeys[i].Value);
                    }
                    else
                    {
                        string regex = string.Format(ControlFlow.AfterRegexModal, tempList[0]);
                        Match match = Regex.Match(text, regex);
                        segmentList.Add(match.Value);
                    }

                    if (tempList.Count == 2)
                    {
                        string regex = string.Format(ControlFlow.AmongRegexModal, tempList[0], tempList[1]);
                        Match match = Regex.Match(text, regex);
                        segmentList.Add(match.Value);
                        tempList.RemoveAt(0);
                    }
                }

                switch (cf.Type)
                {
                    case UnitType.Foreach:
                        HandleForeach(cf, segmentList);
                        break;
                    case UnitType.ForeachRow:
                        HandleForeachRow(cf, segmentList, index, commnadunitStrs, handledIndexs);
                        break;
                    case UnitType.While:
                        HandleWhile(cf, segmentList);
                        break;
                    case UnitType.IfElse:
                        HandleIfElse(cf, segmentList, index, commnadunitStrs);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                return cf;
            }
            return null;
        }

        private void HandleForeachRow(ControlFlow cf, List<string> segmentList, int index, string[] commnadUnitStrs, List<int> handledIndexs)
        {
            ForeachRow fr = cf as ForeachRow;
            fr.DataTable = segmentList.Count > 0 ? segmentList[0] : null;
            fr.Body = new List<Command>();
            for (int i = index + 1; i < commnadUnitStrs.Length; i++)
            {
                string commandUnitStr = commnadUnitStrs[i].TrimStart();
                if (commandUnitStr.StartsWith(ForeachRow.SubCommandFlag) ||
                commandUnitStr.StartsWith("\n" + ForeachRow.SubCommandFlag) ||
                commandUnitStr.StartsWith(Environment.NewLine + ForeachRow.SubCommandFlag)
                )
                {
                    handledIndexs.Add(i);
                    Command command = _textInterpreter.GetCommand(commandUnitStr, i, commnadUnitStrs, handledIndexs, fr);
                    fr.Body.Add(command);
                }
                else
                {
                    break;
                }
            }
        }

        private void HandleForeach(ControlFlow cf, List<string> segmentList)
        {
            Foreach f = cf as Foreach;
            f.Values = segmentList.Count > 0 ? segmentList[0] : null;
            f.Body = segmentList.Count > 1 ? segmentList[1] : null;
        }

        private void HandleWhile(ControlFlow cf, List<string> segmentList)
        {
            While w = cf as While;
            w.Condition = segmentList.Count > 0 ? segmentList[0] : null;
            w.Body = segmentList.Count > 1 ? segmentList[1] : null;
        }

        private void HandleIfElse(ControlFlow cf, List<string> segmentList, int index, string[] commnadunitsStr)
        {
            IfElse i = cf as IfElse;
            i.Condition = segmentList.Count > 0 ? segmentList[0] : null;
            i.Then = segmentList.Count > 1 ? segmentList[1] : null;
            i.Else = segmentList.Count > 2 ? segmentList[2] : null;
            if (!string.IsNullOrEmpty(i.Then))
            {
                i.ThenCommand = _textInterpreter.GetCommand(i.Then, index, commnadunitsStr, null);
            }
            if (!string.IsNullOrEmpty(i.Else))
            {
                i.ElseCommand = _textInterpreter.GetCommand(i.Else, index, commnadunitsStr, null);
            }
        }
    }
}
