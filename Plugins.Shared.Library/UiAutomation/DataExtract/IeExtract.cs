using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using MSHTML;
using Plugins.Shared.Library.UiAutomation.Browser;
using Plugins.Shared.Library.UiAutomation.IEBrowser;

namespace Plugins.Shared.Library.UiAutomation.DataExtract
{
    public class IeExtract : IDataExtract
    {
        public string GetColumnData(UiElement element, string extractOption)
        {
            var ele = element?.uiNode as IeNode;
            var jsCode = @"var getColumnData=function (topContainer, inputXml)
{
    var DOM_NodeType = { ""Element"": 1, ""Text"": 3 };

    var getSessionAttributeName = function (sessionID, attr) {
        return ""data-extract-session-"" + attr + ""-"" + sessionID;
    }

    var hasSessionAttribute = function (crntElem, sessionID, attr) {
        if (crntElem.nodeType !== DOM_NodeType.Element)//check if node is an element (may be a text node or something else. A mutation target element can also be a text node)
            return false;

        var attrNode = crntElem.getAttribute(getSessionAttributeName(sessionID, attr));

        if (attrNode !== null && attrNode !== """") {
            return true;
        }

        return false;
    }

    var setSessionAttribute = function (crntElem, sessionID, attr) {
        crntElem.setAttribute(getSessionAttributeName(sessionID, attr), ""true"");
    }

    var removeSessionAttribute = function (crntElem, sessionID, attr){
        crntElem.removeAttribute(getSessionAttributeName(sessionID, attr));
    }

	var createXmlDoc = function (xmlText) {
		if (typeof window.DOMParser !== ""undefined"") { // Chrome & FF.
			createXmlDoc = function (xmlText) {
				return (new window.DOMParser()).parseFromString(xmlText, ""text/xml"");
			};
		}
		else { // IE.
			createXmlDoc = function (xmlText) {
				var xmlDoc = new ActiveXObject(""Microsoft.XMLDOM"");
				xmlDoc.async = false;
				xmlDoc.loadXML(xmlText);
				if (xmlDoc.parseError.errorCode != 0) {
					throw xmlDoc.parseError.reason;
				}

				return xmlDoc;
			};
		}

		return createXmlDoc(xmlText);
	};
	
	var isSameNode = function (e1, e2) {
		if (typeof e1.isSameNode !== ""undefined"") { // Chrome.
			isSameNode = function (e1, e2) {
				return e1.isSameNode(e2);
			};
		}
		else if (typeof e1.sourceIndex !== ""undefined""){ // IE.
			isSameNode = function (e1, e2) {
				return (e1.sourceIndex === e2.sourceIndex);
			};
		}
		else { // FF.
			isSameNode = function (e1, e2) {
				return (e1 === e2);
			};
		}

		return isSameNode(e1, e2);
	};

	var getAllElemsByTag = function (c, t) {
		if ((typeof c.all !== ""undefined"") && (typeof c.all.tags !== ""undefined"")) { // IE.
			getAllElemsByTag = function (c, t) {
				return c.all.tags(t);
			};
		}
		else {
			getAllElemsByTag = function (c, t) {
				return c.getElementsByTagName(t);
			}
		}
		
		return getAllElemsByTag(c, t);
	};

	var getChildrenByTag = function (e, t) {
		if (typeof e.children.tags !== ""undefined"") { // IE.
			getChildrenByTag = function (e, t) {
				return e.children.tags(t);
			};
		}
		else {
			getChildrenByTag = function (e, t) {
				t = t.toLowerCase();
				var result = [];
				for (var c = 0; c < e.children.length; ++c) {
					if (e.children[c].tagName.toLowerCase() === t) {
						result.push(e.children[c]);
					}
				}

				return result;
			};
		}

		return getChildrenByTag(e, t);
	};

    // strip leading and trailing spaces from a string.
    var trim = function(text) {
        return text.replace(/^\s+|\s+$/g, '');
    };

	var getInnerText = function (e) {
		if (typeof e.innerText !== ""undefined"") {
			getInnerText = function (e) {
				return e.innerText;
			};
		}
		else {
			getInnerText = function (e) {
				return e.textContent;
			};
		}

		return getInnerText(e);
	};

    var isSameSession = false; // Set to true if we encounter at least one object extracted in the same session.
	var sessionIdAttr = """";
	
    // The character range for valid XML is defined here: https://en.wikipedia.org/wiki/Valid_characters_in_XML, see the range specified for 'Char'
    // WARNING: too painful to include supplementary planes, these characters (0x10000 and higher) 
    // will be stripped by this function. See what you are missing (heiroglyphics, emoji, etc) at:
    // http://en.wikipedia.org/wiki/Plane_(Unicode)#Supplementary_Multilingual_Plane
    // C01 and C1 remaining control characters are found at U+007F-U+0084 and U+0086-U+009F
    // Code points allowed but discouraged U+FDD0-U+FDEF (non characters)
	var NOT_SAFE_IN_XML_1_0 = /[^\x09\x0A\x0D\x20-\x7E\x85\xA0-\uD7FF\uE000-\uFDCF\uFDF0-\uFFFD]/gm;
	function sanitizeStringForXML(theString) {
	    return theString.replace(NOT_SAFE_IN_XML_1_0, '');
	}

	var extractTable = function (table, getColName, getEmptyCols) {
		// In memory table matrix structure.
		// It takes advantage that Javascript arrays can have holes (undefined value for some index).
		var createTableMatrix = function () {
			var matrix = [];
			var isSameSession = false;

			return {
				setSameSessionFlag : function (s) {
					isSameSession = s;
				},

				reset : function () {
					matrix = [];
					isSameSession = false;
				},

				setAt : function (i, j, v) {
					if (matrix[i] === undefined) {
						matrix[i] = [];
					}

					matrix[i][j] = v;
				},

				getAt : function (i, j) {
					if (matrix[i] && (matrix[i][j] !== undefined)) {
						return matrix[i][j];
					}

					return null;
				},

				// Transform the matrix into an XML string with the format:
				// <extract>
				//     <row>
				//         <column name=""colname-i"">scraped text value</column>
				//     </row>
				//     ....
				// </extract>
				toXml : function () {
					var xmlDoc   = createXmlDoc(""<extract></extract>"");
					var rootElem = xmlDoc.documentElement;
					var colNames = getColumnsName(table, getColName);

					for (var r = 0; r < matrix.length; ++r) {
						if (matrix[r]) {
							var rowXml = xmlDoc.createElement(""row"");
							for (var c = 0; c < matrix[r].length; ++c) {
							    var crntCellText = matrix[r][c];
							    if (crntCellText === null || crntCellText === undefined) {
							        crntCellText = """";
							    }

							    if (crntCellText || getEmptyCols) {
									var col = xmlDoc.createElement(""column"");
									col.setAttribute(""name"", colNames(c));
									col.appendChild(xmlDoc.createTextNode(crntCellText));
									rowXml.appendChild(col);
								}
							}

							rootElem.appendChild(rowXml);
						}
					}

					if (isSameSession) {
						rootElem.setAttribute(""same-session-encountered"", ""1"");
					}

					var result = xmlDoc.xml ? xmlDoc.xml : (new XMLSerializer()).serializeToString(xmlDoc);

					return sanitizeStringForXML(result);
				},

				// Concatenate all values for each column and return an aray that contains
				// on i-th position all values from i-th column concatenated.
				toColumnNames : function () {
					var columns = [];
					for (var r = 0; r < matrix.length; ++r) {
						if (matrix[r]) {
							for (var c = 0; c < matrix[r].length; ++c) {
								if (matrix[r][c]) {
									if (columns[c] === undefined) {
										columns[c] = matrix[r][c];
									}
									else {
										columns[c] += ("" "" + matrix[r][c]);
									}
								}
							}
						}
					}

					return columns;
				}
			};
		};

		// Extract header values from <th> html elements.
		var getColumnsName = function (t, gcn) {
			var columns = [];
			if (gcn) {
			    var headerMatrix = extractMatrix(t, ""th"", /*skipRowSpans:*/true, /*ignoreSessionId:*/true);
				columns = headerMatrix.toColumnNames();
			}
			
			return function (i) {
				if (columns[i]) {
					return columns[i];
				}
				else {
					return ""Column-"" + i;
				}
			};
		};

		// Iterate <td> or <th> elements (row by row) inside a table and
		// creates a matrix containing the text values of each cell.
		// Also expands <td> cells that have rowSpan and/or colSpan attributes set.
		// A cell text will be expanded to rowSpan x colSpan area.
		var extractMatrix = function (htmlTable, htmltag, skipRowSpans, ignoreSessionId) {
		    // Extract data from html table.
		    skipRowSpans = (skipRowSpans === true) ? skipRowSpans : false;
		    ignoreSessionId = (ignoreSessionId === true) ? ignoreSessionId : false;
			var tableMatrix = createTableMatrix();
			var rows = htmlTable.rows;

			if (rows) {
			    for (var r = 0; r < rows.length; ++r) {
					var cols = getAllElemsByTag(rows[r], htmltag);

					if (cols) {
						var tc = 0;
						for (var c = 0; c < cols.length; ++c) {
							var crntCell = cols[c];
                            if (!ignoreSessionId && (sessionIdAttr !== """"))
                            {
                                if (hasSessionAttribute(crntCell, sessionIdAttr, ""text""))
                                {
                                    // The same extraction session. Don't extract twice on the same page.
                                    tableMatrix.setSameSessionFlag(true);
                                    continue;
                                }

                                setSessionAttribute(crntCell, sessionIdAttr, ""text"");
							}

							// Skip columns perviously set by cells above that have rowSpan
							// and have been expanded down to next rows.
							while (tableMatrix.getAt(r, tc) !== null) {
								tc++;
							}

							// Process row/col span.
							var cs = 1;
							if (crntCell.colSpan) {
								var colSpan = parseInt(crntCell.colSpan);
								if (!isNaN(colSpan)) {
									cs = colSpan;
								}
							}

							var rs = 1;
							if (crntCell.rowSpan) {
								var rowSpan = parseInt(crntCell.rowSpan);
								if (!isNaN(rowSpan)) {
									rs = rowSpan;
								}
							}

							// The text will be expanded to a rowSpan x colSpan area to the right and down
							// starting from (r, tc) position to (r + rs, tc + cs).
							var crntCellTxt = trim(getInnerText(crntCell));
							for (var i = 0; i < cs; ++i) {
								for (var j = 0; j < rs; ++j) {
									tableMatrix.setAt(r + j, tc, ((skipRowSpans && j > 0) ? """" : crntCellTxt));
								}

								tc++;
							}
						}
					}
				}
			}

			return tableMatrix;
		};

		var cellMatrix = extractMatrix(table, ""td"");
		return cellMatrix.toXml();
	};

    // Extract data from XML.
    var inputData = function(xmlText) {
        var getPathList = function (node) {
            var pathRes = [];
            var path    = (typeof node.selectNodes !== ""undefined"" ? node.selectNodes(""./webctrl"") : node.getElementsByTagName(""webctrl""));

            if (!path.length) {
                throw ""Invalid path in XML."";
            }

            for (var w = 0; w < path.length; ++w) {
                var pathData = {};
                var crntCtrl = path.item(w);

                var tag = crntCtrl.getAttribute(""tag"");
                var cls = crntCtrl.getAttribute(""class"");
                var idx = crntCtrl.getAttribute(""idx"");
                var txt = crntCtrl.getAttribute(""text"");

                if (!tag) {
                    throw ""Invalid tag name in XML data."";
                }
                    
                pathData.tag = tag.toLowerCase();

                if (cls) {
                    pathData.cls = cls;
                }

                if (idx) {
                    pathData.index = parseInt(idx) - 1;
                    if (isNaN(pathData.index)) {
                        throw ""Invalid index in XML data."";
                    }
                }

                if (txt) {
                    pathData.text = txt;
                }

                pathRes.push(pathData);
            }

            return pathRes;
        };

        var isTextChange = function (mutation) {
            for (var i = 0; i < mutation.addedNodes.length; ++i) {
                if (mutation.addedNodes[i].nodeType === DOM_NodeType.Text) {
                    return true;
                }
            }
            for (var i = 0; i < mutation.removedNodes.length; ++i) {
                if (mutation.removedNodes[i].nodeType === DOM_NodeType.Text) {
                    return true;
                }
            }

            return false;
        }

        var removeSessionAttributeOnHierarchy = function (e, sessionID, attr) {
            if (!e)
                return;

            if (hasSessionAttribute(e, sessionID, attr)) {
                removeSessionAttribute(e, sessionID, attr);
                return;
            }

            e = e.parentNode;
            removeSessionAttributeOnHierarchy(e, sessionID, attr);
        }

        // Callback function to execute when mutations are observed
        var moCallback = function (mutationsList, observer){
            for (var i = 0; i < mutationsList.length; ++i)
            {
                var mutation = mutationsList[i];

                var targetElement = mutation.target;

                var isTextMutation = mutation.type === ""characterData"" || (mutation.type === ""childList"" && isTextChange(mutation));

                if (isTextMutation) {
                    removeSessionAttributeOnHierarchy(targetElement, observer.sessionID, ""text"");
                }
                
                if (mutation.type === ""attributes"") {
                    if (hasSessionAttribute(targetElement, observer.sessionID, mutation.attributeName)) {
                        removeSessionAttribute(targetElement, observer.sessionID, mutation.attributeName);
                    }
                }
            }
        };

        var isMutationObserverAvailable = function () {//Available in most recent browsers https://developer.mozilla.org/en-US/docs/Web/API/MutationObserver
            return ""MutationObserver"" in window;
        }

        var registerExtractTableObserver = function (sessionId){
            if (sessionId === """")
                return;

            var observersKey = ""uni_dataSessionObservers"";
         
            if (isMutationObserverAvailable()) {
                var attachedObject = window;

                if (!(observersKey in attachedObject)) {
                    attachedObject[observersKey] = {};
                }

                if (!(sessionId in attachedObject[observersKey]))
                {
                    attachedObject[observersKey][sessionId] = [];

                    var newObserver = new MutationObserver(moCallback);
                    //only interested in text changes as the algoritm collects cells inner text
                    newObserver.sessionID = sessionId;
                    newObserver.attr = ""text"";
                    newObserver.observe(topContainer, { childList: true, characterData: true, subtree: true });
                    

                    attachedObject[observersKey][sessionId].push(newObserver);
                }
            }
        }

      
        var registerColumnsObserver = function (sessionId, columns){
            if (sessionId === """")
                return;


            var observersKey = ""uni_dataSessionObservers"";

            if (isMutationObserverAvailable()) {
                var attachedObject = window;

                if (!(observersKey in attachedObject)) {
                    attachedObject[observersKey] = {};
                }

                if (!(sessionId in attachedObject[observersKey]))
                {
                    attachedObject[observersKey][sessionId] = [];

                    var newObserver = new MutationObserver(moCallback);
                    var cdFlag = false;
                    var attrFilters = [];

                    for (var c = 0; c < columns.length; ++c)
                    {                   
                        if (columns[c].attr) {
                            if (columns[c].attr === ""text"")
                                cdFlag = true;
                            else
                                attrFilters.push(columns[c].attr);
                        }

                        if (columns[c].name2 && columns[c].attr2) {
                            if (columns[c].attr2 === ""text"")
                                cdFlag = true;
                            else
                                attrFilters.push(columns[c].attr2);
                        }
                        
                        newObserver.sessionID = sessionId;

                        if (attrFilters.length > 0)
                            newObserver.observe(topContainer, { childList: true, characterData: cdFlag, subtree: true, attributeFilter: attrFilters, attributes: true });
                        else
                            newObserver.observe(topContainer, { childList: true, characterData: cdFlag, subtree: true });

                        attachedObject[observersKey][sessionId].push(newObserver);
                    }
                }
            }
        }


        var xmlDoc = createXmlDoc(xmlText);
    
     // Get sessionID attribute. Every extracted field will be marked with the sessionID
        // so we can decide if we're on the same web page or we advanced to the next one (for multi-page extraction).
        sessionIdAttr = xmlDoc.documentElement.getAttribute(""session_id_attr"");
        if (!sessionIdAttr) {
            sessionIdAttr = """";
        }

        
        if (xmlDoc.documentElement.tagName === ""extract-table"")
        {
            registerExtractTableObserver(sessionIdAttr);

			if (topContainer.tagName.toUpperCase() !== ""TABLE"") {
				throw ""Table object expected!"";
			}

			var getColName = true;
			if (xmlDoc.documentElement.getAttribute(""get_columns_name"") === ""0"") {
				getColName = false;
			}

			var getEmptyCols = false;
			if (xmlDoc.documentElement.getAttribute(""get_empty_columns"") === ""1"") {
			    getEmptyCols = true;
			}

			return extractTable(topContainer, getColName, getEmptyCols);
		}

        var extractedData = { columns : [] };
        var addResultAttr = xmlDoc.documentElement.getAttribute(""add_res_attr"");

        if (!addResultAttr) {
            addResultAttr = ""0"";
        }

        extractedData.addResultAttr = addResultAttr;
        extractedData.sessionID = sessionIdAttr;

        // Extract column info.
		var cols = (typeof xmlDoc.selectNodes !== ""undefined"" ? xmlDoc.selectNodes(""/extract/column"") : xmlDoc.getElementsByTagName(""column""));
        for (var c = 0; c < cols.length; ++c) {
            var crntCol       = cols.item(c);
            var columnName    = crntCol.getAttribute(""name"");
            var attrToExtract = crntCol.getAttribute(""attr"");
            var exact         = (crntCol.getAttribute(""exact"") === ""1"");

            // Column attributes validation.
            if (!columnName) {
                throw ""Column name required in XML input data."";
            }

            if (!attrToExtract) {
                attrToExtract = ""text"";
            }

            var columnData = {};
            columnData.name  = columnName;
            columnData.attr  = attrToExtract;
            columnData.exact = exact;
            columnData.path  = getPathList(crntCol);

			var columnName2    = crntCol.getAttribute(""name2"");
            var attrToExtract2 = crntCol.getAttribute(""attr2"");
			
			if (columnName2 && attrToExtract2) {
				columnData.name2  = columnName2;
				columnData.attr2  = attrToExtract2;
			}

            extractedData.columns.push(columnData);          
        }

        registerColumnsObserver(sessionIdAttr, extractedData.columns);

		var row = null;
        // Select row data.
		if (typeof xmlDoc.selectSingleNode !== ""undefined"") {
			row = xmlDoc.selectSingleNode(""/extract/row"");
		}
		else {
			row = xmlDoc.getElementsByTagName(""row"");
			if (row) {
				row = row[0];
			}
		}

        if (row) {
            var exact = (row.getAttribute(""exact"") === ""1""); 
            extractedData.row = { ""exact"" : exact, ""path"" : getPathList(row) };
        }

       return extractedData;
    }(inputXml);

    // Returns only the text of ""e"" not including the text of children elements.
    var ownText = function (e) {
        var n = e.firstChild;
        var t = """";

        while (n) {
            // Add only the text of text nodes.
            if (n.nodeType === DOM_NodeType.Text) {
                t += n.nodeValue;
            }

            n = n.nextSibling;
        }

        return trim(t);
    };

    var indexInParent = function(elem, columnData) {
        var parent   = elem.parentElement;
        var children = getChildrenByTag(parent, elem.tagName);
        var copyColumnData = { tag : columnData.tag };

        if (columnData.cls) {
            copyColumnData.cls = columnData.cls;
        }

        if (columnData.text) {
            copyColumnData.text = columnData.text;
        }

        var index = 0;
        for (var c = 0; c < children.length; ++c) {
            var crntChild = children[c];
			if (isSameNode(elem, crntChild)) {
                break;
            }

            if (checkElemData(crntChild, copyColumnData)) {
                index++;
            }
        }

        return index;
    };

    var checkElemData = function(elem, columnData) {
        if (elem.tagName.toLowerCase() !== columnData.tag) {
            return false;
        }

        if (columnData.cls && (columnData.cls !== elem.className)) {
            return false;
        }

        if (columnData.text && (columnData.text !== ownText(elem))) {
            return false;
        }

        if ((typeof(columnData.index) !== ""undefined"") && (columnData.index !== indexInParent(elem, columnData))) {
            return false;
        }

        return true;
    };

    var checkHierarchy = function(elem, parent, columnInfo) {
        var crntElem = elem;
        var c = columnInfo.path.length - 1;
        if (!checkElemData(crntElem, columnInfo.path[c])) {
            return false;
        }

        c = c - 1;

        while (c >= 0) {
            crntElem = crntElem.parentElement;
			
            if (!crntElem || isSameNode(crntElem, parent)) {
                return false;
            }

            if (!checkElemData(crntElem, columnInfo.path[c])) {
                if (columnInfo.exact) {
                    return false;
                }
            }
            else {
                c = c - 1;
            }
        }

        return true;
    };

    var getElemAttribute = function (e, attrName) {
		if (attrName === ""text"") {
			return getInnerText(e);
		}
		else if (attrName === ""href"") {
			// If the element is inside an anchor just go up to the parent ancestor to get its href.
			while (true) {
				var href = e.getAttribute(attrName);
				if (href) {
					return href;
				}

				e = e.parentElement;
				if (!e) {
					return """";
				}
			}
		}
		else {
			return e.getAttribute(attrName);
		}
    }; 

   
    /////////////////////////////////////////////////////////////////////////////////////////
	if (typeof (inputData) === ""string"" ) {
		// Table extraction.
		return inputData;
	}
    
	var xmlDoc = createXmlDoc(""<extract></extract>"");
	var rootElem = xmlDoc.documentElement;

    var createRowXml = function (row) {
        var rowResult = [];

        for (var c = 0; c < inputData.columns.length; ++c) {
            var crntCol = inputData.columns[c];
            var colTag  = crntCol.path[crntCol.path.length - 1].tag;
            var tagColl = getAllElemsByTag(row, colTag);

            for (var e = 0; e < tagColl.length; ++e) {
                var crntElem = tagColl[e];
                if (checkHierarchy(crntElem, topContainer, crntCol)) {
                    if (inputData.sessionID !== """")
                    {
                        if (hasSessionAttribute(crntElem, inputData.sessionID, crntCol.attr)
                            || (crntCol.name2 && crntCol.attr2 && hasSessionAttribute(crntElem, inputData.sessionID, crntCol.attr2)))
                        {
                            // The same extract session. Don't extract twice on the same page.
                            isSameSession = true;
                            break;
                        }

                        setSessionAttribute(crntElem, inputData.sessionID, crntCol.attr);
                        if (crntCol.name2 && crntCol.attr2)
                            setSessionAttribute(crntElem, inputData.sessionID, crntCol.attr2);
                    }   

                    var extrData = getElemAttribute(crntElem, crntCol.attr);
					rowResult.push({ ""name"" : crntCol.name, ""extrData"" : extrData });

					if (crntCol.name2 && crntCol.attr2) {
						var extrData2 = getElemAttribute(crntElem, crntCol.attr2);
						rowResult.push({ ""name"" : crntCol.name2, ""extrData"" : extrData2 });
					}

                    if (inputData.addResultAttr === ""1"") {
                        crntElem.setAttribute(""data-extract-result-name"", crntCol.name);
                    }

                    break;
                }
            }
        }

        if (rowResult.length > 0) {
            var rowXml = xmlDoc.createElement(""row"");
            for (var c = 0; c < rowResult.length; ++c) {
                var col = xmlDoc.createElement(""column"");
                rowXml.appendChild(col);

                col.setAttribute(""name"", rowResult[c].name);
                col.appendChild(xmlDoc.createTextNode(rowResult[c].extrData));
            }

            return rowXml;
        }
        else {
            return null;
        }
    };

    if (inputData.row) {
        var rowTag = inputData.row.path[inputData.row.path.length - 1].tag;
        var rowAll = getAllElemsByTag(topContainer, rowTag);

        for (var r = 0; r < rowAll.length; ++r) {
            var crntRow = rowAll[r];
            if (checkHierarchy(crntRow, topContainer, inputData.row)) {
                var rowResXml = createRowXml(crntRow);
                if (rowResXml) {
                    rootElem.appendChild(rowResXml);
                }
            }
        }
    }
    else {
        if (inputData.columns.length !== 1) {
            throw ""No row info provided"";
        }

        // Only one column. Get if from topContainer.
        var firstCol = inputData.columns[0];
        var colTag   = firstCol.path[firstCol.path.length - 1].tag;
        var tagColl = getAllElemsByTag(topContainer, colTag);

        for (var e = 0; e < tagColl.length; ++e) {
            var crntElem = tagColl[e];
			
            if (checkHierarchy(crntElem, topContainer, firstCol)) {
                if (inputData.sessionID !== """")
                {
                    if (hasSessionAttribute(crntElem, inputData.sessionID, firstCol.attr)
                        || (firstCol.name2 && firstCol.attr2 && hasSessionAttribute(crntElem, inputData.sessionID, firstCol.attr2))){
                        // The same extract session. Don't extract twice on the same page.
                        isSameSession = true;
                        continue;
                    }

                    setSessionAttribute(crntElem, inputData.sessionID, firstCol.attr);
                    if (firstCol.name2 && firstCol.attr2)
                        setSessionAttribute(crntElem, inputData.sessionID, firstCol.attr2);
                }


                var extrData = getElemAttribute(crntElem, firstCol.attr);
                var rowXml   = xmlDoc.createElement(""row"");
                var col      = xmlDoc.createElement(""column"");

                col.setAttribute(""name"", firstCol.name);
				col.appendChild(xmlDoc.createTextNode(extrData));
				rowXml.appendChild(col);

				if (firstCol.name2 && firstCol.attr2) {
					var extrData2 = getElemAttribute(crntElem, firstCol.attr2);
					var col2      = xmlDoc.createElement(""column"");

					col2.setAttribute(""name"", firstCol.name2);
					col2.appendChild(xmlDoc.createTextNode(extrData2));
					rowXml.appendChild(col2);
				}

                rootElem.appendChild(rowXml);

                if (inputData.addResultAttr === ""1"") {
                    crntElem.setAttribute(""data-extract-result-name"", firstCol.name);
                }
            }
        }
    }

    if (isSameSession) {
        rootElem.setAttribute(""same-session-encountered"", ""1"");
    }

    
    var result = xmlDoc.xml ? xmlDoc.xml : (new XMLSerializer()).serializeToString(xmlDoc);
    return sanitizeStringForXML(result);
}
";
            var doc = (IHTMLDocument2) ele._element.document;
            InjectJs(doc, jsCode);

            //return browser?.WBrowser.CallScript("getColumnData", ele._element, extractOption).ToString();
            //IHTMLWindow2 windowObject = ele._browser.WindowObject;
            IHTMLWindow2 windowObject = doc.parentWindow;
            return RunJs(windowObject, "getColumnData", ele._element, extractOption);
        }

        /*
        public void ColorColumn(UiElement element, string columnName)
        {
            var browser = Browser.GetBrowser();
            var allElements = browser.Document.all;
            if (allElements.length <= 0)
            {
                return;
            }

            var elements = allElements.Cast<IHTMLElement>()
                .Where(t => t.getAttribute("data-extract-result-name").ToString() == columnName);
            foreach (var el in elements)
            {
                var oldBckg = el.style.backgroundColor;
                el.style.backgroundColor = "wheat";
                if (!string.IsNullOrEmpty(oldBckg))
                {
                    el.setAttribute("data-uni-saved-bckg", oldBckg);
                }
            }
        }

        public void StepTo(UiElement element)
        {
            var browser = Browser.GetBrowser();
            var allElements = browser.Document.all;
            if (allElements.length <= 0)
            {
                return;
            }

            var elements = allElements.Cast<IHTMLElement>()
                .Where(t => !string.IsNullOrEmpty(t.getAttribute("data-extract-result-name").ToString()));
            foreach (var el in elements)
            {
                var savedBckg = el.getAttribute("data-uni-saved-bckg");

                if (savedBckg != null)
                {
                    el.style.backgroundColor = savedBckg;
                    el.removeAttribute("data-uni-saved-bckg");
                }
                else
                {
                    el.style.backgroundColor = "";
                }
                el.removeAttribute("data-extract-result-name");
            }
        }
        */
        public bool Compare(UiElement element1, UiElement element2)
        {
            return element1?.uiNode is IeNode node1 && element2?.uiNode is IeNode node2 &&
                   node1.CssSelector == node2.CssSelector;
        }
        public void ColorColumn(UiElement element, string columnName)
        {
            var ele = element?.uiNode as IeNode;
            var browser = ele?._browser;
            var jsCode = @"var colorColumn=function(dummyElem, colName) {
    // For frame/iframe scenario we need to get the parent document of parameter dummyElem.
    // In Chrome/Firefox, the document refers to top document; for IE it should be already set to iframe/frame document.
    var htmlDoc = document;
    if (dummyElem && dummyElem.ownerDocument) {
        htmlDoc = dummyElem.ownerDocument;
    }

    var getAllElements = function() {
        if (typeof htmlDoc.all !== ""undefined"") {
            return htmlDoc.all;
        } else {
            return htmlDoc.getElementsByTagName(""*"");
        }
    };

    var all = getAllElements();
    for (var i = 0; i < all.length; ++i) {
        var e = all[i];

        if (e.getAttribute(""data-extract-result-name"", 0) === colName) {
            var oldBckg = e.style.backgroundColor;
            e.style.backgroundColor = ""wheat"";

            if (oldBckg) {
                e.setAttribute(""data-uni-saved-bckg"", oldBckg);
            }
        }
    }
}";

            var doc = (IHTMLDocument2)ele._element.document;
            InjectJs(doc, jsCode);

            //IHTMLWindow2 windowObject = ele._browser.WindowObject;
            IHTMLWindow2 windowObject = doc.parentWindow;
            RunJs(windowObject, "colorColumn", ele._element, columnName);
        }
        public void StepTo(UiElement element)
        {
            var ele = element?.uiNode as IeNode;
            var jsCode = @"var stepTo=function(dummyElem) {
    // For frame/iframe scenario we need to get the parent document of parameter dummyElem.
    // In Chrome/Firefox, the document refers to top document; for IE it should be already set to iframe/frame document.
    var htmlDoc = document;
    if (dummyElem && dummyElem.ownerDocument) {
        htmlDoc = dummyElem.ownerDocument;
    }

    var getAllElements = function() {
        if (typeof htmlDoc.all !== ""undefined"") {
            return htmlDoc.all;
        } else {
            return htmlDoc.getElementsByTagName(""*"");
        }
    };

    var all = getAllElements();
    for (var i = 0; i < all.length; ++i) {
        var e = all[i];

        if (e.getAttribute(""data-extract-result-name"", 0)) {
            var savedBckg = e.getAttribute(""data-uni-saved-bckg"");

            if (savedBckg) {
                e.style.backgroundColor = savedBckg;
                e.removeAttribute(""data-uni-saved-bckg"");
            } else {
                e.style.backgroundColor = """";
            }

            e.removeAttribute(""data-extract-result-name"");
        }
    }
}";
            var doc = (IHTMLDocument2)ele._element.document;
            InjectJs(doc, jsCode);

            //IHTMLWindow2 windowObject = ele._browser.WindowObject;
            IHTMLWindow2 windowObject = doc.parentWindow;
            RunJs(windowObject, "stepTo", ele._element);
        }


        public void SetAttribute(UiElement element, string attrName, string attrValue)
        {
            ((IeNode)element?.uiNode)?.SetAttribute(attrName, attrValue);
        }

        public string GetSameColumn(UiElement element, string input)
        {
            var ele = element?.uiNode as IeNode;
            var jsCode = @"var getSameColumn=function(dummyElem, inputXML) {
    // For frame/iframe scenario we need to get the parent document of parameter dummyElem.
    // In Chrome/Firefox, the document refers to top document; for IE it should be already set to iframe/frame document.
    var htmlDoc = document;
    if (dummyElem && dummyElem.ownerDocument) {
        htmlDoc = dummyElem.ownerDocument;
    }

    var columnsData = [];
    var commonParentParam = null;

    var createXmlDoc = function(xmlText) {
        if (typeof window.DOMParser !== ""undefined"") { // Chrome & FF.
            return (new window.DOMParser()).parseFromString(xmlText, ""text/xml"");
        } else { // IE.
            var xmlDoc = new ActiveXObject(""Microsoft.XMLDOM"");
            xmlDoc.async = false;
            xmlDoc.loadXML(xmlText);
            if (xmlDoc.parseError.errorCode != 0) {
                throw xmlDoc.parseError.reason;
            }
        }

        return xmlDoc;
    };

    var isSameNode = function(e1, e2) {
        if (typeof e1.sourceIndex !== ""undefined"") { // IE.
            return (e1.sourceIndex === e2.sourceIndex);
        } else if (typeof e1.isSameNode !== ""undefined"") { // Chrome.
            return e1.isSameNode(e2);
        } else {
            return e1 === e2;
        }
    };

    var getChildrenByTag = function(e, t) {
        t = t.toLowerCase();

        var children = e.children;
        if (typeof children.tags !== ""undefined"") {
            return children.tags(t);
        } else {
            var result = [];
            for (var c = 0; c < children.length; ++c) {
                if (children[c].tagName.toLowerCase() === t) {
                    result.push(children[c]);
                }
            }

            return result;
        }
    };

    var getAllElements = function() {
        if (typeof htmlDoc.all !== ""undefined"") {
            return htmlDoc.all;
        } else {
            return htmlDoc.getElementsByTagName(""*"");
        }
    };

    // Init.
    var initFn = function() {
        var xmlDoc = createXmlDoc(inputXML);
        var cols = (typeof xmlDoc.selectNodes !== ""undefined""
            ? xmlDoc.selectNodes(""/meta-input/column"")
            : xmlDoc.getElementsByTagName(""column""));

        for (var c = 0; c < cols.length; ++c) {
            var crntCol = cols.item(c);
            var columnName = crntCol.getAttribute(""name"");
            var attrToExtract = crntCol.getAttribute(""attr"");

            var uniId1 = crntCol.getAttribute(""uniid1"");
            var uniId2 = crntCol.getAttribute(""uniid2"");
            if (!uniId1 || !uniId2) {
                throw ""ErrId: Invalid uniid"";
            }

            var column = { ""name"": columnName, ""attr"": attrToExtract, ""uniid1"": uniId1, ""uniid2"": uniId2 };
            var columnName2 = crntCol.getAttribute(""name2"");
            var attrToExtract2 = crntCol.getAttribute(""attr2"");

            if (columnName2 && attrToExtract2) {
                column.name2 = columnName2;
                column.attr2 = attrToExtract2;
            }

            columnsData.push(column);
        }

        // Find patterns elements based on TEXT_CAPTURE_X_CUSTOM_ID attribute.
        var elemCount = 0;
        var elems = getAllElements();

        for (var e = 0; e < elems.length; ++e) {
            var crntElem = elems[e];
            var uniid = crntElem.getAttribute(""TEXT_CAPTURE_X_CUSTOM_ID"", 0);

            if (uniid) {
                for (var c = 0; c < columnsData.length; ++c) {
                    if (uniid === columnsData[c].uniid1) {
                        columnsData[c].e1 = crntElem;
                        elemCount++;
                    } else if (uniid === columnsData[c].uniid2) {
                        columnsData[c].e2 = crntElem;
                        elemCount++;
                    }
                }
            }

            var commonParentAttr = crntElem.getAttribute(""data-uni-common-parent"", 0);
            if (commonParentAttr) {
                commonParentParam = crntElem;
                if (elemCount == columnsData.length * 2) {
                    // Found common parent and all columns elements.
                    break;
                }
            }
        }

        if (elemCount !== columnsData.length * 2) {
            throw ""ErrPattern: Cannot find all pattern elements"";
        }

        var skipFormating = function(n) {
            var t = n.tagName.toLowerCase();
            if ((t === ""b"") || (t === ""u"") || (t === ""i"") || (t === ""strong"") || (t === ""font"") || (t === ""em"")) {
                n = n.parentElement;
            }

            return n;
        };

        for (var c = 0; c < columnsData.length; ++c) {
            if (columnsData[c].e1.tagName.toLowerCase() !== columnsData[c].e2.tagName.toLowerCase()) {
                // If the selected element is B, U, I, STRONG, FONT, EM ... then go to parent.
                columnsData[c].e1 = skipFormating(columnsData[c].e1);
                columnsData[c].e2 = skipFormating(columnsData[c].e2);

                if (columnsData[c].e1.tagName.toLowerCase() !== columnsData[c].e2.tagName.toLowerCase()) {
                    throw ""ErrTag: Cannot find pattern, elements differs by tag"";
                }
            }
        }
    };

    initFn();

    // Get the first common parent of elements in a list.
    var commonParent = function(elems) {
        var parentsList = function(e) {
            if (!e) {
                return [];
            }

            var plist = [];
            while (true) {
                e = e.parentElement;
                if (!e) {
                    break;
                }

                plist.push(e);
            }

            plist.reverse();
            return plist;
        };

        var pls = [];
        var minLen = 1000000;

        for (var e = 0; e < elems.length; ++e) {
            var pl = parentsList(elems[e]);
            pls.push(pl);

            if (minLen > pl.length) {
                minLen = pl.length;
            }
        }

        var comnPrnt = null;
        var i = 0;

        while (i < minLen) {
            var crntPrnt = pls[0][i];
            var exitWhile = false;

            for (var pl = 0; pl < pls.length; ++pl) {
                if (!isSameNode(crntPrnt, pls[pl][i])) {
                    exitWhile = true;
                    break;
                }
            }

            if (exitWhile) {
                break;
            }

            comnPrnt = crntPrnt;
            i += 1;
        }

        return comnPrnt;
    };

    var topContainer = function() {
        if (commonParentParam) {
            // Common parent provided in the HTML document with attribute data-uni-common-parent=1
            return commonParentParam;
        }

        var elems = [];
        for (var c = 0; c < columnsData.length; ++c) {
            elems.push(columnsData[c].e1);
            elems.push(columnsData[c].e2);
        }

        return commonParent(elems);
    }();


    if (!topContainer) {
        throw ""ErrParent: Cannot find pattern, not a common parent"";
    }

    // Set an attribute on topContainer so the caller who injected this script can find it.
    topContainer.setAttribute(""data-uni-top-container"", ""1"");

    var row1 = function() {
        var elems = [];
        for (var c = 0; c < columnsData.length; ++c) {
            elems.push(columnsData[c].e1);
        }

        return commonParent(elems);
    }();

    var row2 = function() {
        var elems = [];
        for (var c = 0; c < columnsData.length; ++c) {
            elems.push(columnsData[c].e2);
        }

        return commonParent(elems);
    }();

    // strip leading and trailing spaces from a string.
    var trim = function(text) {
        return text.replace(/^\s+|\s+$/g, """");
    };

    // Returns only the text of ""e"" not including the text of children elements.
    var ownText = function(e) {
        var n = e.firstChild;
        var t = """";

        while (n) {
            // Add only the text of text nodes.
            if (n.nodeType === 3) {
                t += n.nodeValue;
            }

            n = n.nextSibling;
        }

        return trim(t);
    };

    var getElemPath = function(e, p) {
        var indexInParent = function(elem, elemData) {
            var matchData = function(elemData, childData) {
                if (elemData.cls && (elemData.cls !== childData.cls)) {
                    return false;
                }

                if (elemData.text && (elemData.text !== childData.text)) {
                    return false;
                }

                return true;
            };

            var parent = elem.parentElement;
            var children = getChildrenByTag(parent, elem.tagName);
            var index = 0;

            for (var c = 0; c < children.length; ++c) {
                var crntChild = children[c];
                if (isSameNode(elem, crntChild)) {
                    break;
                }

                var childData = getElemData(crntChild);
                if (matchData(elemData, childData)) {
                    // Match on class and text.
                    index++;
                }
            }

            return index;
        };

        var getElemData = function(e, noText) {
            var elemData = { tag: e.tagName.toLowerCase() };
            elemData.cls = e.className ? e.className : """";

            if (!noText) {
                elemData.text = ownText(e);
            }

            return elemData;
        };

        var path = [];
        var noText = true;

        while (true) {
            if (isSameNode(e, p)) {
                break;
            }

            var d = getElemData(e, noText); // First iteration no text.
            noText = false;

            d.index = indexInParent(e, d);
            path.push(d);

            e = e.parentElement;
            if (!e) {
                break;
            }
        }

        path.reverse();
        return path;
    };

    var commonPath = function(p1, p2) {
        if ((p1.length === 0) || (p2.length == 0)) {
            return null;
        }

        var commonData = function(d1, d2) {
            if (d1.tag !== d2.tag) {
                return null;
            }

            var data = { tag: d1.tag };
            var addIdx = true;

            if (d1.cls === d2.cls) {
                if (d1.cls) {
                    data.cls = d1.cls;
                }
            } else {
                addIdx = false;
            }

            if (d1.text === d2.text) {
                if (d1.text) {
                    data.text = d1.text;
                }
            } else {
                addIdx = false;
            }

            // Add indexes only if classes and texts are equal otherwise index are for different classes/texts so not relevant.
            if (addIdx && (d1.index === d2.index)) {
                data.index = d1.index;
            }

            return data;
        };

        var e1Data = p1.pop();
        var e2Data = p2.pop();

        var common = [];
        var len = Math.min(p1.length, p2.length);

        for (var i = 0; i < len; ++i) {
            var cd = commonData(p1[i], p2[i]);
            if (cd) {
                common.push(cd);
            } else {
                break;
            }
        }

        var lastCd = commonData(e1Data, e2Data);
        if (!lastCd) {
            throw ""ErrMatch: Pattern elements don't match"";
        }

        common.push(lastCd);

        var exact = false;
        if ((p1.length === p2.length) && ((common.length - 1) === p1.length)) {
            exact = true;
        }

        return { ""path"": common, ""exact"": exact };
    };

    var columnPaths = [];

    for (var c = 0; c < columnsData.length; ++c) {
        var path1 = getElemPath(columnsData[c].e1, topContainer);
        var path2 = getElemPath(columnsData[c].e2, topContainer);

        var path = commonPath(path1, path2);
        path.name = columnsData[c].name;
        path.attr = columnsData[c].attr;

        if (columnsData[c].name2 && columnsData[c].attr2) {
            path.name2 = columnsData[c].name2;
            path.attr2 = columnsData[c].attr2;
        }

        columnPaths.push(path);
    }

    var outputXml = function() {
        var xmlDoc = createXmlDoc(""<extract></extract>"");
        var rootElem = xmlDoc.documentElement;

        var addPathToNode = function(node, pathObj) {
            for (var p = 0; p < pathObj.path.length; ++p) {
                var webCtrl = xmlDoc.createElement(""webctrl"");
                node.appendChild(webCtrl);

                webCtrl.setAttribute(""tag"", pathObj.path[p].tag);

                if (pathObj.path[p].cls) {
                    webCtrl.setAttribute(""class"", pathObj.path[p].cls);
                }

                if (pathObj.path[p].text) {
                    webCtrl.setAttribute(""text"", pathObj.path[p].text);
                }

                if (typeof(pathObj.path[p].index) !== ""undefined"") {
                    webCtrl.setAttribute(""idx"", pathObj.path[p].index + 1);
                }
            }

            if (pathObj.exact) {
                node.setAttribute(""exact"", ""1"");
            } else {
                node.setAttribute(""exact"", ""0"");
            }
        };

        // For one column we don't need row info.
        if (columnsData.length > 1) {
            var rowPath1 = getElemPath(row1, topContainer);
            var rowPath2 = getElemPath(row2, topContainer);
            var rowPath = commonPath(rowPath1, rowPath2);

            if (rowPath !== null) {
                var row = xmlDoc.createElement(""row"");
                addPathToNode(row, rowPath);
                rootElem.appendChild(row);
            } else {
                throw ""ErrCorrelation: Please indicate fields that are correlated with the fields you indicated to define the first column."";
            }
        }

        for (var p = 0; p < columnPaths.length; ++p) {
            var col = xmlDoc.createElement(""column"");

            addPathToNode(col, columnPaths[p]);
            col.setAttribute(""name"", columnPaths[p].name);
            col.setAttribute(""attr"", columnPaths[p].attr);

            if (columnPaths[p].name2 && columnPaths[p].attr2) {
                col.setAttribute(""name2"", columnPaths[p].name2);
                col.setAttribute(""attr2"", columnPaths[p].attr2);
            }

            rootElem.appendChild(col);
        }

        return xmlDoc.xml ? xmlDoc.xml : (new XMLSerializer()).serializeToString(xmlDoc);
    }();

    return outputXml;
}";
            var doc = (IHTMLDocument2)ele._element.document;
            InjectJs(doc, jsCode);

            //IHTMLWindow2 windowObject = ele._browser.WindowObject;
            IHTMLWindow2 windowObject = doc.parentWindow;
            return RunJs(windowObject, "getSameColumn", ele._element, input);
        }

        public IBrowser GetBrowser(UiElement element)
        {
            var node = element?.uiNode as IeNode;
            return node?._browser;
        }

        private void InjectJs(IHTMLDocument2 document, string jsCode)
        {
            MSHTML.IHTMLElement jsElement = document.createElement("script");
            jsElement.setAttribute("type", "text/javascript");
            jsElement.setAttribute("text", jsCode);
            ((IHTMLDOMNode)document.body).appendChild((IHTMLDOMNode)jsElement);
        }

        private string RunJs(IHTMLWindow2 windowObject, string method, params object[] args)
        {
            return windowObject.GetType().InvokeMember(method, BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, windowObject, args, CultureInfo.CurrentCulture)?.ToString();
        }
    }
}
