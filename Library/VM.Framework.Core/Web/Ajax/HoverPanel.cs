﻿/*
 **************************************************************
 * HoverPanel Control
 ************************************************************** 
 * 
 * This ASP.NET Control class provides a number of AJAX
 * functionalities for ASP.NET pages.
 * 
 * Hover Windows:
 * You can specify a URL and use the HoverWindow option
 * to automatically pop up a window at the current mouse 
 * location. Handles auto positioning, delayed display,
 * and basic display management of the panel displayed.
 * 
 * Callback Url Results:
 * Allows you to call another URL and callback to a specified
 * client script handler method. The values returned from this
 * routine are pure strings.
 * 
 * The class is accompanied by a client library contained in
 * wwScriptLibrary.js which matches the HoverPanel class on
 * the client. The client and server pieces communicate with
 * each other. The library also contains many utility functions.
 * 
 * Source Dependencies:
 * JSONSerializer.cs
 * ReflectionUtils.cs
 * 
 * Acknowledgements:
 * This code borrows concepts from Jason Diamonds MyAjax.NET
 * also known as Anthem:
 * http://sourceforge.net/mailarchive/forum.php?forum=anthem-dot-net-devel
 * 
 * The original JavaScript JSON deserialization code file is based on:
 *     copyright: '(c)2005 JSON.org',
 *       license: 'http://www.crockford.com/JSON/license.html',
 * 
 * License:
 * Free without any restrictions whatsoever
 * 
 * I only ask that if you make changes that you think are useful
 * you let us know and post a message at:
 * 
 * Version History:
 * Please see included help file
 **************************************************************  
*/

using System;
using System.Globalization;

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Design;

using System.Web;

namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// The HoverPanel class provides an easy to use base AJAX control that  
    /// allows quick access to server side content from a URL and embed it into the
    ///  control's content. The control provides various visual customization 
    /// aspects from shadows, transparency, the ability to drag it around and close
    ///  it as well as pop up and auto hide.
    /// 
    /// The most prominent feature of this control is to provide auto-popup 
    /// functionality that shows context sensitive content while hovering and 
    /// hiding it when moving off.
    /// 
    /// &lt;&lt;img src="images/HoverWindow.png"&gt;&gt;
    /// 
    /// For more details on how the control works, see the 
    /// <see cref="_1Q100DYWK">HoverPanel Windows</see> topic. 
    /// This control  allows calling back to explict URLs
    /// either on the same page or calling other URLs within the same site that 
    /// feed the content to be rendered in the  hover panel control.
    /// </summary>
    [ToolboxBitmap(typeof(Panel)), DefaultProperty("Url"),
    ToolboxData("<{0}:HoverPanel runat=\"server\" style=\"width:450px;background:white;display:none;\"></{0}:HoverPanel>")]
    public class HoverPanel : DragPanel
    {
        /// <summary>
        /// 
        /// </summary>
        public HoverPanel()
        {
            // Default background color to white - 
            //     don't want transparent backgrounds on hover windows
            BackColor = Color.White;

            // Default draggable status to false
            Draggable = false;
        }

        /// <summary>
        /// The client script event handler function called when the remote call 
        /// completes just before the result content is displayed. Allows modification 
        /// of the content and possibly blocking of the content by returning false.
        /// 
        /// For more information see the HoverPanel client class and its 
        /// <see cref="_1WD06AVLG">callbackHandler method</see>. The handler is  passed the result from the 
        /// callback and you can return false to stop  rendering of the hover window.
        /// <seealso>Class HoverPanel</seealso>
        /// </summary>
        [Description("The client script event handler function called when the remote call completes. Optional for panel operations. Receives a result string with GetHttpResponse mode is used."),
        DefaultValue(""), Category("Client Events")]
        public string ClientCompleteHandler
        {
            get
            {
                return _ClientCompleteHandler;
            }
            set
            {
                _ClientCompleteHandler = value;
            }
        }
        private string _ClientCompleteHandler = "";


        /// <summary>
        /// The Url to hit on the server for the callback to return the result. Note: Not used when doing a MethodCallback
        /// </summary>
        [Description("The Url to hit on the server for the callback to return the result."),
        DefaultValue(""), Category("HoverPanel")]
        [UrlProperty]
        public string ServerUrl
        {
            get
            {
                return _ServerUrl;
            }
            set
            {
                _ServerUrl = value;
            }
        }
        private string _ServerUrl = "";

        /// <summary>
        /// Determines if the navigation is delayed by a hesitation. Useful for link hovering.
        /// </summary>
        [Description("Determines if the navigation is delayed by a hesitation. Useful for link hovering.")]
        [DefaultValue(0), Category("HoverPanel")]
        public int NavigateDelay
        {
            get
            {
                return _NavigateDelay;
            }
            set
            {
                _NavigateDelay = value;
            }
        }
        private int _NavigateDelay = 0;

        /// <summary>
        /// Determines whether this request is a callbackDetermines whether the current
        ///  request is a callback from the AjaxMethodCallback or HoverPanel control.
        /// 
        /// This property is used internally to trap for method processing, but you can
        ///  also use this in your page or control level code to determine whether you 
        /// need to do any special processing based on the callback situation.
        /// <seealso>Class HoverPanel</seealso>
        /// </summary>
        [Browsable(false)]
        public bool IsCallback
        {
            get
            {
                if (!_IsCallback.HasValue)
                {
                    string Id = Context.Request.Params["__WWEVENTCALLBACK"];
                    if (Id != null && Id == ClientID)
                    {
                        _IsCallback = true;
                        return true;
                    }
                    _IsCallback = false;
                    return false;
                }
                else
                {
                    return _IsCallback.Value;
                }
            }
        }
        private bool? _IsCallback = null;

        /// <summary>
        /// Determines the how the event is handled  on the callback request. ShowHtmlMousePosition shows the result in a window. CallEventHandler fires the specified script function.
        /// </summary>
        [Description("Determines the how the event is handled  on the callback request. ShowHtmlMousePosition shows the result in a window. CallEventHandler fires the specified script function."),
        DefaultValue(HoverEventHandlerModes.ShowHtmlAtMousePosition), Category("HoverPanel")]
        public HoverEventHandlerModes EventHandlerMode
        {
            get
            {
                return _EventHandlerMode;
            }
            set
            {
                _EventHandlerMode = value;
            }
        }
        private HoverEventHandlerModes _EventHandlerMode = HoverEventHandlerModes.ShowHtmlAtMousePosition;

        /// <summary>
        /// The client ID of the control that receives the hoverpanel output. This affects only the HTML if empty the hoverpanel is used.
        /// </summary>
        [Description("The client ID of the control that receives the hoverpanel output. This affects only the HTML if empty the hoverpanel is used."),
         Category("HoverPanel"), DefaultValue("")]
        public string HtmlTargetClientId
        {
            get { return _HtmlTargetClientId; }
            set { _HtmlTargetClientId = value; }
        }
        private string _HtmlTargetClientId = "";


        /// <summary>
        /// if set tries to move up the window if it's too low to fit content. This setting can cause problems with very large content.
        /// </summary>
        [Description("If set tries to move up the window if it's too low to fit content. This setting can cause problems with very large content."),
       DefaultValue(false), Category("Panel Display")]
        public bool AdjustWindowPosition
        {
            get
            {
                return _AdjustWindowPosition;
            }
            set
            {
                _AdjustWindowPosition = value;
            }
        }
        private bool _AdjustWindowPosition = false;

        /// <summary>
        /// Determines whether the window is closed automatically if you mouse off it
        /// when the window is a hover window.
        /// </summary>
        [Description("Determines whether the window is closed automatically if you mouse off it when the window is a hover window."),
         Category("HoverPanel"), DefaultValue(true)]
        public bool AutoCloseHoverWindow
        {
            get { return _AutoCloseHoverWindow; }
            set { _AutoCloseHoverWindow = value; }
        }
        private bool _AutoCloseHoverWindow = true;


        /// <summary>
        /// The right offset when the the panel is shown at the mouse position
        /// </summary>
        [Description("The right offset when the the panel is shown at the mouse position"),
         Category("Panel Display"), DefaultValue(0)]
        public int HoverOffsetRight
        {
            get { return _HoverOffsetRight; }
            set { _HoverOffsetRight = value; }
        }
        private int _HoverOffsetRight = 0;

        /// <summary>
        /// The bottom offset when the the panel is shown at the mouse position
        /// </summary>
        [Description("The bottom offset when the the panel is shown at the mouse position"),
         Category("Panel Display"), DefaultValue(0)]
        public int HoverOffsetBottom
        {
            get { return _HoverOffsetBottom; }
            set { _HoverOffsetBottom = value; }
        }
        private int _HoverOffsetBottom = 0;



        /// <summary>
        /// If true causes the page to post back all form variables.
        /// </summary>
        [Description("If true causes the page to post back all form variables."), DefaultValue(PostBackModes.Get),
         Category("HoverPanel")]
        public PostBackModes PostBackMode
        {
            get
            {
                return _PostBackFormData;
            }
            set
            {
                _PostBackFormData = value;
            }
        }
        private PostBackModes _PostBackFormData = PostBackModes.Get;

        /// <summary>
        /// The name of the form from which values are posted back to the server. Note only a single form's 
        /// values can be posted back!
        /// </summary>
        [Description("If PostBackData is set, this variable determines which form is posted back."), DefaultValue(""), Category("HoverPanel")]
        public string PostBackFormName
        {
            get
            {
                return _PostBackFormName;
            }
            set
            {
                _PostBackFormName = value;
            }
        }
        private string _PostBackFormName = "";

        /// <summary>
        /// The height of an IFRAME if mode is IFrame related. Use this if you need to specifically size the IFRAME within the rendered panel to get the size just right.
        /// </summary>
        [Description("The height of an IFRAME if mode is IFrame related. Use this if you need to specifically size the IFRAME within the rendered panel to get the size just right."),
        DefaultValue(""), Category("Panel Display")]
        public Unit IFrameHeight
        {
            get { return _IFrameHeight; }
            set { _IFrameHeight = value; }
        }
        private Unit _IFrameHeight = Unit.Empty;

        /// <summary>
        /// Override to force simple IDs all around
        /// </summary>
        public override string UniqueID
        {
            get
            {
                if (OverrideClientID)
                    return ID;
                return base.UniqueID;
            }
        }

        /// <summary>
        /// Override to force simple IDs all around
        /// </summary>
        public override string ClientID
        {
            get
            {
                if (OverrideClientID)
                    return ID;
                return base.ClientID;
            }
        }

        /// <summary>
        /// Determines whether ClientID and UniqueID values are returned
        /// as just as the ID or use full naming container syntax.
        /// 
        /// The default is true which returns the simple ID without
        /// naming container prefixes.
        /// </summary>
        [Description("Determines whether ClientID and UniqueID include naming container prefixes. True means simple ID is used, false uses Naming Container names."),
         Category("HoverPanel"), DefaultValue(true)]
        public bool OverrideClientID
        {
            get { return _OverrideClientID; }
            set { _OverrideClientID = value; }
        }
        private bool _OverrideClientID = true;




        protected override void OnLoad(EventArgs e)
        {
            if (IsCallback)
            {
                HttpContext.Current.Response.Expires = -1;
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }
            base.OnLoad(e);
        }

        /// <summary>
        /// This method just builds the various JavaScript blocks as strings
        /// and assigns them to the ClientScript object.
        //
        /// MouseEvents to the panel to show/hide the panel on mouse out 
        /// operations.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            if (!IsCallback)
            {
                GenerateControlSpecificJavaScript();


                // On HoverPanel operation we need to hide the window when we move off
                if (EventHandlerMode == HoverEventHandlerModes.ShowHtmlAtMousePosition)
                {
                    if (AutoCloseHoverWindow)
                    {
                        // Register hover out/in behavior on startup
                        ScriptProxy.RegisterStartupScript(this, typeof(ControlResources), ClientID + "_hpStartup",
@"$().ready( function() { 
    $('#" + ClientID + @"')
        .hover( function() { " + ClientID + @".show(); },
                function() { " + ClientID + @".hide(); } );
});
", true);
                    }
                }
                else if (EventHandlerMode == HoverEventHandlerModes.ShowIFrameAtMousePosition ||
                         EventHandlerMode == HoverEventHandlerModes.ShowIFrameInPanel)
                {
                    LiteralControl Ctl = new LiteralControl();
                    Ctl.Text = "<iframe id='" + ClientID + "_IFrame' frameborder='0' width='" + Width.ToString() + "' height='" + IFrameHeight.ToString() + "'></iframe>";
                    Controls.Add(Ctl);
                }

                if (PanelOpacity != 1)
                {
                    ScriptProxy.RegisterStartupScript(this, typeof(ControlResources), ClientID + "_hpOpacity",
@"$().ready( function() {     
    $('#" + ClientID + @"').css('opacity'," + PanelOpacity.ToString(CultureInfo.InvariantCulture.NumberFormat) + @"');
});
", true);
                }
            }
            base.OnPreRender(e);
        }


        /// <summary>
        /// Generates the ControlSpecific JavaScript. This script is safe to
        /// allow multiple callbacks to run simultaneously.
        /// </summary>
        private void GenerateControlSpecificJavaScript()
        {
            // Figure out the initial URL we're going to 
            // Either it's the provided URL from the control or 
            // we're posting back to the current page
            string Url = null;
            if (ServerUrl == null || ServerUrl == "")
                Url = Context.Request.Path;
            else
                Url = ResolveUrl(ServerUrl);


            //Uri ExistingUrl = Context.Request.Url;

            //// Must fix up URL into fully qualified URL for XmlHttp
            //if (!ServerUrl.ToLower().StartsWith("http"))
            //    Url = ExistingUrl.Scheme + "://" + ExistingUrl.Authority + Url;

            string CallbackHandler = ClientCompleteHandler;
            if (string.IsNullOrEmpty(CallbackHandler))
                CallbackHandler = "null";

            string StartupCode =
"function " + ClientID + @"_GetHoverPanel() {
    var hover = new HoverPanel(""#" + ClientID + @""");
    hover.serverUrl = """ + Url + @""";
    hover.completed = " + CallbackHandler + @";
    hover.htmlTargetId = """ + (HtmlTargetClientId == "" ? ClientID : HtmlTargetClientId) + @""";
    hover.postbackMode = """ + PostBackMode.ToString() + @""";
    hover.navigateDelay = " + NavigateDelay.ToString() + @";
    hover.adjustWindowPosition = " + AdjustWindowPosition.ToString().ToLower() + @";
    hover.eventHandlerMode = """ + EventHandlerMode.ToString() + @""";
    hover.shadowOpacity = " + ShadowOpacity.ToString(CultureInfo.InvariantCulture.NumberFormat) + @";
    hover.shadowOffset = " + ShadowOffset + @";
    hover.hoverOffsetRight = " + HoverOffsetRight.ToString() + @";
    hover.hoverOffsetBottom = " + HoverOffsetBottom.ToString() + @";
    return hover;
}
$().ready( function() { 
    window." + ClientID + " = " + ClientID + @"_GetHoverPanel();
});
";
            ScriptProxy.RegisterStartupScript(this, GetType(), ClientID + "_STARTUP", StartupCode, true);
        }

        /// <summary>
        /// Returns an Event Callback reference string that can be used in Client
        /// script to initiate a callback request. 
        /// </summary>
        /// <param name="QueryStringExpression">
        /// An expression that is evaluated in script code and embedded as the second parameter.
        /// The value of this second parameter is interpreted as a QueryString to the URL that
        /// is fired in response to the request to the server.
        /// 
        /// This expression can be a static string or any value or expression that is in scope
        /// at the time of calling the event method. The expression must evaluate to a string
        ///  
        /// Example: 
        /// string GetCallbackEventReference("'CustomerId=' + forms[0].txtCustomerId.value + "'");
        ///  
        /// A callback event reference result looks like this:
        /// 
        /// ControlID_StartCallback(event,'CustomerId=_12312')
        /// </param>
        /// <returns></returns>
        public string GetCallbackEventReference(string QueryStringExpression)
        {
            if (QueryStringExpression == null)
                return "StartCallback('" + ClientID + "',event)";

            return "StartCallback('" + ClientID + "',event,'" + QueryStringExpression + "')";
        }


    }

    public enum HoverEventHandlerModes
    {

        /// <summary>
        /// Displays a hover window at the current mouse position. Calls a URL 
        /// specified in the ServerUrl property when the call is initiated. The call 
        /// initiation can add an additional queryString to specify 'parameters' for 
        /// the request.
        /// <seealso>Enumeration HoverEventHandlerModes</seealso>
        /// </summary>
        ShowHtmlAtMousePosition,
        /// <summary>
        /// Shows the result of the URL in the panel. Works like ShowHtmlInPanel
        /// except that the panel is not moved when the callback completes.
        /// </summary>
        ShowHtmlInPanel,
        /// <summary>
        /// Displays a URL in an IFRAME which is independent of the
        /// current page.
        /// </summary>
        ShowIFrameAtMousePosition,
        /// <summary>
        /// Shows an IFRAME in a panel
        /// </summary>
        ShowIFrameInPanel,
        /// <summary>
        /// Calls an external Page and returns the HTML result into the 
        /// ClientEventHandler specified for the control. This is a really high level 
        /// mechanism.
        /// <seealso>Enumeration HoverEventHandlerModes</seealso>
        /// </summary>        
        GetHttpResponse
    }
}
