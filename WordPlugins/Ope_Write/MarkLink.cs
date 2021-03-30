using System;
using System.Activities;
using System.ComponentModel;
using System.Reflection;
using Word = Microsoft.Office.Interop.Word;
using Plugins.Shared.Library;
using MouseActivity;
using System.Threading;
using Plugins.Shared.Library.Exceptions;

namespace WordPlugins
{
    [Designer(typeof(MarkLinkDesigner))]
    public sealed class MarkLink : CodeActivity
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


        #region 属性分类：图片

        InArgument<string> _Pic;
        [Category("图片")]
        [RequiredArgument]
        [OverloadGroup("Picture")]
        [DisplayName("图片链接")]
        public InArgument<string> Pic
        {
            get { return _Pic; }
            set { _Pic = value; }
        }

        #endregion


        #region 属性分类：书签

        InArgument<string> _BookMark;
        [Category("书签")]
        [RequiredArgument]
        [OverloadGroup("BookMark")]
        [DisplayName("书签名称")]
        public InArgument<string> BookMark
        {
            get { return _BookMark; }
            set { _BookMark = value; }
        }

        #endregion


        #region 属性份额里：超链接

        InArgument<string> _LinkName;
        [Category("超链接")]
        [RequiredArgument]
        [OverloadGroup("Link")]
        [DisplayName("超链接名称")]
        public InArgument<string> LinkName
        {
            get { return _LinkName; }
            set { _LinkName = value; }
        }

        InArgument<string> _LinkMark;
        [Category("超链接")]
        [RequiredArgument]
        [OverloadGroup("Link")]
        [DisplayName("超链接标签")]
        public InArgument<string> LinkMark
        {
            get { return _LinkMark; }
            set { _LinkMark = value; }
        }

        InArgument<string> _LinkAddr;
        [Category("超链接")]
        [RequiredArgument]
        [OverloadGroup("Link")]
        [DisplayName("超链接地址")]
        public InArgument<string> LinkAddr
        {
            get { return _LinkAddr; }
            set { _LinkAddr = value; }
        }

        #endregion


        #region 杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/word/mark.png"; } }

        #endregion


        public MarkLink()
        {
        }

        //设置属性可见/不可见
        private void SetPropertyVisibility(object obj, string propertyName, bool visible)
        {
            try
            {
                Type type = typeof(BrowsableAttribute);
                PropertyDescriptorCollection props = TypeDescriptor.GetProperties(obj);
                AttributeCollection attrs = props[propertyName].Attributes;
                FieldInfo fld = type.GetField("browsable", BindingFlags.Instance | BindingFlags.NonPublic);
                fld.SetValue(attrs[type], visible);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex);
            }
        }

        //设置属性只读
        //private void SetPropertyReadOnly(object obj, string propertyName, bool readOnly)
        //{
        //    try
        //    {
        //        Type type = typeof(ReadOnlyAttribute);
        //        PropertyDescriptorCollection props = TypeDescriptor.GetProperties(obj);
        //        AttributeCollection attrs = props[propertyName].Attributes;
        //        FieldInfo fld = type.GetField("ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance);
        //        fld.SetValue(attrs[type], readOnly);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Exception: " + ex);
        //    }
        //}

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                CommonVariable.sel = CommonVariable.app.Selection;
                CommonVariable.doc = CommonVariable.app.ActiveDocument;
                CommonVariable.range = CommonVariable.sel.Range;

                string linkName = LinkName.Get(context);
                string linkMark = LinkMark.Get(context);
                string linkAddr = LinkAddr.Get(context);
                string bookMark = BookMark.Get(context);
                string pic = Pic.Get(context);

                if (linkName != null)
                {
                    CommonVariable.links = CommonVariable.doc.Hyperlinks;
                    CommonVariable.links.Add(CommonVariable.range, linkAddr, linkMark, "", linkName, linkMark);
                }
                if (bookMark != null)
                {
                    CommonVariable.marks = CommonVariable.doc.Bookmarks;
                    CommonVariable.mark = CommonVariable.marks.Add(bookMark);
                }
                if (pic != null)
                {
                    Word::InlineShapes lineshapes = CommonVariable.sel.InlineShapes;
                    Word::InlineShape lineshape = lineshapes.AddPicture(pic);
                }
            }
            catch (Exception e)
            {
                SharedObject.Instance.Output(SharedObject.OutputType.Error, DisplayName + "失败", e.Message);
                CommonVariable.realaseProcessExit();
                if (!ContinueOnError)
                {
                    throw new ActivityRuntimeException(this.DisplayName, e);
                }
            }

            Thread.Sleep(delayAfter);
        }
    }
}
