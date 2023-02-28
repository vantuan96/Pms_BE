using System;

namespace GAPIT.MKT.Framework.Core
{
    public class WebConsts
    {
        public const string ERROR_INVALID_JSON_STRING = "The JSON string is invalid";
        public const string ERROR_CLASSWRAPPER_FORWCFASMX_REQUIRES_CLIENTPROXYTYPE = "In order to call a Wcf or ASMX service you have to specify ClientProxyTargetType of the Web Service instance.";

        public const string STR_JsonContentType = "application/json";
        public const string STR_JavaScriptContentType = "application/x-javascript";
        public const string STR_UrlEncodedContentType = "application/x-www-form-urlencoded";


        public const string JQUERY_SCRIPT_RESOURCE = "GAPIT.MKT.Framework.Core.Web.Resources.jquery.js";
        public const string WWJQUERY_SCRIPT_RESOURCE = "GAPIT.MKT.Framework.Core.Web.Resources.ww.jquery.js";
        
        [Obsolete("This library should no longer be used. Please use WWJQUERY_SCRIPT_RESOURCE instead")]
        public const string SCRIPTLIBRARY_SCRIPT_RESOURCE = "GAPIT.MKT.Framework.Core.Web.Resources.wwscriptlibrary.js";

         // Icon Resource Strings
        public const string INFO_ICON_RESOURCE = "GAPIT.MKT.Framework.Core.Web.Resources.info.gif";        
        public const string WARNING_ICON_RESOURCE = "GAPIT.MKT.Framework.Core.Web.Resources.warning.gif";
        public const string CLOSE_ICON_RESOURCE = "GAPIT.MKT.Framework.Core.Web.close.gif";
        public const string HELP_ICON_RESOURCE = "GAPIT.MKT.Framework.Core.Web.help.gif";
        public const string LOADING_ICON_RESOURCE = "GAPIT.MKT.Framework.Core.Web.loading.gif";
        public const string LOADING_SMALL_ICON_RESOURCE = "GAPIT.MKT.Framework.Core.Web.loading_small.gif";
    }
}
