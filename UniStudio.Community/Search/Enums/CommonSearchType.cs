using System.ComponentModel;

namespace UniStudio.Community.Search.Enums
{
    public enum CommonSearchType
    {
        [Description("当前文档")]
        CurrentFile,

        [Description("所有文档")]
        AllFiles,

        [Description("活动")]
        Activities,

        [Description("变量")]
        Variables,

        [Description("参数")]
        Arguments,

        [Description("引入")]
        Imports,

        [Description("项目文件")]
        ProjectFiles,

        [Description("依赖")]
        Dependencies,

        [Description("代码块")]
        Snippets
    }
}
