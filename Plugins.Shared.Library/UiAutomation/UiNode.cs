using FlaUI.Core;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Plugins.Shared.Library.UiAutomation
{
    public interface UiNode
    {
        #region 属性
        /// <summary>
        /// 用户自定义Id，提供额外的标识信息，以辅助元素精确定位，目前主要为Java节点使用
        /// </summary>
        string UserDefineId { get; }

        /// <summary>
        /// 当节点无任何属性信息时，通过当前元素位置Idx(从0开始)来辅助快速定位，比如<Pane />这样的无属性节点
        /// </summary>
        string Idx { get; }

        /// <summary>
        /// AutomationId属性
        /// </summary>
        string AutomationId { get; }

        /// <summary>
        /// ClassName属性
        /// </summary>
        string ClassName { get;}

        /// <summary>
        /// ControlType属性
        /// </summary>
        string ControlType { get; }

        /// <summary>
        /// Name属性
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Role属性
        /// </summary>
        string Role { get; }

        /// <summary>
        /// Description属性
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 所属进程名
        /// </summary>
        string ProcessName { get; }

        /// <summary>
        /// 进程全路径
        /// </summary>
        string ProcessFullPath { get; }

        /// <summary>
        /// 窗口句柄
        /// </summary>
        IntPtr WindowHandle { get; }

        /// <summary>
        /// 父节点
        /// </summary>
        UiNode Parent { get; }

        /// <summary>
        /// AutomationElement父节点
        /// </summary>
        UiNode AutomationElementParent { get; }

        /// <summary>
        /// 包围元素的矩形框
        /// </summary>
        Rectangle BoundingRectangle { get;}

        /// <summary>
        /// 子节点集合
        /// </summary>
        List<UiNode> Children { get; }

        /// <summary>
        /// 是否为顶级窗口
        /// </summary>
        bool IsTopLevelWindow { get;}

        /// <summary>
        /// 支持的所有Pattern
        /// </summary>
        FrameworkAutomationElementBase.IFrameworkPatterns Patterns { get; }

        #endregion

        #region 方法

        #region 公共
        /// <summary>
        /// 查找所有符合条件的节点
        /// </summary>
        /// <param name="scope">查找范围</param>
        /// <param name="condition">查找条件</param>
        /// <returns>符合条件的所有节点的集合</returns>
        List<UiNode> FindAll(TreeScope scope, ConditionBase condition);

        /// <summary>
        /// 设置显示到最前
        /// </summary>
        void SetForeground();

        /// <summary>
        /// 获取点击点的坐标
        /// </summary>
        /// <returns></returns>
        Point GetClickablePoint();

        /// <summary>
        /// 通过索引号获取子节点
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        UiNode GetChildByIdx(int idx);

        #endregion

        #region 鼠标操作

        /// <summary>
        /// 点击
        /// </summary>
        /// <param name="clickParams">可指定鼠标按键、点击类型、是否显示移动过程、操作方式、点击元素位置、偏移坐标</param>
        void MouseClick(UiElementClickParams clickParams = null);

        /// <summary>
        /// 模拟点击
        /// </summary>
        void SimulateClick();

        /// <summary>
        /// 悬停
        /// </summary>
        /// <param name="hoverParams">可指定是否显示鼠标移动过程</param>
        void MouseHover(UiElementHoverParams hoverParams = null);

        #endregion

        #region 键盘操作

        /// <summary>
        /// 模拟录入
        /// </summary>
        /// <param name="text">被录入的字符串</param>
        void SimulateTypeText(string text);

        #endregion

        #region 控件操作
        /// <summary>
        /// 高亮元素
        /// </summary>
        /// <param name="color">颜色</param>
        /// <param name="duration">持续时间</param>
        /// <param name="blocking">是否阻塞</param>
        void HighLight(Color? color = null, TimeSpan? duration = null, bool blocking = false);

        /// <summary>
        /// 设置焦点
        /// </summary>
        void Focus();

        /// <summary>
        /// 获取元素是否被勾选
        /// </summary>
        /// <returns>是否被勾选</returns>
        bool IsChecked();

        /// <summary>
        /// 勾选
        /// </summary>
        void Check();

        /// <summary>
        /// 取消勾选
        /// </summary>
        void UnCheck();

        /// <summary>
        /// 切换勾选
        /// </summary>
        void Toggle();

        /// <summary>
        /// 获取文本
        /// </summary>
        /// <returns>获取到的文本</returns>
        string GetText();

        /// <summary>
        /// 设置文本
        /// </summary>
        void SetText(string text);

        /// <summary>
        /// 获取元素是否为密码控件
        /// </summary>
        /// <returns>是否为密码控件</returns>
        bool IsPassword();

        /// <summary>
        /// 清空当前的选中状态
        /// </summary>
        void ClearSelection();

        /// <summary>
        /// 选择条目
        /// </summary>
        /// <param name="item">要选中的条目</param>
        void SelectItem(string item);

        /// <summary>
        /// 多选条目
        /// </summary>
        /// <param name="items">要选中的条目集合</param>
        void SelectMultiItems(string[] items);

        /// <summary>
        /// 清除文本
        /// </summary>
        void ClearText();

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <param name="attributeName">属性名称</param>
        /// <returns>属性值</returns>
        object GetAttributeValue(String attributeName);

        /// <summary>
        /// 是否可用
        /// </summary>
        /// <returns>是否可用</returns>
        bool IsEnable();

        /// <summary>
        /// 是否可见
        /// </summary>
        /// <returns>是否可见</returns>
        bool IsVisible();

        /// <summary>
        /// 是否为表格
        /// </summary>
        /// <returns>是否为表格</returns>
        bool IsTable();

        #endregion

        #endregion
    }


}
