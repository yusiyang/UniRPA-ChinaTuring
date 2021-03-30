WrapInjectedFunction(
"OnGetHtmlElemById",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function GetHtmlElemByIdResult(customId, isDocumentReady, retCode) {
            var out_result =
            {
                customId: customId,
                isDocumentReady: isDocumentReady,
                retCode: retCode
            };
            return out_result;
        }
        // Input
        var inputParams;

        // Output
        var out_customId = "";
        var out_isDocumentReady = N_FALSE;
        var defaultResult = GetHtmlElemByIdResult(out_customId, out_isDocumentReady, N_FALSE);

        try {
            var coFwd = gcontext.COFwdContent();
            var coFwdInfo = coFwd.GetFwdRequestInfo(message);
            if (coFwdInfo != null) {
                gcontext.TraceMessage("OnGetHtmlElemById: enter forwarded call from parent frame element");

                // get input params from customData
                inputParams = coFwdInfo.customData;
            }
            else {
                gcontext.TraceMessage("OnGetHtmlElemById: enter");

                // initial call, get input params from message
                inputParams = message;
            }

            var parentId = inputParams.parentId;
            var tagName = inputParams.tagName !== null ? inputParams.tagName.toLowerCase() : "";
            var index = inputParams.index;
            var attrMap = gcontext.AttributeValueMap(inputParams.attrMap);

            // if the parent element is a frame/iframe, search in the child frame DOM
            if (parentId != null && parentId.length > 0) {
                var parentElem = gcontext.g_customIdCache[parentId];
                if (parentElem && gcontext.ElementIsFrame(parentElem)) {
                    gcontext.TraceMessage("OnGetHtmlElemById: parentId=" + parentId + " is a frame element; forward call to search the target element");

                    message.parentId = "";	// set search scope to frame's document
                    var coFwdRequest = {};

                    var success = coFwd.AppendFwdRequestToChildFrame(
                        message.frameId, parentElem, message, defaultResult, coFwdRequest);
                    if (!success) {
                        gcontext.TraceError("OnGetHtmlElemById: Cannot create call forwarding request");
                        return sendResponse(defaultResult);
                    }

                    return sendResponse(coFwdRequest);
                }
            }
            var rootDocument = document;
            var cssSelector = gcontext.GetActiveValueString(attrMap["css-selector"]);

            var htmlCollection = gcontext.GetHtmlCollectionForParentId(rootDocument, true, parentId, tagName, cssSelector);
            if (htmlCollection == null) {
                gcontext.TraceMessage("OnGetHtmlElemById: GetHtmlCollectionForParentId failed");
                return sendResponse(defaultResult);
            }

            if (cssSelector) {
                delete attrMap["css-selector"];
            }

            var foundElem = gcontext.FindElementUsingAttributes(htmlCollection, index, attrMap);
            if (foundElem == null) {
                gcontext.TraceMessage("OnGetHtmlElemById: element not found in the DOM");
                return sendResponse(defaultResult);
            }

            //Calculate the custom ID of this element.
            out_customId = gcontext.GenerateCustomIdForElement(foundElem);
            if (foundElem.ownerDocument) {
                out_isDocumentReady = (foundElem.ownerDocument.readyState === "complete") ? N_TRUE : N_FALSE;
            }
        }
        catch (e) {
            gcontext.TraceMessage("GetHtmlElemById exception: " + e);
            return sendResponse(defaultResult);
        }

        sendResponse(GetHtmlElemByIdResult(out_customId, out_isDocumentReady, N_TRUE));

        gcontext.TraceMessage("OnGetHtmlElemById: return");
    }
); 