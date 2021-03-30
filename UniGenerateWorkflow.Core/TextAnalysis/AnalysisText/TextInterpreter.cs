using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Uni.Entity;

namespace Uni.Core
{
    /// <summary>
    /// 文本解释器
    /// </summary>
    public class TextInterpreter
    {
        /// <summary>
        /// 全角终止符号
        /// </summary>`
        private const char SBCCaseEndFlag = '；';

        /// <summary>
        /// 行为和对象关键字文本解析
        /// </summary>
        private readonly ContentCheckManager _akContentCheckManager;

        /// <summary>
        /// 活动属性关键字内容解析
        /// </summary>
        private readonly ContentCheckManager _pkContentCheckManager;

        /// <summary>
        /// 控制流文本解析管理
        /// </summary>
        private readonly ControlFlowAnalysisManager _controlFlowManager;

        /// <summary>
        /// 行为关键字
        /// </summary>
        private readonly List<string> _actionWords;

        /// <summary>
        /// 对象关键字
        /// </summary>
        private readonly List<string> _objectWords;

        private readonly BehaviorManager _behaviorManager;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="actionWords">行为关键字集合</param>
        /// <param name="objectWords">对象关键字集合</param>
        /// <param name="parameterWords">活动属性关键字集合</param>
        /// <param name="controlFlows">控制流文本集合</param>
        /// <param name="behaviorManager">行为映射数据集合</param>
        public TextInterpreter(List<string> actionWords,
            List<string> objectWords,
            List<string> parameterWords,
            Dictionary<string, KeywordTypeEnum> controlFlows,
            BehaviorManager behaviorManager)
        {
            if (actionWords == null || !actionWords.Any())
            {
                throw new ArgumentNullException(nameof(actionWords));
            }
            if (objectWords == null || !objectWords.Any())
            {
                throw new ArgumentNullException(nameof(objectWords));
            }
            if (parameterWords == null || !parameterWords.Any())
            {
                throw new ArgumentNullException(nameof(parameterWords));
            }
            _actionWords = actionWords;
            _objectWords = objectWords;
            _behaviorManager = behaviorManager;

            List<string> words = new List<string>();
            words.AddRange(actionWords);
            words.AddRange(objectWords);

            //初始化流控制文本处理
            _controlFlowManager = new ControlFlowAnalysisManager(controlFlows, this);

            _akContentCheckManager = new ContentCheckManager(new KeywordsLibrary(words.Distinct().ToArray()));
            _pkContentCheckManager = new ContentCheckManager(new KeywordsLibrary(parameterWords.Distinct().ToArray()));
        }

        /// <summary>
        /// 解析文本
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <returns>命令单元</returns>
        public List<Command> AnalysisText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            string[] commnadunitsStr = text.Split(new char[] { TextInterpreter.SBCCaseEndFlag }, StringSplitOptions.RemoveEmptyEntries);

            List<int> handledIndexs = new List<int>();
            List<Command> commands = new List<Command>();
            Command preUnit = null;
            for (int i = 0; i < commnadunitsStr.Length; i++)
            {
                //跳过已经处理过的命令 
                if (handledIndexs.Contains(i))
                {
                    continue;
                }
                handledIndexs.Add(i);
                Command command = GetCommand(commnadunitsStr[i], i, commnadunitsStr, handledIndexs);
                if (i - 1 >= 0)
                {
                    command.Previous = preUnit;
                }
                if (i + 1 < commnadunitsStr.Length)
                {
                    command.Next = GetCommand(commnadunitsStr[i + 1], i + 1, commnadunitsStr, handledIndexs);
                }
                if (command.Previous != null)
                {
                    command.Previous.Next = command;
                }
                preUnit = command;
                commands.Add(command);
            }
            return commands;
        }

        /// <summary>
        /// 获取命令单元
        /// </summary>
        /// <param name="text">当前命令单元对应的文本内容</param>
        /// <param name="index">索引</param>
        /// <param name="commnadunitsStr">所有文本内容集合</param>
        /// <returns>命令单元</returns>
        public Command GetCommand(string text, int index, string[] commnadunitsStr, List<int> handledIndexs, IUnit parent = null)
        {
            Command command = new Command();
            command.Value = text;
            command.Index = index;
            command.Parent = parent;

            Dictionary<int, string> dic = _akContentCheckManager.ExtractKeywords(text, out string error);
            if (!string.IsNullOrEmpty(error))
            {
                command.ErrorMessage = error;
                return command;
            }

            //处理控制流匹配
            command.ControlFlow = _controlFlowManager.GetContorlFlow(text, index, commnadunitsStr, handledIndexs);

            if ((dic == null || !dic.Any()) && command.ControlFlow == null)
            {
                return command;
            }
            IUnit preUnit = null;
            for (int i = 0; i < dic.Count(); i++)
            {
                IUnit unit = GetUnit(i, dic[i], command);
                if (dic.ContainsKey(i - 1))
                {
                    unit.Previous = preUnit;
                }
                if (dic.ContainsKey(i + 1))
                {
                    unit.Next = GetUnit(i, dic[i + 1], command);
                }
                if (unit.Previous != null)
                {
                    unit.Previous.Next = unit;
                }
                preUnit = unit;

                if (unit != null)
                {
                    if (unit is Parameter && unit.Next is ObjectKey && unit.Previous is ObjectKey)
                    {
                        command.ErrorMessage = ErrorMessage.Parameter_UnrecognizeParametersAmongWithObjects + "参数：" + unit.Value;
                        return command;
                    }
                    command.Units = command.Units ?? new List<IUnit>();
                    command.Units.Add(unit);
                }
            }
            return command;
        }

        private IUnit GetUnit(int index, string text, Command parent = null)
        {
            //行为关键字
            if (_actionWords.Contains(text))
            {
                ActionKey actionKey = new ActionKey();
                actionKey.Index = index;
                actionKey.Value = text;
                actionKey.Parent = parent;
                return actionKey;
            }

            //对象关键字
            if (_objectWords.Contains(text))
            {
                ObjectKey objectKey = new ObjectKey();
                objectKey.Index = index;
                objectKey.Value = text;
                objectKey.Parent = parent;
                return objectKey;
            }

            //参数
            if (text.StartsWith(Parameter.StartFlag.ToString()) && text.EndsWith(Parameter.EndFlag.ToString()))
            {
                Parameter p = new Parameter
                {
                    Index = index,
                    Keywords = new List<string>(),
                    Parent = parent,
                    Value = text.Substring(1, text.Length - 2)
                };

                //将参数关键字和父级对象提供的文件数据进行匹配
                if (parent != null && parent.Parent != null)
                {
                    ForeachRow foreachRow = parent.Parent as ForeachRow;
                    if (foreachRow != null)
                    {
                        string key = Path.GetFileNameWithoutExtension(foreachRow.DataTable);
                        if (string.Compare(p.Value, key, true) == 0)
                        {
                            p.Value = foreachRow.DataTable;
                            return p;
                        }
                    }
                }

                //解析当识别到参数文本匹配知识库里的行为数据的时候，取真正惯用的数据代替
                string parameterText = _behaviorManager.GetValue(p.Value);
                if (string.IsNullOrEmpty(parameterText))
                {
                    parameterText = p.Value;
                }
                Dictionary<int, string> propertyDic = _pkContentCheckManager.ExtractKeywords(parameterText.ToLower(), out var pError);
                if (!string.IsNullOrEmpty(pError))
                {
                    return null;
                }
                p.Keywords.AddRange(propertyDic.Values);
                return p;
            }
            return null;
        }
    }
}
