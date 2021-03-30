using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Activities.Presentation.Metadata;
using System.Activities.Presentation.PropertyEditing;
using Plugins.Shared.Library.Editors;
using MouseActivity;
using System.Threading;

namespace WorkflowUtils
{
    [Designer(typeof(CommentDesigner))]
    public sealed class CommentActivity : CodeActivity
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

        #endregion


        #region 属性分类：输入

        [Category("输入")]
        [DisplayName("注释内容")]
        [Description("指定注释内容。")]
        public string Text { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/WorkflowUtils/comment.png";
            }
        }

        #endregion


        public CommentActivity()
        {
            //Text = "// 请在Text属性中写下注释内容";

            var builder = new AttributeTableBuilder();
            builder.AddCustomAttributes(typeof(CommentActivity), "Text", new EditorAttribute(typeof(TextEditor), typeof(DialogPropertyValueEditor)));
            MetadataStore.AddAttributeTable(builder.CreateTable());
        }

        protected override void Execute(CodeActivityContext context)
        {

        }
    }
}
