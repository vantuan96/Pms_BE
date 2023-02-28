﻿using System;
using System.Web;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Collections.Generic;


using GAPIT.MKT.Helpers;
using GAPIT.MKT.Framework.Core.JSON;

namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// Inteface that defines the Callback operations for handling method callbacks
    /// on the server.
    /// 
    /// This interface serves as an abstraction for potentially different implementations
    /// that use XML instead of JSON.
    /// </summary>
    public interface ICallbackMethodProcessor
    {
        JsonDateEncodingModes JsonDateEncoding { get; set; }

        /// <summary>
        /// Generic method that handles processing a Callback request by routing to
        /// a method in a provided target object.
        /// 
        /// </summary>
        /// <param name="target">The target object that is to be called. If null this is used</param>
        void ProcessCallbackMethodCall(object target);

        /// <summary>
        /// Returns an error response to the client from a callback. Code
        /// should exit after this call.
        /// </summary>
        /// <param name="ErrorMessage"></param>
        void WriteErrorResponse(string ErrorMessage, string stackTrace);
    }




    /// <summary>
    /// This is the core implementation of the JSON callback method handler that 
    /// picks up POST data from the request and uses it to call the actual callback
    ///  method on the specified object and return the results back as JSON.
    /// 
    /// This processor is generic and can be used easily from anywhere that needs 
    /// to feed back JSON data from a method callback, simply calling the 
    /// ProcessCallbackMethod() with an object that contains methods that are 
    /// marked up with the [CallbackMethod] attribute.
    /// 
    /// For example, wwCallbackHandler simply forwards all processing like this:
    /// 
    /// &lt;&lt;code lang="C#"&gt;&gt;public void ProcessRequest(HttpContext 
    /// context)
    /// {
    ///     // Pass off to the worker Callback Processor
    ///     ICallbackMethodProcessor processor = new JsonCallbackMethodProcessor();
    /// 
    ///     // Process the inbound request and execute it on this
    ///     // Http Handler's methods
    ///     processor.ProcessCallbackMethodCall(this);
    /// }&lt;&lt;/code&gt;&gt;
    /// 
    /// This processor is expected to execute in an environment where 
    /// HttpContext.Current is available and where POST data is available to 
    /// describe the incoming parameter data and method to call.
    /// </summary>
    public class JsonCallbackMethodProcessor : ICallbackMethodProcessor
    {

        public JsonDateEncodingModes JsonDateEncoding
        {
            get { return _JsonDateEncoding; }
            set { _JsonDateEncoding = value; }
        }
        private JsonDateEncodingModes _JsonDateEncoding = JsonDateEncodingModes.ISO;


        /// <summary>
        /// Generic method that handles processing a Callback request by routing to
        /// a method in a provided target object.
        /// 
        /// </summary>
        /// <param name="target">The target object that is to be called. If null this is used</param>
        public void ProcessCallbackMethodCall(object target, string methodToCall)
        {
            if (target == null)
                target = this;

            HttpRequest Request = HttpContext.Current.Request;
            HttpResponse Response = HttpContext.Current.Response;
            Response.Charset = null;

            CallbackMethodProcessorHelper helper = new CallbackMethodProcessorHelper(this);

            List<string> ParameterList = null;

            string contentType = Request.ContentType.ToLower();

            // Allow for a single JSON object to be POSTed rather than POST variables
            if (contentType.StartsWith(WebConsts.STR_JavaScriptContentType) ||
                contentType.StartsWith(WebConsts.STR_JsonContentType))
            {
                if (string.IsNullOrEmpty(methodToCall))
                    methodToCall = Request.Params["Method"];

                if (string.IsNullOrEmpty(methodToCall))
                {
                    WriteErrorResponse("No method to call specified.", null);
                    return;
                }

                // Pass a Parameter List with our JSON encoded parameters
                ParameterList = new List<string>();

                if (Request.ContentLength > 0L)
                {
                    // Pick up single unencoded JSON parameter
                    StreamReader sr = new StreamReader(Request.InputStream);
                    string singleParm = sr.ReadToEnd();
                    sr.Close();

                    if (!string.IsNullOrEmpty(singleParm))
                        ParameterList.Add(singleParm);
                }
            }
            // Post AjaxMethodCallback style interface            
            else if (contentType.StartsWith(WebConsts.STR_UrlEncodedContentType) && Request.Params["CallbackMethod"] != null)
                // Only pick up the method name - Parameters are parsed out of POST buffer during method calling
                methodToCall = Request.Params["CallbackMethod"];
            else
            {
                if (string.IsNullOrEmpty(methodToCall))
                    methodToCall = Request.QueryString["Method"];

                if (string.IsNullOrEmpty(methodToCall))
                {
                    WriteErrorResponse("No method to call specified.", null);
                    return;
                }
            }

            object Result = null;
            string StringResult = null;
            CallbackMethodAttribute attr = new CallbackMethodAttribute();
            try
            {
                if (ParameterList != null)
                    // use the supplied parameter list
                    Result = helper.ExecuteMethod(methodToCall, target, ParameterList.ToArray(), ref attr);
                else
                    // grab the info out of QueryString Values or POST buffer during parameter parsing 
                    // for optimization
                    Result = helper.ExecuteMethod(methodToCall, target, null, ref attr);
            }
            catch (Exception ex)
            {
                Exception ActiveException = null;
                if (ex.InnerException != null)
                    ActiveException = ex.InnerException;
                else
                    ActiveException = ex;

                WriteErrorResponse(ActiveException.Message,
                                  (HttpContext.Current.IsDebuggingEnabled ? ex.StackTrace : null));
                return;
            }

            string format = (Request.QueryString["ResultFormat"] ?? "").ToLower();

            // Stream data is just sent back RAW as is. Method code should set ContentType
            if (Result is Stream)
            {
                if (!string.IsNullOrEmpty(attr.ContentType))
                    Response.ContentType = attr.ContentType;

                FileUtils.CopyStream((Stream)Result, Response.OutputStream, 4092);
                Response.End();
                return;
            }

            // Raw string result option eith via querystring or CallbackMethod Attribute
            if ((format == "string" || attr.ReturnAsRawString) && Result.GetType() == typeof(string))
            {
                if (!string.IsNullOrEmpty(attr.ContentType))
                    Response.ContentType = attr.ContentType;

                StringResult = Result as string;

                Response.Write(StringResult);
                Response.End();
                return;
            }
            // Standard json formatting
            else
            {
                try
                {
                    JSONSerializer Serializer = new JSONSerializer();
                    Serializer.DateSerializationMode = JsonDateEncoding;

                    // In debug mode show nicely formatted JSON 
                    // In release normal packed JSON is used
                    if (HttpContext.Current.IsDebuggingEnabled)
                        Serializer.FormatJsonOutput = true;

                    StringResult = Serializer.Serialize(Result);
                }
                catch (Exception ex)
                {
                    WriteErrorResponse(ex.Message, HttpContext.Current.IsDebuggingEnabled ? ex.StackTrace : null);
                    return;
                }
            }

            // Explicitly set the content type here
            Response.ContentType = WebConsts.STR_JsonContentType;

            Response.Write(StringResult);
            Response.End();
        }

        /// <summary>
        /// Generic method that handles processing a Callback request by routing to
        /// a method in a provided target object.
        /// 
        /// This version doesn't pass in the method name but retrieves it from the
        /// POST data or query string.
        /// </summary>
        /// <param name="target"></param>
        public void ProcessCallbackMethodCall(object target)
        {
            ProcessCallbackMethodCall(target, null);
        }

        /// <summary>
        /// Returns an error response to the client from a callback. Code
        /// should exit after this call.
        /// </summary>
        /// <param name="ErrorMessage"></param>
        public void WriteErrorResponse(string errorMessage, string stackTrace)
        {
            CallbackException Error = new CallbackException();
            Error.message = errorMessage;
            Error.isCallbackError = true;
            Error.stackTrace = stackTrace;

            JSONSerializer Serializer = new JSONSerializer();
            string Result = Serializer.Serialize(Error);

            HttpResponse Response = HttpContext.Current.Response;
            Response.ContentType = WebConsts.STR_JsonContentType;

            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = 500;
            Response.Write(Result);
            //HttpContext.Current.ApplicationInstance.CompleteRequest();
            Response.End();
        }
    }




    /// <summary>
    /// This class provides helper services to the CallbackProcessor classes with 
    /// the generic services that deal with method execution and parsing POST 
    /// parameters which should be independent of the specific implementation 
    /// (JSON, XML etc.).
    /// 
    /// Extracted here so other Callback Processors can be created more easily 
    /// later on, using other request formats (Xml etc.)
    /// </summary>
    internal class CallbackMethodProcessorHelper
    {
        private ICallbackMethodProcessor Processor = null;

        public CallbackMethodProcessorHelper(ICallbackMethodProcessor processor)
        {
            Processor = processor;
        }

        /// <summary>
        /// Executes the requested method. 
        /// to the proper types for execution.
        /// </summary>
        /// <param name="Method"></param>
        /// <param name="methodParameters">An array of the string json parameters to </param>
        /// <param name="target">The object to call the method on</param>
        /// <param name="jsonParameters">An array of json Strings that make up the parameters for the method call. This value can be null in which case parms are parsed out of GET QueryString or POST values</param>
        /// <param name="callbackMethodAttribute">An optional instance of an CallbackAttribute that is set by this method</param>
        /// <returns>the result of the method execution</returns>
        internal object ExecuteMethod(string Method, object target, string[] jsonParameters, ref CallbackMethodAttribute callbackMethodAttribute)
        {
            HttpRequest Request = HttpContext.Current.Request;
            object Result = null;

            // Stores parsed parameters (from string JSON or QUeryString Values)
            object[] adjustedParms = null;

            Type PageType = target.GetType();
            MethodInfo MI = PageType.GetMethod(Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (MI == null)
                throw new InvalidOperationException("Invalid Server Method.");

            object[] methods = MI.GetCustomAttributes(typeof(CallbackMethodAttribute), false);
            if (methods.Length < 1)
                throw new InvalidOperationException("Server method is not accessible due to missing CallbackMethod attribute");

            if (callbackMethodAttribute != null)
                callbackMethodAttribute = methods[0] as CallbackMethodAttribute;

            ParameterInfo[] parms = MI.GetParameters();

            JSONSerializer serializer = new JSONSerializer();

            int parmCounter = 0;
            adjustedParms = new object[parms.Length];
            foreach (ParameterInfo parameter in parms)
            {
                // Retrieve parameters out of QueryString or POST buffer
                if (jsonParameters == null)
                {
                    // GET parameter are parsed as plain string values - no JSON encoding
                    if (HttpContext.Current.Request.HttpMethod == "GET")
                    {
                        // Look up the parameter by name
                        string parmString = Request.QueryString[parameter.Name];
                        adjustedParms[parmCounter] = ReflectionUtils.StringToTypedValue(parmString, parameter.ParameterType);
                    }
                    // POST parameters are treated as methodParameters that are JSON encoded
                    else
                        //string newVariable = methodParameters.GetValue(parmCounter) as string;
                        adjustedParms[parmCounter] = serializer.Deserialize(Request.Params["parm" + (parmCounter + 1).ToString()], parameter.ParameterType);
                }
                else
                    adjustedParms[parmCounter] = serializer.Deserialize(jsonParameters[parmCounter], parameter.ParameterType);

                parmCounter++;
            }
            Result = target.GetType().GetMethod(Method, BindingFlags.Instance |
                                                        BindingFlags.Public |
                                                        BindingFlags.NonPublic).Invoke(target, adjustedParms);

            return Result;
        }

        ///// <summary>
        ///// Executes the requested method. converts the String parameters
        ///// to the proper types for execution.
        ///// </summary>
        ///// <param name="Method">The method name to call</param>
        ///// <param name="ParameterList">An array of json encoded string parameters</param>
        ///// <param name="target">The object to call the method on</param>
        ///// <returns>the result of the method execution</returns>
        //internal object ExecuteMethodx(string method, string[] parameterList, object target)
        //{
        //    return ExecuteMethod(method, parameterList, target);
        //}
    }

}
