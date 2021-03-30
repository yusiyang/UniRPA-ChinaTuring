using System.Collections.Generic;
using System.Xml;

namespace Uni.Core
{
    /// <summary>
    /// 录制文本解析
    /// </summary>
    public class RecordXamlManager
    {
        /// <summary>
        /// 解析xaml文本获取对应的活动序列信息
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Dictionary<int, XamlActivityDescription> ReadXaml(string fileName)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            XmlNode xn = doc.ChildNodes[0];
            XmlNodeList xnList = xn?.ChildNodes;
            if (xnList == null)
            {
                return null;
            }
            Dictionary<int, XamlActivityDescription> dic = new Dictionary<int, XamlActivityDescription>();
            int count = 0;
            GetSequenceChildren(xnList, dic, ref count);
            return dic;
        }

        private void GetSequenceChildren(XmlNodeList xChildNodes, Dictionary<int, XamlActivityDescription> dic, ref int count)
        {
            foreach (XmlNode xChildNode in xChildNodes)
            {
                if (xChildNode.Name == "Sequence")
                {
                    GetSequenceChildren(xChildNode.ChildNodes, dic, ref count);
                    continue;
                }
                if (xChildNode.ParentNode.Name != "Sequence")
                {
                    continue;
                }
                XamlActivityDescription xad = new XamlActivityDescription();
                xad.ActivityName = xChildNode.LocalName;
                if (xChildNode.Attributes.GetNamedItem("Selector") != null)
                {
                    xad.Selector = xChildNode.Attributes["Selector"].Value;
                }
                if (xChildNode.Attributes.GetNamedItem("Text") != null)
                {
                    xad.Text = xChildNode.Attributes["Text"].Value;
                }
                if (xChildNode.Attributes.GetNamedItem("SelectedKey") != null)
                {
                    xad.SelectedKey = xChildNode.Attributes["SelectedKey"].Value;
                }
                if (xChildNode.Attributes.GetNamedItem("Message") != null)
                {
                    xad.Message = xChildNode.Attributes["Message"].Value;
                }
                dic.Add(count, xad);
                count++;
            }
        }
    }
}
