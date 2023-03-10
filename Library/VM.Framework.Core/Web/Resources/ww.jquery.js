/// <reference path="jquery.js" />
/*
ww.jQuery.js  
Version 0.970 - 10/20/09
West Wind jQuery plug-ins and utilities

(c) 2008-2009 Rick Strahl, West Wind Technologies 
www.west-wind.com

Licensed under MIT License
http://en.wikipedia.org/wiki/MIT_License
*/
// (function($) {
this.HttpClient = function(opt) {
    var _I = this;

    this.completed = null;
    this.errorHandler = null;
    this.errorMessage = "";
    this.async = true;
    this.evalResult = false;   // treat result as JSON    
    this.contentType = "application/x-www-form-urlencoded";
    this.method = "GET";
    this.timeout = 20000;
    this.headers = {};
    $.extend(_I, opt);

    this.appendHeader = function(header, value) {
        _I.headers[header] = value;
    }
    this.send = function(url, postData, completed, errorHandler) {
        completed = completed || _I.completed;
        errorHandler = errorHandler || _I.errorHandler;
        $.ajax(
           { url: url,
               data: postData,
               type: (postData ? "POST" : _I.method),
               processData: false,  // always process on our own!
               contentType: _I.contentType,
               timeout: _I.timeout,
               dataType: "text",
               global: false,
               async: _I.async,
               beforeSend: function(xhr) {
                   for (var header in _I.headers) xhr.setRequestHeader(header, _I.headers[header]);
               },
               success: function(result, status) {
                   var errorException = null;

                   if (_I.evalResult) {
                       try {
                           result = JSON.parseWithDate(result);
                           if (result && result.hasOwnProperty("d"))
                               result = result.d;
                       }
                       catch (e)
                       { errorException = new CallbackException(e); }
                   }
                   if (errorException || (result && (result.isCallbackError || result.iscallbackerror))) {
                       if (errorHandler)
                           errorHandler(errorException, _I);
                       return;
                   }
                   if (completed)
                       completed(result, _I);
               },
               error: function(xhr, status) {
                   var err = null;
                   if (xhr.readyState == 4) {
                       var res = xhr.responseText;
                       if (res && res.charAt(0) == '{')
                           err = JSON.parseWithDate(res);
                       if (!err) {
                           if (xhr.status && xhr.status != 200)
                               err = new CallbackException(xhr.status + " " + xhr.statusText);
                           else
                               err = new CallbackException("Callback Error: " + status);
                           err.detail = res;
                       }
                   }
                   if (!err)
                       err = new CallbackException("Callback Error: " + status);

                   if (errorHandler)
                       errorHandler(err, _I, xhr);
               }
           });
    }
    this.returnError = function(message) {
        var error = new CallbackException(message);
        if (_I.errorHandler)
            _I.errorHandler(error, _I);
    }
}

this.ServiceProxy = function(serviceUrl) {
    /// <summary>
    /// Generic Service Proxy class that can be used to 
    /// call JSON Services generically using jQuery
    /// </summary>
    /// <param name="serviceUrl" type="string">The Url of the service ready to accept the method name</param>
    /// <example>
    /// var proxy = new ServiceProxy("JsonStockService.svc/");
    /// proxy.invoke("GetStockQuote",{symbol:"msft"},function(quote) { alert(result.LastPrice); },onPageError);
    ///</example>        
    var _I = this;
    this.isWcf = true;
    this.timeout = 20000;
    this.serviceUrl = serviceUrl;

    if (typeof serviceUrl === "object")
        $.extend(this, serviceUrl);

    // Call a wrapped object
    this.invoke = function(method, params, callback, errorCallback, isBare) {
        /// <summary>
        /// Calls a WCF/ASMX service and returns the result.
        /// </summary>    
        /// <param name="method" type="string">The method of the service to call</param>
        /// <param name="params" type="object">An object that represents the parameters to pass {symbol:"msft",years:2}       
        /// <param name="callback" type="function">Function called on success. Receives a single parameter of the parsed result value</parm>
        /// <param name="errorCallback" type="function">Function called on failure. Receives a single error object with Message and StackDetail</parm>
        /// <param name="isBar" type="boolean">Set to true if response is not a WCF/ASMX style wrapped object</parm>

        // Convert input data into JSON using internal code
        var json = _I.isWcf ? JSON.stringifyWcf(params) : JSON.stringify(params);

        // The service endpoint URL MyService.svc/       
        var url = _I.serviceUrl + method;

        var http = new HttpClient(
        { contentType: "application/json",
            evalResult: true,
            timeout: _I.timeout
        });
        http.send(url, json, callback, errorCallback);
    }
}

this.AjaxMethodCallback = function(controlId, url, opt) {
    var _I = this;
    this.controlId = controlId;
    this.postbackMode = "PostMethodParametersOnly";  // Post,PostNoViewState,Get
    this.serverUrl = url;
    this.formName = null;
    this.resultMode = "json";  // json,msajax,string
    this.timeout = 20000;

    this.completed = null;
    this.errorHandler = null;
    $.extend(this, opt);

    this.Http = null;

    this.callMethod = function(methodName, parameters, callback, errorCallback) {
        _I.completed = callback;
        _I.errorHandler = errorCallback;

        var http = new HttpClient({ timeout: _I.timeout, evalResult: true });
        _I.Http = http;

        var data = {};
        if (_I.resultMode == "msajax")
            data = JSON.stringifyWithDates(parameters);
        else {
            var parmCount = 0;
            if (parameters.length) {
                parmCount = parameters.length;
                for (var x = 0; x < parmCount; x++) {
                    data["Parm" + (x + 1).toString()] = JSON.stringify(parameters[x]);
                }
            }
            $.extend(data, { CallbackMethod: methodName,
                CallbackParmCount: parmCount,
                __WWEVENTCALLBACK: _I.controlId
            });

            data = $.param(data) + "&"
        }

        var formName = _I.formName || document.forms.length > 0 ? document.forms[0].id : "";

        if (_I.postbackMode == "Post")
            data += $("#" + formName).serialize();
        else if (_I.postbackMode == "PostNoViewstate")
            data += $().serializeNoViewState();
        else if (this.postbackMode == "Get") {
            Url = this.serverUrl;
            if (Url.indexOf('?') > -1)
                Url += data;
            else
                Url += "?" + data;

            return http.send(Url, null, _I.onHttpCallback, _I.onHttpCallback);
        }

        return http.send(this.serverUrl, data, _I.onHttpCallback, _I.onHttpCallback);
    }

    this.onHttpCallback = function(result) {
        if (result && (result.isCallbackError || result.iscallbackerror)) {
            if (_I.errorHandler)
                _I.errorHandler(result, _I);
            return;
        }
        if (_I.completed != null)
            _I.completed(result, _I);
    }
}

ajaxJson = function(url, parm, cb, ecb, options) {
    var ser = parm;
    var opt = { method: "POST", contentType: "application/json", noPostEncoding: false };
    $.extend(opt, options);

    var http = new HttpClient(opt);
    http.evalResult = true;
    if (!opt.noPostEncoding && opt.method == "POST")
        ser = JSON.stringify(parm);

    http.send(url, ser, cb, ecb);
}
ajaxCallMethod = function(url, method, parms, cb, ecb, opt) {
    var proxy = new AjaxMethodCallback(null, url, opt);
    proxy.callMethod(method, parms, cb, ecb);
}
$.postJSON = function(url, data, cb, ecb, opt) {
    var options = { method: "POST", evalResult: true };
    $.extend(options, opt);

    var http = new HttpClient(options);
    if (typeof data === "object")
        data = $.param(data);

    http.send(url, data, cb, ecb);
}
function onPageError(error) {
    showStatus(error.message, 6000, true);
}
this.CallbackException = function(message, detail) {
    this.isCallbackError = true;
    if (typeof (message) == "object") {
        if (message.message)
            this.message = message.message;
        else if (message.Message)
            this.message = message.Message;
    }
    else
        this.message = message;

    if (detail)
        this.detail = detail;
    else
        this.detail = null;
}

this.StatusBar = function(sel, opt) {
    var _I = this;
    var _sb = null;

    // options     
    this.elementId = "_showstatus";
    this.prependMultiline = true;
    this.showCloseButton = false;
    this.afterTimeoutText = null;

    this.cssClass = "statusbar";
    this.highlightClass = "statusbarhighlight";
    this.closeButtonClass = "statusbarclose";
    this.additive = false;

    if (sel)
        _sb = $(sel);

    if (opt)
        $.extend(this, opt);

    // create statusbar object manually
    if (!_sb) {
        _sb = $("<div id='_statusbar' class='" + _I.cssClass + "'>" +
"<div class='" + _I.closeButtonClass + "'>" +
(_I.showCloseButton ? " X </div></div>" : ""))
 .appendTo(document.body)
 .show();
    }
    if (_I.showCloseButton)
        $("." + _I.cssClass).click(function(e) { $(_sb).hide(); });


    this.show = function(message, timeout, isError, additive) {
        if (message == "hide")
            return _I.hide();

        if (_I.additive) {
            var html = $("<div style='margin-bottom: 2px;'>" + message + "</div>");
            if (_I.prependMultiline)
                _sb.prepend(html);
            else
                _sb.append(html);
        }
        else {

            if (!_I.showCloseButton)
                _sb.text(message);
            else {
                var t = _sb.find("div.statusbarclose");
                _sb.text(message).prepend(t);
            }
        }
        _sb.show().maxZIndex();

        if (timeout) {
            _sb.addClass(_I.highlightClass);

            setTimeout(
                function() {
                    _sb.removeClass(_I.highlightClass);
                    if (_I.afterTimeoutText)
                        _I.show(_I.afterTimeoutText);
                },
                timeout);
        }
        return _I;
    }
    this.hide = function() {
        _sb.hide();
        return _I;
    }
    this.release = function() {
        if (_statusbar)
            $(_statusbar).remove();
    }
}
// use this as a global instance to customize constructor
// or do nothing and get a default status bar
var __statusbar = null;
this.showStatus = function(message, timeout, isHighlighted, additive) {
    if (!__statusbar)
        if (typeof message == "object") {
        __statusbar = new StatusBar(null, message).hide();
        return;
    }
    else
        __statusbar = new StatusBar();

    __statusbar.show(message, timeout, isHighlighted, additive);
}


$.fn.centerInClient = function(options) {
    /// <summary>Centers the selected items in the browser window. Takes into account scroll position.
    /// Ideally the selected set should only match a single element.
    /// </summary>    
    /// <param name="fn" type="Function">Optional function called when centering is complete. Passed DOM element as parameter</param>    
    /// <param name="forceAbsolute" type="Boolean">if true forces the element to be removed from the document flow 
    ///  and attached to the body element to ensure proper absolute positioning. 
    /// Be aware that this may cause ID hierachy for CSS styles to be affected.
    /// </param>
    /// <returns type="jQuery" />
    var opt = { forceAbsolute: false,
        container: window,    // selector of element to center in
        completed: null
    };
    $.extend(opt, options);

    return this.each(function(i) {
        var el = $(this);
        var jWin = $(opt.container);
        var isWin = opt.container == window;

        // force to the top of document to ENSURE that 
        // document absolute positioning is available
        if (opt.forceAbsolute) {
            if (isWin)
                el.remove().appendTo("body");
            else
                el.remove().appendTo(jWin[0]);
        }

        // have to make absolute
        el.css("position", "absolute");

        // height is off a bit so fudge it
        var heightFudge = 2.2;

        var x = (isWin ? jWin.width() : jWin.outerWidth()) / 2 - el.outerWidth() / 2;
        var y = (isWin ? jWin.height() : jWin.outerHeight()) / heightFudge - el.outerHeight() / 2;

        x = x + jWin.scrollLeft();
        y = y + jWin.scrollTop();
        y = y < 5 ? 5 : y;
        x = x < 5 ? 5 : x;
        el.css({ left: x, top: y });

        var zi = el.css("zIndex");
        if (!zi || zi == "auto")
            el.css("zIndex", 1);

        // if specified make callback and pass element
        if (opt.completed)
            opt.completed(this);
    });
}

$.fn.moveToMousePosition = function(evt, options) {
    var opt = { left: 0, top: 0 };
    $.extend(opt, options);
    return this.each(function() {
        var el = $(this);
        el.css({ left: evt.pageX + opt.left,
            top: evt.pageY + opt.top,
            position: "absolute"
        });
    });
}

$.fn.shadow = function(action, options, refreshOnly) {
    /// <summary>
    /// Applies a drop shadow to an element by 
    /// underlaying a <div> underneath the element(s)
    ///
    /// Note the shadow is not locked to the element
    /// so if the element is moved the shadow needs
    /// to be reapplied.    
    /// </summary>    
    /// <param name="action" type="string">
    /// optional - hide, remove
    /// can also be the options parameter if action is omitted
    /// </param>    
    /// <param name="options" type="object">
    /// optional parameters.
    ///    offset: 6,
    ///    color: "black",
    ///    opacity: .25,
    ///    callback: null,
    ///    zIndex: 100,
    /// </param>    
    /// <returns type="jQuery" /> 
    if (typeof action == "object")
        options = action;

    var opt = { offset: 5,
        color: "#535353",
        opacity: 0.85,
        callback: null,
        zIndex: 100
    };
    $.extend(opt, options);

    this.each(function() {
        var el = $(this);
        var box = this;
        var sh = $("#" + el.get(0).id + "Shadow");

        if (typeof action == "string") {
            if (action == "hide" || action == "remove") {
                if (typeof box.style.MozBoxShadow == "string") {
                    el.css("-moz-box-shadow", "");
                    return;
                }
                else if (typeof box.style.WebkitBoxShadow == "string") {
                    el.css("-webkit-box-shadow", "");
                    return;
                }
                else {
                    el.unwatch("_shadowMove");
                    sh.remove();
                }
            }
            return;
        }

        // MUST turn into absolute position first        
        el.css("position", "absolute");
        if (typeof box.style.MozBoxShadow == "string") {
            el.css("-moz-box-shadow", String.format("{0}px {0}px {0}px {1}", opt.offset, opt.color));
            return;
        }
        else if (typeof box.style.WebkitBoxShadow == "string") {
            el.css("-webkit-box-shadow", String.format("{0}px {0}px {0}px {1}", opt.offset, opt.color));
            return;
        }

        var exists = true;
        if (sh.length < 1) {
            sh = $("<div>")
                        .css({ height: 1, width: 1 })
                        .attr("id", el.get(0).id + "Shadow")
                        .insertAfter(el);

            var zi = el.css("zIndex");
            if (!zi || zi == "auto") {
                el.css("zIndex", opt.zIndex);
                sh.css("zIndex", opt.zIndex - 1);
            }

            var shEl = sh.get(0);
            if (typeof shEl.style.filter == "string") {
                shEl.style.filter = 'progid:DXImageTransform.Microsoft.Blur(makeShadow=true, pixelradius=3, shadowOpacity=' + opt.opacity.toString() + ')';
            }
            exists = false;
        }

        if (typeof sh.get(0).style.filter == "string")
            opt.offset = opt.offset - 4;

        var vis = el.is(":visible");
        if (!vis)
            el.show();  // must be visible to get .position

        var pos = el.position();

        sh.show()
          .css({ position: "absolute",
              width: el.outerWidth(),
              height: el.outerHeight(),
              opacity: opt.opacity,
              background: opt.color,
              left: pos.left + opt.offset,
              top: pos.top + opt.offset
          });

        if (!vis) {
            sh.hide();
            el.hide();
        }

        zIndex = el.css("zIndex");
        if (zIndex && zIndex != "auto")
            sh.css("zIndex", zIndex - 1);
        else {
            el.css("zIndex", opt.zIndex); sh.css("zIndex", opt.zIndex - 1);
        }

        if (!exists) {
            el.watch("left,top,width,height,display,opacity,zIndex",
                 function(w, i) {
                     if (el.is(":visible")) {
                         $(this).shadow(opt);
                         sh.css("opacity", el.css("opacity") * opt.opacity);
                     }
                     else
                         sh.hide();

                 },
                 100, "_shadowMove");
        }

        if (opt.callback)
            opt.callback(sh);
    });
    return this;
}

$.fn.tooltip = function(msg, timeout, options) {
    var opt = { cssClass: "",
        isHtml: false,
        shadowOffset: 2,
        onRelease: null
    };
    $.extend(opt, options);

    return this.each(function() {
        var tp = new _ToolTip(this, opt);
        if (msg == "hide") {
            tp.hide();
            return;
        }
        tp.show(msg, timeout, opt.isHtml);
    });

    function _ToolTip(sel, opt) {
        var _I = this;
        var jEl = $(sel);

        this.cssClass = "";
        this.onRelease = null;
        $.extend(_I, opt);

        var el = jEl.get(0);
        var tt = $("#" + el.id + "_tt");

        this.show = function(msg, timeout, isHtml) {
            if (tt.length > 0)
                tt.remove();

            tt = $("<div></div>").attr("id", el.id + "_tt");

            $(document.body).append(tt);

            tt.css({
                position: "absolute",
                display: "none",
                zIndex: 1000
            });
            if (_I.cssClass)
                tt.addClass(_I.cssClass);
            else
                tt.css({
                    background: "cornsilk",
                    border: "solid 1px gray",
                    fontSize: "0.80em",
                    padding: 2
                });

            if (isHtml)
                tt.html(msg);
            else
                tt.text(msg);

            var pos = jEl.position();

            var Left = pos.left + 5;
            var Top = pos.top + jEl.height() - 2;

            var Width = tt.width();
            if (Width > 400)
                Width = 400;

            tt.css({ left: Left,
                top: Top,
                width: Width
            });
            tt.show();
            tt.shadow({ offset: opt.shadowOffset });

            if (timeout && timeout > 0)
                setTimeout(function() {
                    if (_I.onRelease)
                        _I.onRelease.call(el, _I);
                    _I.hide();
                }, timeout);
        }
        this.hide = function() {
            if (tt.length > 0)
                tt.fadeOut("slow", function() { tt.shadow("hide") });
        }
    }
}

$.fn.watch = function(props, func, interval, id) {
    /// <summary>
    /// Allows you to monitor changes in a specific
    /// CSS property of an element by polling the value.
    /// when the value changes a function is called.
    /// The function called is called in the context
    /// of the selected element (ie. this)
    /// </summary>    
    /// <param name="prop" type="String">CSS Properties to watch sep. by commas</param>    
    /// <param name="func" type="Function">
    /// Function called when the value has changed.
    /// </param>    
    /// <param name="interval" type="Number">
    /// Optional interval for browsers that don't support DOMAttrModified or propertychange events.
    /// Determines the interval used for setInterval calls.
    /// </param>
    /// <param name="id" type="String">A unique ID that identifies this watch instance on this element</param>  
    /// <returns type="jQuery" /> 
    if (!interval)
        interval = 200;
    if (!id)
        id = "_watcher";

    return this.each(function() {
        var _t = this;
        var el = $(this);
        var fnc = function() { __watcher.call(_t, id) };
        var itId = null;

        if (typeof (this.onpropertychange) == "object")
            el.bind("propertychange." + id, fnc);
        else if ($.browser.mozilla)
            el.bind("DOMAttrModified." + id, fnc);
        else
            itId = setInterval(fnc, interval);

        var data = { id: itId,
            props: props.split(","),
            func: func,
            vals: [props.split(",").length]
        };
        $.each(data.vals, function(i) { data.vals[i] = el.css(data.props[i]); });
        el.data(id, data);
    });

    function __watcher(id) {
        var el = $(this);
        var w = el.data(id);

        var changed = false;
        var i = 0;
        for (i; i < w.props.length; i++) {
            var newVal = el.css(w.props[i]);
            if (w.vals[i] != newVal) {
                w.vals[i] = newVal;
                changed = true;
                break;
            }
        }
        if (changed && w.func) {
            var _t = this;
            w.func.call(_t, w, i)
        }
    }
}
$.fn.unwatch = function(id) {
    this.each(function() {
        var w = $(this).data(id);
        var el = $(this);
        el.removeData();

        try {
            if (typeof (this.onpropertychange) == "object")
                el.unbind("propertychange." + id, fnc);
            else if ($.browser.mozilla)
                el.unbind("DOMAttrModified." + id, fnc);
            else
                clearInterval(w.id);
        }
        // ignore if element was already unbound
        catch (e) { }
    });
    return this;
}


$.fn.listSetData = function(items, options) {
    var opt = { noClear: false,        // don't clear the list first if true
        dataValueField: null,          // optional value field for object lists
        dataTextField: null
    };
    $.extend(opt, options);

    return this.each(function() {
        var el = $(this);

        if (items == null) {
            el.children().remove();
            return;
        }
        if (!opt.noClear)
            el.children().remove();

        if (items.Rows)
            items = items.Rows;

        var IsValueList = false;

        if (!opt.dataTextField && !opt.dataValueField)
            IsValueList = true;

        for (x = 0; x < items.length; x++) {
            var row = items[x];
            if (IsValueList)
                el.listAddItem(row, row);
            else
                el.listAddItem(row[opt.dataTextField], row[opt.dataValueField]);
        }
    });
}
$.fn.listAddItem = function(text, value) {
    return this.each(function() {
        $(this).append($("<option></option>").attr("value", value).text(text));
    });
}
$.fn.listSelectItem = function(value) {
    if (this.length < 1)
        return;
    var list = this.get(0);
    if (!list.options)
        return;

    for (var x = list.options.length - 1; x > -1; x--) {
        if (list.options[x].value == value) {
            list.options[x].selected = true;
            return;
        }
    }
}

this.HoverPanel = function(sel, opt) {
    var _I = this;
    var jEl = $(sel);
    var el = jEl.get(0);

    var busy = -1;
    var lastMouseTop = 0;
    var lastMouseLeft = 0;

    this.serverUrl = "";
    this.controlId = el.id;
    this.htmlTargetId = el.id;
    this.queryString = "";
    this.eventHandlerMode = "ShowHtmlAtMousePosition";
    this.postbackMode = "Get"; // Post/PostNoViewState
    this.completed = null;
    this.errorHandler = null;
    this.hoverOffsetRight = 0;
    this.hoverOffsetBottom = 0;
    this.panelOpacity = 1;
    this.shadowOffset = 0;
    this.shadowOpacity = 0.25;
    this.adjustWindowPosition = true;
    this.formName = "";
    this.navigateDelay = 0;
    this.http = null;
    $.extend(_I, opt);

    this.startCallback = function(e, queryString, postData, errorHandler) {
        try {
            var key = new Date().getTime();
            _I.busy = key;
            var Url = this.serverUrl;
            if (e) {
                _I.lastMouseTop = e.clientY;
                _I.lastMouseLeft = e.clientX;
            }
            else
                _I.lastMouseTop = 0;

            if (queryString == null)
                _I.queryString = queryString = "";
            else
                _I.queryString = queryString;

            if (errorHandler)
                _I.errorHandler = errorHandler;

            if (queryString)
                queryString += "&";
            else
                queryString = "";
            queryString += "__WWEVENTCALLBACK=" + _I.controlId;

            _I.formName = _I.formName || document.forms[0];

            _I.http = new HttpClient();
            _I.http.appendHeader("RequestKey", key);

            if (postData)
                postData += "&";
            else
                postData = "";

            if (_I.postbackMode == "Post")
                postData += $(_I.formName).serialize();
            else if (this.postbackMode == "PostNoViewstate")
                postData += $(_I.formName).serializeNoViewState();
            else if (this.postbackMode == "Get" && postData)
                queryString += postData;

            if (queryString != "") {
                if (Url.indexOf("?") > -1)
                    Url = Url + "&" + queryString
                else
                    Url = Url + "?" + queryString;
            }

            if (_I.eventHandlerMode == 'ShowIFrameAtMousePosition' ||
                _I.eventHandlerMode == 'ShowIFrameInPanel') {
                setTimeout(function() { if (_I.busy) _I.showIFrame.call(_I, Url); }, _I.navigateDelay);
                return;
            }

            // Send the request with navigate delay
            setTimeout(function() {
                if (_I.busy == key)
                    _I.http.send.call(_I, Url, postData, _I.onHttpCallback, _I.onHttpCallback);
            }, _I.navigateDelay);
        }
        catch (e) {
            // Call with 'error message'
            _I.onHttpCallback(new CallbackException(e.message));
        }
    }

    this.onHttpCallback = function(result) {
        _I.busy = -1;

        if (_I.http && _I.http.status && _I.http.status != 200)
            result = new CallbackException(http.statusText);
        if (result == null)
            result = new CallbackException("No output was returned.");

        if (result.isCallbackError) {
            if (_I.errorHandler)
                _I.errorHandler(result);
            return;
        }
        _I.displayResult(result);
    }
    this.displayResult = function(result) {
        if (_I.completed && _I.completed(result, _I) == false)
            return;
        if (_I.eventHandlerMode == "ShowHtmlAtMousePosition") {
            _I.assignContent(result);
            _I.movePanelToPosition(_I.lastMouseLeft + _I.hoverOffsetRight, _I.lastMouseTop + _I.hoverOffsetBottom);
            _I.show();
        }
        else if (_I.eventHandlerMode == "ShowHtmlInPanel") {
            _I.assignContent(result);
            _I.show();
        }
    }
    this.assignContent = function(result) {
        $("#" + _I.htmlTargetId).html(result);
    }
    this.movePanelToPosition = function(x, y) {
        try {
            jEl.css("position", "absolute");

            if (typeof x == "object") {
                _I.lastMouseTop = x.clientY;
                _I.lastMouseLeft = x.clientX;
            }
            else if (typeof x == "number") {
                _I.lastMouseTop = y;
                _I.lastMouseLeft = x;
            }

            x = _I.lastMouseLeft + 3;
            y = _I.lastMouseTop + 3;
            var jWin = $(window);
            jEl.css({ left: x + jWin.scrollLeft(), top: y + jWin.scrollTop() });

            if (_I.adjustWindowPosition && document.body) {
                var mainHeight = jWin.height();
                var panHeight = jEl.outerHeight();
                var mainWidth = jWin.width();
                var panWidth = jEl.outerWidth();

                if (mainHeight < panHeight)
                    y = 0;
                else {
                    if (mainHeight < _I.lastMouseTop + panHeight)
                        y = mainHeight - panHeight - 10;
                }

                if (mainWidth < panWidth)
                    x = 0;
                else {
                    if (mainWidth < _I.lastMouseLeft + panWidth)
                        x = mainWidth - panWidth - 25;
                }
                jEl.css({ left: x + jWin.scrollLeft(), top: y + jWin.scrollTop() });
            }
        }
        catch (e)
        { window.status = 'Moving of window failed: ' + e.message; }
    }
    this.showIFrame = function(Url) {
        _I.busy = false;
        Url = Url ? Url : _I.serverUrl;
        $("#" + _I.controlId + '_IFrame').attr("src", Url).load(_I.completed);
        _I.show();
        if (_I.eventHandlerMode == "ShowIFrameAtMousePosition")
            _I.movePanelToPosition(_I.lastMouseLeft + _I.hoverOffsetRight, _I.lastMouseTop + _I.hoverOffsetBottom);


    }
    this.hide = function() {
        this.abort();
        jEl.hide();
    }
    this.abort = function() { _I.busy = -1; }
    this.show = function() {
        jEl.show().css("opacity", _I.panelOpacity);
        if (_I.shadowOffset)
            jEl.shadow({ offset: _I.shadowOffset, opacity: _I.shadowOpacity });
    }
}

function _ModalDialog(sel, opt) {
    var _I = this;
    var jEl = $(sel);
    if (jEl.length < 1)
        jEl = $("#" + sel);
    if (jEl.length < 1)
        return;

    this.overlayId = "_ModalOverlay";

    this.contentId = jEl.get(0).id;
    this.headerId = "";
    this.backgroundOpacity = .75;
    this.fadeInBackground = false;
    this.zIndex = 0;
    this.jOverlay = null;
    this.keepCentered = true;
    this.dialogHandler = null;
    $.extend(_I, opt);
    var hideLists = null;


    this.show = function(msg, head, asHtml) {
        if (_I.contentId && typeof msg == "string")
            !asHtml ? $("#" + _I.contentId).text(msg) : $("#" + _I.contentId).html(msg);
        if (_I.headerId && typeof head == "string")
            !asHtml ? $("#" + _I.headerId).text(head) : $("#" + _I.headerId).html(head);

        var zi = _I.zIndex > 0 ? _I.zIndex : $.maxZIndex();
        jEl.css({ zIndex: _I.zIndex + 2 }).show().centerInClient();

        var bg = opaqueOverlay({ zIndex: _I.zIndex + 1, sel: "#" + _I.overlayId, opacity: _I.backgroundOpacity });
        _I.zIndex++;
        if (_I.fadeInBackground)
            bg.hide().fadeIn("slow");

        // track any clicks inside modal dialog
        jEl.click(_I.callback);

        if (_I.keepCentered)
            $(window).bind("resize.modal", function() { jEl.centerInClient() })
                         .bind("scroll.modal", function() { jEl.centerInClient() });

        // hide visible IE listboxes and store
        if ($.browser.msie && $.browser.version < "7")
            hideLists = $("select:visible").not($(jEl).find("select")).hide();
    }
    this.hide = function() {
        jEl.hide();
        if (_I.keepCentered)
            $(window).unbind("resize.modal")
                     .unbind("scroll.modal");
        opaqueOverlay("hide", { sel: "#" + _I.overlayId });
        jEl.unbind("click");

        // restore IE list boxes
        if (hideLists) {
            hideLists.show(); hideLists = null;
        }
    }
    this.callback = function(e) {
        if (_I.dialogHandler) {
            if (_I.dialogHandler.call(e.target, e, _I) == false)
                return;
            setTimeout(function() { _I.hide(); }, 10);
            return;
        }
        // by default close window when clicking a button or link
        if ($(e.target).is(":button,a,img"))
            setTimeout(function() { _I.hide(); }, 10);
    }
}
$.fn.modalDialog = function(opt, msg, head, asHtml, handler) {
    if (this.length < 1)
        return this;

    // only works with a single instance
    var el = this.get(0);
    var jEl = $(el);
    var dId = "modal" + el.id;

    var md = jEl.data(dId);
    if (!md)
        md = new _ModalDialog(jEl, opt);
    if (typeof opt == "string") {
        if (opt == "hide" || opt == "close")
            md.hide();
        if (opt == "instance" || opt == "get")
            return md;
        return;
    }
    md.show(msg, head, asHtml);
    jEl.data(dId, md);

    return this;
}
$.modalDialog = function(msg, header, aButtons, handler, isHtml) {
    var dl = $("#_MBOX");
    if (dl.length < 1) {
        dl = $("<div></div>").addClass("blackborder dragwindow").attr("id", "_MBOX").css({ width: 400 });
        var head = $("<div></div>").addClass("gridheader").attr("id", "_MBOXHEADER");
        var ctn = $("<div></div>").addClass("containercontent").attr("id", "_MBOXCONTENT");

        dl.append(head).append(ctn);
        var btns = $("<div></div>").css("margin", "5px 15px");
        if (!aButtons)
            aButtons = [" Close "];
        for (var i = 0; i < aButtons.length; i++) {
            var btn = $("<input type='button' />").attr("id", "_BTN_" + i).css("margin-right", "5px").val(aButtons[i]);
            btns.append(btn);
        }
        dl.append(btns).appendTo(document.body);
    }
    if (!handler) handler = function() { if (this.id.substr(0, 5) == "_BTN_" || $(this).hasClass("closebox")) return true; return false; };
    dl.modalDialog({ dialogHandler: handler, headerId: "_MBOXHEADER", contentId: "_MBOXCONTENT" },
                   msg, header, isHtml)
          .draggable().shadow()
          .closable({ closeHandler: handler || function() { dl.modalDialog("hide"); } });
}
this.opaqueOverlay = function(opt, p2) {
    var _I = this;
    var jWin = $(window);

    this.sel = "#_ShadowOverlay";
    this.opacity = 0.75;
    this.zIndex = 10000;
    $.extend(this, p2 || opt);

    var sh = $(sel);
    if (opt == "hide") {
        if (sh.length < 1)
            return;
        sh.hide();
        sh.get(0).opaqueOverlay = false;
        jWin.unbind("resize.opaque").unbind("scroll.opaque");
        return;
    }

    if (sh.length < 1)
        sh = $("<div></div>")
                 .attr("id", this.sel.substr(1))
                 .css("background", "black")
                 .appendTo(document.body);

    var el = sh.get(0);
    sh.show();

    if (!el.opaqueOverlay)
        jWin.bind("resize.opaque", function() { opaqueOverlay(opt); })
            .bind("scroll.opaque", function() { opaqueOverlay(opt); });

    el.opaqueOverlay = true;

    sh.css({ top: 0 + jWin.scrollTop(), left: 0 + jWin.scrollLeft(), position: "absolute", opacity: _I.opacity, zIndex: _I.zIndex })
        .width(jWin.width())
        .height(jWin.height());
    return sh;
}


if (!$.fn.draggable) {
    $.fn.draggable = function(opt) {
        return this.each(function() {
            var el = $(this);
            var drag = el.data("draggable");

            if (typeof opt == "string") {
                if (drag && opt == "remove") {
                    drag.stopDragging();
                    el.removeData("draggable");
                }
                return;
            }
            if (!drag) {
                drag = new DragBehavior(this, opt);
                el.data("draggable", drag);
            }
        });
    }
    var __dragIndex = 1;

    function DragBehavior(sel, opt) {
        var _I = this;
        var el = $(sel);

        this.handle = "";
        this.opacity = 0.75;
        this.start = null;
        this.stop = null;
        this.dragDelay = 100;
        this.forceAbsolute = false;

        $.extend(_I, opt);

        _I.handle = _I.handle ? $(_I.handle, el) : el;
        if (_I.handle.length < 1)
            _I.handle = el;

        var isMouseDown = false;
        var isDrag = false;
        var timeMD = 0;
        var clicked = -1;
        var deltaX = 0;
        var deltaY = 0;
        var savedOpacity = 1;
        var savedzIndex = 0;

        this.mouseDown = function(e) {
            var dEl = _I.handle.get(0);
            var s = false;
            $(e.target).parents().each(function() { if (this == dEl) s = true; });
            if (isMouseDown || (e.target != dEl && !s) || $(e.target).is(".closebox,input,textara,a"))
                return;

            isMouseDown = true;
            isDrag = false;

            var pos = _I.handle.offset();
            deltaX = e.pageX - pos.left;
            deltaY = e.pageY - pos.top;

            setTimeout(function() {
                if (!isMouseDown) return;
                el.show().makeAbsolute(_I.forceAbsolute);
                _I.dragActivate(e);
            }, _I.dragDelay);

        }
        var nf = function(e) { e.stopPropagation(); e.preventDefault(); };
        this.dragActivate = function(e) {
            if (!isMouseDown) return;
            isDrag = true;
            _I.moveToMouse(e);
            isMouseDown = true;
            savedzIndex = el.css("zIndex");
            el.css("zIndex", 150000);
            savedOpacity = el.css("opacity");
            el.css({ opacity: _I.opacity, cursor: "move" });

            $(document).bind("mousemove.dbh", _I.mouseMove);
            $(document).bind("selectstart.dbh", nf);
            $(document).bind("dragstart.dbh", nf);
            $(document.body).bind("dragstart.dbh", nf);
            $(document.body).bind("selectstart.dbh", nf);
            _I.handle.bind("selectstart.dbh", nf);
            if (_I.start)
                _I.start(e, _I);
        }
        this.dragDeactivate = function(e, noMove) {
            if (!isMouseDown) return;
            isMouseDown = false;

            if (!isDrag) return;
            isDrag = false;

            if (!noMove)
                _I.moveToMouse(e);
            $(document).unbind("mousemove.dbh");

            $(document).unbind("selectstart.dbh");
            $(document).unbind("dragstart.dbh");
            $(document.body).unbind("dragstart.dbh");
            $(document.body).unbind("selectstart.dbh");
            _I.handle.unbind("selectstart.dbh");

            if (!noMove) {
                __dragIndex += 10;
                el.css({ zIndex: 10000 + __dragIndex, cursor: "auto" });
                el.css("opacity", savedOpacity);
                if (_I.stop)
                    _I.stop(e, _I);
            }
        }
        this.mouseUp = function(e) {
            _I.dragDeactivate(e);
        }
        this.mouseMove = function(e) {
            if (isMouseDown)
                _I.moveToMouse(e);
        }
        this.moveToMouse = function(e) {
            el.css({ left: e.pageX - deltaX, top: e.pageY - deltaY });
        }
        this.stopDragging = function() {
            if (!isDrag) return;
            _I.dragDeactivate(null, true);
            $(document).unbind("mousedown", _I.mouseDown);
        }
        $(document).mousedown(_I.mouseDown);
        $(document).mouseup(_I.mouseUp);
    }
}

$.fn.closable = function(options) {
    var opt = { handle: null,
        closeHandler: null,
        cssClass: "closebox",
        imageUrl: null,
        fadeOut: null
    };
    $.extend(opt, options);

    return this.each(function(i) {
        var el = $(this);        
        var pos = el.css("position");
        if (!pos || pos == "static")
            el.css("position", "relative");
        var h = opt.handle ? $(opt.handle).css({ position: "relative" }) : el;
        
        var div = opt.imageUrl ? $("<img />").attr("src", opt.imageUrl).css("cursor", "pointer") : $("<div></div>");
        div.addClass(opt.cssClass)
           .click(function(e) {
               if (opt.closeHandler)
                   if (!opt.closeHandler.call(this, e))
                   return;
               if (opt.fadeOut)
                   $(el).fadeOut(opt.fadeOut);
               else $(el).hide();
           });
        if (opt.imageUrl) div.css("background-image", "none");
        h.append(div);
    });
}

$.fn.contentEditable = function(opt) {
    if (this.length < 1)
        return;
    var oldPadding = "0px";
    var def = { editClass: null,
        saveText: "Save",
        saveHandler: null
    };
    $.extend(def, opt);

    this.each(function() {
        var jContent = $(this);

        if (this.contentEditable == "true")
            return this; // already editing

        var jButton = $("<input type='button' value='" + def.saveText + "' class='editablebutton' style='display: block;'/>");

        var cleanupEditor = function() {
            if (def.editClass)
                jContent.removeClass(def.editClass);
            else
                jContent.css({ background: "transparent", padding: oldPadding });
            jContent.get(0).contentEditable = false;
            jButton.remove();
        };

        jButton.click(function(e) {
            if (def.saveHandler.call(jContent.get(0), e))
                cleanupEditor();
        });
        jContent.keypress(function(e) {
            if (e.keyCode == 27)
                cleanupEditor();
        });

        jContent
            .after(jButton)
            .css("margin", 2);

        this.contentEditable = true;

        if (def.editClass)
            jContent.addClass(def.editClass);
        else {
            oldPadding = jContent.css("padding");
            jContent.css({ background: "lavender", padding: 10 });
        }
        return this;
    });
    return this;
}
$.fn.editable = function(opt) {
    if (this.length < 1)
        return this;

    var oldPadding = "0px";
    var def = { editClass: null,
        saveText: "Save",
        editMode: "text",  // html
        saveHandler: null,
        value: null
    };
    $.extend(def, opt);

    this.each(function() {
        var jContent = $(this);

        if (opt == "cleanup") {            
            jContent.data("cleanupEditor")();
            return this;
        }

        if (jContent.data("editing"))
            return this;

        var jButton = $("<input type='button' />")
                         .addClass("editablebutton")
                         .css({ display: "block" })
                         .val(def.saveText);
        var jEdit = $("<textarea id='_contenteditor'></textarea>")
                       .css({ fontFamily: jContent.css("font-family"), minHeight: "18px" });
        if (def.value) jEdit.val(def.value);
        else jEdit.val(def.editMode == "text" ? jContent.text() : jContext.html());
        if (def.editClass)
            jEdit.addClass(def.editClass);
        else
            jEdit.width(jContent.width() - 10)
                 .height(jContent.height());

        jEdit.focus()
            .hide().fadeIn("slow")
            .data("editing", jContent.get(0))
            .insertBefore(jContent)
            .keypress(function(e) {
                if (e.keyCode == 27)
                    cleanupEditor();
            });

        jContent
            .data("editing", true)
            .hide();

        jContent.data("cleanupEditor", function() {            
            jEdit.remove();
            jButton.remove();
            jContent
                .data("editing", false)
                .data("cleanupEditor", null)
                .fadeIn("slow");
        });

        jButton.click(function(e) {
            var pass = { text: jEdit.val(),
                cleanup: jContent.data("cleanupEditor"),
                button: jButton,
                edit: jEdit,
                content: jContent
            };
            if (def.saveHandler.call(jEdit.get(0), pass))
                cleanupEditor();
        });

        jEdit
            .after(jButton)
            .css("margin", 2);
        return this;
    });
    return this;
}
$.maxZIndex = $.fn.maxZIndex = function(opt) {
    /// <summary>
    /// Returns the max zOrder in the document (no parameter)
    /// Sets max zOrder by passing a non-zero number
    /// which gets added to the highest zOrder.
    /// </summary>    
    /// <param name="opt" type="object">
    /// inc: increment value, 
    /// group: selector for zIndex elements to find max for
    /// </param>
    /// <returns type="jQuery" />
    var def = { inc: 10, group: "*" };
    $.extend(def, opt);
    var zmax = 0;
    $(def.group).each(function() {
        var cur = parseInt($(this).css('z-index'));
        zmax = cur > zmax ? cur : zmax;
    });
    if (!this.jquery)
        return zmax;

    return this.each(function() {
        zmax += def.inc;
        $(this).css("z-index", zmax);
    });
}
/// <script type="text/html" id="script">
/// <div> 
///   <#= content #>
///   <# for(var i=0; i < names.length; i++) { #>
///   Name: <#= names[i] #> <br/>
///   <# } #>
/// </div>
/// </script>
///
/// var tmpl = $("#itemtemplate").html();
/// var data = { content: "This is some textual content",
///              names: ["rick", "markus"]
/// };
/// $("#divResult").html(parseTemplate(tmpl, data));
///
/// based on John Resig's Micro Templating engine
var _tmplCache = {}
this.parseTemplate = function(str, data) {
    /// <summary>
    /// Client side template parser that uses &lt;#= #&gt; and &lt;# code #&gt; expressions.
    /// and # # code blocks for template expansion.
    /// </summary>    
    /// <param name="str" type="string">The text of the template to expand</param>    
    /// <param name="data" type="var">
    /// Any data that is to be merged. Pass an object and
    /// that object's properties are visible as variables.
    /// </param>    
    /// <returns type="string" />  
    var err = "";
    try {
        var func = _tmplCache[str];
        if (!func) {
            var strFunc =
            "var p=[];with(obj){p.push('" +
            str.replace(/[\r\t\n]/g, " ")
               .replace(/'(?=[^#]*#>)/g, "\t")
               .split("'").join("\\'")
               .split("\t").join("'")
               .replace(/<#=(.+?)#>/g, "',$1,'")
               .split("<#").join("');")
               .split("#>").join("p.push('")
               + "');}return p.join('');";
            func = new Function("obj", strFunc);
            _tmplCache[str] = func;
        }
        return func(data);
    } catch (e) { err = e.message; }
    return "< # ERROR: " + err.htmlEncode() + " # >";
}

function $$(id, context) {
    /// <summary>
    /// Searches for an ID based on ASP.NET naming container syntax.
    /// First search by ID as is, then uses attribute based lookup.
    /// Works only on single elements - not list items in enumerated
    /// containers.
    /// </summary>    
    /// <param name="id" type="var">Element ID to look up</param>    
    /// <param name="context" type="var">
    /// </param>    
    /// <returns type="" /> 
    var el = $("#" + id, context);
    if (el.length < 1)
        el = $("[id$=_" + id + "],[id*=[" + id + "_]", context);
    return el;
}

String.prototype.htmlEncode = function() {
    var div = document.createElement('div');
    if (typeof (div.textContent) == 'string')
        div.textContent = this.toString();
    else
        div.innerText = this.toString();
    return div.innerHTML;
}
String.prototype.trimEnd = function(c) {
    if (c)
        return this.replace(new RegExp(c.escapeRegExp() + "*$"), '');
    return this.replace(/\s+$/, '');
}
String.prototype.trimStart = function(c) {
    if (c)
        return this.replace(new RegExp("^" + c.escapeRegExp() + "*"), '');
    return this.replace(/^\s+/, '');
}
String.repeat = function(chr, count) {
    var str = "";
    for (var x = 0; x < count; x++) { str += chr };
    return str;
}
String.prototype.padL = function(width, pad) {
    if (!width || width < 1)
        return this;

    if (!pad) pad = " ";
    var length = width - this.length
    if (length < 1) return this.substr(0, width);

    return (String.repeat(pad, length) + this).substr(0, width);
}
String.prototype.padR = function(width, pad) {
    if (!width || width < 1)
        return this;

    if (!pad) pad = " ";
    var length = width - this.length
    if (length < 1) this.substr(0, width);

    return (this + String.repeat(pad, length)).substr(0, width);
}
String.startsWith = function(str) {
    if (!str) return false;
    return this.substr(0, str.length) == str;
}
String.prototype.escapeRegExp = function() {
    return this.replace(/[.*+?^${}()|[\]\/\\]/g, "\\$0");
};
String.format = function(frmt, args) {
    for (var x = 0; x < arguments.length; x++) {
        frmt = frmt.replace(new RegExp("\\{" + x.toString() + "\\}", "g"), arguments[x + 1]);
    }
    return frmt;
}
String.prototype.format = function() {
    var a = [this];
    $.merge(a, arguments);
    return String.format.apply(this, a);
}
String.prototype.isNumber = function() {

    if (this.length == 0) return false;
    if ("0123456789".indexOf(this.charAt(0)) > -1)
        return true;
    return false;
}
var _monthNames = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
Date.prototype.formatDate = function(format) {
    var date = this;
    if (!format)
        format = "MM/dd/yyyy";

    var month = date.getMonth();
    var year = date.getFullYear();

    if (format.indexOf("yyyy") > -1)
        format = format.replace("yyyy", year.toString());
    else if (format.indexOf("yy") > -1)
        format = format.replace("yy", year.toString().substr(2, 2));

    format = format.replace("dd", date.getDate().toString().padL(2, "0"));

    var hours = date.getHours();
    if (format.indexOf("t") > -1) {
        if (hours > 11)
            format = format.replace("t", "pm")
        else
            format = format.replace("t", "am")
    }
    if (format.indexOf("HH") > -1)
        format = format.replace("HH", hours.toString().padL(2, "0"));
    if (format.indexOf("hh") > -1) {
        if (hours > 12) hours -= 12;
        if (hours == 0) hours = 12;
        format = format.replace("hh", hours.toString().padL(2, "0"));
    }
    if (format.indexOf("mm") > -1)
        format = format.replace("mm", date.getMinutes().toString().padL(2, "0"));
    if (format.indexOf("ss") > -1)
        format = format.replace("ss", date.getSeconds().toString().padL(2, "0"));

    if (format.indexOf("MMMM") > -1)
        format = format.replace("MMMM", _monthNames[month]);
    else if (format.indexOf("MMM") > -1)
        format = format.replace("MMM", _monthNames[month].substr(0, 3));
    else
        format = format.replace("MM", (month + 1).toString().padL(2, "0"));

    return format;
}
Number.prototype.formatNumber = function(format, option) {
    var num = this;
    var fmt = Number.getNumberFormat();
    if (format == "c") {
        num = Math.round(num * 100) / 100;
        option = option || "$";
        num = num.toLocaleString();
        var s = num.split(".");
        var p = s.length > 1 ? s[1] : '';
        return option + s[0] + fmt.d + p.padR(2, '0');
    }
    if (format.charAt(0) == "n") {
        if (format.length == 1)
            return num.toLocaleString()
        var dec = format.substr(1);
        dec = parseInt(dec);
        if (typeof (dec) != "number")
            return num.toLocaleString();
        num = num.toFixed(dec);
        var x = num.split(fmt.d);
        var x1 = x[0];
        var x2 = x.length > 1 ? fmt.d + x[1] : '';
        var rgx = /(\d+)(\d{3})/;
        while (rgx.test(x1))
            x1 = x1.replace(rgx, '$1' + fmt.c + '$2');
        return x1 + x2
    }
    if (format.charAt(0) == "f") {
        if (format.length == 1)
            return num.toString();
        var dc = format.substr(1);
        dc = parseFloat(dec);
        if (typeof (dec) != "number")
            return num.toString();
        return num.toFixed(dec);
    }
    return num.toString();
}
Number.getNumberFormat = function(cur) {
    var t = 1000.1.toLocaleString();
    var r = {};
    r.d = t.charAt(5);
    if (r.d.isNumber())
        r.d = t.charAt(4);
    r.c = t.charAt(1);
    if (r.c.isNumber())
        r.c = ",";
    r.s = cur || "$";
    return r;
}
registerNamespace = function(ns) {
    var pts = ns.split('.');
    var stk = window;
    var nsp = "";
    for (var i = 0; i < pts.length; i++) {
        var pt = pts[i];
        if (stk[pt])
            stk = stk[pt];
        else
            stk = stk[pt] = {};
    }
}
getUrlEncodedKey = function(key, query) {
    if (!query)
        query = window.location.search;
    var re = new RegExp("[?|&]" + key + "=(.*?)&");
    var matches = re.exec(query + "&");
    if (!matches || matches.length < 2)
        return "";
    return decodeURIComponent(matches[1].replace("+", " "));
}
setUrlEncodedKey = function(key, value, query) {

    query = query || window.location.search;
    var q = query + "&";
    var re = new RegExp("[?|&]" + key + "=.*?&");
    if (!re.test(q))
        q += key + "=" + encodeURI(value);
    else
        q = q.replace(re, "&" + key + "=" + encodeURIComponent(value) + "&");
    q = q.trimStart("&").trimEnd("&");
    return q.charAt(0) == "?" ? q : q = "?" + q;
}
$.fn.serializeNoViewState = function() {
    return this.find("input,textarea,select,hidden").not("#__VIEWSTATE,#__EVENTVALIDATION").serialize();
}
$.fn.makeAbsolute = function(rebase) {
    /// <summary>
    /// Makes an element absolute
    /// </summary>    
    /// <param name="rebase" type="boolean">forces element onto the body tag. Note: might effect rendering or references</param>    
    /// </param>    
    /// <returns type="jQuery" /> 
    return this.each(function() {
        var el = $(this);
        var pos = el.position();
        el.css({ position: "absolute",
            marginLeft: 0, marginTop: 0,
            top: pos.top, left: pos.left
        });
        if (rebase)
            el.remove().appendTo("body");
    });
}
if (!this.assert) {
    this.assert = function(cond, msg) {
        if (cond) return;
        if (!msg) msg = "";
        alert("Assert failed\r\n" + (msg ? msg : "") + "\r\n" +
              (arguments.callee.caller ? "in " + arguments.callee.caller.toString() : ""));
    }
}
var _ud = "undefined";
/*
http://www.JSON.org/json2.js
2009-04-16
Public Domain.
*/
if (!this.JSON) { this.JSON = {}; }
(function() {
    function f(n) { return n < 10 ? '0' + n : n; }
    if (typeof Date.prototype.toJSON !== 'function') {
        Date.prototype.toJSON = function(key) {
            return isFinite(this.valueOf()) ? this.getUTCFullYear() + '-' +
f(this.getUTCMonth() + 1) + '-' +
f(this.getUTCDate()) + 'T' +
f(this.getUTCHours()) + ':' +
f(this.getUTCMinutes()) + ':' +
f(this.getUTCSeconds()) + 'Z' : null;
        }; String.prototype.toJSON = Number.prototype.toJSON = Boolean.prototype.toJSON = function(key) { return this.valueOf(); };
    }
    var cx = /[\u0000\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, escapable = /[\\\"\x00-\x1f\x7f-\x9f\u00ad\u0600-\u0604\u070f\u17b4\u17b5\u200c-\u200f\u2028-\u202f\u2060-\u206f\ufeff\ufff0-\uffff]/g, gap, indent, meta = { '\b': '\\b', '\t': '\\t', '\n': '\\n', '\f': '\\f', '\r': '\\r', '"': '\\"', '\\': '\\\\' }, rep; function quote(string) { escapable.lastIndex = 0; return escapable.test(string) ? '"' + string.replace(escapable, function(a) { var c = meta[a]; return typeof c === 'string' ? c : '\\u' + ('0000' + a.charCodeAt(0).toString(16)).slice(-4); }) + '"' : '"' + string + '"'; }
    function str(key, holder) {
        var i, k, v, length, mind = gap, partial, value = holder[key]; if (value && typeof value === 'object' && typeof value.toJSON === 'function') { value = value.toJSON(key); }
        if (typeof rep === 'function') { value = rep.call(holder, key, value); }
        switch (typeof value) {
            case 'string': return quote(value); case 'number': return isFinite(value) ? String(value) : 'null'; case 'boolean': case 'null': return String(value); case 'object': if (!value) { return 'null'; }
                gap += indent; partial = []; if (Object.prototype.toString.apply(value) === '[object Array]') {
                    length = value.length; for (i = 0; i < length; i += 1) { partial[i] = str(i, value) || 'null'; }
                    v = partial.length === 0 ? '[]' : gap ? '[\n' + gap +
partial.join(',\n' + gap) + '\n' +
mind + ']' : '[' + partial.join(',') + ']'; gap = mind; return v;
                }
                if (rep && typeof rep === 'object') { length = rep.length; for (i = 0; i < length; i += 1) { k = rep[i]; if (typeof k === 'string') { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ': ' : ':') + v); } } } } else { for (k in value) { if (Object.hasOwnProperty.call(value, k)) { v = str(k, value); if (v) { partial.push(quote(k) + (gap ? ': ' : ':') + v); } } } }
                v = partial.length === 0 ? '{}' : gap ? '{\n' + gap + partial.join(',\n' + gap) + '\n' +
mind + '}' : '{' + partial.join(',') + '}'; gap = mind; return v;
        }
    }
    if (typeof JSON.stringify !== 'function') {
        JSON.stringify = function(value, replacer, space) {
            var i; gap = ''; indent = ''; if (typeof space === 'number') { for (i = 0; i < space; i += 1) { indent += ' '; } } else if (typeof space === 'string') { indent = space; }
            rep = replacer; if (replacer && typeof replacer !== 'function' && (typeof replacer !== 'object' || typeof replacer.length !== 'number')) { throw new Error('JSON.stringify'); }
            return str('', { '': value });
        };
    }
    if (typeof JSON.parse !== 'function') {
        JSON.parse = function(text, reviver) {
            var j; function walk(holder, key) {
                var k, v, value = holder[key]; if (value && typeof value === 'object') { for (k in value) { if (Object.hasOwnProperty.call(value, k)) { v = walk(value, k); if (v !== undefined) { value[k] = v; } else { delete value[k]; } } } }
                return reviver.call(holder, key, value);
            }
            cx.lastIndex = 0; if (cx.test(text)) {
                text = text.replace(cx, function(a) {
                    return '\\u' +
('0000' + a.charCodeAt(0).toString(16)).slice(-4);
                });
            }
            if (/^[\],:{}\s]*$/.test(text.replace(/\\(?:["\\\/bfnrt]|u[0-9a-fA-F]{4})/g, '@').replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, ']').replace(/(?:^|:|,)(?:\s*\[)+/g, ''))) { j = eval('(' + text + ')'); return typeof reviver === 'function' ? walk({ '': j }, '') : j; }
            throw new SyntaxError('JSON.parse');
        };
    }
} ());

if (this.JSON && !this.JSON.parseWithDate) {
    var reISO = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*)?)Z$/;
    var reMsAjax = /^\/Date\((d|-|.*)\)[\/|\\]$/;

    JSON.parseWithDate = function(json) {
        /// <summary>
        /// parses a JSON string and turns ISO or MSAJAX date strings
        /// into native JS date objects
        /// </summary>    
        /// <param name="json" type="var">json with dates to parse</param>        
        /// </param>
        /// <returns type="value, array or object" />
        try {
            var res = JSON.parse(json,
            function(key, value) {
                if (typeof value === 'string') {
                    var a = reISO.exec(value);
                    if (a)
                        return new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6]));
                    a = reMsAjax.exec(value);
                    if (a) {
                        var b = a[1].split(/[-+,.]/);
                        return new Date(b[0] ? +b[0] : 0 - +b[1]);
                    }
                }
                return value;
            });
            return res;
        } catch (e) {
            // orignal error thrown has no error message so rethrow with message
            throw new Error("JSON content could not be parsed");
            return null;
        }
    };
    JSON.stringifyWcf = function(json) {
        /// <summary>
        /// Wcf specific stringify that encodes dates in the
        /// a WCF compatible format ("/Date(9991231231)/")
        /// Note: this format works ONLY with WCF. 
        ///       ASMX can use ISO dates as of .NET 3.5 SP1
        /// </summary>
        /// <param name="key" type="var">property name</param>
        /// <param name="value" type="var">value of the property</param>         
        return JSON.stringify(json, function(key, value) {
            if (typeof value == "string") {
                var a = reISO.exec(value);
                if (a) {
                    var val = '/Date(' + new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6])).getTime() + ')/';
                    this[key] = val;
                    return val;
                }
            }
            return value;
        })
    };
    JSON.dateStringToDate = function(dtString) {
        /// <summary>
        /// Converts a JSON ISO or MSAJAX string into a date object
        /// </summary>    
        /// <param name="" type="var">Date String</param>
        /// <returns type="date or null if invalid" /> 
        var a = reISO.exec(dtString);
        if (a)
            return new Date(Date.UTC(+a[1], +a[2] - 1, +a[3], +a[4], +a[5], +a[6]));
        a = reMsAjax.exec(dtString);
        if (a) {
            var b = a[1].split(/[-,.]/);
            return new Date(+b[0]);
        }
        return null;
    };
}
//})(jQuery);