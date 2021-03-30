WrapInjectedFunction(
"OnGetHtmlIdInfo",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // The result type is defined in HtmlTools.
        // Input
        var parentId = message.parentId;
        var customId = message.customId;
        var computeIndex = message.computeIndex;

        // Output
        var out_result = gcontext.GetSelectorAttributeListResult("", {}, -1, N_FALSE);

        // Browser dependent
        var rootDocument = document;

        try {
            gcontext.TraceMessage("OnGetHtmlIdInfo: enter");

            //Search the target element in the custom id cache.
            var targetElement = gcontext.g_customIdCache[customId];
            if (targetElement == null) {
                gcontext.TraceMessage("OnGetHtmlIdInfo: customId=" + customId + " not found in the cache");
                return sendResponse(out_result);
            }

            out_result = gcontext.GetSelectorAttributeList(rootDocument, customId, parentId, targetElement.tagName.toLowerCase(), computeIndex)
        }
        catch (e) {
            gcontext.TraceMessage("GetHtmlIdInfo exception: " + e);
            return sendResponse(out_result);
        }

        sendResponse(out_result);
    }
); 