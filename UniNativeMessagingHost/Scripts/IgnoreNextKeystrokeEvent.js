WrapInjectedFunction(
"OnIgnoreNextKeystrokeEvent",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function IgnoreNextKeystrokeEventResult(retCode) {
            var out_result =
            {
                retCode: retCode
            };
            return out_result;
        }
        // Input
        var uiEventFlagsWithKeyCode = message.uiEventFlagsWithKeyCode;
        // Output: none other than the return code

        try {
            //gcontext.TraceMessage("OnIgnoreNextKeystrokeEvent: enter");

            //Prevent the keyboard event listener from processing the keystroke specified by 'uiEventFlagsWithKeyCode'.
            IgnoreNextKeystroke(uiEventFlagsWithKeyCode);
        }
        catch (e) {
            gcontext.TraceError("IgnoreNextKeystrokeEvent exception: " + e);
            return sendResponse(IgnoreNextKeystrokeEventResult(0));
        }

        sendResponse(IgnoreNextKeystrokeEventResult(N_TRUE));
    }
); 