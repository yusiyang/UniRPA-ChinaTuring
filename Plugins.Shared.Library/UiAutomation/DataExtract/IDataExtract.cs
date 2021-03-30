using Plugins.Shared.Library.UiAutomation.Browser;

namespace Plugins.Shared.Library.UiAutomation.DataExtract
{
    public interface IDataExtract
    {
        string GetColumnData(UiElement element, string extractOption);
        void ColorColumn(UiElement element, string name);
        bool Compare(UiElement element1,UiElement element2);
        void StepTo(UiElement element);
        void SetAttribute(UiElement element, string attrName, string attrValue);
        string GetSameColumn(UiElement element, string input);
        IBrowser GetBrowser(UiElement element);
    }
}
