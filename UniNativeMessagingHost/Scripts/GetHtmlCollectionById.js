WrapInjectedFunction(
"OnGetHtmlCollectionById",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type
        function GetHtmlCollectionByIdResult(customIdList, positions, retCode) {
            var out_result =
            {
                customIdList: customIdList,
                positions: positions,
                retCode: retCode
            };
            return out_result;
        }
        // Input
        var inputParams;

        // Output
        var out_customIdList = [];
        var out_positions = [];

        try {
            var coFwd = gcontext.COFwdContent();
            var coFwdInfo = coFwd.GetFwdRequestInfo(message);
            if (coFwdInfo != null) {
                gcontext.TraceMessage("OnGetHtmlCollectionById: enter forwarded call from parent frame/iframe element");

                // get input params from customData
                inputParams = coFwdInfo.customData;
            }
            else {
                gcontext.TraceMessage("OnGetHtmlCollectionById: enter");

                // initial call, get input params from message
                inputParams = message;
            }

            var parentId = inputParams.parentId;
            var getAllDescendants = inputParams.getAllDescendants;
            var tagName = inputParams.tagName !== null ? inputParams.tagName.toLowerCase() : "";
            var attrMap = gcontext.AttributeValueMap(inputParams.attrMap);
            var maxElems = inputParams.maxElems;

            // if the parent element is a frame/iframe, get its descendants from the child frame
            if (parentId != null && parentId.length > 0) {
                var parentElem = gcontext.g_customIdCache[parentId];
                if (parentElem && gcontext.ElementIsFrame(parentElem)) {
                    gcontext.TraceMessage("OnGetHtmlCollectionById: parentId=" + parentId + " is a frame element; forward call to get its descendants");

                    message.parentId = "";
                    var defaultResult = GetHtmlCollectionByIdResult(out_customIdList, out_positions, N_TRUE);
                    var coFwdRequest = {};

                    var success = coFwd.AppendFwdRequestToChildFrame(
                        message.frameId, parentElem, message, defaultResult, coFwdRequest);
                    if (!success) {
                        gcontext.TraceError("OnGetHtmlCollectionById: Cannot create call forwarding request");
                        return sendResponse(defaultResult);
                    }

                    return sendResponse(coFwdRequest);
                }
            }

            var rootDocument = document;
            var cssSelector = gcontext.GetActiveValueString(attrMap["css-selector"]);

            var htmlCollection;
            gcontext.TraceMessage("OnGetHtmlCollectionById: getAllDescendants=" + getAllDescendants + " tagName=[" + tagName + "]" + " parentId=[" + parentId + "]");

            // If css-selector is provided then getAllDescendants is ignored.
            var getAllDocuments = gcontext.xbrowser.isModeSingleFrame(); // in single frame mode, get all reachable documents
            if ((getAllDescendants === N_TRUE) || cssSelector) {
                htmlCollection = gcontext.GetHtmlCollectionForParentId(rootDocument, getAllDocuments, parentId, tagName, cssSelector);
            }
            else {
                htmlCollection = gcontext.GetDirectChildrenCollectionForParentId(rootDocument, getAllDocuments, parentId, tagName);
            }

            if (htmlCollection == null) {
                gcontext.TraceMessage("OnGetHtmlCollectionById: GetHtmlCollectionForParentId failed");
                return sendResponse(GetHtmlCollectionByIdResult(out_customIdList, out_positions, N_TRUE));
            }

            if (cssSelector) {
                delete attrMap["css-selector"];
            }

            var elements = gcontext.FindElementCollectionUsingAttributes(htmlCollection, attrMap, maxElems);
            if (elements == null || elements.length === 0) {
                gcontext.TraceMessage("OnGetHtmlCollectionById: no element was found");
                return sendResponse(GetHtmlCollectionByIdResult(out_customIdList, out_positions, N_TRUE));
            }

            for (var i = 0; i < elements.length; ++i) {
                out_customIdList.push(gcontext.GenerateCustomIdForElement(elements[i]));

                // add element position in frame, as array [left, top, right, bottom]
                var cssRectangle = gcontext.GetElementClientBoundingCssRectangle(document, elements[i]);
                //if (cssRectangle.left < 0)
                //    debugger;

                var rcClient = gcontext.CssToClientRect(cssRectangle, window.devicePixelRatio);

                out_positions.push([rcClient.left, rcClient.top, rcClient.right, rcClient.bottom]);
            }
        }
        catch (e) {
            return sendResponse(GetHtmlCollectionByIdResult([], [], N_FALSE));
        }

        gcontext.TraceMessage("OnGetHtmlCollectionById: return out_customIdList=[" + gcontext.EnumObjectProps(out_customIdList, true) + "]");
        sendResponse(GetHtmlCollectionByIdResult(out_customIdList, out_positions, N_TRUE));
    }
); 