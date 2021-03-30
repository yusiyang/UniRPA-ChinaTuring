WrapInjectedFunction(
"OnHtmlSelectedItems",
	function (message, sender, sendResponse) {
		var gcontext = message.gcontext;
		// Result type.
		function HtmlSelectedItemsResult(selectedItems, hasSelectionSupport, retCode) {
			var out_result =
			{
				selectedItems: selectedItems,
				hasSelectionSupport: hasSelectionSupport,
				retCode: retCode
			};
			return out_result;
		}
		// Input
		var customId = message.customId;
		var getAllItems = message.getAllItems;
		var itemsToSelect = message.itemsToSelect;
		// Output
		var out_selectedItems = [];
		var out_hasSelectionSupport = N_FALSE;

		// Browser dependent
		var htmlWindow = window;

		// Keep in sync with enum E_SELECT_ITEMS_ERR in aahook.h
		var SELECT_FAILURE = 0;
		var SELECT_SUCCESS = 1;
		var SELECT_NOT_SUPPORTED = 2;
		var SELECT_ITEMS_NOT_FOUND = 3;

		try {
			gcontext.TraceMessage("OnHtmlSelectedItems: enter");
			gcontext.TraceMessage("OnHtmlSelectedItems: itemsToSelect=" + gcontext.EnumObjectProps(itemsToSelect, true));
			//Search the target element in the custom id cache.
			var targetElement = gcontext.g_customIdCache[customId];
			if (targetElement == null) {
				gcontext.TraceMessage("OnHtmlSelectedItems: customId=" + customId + " not found in the cache");
				return sendResponse(HtmlSelectedItemsResult(out_selectedItems, out_hasSelectionSupport, SELECT_FAILURE));
			}

			var tagName = targetElement.tagName.toLowerCase();
			if (tagName !== "select") {
				//Return TRUE in this case, but specify that there is no selection support.
				gcontext.TraceMessage("OnHtmlSelectedItems: no selection support");
				return sendResponse(HtmlSelectedItemsResult(out_selectedItems, out_hasSelectionSupport, SELECT_NOT_SUPPORTED));
			}

			out_hasSelectionSupport = N_TRUE;

			if (itemsToSelect.length === 0) {
				// No items to select, this means that the function is called to get the current selection.
				out_selectedItems = gcontext.GetSelectedItems(targetElement, getAllItems);
			}
			else {
				//The items to select are specified, so select them.
				if (targetElement.multiple === true) {
					gcontext.SelectMultipleItems(targetElement, itemsToSelect);
				}
				else {
					if (gcontext.SelectSingleItem(targetElement, itemsToSelect[0]) === false) {
						//Return TRUE in this case, but specify that there is no selection support.
						gcontext.TraceMessage("OnHtmlSelectedItems: single selection item not found");
						return sendResponse(HtmlSelectedItemsResult(out_selectedItems, out_hasSelectionSupport, SELECT_ITEMS_NOT_FOUND));
					}

					//Notify all listeners that the selection has changed.
					gcontext.RaiseUiEvent(targetElement, "change", htmlWindow);
				}
			}
		}
		catch (e) {
			gcontext.TraceMessage("HtmlSelectedItems exception: " + e);
			return sendResponse(HtmlSelectedItemsResult([], 0, SELECT_FAILURE));
		}

		sendResponse(HtmlSelectedItemsResult(out_selectedItems, out_hasSelectionSupport, SELECT_SUCCESS));
	}
); 