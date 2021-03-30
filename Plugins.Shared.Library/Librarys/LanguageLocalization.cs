using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public class LanguageLocalization
    {
        /// <summary>
        /// 汉化 Actipro 控件
        /// </summary>
        public static void ToChinese()
        {
            ActiproSoftware.Products.Shared.SR.SetCustomString(ActiproSoftware.Products.Shared.SRName.UICommandCloseWindowText.ToString(), "关闭");
            ActiproSoftware.Products.Shared.SR.SetCustomString(ActiproSoftware.Products.Shared.SRName.UICommandMaximizeWindowText.ToString(), "最大化");
            ActiproSoftware.Products.Shared.SR.SetCustomString(ActiproSoftware.Products.Shared.SRName.UICommandMinimizeWindowText.ToString(), "最小化");
            ActiproSoftware.Products.Shared.SR.SetCustomString(ActiproSoftware.Products.Shared.SRName.UICommandRestoreWindowText.ToString(), "还原");

            ActiproSoftware.Products.Ribbon.SR.SetCustomString(ActiproSoftware.Products.Ribbon.SRName.UICustomizeMenuItemMinimizeRibbonText.ToString(), "最小化功能区");
            ActiproSoftware.Products.Ribbon.SR.SetCustomString(ActiproSoftware.Products.Ribbon.SRName.UIScreenTipToggleMinimizationUpHeader.ToString(), "最小化功能区");
            ActiproSoftware.Products.Ribbon.SR.SetCustomString(ActiproSoftware.Products.Ribbon.SRName.UIScreenTipToggleMinimizationUpDescription.ToString(), "仅在功能区上显示选项卡名称。");

            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandMakeFloatingWindowText.ToString(), "浮动");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandMakeDockedWindowText.ToString(), "停靠");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandMakeDocumentWindowText.ToString(), "停靠为文档");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandToggleWindowAutoHideStateText.ToString(), "自动隐藏");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandCloseWindowText.ToString(), "关闭");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandCloseOthersText.ToString(), "关闭其他");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandCloseAllInContainerText.ToString(), "关闭选项卡组");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandCloseAllDocumentsText.ToString(), "关闭所有文档");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandPinTabText.ToString(), "固定选项卡");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandMoveToNewHorizontalContainerText.ToString(), "新建水平选项卡组");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandMoveToNewVerticalContainerText.ToString(), "新建垂直选项卡组");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandMoveToNextContainerText.ToString(), "移至下一个选项卡组");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandMoveToPreviousContainerText.ToString(), "移至上一个选项卡组");
            ActiproSoftware.Products.Docking.SR.SetCustomString(ActiproSoftware.Products.Docking.SRName.UICommandMoveToPrimaryMdiHostText.ToString(), "移至主文档组");
        }
    }
}
