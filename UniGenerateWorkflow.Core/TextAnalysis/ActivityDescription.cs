using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Uni.Core
{
    /// <summary>
    /// 活动描述
    /// </summary>
    [Serializable]
    public class ActivityDescription
    {
        /// <summary>
        /// 活动名称
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// 活动属性集合
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// 无法识别的参数
        /// </summary>
        public List<string> UnrecognizedParameters { get; set; }

        /// <summary>
        /// 该活动对应命令的所有参数单元文本集合
        /// </summary>
        public List<string> AllParameters { get; set; }

        /// <summary>
        /// 该活动是由哪些单元对象共同解析而得
        /// </summary>
        public List<string> RelatedUnits { get; set; }

        /// <summary>
        /// 共同解析而得的单元对象索引集合
        /// </summary>
        public List<int> RelatedIndexes { get; set; }

        /// <summary>
        /// 原始命令文本内容
        /// </summary>
        public string OiginalCommandText { get; set; }

        /// <summary>
        /// 是否含有Selector属性
        /// </summary>
        public bool IsHasSelector { get; set; }

        /// <summary>
        /// 复制
        /// </summary>
        /// <returns></returns>
        public ActivityDescription Clone()
        {
            //深复制
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, this);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as ActivityDescription;
            }
        }
    }
}
