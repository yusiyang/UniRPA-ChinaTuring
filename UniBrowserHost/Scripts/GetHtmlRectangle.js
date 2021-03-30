WrapInjectedFunction(
"OnGetHtmlRectangle",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function GetHtmlRectangleResult(left, top, right, bottom, retCode) {
            var out_result =
            {
                left: left,
                top: top,
                right: right,
                bottom: bottom,
                retCode: retCode
            };
            return out_result;
        }

        // Output
        var out_left = 0;
        var out_top = 0;
        var out_right = 0;
        var out_bottom = 0;

        var defaultResult = GetHtmlRectangleResult(0, 0, 0, 0, N_FALSE);

        try {
            //gcontext.TraceMessage("OnGetHtmlRectangle: enter");

            var cssRectangle;
            var cssToScreenParams;

            // read call forwarding info
            var coFwd = gcontext.COFwdContent();
            var coFwdInfo = coFwd.GetFwdRequestInfo(message);
            if (coFwdInfo != null) {
                // this is a forwarded call, get source element
                var srcElement = coFwd.GetSrcChildElementFromRequest(coFwdInfo);
                if (srcElement === null) {
                    gcontext.TraceError("OnGetHtmlRectangle: fowarded call, connot identify source child element");
                    return sendResponse(defaultResult);
                }
                var srcRectangle = gcontext.GetElementClientInnerCssRectangle(srcElement);
                gcontext.TraceMessage("OnGetHtmlRectangle: fowarded call, source element rectangle=" + gcontext.RectToString(srcRectangle));

                // get result rectangle from the previous run, relative to the source element top-left corner
                var resRect = coFwdInfo.customData.cssRectangle;
                cssRectangle = gcontext.UiRect(resRect.left, resRect.top, resRect.right - resRect.left, resRect.bottom - resRect.top);

                // shift result rectangle, relative to this frame top-left corner
                cssRectangle = cssRectangle.Offset(srcRectangle.left, srcRectangle.top);

                // get css-to-screen conversion params
                cssToScreenParams = coFwdInfo.customData;
            }
            else {
                // this is the initial call, get target element and its bounding rectangle
                var customId = message.customId;

                if (customId === "") {
                    //Empty custom id, get the rectangle of the whole frame
                    cssRectangle = gcontext.UiRect(0, 0, window.innerWidth, window.innerHeight);
                }
                else {
                    //Valid custom id, look it up.
                    var targetElement = gcontext.g_customIdCache[customId];
                    if (targetElement == null) {
                        gcontext.TraceMessage("OnGetHtmlRectangle: customId=" + customId + " not found in the cache");
                        return sendResponse(defaultResult);
                    }

                    cssRectangle = gcontext.GetElementClientBoundingCssRectangle(document, targetElement);
                }

                // get css-to-screen conversion params
                cssToScreenParams = message;
            }

            // if this is not the top frame, the result rectangle is relative to the frame top-left corner
            if (!gcontext.IsMainFrame(window)) {
                // forward call to parent to shift the rectangle with the frame top-left corner position
                cssToScreenParams.cssRectangle = {
                    left: cssRectangle.left,
                    top: cssRectangle.top,
                    right: cssRectangle.right,
                    bottom: cssRectangle.bottom
                };

                gcontext.TraceMessage("OnGetHtmlRectangle: the css rectangle is relative to a frame position. Forward call to parent frame.");

                var coFwdRequest = {};
                coFwd.AppendFwdRequestToParentFrame(
                    message.frameId, cssToScreenParams, defaultResult, coFwdRequest);

                return sendResponse(coFwdRequest);
            }

            var finalRectangle = gcontext.CssToScreenRect(cssRectangle, cssToScreenParams, cssToScreenParams.useClientCoordinates);

            out_left = Math.ceil(finalRectangle.left);
            out_top = Math.ceil(finalRectangle.top);
            out_right = Math.ceil(finalRectangle.right);
            out_bottom = Math.ceil(finalRectangle.bottom);

            gcontext.TraceMessage("OnGetHtmlRectangle: final rectangle = [" + out_left + ", " + out_top + ", " + out_right + ", " + out_bottom + "]");
        }
        catch (e) {
            gcontext.TraceError("GetHtmlRectangle exception: " + e);
            return sendResponse(GetHtmlRectangleResult(0, 0, 0, 0, N_FALSE));
        }

        sendResponse(GetHtmlRectangleResult(out_left, out_top, out_right, out_bottom, N_TRUE));

        //gcontext.TraceMessage("OnGetHtmlRectangle: return");
    }
); 