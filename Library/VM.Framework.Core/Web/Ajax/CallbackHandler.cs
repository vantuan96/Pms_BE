using System;
using System.Web;
using System.Text;
using System.Reflection;

using GAPIT.MKT.Helpers;

namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// CallbackHandler is an Http Handler base class that allows you to create a
    /// class with methods marked up with a [CallbackMethod] attribute which are 
    /// then exposed for remote execution. The handler routes the to the methods 
    /// and executes the call and returns the results - or any errors - as JSON 
    /// strings.
    /// 
    /// To use this service you need to create an HttpHandler (either .ASHX or a 
    /// class registered in web.config's httpHandler section) and then add methods 
    /// with the [CallbackMethod] attribute and that's it. The service accepts 
    /// inputs via query string and POST data.
    /// 
    /// If you use the AjaxMethodCallbackControl the process of calling service
    /// methods is fully automated including automatic client proxy creation and
    /// you can call methods with individual parameters mapped from client to server.
    /// 
    /// Alternately you can also use plain REST calls that pass either no parameters
    /// and purely access POST data, or pass a single JSON object that can act as 
    /// a single input parameter.
    /// 
    /// The service can be accessed with:
    /// 
    /// MyHandler.ashx?CallbackMethod=MethodToCall
    /// 
    /// POST data can then be passed in to act as parameters:
    /// 
    /// &lt;&lt;ul&gt;&gt;
    /// &lt;&lt;li&gt;&gt; &lt;&lt;b&gt;&gt;Raw Post Buffer&lt;&lt;/b&gt;&gt;
    /// You simply pass raw POST data that you can access with Request.Form in the 
    /// handler
    /// 
    /// &lt;&lt;li&gt;&gt; &lt;&lt;b&gt;&gt;JSON value or object 
    /// string&lt;&lt;/b&gt;&gt;
    /// Alternately you can set the content type to application/json and pass a 
    /// JSON string of a value or object which calls the server method with a 
    /// single parameter of matching type.
    /// &lt;&lt;/ul&gt;&gt;
    /// 
    /// For more information on how to call these handlers see 
    /// <see cref="_24I0VDWUR">Using CallbackHandler with REST Calls</see>.
    /// </summary>
    public class CallbackHandler : IHttpHandler
    {
        /// <summary>
        /// This handler is not thread-safe
        /// </summary>
        public bool IsReusable
        {
            get { return true; }
        }

        /// <summary>
        /// Handle the actual callback by deferring to JsonCallbackMethodProcessor()
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            // handle WCF/ASMX style type wrappers for handler implementation
            // returns a separate dynamic link with the JavaScript Service Proxy
            if (HttpContext.Current.Request.PathInfo == "/jsdebug" ||
                HttpContext.Current.Request.PathInfo == "/js")
            {
                GenerateClassWrapperForCallbackMethods();
                return;
            }

            // Pass off to the worker Callback Processor
            ICallbackMethodProcessor processor = new JsonCallbackMethodProcessor();

            // Process the inbound request and execute it on this 
            // Http Handler's methods 
            processor.ProcessCallbackMethodCall(this);
        }


        private void GenerateClassWrapperForCallbackMethods()
        {
            HttpRequest Request = HttpContext.Current.Request;
            HttpResponse Response = HttpContext.Current.Response;
            Type objectType = GetType();

            StringBuilder sb = new StringBuilder(2048);
            string nameSpace = GetType().Namespace;
            string typeId = GetType().Name;

            if (!string.IsNullOrEmpty(nameSpace))
            {
                if (true) //ClientScriptProxy.IsMsAjax())
                    sb.AppendLine("registerNamespace(\"" + nameSpace + "\");");

                sb.AppendLine(nameSpace + "." + typeId + " = { ");
            }
            else
                sb.AppendLine(typeId + " = { ");

            MethodInfo[] Methods = objectType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (MethodInfo Method in Methods)
            {
                if (Method.GetCustomAttributes(typeof(CallbackMethodAttribute), false).Length > 0)
                {
                    sb.Append("    " + Method.Name + ": function " + "(");

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
    }},
", typeId, Method.Name, ParameterList.TrimEnd(','));

                }
            }

            if (sb.Length > 2)
                sb.Length -= 3; // strip trailing ,\r\n                                    

            // End of class
            sb.Append("\r\n}\r\n");


            string Url = Request.Path.ToLower().Replace(Request.PathInfo, "");

            sb.Append(
"function " + typeId + @"_GetProxy() {
    var _cb = new AjaxMethodCallback('" + typeId + "','" + Url + @"');
    _cb.serverUrl = '" + Url + @"';
    _cb.postbackMode = 'PostMethodParametersOnly';    
    return _cb;
}    
");

            WebUtils.GZipEncodePage();

            Response.ContentType = WebConsts.STR_JavaScriptContentType;
            HttpContext.Current.Response.Write(sb.ToString());
        }


    }
}
