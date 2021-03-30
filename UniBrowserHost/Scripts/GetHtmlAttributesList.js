WrapInjectedFunction(
"OnGetHtmlAttributesList",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function GetHtmlAttributesListResult(htmlAttributes, retCode) {
            var out_result =
            {
                htmlAttributes: htmlAttributes,
                retCode: retCode
            };
            return out_result;
        }
        // Input
        var customId = message.customId;
        // Output
        var out_htmlAttributes = [];

        try {
            gcontext.TraceMessage("OnGetHtmlAttributesList: enter");
            //Search the target element in the custom id cache.
            var targetElement = gcontext.g_customIdCache[customId];
            if (targetElement == null) {
                gcontext.TraceMessage("OnGetHtmlAttributesList: customId=" + customId + " not found in the cache");
                return sendResponse(GetHtmlAttributesListResult(out_htmlAttributes, N_FALSE));
            }

            out_htmlAttributes = gcontext.GetAttributeListForElement(targetElement);
        }
        catch (e) {
            gcontext.TraceMessage("GetHtmlAttributesList exception: " + e);
            return sendResponse(GetHtmlAttributesListResult([], 0));
        }

        sendResponse(GetHtmlAttributesListResult(out_htmlAttributes, N_TRUE));
    }
); 