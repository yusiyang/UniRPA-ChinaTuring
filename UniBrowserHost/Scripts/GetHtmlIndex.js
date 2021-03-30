WrapInjectedFunction(
"OnGetHtmlIndex",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type
        function GetHtmlIndexResult(index, retCode) {
            var out_result =
            {
                index: index,
                retCode: retCode
            };
            return out_result;
        }
        // Input
        var parentId = message.parentId;
        var customId = message.customId;
        var attrMap = gcontext.AttributeValueMap(message.attrMap);
        // Output
        var out_index = -1;

        // Browser dependent
        var rootDocument = document;

        try {
            gcontext.TraceMessage("OnGetHtmlIndex: enter");

            //Search the target element in the custom id cache.
            var targetElement = gcontext.g_customIdCache[customId];
            if (targetElement == null) {
                gcontext.TraceMessage("OnGetHtmlIdInfo: customId=" + customId + " not found in the cache");
                return sendResponse(GetHtmlIndexResult(out_index, N_FALSE));
            }

            var getAllDocuments = gcontext.xbrowser.isModeSingleFrame(); // in single frame mode, get all reachable documents
            var htmlCollection = gcontext.GetHtmlCollectionForParentId(rootDocument, getAllDocuments, parentId, targetElement.tagName.toLowerCase());
            if (htmlCollection == null) {
                gcontext.TraceMessage("OnGetHtmlIndex: GetHtmlCollectionForParentId failed");
                return sendResponse(GetHtmlIndexResult(out_index, N_FALSE));
            }

            //Calculate the index of this element.
            out_index = gcontext.GetIndexForAttributeList(htmlCollection, customId, attrMap);
        }
        catch (e) {
            //alert("GetHtmlIndex exception: "+e);
            return sendResponse(GetHtmlIndexResult(out_index, 0));
        }

        sendResponse(GetHtmlIndexResult(out_index, N_TRUE));
    }
); 