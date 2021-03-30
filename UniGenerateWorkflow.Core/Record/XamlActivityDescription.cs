namespace Uni.Core
{
    /// <summary>
    /// 解析xaml文件获取活动描述信息
    /// </summary>
    public class XamlActivityDescription
    {
        /// <summary>
        /// 活动名称
        /// </summary>
        public string ActivityName { get; set; }

        /// <summary>
        /// 选择器
        /// </summary>
        public string Selector { get; set; }

        /// <summary>
        /// 活动文本属性
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 活动SelectedKey属性
        /// </summary>
        public string SelectedKey { get; set; }

        /// <summary>
        /// 活动Message属性
        /// </summary>
        public string Message { get; set; }
    }
}
