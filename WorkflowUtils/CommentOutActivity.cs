using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.ComponentModel;
using System.Activities.Statements;
using MouseActivity;
using System.Threading;

namespace WorkflowUtils
{
    [Designer(typeof(CommentOutDesigner))]
    public sealed class CommentOutActivity : CodeActivity
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


        #region 属性分类：杂项

        [Browsable(false)]
        public Activity Body { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/WorkflowUtils/comment.png";
            }
        }

        #endregion


        public CommentOutActivity()
        {
            Body = new Sequence
            {
                DisplayName = "忽略的活动"
            };
        }

        protected override void Execute(CodeActivityContext context)
        {

        }
    }
}
