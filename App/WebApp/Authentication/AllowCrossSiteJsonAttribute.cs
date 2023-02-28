using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PMS.WebApp.Authentication
{
    /// <summary>
    /// Filter Cors
    /// </summary>
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// OnActionExecuting
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //var ctx = filterContext.RequestContext.HttpContext;
            //var origin = ctx.Request.Headers["Origin"];
            //var allowOrigin = !string.IsNullOrWhiteSpace(origin) ? origin : "*";
            //ctx.Response.AddHeader("Access-Control-Allow-Origin", allowOrigin);
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "http://localhost:8085");
            base.OnActionExecuting(filterContext);
        }
    }
}