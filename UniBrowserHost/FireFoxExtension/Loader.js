// Set g_loadExternalCode bellow to false and use manifest.json_debug when developing/debuggin extension.
var g_loadExternalCode = true;
var g_codeMap          = null;
var g_isImplLoaded     = false;

// Disable old extension if we are in Chrome and have access to the management API
if (chrome && chrome.management && chrome.management.get && chrome.management.setEnabled) {
    try {
        var oldExtensionId = "---旧版插件Id";
        chrome.management.get(oldExtensionId, function (result) {
            if (chrome.runtime.lastError) {
                // failed to find old extension, that's ok
            }
            else if (result && result.enabled) {
                console.log("Found deprecated and enabled UniRpa extension");
                chrome.management.setEnabled(oldExtensionId, false, function () {
                    if (chrome.runtime.lastError) {
                        console.error("Disable deprecated UniRpa extension failed, msg: " + chrome.runtime.lastError.message);
                    }
                    else {
                        console.log("Disabled deprecated UniRpa extension");
                    }
                });           
            }
        });  
    } catch (e) {
        console.error("UniRpa Exception: " + e);
    }
}

// Edge has chrome object but only chrome.app so Object which has i18n method is real API object.
chrome = (chrome && chrome.i18n && chrome) || (browser && browser.i18n && browser);

// On first run after installation or update, refresh the opened browser pages
chrome.runtime.onInstalled.addListener(function (details) {
    try {
        if (details.reason == "install") {
            console.log("UniRpa extension first installed");
        } else if (details.reason == "update") {
            var thisVersion = chrome.runtime.getManifest().version;
            console.log("UniRpa extension updated from " + details.previousVersion + " to " + thisVersion + "!");
        }

        chrome.tabs.query({}, function (tabsList) {
            for (var i in tabsList) {
                if (!IsChromeTabUrl(tabsList[i].url)) {
                    chrome.tabs.reload(tabsList[i].id, {});
                }
            }
        });

    } catch (e) {
        console.error("UniRpa Exception: " + e);
        return;
    }    
});

// Requests from ContentLoader.js for the rest of the content code.
chrome.runtime.onMessage.addListener(function (msg, sender, fnResponse) 
{
    if (msg === "uniRpaContentScriptLoadRequest") 
    {
        if (g_codeMap) 
        {
            // code from native host available
            var responseParams = GetContentScriptInitParams(sender.tab);
            fnResponse(responseParams);
        }
        else 
        {
            fnResponse(null);
            console.log("uniRpaContentScriptLoadRequest: code not available");
        }
    }
});

function IsEdgeBrowser()
{
    var isEdgeBrowser = (typeof CSS !== 'undefined' && CSS.supports("(-ms-ime-align:auto)"));
    return isEdgeBrowser;
}

function IsFirefoxBrowser()
{
    var isFirefox = (window.mozInnerScreenX !== undefined);
    return isFirefox;
}

function IsChromeBrowser()
{
    var isChromeBrowser = true;
    if(IsFirefoxBrowser() || IsEdgeBrowser())
    {
        isChromeBrowser = false;
    }
    return isChromeBrowser;
}

// Create the message port that communicates with the Chrome Native Messaging application.
var g_nativeMsgComm = function () 
{
    var m_crtRequestId  = 0;
    var m_returnMap     = { };
    var nativeAppName="com.uni.native_message_host";
    var m_nativeMsgPort = chrome.runtime.connectNative(nativeAppName);
    
    var RegisterReturnCallback = function (returnFunc) 
    {
        ++m_crtRequestId;
        m_returnMap[m_crtRequestId] = returnFunc;
        return m_crtRequestId;
    }
    
    var HandleReturnCallback = function (returnId, params) 
    {
        var returnFunc = m_returnMap[returnId];
        if (returnFunc !== undefined) 
        {
            returnFunc(params);
            delete m_returnMap[returnId];
        }
    }

    var HandleProductVersionChange = function(message)
    {
        console.log("HandleProductVersionChange: begin");
        ChangeImplementation(function()
        {
            console.log("HandleProductVersionChange: finished");

            // Signal that the loading is done
            var response = { returnId: message.requestId, codeVersion: g_codeMap["version"] };
            m_nativeMsgPort.postMessage(response);  
        });
    }

    var HandleFunctionCall = function(message)
    {
        if (typeof g_functionCallMap === "undefined")
        {
            console.error("HandleFunctionCall could not handle message.functionCall: " + message.functionCall);
            return;
        }

        // This message is a function request from "ChromeNativeMsg.EXE" (the native msg host).
        // Call the JS function and return the data to the native msg host.

        //console.log("Function call " + EnumObjectProps(message, true));
        // console.log("Function call "+JSON.stringify(message));

        var requestId = message.requestId;
        var requestCodeVersion = message.codeVersion;

        var currentCodeVersion = g_codeMap["version"];
        if(currentCodeVersion !== requestCodeVersion)
        {
            console.error("HandleFunctionCall: mismatched code versions:" + 
                " currentCodeVersion: " + currentCodeVersion + ", requestCodeVersion: " + requestCodeVersion + 
                " message.functionCall: " + message.functionCall);
            return;
        }

        var requestedFunc = g_functionCallMap[message.functionCall];
        if (requestedFunc !== undefined) 
        {
            // Call the requested function and get the return data.
            requestedFunc(message, function (responseData) 
            {
                // Copy the request id to the return data.
                if (requestId !== undefined) 
                {
                    responseData.returnId = requestId;
                    responseData.codeVersion = requestCodeVersion;
                    // Post the return data back to the native messaging app.
                    //console.log("Returning call data " + EnumObjectProps(responseData, true));
                    // console.log("Returning call "+JSON.stringify(responseData));
                    m_nativeMsgPort.postMessage(responseData);
                }
            });
        }
        else
        {
            console.error("HandleFunctionCall: requested function is not available message.functionCall: " + message.functionCall);
        }
    }

    m_nativeMsgPort.onMessage.addListener(function (message) 
    {
        if (IsEdgeBrowser()) 
        {
            message = JSON.parse(message);
        }
        if(typeof message === "undefined")
        {
            console.error("m_nativeMsgPort.onMessage message param is undefined");
            return;
        }

        if (message.functionCall) 
        {
            if (message.functionCall === "OnProductVersionChanged") 
            {
                HandleProductVersionChange(message);
            }
            else  
            {
                HandleFunctionCall(message);
            }
        }
        else if (message.returnId) 
        {
            // This message contains return data from the native msg host, resulted from a previous "CallFunction".
            // Invoke the callback function associated with this return data.
            HandleReturnCallback(message.returnId, message);
        }
    });
    
    m_nativeMsgPort.onDisconnect.addListener(function (message) 
    {
        console.log("m_nativeMsgPort.onDisconnect: disconnected, message=" + JSON.stringify(message));
    });

    return {
        // This function sends a call request to the native msg host ("ChromeNativeMsg.EXE").
        // These are call requests which need running native code because the Chrome JS API cannot provide the needed functionality.
        // The data will be returned as a message in the "m_nativeMsgPort.onMessage" listener defined above and the associated
        // "returnFunc" callback will be invoked.
        CallFunction : function (functionName, inputParams, returnFunc)
        {
            if (!g_isImplLoaded) 
            {
                if((functionName !== "RegisterNewlyCreatedWindowId") &&
                   (functionName !== "UnregisterWindowId"))
                {
                    console.error("m_nativeMsgPort.CallFunction: implementation library is not loaded yet. functionName: " + functionName);
                    return;
                }
            }

            if (returnFunc !== undefined) 
            {
                inputParams.requestId = RegisterReturnCallback(returnFunc);
            }

            inputParams.functionCall = functionName;
            m_nativeMsgPort.postMessage(inputParams);
        }
    };
}();

function ChangeImplementation(onCompletedCb)
{
    console.log("ChangeImplementation");

    FinalizeImplementation(function()
    {
        InitializeImplementation(onCompletedCb);
    });
}

function InitializeImplementation(onCompletedCb)
{
    console.log("InitializeImplementation");

    g_isImplLoaded = true;
    LoadExtensionScripts(function()
    {
        PostContentInitialize();

        if(onCompletedCb)
        {
            onCompletedCb();
        }
    });
}

function FinalizeImplementation(onCompletedCb)
{
    console.log("FinalizeImplementation");
    
    var CallOnCompletedCb = function()
    {
        if(onCompletedCb)
        {
            onCompletedCb();
        }
    }

    if(!g_isImplLoaded)
    {
        CallOnCompletedCb();
        return;
    }

    var getInputParamsCb = function(tab)
    {
        return {
            type: "content_finalize", 
            tab: tab 
        };
    }

    // Notify content scripts to clean up
    SendMessageToAllTabs(getInputParamsCb, function()
    {
        console.log("FinalizeImplementation: finalized all tabs");

        FinalizeBackground();
        CallOnCompletedCb();
    });
}

function LoadExtensionScripts(onScriptsLoadedCb)
{
    console.log("LoadExtensionScripts");

    var CallOnCompletedCb = function()
    {
        if(onScriptsLoadedCb)
        {
            onScriptsLoadedCb();
        }
    }

    if (!g_loadExternalCode) 
    {
        CallOnCompletedCb();
        return;
    }

    // Ask Chrome Native Messaging for the rest of the background scripts.
    g_nativeMsgComm.CallFunction("LoadScripts", {}, function (response) 
    {
        console.log("LoadExtensionScripts response.version: " + response["version"]);

        g_codeMap = response;

        // Reload background scripts
        eval.call(window, g_codeMap["background"]);
        delete g_codeMap["background"]; // Not used anymore don't keep it in memory.

        InitializeBackground();

        var getInputParamsCb = function(tab)
        {
            return GetContentScriptInitParams(tab);
        }

        // Reload the content scripts in all the opened tabs
        SendMessageToAllTabs(getInputParamsCb, function()
        {
            console.log("LoadExtensionScripts: initialized all tabs");
            CallOnCompletedCb();
        });
    });
}

function SendMessageToAllTabs(getTabInputParamsCb, onCompletedCb)
{
    var OnAllTabMessagesSent = function()
    {
        if(onCompletedCb)
        {
            onCompletedCb();
        }
    }

    var queryParams = { url: "<all_urls>" };
    chrome.tabs.query(queryParams, function (tabsList) 
    {
        var tabsCount = tabsList ? tabsList.length : 0;
        if(tabsCount == 0)
        {
            console.log("SendMessageToAllTabs: no tabs open");
            OnAllTabMessagesSent();
        }
        else
        {
            var SendMessageToTab = function(tab)
            {
                if (IsChromeTabUrl(tab.url)) 
                {
                    console.log("SendMessageToAllTabs: skip chrome:// tab.id: " + tab.id);
                    OnTabMessageSent();
                }
                else
                {
                    console.log("SendMessageToAllTabs: sending msg to tab.id: " + tab.id);

                    var inputParams = getTabInputParamsCb(tab);
                    chrome.tabs.sendMessage(tab.id, inputParams, function (respone) 
                    {
                        if (chrome.runtime.lastError) 
                        {
                            console.error("SendMessageToAllTabs: sendMessage failed to tab.id:" + tab.id + ", tab.url: " + tab.url + ", msg: " + chrome.runtime.lastError.message);
                        }
                        else
                        {
                            console.log("SendMessageToAllTabs: reloaded scripts in tab.id: " + tab.id);
                        }

                        OnTabMessageSent();
                    }); 
                }
            }

            var OnTabMessageSent = function()
            {
                --tabsCount;
                if(tabsCount <= 0)
                {
                    console.log("SendMessageToAllTabs: received replies from all tabs");

                    OnAllTabMessagesSent();
                }
            }

            for (var i = 0; i < tabsList.length; ++i) 
            {
                SendMessageToTab(tabsList[i]);
            }
        }
    });
}

// ===== Background page interface hooks =====
function InitializeBackground()
{
    // Initialize the background script
    if (chrome.InitializeBackgroundImpl) 
    { 
        // hook for implementation dependent initialization
        chrome.InitializeBackgroundImpl();
        chrome.InitializeBackgroundImpl = null;
    }
    else 
    {
        console.error("InitializeBackground: chrome.InitializeBackgroundImpl undefined");
    }
}

function PostContentInitialize()
{
    if (chrome.PostContentInitializeImpl) 
    { 
        // hook for implementation dependent post-initialization
        chrome.PostContentInitializeImpl();
        chrome.PostContentInitializeImpl = null;
    }
    else 
    {
        console.error("PostContentInitialize: chrome.PostContentInitializeImpl undefined");
    }
}

function FinalizeBackground()
{
    // Cleanup the background script
    if (chrome.FinalizeBackgroundImpl) 
    { 
        // Hook for implementation dependent clean-up
        chrome.FinalizeBackgroundImpl();
        chrome.FinalizeBackgroundImpl = null;
    } 
    else 
    {
        console.error("FinalizeBackground: chrome.FinalizeBackgroundImpl is undefined");
    }
}

///////////////////////////////////////////////////////////////////////////////////////////////
function getFileCodeObject(scriptName) {
	if (!g_loadExternalCode) {
		// Debug mode.
		return { file : scriptName };
	}
	else {
		//console.log("getFileCodeObject request: " + scriptName);
		return { code: g_codeMap[scriptName] };
	}
}

function GetContentScriptInitParams(tab)
{
    var responseParams = 
    {
        type: "content_load",
        contentCode: g_codeMap["content"], 
        version: g_codeMap["version"],
        tab : tab
    };
    return responseParams;
}

function IsChromeTabUrl(url)
{
    if (url && url.indexOf("chrome://") === 0) 
    {
        return true;
    }
    return false;
}

// ===== Window monitoring functions =====
window.addEventListener("load", OnPageLoad, false);

function OnPageLoad(event) 
{
    window.removeEventListener("load", OnPageLoad, false);

    console.log("OnPageLoad: enter event.target=" + event.target);

    /////////////////////////////////////
    // Window id to HWND implementation

    chrome.windows.getAll({ populate: false }, function (allWindows) 
    {
        var numNormalWindows = 0;
        var lastNormalWindowId = 0;

        for (var i in allWindows) 
        {
            var crtWindow = allWindows[i];
            if (crtWindow.type === "normal" || crtWindow.type === "popup") 
            {
                if (++numNormalWindows === 2) 
                {
                    break;
                }

                lastNormalWindowId = crtWindow.id;
            }
        }

        // If there are more than 1 windows open,
        // then don't register any of them.
        // This is because we cannot reliably create
        // the mapping between the windowId and the
        // native HWND in that case. 
        if (numNormalWindows === 1) 
        {
            RegisterWindowId(lastNormalWindowId);
        }
        else 
        {
            console.error("UniRpa::OnPageLoad: too many opened browser windows on extension init. Windows count: " + allWindows.length + ". Please restart the browser and try again.");
        }
    });

    chrome.windows.onCreated.addListener(function (wnd) 
    {
        if (wnd.type === "normal" || wnd.type === "popup") 
        {
            RegisterWindowId(wnd.id);
        }
    });

    chrome.windows.onRemoved.addListener(function (windowId) 
    {
        UnregisterWindowId(windowId);
    });

    var RegisterWindowId = function(windowId)
    {
        console.log("RegisterWindowId windowId: " + windowId);

        g_nativeMsgComm.CallFunction("RegisterNewlyCreatedWindowId", { windowId: windowId });
    }

    var UnregisterWindowId = function(windowId)
    {
        console.log("UnregisterWindowId windowId: " + windowId);

        g_nativeMsgComm.CallFunction("UnregisterWindowId", { windowId: windowId });
    }
}
