chrome.extension.onMessage.addListener(
/*
 * OnProxyMessage is used to forward the call to the implementation scripts (e.g. GetHtmlValue, ClickHtmlElement etc).
*/
function OnProxyMessage(message, sender, sendResponse) {
    // Remove this function from the list of listeners.
    chrome.extension.onMessage.removeListener(OnProxyMessage);

    var gcontext = g_globalContext; // contains "global" stuff such as gcontext.TraceMessage, gcontext.g_customIdCache etc.
    var logfunc = gcontext.TraceError ? gcontext.TraceError : console.log;

    //logfunc("---------------------");
    //logfunc("Received " + g_functionNameThatWillBeCalled + " proxy message: // " + JSON.stringify(message) + " //");

    if (typeof g_functionThatWillBeCalled !== "function") {
        logfunc("g_functionThatWillBeCalled doesn't contain a function");
        sendResponse({});
        return;
    }

    message.gcontext = gcontext;
    //logfunc("Proxy call: message: " + JSON.stringify(message));
    g_functionThatWillBeCalled(message, sender, function (json) {
        //logfunc("After calling the function '" + g_functionNameThatWillBeCalled + "' with // " + JSON.stringify(message) + " //");
        //logfunc("Function returned // " + JSON.stringify(json) + " //");

        g_functionNameThatWillBeCalled = "";
        g_functionThatWillBeCalled = null;

        sendResponse(json);
    });
}
);
