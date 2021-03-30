WrapInjectedFunction(
"OnGetHtmlAttrValuesList",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Input
        var customId = message.customId;
        // Output
        var out_htmlAttributes = {};

        try {
            gcontext.TraceMessage("OnGetHtmlAttrValuesList: enter");

            var targetElement = null;
            if (customId.length > 0) {
                targetElement = gcontext.g_customIdCache[customId];
                if (targetElement == null) {
                    gcontext.TraceMessage("OnGetHtmlAttrValuesList: customId = " + customId + " not found in the cache");
                    return sendResponse({ htmlAttributes: {}, retCode: N_FALSE });
                }
            }

            out_htmlAttributes = gcontext.GetAttrValueListForElement(targetElement);
        }
        catch (e) {
            gcontext.TraceMessage("OnGetHtmlAttrValuesList exception: " + e);
            return sendResponse({ htmlAttributes: {}, retCode: N_FALSE });
        }

        sendResponse({ htmlAttributes: out_htmlAttributes, retCode: N_TRUE });
    }
); 