WrapInjectedFunction(
"OnInjectAndRunJs",
	function OnInjectAndRunJs(message, sender, sendResponse) {
		var gcontext = message.gcontext;
		var out_value = "";
		var rootDocument = document;
		var injectScriptErr = 0;

		try {
			gcontext.TraceMessage("InjectAndRunJs: enter");

			var targetElement = null;
			var customId = message.customId;
			var tabId = message.tabId;

			if (customId.length !== 0) {
				// Search the target element in the custom id cache.
				var targetElement = gcontext.g_customIdCache[customId];
				if (targetElement === null) {
					gcontext.TraceMessage("InjectAndRunJs: customId=" + customId + " not found in the cache");
					return sendResponse({ result: "", retCode: N_FALSE, isScriptErr: injectScriptErr });
				}
			}

			var wrapperCode = "var _clientFunc = " + message.jsCode + "; " +
				"var _r = ''; " +
				"try { _r =  _clientFunc(_targetElem, _inputText, _tabId); return { isErr : true, result : _r}; } catch (ex) { _r = ex.toString(); } return { isErr : false, result : _r};";

			try {
				var wrapperFunc = new Function("_targetElem", "_inputText", "_tabId", wrapperCode);
			}
			catch (ex) {
				// Injected script parse error.
				out_value = ex.toString();
				injectScriptErr = 1;
				sendResponse({ result: out_value, retCode: N_TRUE, isScriptErr: injectScriptErr });

				return;
			}

			gcontext.TraceMessage("InjectAndRunJs: Calling WrapperFunction: " + message.inputData + " and tabId=" + tabId);
			var response = wrapperFunc(targetElement, message.inputData, tabId);
			out_value = response.result;
			injectScriptErr = (response.isErr ? 1 : 0);
		}
		catch (e) {
			gcontext.TraceMessage("InjectAndRunJs exception: " + e);
			return sendResponse({ result: e.toString(), retCode: N_FALSE, isScriptErr: injectScriptErr });
		}

		sendResponse({ result: out_value, retCode: N_TRUE, isScriptErr: injectScriptErr });
	}
); 