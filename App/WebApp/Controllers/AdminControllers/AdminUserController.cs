using DataAccess.Models;
using PMS.Authentication;
using PMS.Business.Provider;
using PMS.Contract.Models;
using PMS.Controllers.BaseControllers;
using PMS.Provider;
using Microsoft.Owin.Security;
using Newtonsoft.Json.Linq;
using System;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using VM.Common;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// User Module Management
    /// </summary>
    [SessionAuthorize]
    public class AdminUserController : BaseApiController
    {
        /// <summary>
        /// Get User by AD Information
        /// </summary>
        /// <param name="ad"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/User/AD")]
        [Permission()]
        public IHttpActionResult GetADAPI([FromUri]string ad)
        {
            if (string.IsNullOrEmpty(ad))
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            var valid_format = Regex.IsMatch(ad, @"^[a-zA-Z0-9.]+$");
            if (!valid_format)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            string ip = GetIp();
            if (IsADSpam(ip))
                return Content(HttpStatusCode.BadRequest, Message.FORBIDDEN);
            IncreaseADSpam(ip);

            var user = unitOfWork.UserRepository.FirstOrDefault(s => s.Username.ToLower() == ad.Trim().ToLower());
            if (user != null)
                return Content(HttpStatusCode.BadRequest, Message.USER_EXIST);

            

            using (UserRepo _repo = new UserRepo())
            {
                var result = _repo.GetUserADInfo(ad);
                if (result == null)
                    return Content(HttpStatusCode.BadRequest, Message.USER_NOT_FOUND);

                return Content(HttpStatusCode.OK, new
                {
                    Fullname = result.FullName,
                    result.Department,
                    result.Title,
                    result.DisplayName,
                });
            }
        }
        /// <summary>
        /// Get AD Information
        /// </summary>
        /// <param name="ad"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/User/ADInfo")]
        [Permission()]
        public IHttpActionResult GetADInfoAPI([FromUri]string ad)
        {
            if (string.IsNullOrEmpty(ad))
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            var valid_format = Regex.IsMatch(ad, @"^[a-zA-Z0-9.]+$");
            if (!valid_format)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            string ip = GetIp();
            if (IsADSpam(ip))
                return Content(HttpStatusCode.BadRequest, Message.FORBIDDEN);
            IncreaseADSpam(ip);
            using (UserRepo _repo = new UserRepo())
            {
                var result = _repo.GetUserADInfo(ad);
                if (result == null)
                    return Content(HttpStatusCode.BadRequest, Message.USER_NOT_FOUND);

                return Content(HttpStatusCode.OK, new
                {
                    Fullname = result.FullName,
                    result.Department,
                    result.Title,
                    result.DisplayName,
                });
            }
        }
        /// <summary>
        /// Get List User
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/User")]
        [Permission()]
        public IHttpActionResult GetListUserAPI([FromUri]UserListParameterModel request)
        {
            if (request == null) 
                return Content(HttpStatusCode.OK, Message.FORMAT_INVALID);

            var users = unitOfWork.UserRepository.AsQueryable();
            var search = request.Search?.Trim()?.ToLower();
            if (!string.IsNullOrEmpty(search))
                users = users.Where(
                    e => (!string.IsNullOrEmpty(e.Username) && e.Username.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(e.Fullname) && e.Fullname.ToLower().Contains(search))
                );
            int count = users.Count();
            var items = users.OrderBy(e => e.IsDeleted).ThenByDescending(e => e.UpdatedAt).Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new
                {
                    e.Id,
                    e.Username,
                    e.Fullname,
                    e.DisplayName,
                    e.Title,
                    e.IsDeleted,
                    Sites=e.UserSites.Select(x=>x.SiteId),
                    ListRoles = e.UserRoles.OrderBy(x=>x.Role.Level).Select(x => new { RoleId = x.Role.Id, RoleCode = x.Role.Code, RoleName = x.Role.ViName })
                });
            return Content(HttpStatusCode.OK, new { Count = count, Results = items });
        }
        /// <summary>
        /// Create User
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/User")]
        [Permission()]
        public IHttpActionResult CreateUserDetailAPI([FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            var username = request["Username"]?.ToString();
            if(string.IsNullOrEmpty(username))
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            User user = unitOfWork.UserRepository.FirstOrDefault(e => e.Username == username);
            if (user != null)
                return Content(HttpStatusCode.BadRequest, Message.USER_EXIST);
            var crUserTopRole = GetTopRole(GetUser());
            user = new User
            {
                Username = username,
                EhosAccount = request["EhosAccount"]?.ToString(),
                Fullname = request["Fullname"]?.ToString(),
                DisplayName = request["DisplayName"]?.ToString(),
                Department = request["Department"]?.ToString(),
                Title = request["Title"]?.ToString(),
            };
            unitOfWork.UserRepository.Add(user);

            unitOfWork.UserPositionRepository.HardDeleteRange(user.UserPositions.AsQueryable());
            foreach (var pos in request["Positions"])
                CreateUserPosition(user.Id, pos);

            unitOfWork.UserRoleRepository.HardDeleteRange(user.UserRoles.AsQueryable());
            int TopRoleLevel = -1;
            foreach (var role in request["Roles"])
            {
                Guid? roleId = role?.ToObject<Guid?>();
                if (roleId != null && roleId!=Guid.Empty)
                {
                    var role4Map = unitOfWork.RoleRepository.FirstOrDefault(x => x.Id == roleId);
                    if (role4Map != null)
                    {
                        TopRoleLevel = TopRoleLevel==-1 || TopRoleLevel> role4Map.Level? role4Map.Level: TopRoleLevel;
                        CreateUserRole(user.Id, role);
                    }
                }
                if(TopRoleLevel<= crUserTopRole.Level && crUserTopRole.Level != 1)
                {
                    return Content(HttpStatusCode.BadRequest, Message.CREATE_UPDATE_USER_OVERROLE);
                }
            }

            //Gán Site
            unitOfWork.UserSiteRepository.HardDeleteRange(user.UserSites.AsQueryable());
            if (request["Sites"] != null)
            {
                foreach (var site in request["Sites"])
                    CreateUserSite(user.Id, site);
            }

            unitOfWork.Commit();

            return Content(HttpStatusCode.OK, new { user.Id });
        }

        /// <summary>
        /// Get User detail information
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/User/{id}")]
        [Permission()]
        public IHttpActionResult GetUserDetailAPI(Guid id)
        {
            var user = unitOfWork.UserRepository.FirstOrDefault(e => e.Id == id);
            if (user == null)
                return Content(HttpStatusCode.BadRequest, Message.USER_NOT_FOUND);

            return Content(HttpStatusCode.OK, new {
                user.Id,
                user.Username,
                user.EhosAccount,
                user.Fullname,
                user.DisplayName,
                user.Department,
                user.Title,
                user.Mobile,
                user.IsDeleted,
                Positions = user.UserPositions.Select(e => e.PositionId),
                Roles = user.UserRoles.Select(e => e.RoleId),
                Sites=user.UserSites.Select(e=>e.SiteId)
            });
        }
        /// <summary>
        /// Update User information
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/User/{id}")]
        [Permission()]
        public IHttpActionResult UpdateUserAPI(Guid id, [FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.OK, Message.FORMAT_INVALID);

            var user = unitOfWork.UserRepository.FirstOrDefault(e => e.Id == id);
            if (user == null)
                return Content(HttpStatusCode.BadRequest, Message.USER_NOT_FOUND);

            var crUserTopRole = GetTopRole(GetUser());
            var is_deleted = request["IsDeleted"].ToObject<bool>();
            if (is_deleted)
                unitOfWork.UserRepository.Delete(user);
            else
            {
                user.EhosAccount = request["EhosAccount"].ToString();
                user.IsDeleted = is_deleted;
                unitOfWork.UserPositionRepository.HardDeleteRange(user.UserPositions.AsQueryable());
                foreach (var pos in request["Positions"])
                    CreateUserPosition(user.Id, pos);

                #region Check role hiện đang có của User được sửa
                var roleWasEdit= GetTopRole(user);
                if(roleWasEdit?.Level<= crUserTopRole?.Level && crUserTopRole?.Level != 1)
                {
                    return Content(HttpStatusCode.BadRequest, Message.CREATE_UPDATE_USER_OVERROLE);
                }
                #endregion . Check role hiện đang có của User được sửa
                unitOfWork.UserRoleRepository.HardDeleteRange(user.UserRoles.AsQueryable());
                int TopRoleLevel = -1;
                foreach (var role in request["Roles"])
                {
                    Guid? roleId = role?.ToObject<Guid?>();
                    if (roleId != null && roleId != Guid.Empty)
                    {
                        var role4Map = unitOfWork.RoleRepository.FirstOrDefault(x => x.Id == roleId);
                        if (role4Map != null)
                        {
                            TopRoleLevel = TopRoleLevel == -1 || TopRoleLevel > role4Map.Level ? role4Map.Level : TopRoleLevel;
                            CreateUserRole(user.Id, role);
                        }
                    }
                    if (TopRoleLevel <= crUserTopRole?.Level && crUserTopRole?.Level!=1)
                    {
                        return Content(HttpStatusCode.BadRequest, Message.CREATE_UPDATE_USER_OVERROLE);
                    }
                }
                    
                //Gán Site
                unitOfWork.UserSiteRepository.HardDeleteRange(user.UserSites.AsQueryable());
                foreach (var site in request["Sites"])
                    CreateUserSite(user.Id, site);
            }
            new BusinessHelper(unitOfWork).ClearSessionInDBByUserID(user.Id);
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }
        /// <summary>
        /// API get Notify
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/User/Notify")]
        [Permission()]
        public IHttpActionResult GetNotifyAPI()
        {
            var user = unitOfWork.UserRepository.FirstOrDefault(e => e.Username == CurrentUserName);
            if (user != null && user.NotifyID!=null)
            {
                var notyEntity = unitOfWork.SystemNotificationRepository.FirstOrDefault(x => x.Id == user.NotifyID);
                if (notyEntity != null)
                {
                    return Content(HttpStatusCode.OK, new
                    {
                        notyEntity.Id,
                        notyEntity.Service,
                        notyEntity.Subject,
                        notyEntity.Content
                    });
                }
            }
            return Content(HttpStatusCode.NoContent, Message.NOT_FOUND);
        }
        /// <summary>
        /// API set is read notify
        /// </summary>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/User/IsReadNofify/{id}")]
        [Permission()]
        public IHttpActionResult UpdateReadNofifyAPI(Guid id)
        {
            try
            {
                var user = unitOfWork.UserRepository.FirstOrDefault(e => e.Username == CurrentUserName);
                if (user == null)
                    return Content(HttpStatusCode.BadRequest, Message.USER_NOT_FOUND);
                user.NotifyID = user.NotifyID == id ? null : user.NotifyID;
                unitOfWork.Commit();
                return Content(HttpStatusCode.OK, Message.SUCCESS);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("UpdateReadNofifyAPI fail. Ex: {0}", ex));
                if (ex != null && ex.InnerException != null && ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message.Contains("The duplicate key"))
                {
                    return Content(HttpStatusCode.BadRequest, Message.CODE_DUPLICATE);
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
                }
            }
        }
        private void CreateUserPosition(Guid user_id, JToken pos)
        {
            Guid pos_id = new Guid(pos.ToString());
            var user_pos = new UserPosition
            {
                UserId = user_id,
                PositionId = pos_id
            };
            unitOfWork.UserPositionRepository.Add(user_pos);
        }
        private void CreateUserRole(Guid user_id, JToken role)
        {
            Guid role_id = new Guid(role.ToString());
            var user_role = new UserRole
            {
                UserId = user_id,
                RoleId = role_id
            };
            unitOfWork.UserRoleRepository.Add(user_role);
        }
        private void CreateUserSite(Guid user_id, JToken site)
        {
            Guid site_id = new Guid(site.ToString());
            var user_site = new UserSite
            {
                UserId = user_id,
                SiteId = site_id
            };
            unitOfWork.UserSiteRepository.Add(user_site);
        }


        private bool IsADSpam(string ip)
        {
            string ad_ip = $"AD{ip}";
            var time = DateTime.Now.AddMinutes(-2);
            var login_fail = unitOfWork.LogInFailRepository.FirstOrDefault(
                e => !string.IsNullOrEmpty(e.IPAddress) &&
                e.IPAddress == ad_ip &&
                e.Time >= 10 &&
                e.CreatedAt > time
            );
            return login_fail != null;
        }
        private void IncreaseADSpam(string ip)
        {
            string ad_ip = $"AD{ip}";
            var time = DateTime.Now.AddMinutes(-2);
            var login_fail = unitOfWork.LogInFailRepository.FirstOrDefault(
                e => !string.IsNullOrEmpty(e.IPAddress) &&
                e.IPAddress == ad_ip
            );
            if (login_fail != null)
            {
                if (login_fail.CreatedAt < time)
                {
                    login_fail.Time = 1;
                    login_fail.CreatedAt = DateTime.Now;
                }
                else
                {
                    login_fail.Time += 1;
                    unitOfWork.LogInFailRepository.Update(login_fail);
                }
            }
            else
            {
                var new_login_fail = new LogInFail()
                {
                    IPAddress = ad_ip,
                    Time = 1,
                };
                unitOfWork.LogInFailRepository.Add(new_login_fail);
            }
            unitOfWork.Commit();
        }
        //private ADUserDetailModel GetUserADInfo(string userName, string domainName = "vingroup.local", string container = "DC=VINGROUP,DC=LOCAL")
        //{
        //    try
        //    {
        //        PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName, container);
        //        UserPrincipal userPrincipal = new UserPrincipal(domainContext);
        //        userPrincipal.SamAccountName = userName;
        //        PrincipalSearcher principleSearch = new PrincipalSearcher();
        //        principleSearch.QueryFilter = userPrincipal;
        //        PrincipalSearchResult<Principal> results = principleSearch.FindAll();
        //        Principal principle = results.ToList()[0];
        //        DirectoryEntry directory = (DirectoryEntry)principle.GetUnderlyingObject();
        //        return ADUserDetailModel.GetUser(directory);
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }
        //}
    }
}