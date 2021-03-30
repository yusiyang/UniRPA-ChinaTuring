WrapInjectedFunction(
"OnGetHtmlValue",
	function (message, sender, sendResponse) {
		var gcontext = message.gcontext;
		// Result type.
		function GetHtmlValueResult(value, retCode) {
			var out_result =
			{
				value: value,
				retCode: retCode
			};
			return out_result;
		}
		// Input
		var customId = message.customId;
		var getFullText = message.getFullText;
		// Output
		var out_value = "";

		// Browser dependent
		var rootDocument = document;

		try {
			gcontext.TraceMessage("GetHtmlValue: enter");

			if (customId.length === 0)
				out_attrValue = gcontext.DeleteForbiddenCharacters(rootDocument.body.textContent);
			else {
				//Search the target element in the custom id cache.
				var targetElement = gcontext.g_customIdCache[customId];
				if (targetElement == null) {
					gcontext.TraceMessage("GetHtmlValue: customId=" + customId + " not found in the cache");
					return sendResponse(GetHtmlValueResult("", N_FALSE));
				}

				var tag = targetElement.tagName.toLowerCase();
				gcontext.TraceMessage("GetHtmlValue: tag=" + tag);

				if (tag === "input" || tag === "textarea") {
					out_value = (targetElement.value ? targetElement.value : "");
				}
				else {
					var allText = true;
					if (getFullText === N_FALSE) {
						var isComboOrSingleList = ((tag === "select") && (targetElement.size <= 1) || (targetElement.multiple === false));
						if (isComboOrSingleList) {
							// For combo or single selection list the value is the selected item.
							out_value = gcontext.GetTextFromSelection(targetElement);
							allText = false;
						}
					}

					if (allText) {
						var newNodes = gcontext.AddHiddenSpansForInputValues(targetElement.ownerDocument);

						if (tag === "select") {
							out_value = gcontext.GetFullTextFromSelect(targetElement);
						}
						else {
							//gcontext.TraceMessage("GetHtmlValue: targetElement.textContent=" + targetElement.textContent);
							out_value = targetElement.textContent;
						}

						for (var i = 0; i < newNodes.length; ++i) {
							newNodes[i].parentNode.removeChild(newNodes[i]);
						}
					}
				}
			}
		}
		catch (e) {
			gcontext.TraceMessage("GetHtmlValue exception: " + e);
			return sendResponse(GetHtmlValueResult("", N_FALSE));
		}

		sendResponse(GetHtmlValueResult(out_value, N_TRUE));
	}
); 