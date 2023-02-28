﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// Marker Attribute to be used on Callback methods. Signals
    /// parser that the method is allowed to be executed remotely
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CallbackMethodAttribute : Attribute
    {

        /// <summary>
        /// When set to true indicates that a string result returned to the
        /// client should not be encoded. This can be more efficient for 
        /// large string results passed back to the client when returning
        /// HTML or other plain text and avoids extra encoding and decoding.
        /// 
        /// The client can specify resultMode="string" to receive this data
        /// directly.
        /// </summary>
        public bool ReturnAsRawString
        {
            get { return _NoEncoding; }
            set { _NoEncoding = value; }
        }
        private bool _NoEncoding = false;


        /// <summary>
        /// Content Type used for results that are returned as Stream
        /// or raw values.
        /// </summary>
        public string ContentType
        {
            get { return _ContentType; }
            set { _ContentType = value; }
        }
        private string _ContentType = string.Empty;


        public CallbackMethodAttribute()
        {


        }

    }

    /// <summary>
    /// Special return type used to indicate that an exception was
    /// fired on the server. This object is JSON serialized and the
    /// client can check for Result.IsCallbackError to see if a 
    /// a failure occured on the server.
    /// </summary>
    [Serializable]
    public class CallbackException
    {
        public bool isCallbackError = true;
        public string message = "";
        public string stackTrace = null;
    }

    public enum PostBackModes
    {
        /// No Form data is posted (but there may still be some post state)
        None,

        /// <summary>
        /// No POST data is posted back to the server
        /// </summary>
        Get,
        /// <summary>
        /// Only standard POST data is posted back - ASP.NET Post stuff left out
        /// </summary>
        Post,
        /// <summary>
        /// Posts back POST data but skips ViewState and EventTargets
        /// </summary>
        PostNoViewstate,
        /// <summary>
        /// Posts only the method parameters and nothing else
        /// </summary>
        PostMethodParametersOnly
    }

    public enum JavaScriptCodeLocationTypes
    {
        /// <summary>
        /// Causes the Javascript code to be embedded into the page on every 
        /// generation. Fully self-contained.
        /// <seealso>Enumeration JavaScriptCodeLocationTypes</seealso>
        /// </summary>
        EmbeddedInPage,
        /// <summary>
        /// Keeps the .js file as an external file in the Web application. If this is 
        /// set you should set the &lt;&lt;%= TopicLink([ScriptLocation],[_1Q01F9K4D]) 
        /// %&gt;&gt; Property to point at the location of the file.
        /// 
        /// This option requires that you deploy the .js file with your application.
        /// <seealso>Enumeration JavaScriptCodeLocationTypes</seealso>
        /// </summary>
        ExternalFile,
        /// <summary>
        /// ASP.NET 2.0 option to generate a WebResource.axd call that feeds the .js 
        /// file to the client.
        /// <seealso>Enumeration JavaScriptCodeLocationTypes</seealso>
        /// </summary>
        WebResource,
        /// <summary>
        /// Don't include any script - assume the page owner will handle it all
        /// </summary>
        None
    }

    public enum ProxyClassGenerationModes
    {
        /// <summary>
        /// The proxy is generated inline of the page.
        /// </summary>
        Inline,
        /// <summary>
        /// No proxy is generated at all
        /// </summary>
        None,
        /// <summary>
        /// Works only with CallbackHandler implementations
        /// that run as handlers at a distinct URL.
        /// JsonCallbacks.ashx/jsdebug
        /// </summary>
        jsdebug
    }
}
