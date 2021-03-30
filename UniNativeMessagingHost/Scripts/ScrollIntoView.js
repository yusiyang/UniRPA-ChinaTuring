WrapInjectedFunction(
"OnScrollIntoView",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function ScrollIntoViewResult(retCode) {
            var out_result =
            {
                retCode: retCode
            };
            return out_result;
        }
        // Input
        var customId = message.customId;
        // Output: none

        try {
            gcontext.TraceMessage("OnScrollIntoView: enter");

            if (customId === "") {
                //Empty custom id, do nothing
                return sendResponse(ScrollIntoViewResult(N_TRUE));
            }

            //Search the target element in the custom id cache.
            var targetElement = gcontext.g_customIdCache[customId];
            if (targetElement == null) {
                gcontext.TraceMessage("OnScrollIntoView: customId=" + customId + " not found in the cache");
                return sendResponse(ScrollIntoViewResult(N_FALSE));
            }

            if (gcontext.xbrowser.isEdge()) {
                // This is equivalent to using
                // var scrollParams = {block: "start", inline: "nearest"};
                targetElement.scrollIntoView(true);

                //Try to center the element when scrollingIntoView for
                //1. Consistency with other browsers (Chrome and Firefox have this behavior)
                //2. Avoiding overlapping with fixed menus that will prevent hardware events (like clicks)
                var boundingRect = targetElement.getBoundingClientRect();
                window.scrollBy(0, boundingRect.top + (boundingRect.height - window.innerHeight) / 2);
            }
            else {
                var scrollParams = { block: "center", inline: "nearest" };
                targetElement.scrollIntoView(scrollParams);
            }
        }
        catch (e) {
            gcontext.TraceMessage("ScrollIntoView exception: " + e);
            return sendResponse(ScrollIntoViewResult(N_FALSE));
        }

        sendResponse(ScrollIntoViewResult(N_TRUE));
        gcontext.TraceMessage("OnScrollIntoView: leave");
    }
); 