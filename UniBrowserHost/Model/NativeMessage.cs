using System.Collections.Generic;

namespace UniBrowserHost.Model
{
    public class NativeMessage
    {
        public static int CurrentRequestId;
        public NativeMessage()
        {
            CurrentRequestId++;
        }
        public Dictionary<string, Map> AttrMap { get; set; }
        public int? CodeVersion { get; set; }
        public string FunctionCall { get; set; }
        public int? Index { get; set; }
        public string ParentCustomId { get; set; }
        public int? RequestId { get; set; }
        public object TabId { get; set; }
        public int? WindowId { get; set; }
        public int? ReturnId { get; set; }
        
        public bool? IsTraceEnabled { get; set; }
        
        public int? RetCode { get; set; }

        public string CustomId { get; set; }
        

        public string AttrName { get; set; }
        public string AttrValue { get; set; }

        public int? ScreenX { get; set; }
        public int? ScreenY { get; set; }

        public int? PageRenderOfsX { get; set; }
        public int? PageRenderOfsY { get; set; }
        public int? UseClientCoordinates { get; set; }
        public int? WindowLeft { get; set; }
        public int? WindowTop { get; set; }

        public int? GetFlags { get; set; }
        public List<NodeHierarchy> NodeHierarchyInfo { get; set; }

        public int? Left { get; set; }
        public int? Right { get; set; }
        public int? Top { get; set; }
        public int? Bottom { get; set; }


        public string Command { get; set; }

        public Map TagName { get; set; }

        
        public bool? IsBrowserReady { get; set; }

        public int? DevicePixelRatioPercentage { get; set; }
        public string JsCode { get; set; }
        public bool? IsScriptErr { get; set; }
        public string Result { get; set; }
        public string Text { get; set; }
        public string Input { get; set; }
        public string Url { get; set; }
        public int? DoCheck { get; set; }
        public int? GetAllItems { get; set; }
        public List<string> ItemsToSelect { get; set; }
        public List<string> SelectedItems { get; set; }
        public int? HasSelectionSupport { get; set; }
        public int? TabCount { get; set; }
    }

    public class NodeHierarchy
    {
        public string CustomId { get; set; }
        public int? IsPresentInSelector { get; set; }
        public Selector SelectorInfo { get; set; }
        public Dictionary<string, string> OtherAttributes { get; set; }
    }

    public class Selector
    {
        public int? Index { get; set; }
        public string TagName { get; set; }
        //public Attribute Attributes { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
    }

    //public class Attribute
    //{
    //    public string Tag { get; set; }
    //    public string ParentId { get; set; }
    //    public string Src { get; set; }
    //}

    //public class HtmlMap
    //{
    //    public HtmlMap(string processName, string title)
    //    {
    //        InstanceAffinity = new Map("1");
    //        ProcessName = new Map(processName);
    //        Title = new Map(title);
    //    }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Map InstanceAffinity { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Map ProcessName { get; set; }
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public Map Title { get; set; }
    //    public Map AaName { get; set; }
    //}
    public class Map
    {
        public Map(string value)
        {
            FuzzyLevel = 1;
            IsActive = true;
            MatchingType = 1;
            Value = value;
        }
        public Map()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        //public bool CaseSensitive { get; set; } = true;
        /// <summary>
        /// 
        /// </summary>
        public int? FuzzyLevel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNull => string.IsNullOrEmpty(Value);

        /// <summary>
        /// 
        /// </summary>
        public int? MatchingType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }
    }
}
