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
using PMS.Contract.Models.AdminModels;
using PMS.Contract.Models.Enum;
using PMS.Business.Connection;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// Module Common/General/Masterdata Management
    /// </summary>
    [SessionAuthorize]
    public class AdminCommonController : BaseApiController
    {
        readonly string SubjectRegex = @"[!@#$%^&*()\=\[\]{};':\\|,.<>\/?]";
        /// <summary>
        /// Get list department
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Common/Department")]
        [Permission()]
        public IHttpActionResult GetListDepartmentAPI([FromUri]DepartmentParameterModel request)
        {
            if (string.IsNullOrEmpty(request?.SiteCode))
                return Content(HttpStatusCode.OK, Message.FORMAT_INVALID);

            var xquery = unitOfWork.DepartmentRepository.AsQueryable();
            if (!string.IsNullOrEmpty(request?.Search))
                xquery = xquery.Where(
                    e => e.ViName.Contains(request.Search) || e.EnName.Contains(request.Search)
                );
            if (!string.IsNullOrEmpty(request?.SiteCode))
                xquery = xquery.Where(
                    e => e.HospitalCode.Contains(request.SiteCode)
                );
            int count = xquery.Count();
            var items = xquery.OrderBy(e => e.ViName).Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new
                {
                    e.Id,
                    e.ViName,
                    e.EnName,
                    e.Code,
                    e.HospitalCode
                });
            return Content(HttpStatusCode.OK, new { Count = count, Results = items });
        }
        #region Notification
        /// <summary>
        /// API Create New PackageGroup
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Common/Notify")]
        [Permission()]
        public IHttpActionResult CreateNotifyAPI([FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            try
            {
                var service = request["Service"]?.ToString();
                var subject = request["Subject"]?.ToString();
                var content = request["Content"]?.ToString();
                if(string.IsNullOrEmpty(service) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(content))
                {
                    return Content(HttpStatusCode.BadRequest, Message.REQUIRE);
                }
                var regex = new Regex(SubjectRegex);
                var match = regex.Match(subject);
                if (match.Success)
                {
                    //Có chứa các ký tự đặc biết
                    return Content(HttpStatusCode.BadRequest, Message.FORMAT_NAME_INVALID);
                }
                var scope = Request.RequestUri.Authority;
                var entity = new SystemNotification
                {
                    Service = service,
                    Subject = subject,
                    Content = content,
                    Scope= scope,
                    Status = 1
                };
                unitOfWork.SystemNotificationRepository.Add(entity);
                if(entity.Service == Constant.SERVICE_APP)
                {
                    //Update status for all User is unread notify
                    var userXQuery=unitOfWork.UserRepository.Find(x => !x.IsDeleted);
                    userXQuery.Select(x => x.NotifyID = entity.Id)?.ToList();
                }
                unitOfWork.Commit();

                return Content(HttpStatusCode.OK, new { entity.Id });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CreateNotifyAPI fail. Ex: {0}", ex));
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
        #endregion
    }
}