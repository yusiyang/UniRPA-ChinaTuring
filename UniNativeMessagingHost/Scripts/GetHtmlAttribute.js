WrapInjectedFunction(
"OnGetHtmlAttribute",
	function (message, sender, sendResponse) {
		var gcontext = message.gcontext;
		// Result type.
		function GetHtmlAttributeResult(attrValue, retCode) {
			var out_result =
			{
				attrValue: attrValue,
				retCode: retCode
			};
			return out_result;
		}

		// special handling of "parentCustomId" attribute, which might require cross-origin forwarding
		var coFwd = gcontext.COFwdContent();
		var coFwdInfo = coFwd.GetFwdRequestInfo(message);
		if (coFwdInfo != null) {
			var srcElem = coFwd.GetSrcChildElementFromRequest(coFwdInfo);  // the element which contains the call forwarding source frame

			if (!srcElem) {
				gcontext.TraceError("OnGetHtmlAttribute: cannot identify the source child element of the call forwarding request");
				return sendResponse(GetHtmlAttributeResult("", N_FALSE));
			}
			var parentCustomId = gcontext.ComputeCustomIdWithTabAndFrame(
				message.tabId,
				message.frameId,
				gcontext.GetCustomIdForElement(srcElem));

			gcontext.TraceMessage("OnGetHtmlAttribute: return [parentCustomId]=\"" + parentCustomId + "\"");
			return sendResponse(GetHtmlAttributeResult(parentCustomId, N_TRUE));
		}

		// Input
		var customId = message.customId;
		var attrName = message.attrName.toLowerCase();
		// Output
		var out_attrValue = "";

		// Browser dependent
		var rootDocument = document;

		try {
			//gcontext.TraceMessage("OnGetHtmlAttribute: enter customId=[" + customId + "]");

			if (attrName === "pagetitle" || attrName === "title") {
				out_attrValue = rootDocument.title;
			}
			else if (customId.length === 0) {
				if (attrName === "url")
					out_attrValue = rootDocument.URL;
				else if (attrName === "htmlwindowname")
					out_attrValue = window.name;
				else if (attrName === "cookie")
					out_attrValue = rootDocument.cookie;
				else if (attrName === "innertext")
					out_attrValue = gcontext.DeleteForbiddenCharacters(rootDocument.body.textContent);
				else {
					gcontext.TraceMessage("OnGetHtmlAttribute: empty customId and attrName=" + attrName + " is invalid");
					return sendResponse(GetHtmlAttributeResult("", N_FALSE));
				}
			}
			else {
				//Search the target element in the custom id cache.
				var targetElement = gcontext.g_customIdCache[customId];
				if (targetElement == null) {
					gcontext.TraceMessage("OnGetHtmlAttribute: customId=" + customId + " not found in the cache");
					return sendResponse(GetHtmlAttributeResult("", N_FALSE));
				}

				//gcontext.TraceMessage("OnGetHtmlAttribute: tagName=" + targetElement.tagName);

				if (attrName.toLowerCase() === "parentcustomid") {
					// Special handling of te "parentcustomid" attribute.
					var parentCustomId = "";

					var parentElement = targetElement.parentElement;
					if (parentElement != null) {
						if (gcontext.IsMainFrame(window)) {
							// The root document belongs to the main frame.
							// Skip the root HTML element with the HTML tag.
							if (parentElement !== rootDocument.documentElement) {
								parentCustomId = gcontext.GenerateCustomIdForElement(parentElement);
							}
						}
						else {
							// The root document belongs to a frame/iframe.
							if (parentElement === rootDocument.documentElement) {
								// special handling of "parentCustomId": if the targetElement's parent is the document of a child frame,
								// request call forwarding to the parent frame, to return the customId of the frame/iframe HTML element
								gcontext.TraceMessage("OnGetHtmlAttribute: the parent element is a frame/iframe. Request call forwarding to parent frame");

								var defaultResult = GetHtmlAttributeResult("", N_FALSE);
								var coFwdRequest = {};
								coFwd.AppendFwdRequestToParentFrame(message.frameId, {}, defaultResult, coFwdRequest);

								return sendResponse(coFwdRequest);
							}

							// The parent element is not the HTML root. Cmpute its custom ID and return it.
							parentCustomId = gcontext.GenerateCustomIdForElement(parentElement);
						}
					}

					// Compute the final parent identifier using the tab id and frame id.
					out_attrValue = gcontext.ComputeCustomIdWithTabAndFrame(message.tabId, message.frameId, parentCustomId);
				}
				else {
					// The requested attribute is not "parentcustomid".
					// Get the attribute value using the internal attribute name map.
					out_attrValue = gcontext.GetAttributeValue(targetElement, attrName);
				}
			}
		}
		catch (e) {
			gcontext.TraceError("GetHtmlAttribute exception: " + e);
			return sendResponse(GetHtmlAttributeResult("", N_FALSE));
		}

		//gcontext.TraceMessage("OnGetHtmlAttribute: return [" + attrName + "] = [" + out_attrValue + "]");

		sendResponse(GetHtmlAttributeResult(out_attrValue, N_TRUE));
	}
); 