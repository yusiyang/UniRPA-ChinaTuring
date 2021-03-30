WrapInjectedFunction(
"OnGetTextFromScreenRect",
	function (message, sender, sendResponse) {
		var gcontext = message.gcontext;
		// determine if a HTML Node has visible text content
		function NodeTextIsVisible(node, nodeRect) {
			var nodeTag = node.tagName.toLowerCase();

			// HTML tags without visible content, like "head" (only metadata), "script" (code) or "style",
			// usually have the bounding rectangle=[0,0,0,0] because they don't cover any part of the webpage
			if ((nodeRect.width <= 0 && nodeRect.height <= 0) &&
				(nodeTag === "head" || nodeTag === "script" || nodeTag === "style")) {
				return N_FALSE;
			}

			return N_TRUE;
		}

		// Result type.
		function GetTextFromScreenRectResult(text, textRectInfo, retCode) {
			var out_result =
			{
				text: text,
				words: [],
				wordRects: [],
				retCode: retCode
			};
			for (var i = 0; i < textRectInfo.length; ++i) {
				out_result.words.push(textRectInfo[i].text);
				out_result.wordRects.push(textRectInfo[i].rect.left);
				out_result.wordRects.push(textRectInfo[i].rect.top);
				out_result.wordRects.push(textRectInfo[i].rect.getWidth());
				out_result.wordRects.push(textRectInfo[i].rect.getHeight());
			}
			return out_result;
		}
		// Input
		var left = message.left;
		var top = message.top;
		var width = message.width;
		var height = message.height;
		var pageZoomFactor = message.pageZoomValue;
		var msTimeout = 5000;   // limit execution time because running a script for too long might make the webpage unresponsive
		// Output
		var out_text = "";
		var out_textRectInfo = [];

		// Browser dependent
		var htmlWindow = window;
		var rootDocument = document;
		var runStopwatch = new gcontext.Stopwatch();

		try {
			gcontext.TraceMessage("GetTextFromScreenRect: enter");

			if (width <= 0 || height <= 0)
				out_text = gcontext.DeleteForbiddenCharacters(rootDocument.body.textContent);
			else {
				//Convert the screen rectangle filter to a client CSS rectangle
				var screenFilterRect = gcontext.UiRect(message.left, message.top, message.width, message.height);
				var cssFilterRect = gcontext.ScreenToCssRect(screenFilterRect, message);
				//gcontext.TraceMessage("GetTextFromScreenRect: cssFilterRect = "+RectToString(cssFilterRect));

				//Get all the interesting HTML elements into an array
				var docs = gcontext.GetDocumentList(rootDocument);
				var i, j;

				runStopwatch.Start(msTimeout);
				gcontext.TraceMessage("GetTextFromScreenRect: Stopwatch started with timeout=" + msTimeout + "ms");

				// check for timeout once for a certain number of expensive operations
				// to limit the performance impact of timeout checks
				var timeoutCheckCounter = 0;

				for (i = 0; i < docs.length; ++i) {
					//gcontext.TraceMessage("GetTextFromScreenRect: iterating docs["+i+"]");

					var docElem = docs[i].documentElement;
					if (docElem) {
						var elemTree = [docElem];

						//Iterate through all the HTML element hierarchy and filter their rectangles
						while (elemTree.length > 0) {
							//gcontext.TraceMessage("GetTextFromScreenRect: elemTree.length="+elemTree.length);

							if (timeoutCheckCounter >= 50) {
								if (runStopwatch.TimeoutElapsed())
									break;
								timeoutCheckCounter = 0;
							}

							var crtElem = elemTree.shift();
							var crtElemRect = gcontext.GetElementClientBoundingCssRectangle(rootDocument, crtElem);//crtElem.getBoundingClientRect();
							//gcontext.TraceMessage("GetTextFromScreenRect: crtElem="+crtElem.nodeName);
							//gcontext.TraceMessage("GetTextFromScreenRect: crtElem.children.length="+crtElem.children.length);
							//If the current element is a leaf node and its rectangle intersects the filter rectangle,
							//add it to the array
							if (cssFilterRect.Intersects(crtElemRect) === true) {
								var hasChildElements = false;
								var textExtractor = gcontext.CreateTextExtractorObject(htmlWindow, rootDocument, crtElem, "");

								//Start a text capturing session.
								textExtractor.BeginAccumulateTextRectInfo();

								//Capture the text from the current element.
								var tagName = crtElem.tagName.toLowerCase();
								if (tagName === "select") {
									timeoutCheckCounter += 1;
									textExtractor.AccumulateTextRectInfo(gcontext.GetTextContentFromElement(crtElem));
								}
								else {
									//Process the text for the children.
									for (var childNode = crtElem.firstChild; childNode; childNode = childNode.nextSibling) {
										var tagName = crtElem.tagName.toLowerCase();
										if (childNode.nodeType === childNode.ELEMENT_NODE) {
											//Only child elements with visible content and a valid rectangle are considered valid children
											var childRect = childNode.getBoundingClientRect();
											if (childRect.width >= 0 && childRect.height >= 0 && NodeTextIsVisible(childNode, childRect)) {
												//The current element has visible child elements with valid CSS rectangles, push them into the tree array
												elemTree.push(childNode);
												hasChildElements = true;
												//gcontext.TraceMessage("GetTextFromScreenRect: childNode="+childNode.nodeName+"is added");
												timeoutCheckCounter += 1;
												textExtractor.AdvanceTextOffsets(childRect.width);
											}
										}
										else if (childNode.nodeType === childNode.TEXT_NODE) {
											var nodeText = childNode.textContent;
											if (nodeText != null && nodeText.length > 0) {
												gcontext.TraceMessage("GetTextFromScreenRect: pushing textNode [" + gcontext.TruncateStringUsingWildcard(nodeText, 20) + "]");
												//Get the text rectangles for this text node.
												timeoutCheckCounter += 1;
												textExtractor.AccumulateTextRectInfo(nodeText);
												//We already have some text added.
												hasChildElements = true;
											}
										}
									}
								}
								//If no text nodes were added, try adding the text for the current element.
								if (hasChildElements === false) {
									var elemText = gcontext.GetTextContentFromElement(crtElem);
									gcontext.TraceMessage("GetTextFromScreenRect: pushing elemNode [" + gcontext.TruncateStringUsingWildcard(elemText, 20) + "]");
									timeoutCheckCounter += 1;
									textExtractor.AccumulateTextRectInfo(elemText);
								}

								//Add this text result to the final result.
								var textExtractorResult = textExtractor.EndAccumulateTextRectInfo();
								out_textRectInfo = out_textRectInfo.concat(textExtractorResult);
							}
						}
					}
				}

				if (runStopwatch.TimeoutElapsed()) {
					gcontext.TraceMessage("GetTextFromScreenRect: timeout elapsed (" + msTimeout + "ms) while adding elements, return N_FALSE");
					return sendResponse(GetTextFromScreenRectResult("", [], N_FALSE));
				}

				if (out_textRectInfo.length === 0) {
					gcontext.TraceMessage("GetTextFromScreenRect: no intersecting elements, return N_FALSE");
					return sendResponse(GetTextFromScreenRectResult("", [], N_FALSE));
				}

				//Reformat the text cells which have no whitespace between them.
				var CMP_RECT_TOLERANCE = 3;
				var textFormattedTextInfo = gcontext.FormatTextRectInfoArray(out_textRectInfo, CMP_RECT_TOLERANCE, runStopwatch);

				if (runStopwatch.TimeoutElapsed()) {
					gcontext.TraceMessage("GetTextFromScreenRect: timeout elapsed (" + msTimeout + "ms) in FormatTextRectInfoArray, return N_FALSE");
					return sendResponse(GetTextFromScreenRectResult("", [], N_FALSE));
				}

				//Get the text from the filtered elements.
				out_textRectInfo = [];
				var filteredTextResult = textFormattedTextInfo.GetFilteredText(cssFilterRect);
				for (i = 0; i < filteredTextResult.textRectInfo.length; ++i) {
					var textInfoCss = filteredTextResult.textRectInfo[i];
					var textInfoRectScreen = gcontext.CssToScreenRect(textInfoCss.rect, message, /*useClientCoordinates:*/N_FALSE);
					var textInfoScreen = gcontext.TextRectInfo(textInfoCss.text, textInfoRectScreen, 0, false);

					gcontext.TraceMessage("GetTextFromScreenRect: adding word [" + textInfoScreen.text + "] rect " + textInfoScreen.rect.toString());

					out_textRectInfo.push(textInfoScreen)
				}

				out_text = filteredTextResult.text;

				//Exit with success				
			}
		}
		catch (e) {
			gcontext.TraceError("GetTextFromScreenRect exception: " + e);
			//throw e;
			return sendResponse(GetTextFromScreenRectResult("", [], N_FALSE));
		}

		sendResponse(GetTextFromScreenRectResult(out_text, out_textRectInfo, N_TRUE));
	}
); 