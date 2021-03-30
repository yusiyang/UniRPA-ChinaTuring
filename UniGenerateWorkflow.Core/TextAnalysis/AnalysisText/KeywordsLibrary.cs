using System.Collections.Generic;
using System.Linq;

namespace Uni.Core
{
    /// <summary>
    /// 关键词库
    /// </summary>
    public class KeywordsLibrary
    {
        /// <summary>
        /// 词库树
        /// </summary>
        public CharacterTree Tree { get; }

        /// <summary>
        /// 关键词库
        /// </summary>
        /// <param name="words">关键词组</param>
        public KeywordsLibrary(string[] words)
        {
            if (words == null)
            {
                words = new[] { string.Empty };
            }
            Tree = new CharacterTree() { Character = 'R', IsEnd = false, Children = CreateTree(words) };
        } 

        /// <summary>
        /// 创建词库树
        /// </summary>
        /// <param name="words">关键词组</param>
        /// <returns></returns>
        private List<CharacterTree> CreateTree(string[] words)
        {
            List<CharacterTree> tree = null;
            if (words != null && words.Length > 0)
            {
                tree = new List<CharacterTree>();
                foreach (string item in words)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        char cha = item[0];
                        CharacterTree node = tree.Find(e => e.Character == cha);
                        if (node != null)
                        {
                            AddChildTree(node, item);
                        }
                        else
                        {
                            tree.Add(CreateSingleTree(item));
                        }
                    }
                }
            }
            return tree;
        }

        /// <summary>
        /// 创建单个完整树
        /// </summary>
        /// <param name="word">单个关键词</param>
        /// <returns></returns>
        private CharacterTree CreateSingleTree(string word)
        {
            CharacterTree root = new CharacterTree();
            CharacterTree p = root;
            for (int i = 0; i < word.Length; i++)
            {
                CharacterTree child = new CharacterTree() { Character = word[i], IsEnd = false, Children = null };
                p.Children = new List<CharacterTree>() { child };
                p = child;
            }
            p.IsEnd = true;

            return root.Children.First();
        }

        /// <summary>
        /// 附加分支子树
        /// </summary>
        /// <param name="childTree">子树</param>
        /// <param name="word">单个关键词</param>
        private void AddChildTree(CharacterTree childTree, string word)
        { 
            CharacterTree p = childTree;
            for (int i = 1; i < word.Length; i++)
            {
                char cha = word[i];
                List<CharacterTree> child = p.Children;
                if (child == null)
                {
                    CharacterTree node = new CharacterTree() { Character = cha, IsEnd = false, Children = null };
                    p.Children = new List<CharacterTree>() { node };
                    p = node;
                }
                else
                {
                    CharacterTree node = child.Find(e => e.Character == cha);
                    if (node == null)
                    {
                        node = new CharacterTree() { Character = cha, IsEnd = false, Children = null };
                        child.Add(node);
                        p = node;
                    }
                    else
                    {
                        p = node;
                    }
                }
            }
            p.IsEnd = true;
        }
    }
}