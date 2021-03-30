WrapInjectedFunction(
    "OnCheckHtmlElem",
    function (message, sender, sendResponse)
    {
        var gcontext = message.gcontext;
		// Result type.
		function CheckHtmlElemResult(isCheckable, retCode)
		{
			var out_result =
			{
				isCheckable : isCheckable,
				retCode : retCode
			};
			return out_result;
		}
		// Input
		var customId = message.customId;
		var doCheck = message.doCheck;
		var windowLeft = message.windowLeft;
		var windowTop = message.windowTop;
		// Output
		var out_isCheckable = N_FALSE;
		
		// Browser dependent
		var htmlWindow = window;
		var rootDocument = document;

		try
		{
			gcontext.TraceMessage("OnCheckHtmlElem: enter");
			//Search the target element in the custom id cache.
			var targetElement = gcontext.g_customIdCache[customId];
			if(targetElement == null)
			{
				gcontext.TraceMessage("OnCheckHtmlElem: customId=" + customId + " not found in the cache");
				return sendResponse(CheckHtmlElemResult(out_isCheckable, N_FALSE));
			}
			
			var tagName = targetElement.tagName.toLowerCase();
			if(tagName === "label")
			{
				var forId = targetElement.htmlFor;
				if(forId != null && forId.length !== 0)
				{
					var labelTarget = targetElement.ownerDocument.getElementById(forID);
					if(labelTarget != null)
					{
						targetElement = labelTarget;
						tagName = targetElement.tagName.toLowerCase();
					}
				}
			}
			if(tagName === "input")
			{
				var type = targetElement.type.toLowerCase();
				if((type === "checkbox" || type === "radio") &&
					targetElement.checked != null)
				{
					out_isCheckable = N_TRUE;
					
					var isChecked = (targetElement.checked===true ? N_TRUE : N_FALSE);
					if(doCheck !== isChecked)
					{
						//Change the state of the element by clicking it.
                        var cssRect = gcontext.GetElementClientBoundingCssRectangle(rootDocument, targetElement);
                        var clientRect = gcontext.CssToClientRect(cssRect, window.devicePixelRatio);
                        var clientX = clientRect.left + gcontext.CLICK_OFFSET_X;
                        var clientY = clientRect.top + gcontext.CLICK_OFFSET_Y;
						var screenX = clientX + windowLeft;
						var screenY = clientY + windowTop;
                        if (gcontext.RaiseClickEvent(targetElement, htmlWindow, gcontext.HTML_LEFT_BUTTON, gcontext.HTML_CLICK_SINGLE, 
							screenX, screenY, clientX, clientY,
							false, false, false) === false)
						{
							gcontext.TraceMessage("OnCheckHtmlElem: ClickHtmlElementAt failed");
							return sendResponse(CheckHtmlElemResult(out_isCheckable, N_FALSE));
						}
					}
				}
			}
		}
		catch(e)
		{
			gcontext.TraceMessage("CheckHtmlElem exception: "+e);
			return sendResponse(CheckHtmlElemResult(N_FALSE, N_FALSE));
		}

		sendResponse(CheckHtmlElemResult(out_isCheckable, N_TRUE));
	}
);