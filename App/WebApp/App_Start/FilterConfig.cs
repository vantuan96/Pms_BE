using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Helpers;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace PMS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
    public sealed class ValidateCSRFAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (actionContext.Request.Method.Method != "GET")
            {
                var headers = actionContext.Request.Headers;
                var ck = headers
                    .GetCookies();
                var tokenCookie = headers
                    .GetCookies()
                    .Select(c => c["__PMSRequestVerificationToken"])
                    .FirstOrDefault();

                var cookie = HttpContext.Current.Request.Cookies[AntiForgeryConfig.CookieName];

                var tokenHeader = string.Empty;
                if (headers.Contains("RequestVerificationToken"))
                {
                    tokenHeader = headers.GetValues("RequestVerificationToken").FirstOrDefault();
                }

                //AntiForgery.Validate(
                //    tokenCookie != null ? tokenCookie.Value : null, tokenHeader);
            }
        }
    
        //public override void OnActionExecuting(
        //    System.Web.Http.Controllers.HttpActionContext actionContext)
        //{
        //    if (actionContext == null)
        //    {
        //        throw new ArgumentNullException("actionContext");
        //    }

        //    if (actionContext.Request.Method.Method != "GET")
        //    {
        //        var headers = actionContext.Request.Headers;
        //        var tokenCookie = headers
        //            .GetCookies()
        //            .Select(c => c[AntiForgeryConfig.CookieName])
        //            .FirstOrDefault();

        //        var tokenHeader = string.Empty;
        //        if (headers.Contains("X-XSRF-Token"))
        //        {
        //            tokenHeader = headers.GetValues("X-XSRF-Token").FirstOrDefault();
        //        }

        //        AntiForgery.Validate(
        //            tokenCookie != null ? tokenCookie.Value : null, tokenHeader);
        //    }
        //}
    }
}
