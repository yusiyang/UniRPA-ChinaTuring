using MouseActivity;
using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace FindActivity
{
    [Designer(typeof(AnchorBaseDesigner))]
    public sealed class AnchorBase : NativeActivity
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

        //[Category("常见")]
        //[DisplayName("出错时继续")]
        //[Description("指定即使在当前活动失败的情况下，仍继续执行剩余的活动。仅支持布尔值（True,False）。")]
        //public bool ContinueOnError { get; set; }

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
        [DisplayName("锚点位置")]
        [Description("指定将空间锚定到容器的哪个边。可用的选项如下：自动、左侧、顶部、右侧、底部。")]
        public AnchorPositionEnums AnchorPosition { get; set; }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public Activity AnchorBody { get; set; }

        [Browsable(false)]
        public Activity ActivityBody { get; set; }

        [Browsable(false)]
        public string icoPath
        {
            get
            {                                                                                                                                                                                                                                                                                                                            
                return @"pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/Find/AnchorBase.png";
            }
        }

        [Browsable(false)]
        public string SourceImgPath { get; set; }

        #endregion


        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

            //限制一组允许的活动类型
            //if(AnchorBody!=null)
            //{
            //    if (AnchorBody.GetType() != typeof(ClickActivity))
            //    {
            //        metadata.AddValidationError("Child activity is not of type WriteLine or Assign");
            //    }
            //}
        }

        protected override void Execute(NativeActivityContext context)
        {

            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            // Do something...

            Thread.Sleep(delayAfter);
        }

        //void InternalExecute(NativeActivityContext context, ActivityInstance instance)
        //{
        //    //grab the index of the current Activity
        //    int currentActivityIndex = this.currentIndex.Get(context);
        //    if (currentActivityIndex == children.Count)
        //    {
        //        //if the currentActivityIndex is equal to the count of MySequence's Activities
        //        //MySequence is complete
        //        return;
        //    }

        //    if (this.onChildComplete == null)
        //    {
        //        //on completion of the current child, have the runtime call back on this method
        //        this.onChildComplete = new CompletionCallback(InternalExecute);
        //    }

        //    //grab the next Activity in MySequence.Activities and schedule it
        //    Activity nextChild = children[currentActivityIndex];
        //    context.ScheduleActivity(nextChild, this.onChildComplete);

        //    //increment the currentIndex
        //    this.currentIndex.Set(context, ++currentActivityIndex);
        //}

        private void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            //TODO
        }

        private void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            //TODO
        }
    }
}
