using System;
using System.Collections.Generic;
using System.Linq;

namespace Uni.Core
{
    /// <summary>
    /// 关键词检测
    /// </summary>
    public class ContentCheckManager
    {
        /// <summary>
        /// 词树
        /// </summary>
        private readonly KeywordsLibrary _keywordsLibrary;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="library">词库</param>
        public ContentCheckManager(KeywordsLibrary library)
        {
            if (library.Tree == null)
            {
                throw new ArgumentNullException(nameof(library.Tree));
            }

            _keywordsLibrary = library;
        }

        /// <summary>
        /// 从文本中提取关键字集合
        /// </summary>
        /// <param name="text">文本</param>
        /// <returns>关键字集合</returns>
        public Dictionary<int, string> ExtractKeywords(string text, out string error)
        {
            error = string.Empty;
            Dictionary<int, string> dic = new Dictionary<int, string>();
            CharacterTree p = _keywordsLibrary.Tree;
            string word = string.Empty;
            string pWord = string.Empty;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                //提取参数
                if ((!string.IsNullOrEmpty(pWord) && c == Parameter.StartFlag) || (string.IsNullOrEmpty(pWord) && c == Parameter.EndFlag))
                {
                    error = $"解析参数异常。错误位置：{pWord}";
                    return null;
                }
                else if (c == Parameter.StartFlag)
                {
                    word = string.Empty;
                    pWord = Parameter.StartFlag.ToString();
                    continue;
                }
                else if (!string.IsNullOrEmpty(pWord) && c != Parameter.EndFlag)
                {
                    pWord += c.ToString();
                    continue;
                }
                else if (!string.IsNullOrEmpty(pWord) && c == Parameter.EndFlag)
                {
                    pWord += Parameter.EndFlag.ToString();
                    dic.Add(dic.Count(), pWord);
                    pWord = string.Empty;
                    continue;
                }

                //提取关键字
                List<CharacterTree> child = p.Children;
                CharacterTree node = child.Find(e => e.Character == c);
                if (node != null)
                {
                    word += c.ToString();
                    if (node.IsEnd || node.Children == null)
                    {
                        //当出现重叠关键字时候取最长字符的关键字，例如，‘导航至’和‘导航’，取'导航至’ 
                        if (node.Children == null)
                        {
                            dic.Add(dic.Count(), word);
                        }
                        else
                        {
                            int k = i + 1;
                            if (k < text.Length && node.Children.Exists(e => e.Character == text[k]))
                            {
                                p = node;
                                continue;
                            }
                        }
                        word = string.Empty;
                        p = _keywordsLibrary.Tree;
                    }
                    else
                    {
                        p = node;
                    }
                }
                else
                {
                    word = string.Empty;
                    if (p.GetHashCode() != _keywordsLibrary.GetHashCode())
                    {
                        p = _keywordsLibrary.Tree;
                    }
                }
            }
            return dic;
        }
    }
}