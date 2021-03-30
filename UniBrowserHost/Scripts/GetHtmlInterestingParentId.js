WrapInjectedFunction(
"OnGetHtmlInterestingParentId",
	function (message, sender, sendResponse) {
		var gcontext = message.gcontext;
		// Result type.
		function GetHtmlParentCustomIdResult(parentCustomId, retCode) {
			var out_result =
			{
				parentCustomId: parentCustomId,
				retCode: retCode
			};
			return out_result;
		}

		// special handling of elements inside frames/iframes, which may require cross-origin forwarding
		var coFwd = gcontext.COFwdContent();
		var coFwdInfo = coFwd.GetFwdRequestInfo(message);
		if (coFwdInfo != null) {
			var srcElem = coFwd.GetSrcChildElementFromRequest(coFwdInfo);  // the element which contains the call forwarding source frame

			// the source element is the parent of the child frame which made the request
			if (!srcElem) {
				gcontext.TraceError("GetHtmlInterestingParentId: cannot identify the source child element of the call forwarding request");
				return sendResponse(GetHtmlParentCustomIdResult("", N_FALSE));
			}
			var parentCustomId = gcontext.GetCustomIdForElement(srcElem);

			gcontext.TraceMessage("GetHtmlInterestingParentId: return parentCustomId=\"" + parentCustomId + "\"");
			return sendResponse(GetHtmlParentCustomIdResult(parentCustomId, N_TRUE));
		}

		// Input
		var targetCustomId = message.customId;
		// Output
		var out_parentCustomId = "";

		try {
			gcontext.TraceMessage("GetHtmlInterestingParentId: enter targetCustomId=" + targetCustomId);

			var targetElem = gcontext.g_customIdCache[targetCustomId];
			if (targetElem == null) {
				gcontext.TraceMessage("GetHtmlInterestingParentId: element not found in the DOM");
				return sendResponse(GetHtmlParentCustomIdResult(out_parentCustomId, N_FALSE));
			}

			var parentElem = gcontext.GetNextInterestingParentForSelector(targetElem);

			if (parentElem == null) {
				if (!gcontext.IsMainFrame(window)) {
					// if the targetElement's parent is a document of a child frame, request call forwarding
					// to the parent frame, and return the customId of the HTML element which holds the document
					gcontext.TraceMessage("GetHtmlInterestingParentId: the parent element is a frame/iframe. Request call forwarding to parent frame");

					var defaultResult = GetHtmlParentCustomIdResult(out_parentCustomId, N_FALSE);
					var coFwdRequest = {};

					coFwd.AppendFwdRequestToParentFrame(
						message.frameId, {}, defaultResult, coFwdRequest);

					return sendResponse(coFwdRequest);
				}
			}
			else { //if(parentElem != null)
				out_parentCustomId = gcontext.GenerateCustomIdForElement(parentElem);
				gcontext.TraceMessage("GetHtmlInterestingParentId: out_parentCustomId=[" + out_parentCustomId + "]");
			}
		}
		catch (e) {
			gcontext.TraceMessage("GetHtmlParentCustomId exception: " + e);
			return sendResponse(GetHtmlParentCustomIdResult(out_parentCustomId, 0));
		}

		sendResponse(GetHtmlParentCustomIdResult(out_parentCustomId, N_TRUE));

		gcontext.TraceMessage("GetHtmlInterestingParentId: return");
	}
); 