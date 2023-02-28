using VM.Common;
using Microsoft.Owin;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace PMS.Authentication
{
    public class Permission : AuthorizeAttribute
    {
        public string Code { get; set; }
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var ip = GetClientIpAddress(actionContext.Request);
            try
            {
                if (ConfigurationManager.AppSettings["DevWriteLists"].Contains(ip))
                    return true;
            }
            catch (Exception) { }
            try
            {
                string controllerName = actionContext.ActionDescriptor.ControllerDescriptor != null ? actionContext.ActionDescriptor.ControllerDescriptor.ControllerName : "";
                string actionName = actionContext.ActionDescriptor != null ? actionContext.ActionDescriptor.ActionName : "";
                string GenCode = string.Format("{0}_{1}", controllerName, actionName);
                var url = actionContext.Request.RequestUri.AbsolutePath;
                var method = actionContext.Request.Method.Method;
                var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
                var roles = principal.Claims.FirstOrDefault(c => c.Type == "Roles").Value;
                if (!string.IsNullOrEmpty(roles))
                {
                    string[] arrRoles = roles.ToUpper().Split(',');
                    return arrRoles.Contains(GenCode.ToUpper());
                }
                return false;   
                //return roles.Contains(Code);
            }
            catch (Exception)
            {
                return false;
            }
        }
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
            actionContext.Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Forbidden,
                Content = new ObjectContent<dynamic>(Message.FORBIDDEN, new JsonMediaTypeFormatter())
            };
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