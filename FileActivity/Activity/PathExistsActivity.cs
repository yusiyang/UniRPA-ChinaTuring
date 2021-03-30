using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using System;
using System.Activities;
using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace FileActivity
{
    [Designer(typeof(PathExistsDesigner))]
    public sealed class PathExistsActivity : CodeActivity
    {
        #region 属性分类：常见

        public string _displayName;
        [Category("常见")]
        [DisplayName("显示名称")]
        public new string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    _displayName = base.DisplayName;
                }
                else
                {
                    base.DisplayName = _displayName;
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }
        [Browsable(false)]
        public string _DisplayName
        {
            get
            {
                return this.DisplayName;
            }
        }

        [Category("常见")]
        [DisplayName("出错时继续")]
        [Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        public bool ContinueOnError { get; set; }

        [Category("常见")]
        [DisplayName("在此之前延迟")]
        [Description("活动开始执行任何操作之前的延迟时间（以毫秒为单位）。默认时间量为200毫秒。")]
        public InArgument<int> DelayBefore { get; set; }

        [Category("常见")]
        [DisplayName("在此之后延迟")]
        [Description("执行活动之后的延迟时间（以毫秒为单位）。默认时间量为300毫秒。")]
        public InArgument<int> DelayAfter { get; set; }

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [RequiredArgument]
        [DisplayName("路径")]
        [Description("要检查的全路径。必须将文本放入引号中。")]
        public InArgument<string> Path { get; set; }

        [Category("输入")]
        [DisplayName("路径类型")]
        [Description("指定路径是指向文档还是文件。")]
        public int PathType { get; set; }

        #endregion


        #region 属性分类：输出

        [Category("输出")]
        [DisplayName("存在")]
        [Description("说明是否找到文档或文件。")]
        public OutArgument<Boolean> Exists { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/file/fileexist.png";
            }
        }

        #endregion


        static PathExistsActivity()
        {
            AttributeTableBuilder builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(PathExistsActivity), "PathType", new EditorAttribute(typeof(PathTypeEditor), typeof(PathTypeEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                string _Path = Path.Get(context);
                Boolean _Exists = false;
                if (PathType == (int)PathTypeEditor.PathTypeEnum.File)            
                {
                     _Exists = File.Exists(_Path);
                }
                else
                {
                    _Exists = Directory.Exists(_Path);
                }

                Exists.Set(context, _Exists);
            }

            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }
    }
}
