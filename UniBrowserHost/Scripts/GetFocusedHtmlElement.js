WrapInjectedFunction(
"OnGetFocusedHtmlElement",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function GetFocusedHtmlElementResult(customId, retCode) {
            var out_result =
            {
                customId: customId,
                retCode: retCode
            };
            return out_result;
        }
        // Input: none
        // Output
        var out_customId = "";

        try {
            gcontext.TraceMessage("OnGetFocusedHtmlElement: enter");

            var foundElem = document.activeElement;
            if (foundElem == null) {
                gcontext.TraceMessage("OnGetFocusedHtmlElement: active element not found");
                return sendResponse(GetFocusedHtmlElementResult(out_customId, N_FALSE));
            }

            out_customId = gcontext.GenerateCustomIdForElement(foundElem);
            gcontext.TraceMessage("OnGetFocusedHtmlElement: out_customId=" + out_customId);

            // if the element is a frame, run again into that frame to locate the precise element
            if (gcontext.ElementIsFrame(foundElem)) {
                var errorResult = GetFocusedHtmlElementResult(out_customId, N_TRUE);
                var coFwdRequest = {};

                var coFwd = gcontext.COFwdContent();
                if (coFwd.AppendFwdRequestToChildFrame(
                    message.frameId, foundElem, {}, errorResult, coFwdRequest)) {

                    gcontext.TraceMessage("OnGetHtmlFromPoint: return call forwarding request to child frame");
                    return sendResponse(coFwdRequest);
                }
            }
        }
        catch (e) {
            gcontext.TraceMessage("GetFocusedHtmlElement exception: " + e);
            return sendResponse(GetFocusedHtmlElementResult("", 0));
        }

        sendResponse(GetFocusedHtmlElementResult(out_customId, N_TRUE));

        gcontext.TraceMessage("OnGetFocusedHtmlElement: return");
    }
); 