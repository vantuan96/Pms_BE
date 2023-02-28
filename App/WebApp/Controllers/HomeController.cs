
using DataAccess.Repository;
using Microsoft.Owin;
using PMS.WebApp.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http.Cors;
using System.Web.Mvc;
using VM.Common;

namespace PMS.Controllers
{
    /// <summary>
    /// HomeController
    /// </summary>
    //[AllowCrossSiteJson]
    //[EnableCors(origins: "http://localhost:8085", headers: "*", methods: "*")]
    [Authorize]
    public class HomeController : BaseController
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
     
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                unitOfWork.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>   
        /// Index
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            try
            {
                var form_token = Request.Headers.GetValues("__PMSRequestVerificationToken").First();
                var cookie_token = Request.Cookies["__PMSRequestVerificationToken"].ToString();
                var csrf_token = string.Format("{0}{1}", form_token, cookie_token);
            }
            catch (Exception) { }
            ViewBag.Title = "Home Page";
            var token = CSRFToken.Generate();
            ViewBag.CSRFToken = token.Substring(0, 60);
            HttpCookie cookie = new HttpCookie("__PMSRequestVerificationToken", token.Substring(60));
            cookie.Expires = DateTime.Now.AddMinutes(1440);
            //cookie.Path += ";SameSite=Strict";
            Response.Cookies.Add(cookie);
            return View();
          
        }

        public ActionResult RedirectLogin(string Token)
        {

            ViewBag.token = Token;
            return View();
        }
    }
}
