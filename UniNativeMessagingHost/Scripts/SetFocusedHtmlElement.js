WrapInjectedFunction(
"OnSetFocusedHtmlElement",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function SetFocusedHtmlElementResult(retCode) {
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
            gcontext.TraceMessage("OnSetFocusedHtmlElement: enter");

            //Search the target element in the custom id cache.
            var targetElement = gcontext.g_customIdCache[customId];
            if (targetElement == null) {
                gcontext.TraceMessage("OnSetFocusedHtmlElement: customId=" + customId + " not found in the cache");
                return sendResponse(SetFocusedHtmlElementResult(N_FALSE));
            }

            //Set the focus to the found element.
            targetElement.focus();
        }
        catch (e) {
            gcontext.TraceMessage("SetFocusedHtmlElement exception: " + e);
            return sendResponse(SetFocusedHtmlElementResult(0));
        }

        sendResponse(SetFocusedHtmlElementResult(N_TRUE));
    }
); 