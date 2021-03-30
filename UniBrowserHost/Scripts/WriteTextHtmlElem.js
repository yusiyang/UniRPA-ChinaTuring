WrapInjectedFunction(
"OnWriteTextHtmlElem",
    function OnWriteTextHtmlElem(message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function WriteTextHtmlElemResult(retCode) {
            var out_result =
            {
                retCode: retCode
            };
            return out_result;
        }
        // Input
        var customId = message.customId;
        var text = message.text;
        var append = message.append;
        // Output: none other than the return code

        //Browser dependent.
        var htmlWindow = window;

        try {
            //gcontext.TraceMessage("OnWriteTextHtmlElem: enter");

            //Search the target element in the custom id cache.
            var targetElement = gcontext.g_customIdCache[customId];
            if (targetElement == null) {
                gcontext.TraceMessage("OnWriteTextHtmlElem: customId=" + customId + " not found in the cache");
                return sendResponse(WriteTextHtmlElemResult(N_FALSE));
            }
            //Also modify the "value" attribute for text type elemnts.
            var tagName = targetElement.tagName.toLowerCase();
            var setValue = (tagName === "input" || tagName === "textarea");

            gcontext.SendKeysToHtmlElement(targetElement, htmlWindow, text, setValue, append);
        }
        catch (e) {
            gcontext.TraceError("WriteTextHtmlElem exception: " + e);
            return sendResponse(WriteTextHtmlElemResult(0));
        }

        sendResponse(WriteTextHtmlElemResult(N_TRUE));
    }
); 