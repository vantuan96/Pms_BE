using DataAccess.Repository;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VM.Common;

namespace PMS.WebApp.Controllers
{
    public class AuthenController : Controller
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        // GET: Authen
        // GET: Authen
        [HttpGet]
        public ActionResult Login()
        {
            if (System.Web.HttpContext.Current.User.Identity.Name == "")
            {

                return View();
            }
            else
            {
                var userClaims = User.Identity as System.Security.Claims.ClaimsIdentity;
                var result = userClaims.Name.Split('@');
                var st = result[0].ToString();
                var userData = unitOfWork.UserRepository.FirstOrDefault(s => !s.IsDeleted && s.Username.Trim().ToLower().Equals(st));
                if (userData == null)
                {
                    string host = ConfigurationManager.AppSettings["redirecttoken"];
                    string url = host + "#/404";
                    System.Uri uri = new System.Uri(url);
                    return Redirect(uri.ToString());
                }
                else
                {
                    ClaimsIdentity identity = CreateIdentity(userData);
                    Request.GetOwinContext().Authentication.SignIn(identity);
                    return RedirectToAction("RedirectLogin", "Home", new { Token = "123456" /*Guid.NewGuid().ToString().Substring(1,10)*/ });

                }
            }
         
        }

        private ClaimsIdentity CreateIdentity(DataAccess.Models.User user)
        {
            string username = string.IsNullOrEmpty(user.Username) ? "" : user.Username;
            string roles = string.IsNullOrEmpty(user.Roles) ? "" : user.Roles;

            string role = "";
            var current_roles = user.UserRoles.ToList();
            var actions = new List<string>();
            if (current_roles.Count > 0)
            {
                foreach (var ro in current_roles)
                {
                    #region Group role action
                    var grpAct = ro.Role.RoleGroupActions.Where(x => !x.IsDeleted && !x.GroupAction.IsMenu)?.Select(r => r.GroupAction)?.ToList();
                    if (grpAct?.Count > 0)
                    {
                        var grpMap = grpAct?.Where(x => !x.IsDeleted).Select(x => x.GroupAction_Maps)?.ToList();
                        if (grpMap?.Count > 0)
                        {
                            foreach (var item in grpMap)
                            {
                                actions.AddRange(item?.Where(x => !x.IsDeleted && !x.Action.IsDeleted).Select(x => x.Action.Code));
                            }
                        }
                    }
                    #endregion .Group role action
                }
            }

            //actions.AddRange(ro.Role.RoleGroupActions.Select(r => r.GroupAction.GroupAction_Maps.Select(x=>x.Action.Code)));
            actions = actions.Distinct().ToList();
            role = string.Join(",", actions);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, roles),
                new Claim("Roles", role)
            };
            var identity = new ClaimsIdentity(claims, "ApplicationCookie");
            return identity;
        }

        public async Task<string> GetListAppLogin()
        {
            var appid = ConfigurationManager.AppSettings["AppId"].ToString();
            var uri = "api/ManageAppBase/GetListAppForm?appid=" + appid;
            var value = await ApiHelper.HttpGet(uri, "2krojMdNQkSpZzwybnoR6g==");
            var response = value.Content.ReadAsStringAsync().Result;
            return response;
        }
        public async Task<string> GetListApp()
        {
            var appid = ConfigurationManager.AppSettings["AppId"].ToString();
            var uri = "api/ManageAppBase/GetListApp?appid=" + appid;
            var value = await ApiHelper.HttpGet(uri, "2krojMdNQkSpZzwybnoR6g==");
            var response = value.Content.ReadAsStringAsync().Result;
            return response;
        }

        // Sign in has been triggered from Sign In Button or From Single Sign Out Page.
        public void SignIn(string redirectUri)
        {
            HttpContext.GetOwinContext().Authentication.Challenge(
           new AuthenticationProperties { RedirectUri = "/" },
           OpenIdConnectAuthenticationDefaults.AuthenticationType);
            //return null;
        }
        [Authorize]
        public JsonResult SessionChanged()
        {
            // If the javascript made the reuest, issue a challenge so the OIDC request will be constructed.
            if (HttpContext.GetOwinContext().Request.QueryString.Value == "")
            {

                HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/Authen/SessionChanged" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);

                return Json(new { }, "application/json", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(HttpContext.GetOwinContext().Request.QueryString.Value, "application/json", JsonRequestBehavior.AllowGet);
            }
        }

        // Sign a user out of both AAD and the Application
        public void SignOut()
        {
            var cookie1 = new HttpCookie("__PMSies");
            DateTime nowDateTime = DateTime.Now;
            cookie1.Expires = nowDateTime.AddDays(-1);
            HttpContext.Request.Cookies.Add(cookie1);
      
            HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = Startup.PostLogoutRedirectUri + "Authen/Login" },
                OpenIdConnectAuthenticationDefaults.AuthenticationType,
                CookieAuthenticationDefaults.AuthenticationType);
            HttpContext.GetOwinContext().Authentication.User =
new GenericPrincipal(new GenericIdentity(string.Empty), null);
          //  return RedirectToAction("Login", "Authen");
        }


        // Action for displaying a page notifying the user that they've been signed out automatically.
        public ActionResult SingleSignOut(string redirectUri)
        {
            // RedirectUri is necessary to bring a user back to the same location 
            // if they re-authenticate after a single sign out has occurred. 
            if (redirectUri == null)
                ViewBag.RedirectUri = Startup.PostLogoutRedirectUri;
            else
                ViewBag.RedirectUri = redirectUri;
            HttpContext.GetOwinContext().Authentication.SignOut();
            HttpContext.GetOwinContext().Authentication.User =
                new GenericPrincipal(new GenericIdentity(string.Empty), null);
            HttpContext.GetOwinContext().Authentication.SignOut(
            OpenIdConnectAuthenticationDefaults.AuthenticationType,
            CookieAuthenticationDefaults.AuthenticationType);
            return RedirectToAction("Login", "Authen");
            //return null;
        }

    }
}