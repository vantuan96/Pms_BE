using DataAccess.Repository;
//using PMS.Common;
//using PMS.ScheduleJobs;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using VM.Common;

namespace PMS
{
    /// <summary>
    /// WebApiApplication
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Application_Start
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //JobScheduler.Start();
            //Set value contant for Application
            Business.Helper.HelperBusiness.Instant.SetAppContant();
        }
        /// <summary>
        /// Application_PreSendRequestHeaders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            try
            {
                var resp_header = HttpContext.Current.Response.Headers;
                if (resp_header == null) return;

                resp_header.Remove("Server");
                resp_header.Remove("X-AspNetWebPages-Version");
                resp_header.Remove("X-AspNet-Version");
                resp_header.Remove("X-Powered-By");
                resp_header.Remove("X-AspNetMvc-Version");

                var new_cookie = resp_header.GetValues("Set-Cookie");
                var path1 = HttpContext.Current.Request.Path.ToLower();
                if (new_cookie != null)
                {
                    var path = HttpContext.Current.Request.Path.ToLower();
                    if (Constant.IGNORE_EXTEND_SESSION_PATH.Contains(path))
                        resp_header.Remove("Set-Cookie");

                    else if (!Constant.IGNORE_UPDATE_SESSION_PATH.Contains(path))
                        UpdateSession(new_cookie);
                }
            }
            catch (Exception Ex) { }
        }

        private void UpdateSession(string[] raw_cookie)
        {
            var cookie = Regex.Match(raw_cookie[0], string.Format("^{0}=(.*);", ConfigHelper.AppKey))?.Groups[1].Value;
            if (!string.IsNullOrEmpty(cookie))
            {
                using (IUnitOfWork unitOfWork = new EfUnitOfWork())
                {
                    var username = HttpContext.Current.User.Identity.Name;
                    var user = unitOfWork.UserRepository.FirstOrDefault(m => !m.IsDeleted && m.Username == username);
                    if (user != null)
                    {
                        user.SessionId = cookie.Substring(0, 20);
                        user.Session = cookie;
                        unitOfWork.UserRepository.Update(user);
                        unitOfWork.Commit();
                    }
                }
            }
        }
    }
}
