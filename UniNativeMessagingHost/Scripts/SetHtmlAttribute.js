WrapInjectedFunction(
"OnSetHtmlAttribute",
	function (message, sender, sendResponse) {
		var gcontext = message.gcontext;
		// Result type.
		function SetHtmlAttributeResult(retCode) {
			var out_result =
			{
				retCode: retCode
			};
			return out_result;
		}
		// Input
		var customId = message.customId;
		var attrName = message.attrName;
		var attrValue = message.attrValue;
		// Output: none

		// Browser dependent
		var rootDocument = document;

		try {
			gcontext.TraceMessage("OnSetHtmlAttribute: enter");

			var attrNameLower = attrName.toLowerCase();
			if (customId.length === 0) {
				if (attrNameLower === "cookie")
					rootDocument.cookie = attrValue;
				else {
					gcontext.TraceMessage("OnSetHtmlAttribute: empty customId and attrName=" + attrName + " is invalid");
					return sendResponse(SetHtmlAttributeResult(N_FALSE));
				}
			}
			else {
				//Search the target element in the custom id cache.
				var targetElement = gcontext.g_customIdCache[customId];
				if (targetElement == null) {
					gcontext.TraceMessage("OnSetHtmlAttribute: customId=" + customId + " not found in the cache");
					return sendResponse(SetHtmlAttributeResult(N_FALSE));
				}

				if (attrNameLower === "url")
					targetElement.ownerDocument.defaultView.location = attrValue;
				else if (attrNameLower === "cookie")
					targetElement.ownerDocument.cookie = attrValue;
				else if (attrNameLower === "innerhtml")
					targetElement.innerHTML = attrValue;
				else if (attrNameLower === "outerhtml")
					targetElement.outerHTML = attrValue;
				else if (attrNameLower === "outertext")
					targetElement.textContent = attrValue;
				else
					targetElement.setAttribute(attrName, attrValue);
			}
		}
		catch (e) {
			gcontext.TraceMessage("SetHtmlAttribute exception: " + e);
			return sendResponse(SetHtmlAttributeResult(0));
		}

		sendResponse(SetHtmlAttributeResult(N_TRUE));
	}
); 