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
using PMS.Business.Provider;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// Module Package Group Management
    /// </summary>
    [SessionAuthorize]
    public class AdminPackageGroupController : BaseApiController
    {
        readonly string CodeRegex = @"^[a-zA-Z0-9._-]{2,50}$";
        /// <summary>
        /// API Get List PackageGroup
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/PackageGroup")]
        [Permission()]
        public IHttpActionResult GetListPackageGroupAPI([FromUri]PackageGroupParameterModel request)
        {
            var iQuery = new PackageGroupRepo().GetPackageGroups(request);

            int count = iQuery.Count();
            
            var results = iQuery.OrderBy(e => e.Code)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new { e.Id,e.ParentId, e.Code, e.Name,e.Level,e.IsActived });

            return Content(HttpStatusCode.OK, new { Count = count, Results = results });
        }
        /// <summary>
        /// API Create New PackageGroup
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/PackageGroup")]
        [Permission()]
        public IHttpActionResult CreatePackageGroupAPI([FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            try
            {
                var code = request["Code"]?.ToString();
                Regex regex = new Regex(CodeRegex);
                Match match = regex.Match(code);
                if (!match.Success)
                {
                    return Content(HttpStatusCode.BadRequest, Message.FORMAT_CODE_INVALID);
                }
                var parentId = request["ParentId"]?.ToObject<Guid?>();
                int iLevel = 1;
                if (parentId != null)
                {
                    var entityParent = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == parentId);
                    if (entityParent != null)
                    {
                        iLevel = entityParent.Level + 1;
                    }
                }
                var entity = new PackageGroup
                {
                    ParentId = parentId,
                    Code = code.ToUpper(),
                    Name = request["Name"].ToString(),
                    Level = iLevel,
                    IsActived = request["IsActived"].ToObject<bool>()
                };
                unitOfWork.PackageGroupRepository.Add(entity);
                unitOfWork.Commit();

                return Content(HttpStatusCode.OK, new { entity.Id });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CreatePackageGroupAPI fail. Ex: {0}", ex));
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
        /// <summary>
        /// API Update PackageGroup
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/PackageGroup/{id}")]
        [Permission()]
        public IHttpActionResult UpdatePackageGroupAPI(Guid id, [FromBody]JObject request)
        {
            try
            {
                var entity = unitOfWork.PackageGroupRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
                if (entity == null)
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);

                var code = request["Code"]?.ToString();
                Regex regex = new Regex(CodeRegex);
                Match match = regex.Match(code);
                if (!match.Success)
                {
                    return Content(HttpStatusCode.BadRequest, Message.FORMAT_CODE_INVALID);
                }

                var parentId = request["ParentId"]?.ToObject<Guid?>();
                int iLevel = 1;
                if (parentId != null)
                {
                    #region Check change parrent
                    if (entity.Level == 1)
                    {
                        return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ROOTGROUP);
                    }
                    var newParent = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == parentId);
                    if (newParent != null)
                    {
                        iLevel = newParent.Level + 1;
                    }
                    //get current root
                    var _repo = new PackageGroupRepo();
                    var currentRoot = _repo.GetPackageGroupRoot(entity);
                    if (currentRoot == null)
                    {
                        return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_PACKAGEGROUP_OLD);
                    }
                    var newRoot = _repo.GetPackageGroupRoot(newParent);
                    if (newRoot == null)
                    {
                        return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_PACKAGEGROUP);
                    }
                    if(newRoot?.Id != currentRoot?.Id)
                    {
                        //Không cùng root
                        return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ROOTGROUP);
                    }
                    #endregion .Check change parrent
                    
                }
                entity.ParentId = parentId;
                entity.Code = code.ToUpper();
                entity.Name = request["Name"].ToString();
                entity.Level = iLevel;
                entity.IsActived = request["IsActived"].ToObject<bool>();
                #region Update Children
                UpdatePackageGroupChild(entity);
                #endregion .Update Children

                unitOfWork.PackageGroupRepository.Update(entity);
                unitOfWork.Commit();

                return Content(HttpStatusCode.OK, Message.SUCCESS);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("UpdatePackageGroupAPI fail. Ex: {0}", ex));
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
        /// <summary>
        /// API get detail PackageGroup
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/PackageGroup/{id}")]
        [Permission()]
        public IHttpActionResult GetPackageGroupDetailAPI(Guid id)
        {
            var entity = unitOfWork.PackageGroupRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
            if (entity == null)
                return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
            return Content(HttpStatusCode.OK, new
            {
                entity.Id,
                entity.ParentId,
                entity.Name,
                entity.Code,
                entity.IsActived
            });
        }
        /// <summary>
        /// API Delete PackageGroup.
        /// Can be support multi delection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/PackageGroup/Delete")]
        [Permission()]
        public IHttpActionResult DeletePackageGroupAPI([FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            foreach (var s_id in request["Ids"])
            {
                try
                {
                    var id = new Guid(s_id.ToString());
                    var entity = unitOfWork.PackageGroupRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
                    if (entity != null)
                        unitOfWork.PackageGroupRepository.Delete(entity);
                }
                catch { }
            }
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        #region Function 4 Helper
        private void UpdatePackageGroupChild(PackageGroup parrent)
        {
            var listChild = unitOfWork.PackageGroupRepository.Find(x => x.ParentId == parrent.Id);
            if (listChild.Any())
            {
                foreach (var item in listChild)
                {
                    item.Level = parrent.Level + 1;
                    item.IsActived = parrent.IsActived;
                    item.IsDeleted = parrent.IsDeleted;
                    unitOfWork.PackageGroupRepository.Update(item);
                    UpdatePackageGroupChild(item);
                }
            }
        }
        #endregion .Function 4 Helper
    }
}