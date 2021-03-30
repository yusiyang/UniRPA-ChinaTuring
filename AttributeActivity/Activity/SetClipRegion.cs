using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;
using Plugins.Shared.Library.UiAutomation;
using System;
using System.Activities;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows;

namespace AttributeActivity
{
    [Designer(typeof(SetClipRegionDesigner))]
    public sealed class SetClipRegion : CodeActivity
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
        [DisplayName("元素")]
        [Description("使用另一个活动返回的元素对象。该属性不能与“选取器”属性一起使用。该字段仅支持用户界面元素变量。")]
        public InArgument<UiElement> Element { get; set; }

        [Category("输入")]
        [DisplayName("方向")]
        [Description("剪切区域的扩大方向。可用的选项如下：左侧、顶部、右侧、底部、平移。")]
        public DirectionEnums Direction { get; set; }

        #endregion


        #region 属性分类：大小

        [Category("大小")]
        [DisplayName("左侧")]
        [Description("向左侧移动的像素偏移量。支持正数和负数。")]
        public InArgument<Int32> Left { get; set; }

        [Category("大小")]
        [DisplayName("右侧")]
        [Description("向右侧移动的像素偏移量。支持正数和负数。")]
        public InArgument<Int32> Right { get; set; }

        [Category("大小")]
        [DisplayName("顶部")]
        [Description("向顶部移动的像素偏移量。支持正数和负数。")]
        public InArgument<Int32> Top { get; set; }

        [Category("大小")]
        [DisplayName("底部")]
        [Description("向底部移动的像素偏移量。支持正数和负数。")]
        public InArgument<Int32> Bottom { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath
        {
            get
            {
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Attribute/SetClipRegion.png";
            }
        }

        #endregion


        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
        }

        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                UiElement uiElement = this.Element.Get(context);
                int left = Left.Get(context);
                int right = Right.Get(context);
                int top = Top.Get(context);
                int bottom = Bottom.Get(context);

                Rectangle rect1 = uiElement.BoundingRectangle;
                Rectangle rect2 = default(Rectangle);
                switch (this.Direction)
                {
                    case DirectionEnums.Left:
                        if (left != 0)
                        {
                            rect2 = Rectangle.FromLTRB(rect1.Left - left, rect1.Top, rect1.Right, rect1.Bottom);
                        }
                        break;
                    case DirectionEnums.Top:
                        if (top != 0)
                        {
                            rect2 = Rectangle.FromLTRB(rect1.Left, rect1.Top - top, rect1.Right, rect1.Bottom);
                        }
                        break;
                    case DirectionEnums.Right:
                        if (right != 0)
                        {
                            rect2 = Rectangle.FromLTRB(rect1.Left, rect1.Top, rect1.Right + right, rect1.Bottom);
                        }
                        break;
                    case DirectionEnums.Bottom:
                        if (bottom != 0)
                        {
                            rect2 = Rectangle.FromLTRB(rect1.Left, rect1.Top, rect1.Right, rect1.Bottom + bottom);
                        }
                        break;
                    case DirectionEnums.Translate:
                        rect2 = Rectangle.FromLTRB(rect1.Left - left, rect1.Top - top, rect1.Right + right, rect1.Bottom + bottom);
                        break;
                }
                //if (!rect2.IsEmpty)
                //{
                //    uiElement.ClippingRegion = new Region
                //    {
                //        Rectangle = new Rectangle?(rect2)
                //    };
                //    return;
                //}
                uiElement.BoundingRectangle = rect2;
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
