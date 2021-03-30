using System;
using System.Activities;
using System.ComponentModel;
using System.Threading;
using Microsoft.Office.Interop.Word;
using MouseActivity;
using Plugins.Shared.Library;
using Plugins.Shared.Library.Exceptions;

namespace WordPlugins
{
    [Designer(typeof(CursorMoveDesigner))]
    public sealed class CursorMove : CodeActivity
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

        InArgument<Int32> _MovePos = 1;
        [Category("输入")]
        [DisplayName("游标移动距离")]
        public InArgument<Int32> MovePos
        {
            get
            {
                return _MovePos;
            }
            set
            {
                _MovePos = value;
            }
        }

        #endregion


        #region 属性分类：选项

        bool _IsSelect = false;
        [Category("选项")]
        [DisplayName("是否选中")]
        public bool IsSelect
        {
            get
            {
                return _IsSelect;
            }
            set
            {
                _IsSelect = value;
            }
        }

        #endregion


        #region 属性分类：方向

        bool _Left = true, _Right = false, _Up = false, _Down = false;
        [Category("方向")]
        [DisplayName("左")]
        public bool Left
        {
            get
            {
                return _Left;
            }
            set
            {
                _Left = value;
                NotifyPropertyChanged("Left");
            }
        }

        [Category("方向")]
        [DisplayName("右")]
        public bool Right
        {
            get
            {
                return _Right;
            }
            set
            {
                _Right = value;
                NotifyPropertyChanged("Right");
            }
        }

        [Category("方向")]
        [DisplayName("上")]
        public bool Up
        {
            get
            {
                return _Up;
            }
            set
            {
                _Up = value;
                NotifyPropertyChanged("Up");
            }
        }

        [Category("方向")]
        [DisplayName("下")]
        public bool Down
        {
            get
            {
                return _Down;
            }
            set
            {
                _Down = value;
                NotifyPropertyChanged("Down");
            }
        }

        #endregion


        #region 属性分类：杂项

        [Browsable(false)]
        public string icoPath { get { return "pack://application:,,,/Plugins.Shared.Library;Component/Resource/Image/Activities/word/move1.png"; } }

        #endregion


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        protected override void Execute(CodeActivityContext context)
        {
            int delayAfter = Common.GetValueOrDefault(context, this.DelayAfter, 300);
            int delayBefore = Common.GetValueOrDefault(context, this.DelayBefore, 200);
            Thread.Sleep(delayBefore);

            try
            {
                Int32 _movePos = MovePos.Get(context);
                CommonVariable.sel = CommonVariable.app.Selection;

                if (_Left)
                {
                    if (_IsSelect)
                        CommonVariable.sel.MoveLeft(WdUnits.wdCharacter, _movePos, WdMovementType.wdExtend);
                    else
                        CommonVariable.sel.MoveLeft(WdUnits.wdCharacter, _movePos, WdMovementType.wdMove);
                }
                else if (_Right)
                {
                    if (_IsSelect)
                        CommonVariable.sel.MoveRight(WdUnits.wdCharacter, _movePos, WdMovementType.wdExtend);
                    else
                        CommonVariable.sel.MoveRight(WdUnits.wdCharacter, _movePos, WdMovementType.wdMove);
                }
                else if (_Up)
                {
                    if (_IsSelect)
                        CommonVariable.sel.MoveUp(WdUnits.wdLine, _movePos, WdMovementType.wdExtend);
                    else
                        CommonVariable.sel.MoveUp(WdUnits.wdLine, _movePos, WdMovementType.wdMove);
                }
                else if (_Down)
                {
                    if (_IsSelect)
                        CommonVariable.sel.MoveDown(WdUnits.wdLine, _movePos, WdMovementType.wdExtend);
                    else
                        CommonVariable.sel.MoveDown(WdUnits.wdLine, _movePos, WdMovementType.wdMove);
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
