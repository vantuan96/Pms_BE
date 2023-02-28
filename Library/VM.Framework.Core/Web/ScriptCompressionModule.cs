﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO.Compression;
using System.IO;
using System.Reflection;
using System.Web.Caching;
using System.Linq;
using System.Web.UI;


using GAPIT.MKT.Helpers;

namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// Module that handles compression of JavaScript resources using GZip and
    /// script optimization that strips comments and extra whitespace.
    /// 
    /// This module should be used in conjunction with
    /// ClientScriptProxy.RegisterClientScriptResource which sets up the proper URL
    /// formatting required for this module to handle requests. Format is:
    /// 
    /// wwSc.axd?r=ResourceName&amp;t=FullAssemblyName
    /// 
    /// The type parameter can be omitted if the resource lives in this assembly.
    /// 
    /// To configure the module in web.config (for pre-IIS7):
    /// &lt;&lt;code lang="XML"&gt;&gt;&lt;system.web&gt;
    /// 	&lt;httpModules&gt;
    /// 		&lt;add name="ScriptCompressionModule"
    /// type="Westwind.Web.ScriptCompressionModule,Westwind.Web"/&gt;
    /// 	&lt;/httpModules&gt;
    /// &lt;/system.web&gt;&lt;&lt;/code&gt;&gt;
    /// 
    /// For IIS 7 Integrated mode:
    /// &lt;&lt;code lang="XML"&gt;&gt;&lt;system.webServer&gt;
    ///   &lt;validation validateIntegratedModeConfiguration="false"/&gt;
    ///   &lt;modules&gt;
    ///     &lt;add name="ScriptCompressionModule"
    /// type="Westwind.Web.ScriptCompressionModule,Westwind.Web"/&gt;
    ///     &lt;/modules&gt;
    /// &lt;/system.webServer&gt;&lt;&lt;/code&gt;&gt;
    /// </summary>
    public class ScriptCompressionModule : IHttpModule
    {
        // The two supported compressible content type constants
        private const string STR_JavaScript_ContentType = "application/x-javascript";
        private const string STR_Css_ContentType = "text/css";

        /// <summary>
        /// Stores resource names and their associated content types for lookup
        /// </summary>
        private static List<string> WebResourceContentTypeCache = new List<string>();

        /// <summary>
        /// Global flag that is set when the module is first loaded by ASP.NET and 
        /// allows code to check whether the module is loaded.
        /// 
        /// Used by ClientScriptProxy to determine if the module is active and 
        /// available in the ASP.NET Pipeline.
        /// </summary>
        public static bool ScriptCompressionModuleActive = false;

        public void Init(HttpApplication context)
        {
            ScriptCompressionModuleActive = true;
            context.PostResolveRequestCache += new EventHandler(PostResolveRequestCache);
        }
        public void Dispose()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PostResolveRequestCache(object sender, EventArgs e)
        {
            HttpContext Context = HttpContext.Current;
            HttpRequest Request = Context.Request;

            // Skip over anything we don't care about immediately
            if (!Request.Url.LocalPath.ToLower().Contains("wwsc.axd"))
                return;

            HttpResponse Response = Context.Response;
            string acceptEncoding = Request.Headers["Accept-Encoding"];

            // Start by checking whether GZip is supported by client
            bool useGZip = false;
            if (!string.IsNullOrEmpty(acceptEncoding) &&
                acceptEncoding.ToLower().Contains("gzip"))
                useGZip = true;

            string resource = Request.QueryString["r"] ?? "";
            if (string.IsNullOrEmpty(resource))
            {
                SendErrorResponse("Invalid Resource");
                return;
            }
            //resource = Encoding.ASCII.GetString(Convert.FromBase64String(resource));


            //string contentType = STR_JavaScript_ContentType;
            //if (resource.ToLower().EndsWith(".css"))
            //    contentType = STR_Css_ContentType;

            // Create a cachekey and check whether it exists
            string cacheKey = Request.QueryString.ToString() + useGZip.ToString();

            WebResourceCacheItem cacheItem = Context.Cache[cacheKey] as WebResourceCacheItem;
            if (cacheItem != null)
            {
                // Yup - read cache and send to client
                SendTextOutput(cacheItem.Content, cacheItem.IsCompressed, cacheItem.ContentType);
                return;
            }
            cacheItem = new WebResourceCacheItem();

            // Retrieve information about resource embedded
            // Values are base64 encoded
            string resourceTypeName = Request.QueryString["t"];

            // Try to locate the assembly that houses the Resource
            Assembly resourceAssembly = null;

            // If no type is passed use the current assembly - otherwise
            // run through the loaded assemblies and try to find assembly
            if (string.IsNullOrEmpty(resourceTypeName))
                resourceAssembly = GetType().Assembly;
            else
            {
                Type t = ReflectionUtils.GetTypeFromName(resourceTypeName);
                if (t != null)
                {
                    resourceAssembly = t.Assembly;
                    if (resourceAssembly == null)
                    {
                        SendErrorResponse("Invalid Type Information");
                        return;
                    }
                }
            }

            // Look up the WebResource Attribute
            WebResourceAttribute[] attr = resourceAssembly.GetCustomAttributes(typeof(WebResourceAttribute), false) as WebResourceAttribute[];
            if (attr != null && attr.Length > 0)
                cacheItem.ContentType = attr.Where(at => at.WebResource == resource).First().ContentType;

            // otherwise default to javascript - primary use case
            if (cacheItem.ContentType == null)
                cacheItem.ContentType = STR_JavaScript_ContentType;

            // Load the script file as a string from Resources
            string script = "";
            using (Stream st = resourceAssembly.GetManifestResourceStream(resource))
            {
                StreamReader sr = new StreamReader(st, Encoding.Default);
                script = sr.ReadToEnd();
            }

            // Optimize the script by removing comment lines and stripping spaces
            // Only applies to JavaScript
            if (cacheItem.ContentType == STR_JavaScript_ContentType && !Context.IsDebuggingEnabled)
                script = OptimizeScript(script);

            // Now we're ready to create out output
            // Don't GZip unless at least 4k
            if (useGZip && script.Length > 4096)
            {
                cacheItem.Content = GZipMemory(script);
                cacheItem.IsCompressed = true;
            }
            else
            {
                cacheItem.Content = Encoding.UTF8.GetBytes(script);
                cacheItem.IsCompressed = false;
            }

            // Add into the cache
            Context.Cache.Add(cacheKey, cacheItem, null, DateTime.UtcNow.AddDays(1), TimeSpan.Zero, CacheItemPriority.High, null);

            // Write out to Response object with appropriate Client Cache settings
            SendTextOutput(cacheItem.Content, cacheItem.IsCompressed, cacheItem.ContentType);
        }


        /// <summary>
        /// Returns an error response to the client. Generates a 404 error
        /// </summary>
        /// <param name="Message"></param>
        private void SendErrorResponse(string Message)
        {
            if (!string.IsNullOrEmpty(Message))
                Message = "Invalid Web Resource";

            HttpContext Context = HttpContext.Current;

            Context.Response.StatusCode = 404;
            Context.Response.StatusDescription = Message;

            Context.Response.End();
        }

        /// <summary>
        /// Sends the output to the client using appropriate cache settings.
        /// Content should be already encoded and ready to be sent as binary.
        /// </summary>
        /// <param name="Output"></param>
        /// <param name="UseGZip"></param>
        private void SendOutput(byte[] Output, bool UseGZip)
        {
            SendTextOutput(Output, UseGZip, STR_JavaScript_ContentType);
        }

        private void SendTextOutput(byte[] Output, bool UseGZip, string contentType)
        {
            HttpResponse Response = HttpContext.Current.Response;
            Response.ContentType = contentType;
            Response.Charset = "utf-8";

            if (UseGZip)
                Response.AppendHeader("Content-Encoding", "gzip");

            //if (!HttpContext.Current.IsDebuggingEnabled)
            //{
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.ExpiresAbsolute = DateTime.UtcNow.AddDays(1);
            Response.Cache.SetLastModified(DateTime.UtcNow);
            //}

            Response.BinaryWrite(Output);
            Response.End();
        }

        /// <summary>
        /// Very basic script optimization to reduce size:
        /// Remove any leading white space and any lines starting
        /// with //. 
        /// </summary>
        /// <param name="Script"></param>
        /// <returns></returns>
        public static string OptimizeScript(string Script)
        {
            JavaScriptMinifier min = new JavaScriptMinifier();
            return min.MinifyString(Script);
        }


        /// <summary>
        /// Finds an assembly in the current loaded assembly list
        /// </summary>
        /// <param name="TypeName"></param>
        /// <returns></returns>
        private Assembly FindAssembly(string TypeName)
        {
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                string fn = ass.FullName;
                if (ass.FullName == TypeName)
                    return ass;
            }

            return null;
        }

        /// <summary>
        /// Takes a binary input buffer and GZip encodes the input
        /// </summary>
        /// <param name="Buffer"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(byte[] Buffer)
        {
            MemoryStream ms = new MemoryStream();

            GZipStream GZip = new GZipStream(ms, CompressionMode.Compress);

            GZip.Write(Buffer, 0, Buffer.Length);
            GZip.Close();

            byte[] Result = ms.ToArray();
            ms.Close();

            return Result;
        }

        /// <summary>
        /// Takes a string input and GZip encodes the input
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public static byte[] GZipMemory(string Input)
        {
            return GZipMemory(Encoding.UTF8.GetBytes(Input));
        }

        /// <summary>
        /// Embeds a link to a script resource into the page including the
        /// script tags. Uses Page.ClientScript so the link is embedded into
        /// the page content rather than the header.
        /// 
        /// Preferrably use ClientScriptProxy.RegisterClientScriptInclude instead
        /// as it provides more options (including placement in header)
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="resourceName"></param>        
        public static void RegisterClientScriptResource(Type type,
                                                         string resourceName)
        {
            if (ScriptCompressionModule.ScriptCompressionModuleActive)
            {
                Page page = HttpContext.Current.Handler as Page;
                if (page == null)
                    throw new InvalidOperationException("RegisterClientScriptResource must be called in the context of an ASP.NET Page.");

                string resName = HttpUtility.UrlEncode(resourceName);

                ClientScriptManager script = page.ClientScript;
                string baseUrl = page.ResolveUrl("~/wwSC.axd?");

                // Resources from this assembly don't need assembly name/id
                if (type.Assembly == typeof(ScriptCompressionModule).GetType().Assembly)
                    script.RegisterClientScriptInclude(type, baseUrl + "r=" + resName, baseUrl + "r=" + resName);
                else
                {
                    string url = string.Format(baseUrl + "r={0}&t={1}", resName, type.FullName);
                    string typName = HttpUtility.UrlEncode(type.FullName);
                    script.RegisterClientScriptInclude(type, url, url);
                }
            }
        }
        /// <summary>
        /// Works like GetWebResourceUrl but can be used with javascript resources
        /// to allow using of resource compression (if the module is loaded).
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public string GetClientScriptResourceUrl(Type type, string resourceName)
        {

            // If ScriptCompression Module through Web.config is loaded use it to compress 
            // script resources by using wcSC.axd Url the module intercepts
            if (ScriptCompressionModule.ScriptCompressionModuleActive)
            {
                string url = "~/wwSC.axd?r=" + HttpUtility.UrlEncode(resourceName);
                if (type.Assembly != GetType().Assembly)
                    url += "&t=" + HttpUtility.UrlEncode(type.FullName);

                return WebUtils.ResolveUrl(url);
            }

            Page page = HttpContext.Current.Handler as Page;
            if (page == null)
                throw new InvalidOperationException("GetClientScriptUrl must be called in the context of a page when the ScriptCompressionModule is not active.");

            return page.ClientScript.GetWebResourceUrl(type, resourceName);
        }
    }

    /// <summary>
    /// Item stored in the cache
    /// </summary>
    internal class WebResourceCacheItem
    {
        public byte[] Content = null;
        public string ContentType = "";
        public bool IsCompressed = false;
    }
}
