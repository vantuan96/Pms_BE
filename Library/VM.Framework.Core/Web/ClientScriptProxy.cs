// undefine this constant to remove the code that allows for
// optional script compression with the ScriptingModule
// when MS Ajax is not available.
#define IncludeScriptCompressionModuleSupport

using System;
using System.Web;
using System.Web.UI;
using System.Reflection;
using System.Text;
using System.IO;
using System.Collections.Generic;


using GAPIT.MKT.Helpers;

namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// This is a proxy object for the Page.ClientScript and MS Ajax ScriptManager 
    /// object that can operate when MS Ajax when present otherwise falling back to
    ///  Page.ClientScript. Because MS Ajax may not be available accessing the 
    /// methods directly is not possible and we are required to indirectly 
    /// reference client script methods through this class.
    /// 
    /// This class should be invoked at the Control's start up and be used to 
    /// replace all calls Page.ClientScript. Scriptmanager calls are made through 
    /// Reflection indirectly so there's no dependency on the script manager.
    /// 
    /// This class also provides a few additional page injection helpers like the 
    /// abillity to load scripts in the page header (rather than in the body) and 
    /// to use script compression using wwScriptCompressionModule without using MS 
    /// Ajax.
    /// </summary>
    public class ClientScriptProxy
    {
        private const string STR_CONTEXTID = "__ClientScriptProxy";

        private static Type scriptManagerType = null;

        // Register proxied methods of ScriptManager
        private static MethodInfo RegisterClientScriptBlockMethod;
        private static MethodInfo RegisterStartupScriptMethod;
        private static MethodInfo RegisterClientScriptIncludeMethod;
        private static MethodInfo RegisterClientScriptResourceMethod;
        private static MethodInfo RegisterHiddenFieldMethod;
        private static MethodInfo GetCurrentMethod;

        //private static MethodInfo RegisterPostBackControlMethod;
        //private static MethodInfo GetWebResourceUrlMethod;

        /// <summary>
        /// Determines the default script rendering mode that is uses if no script rendering mode
        /// is explicitly provided on the control.
        /// 
        /// This setting is global and should be set only once in Appplication_Start or
        /// a static constructor.
        /// </summary>
        public static ScriptRenderModes DefaultScriptRenderMode = ScriptRenderModes.Script;

        /// <summary>
        /// List of ResourceToFile map items that map  script resources loaded via Resources to just
        /// script filenames. These filenames can be compared against real file based scripts
        /// (when loaded through ScriptContainer) to be detected and only be loaded once.
        /// 
        /// Note this is a List<> because there may be multiple supported values for a single
        /// resource (ie. not unique).
        /// </summary>
        public static List<ScriptResourceAlias> ScriptResourceAliases = new List<ScriptResourceAlias>() 
        { 
            new ScriptResourceAlias() { Alias = "jquery", Resource = WebConsts.JQUERY_SCRIPT_RESOURCE, FileId="jquery.js"},
            new ScriptResourceAlias() { Alias = "ww.jquery", Resource = WebConsts.WWJQUERY_SCRIPT_RESOURCE, FileId = "ww.jquery.js" },            
            new ScriptResourceAlias() { Alias = "jquery.js", Resource = WebConsts.JQUERY_SCRIPT_RESOURCE, FileId="jquery.js"},
            new ScriptResourceAlias() { Alias = "ww.jquery.js", Resource = WebConsts.WWJQUERY_SCRIPT_RESOURCE, FileId = "ww.jquery.js" },            

            // The following are the min.js versions - just an alias so we can find them
            new ScriptResourceAlias() { Alias = "NONE", Resource = WebConsts.JQUERY_SCRIPT_RESOURCE, FileId = "jquery.min.js" },
            new ScriptResourceAlias() { Alias = "NONE", Resource = WebConsts.WWJQUERY_SCRIPT_RESOURCE, FileId = "ww.jquery.min.js" }
        };




        /// <summary>
        /// Internal global static that gets set when IsMsAjax() is
        /// called. The result is cached once per application so 
        /// we don't have keep making reflection calls for each access
        /// </summary>
        private static bool _IsMsAjax = false;

        /// <summary>
        /// Flag that determines whether check was previously done
        /// </summary>
        private static bool _CheckedForMsAjax = false;

        /// <summary>
        /// Cached value to see whether the script manager is
        /// on the page. This value caches here once per page.
        /// </summary>
        private bool _IsScriptManagerOnPage = false;
        private bool _CheckedForScriptManager = false;

        private List<string> _loadedScripts = new List<string>();

        /// <summary>
        /// Current instance of this class which should always be used to 
        /// access this object. There are no public constructors to
        /// ensure the reference is used as a Singleton to further
        /// ensure that all scripts are written to the same clientscript
        /// manager.
        /// </summary>        
        public static ClientScriptProxy Current
        {
            get
            {
                if (HttpContext.Current == null)
                    return new ClientScriptProxy();

                if (HttpContext.Current.Items.Contains(STR_CONTEXTID))
                    return HttpContext.Current.Items[STR_CONTEXTID] as ClientScriptProxy;

                ClientScriptProxy proxy = new ClientScriptProxy();
                HttpContext.Current.Items[STR_CONTEXTID] = proxy;
                return proxy;
            }
        }

        /// <summary>
        /// No public constructor - use ClientScriptProxy.Current to
        /// get an instance to ensure you once have one instance per
        /// page active.
        /// </summary>
        protected ClientScriptProxy()
        {
        }


        #region ScriptManager Detection routines
        /// <summary>
        /// Checks to see if MS Ajax is registered with the current
        /// Web application.
        /// 
        /// Note: Method is static so it can be directly accessed from
        /// anywhere. If you use the IsMsAjax property to check the
        /// value this method fires only once per application.
        /// </summary>
        /// <returns></returns>
        public static bool IsMsAjax()
        {
            if (_CheckedForMsAjax)
                return _IsMsAjax;

            // Easiest but we don't want to hardcode the version here
            // scriptManagerType = Type.GetType("System.Web.UI.ScriptManager, System.Web.Extensions, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", false);

            // To be safe and compliant we need to look through all loaded assemblies            
            Assembly ScriptAssembly = null; // Assembly.LoadWithPartialName("System.Web.Extensions");
            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                string fn = ass.FullName;
                if (fn.StartsWith("System.Web.Extensions"))
                {
                    ScriptAssembly = ass;
                    break;
                }
            }

            if (ScriptAssembly == null)
                return false;

            scriptManagerType = ScriptAssembly.GetType("System.Web.UI.ScriptManager");

            if (scriptManagerType == null)
            {
                _IsMsAjax = false;
                _CheckedForMsAjax = true;
                return false;
            }

            // Method to check for current instance on a page - cache
            // since we might call this frequently
            GetCurrentMethod = scriptManagerType.GetMethod("GetCurrent");

            _IsMsAjax = true;
            _CheckedForMsAjax = true;

            return true;
        }

        /// <summary>
        /// Checks to see if a script manager is on the page
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public bool IsScriptManagerOnPage(Page page)
        {
            // Check is done only once per page
            if (_CheckedForScriptManager)
                return _IsScriptManagerOnPage;

            // Must check whether MS Ajax is available
            // at all first. Method sets up scriptManager
            // and GetCurrentMethod on success.
            if (!IsMsAjax())
            {
                _CheckedForScriptManager = true;
                _IsScriptManagerOnPage = false;
                return false;
            }

            // Now check and see if we can get a ref to the script manager
            object sm = GetCurrentMethod.Invoke(null, new object[1] { page });
            if (sm == null)
                _IsScriptManagerOnPage = false;
            else
                _IsScriptManagerOnPage = true;

            _CheckedForScriptManager = true;
            return _IsScriptManagerOnPage;
        }
        #endregion


        /// <summary>
        /// High level helper function  that is used to load script resources for various AJAX controls
        /// Loads a script resource based on the following scriptLocation values:
        /// 
        /// * WebResource
        ///   Loads the Web Resource specified out of ControlResources. Specify the resource
        ///   that applied in the resourceName parameter
        ///   
        /// * Url/RelativeUrl
        ///   loads the url with ResolveUrl applied
        ///   
        /// * empty string (no value) 
        ///   No action is taken and nothing is embedded into the page. Use this if you manually
        ///   want to load resources
        /// </summary>
        /// <param name="control">The control instance for which the resource is to be loaded</param>
        /// <param name="scriptLocation">WebResource, a virtual path or a full Url. Empty to not embed any script refs (ie. user loads script herself)</param>
        /// <param name="resourceName">The name of the resource when WebResource is used for scriptLocation null otherwise</param>
        /// <param name="topOfHeader">Determines if scripts are loaded into the header whether they load at the top or bottom</param>
        public void LoadControlScript(Control control, string scriptLocation, string resourceName, ScriptRenderModes renderMode)
        {
            // Specified nothing to do
            if (string.IsNullOrEmpty(scriptLocation))
                return;

            if (scriptLocation == "WebResource")
            {
                RegisterClientScriptResource(control, control.GetType(), resourceName, renderMode);
                return;
            }
            RegisterClientScriptInclude(control, control.GetType(),
                                        control.ResolveUrl(scriptLocation),
                                        renderMode);
        }
        public void LoadControlScript(Control control, string scriptLocation, string resourceName)
        {
            LoadControlScript(control, scriptLocation, resourceName, ScriptRenderModes.Inherit);
        }
        public void LoadControlScript(Control control, string scriptLocation)
        {
            LoadControlScript(control, scriptLocation, null, ScriptRenderModes.Inherit);
        }

        /// <summary>
        /// Returns a WebResource or ScriptResource URL for script resources that are to be
        /// embedded as script includes.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="resourceName"></param>
        public void RegisterClientScriptResource(Control control, Type type,
                                                 string resourceName, ScriptRenderModes renderMode)
        {

#if IncludeScriptCompressionModuleSupport
            // If ScriptCompression Module  is loaded use it to compress 
            // script resources by using wcSC.axd Url the module intercepts
            if (ScriptCompressionModule.ScriptCompressionModuleActive)
            {
                string resName = HttpUtility.UrlEncode(resourceName);

                // Resources from this assembly don't need assembly name/id
                if (type.Assembly == GetType().Assembly)
                    RegisterClientScriptInclude(control, type, "~/wwSC.axd?r=" + resName, renderMode);
                else
                {
                    string typName = HttpUtility.UrlEncode(type.FullName);
                    RegisterClientScriptInclude(control, type,
                                                string.Format("~/wwSC.axd?r={0}&t={1}", resName, typName),
                                                renderMode);
                }
                return;
            }
#endif

            if (IsScriptManagerOnPage(control.Page))
            {
                // NOTE: If MS Ajax is referenced, but no scriptmanager is on the page
                //           script no compression will occur. With a script manager
                //           on the page compression will eb handled by MS Ajax.
                if (RegisterClientScriptResourceMethod == null)
                    RegisterClientScriptResourceMethod = scriptManagerType.GetMethod("RegisterClientScriptResource",
                                                                 new Type[3] { typeof(Control), typeof(Type), typeof(string) });

                RegisterClientScriptResourceMethod.Invoke(null, new object[3] { control, type, resourceName });
                return;
            }

            //  Otherwise just embed a script reference into the page using ClientScript
            control.Page.ClientScript.RegisterClientScriptResource(type, resourceName);
        }



        /// <summary>
        /// Registers a script include tag into the page for an external script url.
        /// 
        /// This version embeds only in the body of the HTML document - ie. underneath the form tag      
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="url"></param>
        public void RegisterClientScriptInclude(Control control, Type type, string key, string url)
        {
            url = WebUtils.ResolveUrl(url);

            if (IsScriptManagerOnPage(control.Page))
            {
                if (RegisterClientScriptIncludeMethod == null)
                    RegisterClientScriptIncludeMethod = scriptManagerType.GetMethod("RegisterClientScriptInclude", new Type[4] { typeof(Control), typeof(Type), typeof(string), typeof(string) });

                RegisterClientScriptIncludeMethod.Invoke(null, new object[4] { control, type, key, url });
            }
            else
                control.Page.ClientScript.RegisterClientScriptInclude(type, key, url);
        }


        /// <summary>
        /// Registers a client script reference in the header instead of inside the document
        /// before the form tag. 
        /// 
        /// The script tag is embedded at the bottom of the HTML header.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="url"></param>
        /// <param name="bool loadAtTop">Determines if the resource is laoded at the to of the header or the bottom</param>
        public void RegisterClientScriptInclude(Control control, Type type, string url, ScriptRenderModes renderMode)
        {
            if (string.IsNullOrEmpty(url))
                return;

            if (renderMode == ScriptRenderModes.Inherit)
                renderMode = DefaultScriptRenderMode;

            // Extract just the script filename
            string fileId = null;


            // Check resource IDs and try to match to mapped file resources
            // Used to allow scripts not to be loaded more than once whether
            // embedded manually (script tag) or via resources with ClientScriptProxy
            if (url.Contains(".axd?r="))
            {
                string res = HttpUtility.UrlDecode(StringHelper.ExtractString(url, "?r=", "&", false, true));
                foreach (ScriptResourceAlias item in ScriptResourceAliases)
                {
                    if (item.Resource == res)
                    {
                        fileId = item.Alias + ".js";
                        break;
                    }
                }
                if (fileId == null)
                    fileId = url.ToLower();
            }
            else
                fileId = Path.GetFileName(url).ToLower();

            // No dupes - ref script include only once
            const string identifier = "script_";
            if (HttpContext.Current.Items.Contains(identifier + fileId))
                return;

            HttpContext.Current.Items.Add(identifier + fileId, string.Empty);


            if (control.Page.Header == null || renderMode == ScriptRenderModes.Script || renderMode == ScriptRenderModes.Inline)
            {
                RegisterClientScriptInclude(control, type, url, url);
                return;
            }

            // Retrieve script index in header
            object val = HttpContext.Current.Items["__ScriptResourceIndex"];
            int index = 0;
            if (val != null)
                index = (int)val;

            StringBuilder sb = new StringBuilder(256);

            url = WebUtils.ResolveUrl(url);

            // Embed in header
            sb.AppendLine(@"<script src=""" + url + @""" type=""text/javascript""></script>");

            if (renderMode == ScriptRenderModes.HeaderTop)
                control.Page.Header.Controls.AddAt(index, new LiteralControl(sb.ToString()));
            else
                control.Page.Header.Controls.Add(new LiteralControl(sb.ToString()));

            index++;
            HttpContext.Current.Items["__ScriptResourceIndex"] = index;
        }




        /// <summary>
        /// Registers a CSS Web Resource in the page
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="resourceName"></param>
        public void RegisterCssResource(Control control, Type type, string resourceName)
        {
            // Otherwise just embed a script reference into the page using standard page methods
            string resourceUrl = GetClientScriptResourceUrl(control, type, resourceName);

            RegisterCssLink(control, type, resourceName, resourceUrl);
        }


        /// <summary>
        /// Registers a CSS stylesheet in the page header and if that's not accessible inside of the form tag.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="url"></param>
        public void RegisterCssLink(Control control, Type type, string key, string url)
        {

            if (string.IsNullOrEmpty(url))
                return;

            string lowerUrl = url.ToLower();
            if (cssLinks.Contains(lowerUrl))
                return;  // already added

            Control container = control.Page.Header;
            if (container == null)
                container = control.Page.Form;

            if (container == null)
                throw new InvalidOperationException("There's no header or form to add CSS to Register Resource CSS on the page.");

            string output = "<link href=\"" + control.ResolveUrl(url) + "\" rel=\"stylesheet\" type=\"text/css\" >\r\n";
            container.Controls.Add(new LiteralControl(output));

            cssLinks.Add(lowerUrl);
        }

        /// <summary>
        /// Keep track of keys that were written
        /// </summary>
        private HashSet<string> cssLinks = new HashSet<string>();


        /// <summary>
        /// Registers a client script block in the page.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="script"></param>
        /// <param name="addScriptTags"></param>
        public void RegisterClientScriptBlock(Control control, Type type, string key, string script, bool addScriptTags)
        {
            if (IsMsAjax())
            {
                if (RegisterClientScriptBlockMethod == null)
                    RegisterClientScriptBlockMethod = scriptManagerType.GetMethod("RegisterClientScriptBlock", new Type[5] { typeof(Control), typeof(Type), typeof(string), typeof(string), typeof(bool) });

                RegisterClientScriptBlockMethod.Invoke(null, new object[5] { control, type, key, script, addScriptTags });
            }
            else
                control.Page.ClientScript.RegisterClientScriptBlock(type, key, script, addScriptTags);
        }

        /// <summary>
        /// Registers a startup code snippet that gets placed at the bottom of the page
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <param name="script"></param>
        /// <param name="addStartupTags"></param>
        public void RegisterStartupScript(Control control, Type type, string key, string script, bool addStartupTags)
        {
            if (IsMsAjax())
            {
                if (RegisterStartupScriptMethod == null)
                    RegisterStartupScriptMethod = scriptManagerType.GetMethod("RegisterStartupScript", new Type[5] { typeof(Control), typeof(Type), typeof(string), typeof(string), typeof(bool) });

                RegisterStartupScriptMethod.Invoke(null, new object[5] { control, type, key, script, addStartupTags });
            }
            else
                control.Page.ClientScript.RegisterStartupScript(type, key, script, addStartupTags);

        }
        /// <summary>
        /// Returns a WebResource URL for non script resources
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public string GetWebResourceUrl(Control control, Type type, string resourceName)
        {
            return control.Page.ClientScript.GetWebResourceUrl(type, resourceName);
        }

        /// <summary>
        /// Works like GetWebResourceUrl but can be used with javascript resources
        /// to allow using of resource compression (if the module is loaded).
        /// </summary>
        /// <param name="control"></param>
        /// <param name="type"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public string GetClientScriptResourceUrl(Control control, Type type, string resourceName)
        {

#if IncludeScriptCompressionModuleSupport

            // If wwScriptCompression Module through Web.config is loaded use it to compress 
            // script resources by using wcSC.axd Url the module intercepts
            if (ScriptCompressionModule.ScriptCompressionModuleActive)
            {
                string url = "~/wwSC.axd?r=" + HttpUtility.UrlEncode(resourceName);
                if (type.Assembly != GetType().Assembly)
                    url += "&t=" + HttpUtility.UrlEncode(type.FullName);

                return WebUtils.ResolveUrl(url);
            }

#endif

            return control.Page.ClientScript.GetWebResourceUrl(type, resourceName);
        }


        /// <summary>
        /// Injects a hidden field into the page
        /// </summary>
        /// <param name="control"></param>
        /// <param name="hiddenFieldName"></param>
        /// <param name="hiddenFieldInitialValue"></param>
        public void RegisterHiddenField(Control control, string hiddenFieldName, string hiddenFieldInitialValue)
        {
            if (IsMsAjax())
            {
                if (RegisterHiddenFieldMethod == null)
                    RegisterHiddenFieldMethod = scriptManagerType.GetMethod("RegisterHiddenField", new Type[3] { typeof(Control), typeof(string), typeof(string) });

                RegisterHiddenFieldMethod.Invoke(null, new object[3] { control, hiddenFieldName, hiddenFieldInitialValue });
            }
            else
                control.Page.ClientScript.RegisterHiddenField(hiddenFieldName, hiddenFieldInitialValue);
        }

    }



    /// <summary>
    /// Determines how scripts are included into the page
    /// </summary>
    public enum ScriptRenderModes
    {

        /// <summary>
        /// Inherits the setting from the control or from the ClientScript.DefaultScriptRenderMode
        /// </summary>
        Inherit,

        /// Renders the script include at the location of the control
        /// </summary>
        Inline,

        /// <summary>
        /// Renders the script include into the bottom of the header of the page
        /// </summary>
        Header,

        /// <summary>
        /// Renders the script include into the top of the header of the page
        /// </summary>
        HeaderTop,

        /// <summary>
        /// Uses ClientScript or ScriptManager to embed the script include
        /// </summary>
        Script
    }

    public struct ScriptResourceAlias
    {
        /// <summary>
        /// An alias/shortcut resource name
        /// </summary>
        public string Alias;


        /// <summary>
        /// The name of the script file that this resource maps to. Should be just
        /// the filename (ie. jquery.js or ww.jquery.js) as well as min.js versions
        /// if those files are loaded as well
        /// </summary>
        public string FileId;

        /// <summary>
        /// The full resource name to the resourceToFileItem
        /// </summary>
        public string Resource;

        /// <summary>
        /// Any type in the assembly that contains the script resource
        /// If null looks in the current executing assembly.
        /// </summary>
        public Type ControlType;
    }
}
