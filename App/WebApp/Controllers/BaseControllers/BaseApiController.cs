using DataAccess.Models;
using DataAccess.Repository;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel.Channels;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace PMS.Controllers.BaseControllers
{
    /// <summary>
    /// 
    /// </summary>
    //[EnableCors(origins: "http://localhost:8085", headers: "*", methods: "*")]
    public class BaseApiController: ApiController
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

        #region RequestInformation
        protected string GetIp()
        {
            try {
                if (Request.Properties.ContainsKey("MS_HttpContext"))
                {
                    return ((HttpContextWrapper)Request.Properties["MS_HttpContext"]).Request.UserHostAddress;
                }
                else if (Request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
                {
                    RemoteEndpointMessageProperty prop = (RemoteEndpointMessageProperty)Request.Properties[RemoteEndpointMessageProperty.Name];
                    return prop.Address;
                }
                else if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Request.UserHostAddress;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch(Exception ex)
            {
                return string.Empty;
            }
        }
        protected User GetUser()
        {
            // return GetUserDev();
            try
            {
                var ip = GetIp();
                if (ConfigurationManager.AppSettings["DevWriteLists"].Contains(ip))
                {
                    return GetUserDev();
                }
                var identity = (ClaimsIdentity)User.Identity;
                var username = identity?.Name;
                return unitOfWork.UserRepository.FirstOrDefault(m => !m.IsDeleted && m.Username == username);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("BaseApi.GetUser fail. Ex: {0}", ex));
                return null;
            }
        }
        /// <summary>
        /// Get current user name
        /// </summary>
        protected string CurrentUserName
        {
            get
            {
                try
                {
                    var ip = GetIp();
                    if (ConfigurationManager.AppSettings["DevWriteLists"].Contains(ip))
                    {
                        return GetUserDev()?.Username;
                    }
                    var identity = (ClaimsIdentity)User.Identity;
                    return identity?.Name;
                }
                catch (Exception ex)
                {
                    VM.Common.CustomLog.accesslog.Error(string.Format("BaseApi.GetUser fail. Ex: {0}", ex));
                    return string.Empty;
                }
            }
        }
        /// <summary>
        /// Lấy ra role cao nhất
        /// </summary>
        /// <returns></returns>
        protected Role GetTopRole(User crUser)
        {
            Role entity = null;
            if (crUser?.UserRoles?.Count > 0)
            {
                entity = crUser?.UserRoles?.Select(x => x.Role).OrderBy(x => x.Level).FirstOrDefault();
            }
            return entity;
        }
        private User GetUserDev()
        {
            return unitOfWork.UserRepository.FirstOrDefault(u => u.Username == "thangdc3");
        }
        public string currentLang { get; set; } = "vn";
        #endregion
    }
}