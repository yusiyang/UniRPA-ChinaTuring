WrapInjectedFunction(
"OnGetHtmlFromPoint",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function GetHtmlFromPointResult(customId, retCode) {
            var out_result =
            {
                customId: customId,
                retCode: retCode
            };
            return out_result;
        }

        // get the element at the selection point by exploring frame content documents
        function SingleFrameGetElementFromPoint(frameElement, cssPos) {
            var out_htmlElement = frameElement; // if it's not possible to explore frame contents, return the frame itself
            gcontext.TraceMessage("OnGetHtmlFromPoint: Single Frame Mode: enter");

            while (gcontext.ElementIsFrame(out_htmlElement)) {
                // try to obtain the frame content document
                var frameDoc = gcontext.GetSafeContentDocument(out_htmlElement);
                if (!frameDoc) {
                    gcontext.TraceMessage("OnGetHtmlFromPoint: Single Frame Mode: cannot get frame content document, return frame element");
                    break;
                }

                // get enclosing rectangle for frame content document
                var frameRectangle = gcontext.GetElementClientInnerCssRectangle(out_htmlElement);
                if (!frameRectangle.Contains(cssPos)) {
                    gcontext.TraceMessage("OnGetHtmlFromPoint: Single Frame Mode: the frame content rectangle doesn't contain the selection point, return frame element");
                    break;
                }

                // update selection coordinates according to frame content rectangle
                cssPos.x = cssPos.x - frameRectangle.left;
                cssPos.y = cssPos.y - frameRectangle.top;

                var crt_htmlElement = frameDoc.elementFromPoint(cssPos.x, cssPos.y);
                if (!crt_htmlElement) {
                    gcontext.TraceMessage("OnGetHtmlFromPoint: Single Frame Mode: the frame content rectangle doesn't contain the selection point, return frame element");
                    break;
                }

                // all ok, update result element
                out_htmlElement = crt_htmlElement;
            }

            gcontext.TraceMessage("OnGetHtmlFromPoint: Single Frame Mode: exit");
            return out_htmlElement;
        }

        // Output
        var out_customId = "";

        try {
            gcontext.TraceMessage("OnGetHtmlFromPoint: enter");

            // read call forwarding info
            var coFwd = gcontext.COFwdContent();
            var coFwdInfo = coFwd.GetFwdRequestInfo(message);

            var cssPos;
            if (coFwdInfo != null) {
                // this is a forwarded call, read position from customData
                cssPos = coFwdInfo.customData;
                gcontext.TraceMessage("OnGetHtmlFromPoint: forwarded call, cssPos=(" + cssPos.x + ", " + cssPos.y + ")");
            }
            else {
                // this is the initial call, read posistion from inputParams
                var screenPos = { x: message.screenX, y: message.screenY };
                gcontext.TraceMessage("OnGetHtmlFromPoint: screenPos=(" + screenPos.x + ", " + screenPos.y + ")");

                cssPos = gcontext.ScreenToCssPos(screenPos, message);
                gcontext.TraceMessage("OnGetHtmlFromPoint: cssPos=(" + cssPos.x + ", " + cssPos.y + ")");
            }

            // get the element at the specified CSS position.
            var foundElem = document.elementFromPoint(cssPos.x, cssPos.y);
            if (foundElem == null) {
                gcontext.TraceMessage("OnGetHtmlFromPoint: element not found at point");
                return sendResponse(GetHtmlFromPointResult(out_customId, N_FALSE));
            }

            // get custom ID for this element.
            out_customId = gcontext.GenerateCustomIdForElement(foundElem);
            gcontext.TraceMessage("OnGetHtmlFromPoint: found element, out_customId=" + out_customId);

            // if the element is a frame, run detection into that frame to locate the precise element
            if (gcontext.ElementIsFrame(foundElem)) {
                if (gcontext.xbrowser.isModeSingleFrame()) {
                    // single frame mode, cross-origin forwarding is not possible
                    foundElem = SingleFrameGetElementFromPoint(foundElem, cssPos);
                    if (foundElem) {
                        out_customId = gcontext.GenerateCustomIdForElement(foundElem);
                    }

                    return sendResponse(GetHtmlFromPointResult(out_customId, N_TRUE));
                }

                var rcClient = gcontext.GetElementClientInnerCssRectangle(foundElem);

                if (rcClient.Contains(cssPos)) {
                    // update CSS position relative to the frame's top-left corner
                    var customData = {
                        x: cssPos.x - rcClient.left,
                        y: cssPos.y - rcClient.top
                    };

                    var errorResult = GetHtmlFromPointResult(out_customId, N_TRUE);
                    var coFwdRequest = {};

                    var success = coFwd.AppendFwdRequestToChildFrame(
                        message.frameId, foundElem, customData, errorResult, coFwdRequest);
                    if (success) {
                        gcontext.TraceMessage("OnGetHtmlFromPoint: return call forwarding request to child frame, cssPos: x=" + customData.x + " y=" + customData.y);
                        return sendResponse(coFwdRequest);
                    }
                }
            }
        }
        catch (e) {
            gcontext.TraceError("GetHtmlFromPoint exception: " + e);
            return sendResponse(GetHtmlFromPointResult(out_customId, 0));
        }

        sendResponse(GetHtmlFromPointResult(out_customId, N_TRUE));
    }
); 