using DataAccess.Models;
using PMS.Authentication;
using PMS.Contract.Models;
using PMS.Controllers.BaseControllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using VM.Common;
using System.Web.Http.Cors;
using static PMS.FilterConfig;
using System.Threading.Tasks;
using System.ComponentModel;

namespace PMS.Controllers.GeneralControllers
{
    /// <summary>
    /// Login/Logout module
    /// </summary>
    /// 
    public enum StatusEnum
    {
        //[Description("Tất cả")]
        //All = -1,

        [Description("InActive")]
        InActive = 0,
        [Description("Active")]
        Active = 1,
        [Description("Locked")]
        Locked = 2,
        [Description("Removed")]
        Removed = 3
    }
    public enum AppTypeEnum
    {
        [Description("Admin")]
        AdminApp = 1,
        [Description("FrontEnd")]
        FrontEndApp = 2
    }
    public enum TargetEnum
    {
        [Description("_Blank")]
        _Blank = 1,
        [Description("_Self")]
        _Self = 2,
        [Description("_Iframe")]
        _Iframe = 3
    }
    public class AppSettingContract
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public AppContract App { get; set; }
        public int AppType { get; set; }
        public AppTypeEnum TypeEnum
        {
            get
            {
                switch (this.AppType)
                {
                    case 1:
                        return AppTypeEnum.AdminApp;
                    case 2:
                        return AppTypeEnum.FrontEndApp;
                    default:
                        return AppTypeEnum.AdminApp;
                }
            }
        }
        public string AppKey { get; set; }
        public string SecretKey { get; set; }
        public string Domain { get; set; }
        public string DefaultUrlRefer { get; set; }
        public string UrlVerify { get; set; }
        public string ADDomainName { get; set; }
        public string DefaultPassword { get; set; }
        public Nullable<int> Target { get; set; }
        public TargetEnum TargetTypeEnum
        {
            get
            {
                switch (this.Target)
                {
                    case 1:
                        return TargetEnum._Blank;
                    case 2:
                        return TargetEnum._Self;
                    case 3:
                        return TargetEnum._Iframe;
                    default:
                        return TargetEnum._Blank;
                }
            }
        }
        public Nullable<int> FailPasswordLockNumber { get; set; }
        public Nullable<bool> IsShowReCaptcha { get; set; }
        public Nullable<int> FailedPassNumberShowReCaptcha { get; set; }
        public bool IsCheckRole { get; set; }
        public string IpWhiteList { get; set; }
        public string IpBlackList { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        public Nullable<int> Status { get; set; }
        public StatusEnum StatusEnum
        {
            get
            {
                switch (this.Status)
                {
                    case 0:
                        return StatusEnum.InActive;
                    case 1:
                        return StatusEnum.Active;
                    case 2:
                        return StatusEnum.Locked;
                    case 3:
                        return StatusEnum.Removed;
                    default:
                        return StatusEnum.InActive;
                }
            }
        }
        public Nullable<bool> IsDeleted { get; set; }

    }
    public class AppContract
    {
        public int AppId { get; set; }
        public AppGroupContract AppGroup { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string NameDisplay { get; set; }
        public string Logo { get; set; }
        public string Icon { get; set; }
        public string BackDrop { get; set; }
        public string CategoryKey { get; set; }
        public string ShortDecription { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<int> CreateBy { get; set; }
        public Nullable<System.DateTime> LastUpdate { get; set; }
        public Nullable<int> UpdateBy { get; set; }
        public Nullable<bool> AppRoot { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public List<AppSettingContract> AppSettings { get; set; }

    }
    public class AppGroupContract
    {
       
    public int Id { get; set; }
   
    public string AppGroupCode { get; set; }

    public string AppGroupName { get; set; }

    public string CssIcon { get; set; }

    public Nullable<int> Sort { get; set; }

    public Nullable<int> Status { get; set; }
   
    public Nullable<int> IsDeleted { get; set; }

    public List<AppContract> ListApps { get; set; }
}

public class AccountController : BaseApiController
    {
        /// <summary>
        /// API Show captcha
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Account/ShowCaptcha")]
        public IHttpActionResult ShowCaptcha()
        {
            var ip = GetIp();
            return Content(HttpStatusCode.OK, IsShowCaptcha(ip));
        }
        /// <summary>
        /// API Login
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        //[CSRFCheck]
        //[ValidateCSRF]
        [Route("api/Account/Login")]
        public IHttpActionResult LoginAPI([FromBody]LoginParameterModel request)
        {
            try {
                if (!request.Validate())
                    return Content(HttpStatusCode.BadRequest, Message.LOGIN_ERROR);

                var ip = GetIp();
                var is_show_captcha = IsShowCaptcha(ip);
                if (is_show_captcha)
                {
                    //Tạm bỏ check valid với server captcha
                    if (string.IsNullOrEmpty(request.captcha) || !Recaptcha.IsReCaptchValid(request.captcha))
                    //if (string.IsNullOrEmpty(request.captcha))
                    {
                        IncreaseLoginFail(ip);
                        return Content(HttpStatusCode.BadRequest, Message.LOGIN_ERROR);
                    }
                }

                var user = ValidateUser(request.username, request.password);
                if (user == null)
                {
                    IncreaseLoginFail(ip);
                    return Content(HttpStatusCode.BadRequest, Message.LOGIN_ERROR);
                }

                ClaimsIdentity identity = CreateIdentity(user);
                Request.GetOwinContext().Authentication.SignIn(identity);
                SetZeroLoginFail(ip);
                var s = Content(HttpStatusCode.OK, new { Toke = "a" });
                return Content(HttpStatusCode.OK, new { Toke     = "a" });
            }
            catch(Exception ex)
            {
                CustomLog.accesslog.Error(string.Format("Login fail. Ex: {0}", ex));
                return Content(HttpStatusCode.OK, new { Token = "e" });
            }
            
        }
        /// <summary>
        /// API Logout
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Account/Logout")]
        public IHttpActionResult GetLogout()
        {
            var resp = new HttpResponseMessage();
            try
            {
                CookieHeaderValue cookie = Request.Headers.GetCookies(string.Format("{0}ies", ConfigHelper.AppKey)).FirstOrDefault();
                if (cookie != null)
                {
                    RemoveSession(cookie[string.Format("{0}ies", ConfigHelper.AppKey)].Value);
                    var new_cookie = new CookieHeaderValue(string.Format("{0}ies", ConfigHelper.AppKey), "")
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        Domain = cookie.Domain,
                        Path = cookie.Path
                    };
                    resp.Headers.AddCookies(new[] { new_cookie });
                }
            }
            catch (Exception) { }
            try
            {
                var form_token = Request.Headers.GetValues("RequestVerificationToken").First();
                var cookie_token = Request.Headers.GetCookies("__PMSRequestVerificationToken").LastOrDefault();
                var csrf_token = string.Format("{0}{1}", form_token, cookie_token["__PMSRequestVerificationToken"].Value);
                var new_cookie_token = new CookieHeaderValue("__PMSRequestVerificationToken", "")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Domain = cookie_token.Domain,
                    Path = cookie_token.Path
                };
                resp.Headers.AddCookies(new[] { new_cookie_token });
            }
            catch (Exception) { }
            resp.StatusCode = HttpStatusCode.OK;
            var cookie1 = new HttpCookie("__PMSies");
            DateTime nowDateTime = DateTime.Now;
            cookie1.Expires = nowDateTime.AddSeconds(-1);
            HttpContext.Current.Response.Cookies.Add(cookie1);
            var value = HttpContext.Current.Request.Cookies["__PMSies"];
            return Content(HttpStatusCode.OK, new { url = "Authen/SignOut" });
              
        }
        public bool IsShowCaptcha(string ip)
        {
            try
            {
                var login_failed = unitOfWork.LogInFailRepository.FirstOrDefault(
                    e => !string.IsNullOrEmpty(e.IPAddress) && e.IPAddress == ip
                );
                if (login_failed != null)
                    return login_failed.Time > Int32.Parse(ConfigurationManager.AppSettings["NumberShowCaptCha"]);
                return false;
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("IsShowCaptcha fail. Ex: {0}", ex));
                return false;
            }
        }

        private ClaimsIdentity CreateIdentity(User user)
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

        private User ValidateUser(string username, string password)
        {
            //if (ConfigurationManager.AppSettings["HiddenError"].Equals("false") && username.Contains("vm.test"))
            //    return unitOfWork.UserRepository.FirstOrDefault(s => s.Username.Equals(username));

            bool isValidADAccount = LoginAdAccount(username, password);
            //bool isValidADAccount = true;
            if (isValidADAccount)
            {
                var userData = unitOfWork.UserRepository.FirstOrDefault(s => !s.IsDeleted && s.Username.Trim().ToLower().Equals(username.Trim().ToLower()));
                if (userData != null)
                {
                    return AdAccountSynchronous(username, userData);
                }
                else
                {
                    var log = new
                    {
                        URI = "api/Account/Login",
                        Request = JsonConvert.SerializeObject(new { Usernam = username, Password = "******" }).ToString(),
                        Response = "Validate AD success, user is not exists in db",
                        Action = "Error"
                    };
                    CustomLog.accesslog.Error(log);
                }
                return userData;
            }
            else
            {
                var log = new
                {
                    URI = "api/Account/Login",
                    Request = JsonConvert.SerializeObject(new { Usernam = username, Password = "******" }).ToString(),
                    Response = "Validate AD fail",
                    Action = "Error"
                };
                CustomLog.accesslog.Error(log);
                return null;
            }
        }
        private User AdAccountSynchronous(string userName, User user)
        {
            //var adUser = GetUserADInfo(userName);
            //user.FirstName = adUser.FirstName;
            //user.LastName = adUser.LastName;
            //user.Fullname = adUser.FullName;
            //user.DisplayName = adUser.DisplayName;
            //user.LoginNameWithDomain = adUser.LoginNameWithDomain;
            //user.Mobile = adUser.Mobile;
            //user.EmailAddress = adUser.EmailAddress;
            //user.Department = adUser.Department;
            //user.Title = adUser.Title;
            //user.Description = adUser.Description;
            //user.Company = adUser.Company;
            //user.ManagerName = adUser.ManagerName;
            //if (adUser.Manager != null)
            //{
            //    user.ManagerId = adUser.Manager.UserId;
            //}
            //user.Username = userName.Trim().ToLower();
            //user.Roles = "Authorized";
            //unitOfWork.UserRepository.Update(user);
            //unitOfWork.Commit();
            //linhht
            return user;
        }
        private ADUserDetailModel GetUserADInfo(string userName, string domainName = "vingroup.local")
        {
            PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName);
            UserPrincipal userPrincipal = new UserPrincipal(domainContext)
            {
                SamAccountName = userName
            };
            PrincipalSearcher principleSearch = new PrincipalSearcher
            {
                QueryFilter = userPrincipal
            };
            PrincipalSearchResult<Principal> results = principleSearch.FindAll();
            Principal principle = results.ToList()[0];
            DirectoryEntry directory = (DirectoryEntry)principle.GetUnderlyingObject();
            principleSearch.Dispose();
            return ADUserDetailModel.GetUser(directory);
        }
        private bool LoginAdAccount(string userName, string password)
        {
            //linhht
            //bool isValidAdAccount = false;
            //using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
            //{
            //    isValidAdAccount = context.ValidateCredentials(userName, password);
            //}
            //return isValidAdAccount;
            return true;
        }
        private void IncreaseLoginFail(string ip)
        {
            var login_fail = unitOfWork.LogInFailRepository.FirstOrDefault(
                e => !string.IsNullOrEmpty(e.IPAddress) && e.IPAddress== ip
            );
            if (login_fail != null)
            {
                login_fail.Time += 1;
                unitOfWork.LogInFailRepository.Update(login_fail);
            }
            else
            {
                var new_login_fail = new LogInFail()
                {
                    IPAddress = ip,
                    Time = 1,
                };
                unitOfWork.LogInFailRepository.Add(new_login_fail);
            }
            unitOfWork.Commit();
        }
        private void SetZeroLoginFail(string ip)
        {
            var login_fail = unitOfWork.LogInFailRepository.FirstOrDefault(
                e => !string.IsNullOrEmpty(e.IPAddress) &&
                e.IPAddress == ip
            );
            if (login_fail != null)
            {
                login_fail.Time = 0;
                unitOfWork.LogInFailRepository.Update(login_fail);
                unitOfWork.Commit();
            }
        }
        private void RemoveSession(string session)
        {
            var session_id = session.Substring(0, 20);
            var user = unitOfWork.UserRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                !string.IsNullOrEmpty(e.SessionId) &&
                e.SessionId == session_id
            );
            if (user != null)
            {
                user.Session = null;
                user.SessionId = null;
                unitOfWork.UserRepository.Update(user);
                unitOfWork.Commit();
            }
        }

        [HttpGet]
        [Route("api/Account/RedirectUrl")]
        public IHttpActionResult RedirectUrl()
        {
            string host = ConfigurationManager.AppSettings["host"];
            var appId = ConfigurationManager.AppSettings["AppId"];
            var urlRedirect = host + appId + "/vn/Authen/Login";       
            return Content(HttpStatusCode.OK, new { url = urlRedirect });
        }

        [HttpGet]
        [Route("api/Account/GetListBoxApp")]
        public async Task<List<AppContract>> GetListBoxApps()
        {
            var appid = ConfigurationManager.AppSettings["AppId"].ToString();
            var uri = "api/ManageAppBase/GetListManageAppFE?appid=" + appid;
            var value = await ApiHelper.HttpGet(uri, "2krojMdNQkSpZzwybnoR6g==");
            var response = value.Content.ReadAsStringAsync().Result;
            List<AppContract> businessunits = JsonConvert.DeserializeObject<List<AppContract>>(response);

            return businessunits;
         
        }

    }
}
