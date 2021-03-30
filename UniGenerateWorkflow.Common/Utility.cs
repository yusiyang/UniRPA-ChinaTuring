using System;

namespace Uni.Common
{
    /// <summary>
    /// 随机数帮助类
    /// </summary>
    public class Utility
    {
        private static int rep = 0;

        /// <summary>
        /// 获取指定长度随机数
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns>随机数文本</returns>
        public static string GetRandomNumber(int length)
        {
            var randomNubmer = new char[length];
            char[] dictionary = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            var random = new Random();
            for (var i = 0; i < length; i++)
            {
                randomNubmer[i] = dictionary[random.Next(dictionary.Length - 1)];
            }
            return new string(randomNubmer);
        }

        /// <summary>
        /// 生成指定长度的随机码
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GenerateRandomCode(int length)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + rep;
            rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> rep)));
            for (int i = 0; i < length; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch.ToString();
            }
            return str;
        }
    }
}
