WrapInjectedFunction(
"OnClickHtmlElem",
	function (message, sender, sendResponse) {
		var gcontext = message.gcontext;
		// Result type.
		function ClickHtmlElemResult(retCode) {
			var out_result =
			{
				retCode: retCode
			};
			return out_result;
		}
		// Input
		var customId = message.customId;
		var flags = message.flags;

		var windowLeft = message.windowLeft;
		var windowTop = message.windowTop;

		var offsetX = message.x;
		var offsetY = message.y;

		// Output: none other than the return code

		//Browser dependent.
		var htmlWindow = window;
		var rootDocument = document;

		try {
			gcontext.TraceMessage("OnClickHtmlElem: enter");

			//Search the target element in the custom id cache.
			var targetElement = gcontext.g_customIdCache[customId];
			if (targetElement == null) {
				gcontext.TraceMessage("OnClickHtmlElem: customId=" + customId + " not found in the cache");
				return sendResponse(ClickHtmlElemResult(N_FALSE));
			}

			var button = (flags & gcontext.UIE_CF_BUTTON_MASK);
			var htmlButton = (button & gcontext.UIE_CF_RIGHT ? gcontext.HTML_RIGHT_BUTTON :
				button & gcontext.UIE_CF_MIDDLE ? gcontext.HTML_MIDDLE_BUTTON :
					gcontext.HTML_LEFT_BUTTON);

			var clickFlags = (flags & gcontext.UIE_CF_CLICK_MASK);
			var clickType = (clickFlags & gcontext.UIE_CF_DOUBLE ? gcontext.HTML_CLICK_DOUBLE :
				clickFlags & gcontext.UIE_CF_HOVER ? gcontext.HTML_CLICK_HOVERONLY :
					gcontext.HTML_CLICK_SINGLE);

			if (message.x === -1 || message.y === -1) {
				gcontext.TraceMessage("OnClickHtmlElem: calculating the click coordinates");

				// Invalid coordinates means that we have to calculate them.
				offsetX = Math.floor(gcontext.CLICK_OFFSET_X * window.devicePixelRatio);
				offsetY = Math.floor(gcontext.CLICK_OFFSET_Y * window.devicePixelRatio);

				//Make sure that these coordinates are interpreted in client space.
				flags &= ~gcontext.UIE_CF_SCREEN_COORDS;
			}

			// evtScreenX and evtScreenX are screen coordinates, in pixels
			// evtClientX and evtClientY are CSS coordinates, in CSS units ( pixels / devicePixelRatio )

			var evtScreenX = 0;
			var evtScreenY = 0;
			var evtClientX = 0;
			var evtClientY = 0;
			if ((flags & gcontext.UIE_CF_SCREEN_COORDS) !== 0) {
				gcontext.TraceMessage("OnClickHtmlElem: using SCREEN_COORDS");

				evtScreenX = offsetX;
				evtScreenY = offsetY;
				evtClientX = Math.floor((offsetX - windowLeft) / window.devicePixelRatio);
				evtClientY = Math.floor((offsetY - windowTop) / window.devicePixelRatio);
			}
			else {
				gcontext.TraceMessage("OnClickHtmlElem: using CLIENT_COORDS");

				// (x,y) are relative to the target Element top-left corner, in pixels
				var cssRect = gcontext.GetElementClientBoundingCssRectangle(rootDocument, targetElement);
				var clientRect = gcontext.CssToClientRect(cssRect, window.devicePixelRatio);

				gcontext.TraceMessage("OnClickHtmlElem: cssRect: " + gcontext.RectToString(cssRect));
				gcontext.TraceMessage("OnClickHtmlElem: clientRect: " + gcontext.RectToString(clientRect));

				evtScreenX = windowLeft + clientRect.left + offsetX;
				evtScreenY = windowTop + clientRect.top + offsetY;

				var cssOffsetX = offsetX / window.devicePixelRatio;
				var cssOffsetY = offsetY / window.devicePixelRatio;
				evtClientX = Math.floor(cssRect.left + cssOffsetX);
				evtClientY = Math.floor(cssRect.top + cssOffsetY);
			}

			gcontext.TraceMessage("OnClickHtmlElem: ( offsetX: " + offsetX + ", offsetY: " + offsetY + " )");
			gcontext.TraceMessage("OnClickHtmlElem: ( evtScreenX: " + evtScreenX + ", evtScreenY: " + evtScreenY + " )");
			gcontext.TraceMessage("OnClickHtmlElem: ( evtClientX: " + evtClientX + ", evtClientY: " + evtClientY + " )");
			gcontext.TraceMessage("OnClickHtmlElem: ( windowLeft: " + windowLeft + ", windowTop: " + windowTop + " )");
			gcontext.TraceMessage("OnClickHtmlElem: window.devicePixelRatio: " + window.devicePixelRatio);

			var ctrlOn = ((flags & gcontext.UIE_CF_MOD_CTRL) !== 0);
			var altOn = ((flags & gcontext.UIE_CF_MOD_ALT) !== 0);
			var shiftOn = ((flags & gcontext.UIE_CF_MOD_SHIFT) !== 0);

			if (gcontext.RaiseClickEvent(targetElement, htmlWindow, htmlButton, clickType,
				evtScreenX, evtScreenY, evtClientX, evtClientY,
				ctrlOn, altOn, shiftOn) === false) {
				gcontext.TraceMessage("OnClickHtmlElem: RaiseClickEvent failed");
				return sendResponse(ClickHtmlElemResult(N_FALSE));
			}
		}
		catch (e) {
			gcontext.TraceMessage("ClickHtmlElem exception: " + e);
			return sendResponse(ClickHtmlElemResult(0));
		}

		gcontext.TraceMessage("OnClickHtmlElem: return");
		sendResponse(ClickHtmlElemResult(N_TRUE));
	}
); 