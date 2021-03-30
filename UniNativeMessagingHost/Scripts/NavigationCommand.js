WrapInjectedFunction(
"OnNavigationCommand",
    function (message, sender, sendResponse) {
        var gcontext = message.gcontext;
        // Result type.
        function NavigationCommandResult(retCode) {
            var out_result =
            {
                retCode: retCode
            };
            return out_result;
        }
        // Input.
        var command = message.command;
        // Output: none other than the return code

        //Browser dependent.
        var htmlWindow = window;

        gcontext.TraceMessage("NavigationCommand command: " + command);

        try {
            //These are navigation commands that cannot be performed in the background page.
            if (command === "stop") {
                htmlWindow.stop();
            }
            else if (command === "forward") {
                htmlWindow.history.forward();
            }
            else if (command === "back") {
                htmlWindow.history.back();
            }
            else if (command === "reload") {
                htmlWindow.location.reload();
            }
            else {
                return sendResponse(NavigationCommandResult(0));
            }
        }
        catch (e) {
            gcontext.TraceMessage("NavigationCommand exception: " + e);
            return sendResponse(NavigationCommandResult(0));
        }

        sendResponse(NavigationCommandResult(N_TRUE));
    }
); 