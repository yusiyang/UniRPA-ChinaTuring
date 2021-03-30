namespace Plugins.Shared.Library.UiAutomation.Browser
{
    public interface IJavaScriptExecutor
    {
        object ExecuteScript(string script, params object[] args);
    }
}