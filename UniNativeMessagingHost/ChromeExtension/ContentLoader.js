var g_csLoaded = false;

window.chrome = window.chrome || window.browser;

// When code map is loaded for a new implementation, this listener to receive the new content code
chrome.runtime.onMessage.addListener(
    function OnBackgroundMessage(message, sender, sendResponse) 
    {
        if (message.type == "content_load") 
        {
            console.log("OnBackgroundMessage: content scripts loaded from new codemap");

            InitializeContent(message);
            sendResponse();
        } 
        else if (message.type == "content_finalize") 
        {
            FinalizeContent(message);
            sendResponse();
        }
    }
);

// For tabs opened after the code implementation was loaded:
// If content code not yet loaded then request it from back-ground page and then eval it.
if (!g_csLoaded) 
{
    chrome.runtime.sendMessage("uniRpaContentScriptLoadRequest", function (response) 
    {
        if (response) 
        {
            console.log("ContentLoader.js: content scripts loaded from existing codemap");
            InitializeContent(response);
        }
    });    
}

function InitializeContent(message)
{
    console.log("InitializeContent: message.version: " + message.version);

    try 
    {
        eval.call(window, message.contentCode);
        g_csLoaded = true;

        if (chrome.InitializeContentImpl) 
        {
            // hook for implementation dependent initialization
            chrome.InitializeContentImpl(message.tab);
            chrome.InitializeContentImpl = null;
        } 
        else 
        {
            console.error("InitializeContentImpl undefined");
        }
    }
    catch (ex) 
    {
        console.log("OnContentScriptLoadRequest failed to evaluate content script " + ex);
    }
}

function FinalizeContent(message)
{   
    console.log("FinalizeContent");

    if (chrome.FinalizeContentImpl) 
    { 
        // hook for implementation dependent clean up
        chrome.FinalizeContentImpl(message.tab);
        chrome.FinalizeContentImpl = null;
    } 
    else 
    {
        console.log("FinalizeContent: chrome.FinalizeContentImpl is undefined");
    }
}