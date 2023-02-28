using DataAccess.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PMS.Authentication;
using PMS.Contract.Models;
using PMS.Controllers.BaseControllers;
using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using VM.Common;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// Module Action Managment
    /// </summary>
    [SessionAuthorize]
    public class AdminActionController : BaseApiController
    {
        #region Module management
        /// <summary>
        /// API Get list modules FN
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Modules")]
        [Permission()]
        public IHttpActionResult GetListModuleAPI([FromUri]GroupActionParameterModel request)
        {
            var xquery = unitOfWork.ModuleRepository.AsQueryable().Where(x => !x.IsDeleted);
            if (request.IsDisplay != -1)
            {
                xquery = xquery.Where(x => x.IsDisplay == request.IsDisplay > 0 ? true : false);
            }
            if (!string.IsNullOrEmpty(request.keyword))
            {
                xquery = xquery.Where(x => x.Name.Contains(request.keyword) || x.Code.Contains(request.keyword));
            }
            var entities = xquery.Select(e => new { e.Id, e.Code, e.Name, e.OrderDisplay }).OrderBy(x=>x.OrderDisplay);
            return Content(HttpStatusCode.OK, entities);
        }
        #endregion .Module management
        #region Group Action Managment
        /// <summary>
        /// API list Group Action FN
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/GroupAction")]
        [Permission()]
        public IHttpActionResult GetListGroupActionAPI([FromUri]GroupActionParameterModel request)
        {
            var xquery = unitOfWork.GroupActionRepository.AsQueryable().Where(x => !x.IsDeleted);
            //if (request.IsDisplay != -1)
            //{
            //    xquery = xquery.Where(x => x.IsDisplay == request.IsDisplay > 0 ? true : false);
            //}
            if (!string.IsNullOrEmpty(request.keyword))
            {
                xquery = xquery.Where(x => x.GroupActionName.Contains(request.keyword) || x.GroupActionCode.Contains(request.keyword));
            }
            if (!string.IsNullOrEmpty(request.Code))
            {
                xquery = xquery.Where(x => x.GroupActionCode.Contains(request.Code));
            }
            if (!string.IsNullOrEmpty(request.Name))
            {
                xquery = xquery.Where(x => x.GroupActionName.Contains(request.Name));
            }
            var entities = xquery.Select(e => new { e.Id, e.GroupActionName, e.GroupActionCode, e.IsMenu, e.OrderDisplay,ModuleId=e.Module.Id, ModuleCode = e.Module.Code, ModuleName=e.Module.Name, ModuleOrder=e.Module.OrderDisplay}).OrderBy(x=>x.ModuleOrder).ThenBy(x=>x.OrderDisplay);
            return Content(HttpStatusCode.OK, entities);
        }
        /// <summary>
        /// API Get group action detail FN
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/GroupAction/{id}")]
        [Permission()]
        public IHttpActionResult GetDetailGroupActionAPI(Guid id)
        {
            var entity = unitOfWork.GroupActionRepository.GetById(id);
            if (entity == null)
                return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);

            return Content(HttpStatusCode.OK, new
            {
                entity.Id,
                entity.GroupActionCode,
                entity.GroupActionName,
                Module =new {
                    entity.Module.Id,
                    entity.Module.Code,
                    entity.Module.Name,
                    entity.Module.IsDisplay,
                    entity.OrderDisplay
                },
               ListAction= entity.GroupAction_Maps.Select(e =>new { ActionId=e.Action.Id,ActionCode=e.Action.Code, ActionName=e.Action.Name,e.IsDeleted })
            });
        }
        /// <summary>
        /// API Maping action into group FN
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/GroupActionMapping/{id}")]
        [Permission()]
        public IHttpActionResult MappingGroupActionAPI(Guid id, [FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.OK, Message.FORMAT_INVALID);
            GroupAction grp = null;
            if (id == Guid.Empty)
            {
                grp = new GroupAction()
                {
                    ModuleId = request["ModuleId"].ToObject<Guid>(),
                    GroupActionName = request["Name"].ToString(),
                    GroupActionCode = request["Code"]?.ToString()
                };
                grp.GroupActionCode = !string.IsNullOrEmpty(grp.GroupActionCode) ? grp.GroupActionCode : StringHelper.ReplaceSpace(grp.GroupActionName, " ","");
                var grpCheck = unitOfWork.GroupActionRepository.FirstOrDefault(x => x.GroupActionCode.ToLower() == grp.GroupActionCode.ToLower());
                if (grpCheck != null)
                {
                    grp = grpCheck;
                }
                else
                    unitOfWork.GroupActionRepository.Add(grp);
            }
            else
            {
                grp = unitOfWork.GroupActionRepository.GetById(id);
                grp.ModuleId = request["ModuleId"].ToObject<Guid>();
                unitOfWork.GroupActionRepository.Update(grp);
            }
            
            if (grp == null)
                return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);

            unitOfWork.GroupAction_MapRepository.HardDeleteRange(grp.GroupAction_Maps.AsQueryable());
            foreach (var act in request["Actions"])
                CreateGroupActionMapping(grp.Id, act);

            new BusinessHelper(unitOfWork).ClearSessionInDBByRoleId(id);
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }
        #endregion . Group Action Managment
        #region Function Helper
        private void CreateGroupActionMapping(Guid grp_id, JToken act)
        {
            Guid pos_id = new Guid(act.ToString());
            var grp_act = new GroupAction_Map
            {
                GaId = grp_id,
                AId = pos_id
            };
            unitOfWork.GroupAction_MapRepository.Add(grp_act);
        }
        #endregion .Function Helper
        #region Action managment
        /// <summary>
        /// Get List Action FN
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Action")]
        [Permission()]
        public IHttpActionResult GetListActionAPI([FromUri]PagingParameterModel request)
        {
            var entities = unitOfWork.ActionRepository.AsQueryable().Select(e => new { e.Id, e.Name, e.Code });

            return Content(HttpStatusCode.OK, entities);
        }
        /// <summary>
        /// Get List Action Api Ver2
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IHttpActionResult GetListActionAPIV2([FromUri]PagingParameterModel request)
        {
            var roles = unitOfWork.ActionRepository.AsQueryable().Select(e => new { e.Id, e.Name });
            return Content(HttpStatusCode.OK, roles);
        }
        #endregion .Action managment
    }
}