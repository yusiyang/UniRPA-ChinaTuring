WrapInjectedFunction(
"OnIsHtmlElemValid",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function IsHtmlElemValidResult(retCode) {
            var out_result =
            {
                retCode: retCode
            };
            return out_result;
        }
        // Input
        var customId = message.customId;
        var text = message.text;
        // Output: none other than the return code

        try {
            //An empty custom ID refers to the browser tab. If this code is executed, the tab is valid.
            if (customId === "") {
                gcontext.TraceMessage("OnIsHtmlElemValid: customId refers to the tab, it's valid");
                return sendResponse(IsHtmlElemValidResult(N_TRUE));
            }

            //Search the target element in the custom id cache.
            var targetElement = gcontext.g_customIdCache[customId];
            if (targetElement == null) {
                gcontext.TraceMessage("OnIsHtmlElemValid: customId=" + customId + " not found in the cache");
                return sendResponse(IsHtmlElemValidResult(N_FALSE));
            }
        }
        catch (e) {
            gcontext.TraceMessage("IsHtmlElemValid exception: " + e);
            return sendResponse(IsHtmlElemValidResult(0));
        }

        sendResponse(IsHtmlElemValidResult(N_TRUE));
    }
); 