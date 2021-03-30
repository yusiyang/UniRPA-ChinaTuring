using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace Plugins.Shared.Library.UiAutomation.Browser
{
    public interface IBrowser:IJavaScriptExecutor
    {
        string Title { get; }
        BrowserType Type { get; }
        string Name { get; }
        UiNode UiNode { get; }
        IntPtr Hwd { get; }
        bool Available { get; }
        bool Active { get; }
        int Ratio { get; }
        ReadyState ReadyState { get; }
        void Navigate(Uri uri);
        void Close();
        void Refresh();
        void Activate();
        void Stop();
        void GoHome();
        void GoForward();
        void GoBack();
        void Open(Uri uri,string arguments,int timeOut);
        void WaitPage(int timeOut);
        UiNode FindHtmlNode(Dictionary<string, string> attrDic);
        UiNode FindHtmlNode(string title, string selector);
        UiNode GetElementFromPoint(Point screenPoint);
        string InjectAndRunJs(string jsCode, string param, UiNode target);
    }

    public enum BrowserType
    {
        InternetExplorer,
        Chrome,
        Firefox
    }
    public enum ReadyState
    {
        None,
        Interactive,
        Complete,
        UnInitialized,
        Loading,
        Loaded,
    }
}
