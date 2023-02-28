using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PMS.WebApp.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            string controllerName = filterContext.HttpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            string actionName = filterContext.HttpContext.Request.RequestContext.RouteData.Values["action"].ToString();
            var cookieCheckLogin = filterContext.HttpContext.Request.Cookies["_CookieCheckLogin"];
            var users = filterContext.HttpContext.Request.GetOwinContext().Authentication.User.Identity;
    
            var value = filterContext.HttpContext.Request.Cookies["__PMSies"];
            if (value == null || value.Value == "")
            {
                string url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Login", "Authen");
                if (filterContext.HttpContext.Request.IsAjaxRequest() && filterContext.HttpContext.Request.HttpMethod == "POST")
                {
                    url = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("Login", "Authen");
                    filterContext.Result = new JsonResult
                    {
                        Data = new { redirect = url, status = 401 },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectResult(url);
                }

            }
            else
            {
                var cookie = filterContext.HttpContext.Request.Cookies[".AspNet.Cookies"];
                if (cookie == null)
                {
                    var url1 = new UrlHelper(filterContext.HttpContext.Request.RequestContext).Action("SignOut", "Authen");
                    filterContext.Result = new RedirectResult(url1);
                }
                else
                {

                }
            }
          
        }
    }
}