using DataAccess.Models;
using PMS.Authentication;
using PMS.Contract.Models;
using PMS.Controllers.BaseControllers;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using VM.Common;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// Amin role management
    /// </summary>
    [SessionAuthorize]
    public class AdminRoleController: BaseApiController
    {
        #region Role management
        /// <summary>
        /// API Get list Role
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Role")]
        [Permission()]
        public IHttpActionResult GetListRoleAPI([FromUri]UserListParameterModel request)
        {
            var search = request.Search?.ToLower().Trim();
            var roles = unitOfWork.RoleRepository.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                roles = roles.Where(e => e.ViName.ToLower().Contains(search));

            int count = roles.Count();
            
            var results = roles.OrderBy(e => e.Level)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new { e.Id, e.ViName, e.EnName });

            return Content(HttpStatusCode.OK, new { Count = count, Results = results });
        }
        /// <summary>
        /// API Create role
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Role")]
        [Permission()]
        public IHttpActionResult CreateRoleAPI([FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.OK, Message.FORMAT_INVALID);

            var role = new Role
            {
                ViName = request["ViName"].ToString(),
                EnName = request["EnName"].ToString(),
            };
            var totalRole = unitOfWork.RoleRepository.Count(x => x.Id != null);
            role.Level = totalRole + 1;
            unitOfWork.RoleRepository.Add(role);
            foreach (var grpAct in request["GroupActions"])
                CreateRoleGroupAction(role.Id, grpAct);
            unitOfWork.Commit();

            return Content(HttpStatusCode.OK, new { role.Id });
        }
        /// <summary>
        /// API view role detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Role/{id}")]
        [Permission()]
        public IHttpActionResult GetDetailRoleAPI(Guid id)
        {
            var role = unitOfWork.RoleRepository.GetById(id);
            if (role == null)
                return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);

            return Content(HttpStatusCode.OK, new {
                role.Id,
                role.ViName,
                role.EnName,
                GroupActions = role.RoleGroupActions.Select(e => new { e.GroupAction.Id,e.GroupAction.GroupActionCode,e.GroupAction.GroupActionName,e.GroupAction.IsMenu,e.GroupAction.IsDisplay,e.GroupAction.OrderDisplay, e.GroupAction.IsDeleted }),
            });
        }
        /// <summary>
        /// API Update role
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Role/{id}")]
        [Permission()]
        public IHttpActionResult UpdateRoleAPI(Guid id, [FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.OK, Message.FORMAT_INVALID);

            var role = unitOfWork.RoleRepository.GetById(id);
            if (role == null)
                return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);

            role.ViName = request["ViName"].ToString();
            role.EnName = request["EnName"].ToString();

            unitOfWork.RoleGroupActionRepository.HardDeleteRange(role.RoleGroupActions.AsQueryable());
            foreach (var grpAct in request["GroupActions"])
                CreateRoleGroupAction(role.Id, grpAct);

            new BusinessHelper(unitOfWork).ClearSessionInDBByRoleId(id);
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }
        #endregion .Role management
        #region Function Helper
        private void CreateRoleGroupAction(Guid role_id, JToken grpAct)
        {
            Guid pos_id = new Guid(grpAct.ToString());
            var role_grpAct = new RoleGroupAction
            {
                RoleId = role_id,
                GaId = pos_id
            };
            unitOfWork.RoleGroupActionRepository.Add(role_grpAct);
        }
        #endregion .Function Helper
    }
}