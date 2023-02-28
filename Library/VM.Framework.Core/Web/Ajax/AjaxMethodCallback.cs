﻿/*
 **************************************************************
 * AjaxMethodCallback Class
 **************************************************************
 * 
 * This ASP.NET Control class provides page and control level
 * callback to the current ASPX page. This makes it super simple
 * to hook up AJAX callbacks with literally a single method
 * call. This control handles routing the request to the server
 * and to the appropriate server side method. The control also
 * manages parameter marshalling via JSON to the server and 
 * JSON results including support for many complex types like
 * entity object, nested objects, collections, datatables and
 * datarows.    
 * 
 * The class works through using the [CallbackMethod] on
 * any Page or custom server or user control methods.
 * 
 * The class is accompanied by a client library contained in
 * ww.jQuery.js which matches the HoverPanel class on
 * the client. The client and server pieces communicate with
 * each other. The library also contains many utility functions.
 * 
 * Source Dependencies:
 * JSONSerializer.cs
 * ReflectionUtils.cs
 * 
 * The original client JavaScript JSON deserialization code 
 * file is based on:
 *     copyright: '(c)2005 JSON.org',
 *       license: 'http://www.crockford.com/JSON/license.html',
 * 
 * License:
 * MIT License
 * 
 * Version History:
 * Version 2.0
 * -----------
 * * Release
 **************************************************************  
*/


using System;
using System.Text;

using System.ComponentModel;
using System.Web.UI;

using System.Reflection;
using System.IO;
using System.Web.UI.Design;
using System.Drawing.Design;
using System.ServiceModel;
using System.Web.Services;


using GAPIT.MKT.Framework.Core.JSON;


namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// The AjaxMethodCallback control provides an easy mechanism for making page or 
    /// control level method callbacks from an HTML page. Working with both client 
    /// side and server side logic you can make remote method calls from the client
    ///  using two-way JSON serialization to access server side content from client
    ///  pages.
    /// Methods are mapped from server to the client with an optional client proxy 
    /// class that allows making direct method calls to the server. Methods on the 
    /// server are marked up with a [CallbackMethod] to indicate that the methods 
    /// are accessible for callbacks. These methods can be implemented on the page,
    ///  any user control, or custom server control. The latter also allows control
    ///  developers to dynamically route callbacks to their own controls.
    /// 
    /// Methods called use JSON to pass data and simple types, hierarchical 
    /// objects, arrays and IList based classes are supported for two-way 
    /// transfers. DataSets/DataTables/DataRows are support for downloading only at
    ///  this time.
    /// 
    /// This control makes accessing server side content as easy as calling a 
    /// single proxy method and implementing a single client side handler to 
    /// receive the result value as a strongly typed object.
    /// </summary>
    [DefaultProperty("Url"), Designer(typeof(AjaxMethodCallbackDesigner)),
    ToolboxData("<{0}:AjaxMethodCallback runat=\"server\" />")]
    public class AjaxMethodCallback : Control
    {
        /// <summary>
        /// Wrapper ClientScript used to allow use ScriptManager methods
        /// </summary>
        private ClientScriptProxy ClientScriptProxy;

        /// <summary>
        /// The Url to hit on the server for the callback to return the result. Note: Not used when doing a MethodCallback
        /// </summary>
        [Description("The Url to hit on the server for the callback to return the result. Empty posts back to the current page."),
        DefaultValue(""), Category("Callback")]
        [UrlProperty]
        public string ServerUrl
        {
            get { return _ServerUrl; }
            set { _ServerUrl = value; }
        }
        private string _ServerUrl = "";


        /// <summary>
        /// Timeout in milliseconds for the request.
        /// </summary>
        [Description("Timeout in milliseconds for the request.")]
        [Category("Miscellaneous"), DefaultValue(20000)]
        public int Timeout
        {
            get { return _Timeout; }
            set { _Timeout = value; }
        }
        private int _Timeout = 20000;


        /// <summary>
        /// Determines whether the current request is in a callback. 
        /// 
        /// This property is used internally to trap for method processing, but you can
        /// also use this in your page or control level code to determine whether you 
        /// need to do any special processing based on the callback situation.        
        /// </summary>
        [Browsable(false)]
        public bool IsCallback
        {
            get
            {
                if (!_IsCallback.HasValue)
                {
                    string Id = Context.Request.Params["__WWEVENTCALLBACK"] ??
                                Context.Request.Params["CallbackTarget"];
                    if (Id != null && Id == ID)
                    {
                        _IsCallback = true;
                        return true;
                    }
                    _IsCallback = false;
                    return false;
                }
                else
                {
                    if (!_IsCallback.HasValue)
                        return false;

                    return _IsCallback.Value;
                }
            }
        }
        private bool? _IsCallback = null;

        /// <summary>
        /// Detemines how data is sent back to the server on a method call.
        /// </summary>
        [Description("Detemines how data is sent back to the server on a method call."),
         DefaultValue(PostBackModes.PostMethodParametersOnly),
         Category("Callback")]
        public PostBackModes PostBackMode
        {
            get { return _PostBackMode; }
            set { _PostBackMode = value; }
        }
        private PostBackModes _PostBackMode = PostBackModes.PostMethodParametersOnly;


        /// <summary>
        /// The name of the form from which POST data is sent to the server if 
        /// posting back with form data. Empty value will post back all 
        /// form variables of all forms.
        /// </summary>
        [Description("The name of the form from which POST data is sent to the server if posting back with form data. Empty value will post back all form variables of all forms."),
         DefaultValue(""),
         Category("Callback")]
        public string PostBackFormName
        {
            get { return _PostBackFormName; }
            set { _PostBackFormName = value; }
        }
        private string _PostBackFormName = "";



        /// <summary>
        /// Determines when Page level callbacks are processed in the Page pipeline.
        /// By default processing occurs in Load, but you can opt to process  
        /// callbacks in Init for better performance if you don't rely on any page
        /// specific logic or POST values from controls. 
        /// </summary>
        [Description("Determines when Page level callbacks are processed in the Page pipeline.")]
        [Category("Callback")]
        [DefaultValue(typeof(CallbackProcessingModes), "PageLoad")]
        public CallbackProcessingModes PageProcessingMode
        {
            get { return _PageProcessingMode; }
            set { _PageProcessingMode = value; }
        }
        private CallbackProcessingModes _PageProcessingMode = CallbackProcessingModes.PageLoad;


        /// <summary>
        /// The type of service to call. For AjaxMethodCallback call the current page, 
        /// control or CallbackHandler. For WCF and ASMX point at ServerUrl at 
        /// a WCF or ASMX Service respectively.
        /// 
        /// This method affects only what type of client proxy is created to allow
        /// for straight method callbacks and for all but Page callbacks you'll need
        /// to set the ClientProxyTargetType in code.
        /// </summary>
        [Category("Callback")]
        [Description("The type of service to call. For AjaxMethodCallback call the current page, control or CallbackHandler. For WCF and ASMX point at ServerUrl at a WCF or ASMX Service respectively.")]
        [DefaultValue(typeof(AjaxMethodCallbackServiceTypes), "AjaxMethodCallback")]
        public AjaxMethodCallbackServiceTypes ServiceType
        {
            get { return _ServiceType; }
            set { _ServiceType = value; }
        }
        private AjaxMethodCallbackServiceTypes _ServiceType = AjaxMethodCallbackServiceTypes.AjaxMethodCallback;


        /// <summary>
        /// Determines how the date format is serialized into JSON.
        /// 
        /// ISO is used by default since latest browsers support this now.
        /// </summary>
        [Category("Callback")]
        [DefaultValue(typeof(JsonDateEncodingModes), "ISO")]
        [Description("Determines how the date format is serialized into JSON.")]
        public JsonDateEncodingModes JsonDateEncoding
        {
            get { return _JsonDateMode; }
            set { _JsonDateMode = value; }
        }
        private JsonDateEncodingModes _JsonDateMode = JsonDateEncodingModes.ISO;



        /// <summary>
        /// Determines where the ww.jquery.js resource is loaded from. WebResources, Url or an empty string (no resource loaded)
        /// </summary>
        [Description("Determines where the ww.jquery.js resource is loaded from. WebResources, Url or leave empty to do nothing"),
        DefaultValue("WebResource"), Category("Resources"),
        Editor("System.Web.UI.Design.UrlEditor", typeof(UITypeEditor))]
        public string ScriptLocation
        {
            get { return _ScriptLocation; }
            set { _ScriptLocation = value; }
        }
        private string _ScriptLocation = "WebResource";


        /// <summary>
        /// Determines where the jquery.js resource is loaded from. WebResources, Url or leave empty to do nothing
        /// </summary>
        [Description("Determines where the jquery.js resource is loaded from. WebResources, Url or leave empty to do nothing"),
        DefaultValue("WebResource"), Category("Resources"),
        Editor("System.Web.UI.Design.UrlEditor", typeof(UITypeEditor))]
        public string jQueryScriptLocation
        {
            get { return _jQueryScriptLocation; }
            set { _jQueryScriptLocation = value; }
        }
        private string _jQueryScriptLocation = "WebResource";



        /// <summary>
        /// An instance of the object that is going to handle the callbacks on the
        /// the server. 
        /// 
        /// If not set defaults to the Page.
        /// </summary>
        [Browsable(false)]
        public object TargetInstance { get; set; }

        /// <summary>
        /// A type that is used to generate the Client Proxy Javascript
        /// class that gets injected into the page with matching methods.
        /// 
        /// This property defaults to the current Page's type, but you can
        /// override it to any object's type that matches your TargetInstance.
        /// 
        /// You can pass null to indicate you don't want to generate a proxy
        /// and instead call Proxy.callMethod explicitly which is a little more
        /// light weight in terms of processing and Javascript generation.
        /// </summary>
        [Browsable(false)]
        public Type ClientProxyTargetType { get; set; }

        /// <summary>
        /// If true generates a proxy class that maps each of the methods exposed with 
        /// [CallbackMethod] on the current page, user control or control and exposes 
        /// it as a client side class. The name of the class will be the same as the ID
        ///  of the AjaxMethodCallback control.
        /// 
        /// By default the class is generated. If false the class is not generated and 
        /// you can use the client side AjaxMethodCallback object and use declaritive 
        /// code to create the method callbacks manually.
        /// <seealso>Class AjaxMethodCallback</seealso>
        /// </summary>
        [Description("If true generates a client proxy class that has the name of this control. Each method of the class matches the server methods exposed plus a callback and error client handler"),
         DefaultValue(true), Category("Callback")]
        public ProxyClassGenerationModes GenerateClientProxyClass
        {
            get { return _GenerateClientProxyClass; }
            set { _GenerateClientProxyClass = value; }
        }
        private ProxyClassGenerationModes _GenerateClientProxyClass = ProxyClassGenerationModes.Inline;


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
         Category("Callback"), DefaultValue(true)]
        public bool OverrideClientID
        {
            get { return _OverrideClientID; }
            set { _OverrideClientID = value; }
        }
        private bool _OverrideClientID = true;




        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!IsCallback)
            {
                // Default page 
                if (ClientProxyTargetType == null)
                    ClientProxyTargetType = Page.GetType();
                return;
            }

            if (PageProcessingMode == CallbackProcessingModes.PageInit)
                HandleCallback();

        }

        protected override void OnLoad(EventArgs e)
        {
            if (!IsCallback)
                return;

            if (PageProcessingMode == CallbackProcessingModes.PageLoad)
                HandleCallback();

        }

        protected void HandleCallback()
        {
            if (TargetInstance == null)
                TargetInstance = Page;

            // Delegate off handling to the Callback Processor 
            // which handles routing and calling the method and returning
            // a JSON response
            ICallbackMethodProcessor processor = new JsonCallbackMethodProcessor();
            processor.JsonDateEncoding = JsonDateEncoding;

            // Let the Processor do all the work of parsing parms, calling and returning result
            processor.ProcessCallbackMethodCall(TargetInstance);
        }

        /// <summary>
        /// This method just builds the various JavaScript blocks as strings
        /// and assigns them to the ClientScript object.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            if (!IsCallback)
            {
                ClientScriptProxy = ClientScriptProxy.Current;

                // If we're not in a callback provide script to client 
                ClientScriptProxy.LoadControlScript(this, jQueryScriptLocation, WebConsts.JQUERY_SCRIPT_RESOURCE, ScriptRenderModes.HeaderTop);
                ClientScriptProxy.LoadControlScript(this, ScriptLocation, WebConsts.WWJQUERY_SCRIPT_RESOURCE, ScriptRenderModes.Header);

                // Generate the class wrapper and class loader function
                GenerateControlSpecificJavaScript();
            }
            else
            {
                if (PageProcessingMode == CallbackProcessingModes.PagePreRender)
                    HandleCallback();
            }
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
            if (string.IsNullOrEmpty(ServerUrl))
                Url = Context.Request.Path;
            else
                Url = ResolveUrl(ServerUrl);

            Uri ExistingUrl = Context.Request.Url;

            // Must fix up URL into fully qualified URL for XmlHttp
            if (!ServerUrl.ToLower().StartsWith("http"))
                Url = ExistingUrl.Scheme + "://" + ExistingUrl.Authority + Url;

            GenerateClassWrapperForCallbackMethods();
        }


        /// <summary>
        /// Creates the JavaScript client side object that matches the 
        /// server side method signature. The JScript function maps
        /// to a CallMethod() call on the client.
        /// </summary>
        private void GenerateClassWrapperForCallbackMethods()
        {
            if (ServiceType != AjaxMethodCallbackServiceTypes.AjaxMethodCallback)
            {
                GenerateClassWrapperForWcfAndAsmx();
                return;
            }

            StringBuilder sb = new StringBuilder();

            if (GenerateClientProxyClass == ProxyClassGenerationModes.jsdebug)
            {
                ClientScriptProxy.Current.RegisterClientScriptInclude(this, typeof(ControlResources), ServerUrl.ToLower() + "/jsdebug", ScriptRenderModes.Script);
                return;
            }
            if (GenerateClientProxyClass == ProxyClassGenerationModes.Inline)
            {
                Type objectType = null;

                if (TargetInstance != null)
                    objectType = TargetInstance.GetType();
                else if (ClientProxyTargetType != null)
                    objectType = ClientProxyTargetType;                 
                // assume Page as default
                else
                    objectType = Page.GetType();

                sb.Append("var " + ID + " = { ");

                MethodInfo[] Methods = objectType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (MethodInfo Method in Methods)
                {
                    if (Method.GetCustomAttributes(typeof(CallbackMethodAttribute), false).Length > 0)
                    {
                        sb.Append("\r\n    " + Method.Name + ": function " + "(");

                        string ParameterList = "";
                        foreach (ParameterInfo Parm in Method.GetParameters())
                        {
                            ParameterList += Parm.Name + ",";
                        }
                        sb.Append(ParameterList + "completed,errorHandler)");

                        sb.AppendFormat(
    @"
    {{
        var _cb = {0}_GetProxy();
        _cb.callMethod(""{1}"",[{2}],completed,errorHandler);
        return _cb;           
    }},", ID, Method.Name, ParameterList.TrimEnd(','));

                    }
                }

                if (sb.Length > 0)
                    sb.Length--; // strip trailing ,

                // End of class
                sb.Append("\r\n}\r\n");
            }
            string Url = null;
            if (string.IsNullOrEmpty(ServerUrl))
                Url = Context.Request.Path;
            else
                Url = ResolveUrl(ServerUrl);

            sb.Append(
"function " + ID + @"_GetProxy() {
    var _cb = new AjaxMethodCallback('" + ID + "','" + Url + @"',
                                    { timeout: " + Timeout.ToString() + @",
                                      postbackMode: '" + PostBackMode.ToString() + @"',
                                      formName: '" + PostBackFormName + @"' 
                                    });
    return _cb;
}
");
            ClientScriptProxy.RegisterStartupScript(this, GetType(), ID + "_ClientProxy", sb.ToString(), true);
        }

        /// <summary>
        /// Create
        /// </summary>
        private void GenerateClassWrapperForWcfAndAsmx()
        {
            StringBuilder sb = new StringBuilder();

            if (GenerateClientProxyClass == ProxyClassGenerationModes.jsdebug)
            {
                ClientScriptProxy.Current.RegisterClientScriptInclude(this, typeof(ControlResources), ServerUrl.ToLower() + "/jsdebug", ScriptRenderModes.Script);
                return;
            }

            string serverUrl = ServerUrl;
            if (!serverUrl.EndsWith("/"))
            {
                serverUrl += "/";

                Type objectType = null;

                if (ClientProxyTargetType != null)
                    objectType = ClientProxyTargetType;
                else
                    throw new InvalidOperationException(WebConsts.ERROR_CLASSWRAPPER_FORWCFASMX_REQUIRES_CLIENTPROXYTYPE);

                sb.Append("var " + ID + " = { ");

                MethodInfo[] Methods = objectType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (MethodInfo Method in Methods)
                {
                    if ((ServiceType == AjaxMethodCallbackServiceTypes.Wcf && Method.GetCustomAttributes(typeof(OperationContractAttribute), false).Length > 0) ||
                         (ServiceType == AjaxMethodCallbackServiceTypes.Asmx && Method.GetCustomAttributes(typeof(WebMethodAttribute), false).Length > 0))
                    {
                        sb.Append("\r\n    " + Method.Name + ": function(");

                        string ParameterList = "";
                        foreach (ParameterInfo Parm in Method.GetParameters())
                        {
                            ParameterList += Parm.Name + ",";
                        }
                        sb.Append(ParameterList + "completed,errorHandler) ");

                        sb.AppendFormat(
    @"
    {{
        var _cb = new {0}_GetProxy();
        _cb.invoke(""{1}"",{{", ID, Method.Name);

                        string[] parms = ParameterList.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string parm in parms)
                        {
                            sb.Append(parm + ": " + parm + ",");
                        }

                        if (parms.Length > 0)
                            sb.Length--;  // remove last comma                        


                        sb.Append(@"},completed,errorHandler);
        return _cb;
    },");


                    }
                }

                if (sb.Length > 0)
                    sb.Length--; // strip trailing ,

                // End of class
                sb.Append("\r\n}\r\n");
            }
            string Url = null;
            if (string.IsNullOrEmpty(ServerUrl))
                Url = Context.Request.Path;
            else
                Url = ResolveUrl(ServerUrl);

            sb.Append(
"function " + ID + @"_GetProxy() {
    var _cb = new ServiceProxy('" + serverUrl + @"',
                               { timeout: " + Timeout.ToString() +
                                    @", isWcf: " + (ServiceType == AjaxMethodCallbackServiceTypes.Wcf).ToString().ToLower() +
                                    @"});
    return _cb;
}
");
            ClientScriptProxy.RegisterStartupScript(this, GetType(), ID + "_ClientProxy", sb.ToString(), true);
        }

        /// <summary>
        /// Can be called to create a new instance of the AjaxMethodCallbackControl
        /// and attach it to the current page. This can be useful for control developers
        /// so they don't have to manually add the control to the page.
        /// 
        /// The call to this method should be made pre-OnLoad() preferrably in OnInit().
        /// </summary>
        /// <param name="page">Instance of the Page object</param>
        /// <returns>An instance of the Callback Control</returns>
        public static AjaxMethodCallback CreateControlInstanceOnPage(Control control, object targetInstance)
        {
            AjaxMethodCallback callback = new AjaxMethodCallback();
            callback.Page = control.Page;
            callback.ID = control.ID + "_Callback";
            if (targetInstance != null)
                callback.TargetInstance = targetInstance;
            else
                callback.TargetInstance = control;
            control.Controls.Add(callback);
            return callback;
        }

        /// <summary>
        /// Can be called to create a new instance of the AjaxMethodCallbackControl
        /// and attach it to the current page. This can be useful for control developers
        /// so they don't have to manually add the control to the page.
        /// 
        /// The call to this method should be made pre-OnLoad() preferrably in OnInit().
        /// </summary>
        /// <param name="page">Instance of the Page object</param>
        /// <returns>An instance of the Callback Control</returns>
        public static AjaxMethodCallback CreateControlInstanceOnPage(Control control)
        {
            return CreateControlInstanceOnPage(control, null);
        }

    }

    /// <summary>
    /// Control designer used so we get a grey button display instead of the 
    /// default label display for the control.
    /// </summary>
    internal class AjaxMethodCallbackDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {

            return base.CreatePlaceHolderDesignTimeHtml("");
        }
    }

    /// <summary>
    /// Determines when page level callbacks are processed
    /// </summary>
    public enum CallbackProcessingModes
    {
        /// <summary>
        /// Provides best performance for page callbacks. No page logic accessible
        /// </summary>
        PageInit,
        /// <summary>
        /// Default behavior fires callback methods in Page Load after ViewState and 
        /// Form vars have been processed by the pageLoad
        /// </summary>
        PageLoad,
        /// <summary>
        /// Fires Callback method in PreRender after Load processing is completed.
        /// Note events on controls may or may not have fired yet.
        /// </summary>
        PagePreRender
    }

    /// <summary>
    /// Determines what kind of client proxy is created
    /// for you
    /// </summary>
    public enum AjaxMethodCallbackServiceTypes
    {
        AjaxMethodCallback,
        Wcf,
        Asmx
    }
}
