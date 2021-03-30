WrapInjectedFunction(
    "OnGetHtmlDevicePixelRatio",
    function (message, sender, sendResponse)
    {
        var gcontext = message.gcontext;
		gcontext.TraceMessage("OnGetHtmlDevicePixelRatio window.devicePixelRatio: " + window.devicePixelRatio);
		
		var devicePixelRatioPercentage = Math.round(window.devicePixelRatio * 100);
		var out_result =
		{
			devicePixelRatioPercentage : devicePixelRatioPercentage,
			retCode : N_TRUE
		};
		sendResponse(out_result);
	}
);