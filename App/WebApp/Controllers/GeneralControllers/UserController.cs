                                      using DataAccess.Models;
using PMS.Authentication;
using PMS.Contract.Models;
using PMS.Contract.Models.AdminModels;
using PMS.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using VM.Common;

namespace PMS.Controllers.GeneralControllers
{
    /// <summary>
    /// User Genera Module
    /// </summary>
    [SessionAuthorize]
    public class UserController : BaseApiController
    {
        /// <summary>
        /// API Get List User FN
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/User")]
        [Permission()]
        public IHttpActionResult GetUserAPI()
        {
            var user = GetUser();
            var user_actions = GetListAction(user.Id);
            var user_position = GetListPosition(user.Id);
            string _defaultPage = string.Empty;
            List<MenuModel> dMenus = new List<MenuModel>();
            List<Menu> dSubMenus = new List<Menu>();
            #region  Menu
            if (user.UserRoles.Count > 0)
            {
                #region Get Top role
                var topRole = GetTopRole(user);
                _defaultPage = topRole?.DefaultMenu?.Url;
                #endregion .Get Top role
                foreach (var ro in user.UserRoles)
                {

                    var grpMenu = ro.Role.RoleGroupActions.Where(x => !x.IsDeleted && x.GroupAction.IsMenu && x.GroupAction.IsDisplay)?.Select(r => r.GroupAction)?.ToList();
                    if (grpMenu?.Count > 0)
                    {
                        var groupLevel1 = grpMenu.GroupBy(x => x.Module).Select(x => x.Key);
                        if (groupLevel1.Any())
                        {
                            #region Group by Menu
                            var menuLv1 = groupLevel1.Select(x => new MenuModel()
                            {
                                Id = x.Id,
                                Code = x.Code,
                                Name = x.Name,
                                Level = 1,
                                Order = x.OrderDisplay
                            });
                            dMenus.AddRange(menuLv1.Where(x=> !dMenus.Any(y=>y.Id==x.Id)));
                            if (menuLv1.Any())
                            {
                                foreach (var item in menuLv1)
                                {
                                    var submenus = grpMenu.Where(y => y.ModuleId == item.Id).OrderBy(y => y.OrderDisplay).Select(y => new Menu()
                                    {
                                        Id = y.Id,
                                        ModuleId= y.ModuleId,
                                        Code = y.GroupActionCode,
                                        Name = y.GroupActionName,
                                        Level = 2,
                                        Url = y.Url,
                                        UrlTarget = y.UrlTarget,
                                        Order = y.OrderDisplay
                                    })?.ToList();
                                    if(submenus.Any())
                                        dSubMenus.AddRange(submenus.Where(x=> !dSubMenus.Any(y=>y.Id==x.Id)));
                                } 
                            }
                            #endregion
                        }
                    }
                }
            }
            dMenus.ForEach(x => x.SubMenus = (dSubMenus.Where(y => y.ModuleId == x.Id)?.OrderBy(z=>z.Order).ToList()));
            #endregion .Menu
            #region Get current Notify
            var notyEntity = unitOfWork.SystemNotificationRepository.FirstOrDefault(x => x.Id == user.NotifyID);
            #endregion
            return Content(HttpStatusCode.OK, new
            {
                user.Username,
                user.Fullname,
                Actions = user_actions,
                Positions = user_position,
                UserSites= user.UserSites.Select(x=>x.SiteId),
                Menus= dMenus?.OrderBy(x=>x.Order),
                DefaultPage= _defaultPage,
                Notify=new {
                    notyEntity?.Id,
                    notyEntity?.Service,
                    notyEntity?.Subject,
                    notyEntity?.Content
                }
            });
        }
        /// <summary>
        /// API get user info
        /// </summary>
        /// <param name="username"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/User/Info")]
        [Permission()]
        public IHttpActionResult GetUserInfoAPI([FromUri]string username, string type = "AD")
        {
            if (!string.IsNullOrEmpty(username))
            {
                if (type.Equals("EHOS"))
                {
                    User user = unitOfWork.UserRepository.FirstOrDefault(
                        e => !e.IsDeleted &&
                        !string.IsNullOrEmpty(e.EhosAccount) &&
                        e.EhosAccount.Equals(username)
                    );
                    if (user != null)
                        return Content(HttpStatusCode.OK, GetUserInfo(user));
                }
                else
                {
                    User user = unitOfWork.UserRepository.FirstOrDefault(
                    e => !e.IsDeleted &&
                    !string.IsNullOrEmpty(e.Username) &&
                    e.Username.Equals(username)
                );
                    if (user != null)
                        return Content(HttpStatusCode.OK, GetUserInfo(user));
                }
            }
            return Content(HttpStatusCode.BadRequest, Message.USER_NOT_FOUND);
        }


        private List<string> GetListAction(Guid user_id)
        {
            var actions = (from user_role in unitOfWork.UserRoleRepository.AsQueryable()
                           .Where(
                                e => e.UserId != null &&
                                e.UserId == user_id &&
                                e.RoleId != null
                            )
                           join role_grp_action in unitOfWork.RoleGroupActionRepository.AsQueryable() on user_role.RoleId equals role_grp_action.RoleId
                           join grp_action_map in unitOfWork.GroupAction_MapRepository.AsQueryable() on role_grp_action.GaId equals grp_action_map.GaId
                           select grp_action_map.Action.Code
                           ).Distinct().ToList();
            return actions;
        }
        private dynamic GetListPosition(Guid user_id)
        {
            return unitOfWork.UserPositionRepository
                .Include("Position")
                .Where(e => e.UserId != null && e.UserId == user_id)
                .Select(e => new { e.Id, e.Position.ViName, e.Position.EnName });
        }

        private UserModel GetUserInfo(User user)
        {
            var fullname = user.DisplayName;
            if (string.IsNullOrEmpty(fullname))
                fullname = user.Fullname;
            return new UserModel
            {
                Username = user.Username,
                Fullname = fullname,
                FullShortName = user.Fullname,
                Department = user.Department,
                Title = user.Title,
                Mobile = user.Mobile
            };
        }
    }
}