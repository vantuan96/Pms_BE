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
using Newtonsoft.Json;
using PMS.Business.Helper;
using System.Threading.Tasks;
using System.Data;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// Module Package Management
    /// </summary>
    [SessionAuthorize]
    public class AdminPackageController : BaseApiController
    {
        readonly string CodeRegex = @"^[a-zA-Z0-9 ()._-]{2,50}$";
        readonly string NameRegex = @"[!@#$%^&*()\=\[\]{};':\\|,.<>\/?]";
        #region Package Master Function
        /// <summary>
        /// API Get List Package
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package")]
        [Permission()]
        public IHttpActionResult GetListPackageAPI([FromUri]PackageParameterModel request)
        {
            var iQuery = new PackageRepo().GetPackages(request);

            int count = iQuery.Count();
            if (iQuery.Any() && !string.IsNullOrEmpty(request.CurrentGroupId) && request.OnlyShowSameRoot)
            {
                #region Filter the same root
                Guid GroupPackageId = new Guid(request.CurrentGroupId);
                var _repo = new PackageGroupRepo();
                var groupPackage = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == GroupPackageId);
                var currGroupRoot = _repo.GetPackageGroupRoot(groupPackage);
                List<Guid> ignoreListGroupPK = new List<Guid>();
                foreach (var item in iQuery)
                {
                    var groupRoot = _repo.GetPackageGroupRoot(item.PackageGroup);
                    if (currGroupRoot.Id != groupRoot.Id)
                    {
                        ignoreListGroupPK.Add(item.PackageGroup.Id);
                    }
                }
                if (ignoreListGroupPK.Count > 0)
                {
                    iQuery = iQuery.Where(x => !ignoreListGroupPK.Contains(x.PackageGroup.Id));
                }
                count = iQuery.Count();
                #endregion .Filter the same root
            }
            PackageGroupRepo groupRepo = new PackageGroupRepo();
            var results = iQuery.OrderBy(e => e.Code)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new { 
                    e.Id,e.PackageGroup, e.Code, e.Name, 
                    Sites = e.PackagePrices.Select(x=>x.PackagePriceSites.Where(y=> !request.IsShowExpireDate && (y.EndAt==null || (y.EndAt!=null && y.EndAt>=Constant.CurrentDate)))
                    .Select(y => new
                    {
                        y.Site.Id,
                        Name = y.Site.FullNameL,
                        y.Site.Code,
                        y.Site.HISCode,
                        y.Site.ApiCode,
                        y.Site.ApiLabCode,
                        y.Site.ApiXRayCode,
                        y.Site.HospitalId,
                        y.Site.FullNameL,
                        y.Site.FullNameE,
                        y.Site.AddressL,
                        y.Site.AddressE,
                        y.Site.Tel,
                        y.Site.Fax,
                        y.Site.Hotline,
                        y.Site.Emergency,
                        y.Site.Level,
                        y.Site.OnsitePercent,
                        y.Site.IsActived
                    })), 
                    e.IsActived, 
                    IsPriceSetted= e.PackagePrices.Any(x=>x.PackageId==e.Id),
                    IsExpireDate = e.PackagePrices.Any(x=>x.PackagePriceSites.Count > 0 && !x.PackagePriceSites.Any(y => y.EndAt >= Constant.CurrentDate || y.EndAt == null)),
                    e.IsLimitedDrugConsum,
                    IsMaternityPackage=/*Constant*/HelperBusiness.Instant.ListGroupCodeIsMaternityPackage.Contains(e.PackageGroup.Code),
                    //linhht bundle payment
                    IsBundlePackage =/*Constant*/HelperBusiness.Instant.ListGroupCodeIsBundlePackage.Contains(e.PackageGroup.Code),
                    IsIncludeChild= /*Constant*/HelperBusiness.Instant.ListGroupCodeIsIncludeChildPackage.Contains(e.PackageGroup.Code)
                });
            return Content(HttpStatusCode.OK, new { Count = count, Results = results });
        }
        /// <summary>
        /// API Create New Package
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package")]
        [Permission()]
        public IHttpActionResult CreatePackageAPI([FromBody]JObject request)
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
                #region Comment check format Name
                //regex = new Regex(NameRegex);
                //match = regex.Match(request["Name"].ToString());
                //if (match.Success)
                //{
                //    //Có chứa các ký tự đặc biết
                //    return Content(HttpStatusCode.BadRequest, Message.FORMAT_NAME_INVALID);
                //}
                #endregion .Comment check format Name
                var entity = unitOfWork.PackageRepository.FirstOrDefault(e => e.Code == code);
                if (entity != null)
                    return Content(HttpStatusCode.BadRequest, Message.CODE_DUPLICATE);
                entity = new Package
                {
                    PackageGroupId = request["PackageGroupId"].ToObject<Guid>(),
                    Code = code.ToUpper(),
                    Name = request["Name"].ToString(),
                    IsActived = request["IsActived"].ToObject<bool>(),
                    //IsPriceSetted= request["IsPriceSetted"].ToObject<bool>(),
                    /*Không theo định mức | Theo định mức*/
                    //IsLimitedDrugConsum = request["IsLimitedDrugConsum"].ToObject<bool>()
                    //Sửa thành luôn theo định mức
                    IsLimitedDrugConsum=true
                };
                unitOfWork.PackageRepository.Add(entity);
                unitOfWork.Commit();
                
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
        /// API Update Package
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package/{id}")]
        [Permission()]
        public IHttpActionResult UpdatePackageAPI(Guid id, [FromBody]JObject request)
        {
            try
            {
                #region Valid Data
                bool isValidInput = true;
                var checkRequire = CheckValidInputUpdatePackage(id,request, out isValidInput);
                if (!isValidInput)
                {
                    return checkRequire;
                }
                #endregion .Valid Data
                var entity = unitOfWork.PackageRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
                var code = request["Code"]?.ToString();
                entity.PackageGroupId = request["PackageGroupId"].ToObject<Guid>();
                entity.Code = code.ToUpper();
                entity.Name = request["Name"].ToString();
                entity.IsActived = request["IsActived"].ToObject<bool>();
                entity.IsPriceSetted = request["IsPriceSetted"].ToObject<bool>();
                /*Không theo định mức | Theo định mức*/
                //entity.IsLimitedDrugConsum = request["IsLimitedDrugConsum"].ToObject<bool>();
                //Sửa thành luôn theo định mức
                entity.IsLimitedDrugConsum = true;
                unitOfWork.PackageRepository.Update(entity);
                unitOfWork.Commit();
                
                return Content(HttpStatusCode.OK, Message.SUCCESS);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("UpdatePackageAPI fail. Ex: {0}", ex));
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
        /// API get detail Package
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/{id}")]
        [Permission()]
        public IHttpActionResult GetPackageDetailAPI(Guid id)
        {
            var entity = unitOfWork.PackageRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
            if (entity == null)
                return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
            PackageGroupRepo groupRepo = new PackageGroupRepo();
            return Content(HttpStatusCode.OK, new
            {
                entity.Id,
                entity.PackageGroupId,
                entity.PackageGroup,
                IsVaccinePackage=Constant.ListGroupCodeIsVaccinePackage.Contains(groupRepo.GetPackageGroupRoot(entity.PackageGroup)?.Code),
                IsHaveServiceDrugConsum=unitOfWork.ServiceInPackageRepository.Find(x=>x.PackageId==entity.Id && !x.IsDeleted && x.IsPackageDrugConsum).Any(),
                IsHaveInventory = unitOfWork.ServiceInPackageRepository.Find(x => x.PackageId == entity.Id && !x.IsDeleted && x.Service.ServiceType==Constant.SERVICE_TYPE_INV).Any(),
                entity.Name,
                entity.Code,
                entity.IsActived,
                entity.IsPriceSetted,
                entity.IsLimitedDrugConsum,
                Sites=entity.PackagePrices.Select(x => x.PackagePriceSites.Select(y=>new 
                { 
                    y.Site.Id,
                    Name=y.Site.FullNameL,
                    y.Site.Code,
                    y.Site.HISCode,
                    y.Site.ApiCode,
                    y.Site.ApiLabCode,
                    y.Site.ApiXRayCode,
                    y.Site.HospitalId,
                    y.Site.FullNameL,
                    y.Site.FullNameE,
                    y.Site.AddressL,
                    y.Site.AddressE,
                    y.Site.Tel,
                    y.Site.Fax,
                    y.Site.Hotline,
                    y.Site.Emergency,
                    y.Site.Level,
                    y.Site.OnsitePercent,
                    y.Site.IsActived
                }))
            });
        }
        /// <summary>
        /// API Delete Package.
        /// Can be support multi delection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package/Delete")]
        [Permission()]
        public IHttpActionResult DeletePackageAPI([FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            foreach (var s_id in request["Ids"])
            {
                try
                {
                    var id = new Guid(s_id.ToString());
                    var entity = unitOfWork.PackageRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
                    if (entity != null)
                    {
                        ////Xóa PackageSite
                        //unitOfWork.PackageSiteRepository.HardDeleteRange(entity.PackageSites.AsQueryable());
                        //Xóa Package
                        unitOfWork.PackageRepository.Delete(entity);
                    }
                        
                }
                catch { }
            }
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }

        #endregion .Package Master Data
        #region Package Service Function
        /// <summary>
        /// API Get List service in Package
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/Service")]
        [Permission()]
        public IHttpActionResult GetListServiceInPackageAPI([FromUri]ServiceInPackageParameterModel request)
        {
            IQueryable<ServiceInPackage> xqueryNoFil = null;
            var iQuery = new PackageRepo().GetServiceInPackages(request,out xqueryNoFil);
            int count = iQuery.Count();

            var results = iQuery.OrderBy(e=>e.ServiceType).ThenBy(e => e.IsPackageDrugConsum).ThenBy(e=> e.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new {
                    e.Id,
                    e.PackageId,
                    e.RootId,
                    ItemsReplace= xqueryNoFil.Where(y=>y.RootId== e.Id).Select(z=> new { z.Id ,z.ServiceId, z.Service,z.IsPackageDrugConsum,z.ServiceType,z.LimitQty,z.IsDeleted}),
                    e.Service,
                    e.ServiceType,
                    e.LimitQty,
                    e.IsPackageDrugConsum,
                    IsActived=e.Service.IsActive,
                    //26-07-2022 tungdd14 giá trị trả về check là dịch vụ tái khám
                    e.IsReExamService
                });
            var items = results.ToList();
            return Content(HttpStatusCode.OK, new { Count = count, Results = results });
        }
        /// <summary>
        /// API valid setup service in package
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package/ValidSetupService")]
        [Permission()]
        public IHttpActionResult CheckValidInputUpdateServiceInPackageAPI([FromBody]JObject request)
        {
            try
            {
                #region Valid Data
                bool isValidInput = true;
                bool isAddnewReplaceSv = true;
                var checkRequire = CheckValidInputUpdateServiceInPackage(request, out isValidInput,out isAddnewReplaceSv);
                if (!isValidInput)
                {
                    return checkRequire;
                }
                #endregion
                return Content(HttpStatusCode.OK, string.Empty);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CheckValidInputUpdateServiceInPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API Create New Or Update Service In Package.
        /// Support to insert/update multi Service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package/Service")]
        [Permission()]
        public IHttpActionResult CreateOrUpDateServiceInPackageAPI([FromBody]JObject request)
        {
            try
            {
                #region Valid Data
                bool isValidInput = true;
                bool isAddnewReplaceSv = true;
                var checkRequire = CheckValidInputUpdateServiceInPackage(request, out isValidInput,out isAddnewReplaceSv);
                if (!isValidInput)
                {
                    return checkRequire;
                }
                #endregion .Valid Data
                List<Guid> listServiceInPackagesReplace = new List<Guid>();
                var packageId = new Guid(request["PackageId"]?.ToString());
                foreach (var item in request["Services"])
                {
                    try
                    {
                        List<Guid> serviceInPackagesReplace = new List<Guid>();
                        var service_id = new Guid(item["ServiceId"].ToString());
                        CreateOrUpdateServiceInPackage(packageId, service_id, item,out serviceInPackagesReplace);
                        listServiceInPackagesReplace.AddRange(serviceInPackagesReplace);
                    }
                    catch(Exception ex)
                    {
                        VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpDateServiceInPackageAPI fail. Ex: {0}", ex));
                        continue;
                    }
                }
                unitOfWork.Commit();
                if (isAddnewReplaceSv && listServiceInPackagesReplace.Count()>0)
                {
                    #region Thêm các dịch vụ thay thế vào các gói khách hàng đã đăng ký (Trạng thái=Đăng ký, Đang sử dụng)
                    //Cần cập nhật thêm item thay thế trong bảng PatientInPackageDetails
                    //var xQueryListReg = unitOfWork.PatientInPackageRepository.AsEnumerable().Where(x => !x.IsDeleted && x.PackagePriceSite.PackagePrice.PackageId == packageId && (x.Status == (int)PatientInPackageEnum.REGISTERED || x.Status == (int)PatientInPackageEnum.ACTIVATED));
                    //tungdd14 fix CR 5055: thêm điều kiện x.Status == (int)PatientInPackageEnum.EXPIRED || x.Status == (int)PatientInPackageEnum.RE_EXAMINATE fix chỉ định thay thế trạng thái hết hạn và tái khám không áp dụng
                    var xQueryListReg = unitOfWork.PatientInPackageRepository.Find(x => !x.IsDeleted && x.PackagePriceSite.PackagePrice.PackageId == packageId && (x.Status == (int)PatientInPackageEnum.REGISTERED || x.Status == (int)PatientInPackageEnum.ACTIVATED || x.Status == (int)PatientInPackageEnum.EXPIRED || x.Status == (int)PatientInPackageEnum.RE_EXAMINATE));
                    if (xQueryListReg.Any())
                    {
                        foreach(var itemReged in xQueryListReg)
                        {
                            foreach (var serviceInPkReplaceID in listServiceInPackagesReplace)
                            {
                                var existItem = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == itemReged.Id && x.ServiceInPackageId == serviceInPkReplaceID).FirstOrDefault();
                                if (existItem != null)
                                {
                                    if (existItem.IsDeleted)
                                    {
                                        existItem.IsDeleted = false;
                                        unitOfWork.PatientInPackageDetailRepository.Update(existItem);
                                    }
                                        
                                }
                                else
                                {
                                    //Get item root
                                    var itemReplace = unitOfWork.ServiceInPackageRepository.Find(x => !x.IsDeleted && x.Id == serviceInPkReplaceID).FirstOrDefault();
                                    if (itemReplace != null && itemReplace.RootId != null)
                                    {
                                        var itemRoot = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == itemReged.Id && x.ServiceInPackageId == itemReplace.RootId).FirstOrDefault();
                                        if (itemRoot == null)
                                            continue;
                                        var PiInPkDetail = new PatientInPackageDetail();
                                        PiInPkDetail.PatientInPackageId = itemReged.Id;
                                        PiInPkDetail.ServiceInPackageId = serviceInPkReplaceID;
                                        PiInPkDetail.BasePrice = itemRoot.BasePrice;
                                        PiInPkDetail.BaseAmount = itemRoot.BaseAmount;
                                        PiInPkDetail.PkgPrice = itemRoot.PkgPrice;
                                        PiInPkDetail.PkgAmount = itemRoot.PkgAmount;
                                        PiInPkDetail.QtyWasUsed = itemRoot.QtyWasUsed;
                                        PiInPkDetail.QtyRemain = itemRoot.QtyRemain;
                                        PiInPkDetail.IsDeleted = itemRoot.IsDeleted;
                                        //tungdd14 cập nhật định mức tái khám
                                        PiInPkDetail.ReExamQtyLimit = itemRoot.ReExamQtyLimit;
                                        unitOfWork.PatientInPackageDetailRepository.Add(PiInPkDetail);
                                    }
                                }
                            }
                        }
                        unitOfWork.Commit();
                    }
                    #endregion .Thêm các dịch vụ thay thế vào các gói khách hàng đã đăng ký (Trạng thái=Đăng ký, Đang sử dụng)
                }
                return Content(HttpStatusCode.OK, new { packageId });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpDateServiceInPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API Delete Service In Package.
        /// Can be support multi delection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package/Service/Delete")]
        [Permission()]
        public IHttpActionResult DeleteServiceInPackageAPI([FromBody]JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            foreach (var s_id in request["Ids"])
            {
                try
                {
                    var id = new Guid(s_id.ToString());
                    var entity = unitOfWork.ServiceInPackageRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == id);
                    if (entity != null)
                    {
                        //Xóa Service In Package
                        unitOfWork.ServiceInPackageRepository.Delete(entity);
                    }

                }
                catch { }
            }
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }
        #endregion .Package Service Function
        #region Price policy setting
        #region Get Data config from HIS - CORE
        /// <summary>
        /// API get list ChargeType base from His (OH Core)
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/ChargeType/{code}")]
        [Permission()]
        public IHttpActionResult GetChargeTypeByHospital(string code)
        {
            var entities = OHConnectionAPI.GetChargeType(code);

            return Content(HttpStatusCode.OK, new { entities });
        }
        /// <summary>
        /// API get list Service Price from His (OH Core)
        /// </summary>
        /// <param name="chargetype"></param>
        /// <param name="servicecode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/ServicePrice/{chargetype}")]
        [Permission()]
        public IHttpActionResult GetServicePrice(string chargetype, [FromUri]string servicecode)
        {
            var entities = OHConnectionAPI.GetServicePrice(chargetype, new List<string> { servicecode });

            return Content(HttpStatusCode.OK, new { entities });
        }
        #endregion .Get Data config from HIS - CORE
        /// <summary>
        /// API get list policy price code (Danh sách Mã chính sách giá)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/PricePolicyCode/{id}")]
        [Permission()]
        public IHttpActionResult GetListPricePolicyCodeAPI(Guid id)
        {
            var iQuery = unitOfWork.PackagePriceRepository.Find(x => x.PackageId == id);
            
            var results = iQuery.OrderBy(e => e.StartAt)
                .Select(e => new {
                    e.PackageId,
                    e.Code,
                    e.StartAt,
                    IsExpireDate= e.PackagePriceSites.Count>0?!e.PackagePriceSites.Any(x=>x.EndAt>=Constant.CurrentDate || x.EndAt==null):false
                }).Distinct();

            int count = results.Count();

            return Content(HttpStatusCode.OK, new { Count = count, Results = results });
        }
        /// <summary>
        /// API get list policy price site (Danh sách Mã các site đã được áp dụng)
        /// </summary>
        /// <param name="packageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/PricePolicySite/{packageid}")]
        [Permission()]
        public IHttpActionResult GetListPricePolicySiteAPI(Guid packageid)
        {
            try {
                var entities = new PackageRepo().GetListPricePolicyViaSite(packageid);
                return Content(HttpStatusCode.OK, new { entities });
            }
            catch(Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("GetListPricePolicySiteAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API Create or Update Policy Price (Thiết lập chính sách giá)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Package/PricePolicy")]
        [Permission()]
        public IHttpActionResult CreateOrUpdatePricePolicyAPI([FromBody]PackagePricePolicyModel request)
        {
            #region Valid Data
            bool isValidInput = true;
            var checkRequire = CheckValidInputSetupPolicy(request, out isValidInput);
            if (!isValidInput)
            {
                return checkRequire;
            }
            var packageId = request.PackageId.Value;
            //Ktra xem đã có khách hàng đăng ký gói hay chưa
            bool isHaveReg = new PackageRepo().CheckExistPatientRegWithPolicy(request.Code);
            #endregion .Valid Data
            #region Check Update with personal type object
            checkRequire = CheckValidUpdatePolicyPersonal(request, isHaveReg, out isValidInput);
            if (!isValidInput)
            {
                return checkRequire;
            }
            #endregion .Check Update with personal type object
            var masterList = CreateOrUpdatePricePolicy(request);
            /*Setup chi tiết giá dịch vụ trong gói*/
            #region Check Update setup apply for price detail
            checkRequire = CheckValidUpdateApplyPolicyPriceDetail(request, isHaveReg, masterList, out isValidInput);
            if (!isValidInput)
            {
                return checkRequire;
            }
            #endregion .Check Update setup apply price detail
            if (request.Details != null)
            {
                //Create or update
                foreach (var item in request.Details.Where(x => x.ServiceType != (int)ServiceInPackageTypeEnum.TOTAL))
                    CreateOrUpdatePricePolicyDetail(item, masterList);
                //2022-08-01:Phubq edit to update for items service inside package was deleted
                //Mark delete
                #region Mark delete
                if (masterList?.Count > 0)
                {
                    var xqueryItemsWillBeDeleting = unitOfWork.PackagePriceDetailRepository.AsEnumerable().Where(x => masterList.Any(y => y.Id == x.PackagePriceId)
                && !request.Details.Any(z => z.ServiceInPackageId == x.ServiceInPackage.Id) && !x.IsDeleted);
                    if (xqueryItemsWillBeDeleting.Any())
                    {
                        foreach (var itemDel in xqueryItemsWillBeDeleting)
                            unitOfWork.PackagePriceDetailRepository.Delete(itemDel);
                    }
                }
                #endregion Mark delete
            }
            /*Setup Site apply*/
            #region Check Update setup apply for site
            checkRequire = CheckValidUpdateApplyPolicyForSite(request, isHaveReg, masterList, out isValidInput);
            if (!isValidInput)
            {
                return checkRequire;
            }
            #endregion .Check Update setup apply for site
            if (request.ListSites != null)
                foreach (var item in request.ListSites)
                    GrandPricePolicyForSite(item, masterList);

            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, new { ListId = masterList.Select(x => x.Id), Code = masterList?[0].Code });
        }
        /// <summary>
        /// API get detail Package Price Policy
        /// </summary>
        /// <param name="packageid"></param>
        /// <param name="policycode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/PricePolicy/{packageid}")]
        [Permission()]
        public IHttpActionResult PricePolicyDetailAPI(Guid packageid, [FromUri]string policycode)
        {
            try {
                var model = new PackagePricePolicyModel();
                model.PackageId = packageid;
                model.Code = policycode;
                //Get list Policy by policycode
                var listPolicies = unitOfWork.PackagePriceRepository.Find(x => x.PackageId == packageid && x.Code == policycode);
                if (listPolicies.Any())
                {
                    model.Policy = listPolicies.Select(x => new PackagePriceModel{ 
                        Id=x.Id,
                        SiteBaseCode=x.SiteBaseCode,
                        ChargeType=x.ChargeType,
                        PersonalType=x.PersonalType,
                        Amount=x.Amount,
                        IsLimitedDrugConsum=x.IsLimitedDrugConsum,
                        LimitedDrugConsumAmount=x.LimitedDrugConsumAmount,
                        StartAt=x.StartAt?.ToString(Constant.DATE_FORMAT),
                        //tungdd14 tính giá vaccine theo hệ số
                        RateINV = x.RateINV
                    })?.OrderBy(x=>x.PersonalType).ToList();
                }
                //Get Service price detail
                model.Details = new PackageRepo().PackagePriceDetail(packageid, policycode);
                #region Add more Total Row
                if (model.Details != null && model.Details.Count > 0)
                {
                    double? total_Base_VN = model.Details.Sum(x => x.BaseAmount);
                    double? total_Base_FN = model.Details.Sum(x => x.BaseAmountForeign);
                    double? total_Package_VN = model.Details.Sum(x => x.PkgAmount);
                    double? total_Package_FN = model.Details.Sum(x => x.PkgAmountForeign);
                    model.Details.Add(new PackagePriceDetailModel() { ServiceType = 0, BaseAmount = total_Base_VN, PkgAmount = total_Package_VN, BaseAmountForeign = total_Base_FN, PkgAmountForeign = total_Package_FN });
                }
                #endregion .Add more Total Row
                //Get list Sites
                var listSites = unitOfWork.PackagePriceSiteRepository.Find(x => !x.IsDeleted && listPolicies.Any(y=>y.Id==x.PackagePriceId) /*&& (x.EndAt==null || (x.EndAt!=null && x.EndAt>=Constant.CurrentDate))*/);
                if (listSites.Any())
                {
                    var listDistinct = listSites.Select(x => new { x.SiteId,x.Site,x.EndAt,x.Notes}).Distinct();
                    model.ListSites = listDistinct.Select(x => new PackagePriceSitesModel {
                        SiteId=x.SiteId,
                        Site= new Site()
                        {
                            Id=x.Site.Id,
                            Name = x.Site.FullNameL,
                            Code=x.Site.Code,
                            HISCode=x.Site.HISCode,
                            ApiCode=x.Site.ApiCode,
                            ApiLabCode=x.Site.ApiLabCode,
                            ApiXRayCode=x.Site.ApiXRayCode,
                            HospitalId=x.Site.HospitalId,
                            FullNameL=x.Site.FullNameL,
                            FullNameE=x.Site.FullNameE,
                            AddressL=x.Site.AddressL,
                            AddressE=x.Site.AddressE,
                            Tel=x.Site.Tel,
                            Fax=x.Site.Fax,
                            Hotline=x.Site.Hotline,
                            Emergency=x.Site.Emergency,
                            Level=x.Site.Level,
                            OnsitePercent=x.Site.OnsitePercent,
                            IsActived=x.Site.IsActived
                        },
                        EndAt=x.EndAt?.ToString(Constant.DATE_FORMAT),
                        //Notes= "Chưa biết notes gì"
                    })?.ToList();
                }

                return Content(HttpStatusCode.OK, new { model });
            }
            catch(Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("PricePolicyDetailAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// Get List Price Policy available
        /// </summary>
        /// <param name="packageid"></param>
        /// <param name="sitecode"></param>
        /// <param name="personaltype"></param>
        /// <param name="applydate"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/PricePolicyAvailable/{packageid}")]
        [Permission()]
        public IHttpActionResult PricePolicyAvailableAPI(Guid packageid, [FromUri]string sitecode, [FromUri] int? personaltype, [FromUri]string applydate)
        {
            try
            {
                DateTime applyDate = Constant.CurrentDate;
                if (!string.IsNullOrEmpty(applydate))
                {
                    DateTime.TryParse(applydate,out applyDate);
                }
                var siteId = unitOfWork.SiteRepository.FirstOrDefault(x=>x.ApiCode==sitecode)?.Id;
                //var xquery = unitOfWork.PackagePriceRepository.Find(x=> x.StartAt <= applyDate /*&& x.PackagePriceSites.Any(y => y.SiteId == siteId /*&& (y.EndAt >= applyDate || y.EndAt == null)*/).AsQueryable();
                var xqueryPolicy = unitOfWork.PackagePriceRepository.AsQueryable().Where(x => !x.IsDeleted && !x.IsNotForRegOnline && x.StartAt <= applyDate);
                var xqueryPackage = unitOfWork.PackageRepository.AsQueryable().Where(x => !x.IsDeleted);
                var xquery = (from a in xqueryPolicy
                              join b in xqueryPackage
                                   on a.PackageId equals b.Id into bx
                              from bxg in bx.DefaultIfEmpty()
                              where a.PackagePriceSites.Any(y => y.SiteId == siteId && (y.EndAt >= applyDate || y.EndAt == null))
                              && bxg.Id==packageid
                              select new {
                                  PolicyId=a.Id,
                                  a.Amount,
                                  a.PersonalType,
                                  Package=new {
                                      Id=bxg.Id,
                                      Code= bxg.Code,
                                      Name = bxg.Name,
                                      IsLimitedDrugConsum = bxg.IsLimitedDrugConsum,
                                      IsActived = bxg.IsActived,
                                      IsMaternityPackage=/*Constant*/HelperBusiness.Instant.ListGroupCodeIsMaternityPackage.Contains(bxg.PackageGroup.Code),
                                      //linhht bundle payment
                                      IsBundlePackage =/*Constant*/HelperBusiness.Instant.ListGroupCodeIsBundlePackage.Contains(bxg.PackageGroup.Code),
                                      IsIncludeChild= /*Constant*/HelperBusiness.Instant.ListGroupCodeIsIncludeChildPackage.Contains(bxg.PackageGroup.Code)
                                  }
                              });

                if (personaltype != null)
                    xquery = xquery.Where(x => x.PersonalType == personaltype);
                var model = xquery;
                return Content(HttpStatusCode.OK, new { model });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("PricePolicyDetailAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API Update PackagePriceDetail (Cập nhật Chi tiết thiết lập giá/dịch vụ)
        /// </summary>
        /// <param name="packageid"></param>
        /// <param name="code"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package/PricePolicyDetail/{packageid}/{code}")]
        [Permission()]
        public IHttpActionResult UpdatePackagePriceDetailAPI(string packageid, string code, [FromBody]PackagePriceDetailModel request)
        {
            try
            {
                var listPricePolicy = unitOfWork.PackagePriceRepository.Find(x=>!x.IsDeleted && !x.IsNotForRegOnline &&x.Code==code && x.PackageId.ToString()==packageid);
                if (listPricePolicy == null)
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);

                CreateOrUpdatePricePolicyDetail(request, listPricePolicy.ToList());

                unitOfWork.Commit();

                return Content(HttpStatusCode.OK, Message.SUCCESS);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("UpdatePackagePriceDetailAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API calculate Service Price (Tính giá). (Dùng trong TH sửa lại giá.)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package/PricePolicy/CalculateDetail")]
        [Permission()]
        public IHttpActionResult CalculatePriceAPI([FromBody]PackagePricePolicyModel request)
        {
            try
            {
                //var entities = new PackageRepo().CalculatePriceDetail(request);
                var entities = request?.Details;
                #region Add more Total Row
                if (entities != null && entities.Count > 0)
                {
                    #region Tính lại thành tiền
                    foreach (var item in entities)
                    {
                        item.PkgAmount = item.PkgPrice * item.Qty;
                        item.PkgAmountForeign = item.PkgPriceForeign * item.Qty;
                    }
                    #endregion .Tính lại thành tiền
                    double? total_Base_VN = entities.Sum(x => x.BaseAmount);
                    double? total_Base_FN = entities.Sum(x => x.BaseAmountForeign);
                    double? total_Package_VN = entities.Sum(x => x.PkgAmount);
                    double? total_Package_FN = entities.Sum(x => x.PkgAmountForeign);
                    
                    entities.Add(new PackagePriceDetailModel() { ServiceType = 0, BaseAmount = total_Base_VN, PkgAmount = total_Package_VN, BaseAmountForeign = total_Base_FN, PkgAmountForeign = total_Package_FN });
                }
                #endregion .Add more Total Row
                return Content(HttpStatusCode.OK,new { entities});
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CalculatePriceDetailAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API calculate Service Price from CORE (OH)(Tính giá)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Package/PricePolicy/CalculatePrice")]
        [Permission()]
        public IHttpActionResult CalculatePriceDetailAPI([FromBody]JObject request)
        {
            try
            {
                var packageId = new Guid(request["packageId"]?.ToString());
                var chargetypecode = request["chargetypecode"]?.ToObject<string>();
                var pkgAmount = request["pkgAmount"]?.ToObject<double?>();
                var chargetypecode_fn = request["chargetypecode_fn"]?.ToObject<string>();
                var pkgAmount_fn = request["pkgAmount_fn"]?.ToObject<double?>();
                var isLimitedDrugConsum = request["isLimitedDrugConsum"]!=null?request["isLimitedDrugConsum"].ToObject<bool>():false;
                var limitedDrugConsumAmount = request["limitedDrugConsumAmount"]?.ToObject<double?>();
                //tungdd14 tính giá vaccine theo hệ số
                var rateINV = request["RateINV"]?.ToObject<double?>();
                //Find Package detail
                var entity = unitOfWork.PackageRepository.FirstOrDefault(e => !e.IsDeleted && e.Id == packageId);
                if (entity == null)
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                //tungdd14 thêm rateINV tính giá vaccine theo hệ số
                var entities = new PackageRepo().GeneratePackagePriceDetail(entity, chargetypecode, pkgAmount, chargetypecode_fn, pkgAmount_fn, isLimitedDrugConsum,limitedDrugConsumAmount, rateINV);
                #region Add more Total Row
                if(entities!=null && entities.Count > 0)
                {
                    double? total_Base_VN = entities.Sum(x => x.BaseAmount);
                    double? total_Base_FN = entities.Sum(x => x.BaseAmountForeign);
                    double? total_Package_VN = entities.Sum(x => x.PkgAmount);
                    double? total_Package_FN = entities.Sum(x => x.PkgAmountForeign);
                    entities.Add(new PackagePriceDetailModel() { ServiceType=0,BaseAmount=total_Base_VN ,PkgAmount= total_Package_VN,BaseAmountForeign= total_Base_FN,PkgAmountForeign= total_Package_FN });
                }
                #endregion .Add more Total Row
                return Content(HttpStatusCode.OK, new { entities });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CalculatePriceDetailAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API check have patient was reg this package ?
        /// </summary>
        /// <param name="packageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Package/CheckExistPatientReg/{packageid}")]
        [Permission()]
        public IHttpActionResult CheckExistPatientReg(Guid packageid)
        {
            try
            {
                var returnValue = new PackageRepo().CheckExistPatientReg(packageid);
                return Content(HttpStatusCode.OK, new { returnValue });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CheckHavePatientReg fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        #endregion .Price policy setting
        #region Tiện ích/Addon
        /// <summary>
        /// Cập nhật các dịch vụ thay thế trong gói/Upgrate service replacing
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Package/AddOn/UpdateServiceReplace")]
        [Permission()]
        public async Task<IHttpActionResult> UpdateServiceReplace()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            foreach (var file in provider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsStreamAsync();
                if(buffer==null)
                    return Content(HttpStatusCode.BadRequest, Message.DATA_NOT_FOUND);
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (var stream = buffer)
                    {
                        pck.Load(stream);
                    }
                    var ws = pck.Workbook.Worksheets.First();
                    DataTable tbl = new DataTable();
                    foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                    {
                        tbl.Columns.Add(true ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                    }
                    var startRow = true ? 2 : 1;
                    for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                        DataRow row = tbl.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    //return tbl;
                    if (tbl.Rows?.Count > 0)
                    {
                        PatientInPackageRepo _repo = new PatientInPackageRepo();
                        var returnList = new List<dynamic>();
                        foreach (DataRow dtr in tbl.Rows)
                        {
                            //Get packages to update replace service
                            var packageCode = dtr["PackageCode"]?.ToString();
                            var serviceCode = dtr["ServiceCode"]?.ToString();
                            var serviceName = dtr["ServiceName"]?.ToString();
                            var serviceCodeReplace = dtr["ReplaceServiceCode"]?.ToString();
                            if (!string.IsNullOrEmpty(packageCode) && !string.IsNullOrEmpty(serviceCode) && !string.IsNullOrEmpty(serviceCodeReplace))
                            {
                                var pkEntity = unitOfWork.PackageRepository.FirstOrDefault(x => x.Code == packageCode);
                                if (pkEntity != null)
                                {
                                    #region Kiểm tra có phải là dịch vụ gốc hay không
                                    var IsRoot = unitOfWork.ServiceInPackageRepository.Find(x => x.PackageId == pkEntity.Id && x.Service.Code == serviceCodeReplace && x.RootId ==null && !x.IsDeleted).Any();
                                    if (IsRoot)
                                    {
                                        //Là dịch vụ gốc
                                        dynamic entity = new
                                        {
                                            PackageCode = packageCode,
                                            ServiceCode = serviceCode,
                                            ServiceName = serviceName,
                                            ServiceCodeReplace = serviceCodeReplace,
                                            StatusName = Message.SERVICE_ISROOT_NOTALLOW_REPLACE
                                        };
                                        returnList.Add(entity);
                                        continue;
                                    }
                                    var IsUsedReplaced = unitOfWork.ServiceInPackageRepository.Find(x => x.PackageId == pkEntity.Id && x.Service.Code == serviceCodeReplace && x.RootId != null && !x.IsDeleted).Any();
                                    if (IsUsedReplaced)
                                    {
                                        //Đã được sử dụng để thay thế cho dịch vụ khác
                                        dynamic entity = new
                                        {
                                            PackageCode = packageCode,
                                            ServiceCode = serviceCode,
                                            ServiceName = serviceName,
                                            ServiceCodeReplace = serviceCodeReplace,
                                            StatusName = Message.SERVICE_REPLACED_OTHERSERVICE
                                        };
                                        returnList.Add(entity);
                                        continue;
                                    }
                                    #endregion
                                    //CreateOrUpdateServiceInPackage
                                    var serviceRoot = unitOfWork.ServiceInPackageRepository.FirstOrDefault(x=>x.PackageId == pkEntity.Id && x.Service.Code== serviceCode);
                                    if (serviceRoot != null)
                                    {
                                        var serviceRpl = unitOfWork.ServiceInPackageRepository.FirstOrDefault(x => x.PackageId == pkEntity.Id && x.Service.Code == serviceCodeReplace && x.RootId== serviceRoot.Id);
                                        if (serviceRpl == null)
                                        {
                                            //Get serviceId=
                                            var serviceId = unitOfWork.ServiceRepository.FirstOrDefault(x=>x.Code== serviceCodeReplace)?.Id;
                                            if (serviceId != null)
                                            {
                                                serviceRpl = new ServiceInPackage()
                                                {
                                                    RootId = serviceRoot.Id,
                                                    PackageId = pkEntity.Id,
                                                    ServiceId = serviceId,
                                                    LimitQty = serviceRoot.LimitQty,
                                                    ServiceType = serviceRoot.ServiceType,
                                                    IsDeleted = serviceRoot.IsDeleted
                                                };
                                                unitOfWork.ServiceInPackageRepository.Add(serviceRpl);
                                                unitOfWork.Commit();
                                            }
                                            else
                                            {
                                                //Not found service
                                                dynamic entity = new
                                                {
                                                    PackageCode = packageCode,
                                                    ServiceCode = serviceCode,
                                                    ServiceName = serviceName,
                                                    ServiceCodeReplace = serviceCodeReplace,
                                                    StatusName = Message.NOT_FOUND_SERVICE
                                                };
                                                returnList.Add(entity);
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            //Đã tồn tại
                                            serviceRpl.IsDeleted = false;
                                            unitOfWork.ServiceInPackageRepository.Update(serviceRpl);
                                            unitOfWork.Commit();
                                        }

                                        var returnUpdate = _repo.UpdatePatientInPackageDetailWhenHaveNewReplaceService(new List<Guid> { serviceRpl.Id}, pkEntity.Id);
                                        if (returnUpdate)
                                        {
                                            //update service replace on success
                                            dynamic entity = new
                                            {
                                                PackageCode = packageCode,
                                                ServiceCode = serviceCode,
                                                ServiceName = serviceName,
                                                ServiceCodeReplace = serviceCodeReplace,
                                                StatusName = Message.SUCCESS
                                            };
                                            returnList.Add(entity);
                                        }
                                        else {
                                            //update service replace on fail
                                            dynamic entity = new
                                            {
                                                PackageCode = packageCode,
                                                ServiceCode = serviceCode,
                                                ServiceName = serviceName,
                                                ServiceCodeReplace = serviceCodeReplace,
                                                StatusName = Message.FAIL
                                            };
                                            returnList.Add(entity);
                                        }
                                    }
                                    else
                                    {
                                        //Not found root service
                                        dynamic entity = new
                                        {
                                            PackageCode = packageCode,
                                            ServiceCode = serviceCode,
                                            ServiceName = serviceName,
                                            ServiceCodeReplace = serviceCodeReplace,
                                            StatusName = Message.NOT_FOUND_SERVICEINPACAKGE
                                        };
                                        returnList.Add(entity);
                                    }
                                }
                                else
                                {
                                    dynamic entity = new
                                    {
                                        PackageCode = packageCode,
                                        ServiceCode = serviceCode,
                                        ServiceName = serviceName,
                                        ServiceCodeReplace = serviceCodeReplace,
                                        StatusName = Message.NOT_FOUND_PACKAGE
                                    };
                                    returnList.Add(entity);
                                }
                            }
                            else{
                                dynamic entity = new
                                {
                                    PackageCode = packageCode,
                                    ServiceCode = serviceCode,
                                    ServiceName = serviceName,
                                    ServiceCodeReplace = serviceCodeReplace,
                                    StatusName = Message.FORMAT_INVALID
                                };
                                returnList.Add(entity);
                            }
                        }
                        return Content(HttpStatusCode.OK, new { returnList });
                    }
                    else
                    {
                        Content(HttpStatusCode.BadRequest, Message.DATA_NOT_FOUND);
                    }
                }
                //Do whatever you want with filename and its binary data.
            }

            return Ok();
        }

        /// <summary>
        /// Cập nhật ngày hết hiệu lực gói (import) (Inactive gói hàng loạt)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Package/AddOn/UpdateExpirationDate")]
        [Permission()]
        public async Task<IHttpActionResult> UpdateExpirationDate()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);
            foreach (var file in provider.Contents)
            {
                var filename = file.Headers.ContentDisposition.FileName.Trim('\"');
                var buffer = await file.ReadAsStreamAsync();
                if (buffer == null)
                    return Content(HttpStatusCode.BadRequest, Message.DATA_NOT_FOUND);
                using (var pck = new OfficeOpenXml.ExcelPackage())
                {
                    using (var stream = buffer)
                    {
                        pck.Load(stream);
                    }
                    var ws = pck.Workbook.Worksheets.First();
                    DataTable tbl = new DataTable();
                    foreach (var firstRowCell in ws.Cells[3, 1, 3, ws.Dimension.End.Column])
                    {
                        tbl.Columns.Add(true ? (firstRowCell.Text == "Ngày hết hiệu lực (*)\n(YYYY-MM-DD)" ? "Ngày hết hiệu lực (*)" : firstRowCell.Text) : string.Format("Column {0}", firstRowCell.Start.Column));
                    }
                    var startRow = true ? 4 : 1;
                    for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                        DataRow row = tbl.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    //return tbl;
                    if (tbl.Rows?.Count > 0)
                    {
                        PatientInPackageRepo _repo = new PatientInPackageRepo();
                        var returnList = new List<ImportExpirationDateNoteModel>();
                        foreach (DataRow dtr in tbl.Rows)
                        {
                            //Get packages to update replace service
                            var packageCode = dtr["Mã gói (*)"]?.ToString().Trim();
                            var packageName = dtr["Tên gói (*)"]?.ToString().Trim();
                            var expirationDate = dtr["Ngày hết hiệu lực (*)"]?.ToString().Trim();
                            var siteBases = dtr["Phạm vi áp dụng"]?.ToString().Trim();
                            var expirationDatetime = new DateTime();
                            if (string.IsNullOrEmpty(packageCode) && string.IsNullOrEmpty(packageName) && string.IsNullOrEmpty(expirationDate) && string.IsNullOrEmpty(siteBases))
                            {
                                continue;
                            }
                            else if (string.IsNullOrEmpty(packageCode) || string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(expirationDate))
                            {
                                ImportExpirationDateNoteModel entity = new ImportExpirationDateNoteModel
                                {
                                    PackageCode = packageCode,
                                    packageName = packageName,
                                    site = "",
                                    StatusName = Message.NOTE_FAIL,
                                    ErrorMessage = new List<MessageModel>(),
                                };
                                if (string.IsNullOrEmpty(packageCode))
                                {
                                    entity.ErrorMessage.Add(Message.PACKAGE_CODE_REQUIRED);
                                }
                                if (string.IsNullOrEmpty(packageName))
                                {
                                    entity.ErrorMessage.Add(Message.PACKAGE_NAME_REQUIRED);
                                }
                                if (string.IsNullOrEmpty(expirationDate))
                                {
                                    entity.ErrorMessage.Add(Message.EXPIRSTION_DATE_REQUIRED);
                                }
                                returnList.Add(entity);
                            }
                            else if (!DateTime.TryParseExact(expirationDate, Constant.DATE_SQL, null, System.Globalization.DateTimeStyles.None, out expirationDatetime))
                            {
                                ImportExpirationDateNoteModel entity = new ImportExpirationDateNoteModel
                                {
                                    PackageCode = packageCode,
                                    packageName = packageName,
                                    site = "",
                                    StatusName = Message.NOTE_FAIL,
                                    ErrorMessage = new List<MessageModel>() { Message.EXPIRSTION_DATE_FAIL_FORMAT }
                                };
                                returnList.Add(entity);
                            }
                            else
                            {
                                var pkEntity = unitOfWork.PackageRepository.FirstOrDefault(x => x.Code.Trim() == packageCode && x.Name.Trim() == packageName);
                                //Kiểm tra gói có tồn tại không
                                if (pkEntity != null)
                                {
                                    bool pkActive = true;
                                    int errorMessageInt = 0;
                                    var listSiteNotPriceSite = "";
                                    //trường hợp có điền phạm vi áp dụng
                                    if (!string.IsNullOrEmpty(siteBases))
                                    {
                                        var listSideBase = siteBases.Split(',').ToList();
                                        foreach (var sitebase in listSideBase)
                                        {
                                            var siteBaseEntity = unitOfWork.SiteRepository.FirstOrDefault(x => x.Code == sitebase && x.IsActived);
                                            if (siteBaseEntity != null)
                                            {
                                                var listPKPriceSiteEntity = unitOfWork.PackagePriceSiteRepository.Find(x => x.PackagePrice.PackageId == pkEntity.Id && x.SiteId == siteBaseEntity.Id && !x.IsDeleted);
                                                //Kiểm tra gói đã thiết lập giá chưa
                                                if (listPKPriceSiteEntity.Any())
                                                {
                                                    foreach (var pkPriceSiteEntity in listPKPriceSiteEntity)
                                                    {
                                                        if (pkPriceSiteEntity.PackagePrice.StartAt > expirationDatetime)
                                                        {
                                                            listSiteNotPriceSite += sitebase + ", ";
                                                            pkActive = pkActive ? false : pkActive;
                                                            errorMessageInt = errorMessageInt != 2 ? 2 : errorMessageInt;
                                                            break;
                                                        }
                                                        else if ((pkPriceSiteEntity.EndAt < DateTime.Now))
                                                        {
                                                            listSiteNotPriceSite += sitebase + ", ";
                                                            pkActive = pkActive ? false : pkActive;
                                                            errorMessageInt = errorMessageInt != 3 ? 3 : errorMessageInt;
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            pkPriceSiteEntity.EndAt = expirationDatetime;
                                                            unitOfWork.PackagePriceSiteRepository.Update(pkPriceSiteEntity);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    listSiteNotPriceSite += sitebase + ", ";
                                                    pkActive = pkActive ? false : pkActive;
                                                }
                                            }
                                            else
                                            {
                                                pkActive = !pkActive ? true : pkActive;
                                                errorMessageInt = errorMessageInt != 1 ? 1 : errorMessageInt;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Nếu không nhập thông tin Phạm vi áp dụng sẽ inactive gói tại tất cả các site
                                        var listPKPriceSiteEntity = unitOfWork.PackagePriceSiteRepository.Find(x => x.PackagePrice.PackageId == pkEntity.Id && !x.IsDeleted);
                                        if (listPKPriceSiteEntity.Any())
                                        {
                                            foreach (var pkPriceSiteEntity in listPKPriceSiteEntity)
                                            {
                                                if (pkPriceSiteEntity.PackagePrice.StartAt > expirationDatetime)
                                                {
                                                    listSiteNotPriceSite += pkPriceSiteEntity.Site.Code + ", ";
                                                    pkActive = pkActive ? false : pkActive;
                                                    errorMessageInt = errorMessageInt != 2 ? 2 : errorMessageInt;
                                                    break;
                                                }
                                                else if ((pkPriceSiteEntity.EndAt < DateTime.Now))
                                                {
                                                    listSiteNotPriceSite += pkPriceSiteEntity.Site.Code + ", ";
                                                    pkActive = pkActive ? false : pkActive;
                                                    errorMessageInt = errorMessageInt != 3 ? 3 : errorMessageInt;
                                                    break;
                                                }
                                                else
                                                {
                                                    pkPriceSiteEntity.EndAt = expirationDatetime;
                                                    unitOfWork.PackagePriceSiteRepository.Update(pkPriceSiteEntity);
                                                }
                                            }
                                        }
                                        else pkActive = pkActive ? false : pkActive;
                                    }

                                    if (pkActive)
                                    {
                                        //update service replace on success
                                        ImportExpirationDateNoteModel entity = new ImportExpirationDateNoteModel
                                        {
                                            PackageCode = packageCode,
                                            packageName = packageName,
                                            site = "",
                                            StatusName = Message.SUCCESS,
                                            ErrorMessage = new List<MessageModel>()
                                        };
                                        returnList.Add(entity);
                                        unitOfWork.Commit();
                                    }
                                    else
                                    {
                                        if (listSiteNotPriceSite.Length > 2)
                                            listSiteNotPriceSite = listSiteNotPriceSite.Substring(0, listSiteNotPriceSite.Length - 2);
                                        var errorMessage = new MessageModel();
                                        switch (errorMessageInt)
                                        {
                                            case 1:
                                                errorMessage = Message.NOT_FOUND_SITE;
                                                break;
                                            case 2:
                                                errorMessage = Message.ENDAT_GREATER_THAN_EXPIRSTION_DATE;
                                                break;
                                            case 3:
                                                errorMessage = Message.PACKAGE_PRICE_EXPIRSTION;
                                                break;
                                            default:
                                                errorMessage = Message.NOT_FOUND_PACKAGE_PRICE;
                                                break;
                                        }
                                        ImportExpirationDateNoteModel entity = new ImportExpirationDateNoteModel
                                        {
                                            PackageCode = packageCode,
                                            packageName = packageName,
                                            site = listSiteNotPriceSite,
                                            StatusName = Message.NOTE_FAIL,
                                            ErrorMessage = new List<MessageModel>() { errorMessage}
                                        };
                                        returnList.Add(entity);
                                    }
                                }
                                else
                                {
                                    ImportExpirationDateNoteModel entity = new ImportExpirationDateNoteModel
                                    {
                                        PackageCode = packageCode,
                                        packageName = packageName,
                                        site = "",
                                        StatusName = Message.NOTE_FAIL,
                                        ErrorMessage = new List<MessageModel>() { Message.NOT_FOUND_PACKAGE }
                                    };
                                    returnList.Add(entity);
                                }
                            }
                        }
                        return Content(HttpStatusCode.OK, new { returnList });
                    }
                    else
                    {
                        Content(HttpStatusCode.BadRequest, Message.DATA_NOT_FOUND);
                    }
                }
                //Do whatever you want with filename and its binary data.
            }

            return Ok();
        }
        #endregion .Tiện ích/Addon
        #region Function 4 Helper
        IHttpActionResult CheckValidInputUpdatePackage(Guid id, JObject request, out bool isValid)
        {
            string actionName = this.ActionContext.ActionDescriptor.ActionName;
            string strValue = string.Empty;
            #region Valid Data
            if (request != null)
            {
                var code = request["Code"]?.ToString();
                Regex regex = new Regex(CodeRegex);
                Match match = regex.Match(code);
                if (!match.Success)
                {
                    //Mã code sai format
                    isValid = false;
                    return Content(HttpStatusCode.BadRequest, Message.FORMAT_CODE_INVALID);
                }
                //Ktra xem đã có khách hàng đăng ký gói hay chưa
                bool isHaveReg = new PackageRepo().CheckExistPatientReg(id);
                if (isHaveReg)
                {
                    //Đã có khách hàng đăng ký
                    #region Check change Master
                    //Ktra xem có thay đổi thông tin Master hay không
                    var crMasterData = unitOfWork.PackageRepository.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
                    if (crMasterData != null)
                    {
                        var PackageGroupId = request["PackageGroupId"].ToObject<Guid>();
                        var Code = code.ToUpper();
                        var IsActived = request["IsActived"].ToObject<bool>();
                        /*Không theo định mức | Theo định mức*/
                        //var IsLimitedDrugConsum = request["IsLimitedDrugConsum"].ToObject<bool>();
                        // Sửa thành luôn theo định mức
                        var IsLimitedDrugConsum = true;

                        //Ktra xem có đổi mã code ko
                        if (crMasterData.Code != Code)
                        {
                            //Không đc thay đổi mã code
                            VM.Common.CustomLog.accesslog.Error(string.Format("{0} Invalid input data: [packageId={1} | Status: {2}. Message={3}", actionName, id, HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_CODE));
                            isValid = false;
                            return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_CODE);
                        }
                        if (crMasterData.IsActived != IsActived)
                        {
                            //Không đc thay đổi trạng thái gói
                            VM.Common.CustomLog.accesslog.Error(string.Format("{0} Invalid input data: [packageId={1} | Status: {2}. Message={3}", actionName, id, HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_STATUS));
                            isValid = false;
                            return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_STATUS);
                        }
                        if (crMasterData.IsLimitedDrugConsum!= IsLimitedDrugConsum)
                        {
                            //Không được thay đổi loại định mức Thuốc và VTTH
                            VM.Common.CustomLog.accesslog.Error(string.Format("{0} Invalid input data: [packageId={1} | Status: {2}. Message={3}", actionName, id, HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_TYPE_ISLIMITEDDRUGCONSUM));
                            isValid = false;
                            return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_TYPE_ISLIMITEDDRUGCONSUM);
                        }
                        #region Check change group package
                        //Không cho phép đổi nhóm cha
                        if (crMasterData.PackageGroupId != PackageGroupId)
                        {
                            //Get Root change
                            var rootPackageChange = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == PackageGroupId);
                            if (rootPackageChange != null)
                            {
                                rootPackageChange = new PackageGroupRepo().GetPackageGroupRoot(rootPackageChange);
                            }
                            else
                            {
                                isValid = false;
                                return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                            }
                            //Get Root current
                            var rootPackageCurrent = new PackageGroupRepo().GetPackageGroupRoot(crMasterData.PackageGroup);
                            if(rootPackageCurrent?.Id != rootPackageChange?.Id)
                            {
                                //Không được đổi sang nhóm gói có cấp 1 khác
                                VM.Common.CustomLog.accesslog.Error(string.Format("{0} Invalid input data: [packageId={1} | Status: {2}. Message={3}", actionName, id, HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ROOTGROUP));
                                isValid = false;
                                return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ROOTGROUP);
                            }
                        }
                        #endregion .Check change group package
                    }
                    else
                    {
                        isValid = false;
                        return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                    }
                    #endregion .Check change Master
                }
                else
                {
                    isValid = true;
                    return Content(HttpStatusCode.OK, strValue);
                }
            }
            else
            {
                isValid = false;
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            #endregion .Valid Data
            isValid = true;
            return Content(HttpStatusCode.OK, strValue);
        }
        IHttpActionResult CheckValidInputUpdateServiceInPackage(JObject request, out bool isValid,out bool isAddnewReplaceSv)
        {
            bool isAddnewReplaceService = false;
            string actionName = this.ActionContext.ActionDescriptor.ActionName;
            string strValue = string.Empty;
            #region Valid Data
            if (request != null)
            {
                string sPackageId = request["PackageId"]?.ToString();
                var packageId =!string.IsNullOrEmpty(sPackageId)? new Guid(sPackageId) : Guid.Empty;
                var listSV = request["Services"];
                var countServicesDelete = 0;
                #region Valid Input service
                if (listSV.Any())
                {
                    List<string> listServiceId = new List<string>();
                    List<string> listServiceIdReplace = new List<string>();
                    foreach (var item in listSV)
                    {
                        var isDeleted = item["IsDeleted"]?.ToObject<bool?>();
                        if (isDeleted!=true)
                        {
                            if (!string.IsNullOrEmpty(item["ServiceId"]?.ToString()))
                                listServiceId.Add(item["ServiceId"]?.ToString());
                            if (item["ItemsReplace"].Any())
                            {
                                foreach (var itemRpl in item["ItemsReplace"])
                                {
                                    var isDeletedSub = itemRpl["IsDeleted"]?.ToObject<bool?>();
                                    if (isDeletedSub!=true && !string.IsNullOrEmpty(itemRpl["ServiceId"]?.ToString()))
                                        listServiceIdReplace.Add(itemRpl["ServiceId"]?.ToString());
                                }
                            }
                        }
                        //tungdd14 thêm trường check số lượng service delete
                        else countServicesDelete++;
                    }
                    var groupSv= listServiceId.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                    var groupSvReplace = listServiceIdReplace.GroupBy(x => x).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
                    var firstInSecond = listServiceId.Where(x => listServiceIdReplace.Any(y => y == x)).ToList();
                    var secondInFirst = listServiceIdReplace.Where(x => listServiceId.Any(y => y == x)).ToList();
                    if (firstInSecond.Any() || secondInFirst.Any() || groupSv?.Count>0 || groupSvReplace?.Count>0)
                    {
                        //Invalid vì đã có 1 item trong dịch vụ thay thế tồn tại trong dịch chính hoặc ngược lại
                        isValid = false;
                        isAddnewReplaceSv = isAddnewReplaceService;
                        return Content(HttpStatusCode.BadRequest, Message.SETTING_PACKAGE_SERVICE_DUPLICATE_SERVICE);
                    }
                }
                //tungdd14 check trường hợp xóa hết services trong package
                if (countServicesDelete != 0 && (countServicesDelete == listSV.Count()))
                {
                    isValid = false;
                    isAddnewReplaceSv = isAddnewReplaceService;
                    return Content(HttpStatusCode.BadRequest, Message.SETTING_PACKAGE_SERVICE_NO_HAVE_SERVICE);
                }
                if (packageId == Guid.Empty)
                {
                    //Bỏ quả ktra khi là TH mới tạo gói khám
                    isValid = true;
                    isAddnewReplaceSv = isAddnewReplaceService;
                    return Content(HttpStatusCode.OK, strValue);
                }
                #endregion .Valid Input service
                //Ktra xem đã có khách hàng đăng ký gói hay chưa
                bool isHaveReg = new PackageRepo().CheckExistPatientReg(packageId);
                if (isHaveReg)
                {
                    //Đã có khách hàng đăng ký
                    List<ServiceInPackage> listSVInput = null;
                    List<Guid?> listServiceId = new List<Guid?>();
                    if (listSV.Any())
                    {
                        listSVInput = new List<ServiceInPackage>();
                        foreach (var item in listSV)
                        {
                            if(item["Id"] == null)
                            {
                                //Invalid vì đã có 1 item bị thay đổi (Không tồn tại)
                                isValid = false;
                                isAddnewReplaceSv = isAddnewReplaceService;
                                if (item["ServiceType"].ToObject<int>() == (int)ServiceInPackageTypeEnum.SERVICE)
                                    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ITEMSERVICE);
                                else if (item["ServiceType"].ToObject<int>() == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM)
                                    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ITEMDRUG_CONSUM);
                            }
                            var serviceId = item["ServiceId"].ToObject<Guid>();
                            var entityDB = unitOfWork.ServiceInPackageRepository.FirstOrDefault(e => e.PackageId == packageId && e.ServiceId == serviceId);
                            if (entityDB == null)
                            {
                                //Invalid vì đã có 1 item bị thay đổi (Không tồn tại)
                                isValid = false;
                                isAddnewReplaceSv = isAddnewReplaceService;
                                if (item["ServiceType"].ToObject<int>() == (int)ServiceInPackageTypeEnum.SERVICE)
                                    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ITEMSERVICE);
                                else if (item["ServiceType"].ToObject<int>() == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM)
                                    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ITEMDRUG_CONSUM);
                            }
                            var entity = (ServiceInPackage)entityDB.Clone();
                            entity.RootId = item["RootId"]?.ToObject<Guid?>();
                            entity.PackageId = packageId;
                            entity.ServiceId = item["ServiceId"].ToObject<Guid>();
                            entity.LimitQty = item["LimitQty"]?.ToObject<int?>();
                            entity.IsPackageDrugConsum = item["IsPackageDrugConsum"].ToObject<bool>();
                            entity.ServiceType = item["ServiceType"].ToObject<int>();
                            entity.IsDeleted = item["IsDeleted"] != null ? item["IsDeleted"].ToObject<bool>() : false;
                            listSVInput.Add(entity);
                            listServiceId.Add(serviceId);
                            
                            //Set from replace item
                            foreach (var itemRpl in item["ItemsReplace"])
                            {
                                Guid? serviceReplaceId = itemRpl["ServiceId"]?.ToObject<Guid?>();
                                listServiceId.Add(serviceReplaceId);
                                var itemReplaceIsDeleted = itemRpl["IsDeleted"] != null ? itemRpl["IsDeleted"].ToObject<bool>() : false;
                                var rootId = itemRpl["RootId"]?.ToObject<Guid?>();
                                //tungdd14 thêm điều kiện e.RootId == rootId fix bug 5365 check item reolace delete sai dịch vụ
                                var entityReplaceDB = unitOfWork.ServiceInPackageRepository.FirstOrDefault(e => e.PackageId == packageId && e.ServiceId == serviceReplaceId && e.RootId == rootId);
                                if (entityReplaceDB == null)
                                {
                                    isAddnewReplaceService = true;
                                    #region comment & allow add more replace service
                                    //Invalid vì đã có 1 item bị thay đổi (Không tồn tại)
                                    //isValid = false;
                                    //return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ITEMREPLACE);
                                    #endregion .comment & allow add more replace service

                                }
                                else if(entityReplaceDB!=null && itemReplaceIsDeleted)
                                {
                                    //Invalid vì đã có 1 item bị thay đổi(Không tồn tại)
                                    isValid = false;
                                    isAddnewReplaceSv = isAddnewReplaceService;
                                    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_REMOVE_ITEMREPLACE);
                                }
                                var entityReplace = (ServiceInPackage)entityReplaceDB?.Clone();
                                if (entityReplace == null)
                                    entityReplace = new ServiceInPackage();
                                entityReplace.RootId = entity.Id;
                                entityReplace.PackageId = packageId;
                                entityReplace.ServiceId = serviceReplaceId;
                                entityReplace.LimitQty = itemRpl["LimitQty"]?.ToObject<int?>();
                                entityReplace.IsPackageDrugConsum = itemRpl["IsPackageDrugConsum"] !=null?itemRpl["IsPackageDrugConsum"].ToObject<bool>():false;
                                entityReplace.ServiceType = itemRpl["ServiceType"].ToObject<int>();
                                entityReplace.IsDeleted = itemRpl["IsDeleted"] != null ? itemRpl["IsDeleted"].ToObject<bool>() : false;
                                listSVInput.Add(entityReplace);
                            }
                        }
                        //Get List Exist inside Database 
                        var listInDB = unitOfWork.ServiceInPackageRepository.Find(x => !x.IsDeleted && x.PackageId == packageId && listServiceId.Contains(x.ServiceId))?.OrderBy(e => e.ServiceType).ThenBy(e => e.IsPackageDrugConsum).ThenBy(e => e.CreatedAt).ToList();
                        //var strFirst= JsonConvert.SerializeObject(listSVInput.Where(x=>x.RootId==null).OrderBy(x => x.CreatedAt).Select(x => new {
                        //    x.Id,
                        //    x.RootId,
                        //    x.PackageId,
                        //    x.ServiceId,
                        //    x.LimitQty,
                        //    x.IsPackageDrugConsum,
                        //    x.ServiceType,
                        //    x.CreatedAt,
                        //    x.CreatedBy,
                        //    x.UpdatedAt,
                        //    x.UpdatedBy,
                        //    x.IsDeleted,
                        //    x.DeletedBy,
                        //    x.DeletedAt
                        //}));
                        //var strSecond = JsonConvert.SerializeObject(listInDB.Where(x=>x.RootId==null).OrderBy(x => x.CreatedAt).Select(x => new {
                        //    x.Id,
                        //    x.RootId,
                        //    x.PackageId,
                        //    x.ServiceId,
                        //    x.LimitQty,
                        //    x.IsPackageDrugConsum,
                        //    x.ServiceType,
                        //    x.CreatedAt,
                        //    x.CreatedBy,
                        //    x.UpdatedAt,
                        //    x.UpdatedBy,
                        //    x.IsDeleted,
                        //    x.DeletedBy,
                        //    x.DeletedAt
                        //}));
                        listInDB = listInDB.Where(x => x.RootId == null)?.ToList();
                        listSVInput = listSVInput.Where(x => x.RootId == null)?.ToList();
                        var firstInSecond = listInDB.Where(x => listSVInput.Any(y => y == x)).ToList();
                        var secondInFirst = listInDB.Where(x => listSVInput.Any(y => y == x)).ToList();
                        if (firstInSecond.Any() || secondInFirst.Any())
                        {
                            //Invalid vì đã có 1 item trong dịch vụ thay thế tồn tại trong dịch chính hoặc ngược lại
                            isValid = false;
                            isAddnewReplaceSv = isAddnewReplaceService;
                            return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ITEMSERVICE);
                        }
                        //if (strFirst!= strSecond)
                        //{
                        //    //Có sự thay đổi trong thiết lập dịch vụ/ thuốc & VTTH trong gói
                        //    isValid = false;
                        //    isAddnewReplaceSv = isAddnewReplaceService;
                        //    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ITEMSERVICE);
                        //}
                    }
                }
                else
                {
                    isValid = true;
                    isAddnewReplaceSv = isAddnewReplaceService;
                    return Content(HttpStatusCode.OK, strValue);
                }
            }
            else
            {
                isValid = false;
                isAddnewReplaceSv = isAddnewReplaceService;
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            #endregion .Valid Data
            isValid = true;
            isAddnewReplaceSv = isAddnewReplaceService;
            return Content(HttpStatusCode.OK, strValue);
        }
        private void CreatePackagePriceSite(Guid packagePrice_id, JToken site)
        {
            Guid site_id = new Guid(site.ToString());
            var package_site = new PackagePriceSite
            {
                PackagePriceId = packagePrice_id,
                SiteId = site_id
            };
            unitOfWork.PackagePriceSiteRepository.Add(package_site);
        }
        private bool IsExistServiceInPackage(Guid packge_id, Guid service_id)
        {
            var entity = unitOfWork.ServiceInPackageRepository.FirstOrDefault(
                e => !e.IsDeleted &&
                e.PackageId==packge_id &&
                e.ServiceId == service_id
            );
            return entity != null;
        }
        private void CreateOrUpdateServiceInPackage(Guid packge_id, Guid service_id,JToken jService, out List<Guid> listServiceInPackageReplace)
        {
            List<Guid> newServiceInPackageReplaces = new List<Guid>();
            var entity = unitOfWork.ServiceInPackageRepository.FirstOrDefault(e => !e.IsDeleted && e.PackageId == packge_id && e.RootId == null && e.ServiceId== service_id);
            if (entity != null)
            {
                entity.RootId = jService["RootId"]?.ToObject<Guid?>();
                //entity.PackageId = packge_id;
                //entity.ServiceId = service_id;
                entity.LimitQty = jService["LimitQty"]?.ToObject<int?>();
                entity.IsPackageDrugConsum = jService["IsPackageDrugConsum"].ToObject<bool>();
                entity.ServiceType = jService["ServiceType"].ToObject<int>();
                entity.IsDeleted = jService["IsDeleted"] != null ? jService["IsDeleted"].ToObject<bool>() : false;
                //26-07-2022: tungdd update thêm thông tin là dịch vụ tái khám
                entity.IsReExamService = jService["IsReExamService"].ToObject<bool>();
                //entity.IsActived = jService["IsActived"].ToObject<bool>();
                foreach (var item in jService["ItemsReplace"])
                {
                    Guid? serviceId = item["ServiceId"]?.ToObject<Guid?>();
                    var entityReplace= unitOfWork.ServiceInPackageRepository.FirstOrDefault(e => !e.IsDeleted && e.PackageId == packge_id && e.RootId!=null && e.ServiceId == serviceId);
                    if (entityReplace != null)
                    {
                        entityReplace.RootId = entity.Id;
                        entityReplace.LimitQty = item["LimitQty"]?.ToObject<int?>();
                        if(item["IsPackageDrugConsum"]!=null)
                            entityReplace.IsPackageDrugConsum = item["IsPackageDrugConsum"].ToObject<bool>();
                        entityReplace.ServiceType = item["ServiceType"].ToObject<int>();
                        entityReplace.IsDeleted = item["IsDeleted"] != null ? item["IsDeleted"].ToObject<bool>() : false;
                        if (entityReplace.IsDeleted)
                        {
                            //Xóa Service In Package
                            unitOfWork.ServiceInPackageRepository.Delete(entityReplace);
                        }
                        else
                        {
                            unitOfWork.ServiceInPackageRepository.Update(entityReplace);
                        } 
                    }
                    else
                    {
                        entityReplace = new ServiceInPackage();
                        entityReplace.RootId = entity.Id;
                        entityReplace.PackageId = packge_id;
                        entityReplace.ServiceId = serviceId;
                        entityReplace.LimitQty = item["LimitQty"]?.ToObject<int?>();
                        if (item["IsPackageDrugConsum"] != null)
                            entityReplace.IsPackageDrugConsum = item["IsPackageDrugConsum"].ToObject<bool>();
                        entityReplace.ServiceType = item["ServiceType"].ToObject<int>();
                        entityReplace.IsDeleted = item["IsDeleted"] != null ? item["IsDeleted"].ToObject<bool>() : false;
                        if (!entityReplace.IsDeleted)
                        {
                            unitOfWork.ServiceInPackageRepository.Add(entityReplace);
                            newServiceInPackageReplaces.Add(entityReplace.Id);
                        }
                            
                    }
                }
                if (entity.IsDeleted)
                {
                    //Xóa Service In Package
                    unitOfWork.ServiceInPackageRepository.Delete(entity);
                }
                else
                {
                    unitOfWork.ServiceInPackageRepository.Update(entity);
                }
            }
            else
            {
                entity = new ServiceInPackage
                {
                    RootId = jService["RootId"]?.ToObject<Guid?>(),
                    PackageId = packge_id,
                    ServiceId = service_id,
                    LimitQty = jService["LimitQty"]?.ToObject<int?>(),
                    IsPackageDrugConsum = jService["IsPackageDrugConsum"].ToObject<bool>(),
                    ServiceType = jService["ServiceType"].ToObject<int>(),
                    IsDeleted = jService["IsDeleted"] != null ? jService["IsDeleted"].ToObject<bool>() : false,
                    //26-07-2022: tungdd lưu thêm thông tin là dịch vụ tái khám
                    IsReExamService = jService["IsReExamService"] != null ? jService["IsReExamService"].ToObject<bool>() : false,
                    //IsActived = jService["IsActived"].ToObject<bool>()
                };
                if (!entity.IsDeleted)
                {
                    unitOfWork.ServiceInPackageRepository.Add(entity);
                    foreach (var item in jService["ItemsReplace"])
                    {
                        Guid? serviceId = item["ServiceId"]?.ToObject<Guid?>();
                        var entityReplace = unitOfWork.ServiceInPackageRepository.FirstOrDefault(e => !e.IsDeleted && e.PackageId == packge_id && e.ServiceId == serviceId);
                        if (entityReplace != null)
                        {
                            entityReplace.RootId = entity.Id;
                            entityReplace.LimitQty = item["LimitQty"]?.ToObject<int?>();
                            entityReplace.IsPackageDrugConsum = item["IsPackageDrugConsum"].ToObject<bool>();
                            entityReplace.ServiceType = item["ServiceType"].ToObject<int>();
                            entityReplace.IsDeleted = item["IsDeleted"] != null ? item["IsDeleted"].ToObject<bool>() : false;
                            if (entityReplace.IsDeleted)
                            {
                                //Xóa Service In Package
                                unitOfWork.ServiceInPackageRepository.Delete(entity);
                            }
                            else
                            {
                                unitOfWork.ServiceInPackageRepository.Update(entityReplace);
                            }
                        }
                        else
                        {
                            entityReplace = new ServiceInPackage();
                            entityReplace.RootId = entity.Id;
                            entityReplace.PackageId = packge_id;
                            entityReplace.ServiceId = serviceId;
                            entityReplace.LimitQty = item["LimitQty"]?.ToObject<int?>();
                            entityReplace.IsPackageDrugConsum = item["IsPackageDrugConsum"].ToObject<bool>();
                            entityReplace.ServiceType = item["ServiceType"].ToObject<int>();
                            entityReplace.IsDeleted = item["IsDeleted"] != null ? item["IsDeleted"].ToObject<bool>() : false;
                            if (!entityReplace.IsDeleted)
                            {
                                unitOfWork.ServiceInPackageRepository.Add(entityReplace);
                                newServiceInPackageReplaces.Add(entityReplace.Id);
                            }
                                
                        }
                    }
                }
            }
            listServiceInPackageReplace = newServiceInPackageReplaces;
        }

        #region Price policy setting
        #region Valid for create or update setup price policy
        IHttpActionResult CheckValidInputSetupPolicy(PackagePricePolicyModel request,out bool isValid)
        {
            string actionName = this.ActionContext.ActionDescriptor.ActionName;
            string strValue = string.Empty;
            #region Valid Data
            if (request != null)
            {
                //Get Package Info
                var pkgEntity = unitOfWork.PackageRepository.Find(x => x.Id == request.PackageId).FirstOrDefault();
                if (pkgEntity == null)
                {
                    //Không tìm thấy thông tin gói
                    isValid = false;
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                }
                /*Valid không được chọn 2 lần 1 site trong 1 chính sách giá*/
                if (request.ListSites?.Count > 0)
                {
                    //get StartDate
                    var startDate = request.Policy?.Select(x=>x.StartAt).FirstOrDefault();
                    request.ListSites?.ForEach(x => x.StartAt = startDate);
                    //tungdd14 loại bỏ các site đã xóa
                    var isDupplicateSite = request.ListSites.Where(x => !x.IsDeleted).GroupBy(x => x.SiteId).Select(y => new { Item = y.Key, Count = y.Count() }).Any(z => z.Count > 1);
                    if (isDupplicateSite)
                    {
                        VM.Common.CustomLog.accesslog.Error(string.Format("{0} Invalid input data: [packageId={1} | Status: {2}. Message={3}", actionName, request?.PackageId, HttpStatusCode.BadRequest, Message.DUPLICATE_SITE));
                        isValid = false;
                        return Content(HttpStatusCode.BadRequest, Message.DUPLICATE_SITE);
                    }
                }
                /*Chính sách giá không được chồng khung thời gian*/
                var isExistPolicy = unitOfWork.PackagePriceSiteRepository.Find(x => !x.PackagePrice.IsNotForRegOnline && x.PackagePrice.PackageId == request.PackageId).ToList().Any(x => (request.ListSites!=null && request.ListSites.Count>0 && request.ListSites.Any(y=>y.Site==null && y.SiteId==x.SiteId 
                && ((y.GetStartAt()<= x.EndAt && y.GetStartAt()>=x.PackagePrice.StartAt) || (y.GetEndAt() <= x.EndAt && y.GetEndAt() >= x.PackagePrice.StartAt) || (y.GetStartAt() <= x.PackagePrice.StartAt && y.GetEndAt() >= x.EndAt) || x.EndAt==null) 
                )));
                if (isExistPolicy)
                {
                    //Đã tồn tại chính sách cho site tại khoảng thời gian đang setup
                    isValid = false;
                    return Content(HttpStatusCode.BadRequest, Message.DUPLICATE_SITE_TIME);
                }
                #region Check Giới hạn số tiền thuôc/VTTH
                if (request.Policy?.Count > 0)
                {
                    var checkDrugConsumAmountGreaterThan = request.Policy.Any(x => x.IsLimitedDrugConsum && x.LimitedDrugConsumAmount > x.Amount);
                    if (checkDrugConsumAmountGreaterThan)
                    {
                        //Giới hạn thuốc/VTTH lớn hơn giá gói
                        isValid = false;
                        return Content(HttpStatusCode.BadRequest, Message.LIMITDRUGCONSUMAMOUNT_GREATER_THAN_AMOUNTPACAKGE);
                    }
                    var checkDrugConsumAmountRequired = request.Policy.Any(x => x.IsLimitedDrugConsum && (x.LimitedDrugConsumAmount ==null || x.LimitedDrugConsumAmount <0));
                    if (checkDrugConsumAmountRequired)
                    {
                        //Giới hạn thuốc/VTTH không hợp lệ
                        isValid = false;
                        return Content(HttpStatusCode.BadRequest, Message.LIMITDRUGCONSUMAMOUNT_INVALID);
                    }
                }
                #endregion .Check Giới hạn số tiền thuôc/VTTH
                /*Check tổng thành tiền trong gói khác giá gói*/
                if (request.Details != null && request.Details.Count > 0)
                {
                    #region Check exist item was not set price on OH
                    List<PackagePriceDetailModel> listItemNotSetPriceOnOH = request.Details.Where(x => x.BasePrice == null && x.ServiceType!=0)?.ToList();
                    if (listItemNotSetPriceOnOH?.Count > 0)
                    {
                        //1 số item chưa được thiết lập giá trên OH
                        isValid = false;
                        var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.ITEM_NOT_SET_PRICE_ON_OH);
                        MessageModel mdMsg = (MessageModel)msg.Clone();
                        string strItemCode = string.Join(", ",listItemNotSetPriceOnOH.Select(x => x.Service.Code).ToList());
                        mdMsg.ViMessage = string.Format(msg.ViMessage, strItemCode);
                        mdMsg.EnMessage = string.Format(msg.EnMessage, strItemCode);
                        return Content(HttpStatusCode.BadRequest, mdMsg);
                    }
                    #endregion .Check exist item was not set price on OH
                    double? total_Package_VN = request.Details.Where(x => x.ServiceType != (int)ServiceInPackageTypeEnum.TOTAL).Sum(x => x.PkgAmount);
                    total_Package_VN = total_Package_VN!=null? Math.Round(total_Package_VN.Value):0;
                    double? total_Package_FN = request.Details.Where(x => x.ServiceType != (int)ServiceInPackageTypeEnum.TOTAL).Sum(x => x.PkgAmountForeign);
                    total_Package_FN = total_Package_FN != null ? Math.Round(total_Package_FN.Value) : 0;
                    if (total_Package_VN != request?.Policy.Where(x => x.PersonalType == (int)PersonalEnum.VIETNAMESE).Select(x => x.Amount).FirstOrDefault())
                    {
                        //Tổng tiền trong gói khác giá gói (For Vietnamese)
                        isValid = false;
                        return Content(HttpStatusCode.BadRequest, Message.TOTAL_AMOUNT_INPACKAGE_NOTEQUAL_VN);
                    }
                    else if (total_Package_FN != request?.Policy.Where(x => x.PersonalType == (int)PersonalEnum.FOREIGN).Select(x => x.Amount).FirstOrDefault())
                    {
                        //Tổng tiền trong gói khác giá gói (For Foreign)
                        isValid = false;
                        return Content(HttpStatusCode.BadRequest, Message.TOTAL_AMOUNT_INPACKAGE_NOTEQUAL_FN);
                    }
                }
            }
            else
            {
                isValid = false;
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            #endregion .Valid Data
            isValid = true;
            return Content(HttpStatusCode.OK, strValue);
        }
        IHttpActionResult CheckValidUpdatePolicyPersonal(PackagePricePolicyModel request, bool isHaveReg, out bool isValid)
        {
            string actionName = this.ActionContext.ActionDescriptor.ActionName;
            string strValue = string.Empty;
            #region Valid Data
            var packageId = request.PackageId.Value;
            if (isHaveReg)
            {
                #region Check Policy per object persional have modify?
                //Get List Exist inside Database 
                var listPolicyInDB = unitOfWork.PackagePriceRepository.Find(x => !x.IsDeleted && x.PackageId == packageId && x.Code == request.Code && !x.IsNotForRegOnline)?.ToList();
                List<PackagePrice> listPolicy = new List<PackagePrice>();
                if (request?.Policy?.Count > 0)
                {
                    foreach (var item in request?.Policy)
                    {
                        var itemInDB = unitOfWork.PackagePriceRepository.FirstOrDefault(x => x.Id == item.Id);
                        if (itemInDB == null)
                        {
                            //Đã có thay đổi thông tin không được phép thay đổi
                            isValid = false;
                            return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_PRICEPOLICY_PERSONAL);
                        }
                        var entity = (PackagePrice)itemInDB.Clone();
                        entity.Amount = item.Amount;
                        entity.ChargeType = item.ChargeType;
                        entity.PersonalType = item.PersonalType;
                        entity.SiteBaseCode = item.SiteBaseCode;
                        entity.StartAt = item.GetStartAt();
                        listPolicy.Add(entity);
                    }
                }
                var ListFirst = listPolicyInDB.Select(x => new PackagePriceModel()
                {
                    Id = x.Id,
                    PersonalType = x.PersonalType,
                    ChargeType = x.ChargeType,
                    SiteBaseCode = x.SiteBaseCode,
                    Amount = x.Amount,
                    StartAt = x.StartAt.Value.ToString(Constant.DATE_FORMAT)
                })?.OrderBy(x=>x.PersonalType).ToList();
                var ListSecond = listPolicy.Select(x => new PackagePriceModel()
                {
                    Id = x.Id,
                    PersonalType = x.PersonalType,
                    ChargeType = x.ChargeType,
                    SiteBaseCode = x.SiteBaseCode,
                    Amount = x.Amount,
                    StartAt = x.StartAt.Value.ToString(Constant.DATE_FORMAT)
                }).OrderBy(x => x.PersonalType).ToList();
                var strFirst = JsonConvert.SerializeObject(ListFirst);
                var strSecond = JsonConvert.SerializeObject(ListSecond);
                if (strFirst != strSecond)
                {
                    //Đã có thay đổi thông tin không được phép thay đổi
                    isValid = false;
                    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_PRICEPOLICY_PERSONAL);
                }
                #endregion .Check Policy per object persional have modify?
            }
            #endregion .Valid Data
            isValid = true;
            return Content(HttpStatusCode.OK, strValue);
        }
        IHttpActionResult CheckValidUpdateApplyPolicyForSite(PackagePricePolicyModel request,bool isHaveReg, List<PackagePrice> listMaster, out bool isValid)
        {
            string actionName = this.ActionContext.ActionDescriptor.ActionName;
            string strValue = string.Empty;
            #region Valid Data
            #region Valid Endate of site
            if (request.ListSites?.Count > 0)
            {
                DateTime? StartDate = request.Policy[0].GetStartAt();
                if (StartDate != null)
                {
                    var xExistCheckValid = request.ListSites.Where(x => !string.IsNullOrEmpty(x.EndAt) && StartDate > x.GetEndAt());
                    if (xExistCheckValid.Any())
                    {
                        var itemSiteInvalid = xExistCheckValid.FirstOrDefault();
                        var siteCode = unitOfWork.SiteRepository.FirstOrDefault(x => x.Id == itemSiteInvalid.SiteId)?.Code;
                        //Tồn tại config site áp dụng có ngày hết hiệu lực nhỏ hơn thời gian áp dụng của chính sách giá
                        var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.LABEL_WARNING_SITEAPPLY_ENDDATE_LESS_START);
                        MessageModel mdMsg = (MessageModel)msg.Clone();
                        mdMsg.ViMessage = string.Format(msg.ViMessage, siteCode);
                        mdMsg.EnMessage = string.Format(msg.EnMessage, siteCode);
                        isValid = false;
                        return Content(HttpStatusCode.BadRequest, mdMsg);
                    }
                }
                
            }
            #endregion
            if (isHaveReg)
            {
                #region Check Site was applied have modify?
                if (listMaster != null)
                {
                    foreach(var itemP in listMaster)
                    {
                        if (request?.ListSites?.Count > 0)
                        {
                            foreach (var item in request.ListSites)
                            {
                                var itemInDB = unitOfWork.PackagePriceSiteRepository.FirstOrDefault(x => !x.IsDeleted && x.PackagePriceId == itemP.Id && x.SiteId == item.SiteId);
                                if (itemInDB != null)
                                {
                                    if (item.IsDeleted)
                                    {
                                        //Site đã được apply không được xóa
                                        isValid = false;
                                        return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_PRICEPOLICY_REMOVESITE);
                                    }
                                    //Valid ngày hết hiệu lực
                                    var endDate = item.GetEndAt();
                                    var endDateInDB = itemInDB.EndAt;
                                    if(endDate!=null && endDate!= endDateInDB)
                                    {
                                        //Get Max Date have Patient reg package
                                        var theEndDateReg = unitOfWork.PatientInPackageRepository.Find(x => x.PackagePriceSite.Id == itemInDB.Id).OrderByDescending(x=>x.CreatedAt).FirstOrDefault()?.CreatedAt;
                                        if (theEndDateReg != null)
                                        {
                                            theEndDateReg = Convert.ToDateTime(theEndDateReg.Value.ToShortDateString());
                                            int compareTime = DateTime.Compare(endDate.Value, theEndDateReg.Value);
                                            if (compareTime < 0)
                                            {
                                                //Thời gian hết hiệu lực không được nhỏ hơn ngày phát sinh khách hàng đăng ký gói gần nhất
                                                isValid = false;
                                                return Content(HttpStatusCode.BadRequest, Message.NOTE_MODIFY_PRICEPOLICY_ENDDATE_EARLIERTHAN);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion .Check Site was applied have modify?
            }
            #endregion .Valid Data
            isValid = true;
            return Content(HttpStatusCode.OK, strValue);
        }
        IHttpActionResult CheckValidUpdateApplyPolicyPriceDetail(PackagePricePolicyModel request, bool isHaveReg, List<PackagePrice> listMaster, out bool isValid)
        {
            string actionName = this.ActionContext.ActionDescriptor.ActionName;
            string strValue = string.Empty;
            #region Valid Data
            if (isHaveReg)
            {
                #region Check Site was applied have modify?
                List<PackagePriceDetail> listDetail = new List<PackagePriceDetail>();
                if (listMaster != null)
                {
                    foreach (var itemP in listMaster)
                    {
                        if (request?.Details?.Count > 0)
                        {
                            foreach (var item in request.Details.Where(x => x.ServiceType != (int)ServiceInPackageTypeEnum.TOTAL))
                            {
                                var itemInDB = unitOfWork.PackagePriceDetailRepository.FirstOrDefault(x => !x.IsDeleted && x.PackagePriceId == itemP.Id && x.ServiceInPackageId == item.ServiceInPackageId);
                                if (itemInDB != null)
                                {
                                    var entity = (PackagePriceDetail)itemInDB.Clone();
                                    if (itemP.PersonalType == (int)PersonalEnum.VIETNAMESE){
                                        entity.BasePrice = item.BasePrice;
                                        entity.BaseAmount = item.BaseAmount;
                                        entity.PkgPrice = item.PkgPrice;
                                        entity.PkgAmount = item.PkgAmount;
                                    }
                                    else if (itemP.PersonalType == (int)PersonalEnum.FOREIGN)
                                    {
                                        entity.BasePrice = item.BasePriceForeign;
                                        entity.BaseAmount = item.BaseAmountForeign;
                                        entity.PkgPrice = item.PkgPriceForeign;
                                        entity.PkgAmount = item.PkgAmountForeign;
                                    }
                                    listDetail.Add(entity);
                                }
                                else
                                {
                                    //Không cho phép thay đổi dịch vụ trong gói
                                    isValid = false;
                                    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_PRICEPOLICY_PERSONAL);
                                }
                            }
                        }
                    }
                    var listMasterId = listMaster.Select(x => x.Id)?.ToList();
                    //var listEntityInDB = unitOfWork.PackagePriceDetailRepository.AsEnumerable().Where(x=>!x.IsDeleted && listMaster.Any(y=>y.Id==x.PackagePriceId))?.ToList();
                    var listEntityInDB = unitOfWork.PackagePriceDetailRepository.Find(x => !x.IsDeleted && listMasterId.Any(y => y == x.PackagePriceId))?.ToList();
                    var listFist = listDetail.OrderBy(x => x.CreatedAt).Select(x => new
                    {
                        x.Id,
                        x.PackagePriceId,
                        x.ServiceInPackageId,
                        x.BasePrice,
                        x.BaseAmount,
                        x.PkgPrice,
                        x.PkgAmount,
                        x.CreatedBy,
                        x.CreatedAt,
                        x.UpdatedBy,
                        x.UpdatedAt,
                        x.IsDeleted,
                        x.DeletedBy,
                        x.DeletedAt
                    });
                    var listSecond = listEntityInDB.OrderBy(x => x.CreatedAt).Select(x => new
                    {
                        x.Id,
                        x.PackagePriceId,
                        x.ServiceInPackageId,
                        x.BasePrice,
                        x.BaseAmount,
                        x.PkgPrice,
                        x.PkgAmount,
                        x.CreatedBy,
                        x.CreatedAt,
                        x.UpdatedBy,
                        x.UpdatedAt,
                        x.IsDeleted,
                        x.DeletedBy,
                        x.DeletedAt
                    });
                    var firstNotSecond = listFist.Except(listSecond)?.ToList();
                    var secondNotFirst = listSecond.Except(listFist)?.ToList();
                    if(firstNotSecond.Any() && secondNotFirst.Any())
                    {
                        //Có sự thay đổi trong chi tiết giá gói
                        isValid = false;
                        return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_PRICEPOLICY_DETAIL);
                    }
                    #region old code compare
                    //var strFirst = JsonConvert.SerializeObject(listDetail.OrderBy(x=>x.CreatedAt).Select(x=>new { 
                    //    x.Id,
                    //    x.PackagePriceId,
                    //    x.ServiceInPackageId,
                    //    x.BasePrice,
                    //    x.BaseAmount,
                    //    x.PkgPrice,
                    //    x.PkgAmount,
                    //    x.CreatedBy,
                    //    x.CreatedAt,
                    //    x.UpdatedBy,
                    //    x.UpdatedAt,
                    //    x.IsDeleted,
                    //    x.DeletedBy,
                    //    x.DeletedAt
                    //}));
                    //var strSecond = JsonConvert.SerializeObject(listEntityInDB.OrderBy(x => x.CreatedAt).Select(x => new {
                    //    x.Id,
                    //    x.PackagePriceId,
                    //    x.ServiceInPackageId,
                    //    x.BasePrice,
                    //    x.BaseAmount,
                    //    x.PkgPrice,
                    //    x.PkgAmount,
                    //    x.CreatedBy,
                    //    x.CreatedAt,
                    //    x.UpdatedBy,
                    //    x.UpdatedAt,
                    //    x.IsDeleted,
                    //    x.DeletedBy,
                    //    x.DeletedAt
                    //}));
                    //if(strFirst!= strSecond)
                    //{
                    //    //Có sự thay đổi trong chi tiết giá gói
                    //    isValid = false;
                    //    return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_PRICEPOLICY_DETAIL);
                    //}
                    #endregion .old code compare
                }
                #endregion .Check Site was applied have modify?
            }
            #endregion .Valid Data
            isValid = true;
            return Content(HttpStatusCode.OK, strValue);
        }
        #endregion .Valid for create or update setup price policy
        private List<PackagePrice> CreateOrUpdatePricePolicy(PackagePricePolicyModel request)
        {
            List<PackagePrice> returnList = null;
            foreach (var item in request.Policy)
            {
                PackagePrice entity = null;
                if (item.Id !=null && item.Id != Guid.Empty)
                {
                    entity = unitOfWork.PackagePriceRepository.FirstOrDefault(x => x.Id == item.Id);
                }
                if (entity != null)
                {
                    //Cập nhật
                    entity.PackageId = request.PackageId;
                    entity.Code = request.Code;
                    entity.PersonalType = item.PersonalType;
                    entity.SiteBaseCode = item.SiteBaseCode;
                    entity.ChargeType = item.ChargeType;
                    entity.Amount = item.Amount;
                    entity.IsLimitedDrugConsum = item.IsLimitedDrugConsum;
                    entity.LimitedDrugConsumAmount = item.LimitedDrugConsumAmount;
                    entity.StartAt = item.GetStartAt();
                    //tungdd14 tính giá vaccine theo hệ số
                    entity.RateINV = item.RateINV;
                    unitOfWork.PackagePriceRepository.Update(entity);
                }
                else
                {
                    string policyCode = GeneratePricePolicyCode(request.PackageId.Value);
                    entity = new PackagePrice
                    {
                        PackageId=request.PackageId,
                        Code= policyCode,
                        PersonalType=item.PersonalType,
                        SiteBaseCode = item.SiteBaseCode,
                        ChargeType =item.ChargeType,
                        Amount=item.Amount,
                        IsLimitedDrugConsum=item.IsLimitedDrugConsum,
                        LimitedDrugConsumAmount=item.LimitedDrugConsumAmount,
                        //tungdd14 tính giá vaccine theo hệ số
                        RateINV = item.RateINV,
                        StartAt=item.GetStartAt()
                    };
                    unitOfWork.PackagePriceRepository.Add(entity);
                }
                if (returnList == null)
                    returnList = new List<PackagePrice>();
                returnList.Add(entity);
            }
            return returnList;
        }
        private void CreateOrUpdatePricePolicyDetail(PackagePriceDetailModel model,List<PackagePrice> listMaster)
        {
            if(listMaster!=null && listMaster.Count>0 && model != null)
            {
                foreach(var item in listMaster)
                {
                    //K.Tra xem đã tồn tại trong setting detail hay chưa
                    var entityDetail = unitOfWork.PackagePriceDetailRepository.FirstOrDefault(e => !e.IsDeleted && e.PackagePriceId == item.Id && e.ServiceInPackageId == model.ServiceInPackageId);
                    if (entityDetail != null)
                    {
                        entityDetail.BasePrice = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.BasePrice: model.BasePriceForeign;
                        entityDetail.BaseAmount = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.BaseAmount : model.BaseAmountForeign;
                        entityDetail.PkgPrice = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.PkgPrice : model.PkgPriceForeign;
                        entityDetail.PkgAmount = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.PkgAmount : model.PkgAmountForeign;
                        unitOfWork.PackagePriceDetailRepository.Update(entityDetail);
                    }
                    else
                    {
                        entityDetail = new PackagePriceDetail {
                            PackagePriceId= item.Id,
                            ServiceInPackageId= model.ServiceInPackageId,
                            BasePrice = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.BasePrice : model.BasePriceForeign,
                            BaseAmount = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.BaseAmount : model.BaseAmountForeign,
                            PkgPrice = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.PkgPrice : model.PkgPriceForeign,
                            PkgAmount = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.PkgAmount : model.PkgAmountForeign
                        };
                        unitOfWork.PackagePriceDetailRepository.Add(entityDetail);
                    }
                }
            }
        }
        private void GrandPricePolicyForSite(PackagePriceSitesModel model, List<PackagePrice> listMaster)
        {
            if (listMaster != null && listMaster.Count > 0 && model != null)
            {
                foreach (var item in listMaster)
                {
                    //K.Tra xem đã tồn tại trong setting PackagePriceSites hay chưa
                    //Trường hợp thay đổi site thành site khác thì updte lại siteId
                    var siteId = model.Site != null ? model.Site.Id : model.SiteId;
                    var entityDetail = unitOfWork.PackagePriceSiteRepository.FirstOrDefault(e => e.PackagePriceId == item.Id && e.SiteId == siteId);
                    if (entityDetail != null)
                    {
                        entityDetail.EndAt = model.GetEndAt();
                        entityDetail.SiteId = model.SiteId;
                        if (model.IsDeleted)
                        {
                            unitOfWork.PackagePriceSiteRepository.HardDelete(entityDetail);
                        }
                        else
                        {
                            entityDetail.IsDeleted = model.IsDeleted;
                            unitOfWork.PackagePriceSiteRepository.Update(entityDetail);
                        }
                    }
                    else
                    {
                        entityDetail = new PackagePriceSite
                        {
                            PackagePriceId = item.Id,
                            SiteId = model.SiteId,
                            EndAt = model.GetEndAt(),
                            IsDeleted= model.IsDeleted
                        };
                        unitOfWork.PackagePriceSiteRepository.Add(entityDetail);
                    }
                    //if (model.SiteId != model.Site.Id)
                    //{
                    //    var pkPriceEntity = unitOfWork.PackagePriceSiteRepository.FirstOrDefault(e => e.PackagePriceId == item.Id && e.SiteId == model.Site.Id);
                    //    if (pkPriceEntity != null)
                    //    {
                    //        unitOfWork.PackagePriceSiteRepository.HardDelete(entityDetail);
                    //    }
                    //}
                }
            }
        }
        private string GeneratePricePolicyCode(Guid packageId)
        {
            string strCode = string.Empty;
            var pkrEntity = unitOfWork.PackageRepository.FirstOrDefault(x => x.Id == packageId);
            if (pkrEntity != null)
            {
                var countPolicy = unitOfWork.PackagePriceRepository.Find(x => x.PackageId == packageId).Count();
                int nextNumber = (int)Math.Round((double)(countPolicy / 2));
                nextNumber++;
                string strinvnumber = $"{nextNumber:D2}";
                strCode = string.Format("{0}_CS{1}", pkrEntity.Code, strinvnumber);
            }
            
            return strCode;
        }
        
        #endregion .Price policy setting
        #endregion .Function 4 Helper
    }
}