using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web;
using System.IO;
using System.Reflection;



[assembly: TagPrefix("GAPIT.MKT.Framework.Core", "ww")]

[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.jquery.js", "application/x-javascript")]
[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.ww.jquery.js", "application/x-javascript")]

[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.warning.gif", "image/gif")]
[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.info.gif", "image/gif")]
[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.loading.gif", "image/gif")]
[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.loading_small.gif", "image/gif")]
[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.close.gif", "image/gif")]
[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.help.gif", "image/gif")]

//[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.ui.datepicker.js", "text/javascript")]
//[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.ui.datepicker.css", "text/css")]
//[assembly: WebResource("GAPIT.MKT.Framework.Core.Web.Resources.calendar.gif", "image/gif")]


namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// Class is used as to consolidate access to resources
    /// </summary>
    public class ControlResources
    {       
        /// <summary>
        /// Loads the appropriate jScript library out of the scripts directory
        /// </summary>
        /// <param name="control"></param>
        public static void LoadjQuery(Control control, string jQueryUrl)
        {
            ClientScriptProxy p = ClientScriptProxy.Current;
            p.RegisterClientScriptResource(control, typeof(ControlResources), WebConsts.JQUERY_SCRIPT_RESOURCE, ScriptRenderModes.HeaderTop);

            return;
        }

        /// <summary>
        /// Loads the jQuery component uniquely into the page
        /// </summary>
        /// <param name="control"></param>
        /// <param name="jQueryUrl">Optional Url to the jQuery Library. NOTE: Should also have a .min version in place</param>
        public static void LoadjQuery(Control control)
        {
            LoadjQuery(control, null);
        }

        /// <summary>
        /// Loads the ww.jquery.js library from Resources at the end of the Html Header (if available)
        /// </summary>
        /// <param name="control"></param>
        /// <param name="loadjQuery"></param>
        public static void LoadwwjQuery(Control control, bool loadjQuery)
        {
            // jQuery is also required
            if (loadjQuery)
                LoadjQuery(control);

            ClientScriptProxy.Current.LoadControlScript(control, "WebResource", WebConsts.WWJQUERY_SCRIPT_RESOURCE, ScriptRenderModes.Header);
        }

        /// <summary>
        /// Loads the ww.jquery.js library from Resources at the end of the Html Header (if available)
        /// </summary>
        /// <param name="control"></param>
        public static void LoadwwjQuery(Control control)
        {
            LoadwwjQuery(control, true);
        }



        /// <summary>
        /// Returns a string resource from a given assembly.
        /// </summary>
        /// <param name="assembly">Assembly reference (ie. typeof(ControlResources).Assembly) </param>
        /// <param name="ResourceName">Name of the resource to retrieve</param>
        /// <returns></returns>
        public static string GetStringResource(Assembly assembly, string ResourceName)
        {
            Stream st = assembly.GetManifestResourceStream(ResourceName);
            StreamReader sr = new StreamReader(st);
            string content = sr.ReadToEnd();
            st.Close();
            return content;
        }

        /// <summary>
        /// Returns a string resource from the from the ControlResources Assembly
        /// </summary>
        /// <param name="ResourceName"></param>
        /// <returns></returns>
        public static string GetStringResource(string ResourceName)
        {
            return GetStringResource(typeof(ControlResources).Assembly, ResourceName);
        }



        #region ObsoleteCode

        /// <summary>
        /// Embeds the client script library into the page as a Resource
        /// </summary>
        /// <param name="page"></param>
        [Obsolete("wwScriptLibrary is no longer used. Please use LoadwwJquery instead")]
        public static void LoadwwScriptLibrary(Control control)
        {
            ClientScriptProxy.Current.LoadControlScript(control, "WebResource", WebConsts.SCRIPTLIBRARY_SCRIPT_RESOURCE);
        }

        #endregion
    }    
}
