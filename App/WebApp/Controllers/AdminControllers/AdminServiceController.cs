using DataAccess.Models;
using Newtonsoft.Json.Linq;
using PMS.Authentication;
using PMS.Business.Provider;
using PMS.Contract.Models.AdminModels;
using PMS.Controllers.BaseControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using VM.Common;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// Module Service
    /// </summary>
    [SessionAuthorize]
    public class AdminServiceController : BaseApiController
    {
        /// <summary>
        /// Get List Service API
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Service")]
        [Permission()]
        public IHttpActionResult GetServiceAPI([FromUri]ServiceParameterModel request)
        {
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            var results = new ServiceRepo().GetServices(request);

            var count = results.Count();
            results = results.OrderBy(e => e.Code)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            return Content(HttpStatusCode.OK, new { Count = count, Results = results });
        }
        /// <summary>
        /// Get List Service Group API
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/ServiceGroup")]
        [Permission()]
        public IHttpActionResult GetServiceGroupAPI([FromUri]string query)
        {
            var groups = unitOfWork.ServiceGroupRepository.Find(e => !e.IsDeleted);
            if (!string.IsNullOrEmpty(query))
            {
                var search = query.ToLower().Trim();
                groups = groups.Where(
                    e => (e.Code.ToLower().Contains(search)) ||
                    (e.ViName.ToLower().Contains(search)) ||
                    (e.EnName.ToLower().Contains(search))
                );
            };

            return Content(HttpStatusCode.OK, groups.Select(e => new
            {
                e.Id,
                e.Code,
                e.ViName,
                e.EnName
            }));
        }
        /// <summary>
        /// Get List Service Category API
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/ServiceCategory")]
        [Permission()]
        public IHttpActionResult GetServiceCategoryAPI([FromUri]string query)
        {
            var categories = unitOfWork.ServiceCategoryRepository.AsQueryable();
            if (!string.IsNullOrEmpty(query))
            {
                var search = query.ToLower().Trim();
                categories = categories.Where(
                    e =>
                    //e.IsShow && 
                    (e.ViName.ToLower().Contains(search) ||
                    e.EnName.ToLower().Contains(search))
                );
            }
            //else
            //{
            //    categories = categories.Where(
            //        e => e.IsShow
            //    );
            //}

            return Content(HttpStatusCode.OK, categories.OrderBy(x => x.Order).Select(e => new
            {
                e.Id,
                e.ViName,
                e.EnName,
                e.Code,
                e.IsConfig,
                e.IsShow,
                e.Order
            }));
        }
		/// <summary>
		/// API Get List ServiceFreeInPackage
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("admin/ServiceFreeInPackage")]
        [Permission()]
        public IHttpActionResult GetListServiceFreeInPackageAPI([FromUri] ServiceFreeInPackageParameterModel request)
		{
			var iQuery = unitOfWork.ServiceFreeInPackageRepository.AsQueryable();

			if (request.Code != null)
				iQuery = iQuery.Where(e => e.Service.Code.Contains(request.Code));
			if (request.Name != null)
				iQuery = iQuery.Where(e => e.Service.ViName.Contains(request.Name));
			if (request.SearchGroup != null)
			{
				var packageGroup = unitOfWork.PackageGroupRepository.Find(x => (x.Code.Contains(request.SearchGroup) || x.Name.Contains(request.SearchGroup)) && x.IsActived && !x.IsDeleted).Select(x => x.Code).ToList();
				if (packageGroup.Count > 0)
				{
					iQuery = iQuery.Where(e => packageGroup.Contains(e.GroupCode));
				}
				else
				{
					iQuery = iQuery.Where(e => string.IsNullOrEmpty(e.GroupCode));
				}
			}
			var returnValue = new List<ServiceFreeInPackageRepo>();
			if (iQuery.Any())
			{
				foreach (var item in iQuery)
				{
					var packageGroup = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Code == item.GroupCode && x.IsActived && !x.IsDeleted);
					returnValue.Add(new ServiceFreeInPackageRepo()
					{
						Id = item.Id,
						ServiceId = item.ServiceId,
						ServiceCode = item.Service.Code,
						ServiceName = item.Service.ViName,
						GroupCode = item.GroupCode,
						GroupName = packageGroup != null ? packageGroup.Name : "",
						IsActive = item.IsActived
					});
				}
			}

			int count = returnValue.Count();

			var results = returnValue.OrderBy(e => e.ServiceCode)
				.Skip((request.PageNumber - 1) * request.PageSize)
				.Take(request.PageSize)
				.Select(e => new {
					e.Id,
					e.ServiceId,
					e.ServiceCode,
					e.ServiceName,
					e.GroupCode,
					e.GroupName,
					e.IsActive
				});
			return Content(HttpStatusCode.OK, new { Count = count, Results = results });
		}

		/// <summary>
		/// API get detail Package
		/// </summary>
		/// <param name="serviceId"></param>
		/// <returns></returns>
		[HttpGet]
		[Route("admin/ServiceFreeInPackage/{serviceId}")]
		[Permission()]
		public IHttpActionResult GetServiceFreeInPackageDetailAPI(Guid serviceId)
		{
			var entity = unitOfWork.ServiceFreeInPackageRepository.Find(e => !e.IsDeleted && e.ServiceId == serviceId);
			var returnValue = new ServiceFreeInPackageRepo();
			if (!entity.Any())
				return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
			var listPackageGroup = new List<PackageGroup>();
			foreach (var item in entity)
			{
				if (returnValue.ServiceId == null)
				{
					returnValue.ServiceId = item.ServiceId;
					returnValue.ServiceCode = item.Service.Code;
					returnValue.ServiceName = item.Service.ViName;
					returnValue.IsActive = item.IsActived;
				}
				var packageGroupEntity = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Code == item.GroupCode);
				listPackageGroup.Add(packageGroupEntity);
			}
			return Content(HttpStatusCode.OK, new
            {
				returnValue.Id,
				returnValue.ServiceId,
				returnValue.ServiceCode,
				returnValue.ServiceName,
				returnValue.IsActive,
				PackageGroups = listPackageGroup
			});
		}

		/// <summary>
		/// API Create New ServiceFreeInPackage
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[CSRFCheck]
		[HttpPost]
		[Route("admin/ServiceFreeInPackage")]
		[Permission()]
		public IHttpActionResult CreateServiceFreeInPackageAPI([FromBody] JObject request)
		{
			if (request == null)
				return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
			try
			{
				var serviceId = request["ServiceId"].ToObject<Guid>();
				var entity = unitOfWork.ServiceFreeInPackageRepository.FirstOrDefault(e => e.ServiceId == serviceId);
				if (entity != null)
					return Content(HttpStatusCode.BadRequest, Message.CODE_DUPLICATE);

                if (request["PackageGroups"] != null)
                {
					foreach (var pkGroup in request["PackageGroups"])
					{
						var pkGroupId = new Guid(pkGroup["Id"].ToString());
						var pkGroupEntity = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == pkGroupId &&  x.IsActived && !x.IsDeleted);
                        if (pkGroupEntity != null)
                        {
							entity = new ServiceFreeInPackage
							{
								ServiceId = serviceId,
								GroupCode = pkGroupEntity.Code,
                                IsActived = request["IsActived"].ToObject<bool>()
                            };
							unitOfWork.ServiceFreeInPackageRepository.Add(entity);
						}
					}
					
					unitOfWork.Commit();
				}
				else
                {
					var pkGroupEntitys = unitOfWork.PackageGroupRepository.Find(x => x.IsActived && !x.IsDeleted).ToList();
                    if (pkGroupEntitys.Count > 0)
                    {
						foreach (var pkGroup in pkGroupEntitys)
						{
							entity = new ServiceFreeInPackage
							{
								ServiceId = serviceId,
								GroupCode = pkGroup.Code,
                                IsActived = request["IsActived"].ToObject<bool>()
                            };
							unitOfWork.ServiceFreeInPackageRepository.Add(entity);
						}
						
						unitOfWork.Commit();
					}
				}

				return Content(HttpStatusCode.OK, new { entity.Id });
			}
			catch (Exception ex)
			{
				VM.Common.CustomLog.accesslog.Error(string.Format("CreatePackageAPI fail. Ex: {0}", ex));
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
		/// API Update ServiceFreeInPackage
		/// </summary>
		/// <param name="serviceId"></param>
		/// <param name="request"></param>
		/// <returns></returns>
		[CSRFCheck]
		[HttpPost]
		[Route("admin/ServiceFreeInPackage/{serviceId}")]
		[Permission()]
		public IHttpActionResult UpdateServiceFreeInPackageAPI(Guid serviceId, [FromBody] JObject request)
		{
			try
			{
				#region Valid Data
				if (request == null)
				{
					return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
				}
				var entity = unitOfWork.ServiceFreeInPackageRepository.FirstOrDefault(e => !e.IsDeleted && e.ServiceId == serviceId);
				if (entity == null)
				{
					return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
				}
				#endregion .Valid Data
				//var serviceId = new Guid(request["ServiceId"].ToString());
				if (request["PackageGroups"] != null)
                {
					foreach (var pkGroup in request["PackageGroups"])
					{
						var pkGroupId = new Guid(pkGroup["Id"].ToString());
						var pkGroupEntity = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == pkGroupId && x.IsActived && !x.IsDeleted);
						if (pkGroupEntity != null)
						{
							entity.GroupCode = pkGroupEntity.Code;
							unitOfWork.ServiceFreeInPackageRepository.Update(entity);
						}
					}
					entity.IsActived = request["IsActived"].ToObject<bool>();
					unitOfWork.Commit();
				}
				else
				{
					var pkGroupEntitys = unitOfWork.PackageGroupRepository.Find(x => x.IsActived && !x.IsDeleted).ToList();
					if (pkGroupEntitys.Count > 0)
					{
						foreach (var pkGroup in pkGroupEntitys)
						{
							entity.GroupCode = pkGroup.Code;
							unitOfWork.ServiceFreeInPackageRepository.Update(entity);
						}
						entity.IsActived = request["IsActived"].ToObject<bool>();
						unitOfWork.Commit();
					}
				}

				return Content(HttpStatusCode.OK, Message.SUCCESS);
			}
			catch (Exception ex)
			{
				VM.Common.CustomLog.accesslog.Error(string.Format("ServiceFreeInPackage fail. Ex: {0}", ex));
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
	}
}