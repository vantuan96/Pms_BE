using VM.Common;
using Microsoft.Owin;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using DataAccess.Repository;

namespace PMS.Authentication
{
    public class SessionAuthorize : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var ip = GetClientIpAddress(actionContext.Request);
           
            try
            {
                if (ConfigurationManager.AppSettings["DevWriteLists"].Contains(ip))
                    return true;
            }
            catch (Exception) { }


            if (!base.IsAuthorized(actionContext))
                return false;

            dynamic authen_cookie;
            try
            {
                authen_cookie = actionContext.Request.Headers.GetCookies(string.Format("{0}ies",ConfigHelper.AppKey)).FirstOrDefault();
                if (authen_cookie != null)
                {
                    var session_id = authen_cookie[string.Format("{0}ies", ConfigHelper.AppKey)].Value.Substring(0, 20);
                    return IsTokenValid(session_id);
                }
            }
            catch (Exception) { }
            return false;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
            actionContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new ObjectContent<dynamic>(Message.UNAUTHORIZED, new JsonMediaTypeFormatter())
            };
        }

        private bool IsTokenValid(string session_id)
        {
            using (IUnitOfWork unitOfWork = new EfUnitOfWork())
            {
                var user = unitOfWork.UserRepository.FirstOrDefault(
                    e => !e.IsDeleted &&
                    !string.IsNullOrEmpty(e.SessionId) &&
                    e.SessionId == session_id
                );
                if (user != null)
                    return user.Username == HttpContext.Current.User.Identity.Name;

                return false;
            }
        }

        private string GetClientIpAddress(HttpRequestMessage request)
        {
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return IPAddress.Parse(((HttpContextBase)request.Properties["MS_HttpContext"]).Request.UserHostAddress).ToString();
            }
            if (request.Properties.ContainsKey("MS_OwinContext"))
            {
                return IPAddress.Parse(((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress).ToString();
            }
            return String.Empty;
        }
    }
}