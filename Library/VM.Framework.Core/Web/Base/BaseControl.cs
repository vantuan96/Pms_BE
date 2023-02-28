using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;
using System.IO;
using System.Web.Security;

using System.Threading;
using System.Globalization;
using log4net;

using GAPIT.MKT.Helpers;
using System.Web.UI.HtmlControls;

namespace GAPIT.MKT.Framework.Core
{
    /// <summary>
    /// The skined base user control represent an .ascx file
    /// </summary>
    public class BaseControl : UserControl
    {
        public static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseControl()
            : base()
        {

        }

        protected override void OnInit(EventArgs e)
        {            
            base.OnInit(e);                                    
        }


        /// <summary>
        /// This event handler adds the children controls.
        /// The CreateChildControls method is not listed in the table because it is called 
        /// whenever the ASP.NET page framework needs to create the controls tree 
        /// and this method call is not limited to a specific phase in a control's lifecycle. 
        /// For example, CreateChildControls can be invoked when loading a page, 
        /// during data binding, or during rendering.
        /// </summary>                
        protected override void CreateChildControls()
        {
            Control skin;

            // Load the skin
            skin = LoadSkin();

            // Initialize the skin
            InitializeSkin(skin);

            //Create child controls
            Controls.Add(skin);            
        }


        private void InitializeAjax()
        {
            //Ensure that all child controls were created
            this.EnsureChildControls();

            //Calculate javascript libraries included
            ScriptContainer Scripts = (ScriptContainer)WebUtils.FindControlRecursive(this.Page, "Scripts");

            if (!string.IsNullOrEmpty(IncludedScripts) && Scripts != null)
            {
                string[] scripts = IncludedScripts.Split(new char[] { ',' });

                ScriptItem item = null;

                foreach (string s in scripts)
                    if (!string.IsNullOrEmpty(s))
                    {
                        item = new ScriptItem();
                        item.AllowMinScript = false;
                        item.Src = s;
                        item.RenderMode = ScriptRenderModes.Header;
                        Scripts.AddScript(item);
                    }
            }                       


            //Calculate stylesheet included
            if (!string.IsNullOrEmpty(IncludedStyles))
            {                
                string[] styles = IncludedStyles.Split(new char[] { ',' });
                foreach (string s in styles)
                    if (!string.IsNullOrEmpty(s))
                    {
                        HtmlLink css = new HtmlLink();
                        css.Href = Page.ResolveUrl(s);
                        css.Attributes["rel"] = "stylesheet";
                        css.Attributes["type"] = "text/css";
                        css.Attributes["media"] = "all";
                        Page.Header.Controls.Add(css);
                    }
            }

            if (EnableAJAX)
            {
                // Create ScriptVariables container that allows pushing
                // variables into the page from server code.
                // Create client object "serverVars"
                ScriptVariables serverVars = new ScriptVariables(this.Page, "serverVars");

                // Make all ClientIds show as serverVars.controlId where Id is the postfix
                serverVars.AddClientIds(this, true);

                // Create an instance of the Callback control (adds to Controls collection of the passed control)
                AjaxMethodCallback callback = AjaxMethodCallback.CreateControlInstanceOnPage(this);
                callback.PageProcessingMode = CallbackProcessingModes.PageLoad;
                callback.PostBackMode = PostBackModes.Post;

                // point at the control/object that has [CallbackMethod] attributes to handle callbacks
                callback.TargetInstance = this;

                // Correct URL
                callback.ServerUrl = this.Request.RawUrl;

                if (Scripts != null)
                {
                    ScriptItem item = new ScriptItem();
                    item.AllowMinScript = false;
                    item.Src = CallbackJSFile;
                    item.RenderMode = ScriptRenderModes.Header;
                    Scripts.AddScript(item);
                }
            }
        }


        protected Control LoadSkin()
        {
            Control skin;

            // Do we have a skin?
            if (SkinFilename == null)
                throw new Exception("You must specify a skin.");

            string skinFullName = SkinFilePath + SkinFilename.TrimStart('/');

            // Attempt to load the control. If this fails, we're done
            try
            {
                skin = Page.LoadControl(skinFullName);
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Critical error: The skin file " + skinFullName + " could not be found. The skin must exist for this control to render.");
            }
            catch (Exception e)
            {
                throw e;
            }

            return skin;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void Page_Load(object sender, EventArgs args)
        {
            this.EnsureChildControls();

            InitializeAjax();

            LoadOnPage();
        }

        /// <summary>
        /// Initialize the control template and populate the control with values
        /// </summary>
        protected virtual void InitializeSkin(Control skin)
        {

        }

        /// <summary>
        /// Processing wih control data when the control is load in to the page.
        /// May be used for fill data into control        
        /// </summary>
        protected virtual void LoadOnPage()
        {

        }

        /// <summary>
        /// Allows the default control template to be overridden
        /// </summary>
        public string SkinFilename
        {
            get
            {
                if (ViewState["SkinFileName"] == null)
                    return String.Empty;
                return (string)ViewState["SkinFileName"];
            }
            set
            {
                ViewState["SkinFileName"] = value;
            }
        }


        /// <summary>
        /// The path of the skin file
        /// </summary>
        public string SkinFilePath
        {
            get
            {
                if (ViewState["SkinFilePath"] == null)
                    return "/";
                return (string)ViewState["SkinFilePath"];
            }
            set
            {
                ViewState["SkinFilePath"] = value;
            }
        }

        /// <summary>
        /// Duong dan den file xu ly Callback cho control neu EnableAjax duoc cho phep
        /// </summary>
        public string CallbackJSFile
        {
            get
            {
                if (ViewState["CallbackJSFile"] == null)
                    return "";
                return (string)ViewState["CallbackJSFile"];
            }
            set
            {
                ViewState["CallbackJSFile"] = value;
            }
        }

        public bool IsCheckFresh { get; set; }
        public bool IsFresh { get; set; }

        public CultureInfo GetCurrentCulture()
        {
            return System.Threading.Thread.CurrentThread.CurrentCulture;
        }
        public string CurrentCultureName
        {
            get { return System.Threading.Thread.CurrentThread.CurrentCulture.Name; }

        }
        public bool IsAgentRoot
        {
            get
            {
                bool returnValue = false;
                var sValue = Session["IsAgentRoot"];
                if (sValue != null)
                {
                    returnValue = Convert.ToBoolean(sValue);
                }
                return returnValue;
            }
        }
        public int AgentIdCurrent
        {
            get
            {
                int iAgentId = 0;
                string sAgent = UrlHelper.GetUrlParameter("AgentId");
                var sSAgent = Session["CurrentAgentId"];
                if (string.IsNullOrEmpty(sAgent) || (sAgent != null && sAgent.Equals("undefined")))
                {
                    if (sSAgent != null)
                    {
                        sAgent = sSAgent.ToString();
                    }
                }
                int.TryParse(sAgent, out iAgentId);
                return iAgentId;
            }
        }
        public string AgentNameCurrent
        {
            get
            {
                string sAgentName = UrlHelper.GetUrlParameter("AgentName");
                var sSAgentName = Session["AgentName"];
                if (string.IsNullOrEmpty(sAgentName))
                {
                    if (sSAgentName != null)
                    {
                        sAgentName = sSAgentName.ToString();
                    }
                }
                return sAgentName;
            }
        }

        public string CurrentRoleName
        {
            get
            {
                string strReturn = string.Empty;
                if (Session["ListRole"] != null)
                {
                    //object listObj = Session["ListRole"];
                    dynamic listObj = Session["ListRole"];
                    if (listObj != null)
                    {
                        dynamic roleObj = listObj[0];
                        if (roleObj != null)
                        {
                            strReturn = roleObj.RoleName;
                        }
                    }
                }
                return strReturn;
            }
        }
        /// <summary>
        /// Get the current UserName of the online user
        /// </summary>        
        public  string SCurrentUserId
        {
            get
            {
                string sReturn = string.Empty;
                if (Session["UserId"] != null)
                {
                    sReturn = Session["UserId"].ToString();
                }
                return sReturn;
            }
        }
        public string CurrentUserName
        {
            get
            {
                string userName = string.Empty;
                if (HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    userName = HttpContext.Current.User.Identity.Name;
                }
                return userName;
            }
        }
        public string gCurrentViewId
        {
            get
            {
                string sGViewId = UrlHelper.GetUrlParameter("site");
                var cookieGViewId = Request.Cookies["GINS-GViewId"];
                if (string.IsNullOrEmpty(sGViewId))
                {
                    if (cookieGViewId != null && cookieGViewId.Value != null)
                    {
                        sGViewId = cookieGViewId.Value;
                    }
                }
                else
                {
                    if (cookieGViewId != null && cookieGViewId.Value != null)
                    {
                        cookieGViewId.Value = sGViewId;
                        Response.SetCookie(cookieGViewId);
                    }
                    else
                    {
                        var cookieNew = new HttpCookie("GINS-GViewId");
                        cookieNew.Value = sGViewId;
                        cookieNew.Expires = DateTime.Now.AddMonths(6);
                        Response.SetCookie(cookieNew);
                    }
                }
                //Value =-1: ko so huu site nao dang duoc cai dat de theo doi
                return sGViewId = (bExistSite)?sGViewId:"-1";
            }
        }

        public int CurrentSiteId
        {
            get
            {
                int iReturn = -1;
                if (Session["SiteId"]!=null)
                {
                    iReturn = (int)Session["SiteId"];
                }
                return iReturn;
            }
        }
        public bool bExistSite
        {
            get
            {
                bool bReturn = false;
                if (Session["ExistSite"] != null)
                {
                    bReturn = (bool)Session["ExistSite"];
                }
                return bReturn;
            }
        }

        public string GActCurrentId
        {
            get
            {
                string sActId = UrlHelper.GetUrlParameter("gact");
                var cookieActId = Request.Cookies["GINS-GActId"];
                if (string.IsNullOrEmpty(sActId))
                {
                    if (cookieActId != null && cookieActId.Value != null)
                    {
                        sActId = cookieActId.Value;
                    }
                }
                else
                {
                    if (cookieActId != null && cookieActId.Value != null)
                    {
                        cookieActId.Value = sActId;
                        Response.SetCookie(cookieActId);
                    }
                    else
                    {
                        var cookieNew = new HttpCookie("GINS-GActId");
                        cookieNew.Value = sActId;
                        cookieNew.Expires = DateTime.Now.AddMonths(6);
                        Response.SetCookie(cookieNew);
                    }
                }
                //Value =-1: ko so huu tk Quang Cao nao dang duoc cai dat de theo doi
                return sActId = (bExistGAct) ? sActId : "-1";
            }
        }
        public bool bExistGAct
        {
            get
            {
                bool bReturn = false;
                if (Session["ExistGAct"] != null)
                {
                    bReturn = (bool)Session["ExistGAct"];
                }
                return bReturn;
            }
        }

        public string FbActCurrentId
        {
            get
            {
                string sActId = UrlHelper.GetUrlParameter("fbact");
                var cookieActId = Request.Cookies["GINS-FbActId"];
                if (string.IsNullOrEmpty(sActId))
                {
                    if (cookieActId != null && cookieActId.Value != null)
                    {
                        sActId = cookieActId.Value;
                    }
                }
                else
                {
                    if (cookieActId != null && cookieActId.Value != null)
                    {
                        cookieActId.Value = sActId;
                        Response.SetCookie(cookieActId);
                    }
                    else
                    {
                        var cookieNew = new HttpCookie("GINS-FbActId");
                        cookieNew.Value = sActId;
                        cookieNew.Expires = DateTime.Now.AddMonths(6);
                        Response.SetCookie(cookieNew);
                    }
                }
                //Value =-1: ko so huu tk Quang Cao nao dang duoc cai dat de theo doi
                return sActId = (bExistFbAct) ? sActId : "-1";
            }
        }
        public bool bExistFbAct
        {
            get
            {
                bool bReturn = false;
                if (Session["ExistFbAct"] != null)
                {
                    bReturn = (bool)Session["ExistFbAct"];
                }
                return bReturn;
            }
        }
        public Dictionary<string, string> ListCrmDimensions
        {
            get
            {
                var dic = new Dictionary<string, string>();
                var cookieListCrmDimensions = Request.Cookies["GINS-ListCrmDimensions"];
                if (cookieListCrmDimensions==null)
                {
                    dic.Add("FullName", "Full Name");
                    dic.Add("Email", "Email");
                    dic.Add("MobilePhone", "Phone");
                    dic.Add("SexText", "Gender");
                    dic.Add("Address", "Address");
                    dic.Add("CreateDate", "Date register");
                    dic.Add("InputTypeText", "Type register");
                }
                else
                {
                    //dic = cookieListCrmDimensions;
                }
                return dic;
            }
        }
        public int CurrentPage
        {
            get
            {
                string sCurrentPage = UrlHelper.GetUrlParameter("index");
                if (!string.IsNullOrEmpty(sCurrentPage))
                {
                    try
                    {
                        return Convert.ToInt32(sCurrentPage);
                    }
                    catch
                    {
                        return 1;
                    }
                }
                else
                {
                    return 1;
                }
            }
        }
        public string GetTableNameReport
        {
            get
            {
                string strReturnValue = string.Empty;
                string strPageCode = UrlHelper.GetUrlParameter("PageCode");
                strPageCode = (strPageCode == null || strPageCode == "undefined" || strPageCode == string.Empty) ? UrlHelper.GetUrlParameter("Page") : strPageCode;


                if (!string.IsNullOrEmpty(strPageCode))
                {
                    switch (strPageCode)
                    {
                        case "ADS_REPORT_OVERVIEW":
                            strReturnValue = "tblCampaign_Performance_Report";
                            break;
                        case "ADS_REPORT_GOOGLE_BYCAMPAIGN":
                            strReturnValue = "tblCampaign_Performance_Report";
                            break;
                        case "ADS_REPORT_FACEBOOK_BYCAMPAIGN":
                            strReturnValue = "tblFb_Campaign_Performance_Report";
                            break;
                        case "ADS_REPORT_FACEBOOK_BYGROUP":
                            strReturnValue = "tblFb_Adset_Performance_Report";
                            break;
                        default:
                            strReturnValue = "tblCampaign_Performance_Report";
                            break;
                    }
                }

                return strReturnValue;
            }
        }
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            if (IsCheckFresh)
            {
                string viewStateTicket = (string)ViewState["ViewStateTicket_" + this.UniqueID];
                string sessionTicket = (string)Session["SessionTicket_" + this.UniqueID];
                if (viewStateTicket == sessionTicket) IsFresh = false;
                else IsFresh = true;
            }
        }

        /// <summary>
        /// Enable Ajax utilities
        /// </summary>
        public bool EnableAJAX
        {
            get
            {
                if (ViewState["EnableAJAX"] == null)
                    return false;
                return (bool)ViewState["EnableAJAX"];
            }
            set
            {
                ViewState["EnableAJAX"] = value;
            }
        }

        private string _IncludedScripts = string.Empty;
        public string IncludedScripts
        {
            get
            {
                return _IncludedScripts;
            }
            set
            {
                _IncludedScripts = value;
            }
        }


        private string _IncludedStyles = string.Empty;
        public string IncludedStyles
        {
            get
            {
                return _IncludedStyles;
            }
            set
            {
                _IncludedStyles = value;
            }
        }

    }
}
