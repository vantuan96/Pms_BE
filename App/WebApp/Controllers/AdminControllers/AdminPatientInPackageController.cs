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
using Newtonsoft.Json;
using System.Data;
using OfficeOpenXml;
using System.Web;
using OfficeOpenXml.Style;
using System.Drawing;
using System.IO;
using Zen.Barcode;
using PMS.Business.Helper;

namespace PMS.Controllers.AdminControllers
{
    /// <summary>
    /// Module Patient & Patient in package Management
    /// </summary>
    [SessionAuthorize]
    public class AdminPatientInPackageController : BaseApiController
    {
        readonly string CodeRegex = @"^[a-zA-Z0-9._-]{2,50}$";
        #region Patient Function
        /// <summary>
        /// API Get List Patient
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient")]
        [Permission()]
        public IHttpActionResult GetListPatientAPI([FromUri] PatientParameterModel request)
        {
            if (string.IsNullOrEmpty(request.Pid) && string.IsNullOrEmpty(request.Mobile) && string.IsNullOrEmpty(request.Name) && string.IsNullOrEmpty(request.Birthday))
            {
                return Content(HttpStatusCode.BadRequest, Message.REQUIRE);
            }
            var iQuery = OHConnectionAPI.GetPatients(request);

            int count = iQuery.Count();

            var results = iQuery.OrderBy(e => e.FullName)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new
                {
                    e.Id,
                    e.PatientId,
                    e.FullName,
                    e.PID,
                    e.Gender,
                    e.DateOfBirth,
                    e.Age,
                    e.Mobile,
                    e.Address
                });

            return Content(HttpStatusCode.OK, new { Count = count, Results = results });
        }
        /// <summary>
        /// Get list patient in package (Search patient in package)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package")]
        [Permission()]
        public IHttpActionResult GetListPatientInPackageAPI([FromUri] PatientInPackageParameterModel request)
        {
            var start_time = DateTime.Now;
            var start_time_total = DateTime.Now;
            TimeSpan tp;
            //if (string.IsNullOrEmpty(request?.Pid) && string.IsNullOrEmpty(request?.PackageId) && string.IsNullOrEmpty(request?.Sites) && string.IsNullOrEmpty(request?.Statuses) && string.IsNullOrEmpty(request?.ContractOwner))
            //    return Content(HttpStatusCode.OK, Message.FORMAT_INVALID);

            var xquery = unitOfWork.PatientInPackageRepository.AsQueryable().Where(x => !x.IsDeleted);
            if (!string.IsNullOrEmpty(request?.Search))
            {
                xquery = xquery.Where(
                    e => e.PatientInformation.PID == request.Search
                    || e.PatientInformation.FullName.Contains(request.Search)
                );
            }
            if (!string.IsNullOrEmpty(request?.Pid))
            {
                xquery = xquery.Where(
                    e => e.PatientInformation.PID == request.Pid
                );
            }
            if (!string.IsNullOrEmpty(request?.PackageCode))
            {
                xquery = xquery.Where(
                    e => e.PackagePriceSite.PackagePrice.Package.Code.Contains(request.PackageCode)
                );
            }
            if (!string.IsNullOrEmpty(request?.PackageName))
            {
                xquery = xquery.Where(
                    e => e.PackagePriceSite.PackagePrice.Package.Name.Contains(request.PackageName)
                );
            }

            if (!string.IsNullOrEmpty(request?.Sites))
            {
                if (request.Sites == "0000")
                    xquery = xquery.Where(e => e.PackagePriceSite.SiteId == null);
                else
                {
                    var site_ids = request.GetSites();
                    xquery = xquery.Where(e => site_ids.Contains(e.PackagePriceSite.SiteId));
                }
            }
            if (!string.IsNullOrEmpty(request?.Statuses))
            {
                var statuses = request.GetStatus();
                //tungdd14 fix bug search đang sử dụng và hết hạn vẫn check EndAt < ngày hiện tại
                if (statuses.Contains((int)PatientInPackageEnum.EXPIRED))
                {
                    statuses.Remove((int)PatientInPackageEnum.EXPIRED);
                    xquery = xquery.Where(e => ((e.EndAt.HasValue && e.EndAt.Value < Constant.CurrentDate && e.Status == (int)PatientInPackageEnum.EXPIRED)) || statuses.Contains(e.Status));
                }
                else
                {
                    xquery = xquery.Where(e => statuses.Contains(e.Status));
                }
            }
            if (!string.IsNullOrEmpty(request?.ContractOwner))
            {
                xquery = xquery.Where(
                    e => e.ContractOwner.Contains(request.ContractOwner) || e.ContractOwnerFullName.Contains(request.ContractOwner)
                );
            }
            int count = xquery.Count();
            var items = xquery.OrderByDescending(e => e.CreatedAt).Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(e => new
                {
                    e.Id,
                    e.PatientInformation,
                    e.ContractOwner,
                    e.ContractOwnerFullName,
                    Package = new { PackageId = e.PackagePriceSite.PackagePrice.Package.Id, PackageCode = e.PackagePriceSite.PackagePrice.Package.Code, PackageName = e.PackagePriceSite.PackagePrice.Package.Name },
                    e.PackagePriceSite.Site,
                    e.Status,
                    e.StartAt,
                    e.EndAt,
                    IsExpireDate = (e.EndAt.HasValue && e.EndAt.Value < Constant.CurrentDate)
                });

            #region Log Performace Final
            tp = DateTime.Now - start_time_total;
            CustomLog.performancejoblog.Info(string.Format("Request[Id={0}]: {1} processing spen time in {2} (ms)", JsonConvert.SerializeObject(request), "GetListPatientInPackageAPI", tp.TotalMilliseconds));
            #endregion .Log Performace
            //var entities = items.ToList();
            return Content(HttpStatusCode.OK, new { Count = count, Results = items });
        }
        /// <summary>
        /// API Create or Update Patient in package (Đăng ký gói)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package")]
        [Permission()]
        public IHttpActionResult RegisterPackageAPI([FromBody] PatientInPackageModel request)
        {
            CustomLog.requestlog.Info(string.Format("RegisterPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            #region Check valid data
            if (!ModelState.IsValid)
            {
                var firstMsg = ModelState.Values?.FirstOrDefault()?.Errors?.FirstOrDefault();
                return Content(HttpStatusCode.BadRequest, new { ViMessage = firstMsg.ErrorMessage, EnMessage = firstMsg.ErrorMessage });
            }
            #endregion
            PatientInPackage overlapPiPkg = null;
            var returnValue = new PatientInPackageRepo().RegisterPackage(request, out overlapPiPkg);
            if (returnValue == (int)StatusEnum.SUCCESS)
            {
                return Content(HttpStatusCode.OK, new { PatientId = request.PatientModel?.Id, PatientInPackageId = request?.Id });
            }
            else if (returnValue == (int)StatusEnum.PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER)
            {
                return Content(HttpStatusCode.BadRequest, Message.STARTDATE_EARLER_CONTRACTDATE);
            }
            else if (returnValue == (int)StatusEnum.PATIENT_INPACKAGE_CONTRACTDATE_PRICE_SITE_CREATEDATE_INVALID_EARLIER)
            {
                return Content(HttpStatusCode.BadRequest, Message.CONTRACTDATE_EARLER_ACTIVEDATE_POLICY_SITE);
            }
            else if (returnValue == (int)StatusEnum.TOTAL_AMOUNT_SERVICE_NOT_EQUAL_NETAMOUNT)
            {
                return Content(HttpStatusCode.BadRequest, Message.TOTAL_AMOUNT_SERVICE_NOT_EQUAL_NETAMOUNT);
            }
            else if (returnValue == (int)StatusEnum.TOTAL_AMOUNT_DRUG_CONSUM_GREATER_THAN_NETAMOUNT)
            {
                return Content(HttpStatusCode.BadRequest, Message.TOTAL_AMOUNT_DRUG_CONSUM_GREATER_THAN_NETAMOUNT);
            }
            else if (returnValue == (int)StatusEnum.CONFLICT)
            {
                //Overlap
                var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.OVERLAP_PACKAGE_WARNING);
                MessageModel mdMsg = (MessageModel)msg.Clone();
                mdMsg.ViMessage = string.Format(msg.ViMessage, overlapPiPkg?.PackagePriceSite?.PackagePrice?.Package?.Code, overlapPiPkg?.PackagePriceSite?.PackagePrice?.Package?.Name, overlapPiPkg.StartAt.ToString(Constant.DATE_FORMAT), overlapPiPkg.EndAt?.ToString(Constant.DATE_FORMAT));
                mdMsg.EnMessage = string.Format(msg.EnMessage, overlapPiPkg?.PackagePriceSite?.PackagePrice?.Package?.Code, overlapPiPkg?.PackagePriceSite?.PackagePrice?.Package?.Name, overlapPiPkg.StartAt.ToString(Constant.DATE_FORMAT), overlapPiPkg.EndAt?.ToString(Constant.DATE_FORMAT));
                return Content(HttpStatusCode.BadRequest, mdMsg);
                //return Content(HttpStatusCode.BadRequest, Message.OVERLAP_RANGETIME);
            }
            else
            {
                return Content(HttpStatusCode.BadRequest, Message.FAIL);
            }
        }
        /// <summary>
        /// Get detail service in policy for reg package/re-calculate price/amount in package (If have discount)
        /// </summary>
        /// <param name="policyid"></param>
        /// <param name="pkgamountafterdiscount"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Service/{policyid}")]
        [Permission()]
        public IHttpActionResult GetListPatientInServiceAPI(Guid policyid, [FromUri] string pkgamountafterdiscount)
        {
            if (policyid == null || policyid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            double netAmount = 0;
            int outStatusValue = 1;
            var entities = new PatientInPackageRepo().GetListPatientInPackageService(policyid, pkgamountafterdiscount, out netAmount, out outStatusValue);
            if (outStatusValue == -2)
            {
                return Content(HttpStatusCode.BadRequest, Message.NETAMOUNT_VALUE_SMALLERTHANDRUGNCONSUM);
            }
            var models = entities?.Select(x => new
            {
                x.ServiceInPackageId,
                x.Service,
                x.Qty,
                x.BasePrice,
                x.BaseAmount,
                x.PkgPrice,
                x.PkgAmount,
                x.IsPackageDrugConsum,
                x.ServiceType,
                x.ItemsReplace
            });

            return Content(HttpStatusCode.OK, new { Count = entities?.Count, Results = entities });
        }
        /// <summary>
        /// linhht API Update contract, scale enddate PatientInPackage (chỉnh sửa hợp đồng, gia hạn gói)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/ScaleUpPatientInPackageAPI")]
        [Permission()]
        public IHttpActionResult ScaleUpPatientInPackageAPI([FromBody] PatientInPackageUpdateModel request)
        {
            #region Validate input
            //CustomLog.requestlog.Info(string.Format("ScaleUpPatientInPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            if (!ModelState.IsValid)
            {
                var firstMsg = ModelState.Values?.FirstOrDefault()?.Errors?.FirstOrDefault();
                #region store log action
                LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.SCALEUP, firstMsg.ErrorMessage);
                #endregion store log action
                return Content(HttpStatusCode.BadRequest, new { ViMessage = firstMsg.ErrorMessage, EnMessage = firstMsg.ErrorMessage });
            }
            if (request.Id == Guid.Empty || string.IsNullOrEmpty(request.EndAt))
            {
                return Content(HttpStatusCode.BadRequest, Message.REQUIRE);
            }
            #endregion Validate input
            #region Update contract, scale enddate PatientInPackage
            var returnValue = new PatientInPackageRepo().ScaleUpPatientInPackage(request);
            #endregion Update contract, scale enddate PatientInPackage
            #region return
            if (returnValue == null)
            {
                #region store log action
                LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.SCALEUP, "OK");
                #endregion store log action
                return Content(HttpStatusCode.OK, new { PatientId = request?.PatientId, PatientInPackageId = request?.Id });
            }
            else
            {
                #region store log action
                LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.SCALEUP, returnValue.EnMessage);
                #endregion store log action
                return Content(HttpStatusCode.BadRequest, returnValue);
            }
            #endregion return
        }

        /// <summary>
        /// linhht API Reopen (Mở lại gói đã đóng)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/ReOpenPatientInPackageAPI")]
        [Permission()]
        public IHttpActionResult ReOpenPatientInPackageAPI([FromBody] PatientInPackageReopenModel request)
        {
            #region Validate input
            //CustomLog.requestlog.Info(string.Format("ReOpenPatientInPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

            if (!ModelState.IsValid)
            {
                var firstMsg = ModelState.Values?.FirstOrDefault()?.Errors?.FirstOrDefault();
                #region store log action
                LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.REOPEN, firstMsg.ErrorMessage);
                #endregion store log action
                return Content(HttpStatusCode.BadRequest, new { ViMessage = firstMsg.ErrorMessage, EnMessage = firstMsg.ErrorMessage });
            }
            #endregion Validate input
            #region Update contract, scale enddate PatientInPackage
            var returnValue = new PatientInPackageRepo().ReOpenPatientInPackage(request);
            #endregion Update contract, scale enddate PatientInPackage
            #region return
            if (returnValue == null)
            {
                #region store log action
                LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.REOPEN, "OK");
                #endregion store log action
                return Content(HttpStatusCode.OK, new { PatientId = request.PatientId, PatientInPackageId = request.Id });
            }
            else
            {
                #region store log action
                LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.REOPEN, returnValue.EnMessage);
                #endregion store log action
                return Content(HttpStatusCode.BadRequest, returnValue);
            }
            #endregion return
        }
        #region tái khám Re-Examination
        /// <summary>
        /// linhht Get detail patient in tab re-exmainate
        /// tab Theo dõi tái khám
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/ServiceReExaminate/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult PatientInPackageServiceReExaminateAPI(Guid patientinpackageid)
        {
            var start_time = DateTime.Now;
            var start_time_total = DateTime.Now;
            TimeSpan tp;
            if (patientinpackageid == null || patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            var entities = new PatientInPackageRepo().GetListPatientInPackageServiceUsing(patientinpackageid);
            entities = entities.Where(x => x.IsReExamService && x.QtyWasUsed > 0).ToList();

            if (entities == null || entities.Count == 0)
            {
                return Content(HttpStatusCode.BadRequest, Message.MSG38);
            }

            #region Log Performace Final
            tp = DateTime.Now - start_time_total;
            CustomLog.performancejoblog.Info(string.Format("Request[Id={0}]: {1} processing spen time in {2} (ms)", patientinpackageid, "PatientInPackageServiceReExaminateAPI", tp.TotalMilliseconds));
            #endregion .Log Performace

            return Content(HttpStatusCode.OK, new { Count = entities?.Count, Results = entities });
        }
        /// <summary>
        /// linhht Get detail patient in package using re-exmainate service
        /// mở popup chuyển theo dõi tái khám, lấy các dịch vụ đủ điều kiện chuyển tái khám để chọn
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        //[HttpGet]
        //[Route("admin/Patient/Package/PatientInPackageTrackingReExaminateAPI/{patientinpackageid}")]
        //[Permission()]
        //public IHttpActionResult PatientInPackageTrackingReExaminateAPI(Guid patientinpackageid)
        //{
        //    var start_time = DateTime.Now;
        //    var start_time_total = DateTime.Now;
        //    TimeSpan tp;
        //    if (patientinpackageid == null || patientinpackageid == Guid.Empty)
        //    {
        //        return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
        //    }
        //    var entities = new PatientInPackageRepo().GetListPatientInPackageTrackingReExaminate(patientinpackageid);

        //    #region Log Performace Final
        //    tp = DateTime.Now - start_time_total;
        //    CustomLog.performancejoblog.Info(string.Format("Request[Id={0}]: {1} processing spen time in {2} (ms)", patientinpackageid, "PatientInPackageServiceReExaminateAPI", tp.TotalMilliseconds));
        //    #endregion .Log Performace

        //    return Content(HttpStatusCode.OK, new { Count = entities?.Count, Results = entities });
        //}
        /// <summary>
        /// linhht save re-exmainate service 
        /// lưu các dịch vụ theo dõi tái khám
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/PatientInPackageSaveReExaminateServicesAPI/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult PatientInPackageSaveReExaminateServicesAPI(Guid patientinpackageid)
        {
            #region Validate input
            CustomLog.requestlog.Info(string.Format("ReExamPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(patientinpackageid)));
            if (patientinpackageid == null || patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            #endregion Validate input
            try
            {
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
                var _repo = new PatientInPackageRepo();
                //Capture statistic data using current package
                #region Capture statistic data using current package
                var entities = _repo.GetListPatientInPackageServiceUsing(patientinpackageid);
                entity.DataStatUsing = JsonConvert.SerializeObject(entities);
                unitOfWork.PatientInPackageRepository.Update(entity);
                unitOfWork.Commit();
                #endregion .Capture statistic data using current package
                #region Update status PatientInPackage
                var returnValue = new PatientInPackageRepo().SaveReExaminateServices(patientinpackageid);
                #endregion Update status PatientInPackage
                #region return
                if (returnValue == null)
                {
                    #region store log action
                    LogRepo.AddLogAction(patientinpackageid, "PatientInPackages", (int)ActionEnum.REEXAM, "OK");
                    #endregion store log action
                    return Content(HttpStatusCode.OK, new { PatientInPackageId = patientinpackageid });
                }
                else
                {
                    #region store log action
                    LogRepo.AddLogAction(patientinpackageid, "PatientInPackages", (int)ActionEnum.REEXAM, returnValue.EnMessage);
                    #endregion store log action
                    return Content(HttpStatusCode.BadRequest, returnValue);
                }
                #endregion return
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("ClosePackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// linhht Get save record of re-examination 
        /// lưu ghi nhận tái khám
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("admin/Patient/Package/PatientInPackageSaveReExaminateRecordAPI")]
        //[Permission()]
        //public IHttpActionResult PatientInPackageSaveReExaminateRecordAPI(PatientInPackageReExaminateModel request)
        //{
        //    #region Validate input
        //    //CustomLog.requestlog.Info(string.Format("ReOpenPatientInPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
        //    if (request == null)
        //        return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);

        //    if (!ModelState.IsValid)
        //    {
        //        var firstMsg = ModelState.Values?.FirstOrDefault()?.Errors?.FirstOrDefault();
        //        #region store log action
        //        LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.REOPEN, firstMsg.ErrorMessage);
        //        #endregion store log action
        //        return Content(HttpStatusCode.BadRequest, new { ViMessage = firstMsg.ErrorMessage, EnMessage = firstMsg.ErrorMessage });
        //    }
        //    #endregion Validate input
        //    #region Update contract, scale enddate PatientInPackage
        //    var returnValue = new PatientInPackageRepo().SaveReExaminateRecord(request);
        //    #endregion Update contract, scale enddate PatientInPackage
        //    #region return
        //    if (returnValue == null)
        //    {
        //        #region store log action
        //        LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.REOPEN, "OK");
        //        #endregion store log action
        //        return Content(HttpStatusCode.OK, new { PatientInPackageId = request.Id });
        //    }
        //    else
        //    {
        //        #region store log action
        //        LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.REOPEN, returnValue.EnMessage);
        //        #endregion store log action
        //        return Content(HttpStatusCode.BadRequest, returnValue);
        //    }
        //    #endregion return
        //}
        /// <summary>
        /// Tungdd14: Hủy tái khám
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/CancelReExaminateAPI/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult ClosePackageAPI(Guid patientinpackageid)
        {
            #region Validate input
            CustomLog.requestlog.Info(string.Format("ReExamPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(patientinpackageid)));
            if (patientinpackageid == null || patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            var patientInPackage = unitOfWork.PatientInPackageRepository.GetById(patientinpackageid);
            if (patientInPackage == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_PACKAGE);
            }
            if (patientInPackage.Status != (int)PatientInPackageEnum.RE_EXAMINATE && patientInPackage.LastStatus != (int)PatientInPackageEnum.RE_EXAMINATE)
            {
                return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_STATUS);
            }
            #endregion Validate input
            try
            {
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
                if (entity == null)
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                var listPatientInPackageDetail = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientinpackageid && (x.ReExamQtyWasUsed != null ? x.ReExamQtyWasUsed.Value : 0) > 0 && !x.IsDeleted);
                if (listPatientInPackageDetail.Any())
                {
                    return Content(HttpStatusCode.BadRequest, Message.CANCEL_REEXAM_WAS_USE);
                }

                var now = DateTime.Now;
                //Nếu [Ngày hết hạn] < {Ngày hiện tại}: Cập nhật trạng thái thành “Hết hạn”.
                var endAt = patientInPackage.EndAt?.AddHours(23).AddMinutes(59).AddSeconds(59);
                if (endAt < now)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.EXPIRED;
                    patientInPackage.LastStatus = (int)PatientInPackageEnum.ACTIVATED;
                }
                //Nếu [Ngày hết hạn] ≥ {Ngày hiện tại}: Cập nhật trạng thái gói thành “Đang sử dụng”
                if (patientInPackage.StartAt > now && endAt > now)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.REGISTERED;
                }
                else if (patientInPackage.StartAt < now && now < endAt)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.ACTIVATED;
                }

                var entityDetail = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientinpackageid && !x.IsDeleted && !x.ServiceInPackage.IsDeleted);
                foreach (var item in entityDetail)
                {
                    item.ReExamQtyLimit = 0;
                    unitOfWork.PatientInPackageDetailRepository.Update(item);
                }

                #region Cập nhật thông tin: Bảng tình hình sử dụng gói, Thông tin lượt khám & Thông tin Theo dõi tái khám (nếu có)
                unitOfWork.PatientInPackageRepository.Update(patientInPackage);
                unitOfWork.Commit();
                #endregion
                //Lưu log action khi thực hiện thành công
                #region store log action
                LogRepo.AddLogAction(patientinpackageid, "PatientInPackages", (int)ActionEnum.CANCEL_REEXAM, string.Empty);
                #endregion store log action
                return Content(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("ClosePackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        #endregion tái khám Re-Examination
        #region Detail Patient in package & using package status
        /// <summary>
        /// API get detail Patient in package information
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult PatientInPackageInfoDetailAPI(Guid patientinpackageid)
        {
            try
            {
                #region For statistic performance
                var start_time = DateTime.Now;
                var start_time_total = DateTime.Now;
                TimeSpan tp;
                #endregion .For statistic performance

                if (patientinpackageid == Guid.Empty)
                    return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
                var _repo = new PatientInPackageRepo();
                #region Pre update infor before view
                var ptInPKEntity = _repo.RefreshInformationPatientInPackage(patientinpackageid);
                #endregion

                #region Log Performace
                tp = DateTime.Now - start_time;
                CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step processing spen time in {2} (ms)", patientinpackageid, "PatientInPackageInfoDetailAPI.RefreshInformationPatientInPackage", tp.TotalMilliseconds));
                #endregion .Log Performace
                start_time = DateTime.Now;

                PatientInPackageInfoModel model = null;
                var xQuery = _repo.GetPatientInPackageInfo(patientinpackageid);
                if (xQuery.Any())
                {
                    model = xQuery.Select(x => new PatientInPackageInfoModel
                    {
                        Id = x.Id,
                        PolicyId = x.PackagePriceSite.PackagePrice.Id,
                        NewPatientInPackageId = x.NewPatientInPackageId,
                        SiteId = x.PackagePriceSite.SiteId,
                        SiteCode = x.PackagePriceSite.Site.Code,
                        SiteName = x.PackagePriceSite.Site.Name,
                        PackageCode = x.PackagePriceSite.PackagePrice.Package.Code,
                        PackageName = x.PackagePriceSite.PackagePrice.Package.Name,
                        GroupPackageId = x.PackagePriceSite.PackagePrice.Package.PackageGroup.Id,
                        GroupPackageCode = x.PackagePriceSite.PackagePrice.Package.PackageGroup.Code,
                        GroupPackageName = x.PackagePriceSite.PackagePrice.Package.PackageGroup.Name,
                        IsLimitedDrugConsum = x.PackagePriceSite.PackagePrice.Package.IsLimitedDrugConsum,
                        //Patient Info
                        PatientModel = new PatientInformationModel()
                        {
                            Id = x.PatientInformation.Id,
                            PID = x.PatientInformation.PID,
                            FullName = x.PatientInformation.FullName,
                            Gender = x.PatientInformation.Gender,
                            DateOfBirth = x.PatientInformation.DateOfBirth,
                            Email = x.PatientInformation.Email,
                            Mobile = x.PatientInformation.Mobile,
                            National = x.PatientInformation.National,
                            Address = x.PatientInformation.Address,
                            PatientId = x.PatientInformation.PatientId
                        },
                        PersonalType = x.PackagePriceSite.PackagePrice.PersonalType,
                        ContractNo = x.ContractNo,
                        ContractDate = x.ContractDate != null ? x.ContractDate.Value.ToString(Constant.DATE_FORMAT) : string.Empty,
                        ContractOwnerAd = x.ContractOwner,
                        ContractOwnerFullName = x.ContractOwnerFullName,
                        DoctorConsultAd = x.DoctorConsult,
                        DoctorConsultFullName = x.DoctorConsultFullName,
                        DepartmentId = x.DepartmentId,
                        DepartmentName = x.Department?.ViName,
                        StartAt = x.StartAt.ToString(Constant.DATE_FORMAT),
                        EndAt = x.EndAt != null ? x.EndAt.Value.ToString(Constant.DATE_FORMAT) : string.Empty,
                        //IsMaternityPackage = x.IsMaternityPackage,
                        EstimateBornDate = x.EstimateBornDate != null ? x.EstimateBornDate.Value.ToString(Constant.DATE_FORMAT) : string.Empty,
                        Amount = x.PackagePriceSite.PackagePrice.Amount,
                        IsDiscount = x.IsDiscount,
                        DiscountType = x.DiscountType,
                        DiscountAmount = x.DiscountAmount,
                        NetAmount = x.NetAmount,
                        DiscountNote = x.DiscountNote,
                        CreatedAt = x.CreatedAt,
                        ActivatedAt = x.ActivatedAt,
                        ClosedAt = x.ClosedAt,
                        CancelledAt = x.CancelledAt,
                        TerminatedAt = x.TerminatedAt,
                        TransferredAt = x.TransferredAt,
                        Status = (x.EndAt.HasValue && x.EndAt < Constant.CurrentDate && x.Status == (int)PatientInPackageEnum.REGISTERED) ? (int)PatientInPackageEnum.EXPIRED : x.Status,
                        //tungdd14 check trạng thái Hiện thị với các gói có trạng thái “Theo dõi tái khám” hoặc “Hết hạn” nhưng trước đó đã được chuyển sang trạng thái “Theo dõi tái khám”
                        IsPackageReExam = x.Status == (int)PatientInPackageEnum.RE_EXAMINATE || (x.Status == (int)PatientInPackageEnum.EXPIRED && x.LastStatus == (int)PatientInPackageEnum.RE_EXAMINATE),
                        LastStatus = x.LastStatus

                    }).FirstOrDefault();
                    #region Re-get patient information from HIS and sync
                    if (model != null)
                    {
                        #region Get info package was tranfer (Gói nâng cấp)
                        if (model.NewPatientInPackageId != null && model.NewPatientInPackageId != Guid.Empty)
                        {
                            var tranferPtInPkg = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == model.NewPatientInPackageId);
                            if (tranferPtInPkg != null)
                            {
                                model.PackageTranferId = tranferPtInPkg?.PackagePriceSite?.PackagePrice?.PackageId;
                                model.PackageTranferCode = tranferPtInPkg?.PackagePriceSite?.PackagePrice?.Package.Code;
                                model.PackageTranferName = tranferPtInPkg?.PackagePriceSite?.PackagePrice?.Package.Name;
                            }
                        }
                        #endregion .Get info package was tranfer (Gói nâng cấp)
                        #region Get info package was tranferred (Gói được nâng cấp)
                        var tranferredPtInPkg = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.NewPatientInPackageId == model.Id);
                        if (tranferredPtInPkg != null)
                        {
                            var oldPackage = tranferredPtInPkg?.PackagePriceSite?.PackagePrice?.Package;
                            model.OldPatientInPackageId = tranferredPtInPkg?.Id;
                            model.TransferredFromAt = tranferredPtInPkg.TransferredAt;
                            model.FromPackageId = oldPackage?.Id;
                            model.FromPackageCode = oldPackage?.Code;
                            model.FromPackageName = oldPackage?.Name;
                        }
                        #endregion .Get info package was tranferred (Gói được nâng cấp)

                        #region Log Performace
                        tp = DateTime.Now - start_time;
                        CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step processing spen time in {2} (ms)", patientinpackageid, "PatientInPackageInfoDetailAPI.GetPatientInPackageInfo", tp.TotalMilliseconds));
                        #endregion .Log Performace
                        start_time = DateTime.Now;
                        List<PatientInformationModel> children = null;
                        bool IsIncludeChild = Constant.ListGroupCodeIsIncludeChildPackage.Contains(new PackageGroupRepo().GetPackageGroupRoot(model.GroupPackageCode)?.Code);
                        //if (model.IsMaternityPackage)
                        if (IsIncludeChild)
                        {
                            children = _repo.GetChildrenByPatientInPackageId(model.Id);
                        }
                        var patient = _repo.SyncPatient(model.PatientModel.PID, children);
                        if (patient != null)
                        {
                            model.PatientModel = patient;
                        }
                        //tungdd14 kiểm tra gói khách hàng đăng ký có dịch vụ được cấu hình tái khám không
                        var patientInPackageDetails = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == model.Id);
                        if (patientInPackageDetails.Any())
                        {
                            model.hasReExamService = patientInPackageDetails.Where(x => x.ServiceInPackage.IsReExamService).FirstOrDefault() != null;
                        }
                        #region Log Performace
                        tp = DateTime.Now - start_time;
                        CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step processing spen time in {2} (ms)", patientinpackageid, "PatientInPackageInfoDetailAPI.SyncPatient", tp.TotalMilliseconds));
                        #endregion .Log Performace
                    }

                    #endregion
                }

                #region Log Performace Final
                tp = DateTime.Now - start_time_total;
                CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} processing spen time in {2} (ms)", patientinpackageid, "PatientInPackageInfoDetailAPI", tp.TotalMilliseconds));
                #endregion .Log Performace

                return Content(HttpStatusCode.OK, new { model });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("PatientInPackageInfoDetailAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// Get detail patient in package using service current status
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/Service/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult PatientInPackageServiceUsingAPI(Guid patientinpackageid)
        {
            var start_time = DateTime.Now;
            var start_time_total = DateTime.Now;
            TimeSpan tp;
            if (patientinpackageid == null || patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            var entities = new PatientInPackageRepo().GetListPatientInPackageServiceUsing(patientinpackageid);

            #region Log Performace Final
            tp = DateTime.Now - start_time_total;
            CustomLog.performancejoblog.Info(string.Format("Request[Id={0}]: {1} processing spen time in {2} (ms)", patientinpackageid, "PatientInPackageServiceUsingAPI", tp.TotalMilliseconds));
            #endregion .Log Performace

            return Content(HttpStatusCode.OK, new { Count = entities?.Count, Results = entities });
        }
        /// <summary>
        /// API Export file for statistic service using
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/ExportServiceUsing/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult PatientInPackageExportServiceUsingAPI(Guid patientinpackageid)
        {
            if (patientinpackageid == null || patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            var model = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
            var entities = new PatientInPackageRepo().GetListPatientInPackageServiceUsing(patientinpackageid);
            #region change position total item
            if (entities?.Count > 0)
            {
                var itemTotal = entities.LastOrDefault();
                entities.Remove(itemTotal); //Removes the specific item
                entities.Insert(0, itemTotal);
            }
            #endregion
            var dtTemp = new DataTable { TableName = "PatientInPackageServiceUsingStatusModel_Temp" };
            dtTemp = IList2Table.ToDataTable(entities);
            string strFileName = string.Format("PMS.STAT-SERVICE-USING-{0}-From-" + model?.StartAt + "-To-" + model?.EndAt + "-On-" + DateTime.Now.ToString("ddMMyyHHmm"), patientinpackageid);
            System.Web.Mvc.FileStreamResult dataExport = PatientInPackageServiceUsing_ExportExcel(dtTemp, model, strFileName);
            var returnValue = new System.Web.Mvc.JsonResult()
            {
                Data = new { FileData = dataExport != null ? Convert.ToBase64String(FileUtil.ReadToEnd(dataExport?.FileStream)) : null, FileName = string.Format("{0}.xlsx", strFileName) }
                ,
                MaxJsonLength = Int32.MaxValue
            };
            //Lưu log action khi thực hiện thành công
            #region store log action
            LogRepo.AddLogAction(patientinpackageid, "PatientInPackages", (int)ActionEnum.EXPORTSTATUSING, string.Empty);
            #endregion store log action
            return Content(HttpStatusCode.OK, returnValue);
        }
        /// <summary>
        /// Get list visit in package
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/Visit/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult PatientInPackageVisitAPI(Guid patientinpackageid)
        {
            var start_time = DateTime.Now;
            var start_time_total = DateTime.Now;
            TimeSpan tp;

            if (patientinpackageid == null || patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            var entities = new PatientInPackageRepo().GetListPatientInPackageVisit(patientinpackageid);

            #region Log Performace Final
            tp = DateTime.Now - start_time_total;
            CustomLog.performancejoblog.Info(string.Format("Request[Id={0}]: {1} processing spen time in {2} (ms)", patientinpackageid, "PatientInPackageVisitAPI", tp.TotalMilliseconds));
            #endregion .Log Performace

            return Content(HttpStatusCode.OK, new { Count = entities?.Count, Results = entities });
        }
        #region Apply giá gói
        /// <summary>
        /// API get list charge mapping with service in package to apply into Package
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <param name="listChargeUncheck"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/Charge/ListToConfirm/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult ListChargeToConfirmInPackageAPI(Guid patientinpackageid, [FromUri] string listChargeUncheck)
        {
            var start_time = DateTime.Now;
            var start_time_total = DateTime.Now;
            TimeSpan tp;

            if (patientinpackageid == null || patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            ConfirmServiceInPackageModel model = null;
            try
            {

                //Get master infor
                var _repo = new PatientInPackageRepo();
                var xQuery = _repo.GetPatientInPackageInfo(patientinpackageid);
                if (xQuery.Any())
                {
                    var groupRepo = new PackageGroupRepo();
                    model = xQuery.Select(x => new ConfirmServiceInPackageModel()
                    {
                        PatientInPackageId = x.Id,
                        PID = x.PatientInformation.PID,
                        GroupPackageCode = x.PackagePriceSite.PackagePrice.Package.PackageGroup.Code,
                        PackageCode = x.PackagePriceSite.PackagePrice.Package.Code,
                        PackageName = x.PackagePriceSite.PackagePrice.Package.Name,
                        IsLimitedDrugConsum = x.PackagePriceSite.PackagePrice.Package.IsLimitedDrugConsum,
                        PolicyId = x.PackagePriceSite.PackagePrice.Id,
                        IsMaternityPackage = /*Constant*/HelperBusiness.Instant.ListGroupCodeIsMaternityPackage.Contains(x.PackagePriceSite.PackagePrice.Package.PackageGroup.Code),
                        //linhht bundle payment
                        IsBundlePackage = /*Constant*/HelperBusiness.Instant.ListGroupCodeIsBundlePackage.Contains(x.PackagePriceSite.PackagePrice.Package.PackageGroup.Code),
                        IsIncludeChild = /*Constant*/HelperBusiness.Instant.ListGroupCodeIsIncludeChildPackage.Contains(x.PackagePriceSite.PackagePrice.Package.PackageGroup.Code),
                        EndDate = x.EndAt
                    }).FirstOrDefault();
                }
                #region Log Performace
                tp = DateTime.Now - start_time;
                CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step 1 processing spen time in {2} (ms)", patientinpackageid, "ListChargeToConfirmInPackageAPI.GetPatientInPackageInfo", tp.TotalMilliseconds));
                #endregion .Log Performace
                start_time = DateTime.Now;
                if (model != null)
                {
                    //Get and Syn Patient from OH
                    var patient = _repo.SyncPatient(model.PID);
                    if (patient != null)
                    {
                        model.PatientName = patient.FullName;
                    }
                    //Là gói Thai Sản/MCR (Is include Child)
                    //if (model.IsMaternityPackage)
                    if (model.IsIncludeChild)
                    {
                        //Lấy danh sách CON
                        model.Children = _repo.GetChildrenByPatientInPackageId(patientinpackageid);
                    }

                    #region Log Performace
                    tp = DateTime.Now - start_time;
                    CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step 1 processing spen time in {2} (ms)", patientinpackageid, "ListChargeToConfirmInPackageAPI.SyncPatient", tp.TotalMilliseconds));
                    #endregion .Log Performace
                    start_time = DateTime.Now;

                    //Kiểm tra xem khách hàng có Visit package đang mở hay không
                    #region Check on OH have visit open
                    var checkExistVisitPackage = OHConnectionAPI.CheckExistVisitPackageOpen(model.PID);
                    if (!checkExistVisitPackage)
                    {
                        bool existVisitPackageChild = false;
                        //if(model.IsMaternityPackage && model.Children?.Count > 0)
                        if (model.IsIncludeChild && model.Children?.Count > 0)
                        {
                            foreach (var item in model.Children)
                            {
                                existVisitPackageChild = OHConnectionAPI.CheckExistVisitPackageOpen(item.PID);
                                if (existVisitPackageChild)
                                    break;
                            }
                        }
                        if (!checkExistVisitPackage && !existVisitPackageChild)
                        {
                            //Khách hàng chưa có visit package mở trên OH
                            return Content(HttpStatusCode.BadRequest, Message.NOTEXIST_VISIT_PACKAGE_OPEN);
                        }
                    }

                    #region Log Performace
                    tp = DateTime.Now - start_time;
                    CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step 1 processing spen time in {2} (ms)", patientinpackageid, "ListChargeToConfirmInPackageAPI.CheckExistVisitPackageOpen", tp.TotalMilliseconds));
                    #endregion .Log Performace
                    start_time = DateTime.Now;

                    #endregion .Check on OH have visit open
                    List<string> listChargeBelongPackageUncheck = listChargeUncheck?.Split(',')?.ToList();
                    //Get and mapping Charge from HIS
                    _repo.MappingChargeIntoServiceInPackage(model, patientinpackageid, model.PID, listChargeBelongPackageUncheck);
                    model.SessionProcessId = Guid.NewGuid();
                    #region Update current patient in package id, Current User Process to process confirm
                    if (patient != null && patient?.Id != null)
                        _repo.UpdateCurrentPatientInPackageId(patient.Id.Value, model.Children, patientinpackageid, model.PatientInPackageId, model.SessionProcessId);
                    #endregion
                    #region Log Performace
                    tp = DateTime.Now - start_time;
                    CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step 1 processing spen time in {2} (ms)", patientinpackageid, "ListChargeToConfirmInPackageAPI.MappingChargeIntoServiceInPackage", tp.TotalMilliseconds));
                    #endregion .Log Performace

                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("ListChargeToConfirmInPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
            #region Log Performace Final
            tp = DateTime.Now - start_time_total;
            CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} processing spen time in {2} (ms)", patientinpackageid, "ListChargeToConfirmInPackageAPI", tp.TotalMilliseconds));
            #endregion .Log Performace

            return Content(HttpStatusCode.OK, new { model });
        }
        /// <summary>
        /// API Confirm charge in (belong) package
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/ServiceConfirm")]
        [Permission()]
        public IHttpActionResult ConfirmChargeToInPackageAPI([FromBody] ConfirmServiceInPackageModel request)
        {
            CustomLog.requestlog.Info(string.Format("ConfirmChargeToInPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            string outMsg = string.Empty;
            var returnValue = new PatientInPackageRepo().ConfirmChargeBelongPackage(request, IsCommit: true, out outMsg);
            if (returnValue)
            {
                if (Constant.StatusUpdatePriceOKs.Contains(outMsg))
                {
                    //Lưu log action khi thực hiện thành công
                    #region store log action
                    LogRepo.AddLogAction(request.PatientInPackageId, "PatientInPackages", (int)ActionEnum.APPLYINPACKAGE, string.Empty);
                    #endregion store log action
                    return Content(HttpStatusCode.OK, string.Empty);
                }
                else if (outMsg == Constant.StatusUpdatePriceError_No_User)
                    return Content(HttpStatusCode.BadRequest, Message.OH_USER_NOT_FOUND);
                else
                    return Content(HttpStatusCode.BadRequest, Message.FAIL);
            }
            else
            {
                if (string.IsNullOrEmpty(outMsg))
                    return Content(HttpStatusCode.BadRequest, Message.FAIL);
                else if (outMsg == Constant.Patient_Not_Found)
                {
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_PATIENT);
                }
                else if (outMsg == Constant.Confirm_Apply_Charge_IsOtherSession)
                {
                    return Content(HttpStatusCode.BadRequest, Message.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER_SESSION);
                }
                else if (outMsg == Constant.Confirm_Apply_Charge_IsOtherPatientInPackage)
                {
                    return Content(HttpStatusCode.BadRequest, Message.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER);
                }
                else if (outMsg == Constant.Confirm_Apply_Charge_IsOtherUserProcess)
                {
                    return Content(HttpStatusCode.BadRequest, Message.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER_USER);
                }
                else if (outMsg == Constant.StatusUpdatePriceError_No_User)
                    return Content(HttpStatusCode.BadRequest, Message.OH_USER_NOT_FOUND);
                else if (Constant.StatusUpdatePriceError_DonotExist_ChargeId.Contains(outMsg))
                    return Content(HttpStatusCode.BadRequest, Message.UPDATEPRICE_OH_FAIL_CHARGEID_DONOT_EXIST);
                else if (Constant.StatusUpdatePriceFAILs.Contains(outMsg))
                    return Content(HttpStatusCode.BadRequest, Message.UPDATEPRICE_OH_FAIL);
                else
                    return Content(HttpStatusCode.BadRequest, Message.FAIL);
            }
        }
        #endregion .Apply giá gói
        #region Action CLOSED/CANCELLED/TERMINATED/TRANSFERRED (Đóng gói/Hủy/Hủy ngang/Nâng cấp)
        /// <summary>
        /// Api event action closed patient's package service (Đóng gói dịch vụ)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/Closed")]
        [Permission()]
        public IHttpActionResult ClosePackageAPI([FromBody] JObject request)
        {
            CustomLog.requestlog.Info(string.Format("ClosePackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            try
            {
                var patientinpackageid = new Guid(request["patientinpackageid"]?.ToString());
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
                if (entity == null)
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                //Tungdd14 trường hợp gói ở trạng thái hết hạn thì không cập nhật LastStatus
                if (entity.Status != (int)PatientInPackageEnum.EXPIRED)
                {
                    entity.LastStatus = entity.Status;
                }
                
                entity.Status = (int)PatientInPackageEnum.CLOSED;
                var _repo = new PatientInPackageRepo();
                //Capture statistic data using current package
                #region Capture statistic data using current package
                var entities = _repo.GetListPatientInPackageServiceUsing(patientinpackageid);
                entity.DataStatUsing = JsonConvert.SerializeObject(entities);
                #endregion .Capture statistic data using current package
                entity.ClosedAt = DateTime.Now;

                unitOfWork.PatientInPackageRepository.Update(entity);
                //Release/Giải phóng các chỉ định (Vượt gói, ngoài gói, SL Invalid) khỏi gói dịch vụ
                #region Release charge
                _repo.ReleaseChargeFromPatientInPackageWhenClosed(patientinpackageid, unitOfWork);
                #endregion .Release charge 
                unitOfWork.Commit();
                //Lưu log action khi thực hiện thành công
                #region store log action
                LogRepo.AddLogAction(patientinpackageid, "PatientInPackages", (int)ActionEnum.CLOSED, string.Empty);
                #endregion store log action
                return Content(HttpStatusCode.OK, true);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("ClosePackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// Api event action cancelled patient's package service (Hủy gói dịch vụ)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/Cancelled")]
        [Permission()]
        public IHttpActionResult CancelledPackageAPI([FromBody] JObject request)
        {
            CustomLog.requestlog.Info(string.Format("CancelledPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            try
            {
                var patientinpackageid = new Guid(request["patientinpackageid"]?.ToString());
                bool isconfirmaction = request["isconfirmaction"] != null ? request["isconfirmaction"].ToObject<bool>() : false;
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
                if (entity == null)
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                var _repo = new PatientInPackageRepo();
                #region Check exist charge in package
                var isExistChargeBelong = _repo.CheckExistChargeInsidePackage(patientinpackageid);
                if (!isconfirmaction && isExistChargeBelong)
                {
                    return Content(HttpStatusCode.OK, new { Status = (int)StatusEnum.FOUND, Message = Message.EXIST_CHARGE_INSIDE_PACKAGE });
                }
                #endregion .Check exist charge in package
                //linhht
                entity.LastStatus = entity.Status;
                if (isconfirmaction && !isExistChargeBelong)
                {
                    //Hủy thông thường
                    entity.Status = (int)PatientInPackageEnum.CANCELLED;
                    //Capture statistic data using current package
                    #region Capture statistic data using current package
                    var entities = _repo.GetListPatientInPackageServiceUsing(patientinpackageid);
                    entity.DataStatUsing = JsonConvert.SerializeObject(entities);
                    #endregion .Capture statistic data using current package
                    entity.CancelledAt = DateTime.Now;
                    //Release/Giải phóng các chỉ định khỏi gói dịch vụ
                    #region Release charge
                    _repo.ReleaseChargeFromPatientInPackage(patientinpackageid, unitOfWork);
                    #endregion .Release charge 
                    unitOfWork.PatientInPackageRepository.Update(entity);
                    unitOfWork.Commit();
                    //Lưu log action khi thực hiện thành công
                    #region store log action
                    LogRepo.AddLogAction(patientinpackageid, "PatientInPackages", (int)ActionEnum.CANCELLED, string.Empty);
                    #endregion store log action
                    return Content(HttpStatusCode.OK, Message.SUCCESS);
                }
                else if (!isconfirmaction)
                {
                    //Require confirm
                    return Content(HttpStatusCode.OK, new { Status = (int)StatusEnum.PAYMENT_REQUIRED });
                }
                else
                {
                    return Content(HttpStatusCode.OK, new { Status = (int)StatusEnum.NOT_MODIFIED });
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CancelledPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// Api event action terminated patient's package service (Hủy ngang gói dịch vụ)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/Terminated")]
        [Permission()]
        public IHttpActionResult TerminatedPackageAPI([FromBody] JObject request)
        {
            CustomLog.requestlog.Info(string.Format("TerminatedPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            try
            {
                var patientinpackageid = new Guid(request["patientinpackageid"]?.ToString());
                bool isconfirmaction = request["isconfirmaction"] != null ? request["isconfirmaction"].ToObject<bool>() : false;
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
                if (entity == null)
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                var _repo = new PatientInPackageRepo();
                if (!isconfirmaction)
                {
                    //Kiểm tra TH có chỉ định nào đã được xuất bảng kê hay chưa
                    #region Check hava charge have been invoiced yet?
                    var isExistChargeInvoiced = _repo.CheckExistChargeInsidePackageInvoiced(patientinpackageid);
                    if (isExistChargeInvoiced)
                    {
                        //Gói khám của khách đang có chỉ định được thanh toán.
                        return Content(HttpStatusCode.BadRequest, Message.EXIST_CHARGE_INSIDE_PACKAGE_INVOICED);
                    }
                    #endregion .Check hava charge have been invoiced yet?
                    #region ReturnValue to Client Show popup review Stat Chage in Package (Thống kê chỉ định trong gói sau hủy)
                    //Yêu cầu mở Popup thống kê
                    return Content(HttpStatusCode.OK, (int)StatusEnum.REQUIRE_OPEN_POPUP);
                    #endregion .ReturnValue to Client Show popup review Stat Chage in Package (Thống kê chỉ định trong gói sau hủy)
                }
                //Kiểm tra xem khách hàng có Visit đang mở hay không
                #region Check on OH have visit open
                var checkExistVisitPackage = OHConnectionAPI.CheckExistVisitPackageOpen(entity.PatientInformation.PID);
                if (!checkExistVisitPackage)
                {
                    //Khách hàng chưa có visit mở trên OH
                    return Content(HttpStatusCode.BadRequest, Message.NOTEXIST_VISIT_OPEN);
                }
                #endregion .Check on OH have visit open
                if (isconfirmaction)
                {
                    //Hủy ngang gói
                    //Capture statistic data using current package
                    #region Capture statistic data using current package
                    var entities = _repo.GetListPatientInPackageServiceUsing(patientinpackageid);
                    entity.DataStatUsing = JsonConvert.SerializeObject(entities);
                    #endregion .Capture statistic data using current package

                    //Release/Giải phóng các chỉ định khỏi gói dịch vụ
                    #region Release charge
                    _repo.ReleaseChargeFromPatientInPackage(patientinpackageid, unitOfWork);
                    #endregion .Release charge 
                    //linhht
                    entity.LastStatus = entity.Status;

                    entity.Status = (int)PatientInPackageEnum.TERMINATED;
                    entity.TerminatedAt = DateTime.Now;
                    unitOfWork.PatientInPackageRepository.Update(entity);
                    //Cập nhật giá trên OH
                    #region Update price charge on OH
                    var xQueryChargeDetail = unitOfWork.HISChargeDetailRepository.AsQueryable().Where(x => !x.IsDeleted && x.PatientInPackageId == patientinpackageid && x.InPackageType == (int)InPackageType.INPACKAGE);
                    if (xQueryChargeDetail.Any())
                    {
                        List<ChargeInPackageModel> listCharges = xQueryChargeDetail.Select(x => new ChargeInPackageModel()
                        {
                            IsChecked = true,
                            ChargeId = x.HISCharge.ChargeId,
                            InPackageType = x.InPackageType,
                            Price = x.ChargePrice,
                            //Cập nhật lại giá tại thời điểm chỉ định
                            PkgPrice = x.ChargePrice
                        })?.ToList();
                        if (listCharges != null)
                        {
                            string returnMsg = string.Empty;
                            var returnUpdateOH = OHConnectionAPI.UpdateChargePrice(listCharges, out returnMsg);
                            if (returnUpdateOH)
                            {
                                if (Constant.StatusUpdatePriceOKs.Contains(returnMsg))
                                {
                                    unitOfWork.Commit();
                                    return Content(HttpStatusCode.OK, Message.SUCCESS);
                                }
                                else if (returnMsg == Constant.StatusUpdatePriceError_No_User)
                                {
                                    return Content(HttpStatusCode.BadRequest, Message.OH_USER_NOT_FOUND);
                                }
                                else
                                {
                                    return Content(HttpStatusCode.BadRequest, Message.UPDATEPRICE_OH_FAIL);
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(returnMsg))
                                {
                                    return Content(HttpStatusCode.BadRequest, Message.UPDATEPRICE_OH_FAIL);
                                }
                                else if (returnMsg == Constant.StatusUpdatePriceError_No_User)
                                {
                                    return Content(HttpStatusCode.BadRequest, Message.OH_USER_NOT_FOUND);
                                }
                                else
                                {
                                    return Content(HttpStatusCode.BadRequest, Message.UPDATEPRICE_OH_FAIL);
                                }
                            }
                        }
                    }
                    else
                    {
                        unitOfWork.Commit();
                        //Lưu log action khi thực hiện thành công
                        #region store log action
                        LogRepo.AddLogAction(patientinpackageid, "PatientInPackages", (int)ActionEnum.TERMINATED, string.Empty);
                        #endregion store log action
                        return Content(HttpStatusCode.OK, Message.SUCCESS);
                    }
                    #endregion .Update price charge on OH

                }
                //Require confirm
                return Content(HttpStatusCode.OK, new { Status = (int)StatusEnum.PAYMENT_REQUIRED });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("TerminatedPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        #region FN TRANSFERRED PACKAGE
        /// <summary>
        /// Api Check exist invoiced (Kiểm tra có chỉ định nào được xuất bảng kê chưa?)
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/Transferred_CheckExistInvoiced/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult Transferred_CheckExistInvoicedPackageAPI(Guid patientinpackageid)
        {
            if (patientinpackageid == null || patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            try
            {
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
                if (entity == null)
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);

                var _repo = new PatientInPackageRepo();
                //Kiểm tra TH có chỉ định nào đã được xuất bảng kê hay chưa
                #region Check hava charge have been invoiced yet?
                var isExistChargeInvoiced = _repo.CheckExistChargeInsidePackageInvoiced(patientinpackageid);
                if (isExistChargeInvoiced)
                {
                    //Gói khám của khách đang có chỉ định được thanh toán.
                    return Content(HttpStatusCode.BadRequest, Message.TRANSFERRED_EXIST_CHARGE_INSIDE_PACKAGE_INVOICED);
                }
                #endregion .Check hava charge have been invoiced yet?
                //Trả về thông tin gói dịch vụ và message
                var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.MSG31_TRANSFERRED_CONFIRM);
                MessageModel mdMsg = (MessageModel)msg.Clone();
                mdMsg.ViMessage = string.Format(msg.ViMessage, entity?.PackagePriceSite?.PackagePrice?.Package?.Code, entity?.PackagePriceSite?.PackagePrice?.Package?.Name);
                mdMsg.EnMessage = string.Format(msg.EnMessage, entity?.PackagePriceSite?.PackagePrice?.Package?.Code, entity?.PackagePriceSite?.PackagePrice?.Package?.Name);
                return Content(HttpStatusCode.OK, new { PackageCode = entity?.PackagePriceSite?.PackagePrice?.Package?.Code, PackageName = entity?.PackagePriceSite?.PackagePrice?.Package?.Name, Message = mdMsg });
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("Transferred_CheckExistInvoicedPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API check valid data and get data payment information pre transferrence package  (Kiểm tra dữ liệu có hợp lệ để nâng cấp gói hay ko? lấy dữ liệu để show màn hình thông tin thanh toán khi nâng cấp gói)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/Transferred_ViewPaymentInPackage")]
        [Permission()]
        public IHttpActionResult Transferred_ViewPaymentInPackageAPI([FromBody] PatientInPackageTransferredModel request)
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            var start_time_total = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance
            string actionName = this.ActionContext.ActionDescriptor.ActionName;
            CustomLog.requestlog.Info(string.Format("{0}.Request Post from user: {1}, Data: {2}", actionName, GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            try
            {
                var _repoPtInPackage = new PatientInPackageRepo();
                #region Check valid data
                if (!ModelState.IsValid)
                {
                    var firstMsg = ModelState.Values?.FirstOrDefault()?.Errors?.FirstOrDefault();
                    return Content(HttpStatusCode.BadRequest, new { Message = Message.REQUIRE, MsgRequired = firstMsg.ErrorMessage });
                }
                else
                {
                    if (request.IsDiscount && string.IsNullOrEmpty(request.DiscountNote))
                    {
                        return Content(HttpStatusCode.BadRequest, new { Message = Message.REQUIRE, MsgRequired = "The DiscountNote field is required." });
                    }
                    if (!request.PatientModel.Id.HasValue)
                    {
                        //Get and Syn Patient from OH
                        var patient = _repoPtInPackage.SyncPatient(request.PatientModel.PID);
                        if (patient != null)
                        {
                            request.PatientModel = patient;
                        }
                    }
                }
                #region Check overlap package will be transferred
                var pkgPriceSite = unitOfWork.PackagePriceSiteRepository.FirstOrDefault(x => x.SiteId == request.SiteId && x.PackagePriceId == request.PolicyId);
                if (pkgPriceSite == null)
                {
                    //return false;
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_POLICY);
                }
                //Check have setting PackagePriceDetails not yet?
                //var IsSettingPriceDetail = unitOfWork.PackagePriceDetailRepository.AsEnumerable().Any(x => x.PackagePriceId == request.PolicyId && !x.IsDeleted);
                var IsSettingPriceDetail = unitOfWork.PackagePriceDetailRepository.Find(x => x.PackagePriceId == request.PolicyId && !x.IsDeleted).Any();
                if (!IsSettingPriceDetail)
                {
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_POLICY_DETAIL);
                }
                #region Check Have exist reg the same time
                //var pkgEntity = unitOfWork.PackageRepository.AsEnumerable().Where(x => x.PackagePrices.Any(y => y.Id == request.PolicyId)).FirstOrDefault();
                var pkgEntity = unitOfWork.PackageRepository.Find(x => x.PackagePrices.Any(y => y.Id == request.PolicyId)).FirstOrDefault();
                if (pkgEntity == null)
                {
                    //Gói khám không tồn tại
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_PACKAGE);
                }
                PatientInPackage entity = null;
                if (_repoPtInPackage.CheckDupplicateRegistered(request.PatientModel.Id.Value, pkgEntity.Id, request.GetStartAt(), request.GetEndAt(), out entity))
                {
                    //Overlap
                    var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.OVERLAP_PACKAGE_WARNING);
                    MessageModel mdMsg = (MessageModel)msg.Clone();
                    mdMsg.ViMessage = string.Format(msg.ViMessage, entity?.PackagePriceSite?.PackagePrice?.Package?.Code, entity?.PackagePriceSite?.PackagePrice?.Package?.Name, entity.StartAt.ToString(Constant.DATE_FORMAT), entity.EndAt?.ToString(Constant.DATE_FORMAT));
                    mdMsg.EnMessage = string.Format(msg.EnMessage, entity?.PackagePriceSite?.PackagePrice?.Package?.Code, entity?.PackagePriceSite?.PackagePrice?.Package?.Name, entity.StartAt.ToString(Constant.DATE_FORMAT), entity.EndAt?.ToString(Constant.DATE_FORMAT));
                    return Content(HttpStatusCode.BadRequest, mdMsg);
                }
                #endregion
                #endregion .Check overlap package will be transferred
                #region Check change group package - Không cho phép nâng cấp sang gói khác nhóm cha
                //Không cho phép nâng cấp sang gói khác nhóm cha
                var oldPtInPackage = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == request.OldPatientInPackageId);
                if (oldPtInPackage == null)
                {
                    //Gói khám không tồn tại
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_PACKAGE_OLD);
                }
                var transferred_PackageGroupId = pkgPriceSite.PackagePrice.Package.PackageGroupId;
                var old_PackageGroupId = oldPtInPackage.PackagePriceSite.PackagePrice.Package.PackageGroupId;

                if (old_PackageGroupId != transferred_PackageGroupId)
                {
                    //Get Root change
                    var rootPackageChange = unitOfWork.PackageGroupRepository.FirstOrDefault(x => x.Id == transferred_PackageGroupId);
                    if (rootPackageChange != null)
                    {
                        rootPackageChange = new PackageGroupRepo().GetPackageGroupRoot(rootPackageChange);
                    }
                    else
                    {
                        return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_PACKAGEGROUP);
                    }
                    //Get Root current
                    var rootPackageCurrent = new PackageGroupRepo().GetPackageGroupRoot(oldPtInPackage.PackagePriceSite.PackagePrice.Package.PackageGroup);
                    if (rootPackageCurrent?.Id != rootPackageChange?.Id)
                    {
                        //Không được đổi sang nhóm gói có cấp 1 khác
                        VM.Common.CustomLog.accesslog.Error(string.Format("{0} Invalid input data: [packageId={1} | Status: {2}. Message={3}", actionName, request.OldPatientInPackageId, HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ROOTGROUP));
                        return Content(HttpStatusCode.BadRequest, Message.NOTE_NOTALLOW_MODIFY_ROOTGROUP);
                    }
                }
                #endregion .Check change group package - Không cho phép nâng cấp sang gói khác nhóm cha
                #endregion
                #region Build Data payment information model
                //Buil Master (Thông tin thanh toán)
                //Get old package infor
                #region Get old infor package
                request.OldPackageCode = oldPtInPackage?.PackagePriceSite?.PackagePrice?.Package?.Code;
                request.OldPackageName = oldPtInPackage?.PackagePriceSite?.PackagePrice?.Package?.Name;
                request.OldPackageOriginalAmount = oldPtInPackage?.PackagePriceSite?.PackagePrice?.Amount;
                request.OldPackageNetAmount = oldPtInPackage?.NetAmount;
                #endregion Get old infor package
                #region Build new package information (Gói được nâng cấp)
                request.PackageCode = pkgPriceSite?.PackagePrice?.Package.Code;
                request.PackageName = pkgPriceSite?.PackagePrice?.Package.Name;
                request.IsLimitedDrugConsum = pkgPriceSite.PackagePrice.Package.IsLimitedDrugConsum;
                #region Get and set service in package for patient
                double netAmount = 0;
                int outStatusValue = 1;
                #region Log Performace
                start_time = DateTime.Now;
                #endregion .Log Performace
                var entities = _repoPtInPackage.GetListPatientInPackageService(request.PolicyId.Value, request.NetAmount.ToString(), out netAmount, out outStatusValue);
                #region Log Performace
                tp = DateTime.Now - start_time;
                CustomLog.performancejoblog.Info(string.Format("PatientInPackages[Id={0}]: {1} step processing spen time in {2} (ms)", request.OldPatientInPackageId, "Transferred_ViewPaymentInPackageAPI.GetListPatientInPackageService", tp.TotalMilliseconds));
                #endregion .Log Performace
                if (outStatusValue == -2)
                {
                    return Content(HttpStatusCode.BadRequest, Message.NETAMOUNT_VALUE_SMALLERTHANDRUGNCONSUM);
                }
                request.Services = entities?.Select(x => new PatientInPackageDetailModel()
                {
                    ServiceInPackageId = x.ServiceInPackageId,
                    ServiceInPackageRootId = x.ServiceInPackageRootId,
                    Service = x.Service,
                    Qty = x.Qty,
                    BasePrice = x.BasePrice,
                    BaseAmount = x.BaseAmount,
                    PkgPrice = x.PkgPrice,
                    PkgAmount = x.PkgAmount,
                    IsPackageDrugConsum = x.IsPackageDrugConsum,
                    ServiceType = x.ServiceType,
                    ItemsReplace = x.ItemsReplace
                })?.ToList();
                #endregion
                //Là gói Thai Sản
                //if (request.IsMaternityPackage)
                if (request.IsIncludeChild)
                {
                    //Lấy danh sách CON
                    request.Children = _repoPtInPackage.GetChildrenByPatientInPackageId(request.OldPatientInPackageId);
                }
                #region generate Charge inside Patient in package detail
                //Get and mapping Charge from HIS
                #region Log Performace
                start_time = DateTime.Now;
                #endregion .Log Performace
                _repoPtInPackage.MappingChargeIntoServiceInPackageTransferred(request, request.PatientModel.PID);
                #endregion .generate Charge inside Patient in package detail
                #region Log Performace
                tp = DateTime.Now - start_time;
                CustomLog.performancejoblog.Info(string.Format("PatientInPackages[Id={0}]: {1} step processing spen time in {2} (ms)", request.OldPatientInPackageId, "Transferred_ViewPaymentInPackageAPI.MappingChargeIntoServiceInPackageTransferred", tp.TotalMilliseconds));
                #endregion .Log Performace

                //Receivable from customer (Thay đổi nguyên tắc thành NetAmount gói sau nâng cấp - NetAmount gói được nâng cấp)
                request.DebitAmount = netAmount - (request.OldPackageNetAmount != null ? request.OldPackageNetAmount.Value : 0);
                request.ReceivableAmount = request.DebitAmount;
                var totalAmountOutSide = request?.listCharge?.Where(x => x.InPackageType == (int)InPackageType.OUTSIDEPACKAGE)?.Sum(x => x.Amount);
                if (totalAmountOutSide != null)
                    request.ReceivableAmount += totalAmountOutSide;
                var totalAmountOver = request?.listCharge?.Where(x => x.InPackageType == (int)InPackageType.OVERPACKAGE)?.Sum(x => x.Amount);
                if (totalAmountOver != null)
                    request.ReceivableAmount += totalAmountOver;
                var totalAmountInvalid = request?.listCharge?.Where(x => x.InPackageType == (int)InPackageType.QTYINCHARGEGREATTHANREMAIN)?.Sum(x => x.Amount);
                if (totalAmountInvalid != null)
                    request.ReceivableAmount += totalAmountInvalid;
                //Phí dịch vụ vượt/ngoài gói
                request.Over_OutSidePackageFee = (totalAmountOutSide != null ? totalAmountOutSide : 0) + (totalAmountOver != null ? totalAmountOver : 0) + (totalAmountInvalid != null ? totalAmountInvalid : 0);
                #endregion .Build new package information (Gói được nâng cấp)
                //Set new id for patient Inpackage
                request.NewPatientInPackageId = Guid.NewGuid();
                request.SessionProcessId = Guid.NewGuid();
                #region Update current patient in package id to process confirm
                if (request != null && request?.PatientModel != null && request?.PatientModel?.Id != null)
                    _repoPtInPackage.UpdateCurrentPatientInPackageId(request.PatientModel.Id.Value, request.Children, request.NewPatientInPackageId, request.OldPatientInPackageId.Value, request.SessionProcessId);
                #endregion
                #region Log Performace Final
                tp = DateTime.Now - start_time_total;
                CustomLog.performancejoblog.Info(string.Format("PatientInPackages[Id={0}]: {1} processing spen time in {2} (ms)", request.OldPatientInPackageId, "Transferred_ViewPaymentInPackageAPI", tp.TotalMilliseconds));
                #endregion .Log Performace
                return Content(HttpStatusCode.OK, new { request });
                #endregion .Build Data payment information model
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("Transferred_ValidDataPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API Post to sync data from OH
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/Transferred_SyncInPackage")]
        [Permission()]
        public IHttpActionResult Transferred_SyncInPackageAPI([FromBody] PatientInPackageTransferredModel request)
        {
            if (request == null)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            try
            {
                return Content(HttpStatusCode.OK, string.Empty);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("Transferred_SyncInPackage fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API function to transferred patient in package (Nâng cấp gói)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("admin/Patient/Package/TransferredPackage")]
        [Permission()]
        public IHttpActionResult TransferredPackageAPI([FromBody] PatientInPackageTransferredModel request)
        {
            CustomLog.requestlog.Info(string.Format("TransferredPackageAPI.Request Post from user: {0}, Data: {1}", GetUser().Username, JsonConvert.SerializeObject(request)));
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            #region Check valid data
            if (!ModelState.IsValid)
            {
                var firstMsg = ModelState.Values?.FirstOrDefault()?.Errors?.FirstOrDefault();
                return Content(HttpStatusCode.BadRequest, new { Message = Message.REQUIRE, MsgRequired = firstMsg.ErrorMessage });
            }
            #endregion .Check valid data
            string outMsg = string.Empty;
            PatientInPackage overlapPiPkg = null;
            var returnValue = new PatientInPackageRepo().TransferredPackage(request, out outMsg, out overlapPiPkg);
            if (returnValue == (int)StatusEnum.SUCCESS)
            {
                return Content(HttpStatusCode.OK, new { PatientId = request.PatientModel?.Id, PatientInPackageId = request?.Id });
            }
            else if (returnValue == (int)StatusEnum.PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER)
            {
                return Content(HttpStatusCode.BadRequest, Message.STARTDATE_EARLER_CONTRACTDATE);
            }
            else if (returnValue == (int)StatusEnum.PATIENT_INPACKAGE_CONTRACTDATE_PRICE_SITE_CREATEDATE_INVALID_EARLIER)
            {
                return Content(HttpStatusCode.BadRequest, Message.CONTRACTDATE_EARLER_ACTIVEDATE_POLICY_SITE);
            }
            else if (returnValue == (int)StatusEnum.TOTAL_AMOUNT_SERVICE_NOT_EQUAL_NETAMOUNT)
            {
                return Content(HttpStatusCode.BadRequest, Message.TOTAL_AMOUNT_SERVICE_NOT_EQUAL_NETAMOUNT);
            }
            else if (returnValue == (int)StatusEnum.TOTAL_AMOUNT_DRUG_CONSUM_GREATER_THAN_NETAMOUNT)
            {
                return Content(HttpStatusCode.BadRequest, Message.TOTAL_AMOUNT_DRUG_CONSUM_GREATER_THAN_NETAMOUNT);
            }
            else if (returnValue == (int)StatusEnum.CONFLICT)
            {
                //Overlap
                var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.OVERLAP_PACKAGE_WARNING);
                MessageModel mdMsg = (MessageModel)msg.Clone();
                mdMsg.ViMessage = string.Format(msg.ViMessage, overlapPiPkg?.PackagePriceSite?.PackagePrice?.Package?.Code, overlapPiPkg?.PackagePriceSite?.PackagePrice?.Package?.Name, overlapPiPkg.StartAt.ToString(Constant.DATE_FORMAT), overlapPiPkg.EndAt?.ToString(Constant.DATE_FORMAT));
                mdMsg.EnMessage = string.Format(msg.EnMessage, overlapPiPkg?.PackagePriceSite?.PackagePrice?.Package?.Code, overlapPiPkg?.PackagePriceSite?.PackagePrice?.Package?.Name, overlapPiPkg.StartAt.ToString(Constant.DATE_FORMAT), overlapPiPkg.EndAt?.ToString(Constant.DATE_FORMAT));
                return Content(HttpStatusCode.BadRequest, mdMsg);
                //return Content(HttpStatusCode.BadRequest, Message.OVERLAP_RANGETIME);
            }
            else
            {
                if (string.IsNullOrEmpty(outMsg))
                    return Content(HttpStatusCode.BadRequest, Message.FAIL);
                else if (outMsg == Constant.Patient_Not_Found)
                {
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND_PATIENT);
                }
                else if (outMsg == Constant.Confirm_Apply_Charge_IsOtherSession)
                {
                    return Content(HttpStatusCode.BadRequest, Message.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER_SESSION);
                }
                else if (outMsg == Constant.Confirm_Apply_Charge_IsOtherPatientInPackage)
                {
                    return Content(HttpStatusCode.BadRequest, Message.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER);
                }
                else if (outMsg == Constant.Confirm_Apply_Charge_IsOtherUserProcess)
                {
                    return Content(HttpStatusCode.BadRequest, Message.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER_USER);
                }
                else if (outMsg == Constant.StatusUpdatePriceError_No_User)
                    return Content(HttpStatusCode.BadRequest, Message.OH_USER_NOT_FOUND);
                else if (Constant.StatusUpdatePriceError_DonotExist_ChargeId.Contains(outMsg))
                    return Content(HttpStatusCode.BadRequest, Message.UPDATEPRICE_OH_FAIL_CHARGEID_DONOT_EXIST);
                else if (Constant.StatusUpdatePriceFAILs.Contains(outMsg))
                    return Content(HttpStatusCode.BadRequest, Message.UPDATEPRICE_OH_FAIL);
                else
                    return Content(HttpStatusCode.BadRequest, Message.FAIL);
            }
        }
        #endregion .FN TRANSFERRED PACKAGE
        #endregion .Action CLOSED/CANCELLED/TERMINATED/TRANSFERRED (Đóng gói/Hủy/Hủy ngang)
        /// <summary>
        /// API get statitic charge via visit (Bảng thống kê chỉ định theo lượt khám)
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="visitcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/Charge/StatisticViaVisit/{pid}")]
        [Permission()]
        public IHttpActionResult GetStatisticChargeViaVisitAPI(string pid, [FromUri] string visitcode)
        {
            if (string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(visitcode))
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            ChargeStatisticModel model = null;
            try
            {
                var _repo = new PatientInPackageRepo();
                //Get and Syn Patient from OH
                var patient = _repo.SyncPatient(pid);
                if (patient != null)
                {
                    model = new ChargeStatisticModel();
                    model.PatientName = patient.FullName;
                    model.PID = patient.PID;
                    //Get statistic
                    _repo.StatisticChargeViaVisitInPackage(model, model.PID, visitcode);
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("GetStatisticChargeViaVisitAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
            return Content(HttpStatusCode.OK, new { model });
        }
        /// <summary>
        /// API Export file for statistic via visit
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="visitcode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/Charge/ExportStatisticViaVisit/{pid}")]
        [Permission()]
        public IHttpActionResult ExportStatisticChargeViaVisitAPI(string pid, [FromUri] string visitcode)
        {
            if (string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(visitcode))
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            ChargeStatisticModel model = null;
            try
            {
                var _repo = new PatientInPackageRepo();
                //Get and Syn Patient from OH
                var patient = _repo.SyncPatient(pid);
                if (patient != null)
                {
                    model = new ChargeStatisticModel();
                    model.PatientName = patient.FullName;
                    model.PID = patient.PID;
                    //Get statistic
                    var entities = _repo.StatisticChargeViaVisitInPackage(model, model.PID, visitcode);
                    var dtTemp = new DataTable { TableName = "ChargeStatisticDetailModel_Temp" };
                    dtTemp = IList2Table.ToDataTable(entities);
                    string strFileName = string.Format("PMS.STAT-CHARGE-VIA-VISIT-PID-{0}-Visit-{1}" + "-On-" + DateTime.Now.ToString("ddMMyyHHmm"), model?.PID, visitcode);
                    System.Web.Mvc.FileStreamResult dataExport = PatientInPackageStatChargeViaVisit_ExportExcel(dtTemp, model, patient, strFileName);
                    var returnValue = new System.Web.Mvc.JsonResult()
                    {
                        Data = new { FileData = dataExport != null ? Convert.ToBase64String(FileUtil.ReadToEnd(dataExport?.FileStream)) : null, FileName = string.Format("{0}.xlsx", strFileName) }
                        ,
                        MaxJsonLength = Int32.MaxValue
                    };
                    if (entities?.Count > 0)
                    {
                        //Lưu log action khi thực hiện thành công
                        #region store log action
                        LogRepo.AddLogAction(entities[0].PatientInPackageId, "PatientInPackages", (int)ActionEnum.EXPORTSTATVIAVISIT, string.Empty);
                        #endregion store log action
                    }
                    return Content(HttpStatusCode.OK, returnValue);
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("ExportStatisticChargeViaVisitAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
            return Content(HttpStatusCode.OK, Message.FORMAT_INVALID);
        }
        /// <summary>
        /// API Stat charge in package when cancelled/
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/Charge/StatisticWhenCancelled/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult GetStatisticChargeWhenCancelledInPackageAPI(Guid patientinpackageid)
        {
            if (patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            ChargeStatisticWhenCancelledModel model = null;
            try
            {
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
                if (entity == null)
                {
                    return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                }
                var _repo = new PatientInPackageRepo();
                //Get and Syn Patient from OH
                var patient = _repo.SyncPatient(entity.PatientInformation.PID);
                if (patient != null)
                {
                    model = new ChargeStatisticWhenCancelledModel();
                    model.PatientName = patient.FullName;
                    model.PID = patient.PID;
                    model.PackageCode = entity.PackagePriceSite.PackagePrice.Package.Code;
                    model.PackageName = entity.PackagePriceSite.PackagePrice.Package.Name;
                    model.PkgAmount = entity.NetAmount;
                    model.StartAt = entity.StartAt;
                    model.EndAt = entity.EndAt;
                    //Get statistic
                    _repo.StatisticChargeInPackageWhenCancelled(model, patientinpackageid);
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("GetStatisticChargeWhenCancelledInPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
            return Content(HttpStatusCode.OK, new { model });
        }
        //Process with Children in package
        #region Children inside maternity package
        /// <summary>
        /// API get children inside Package
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/Children/{patientinpackageid}")]
        [Permission()]
        public IHttpActionResult GetChildrenInPacakgeAPI(Guid patientinpackageid)
        {
            if (patientinpackageid == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            }
            var xquery = unitOfWork.PatientInPackageChildRepository.AsQueryable().Where(x => !x.IsDeleted && x.PatientInPackageId == patientinpackageid);
            var results = xquery.OrderBy(e => e.CreatedAt)
                .Select(e => new
                {
                    e.Id,
                    e.PatientInPackageId,
                    Package = new
                    {
                        e.PatientInPackage.PackagePriceSite.PackagePrice.Package.Code,
                        e.PatientInPackage.PackagePriceSite.PackagePrice.Package.Name
                    },
                    e.PatientInformation,
                    e.CreatedAt,
                    e.CreatedBy,
                    e.UpdatedAt,
                    e.UpdatedBy,
                    e.IsDeleted
                });

            return Content(HttpStatusCode.OK, new { results });
        }
        /// <summary>
        /// API Create New Or Update Service In Package.
        /// Support to insert/update multi Service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Patient/Package/Children")]
        [Permission()]
        public IHttpActionResult CreateOrUpDateChildrenInPackageAPI([FromBody] JObject request)
        {
            try
            {
                List<Guid> listServiceInPackagesReplace = new List<Guid>();
                var packageInPackageId = new Guid(request["PatientInPackageId"]?.ToString());
                if (packageInPackageId == Guid.Empty)
                {
                    return Content(HttpStatusCode.BadRequest, Message.REQUIRE);
                }
                if (request["Children"] == null || request["Children"]?.Count() <= 0)
                {
                    return Content(HttpStatusCode.BadRequest, Message.REQUIRE);
                }
                var _repo = new PatientInPackageRepo();
                foreach (var item in request["Children"])
                {
                    try
                    {
                        var pid = item["pid"]?.ToString();
                        if (string.IsNullOrEmpty(pid))
                            continue;
                        #region Check exist in other mother
                        var otherMotherPatient = _repo.CheckChildBelongOtherMother(pid, packageInPackageId);
                        if (otherMotherPatient != null)
                        {
                            //Warning message used service in patient
                            var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.CHILD_BELONG_OTHERMOTHER);
                            MessageModel mdMsg = (MessageModel)msg.Clone();
                            mdMsg.ViMessage = string.Format(msg.ViMessage, pid, otherMotherPatient.PID, otherMotherPatient.FullName);
                            mdMsg.EnMessage = string.Format(msg.EnMessage, pid, otherMotherPatient.PID, otherMotherPatient.FullName);
                            return Content(HttpStatusCode.BadRequest, mdMsg);
                        }
                        #endregion .Check exist in other mother
                        var itemInDB = unitOfWork.PatientInPackageChildRepository.FirstOrDefault(x => x.PatientInformation.PID == pid && x.PatientInPackageId == packageInPackageId);
                        if (itemInDB != null)
                        {
                            //Đã tồn tại trong DB
                            itemInDB.IsDeleted = item["IsDeleted"] != null ? item["IsDeleted"].ToObject<bool>() : false;
                            var patient = _repo.SyncPatient(pid);
                            if (patient != null)
                            {
                                itemInDB.PatientChildInforId = patient.Id;
                            }
                            unitOfWork.PatientInPackageChildRepository.Update(itemInDB);
                        }
                        else
                        {
                            //Get and Syn Patient from OH
                            var patient = _repo.SyncPatient(pid);
                            if (patient != null)
                            {
                                itemInDB = new PatientInPackageChild();
                                itemInDB.PatientChildInforId = patient.Id;
                                itemInDB.PatientInPackageId = packageInPackageId;
                                itemInDB.IsDeleted = item["IsDeleted"] != null ? item["IsDeleted"].ToObject<bool>() : false;
                                unitOfWork.PatientInPackageChildRepository.Add(itemInDB);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpDateChildrenInPackageAPI fail. Ex: {0}", ex));
                        continue;
                    }
                }
                unitOfWork.Commit();
                return GetChildrenInPacakgeAPI(packageInPackageId);
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpDateChildrenInPackageAPI fail. Ex: {0}", ex));
                return Content(HttpStatusCode.BadRequest, Message.INTERAL_SERVER_ERROR);
            }
        }
        /// <summary>
        /// API Delete children in PatientInPackage.
        /// Can be support multi delection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [CSRFCheck]
        [HttpPost]
        [Route("admin/Patient/Package/DeleteChildren")]
        [Permission()]
        public IHttpActionResult DeleteChildrenInPackageAPI([FromBody] JObject request)
        {
            if (request == null)
                return Content(HttpStatusCode.BadRequest, Message.FORMAT_INVALID);
            var packageInPackageId = new Guid(request["PatientInPackageId"]?.ToString());
            if (packageInPackageId == Guid.Empty)
            {
                return Content(HttpStatusCode.BadRequest, Message.REQUIRE);
            }
            foreach (var s_pid in request["PIDs"])
            {
                try
                {
                    string pid = s_pid.ToString();
                    #region check have service in patientInPackage
                    var existUsing = new PatientInPackageRepo().CheckChildExistServiceUsingInPatientPackage(pid, packageInPackageId);
                    #endregion
                    if (!existUsing)
                    {
                        var entity = unitOfWork.PatientInPackageChildRepository.FirstOrDefault(e => !e.IsDeleted && e.PatientInformation.PID == pid && e.PatientInPackageId == packageInPackageId);
                        if (entity != null)
                        {
                            //Xóa Package
                            unitOfWork.PatientInPackageChildRepository.Delete(entity);
                        }
                    }
                    else
                    {
                        //Find detail child Patient
                        var paEntity = unitOfWork.PatientInformationRepository.FirstOrDefault(x => x.PID == pid && !x.IsDeleted);
                        if (paEntity == null)
                            return Content(HttpStatusCode.BadRequest, Message.NOT_FOUND);
                        //Warning message used service in patient
                        var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.CHILD_DELETE_INSIDE_PACKAGE_WARNING_USED);
                        MessageModel mdMsg = (MessageModel)msg.Clone();
                        mdMsg.ViMessage = string.Format(msg.ViMessage, paEntity.FullName, paEntity.PID);
                        mdMsg.EnMessage = string.Format(msg.EnMessage, paEntity.FullName, paEntity.PID);
                        return Content(HttpStatusCode.BadRequest, mdMsg);
                    }
                }
                catch { }
            }
            unitOfWork.Commit();
            return Content(HttpStatusCode.OK, Message.SUCCESS);
        }
        #endregion .Children inside maternity package
        #endregion .Detail Patient in package & using package status
        #region Some Action for assign on button
        /// <summary>
        /// API Print form statistic via visit FN
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/PrintStatisticViaVisit")]
        [Permission()]
        public IHttpActionResult PrintStatisticViaVisitAPI(Guid patientinpackageid)
        {
            return Content(HttpStatusCode.OK, string.Empty);
        }
        /// <summary>
        /// API Print form registration package
        /// </summary>
        /// <param name="patientinpackageid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("admin/Patient/Package/PrintPackagRegistrationForm")]
        [Permission()]
        public IHttpActionResult PrintPackagRegistrationFormAPI(Guid patientinpackageid)
        {
            return Content(HttpStatusCode.OK, string.Empty);
        }
        #endregion .Some Action for assign on button
        #endregion .Patient Function
        #region Function 4 Helper
        #region Export Statistic using template report
        /// <summary>
        /// Function export stat using package
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="model"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private System.Web.Mvc.FileStreamResult PatientInPackageServiceUsing_ExportExcel(DataTable dt, PatientInPackage model, string fileName)
        {
            try
            {
                using (var officePackage = new ExcelPackage())
                {
                    var ws = officePackage.Workbook.Worksheets.Add(string.Format("PMS.{0}_{1}", model.StartAt.ToString("yyyyMMdd"), model.EndAt.Value.ToString("yyyyMMdd")));
                    //Create WordArt Logo
                    System.Drawing.Image logo = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("/images/Vinmec-logo-200x123.png"));
                    var pic = ws.Drawings.AddPicture("Logo", logo);
                    // Row, RowoffsetPixel, Column, ColumnOffSetPixel
                    pic.SetSize(55);
                    pic.SetPosition(0, 0, 0, 0);
                    pic.SetPosition(6, 15);
                    ws.Cells["A1:B4"].Merge = true;
                    //Create Title Report
                    ws.Cells[1, 3, 2, 9].Merge = true;
                    ws.Cells[1, 3, 2, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[1, 3, 2, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelRichText ert = ws.Cells[1, 3].RichText.Add("BẢNG TÌNH HÌNH SỬ DỤNG GÓI");
                    ert.Bold = true;
                    ert.Color = System.Drawing.Color.Black;
                    ert.FontName = "Arial";
                    ert.Size = 14;
                    #region Barcode
                    //generate barcode image and save it to disk
                    //string barcodeFileName = string.Format("barcode_{0}", model?.PatientInformation.PID);
                    //BarcodeGenerator generator = new BarcodeGenerator(EncodeTypes.Code128, model?.PatientInformation.PID);
                    //generator.Parameters.Barcode.XDimension.Millimeters = 1f;
                    //// Save the image to your system and set its image format to Jpeg
                    //generator.Save(ConfigHelper.Folder_store_temp_barcode + string.Format("{0}.jpg", barcodeFileName), BarCodeImageFormat.Jpeg);
                    ws.Cells[1, 10, 2, 11].Merge = true;
                    ws.Cells[1, 10, 2, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    int maxheight = 35;
                    Code39BarcodeDraw barcode39 = BarcodeDrawFactory.Code39WithoutChecksum;
                    Image imgBarCode = barcode39.Draw(model?.PatientInformation.PID, maxheight);

                    var picBarCode = ws.Drawings.AddPicture("BarCode", imgBarCode);
                    picBarCode.SetSize(90);
                    picBarCode.SetPosition(0, 5, 9, 10);

                    ws.Cells[3, 10, 3, 11].Merge = true;
                    ws.Cells[3, 10, 3, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[3, 10, 3, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["J3"].Value = string.Format("Patient HN #: {0}", model?.PatientInformation.PID);
                    //ws.Cells["J3"].Value = "Patient HN #: 616004661";
                    #endregion
                    //Bind more info

                    #region Package info
                    ws.Cells["C4"].Value = "Họ tên bệnh nhân/ Patient's Name:";
                    ws.Cells["C4"].Style.Font.Bold = true;
                    //ws.Cells["C3"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["D4"].Value = model?.PatientInformation.FullName;
                    //ws.Cells["D3"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["C5"].Value = "Mã gói/ Package code:";
                    ws.Cells["C5"].Style.Font.Bold = true;
                    //ws.Cells["C4"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["D5"].Value = model.PackagePriceSite?.PackagePrice.Package.Code;
                    //ws.Cells["D4"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["C6"].Value = "Tên gói/ Package name:";
                    ws.Cells["C6"].Style.Font.Bold = true;
                    //ws.Cells["C5"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["D6"].Value = model.PackagePriceSite?.PackagePrice.Package.Name;
                    //ws.Cells["D5"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["H4"].Value = "Thời gian áp dụng:";
                    ws.Cells["H4"].Style.Font.Bold = true;
                    ws.Cells[4, 10, 4, 11].Merge = true;
                    ws.Cells[4, 10, 4, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    // ws.Cells["H3"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["J4"].Value = string.Format("{0} - {1}", model.StartAt.ToString(Constant.DATE_FORMAT), model.EndAt.Value.ToString(Constant.DATE_FORMAT));
                    //ws.Cells["J3"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["H5"].Value = "Nguyên giá gói:";
                    ws.Cells["H5"].Style.Font.Bold = true;
                    ws.Cells[5, 10, 5, 11].Merge = true;
                    ws.Cells[5, 10, 5, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //ws.Cells["H4"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["J5"].Value = model.PackagePriceSite.PackagePrice.Amount;
                    ws.Cells["J5"].Style.Numberformat.Format = "#,##0";
                    //ws.Cells["J4"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["H6"].Value = "Mức giảm giá:";
                    ws.Cells["H6"].Style.Font.Bold = true;
                    ws.Cells[6, 10, 6, 11].Merge = true;
                    ws.Cells[6, 10, 6, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //ws.Cells["H5"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["J6"].Value = model.PackagePriceSite.PackagePrice.Amount - model.NetAmount;
                    ws.Cells["J6"].Style.Numberformat.Format = "#,##0";
                    //ws.Cells["J5"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));
                    #endregion
                    //Print time
                    //ws.Cells[4, 12].Value = "In ngày: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                    //ws.Cells[4, 12].Style.Font.Color.SetColor(Color.FromArgb(0, 0, 0));
                    //ws.Cells[4, 12].Style.Font.Name = "Times New Roman";
                    //ws.Cells[4, 12].Style.Font.Size = 10;
                    //ws.Cells[4, 12].Style.Font.Italic = true;
                    //ws.Cells[4, 12].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    //Build Header row
                    List<string> listNotNumber = new List<string> { "CustomerName", "CustomerPID", "ServiceCode", "ServiceName", "RevenueDate", "ChargeDoctor", "ChargeDate", "BillingNumber", "CatName", "ChargeMonth", "OperationDate", "OperationMonth", "OperationDoctor" };
                    List<string> ListColBreak = new List<string>() { "Id", "ServiceInPackageId", "ItemsReplace", "IsPackageDrugConsum", "ServiceType" };
                    //Format the header
                    //Build Header row
                    if (dt.Columns.Count > 0)
                    {
                        //var categories = unitOfWork.ServiceCategoryRepository.Find(x => x.IsShow).OrderBy(x => x.Order).ToList();
                        //Format the header
                        //Bỏ VisitCode ko hiển thị trên excel
                        //Header column
                        ws.Row(8).Height = 32;
                        using (var rng = ws.Cells[8, 1, 8, dt.Columns.Count - 4])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.WrapText = true;
                            rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(219, 219, 219));
                            rng.Style.Font.Size = 12;
                        }
                        ws.Row(9).Height = 32;
                        //Sub header column
                        using (var rng = ws.Cells[9, 1, 9, dt.Columns.Count - 4])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.WrapText = true;
                            rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(219, 219, 219));
                            rng.Style.Font.Size = 12;
                        }
                        ws.View.FreezePanes(10, 3);
                        ws.Cells[9, 1, 9, dt.Columns.Count - 4].AutoFilter = true;
                        int iBreakCol = 0;
                        string GroupData = string.Empty;
                        string GroupColName = string.Empty;
                        //Add column STT
                        ws.Cells[8, 1, 9, 1].Merge = true;
                        ws.Cells[8, 1, 9, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[8, 1, 9, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[8, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[8, 1].Value = currentLang == "en" ? "STT/ No." : "STT/ No.";
                        ws.Column(1).Width = 7;
                        ws.Cells[8, 6, 8, 8].Merge = true;
                        ws.Cells[8, 6, 8, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[8, 6, 8, 8].Value = (currentLang == "en" ? "Was used" : "Đã sử dụng").ToUpper();
                        ws.Cells[8, 9, 8, 10].Merge = true;
                        ws.Cells[8, 9, 8, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[8, 9, 8, 10].Value = (currentLang == "en" ? "use not yet" : "Chưa sử dụng").ToUpper();
                        ws.Cells[8, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[8, 11].Value = (currentLang == "en" ? "Over package" : "Vượt gói").ToUpper();
                        iBreakCol = -1;
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            var sName = dt.Columns[i].ColumnName;
                            if (ListColBreak.Contains(sName))
                            {
                                iBreakCol++;
                                continue;
                            }
                            else
                            {
                                switch (sName)
                                {
                                    case "CustomerPID":
                                        sName = currentLang == "en" ? "PID" : "PID";
                                        ws.Cells[8, (i - iBreakCol) + 1, 9, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[8, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[8, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "ServiceCode":
                                        sName = currentLang == "en" ? "Mã dịch vụ/ Service code" : "Mã dịch vụ/ Service code";
                                        ws.Cells[8, (i - iBreakCol) + 1, 9, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[8, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[8, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "ServiceName":
                                        sName = currentLang == "en" ? "Tên dịch vụ/ Service name" : "Tên dịch vụ/ Service name";
                                        ws.Cells[8, (i - iBreakCol) + 1, 9, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[8, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[8, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "Qty":
                                        sName = currentLang == "en" ? "Qty limited" : "Định mức";
                                        ws.Cells[8, (i - iBreakCol) + 1, 9, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[8, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[8, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "PkgPrice":
                                        sName = currentLang == "en" ? "Price in package" : "Đơn giá trong gói";
                                        ws.Cells[8, (i - iBreakCol) + 1, 9, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[8, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[8, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "QtyWasUsed":
                                        sName = currentLang == "en" ? "SL/ QTY" : "SL/ QTY";
                                        //ws.Cells[5, (i - iBreakCol) + 1, 6, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[9, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[9, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "QtyWasInvoiced":
                                        sName = currentLang == "en" ? "Invoiced QTY" : "Đã xuất bảng kê";
                                        //ws.Cells[5, (i - iBreakCol) + 1, 6, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[9, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[9, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "AmountWasUsed":
                                        sName = currentLang == "en" ? "TT/ Amount" : "TT/ Amount";
                                        //ws.Cells[5, (i - iBreakCol) + 1, 6, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[9, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[9, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "QtyNotUsedYet":
                                        sName = currentLang == "en" ? "SL/ QTY" : "SL/ QTY";
                                        //ws.Cells[5, (i - iBreakCol) + 1, 6, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[9, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[9, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "AmountNotUsedYet":
                                        sName = currentLang == "en" ? "TT/ Amount" : "TT/ Amount";
                                        //ws.Cells[5, (i - iBreakCol) + 1, 6, (i - iBreakCol) + 1].Merge = true;
                                        ws.Cells[9, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[9, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                    case "QtyOver":
                                        sName = currentLang == "en" ? "SL/ QTY" : "SL/ QTY";
                                        break;
                                    case "OperationDate":
                                        sName = currentLang == "en" ? "OperationDate" : "Ngày thực hiện";
                                        ws.Cells[9, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        ws.Cells[9, (i - iBreakCol) + 1].Value = sName;
                                        break;
                                }
                            }

                            //ws.Cells[8, (i - iBreakCol) + 1].Value = dt.Columns[i].ColumnName;
                            ws.Cells[9, (i - iBreakCol) + 1].Value = sName;
                            ws.Cells[8, (i - iBreakCol) + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            ws.Cells[9, (i - iBreakCol) + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                            switch (dt.Columns[i].ColumnName)
                            {
                                case "ServiceCode":
                                    ws.Column((i - iBreakCol) + 1).Width = 15;
                                    break;
                                case "ServiceName":
                                    ws.Column((i - iBreakCol) + 1).Width = 35;
                                    break;
                                default:
                                    //ws.Cells[8, (i - iBreakCol) + 1].AutoFitColumns(15);
                                    ws.Column((i - iBreakCol) + 1).Width = 15;
                                    break;
                            }
                        }
                    }

                    int rowIndexBeginOrders = 10;
                    int rowIndexCurrentRecord = rowIndexBeginOrders;
                    if (dt.Rows.Count > 0)
                    {
                        int iRow = 0;
                        //Begin set value into cell
                        foreach (DataRow dtr in dt.Rows)
                        {
                            bool isTotal = dtr["ServiceType"]?.ToString() == "0";
                            //bool isTotal = false;
                            int iBreakCol = 0;
                            //Set Value for STT
                            if (iRow <= dt.Rows.Count - 1)
                            {
                                ws.Cells[rowIndexCurrentRecord, 1].Value = (!isTotal) ? (iRow + 1).ToString() : " TỔNG:";
                                if (isTotal)
                                {
                                    ws.Cells[rowIndexCurrentRecord, 1].Style.Font.Bold = true;
                                    ws.Cells[rowIndexCurrentRecord, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    ws.Cells[rowIndexCurrentRecord, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(204, 204, 204));
                                    ws.Cells[rowIndexCurrentRecord, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 245, 220));
                                }
                                ws.Cells[rowIndexCurrentRecord, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }
                            iBreakCol = -1;
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                if (ListColBreak.Contains(dt.Columns[i].ColumnName))
                                {
                                    iBreakCol++;
                                    continue;
                                }
                                else
                                {
                                    switch (dt.Columns[i].ColumnName)
                                    {
                                        case "ServiceCode":
                                            ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = dtr["ServiceCode"];
                                            if (isTotal)
                                            {
                                                ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Font.Bold = true;
                                                ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(204, 204, 204));
                                                ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 245, 220));
                                                ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = string.Empty;
                                            }
                                            break;
                                        default:
                                            if (dtr["" + dt.Columns[i].ColumnName + ""] != null &&
                                                listNotNumber.Contains(dt.Columns[i].ColumnName))
                                            {
                                                if (isTotal)
                                                {
                                                    if (dt.Columns[i].ColumnName == "CustomerPID")
                                                    {
                                                        //Set Total at row footer
                                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, (i - iBreakCol) + 1].Merge = true;
                                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = dtr["" + dt.Columns[i].ColumnName + ""];
                                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Font.Bold = true;
                                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(204, 204, 204));
                                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 245, 220));
                                                    }
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Font.Bold = true;
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(204, 204, 204));
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 245, 220));
                                                }
                                                ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                                                if (new List<string> { "RevenueDate", "ChargeDate", "OperationDate" }.Contains(dt.Columns[i].ColumnName) && !isTotal)
                                                {
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                                    string strValue = dtr["" + dt.Columns[i].ColumnName + ""].ToString();
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = dtr["" + dt.Columns[i].ColumnName + ""] != DBNull.Value ? Convert.ToDateTime(dtr["" + dt.Columns[i].ColumnName + ""].ToString()).ToString("dd-MM-yyyy HH:mm") : string.Empty;
                                                }
                                                else
                                                {
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = dtr["" + dt.Columns[i].ColumnName + ""];
                                                }
                                            }
                                            else
                                            {
                                                if (isTotal)
                                                {
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Font.Bold = true;
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.FromArgb(204, 204, 204));
                                                    ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 245, 220));
                                                    //ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1, rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(245, 245, 220));
                                                }
                                                ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                                ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Style.Numberformat.Format = "#,##0";
                                                //ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = dtr["" + dt.Columns[i].ColumnName + ""].ToString();
                                                if (!isTotal)
                                                {
                                                    if (!string.IsNullOrEmpty(dtr["" + dt.Columns[i].ColumnName + ""]?.ToString()))
                                                        ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = dtr["" + dt.Columns[i].ColumnName + ""];
                                                    else
                                                        ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = "0";
                                                }
                                                else if (new List<string>() { "AmountWasUsed", "AmountNotUsedYet" }.Contains(dt.Columns[i].ColumnName))
                                                {
                                                    if (!string.IsNullOrEmpty(dtr["" + dt.Columns[i].ColumnName + ""]?.ToString()))
                                                        ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = dtr["" + dt.Columns[i].ColumnName + ""];
                                                    else
                                                        ws.Cells[rowIndexCurrentRecord, (i - iBreakCol) + 1].Value = "0";
                                                }
                                            }
                                            break;
                                    }
                                }

                            }
                            rowIndexCurrentRecord++;
                            if (!isTotal)
                                iRow++;
                        }
                    }

                    officePackage.Workbook.Properties.Title = "PMS | STAT SERVICE USING";
                    officePackage.Workbook.Properties.Author = string.Join(Environment.NewLine, "info@vinmec.com", " | VINMEC INTERNATIONAL HOSPITAL");
                    officePackage.Workbook.Properties.Company = "VinMec International Hospital";
                    #region Stream file
                    var fileStream = new MemoryStream();
                    officePackage.SaveAs(fileStream);
                    fileStream.Position = 0;
                    var fsr = new System.Web.Mvc.FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    fsr.FileDownloadName = fileName + ".xlsx";
                    return fsr;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Error(string.Format("Error when export Report PatientInPackageServiceUsing_ExportExcel. ExDetail: {0}", ex));
                return null;
            }
        }
        private System.Web.Mvc.FileStreamResult PatientInPackageStatChargeViaVisit_ExportExcel(DataTable dt, ChargeStatisticModel model, PatientInformationModel patient, string fileName)
        {
            try
            {

                using (var officePackage = new ExcelPackage())
                {
                    var ws = officePackage.Workbook.Worksheets.Add(string.Format("PMS.{0}_{1}", model.PID, model.VisitCode));
                    //Create WordArt Logo
                    System.Drawing.Image logo = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("/images/Vinmec-logo-200x123.png"));
                    var pic = ws.Drawings.AddPicture("Logo", logo);
                    // Row, RowoffsetPixel, Column, ColumnOffSetPixel
                    pic.SetSize(55);
                    pic.SetPosition(0, 0, 0, 0);
                    pic.SetPosition(6, 15);
                    ws.Cells["A1:B4"].Merge = true;
                    //Create Title Report
                    ws.Cells[1, 3, 2, 8].Merge = true;
                    ws.Cells[1, 3, 2, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[1, 3, 2, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelRichText ert = ws.Cells[1, 3].RichText.Add("BẢNG KÊ THU PHÍ DỊCH VỤ GÓI");
                    ert.Bold = true;
                    ert.Color = System.Drawing.Color.Black;
                    ert.FontName = "Arial";
                    ert.Size = 14;
                    ws.Cells[3, 3, 3, 8].Merge = true;
                    ws.Cells[3, 3, 3, 8].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[3, 3, 3, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExcelRichText ert_EN = ws.Cells[3, 3].RichText.Add("LIST OF PACKAGE SERVICES");
                    ert_EN.Bold = true;
                    ert_EN.Color = System.Drawing.Color.Black;
                    ert_EN.FontName = "Arial";
                    ert_EN.Italic = true;
                    ert_EN.Size = 11;
                    #region Barcode
                    //generate barcode image and save it to disk
                    //string barcodeFileName = string.Format("barcode_{0}", model?.PatientInformation.PID);
                    //BarcodeGenerator generator = new BarcodeGenerator(EncodeTypes.Code128, model?.PatientInformation.PID);
                    //generator.Parameters.Barcode.XDimension.Millimeters = 1f;
                    //// Save the image to your system and set its image format to Jpeg
                    //generator.Save(ConfigHelper.Folder_store_temp_barcode + string.Format("{0}.jpg", barcodeFileName), BarCodeImageFormat.Jpeg);
                    ws.Cells[1, 9, 2, 10].Merge = true;
                    ws.Cells[1, 9, 2, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    int maxheight = 35;
                    Code39BarcodeDraw barcode39 = BarcodeDrawFactory.Code39WithoutChecksum;
                    Image imgBarCode = barcode39.Draw(model?.PID, maxheight);

                    var picBarCode = ws.Drawings.AddPicture("BarCode", imgBarCode);
                    picBarCode.SetSize(90);
                    picBarCode.SetPosition(0, 5, 8, 50);

                    ws.Cells[3, 9, 3, 10].Merge = true;
                    ws.Cells[3, 9, 3, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    ws.Cells[3, 9, 3, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ws.Cells["I3"].Value = string.Format("Patient HN #: {0}", model?.PID);
                    //ws.Cells["J3"].Value = "Patient HN #: 616004661";
                    #endregion
                    //Bind more info

                    #region Package info
                    ws.Cells["C4"].Value = "Họ tên bệnh nhân/ Patient's Name:";
                    ws.Cells["C4"].Style.Font.Bold = true;
                    //ws.Cells["C3"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["D4"].Value = model?.PatientName;
                    //ws.Cells["D3"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["C5"].Value = "Địa chỉ/ Address:";
                    ws.Cells["C5"].Style.Font.Bold = true;
                    //ws.Cells["C4"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["D5"].Value = patient.Address;
                    //ws.Cells["D4"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    //ws.Cells["C6"].Value = "Tên gói/ Package name:";
                    //ws.Cells["C6"].Style.Font.Bold = true;
                    ////ws.Cells["C5"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    //ws.Cells["D6"].Value = model.PackagePriceSite?.PackagePrice.Package.Name;
                    ////ws.Cells["D5"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["H4"].Value = "Ngày sinh/DOB:";
                    ws.Cells["H4"].Style.Font.Bold = true;
                    ws.Cells[4, 10, 4, 11].Merge = true;
                    ws.Cells[4, 10, 4, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    // ws.Cells["H3"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["J4"].Value = patient.DateOfBirth != null ? patient.DateOfBirth.Value.ToString(Constant.DATE_FORMAT) : "";
                    //ws.Cells["J3"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["H5"].Value = "Ngày khám/ Visit date:";
                    ws.Cells["H5"].Style.Font.Bold = true;
                    ws.Cells[5, 10, 5, 11].Merge = true;
                    ws.Cells[5, 10, 5, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //ws.Cells["H4"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["J5"].Value = model.VisitDate;
                    //ws.Cells["J4"].Style.Font.Color.SetColor(Color.FromArgb(192, 0, 0));

                    ws.Cells["H6"].Value = "Visit No.:";
                    ws.Cells["H6"].Style.Font.Bold = true;
                    ws.Cells[6, 10, 6, 11].Merge = true;
                    ws.Cells[6, 10, 6, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    ws.Cells["J6"].Value = model.VisitCode;

                    ws.Cells["H7"].Value = "Giới tính/ Sex:";
                    ws.Cells["H7"].Style.Font.Bold = true;
                    ws.Cells[7, 10, 7, 11].Merge = true;
                    ws.Cells[7, 10, 7, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    //ws.Cells["H5"].Style.Font.Color.SetColor(Color.FromArgb(0, 112, 192));
                    ws.Cells["J7"].Value = patient.Gender == 1 ? "Nam" : "Nữ";
                    #endregion
                    //Build Header row
                    List<string> listNotNumber = new List<string> { "CustomerName", "CustomerPID", "ServiceCode", "ServiceName", "RevenueDate", "ChargeDoctor", "ChargeDate", "BillingNumber", "CatName", "ChargeMonth", "OperationDate", "OperationMonth", "OperationDoctor" };
                    List<string> ListColBreak = new List<string>() { "ChargeId", "ChargeDate", "RootId", "ItemType", "IsTotal", "IsInvoiced", "InPackageType", "PatientInPackageId" };
                    //Format the header
                    //Build Header row
                    if (dt.Columns.Count > 0)
                    {
                        //var categories = unitOfWork.ServiceCategoryRepository.Find(x => x.IsShow).OrderBy(x => x.Order).ToList();
                        //Format the header
                        //Bỏ VisitCode ko hiển thị trên excel
                        //Header column
                        ws.Row(9).Height = 32;
                        using (var rng = ws.Cells[9, 1, 9, dt.Columns.Count - 8])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.WrapText = true;
                            rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(219, 219, 219));
                            rng.Style.Font.Size = 12;
                        }
                        ws.Row(10).Height = 32;
                        //Sub header column
                        using (var rng = ws.Cells[10, 1, 10, dt.Columns.Count - 8])
                        {
                            rng.Style.Font.Bold = true;
                            rng.Style.WrapText = true;
                            rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(219, 219, 219));
                            rng.Style.Font.Size = 12;
                        }
                        ws.View.FreezePanes(11, 3);
                        ws.Cells[10, 1, 10, dt.Columns.Count - 8].AutoFilter = true;
                        int iBreakCol = 0;
                        string GroupData = string.Empty;
                        string GroupColName = string.Empty;
                        //Add column STT
                        ws.Cells[9, 1, 10, 1].Merge = true;
                        ws.Cells[9, 1, 10, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[9, 1, 10, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[9, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[9, 1].Value = currentLang == "en" ? "STT/ No." : "STT/ No.";
                        ws.Column(1).Width = 7;
                        ws.Cells[9, 2, 10, 2].Merge = true;
                        ws.Cells[9, 2, 10, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[9, 2, 10, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[9, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[9, 2].Value = currentLang == "en" ? "Mã dịch vụ/ Service code" : "Mã dịch vụ/ Service code";
                        ws.Column(2).Width = 15;

                        ws.Cells[9, 3, 10, 3].Merge = true;
                        ws.Cells[9, 3, 10, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[9, 3, 10, 3].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[9, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[9, 3].Value = currentLang == "en" ? "Tên dịch vụ/ Service name" : "Tên dịch vụ/ Service name";
                        ws.Column(3).Width = 35;

                        ws.Cells[9, 4, 10, 4].Merge = true;
                        ws.Cells[9, 4, 10, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[9, 4, 10, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[9, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[9, 4].Value = currentLang == "en" ? "Đơn giá trong gói/ Unit price" : "Đơn giá trong gói/ Unit price";
                        ws.Column(4).Width = 15;

                        ws.Cells[9, 5, 10, 5].Merge = true;
                        ws.Cells[9, 5, 10, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[9, 5, 10, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[9, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[9, 5].Value = currentLang == "en" ? "Đơn giá lẻ/ Unit price over" : "Đơn giá lẻ/ Unit price over";
                        ws.Column(5).Width = 15;

                        ws.Cells[9, 6, 9, 7].Merge = true;
                        ws.Cells[9, 6, 9, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[9, 6, 9, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[9, 6, 9, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[9, 6, 9, 7].Value = (currentLang == "en" ? "Trong gói/ In-package" : "Trong gói/ In-package");
                        ws.Column(6).Width = 8;
                        ws.Cells[10, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[10, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[10, 6].Value = "SL/ QTY";
                        ws.Column(7).Width = 15;
                        ws.Cells[10, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[10, 7].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[10, 7].Value = "TT/ Amount";

                        ws.Cells[9, 8, 9, 9].Merge = true;
                        ws.Cells[9, 8, 9, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[9, 8, 9, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[9, 8, 9, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[9, 8, 9, 9].Value = (currentLang == "en" ? "Vượt gói/ Over-package" : "Vượt gói/ Over-package");
                        ws.Column(8).Width = 8;
                        ws.Cells[10, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[10, 8].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[10, 8].Value = "SL/ QTY";
                        ws.Column(9).Width = 15;
                        ws.Cells[10, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[10, 9].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[10, 9].Value = "TT/ Amount";

                        ws.Cells[9, 10, 10, 10].Merge = true;
                        ws.Cells[9, 10, 10, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[9, 10, 10, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        ws.Cells[9, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws.Cells[9, 10].Value = (currentLang == "en" ? "Ghi chú/ Note" : "Ghi chú/ Note");
                        ws.Column(10).Width = 25;
                        iBreakCol = -1;

                    }

                    int rowIndexBeginOrders = 11;
                    int rowIndexCurrentRecord = rowIndexBeginOrders;
                    if (dt.Rows.Count > 0)
                    {
                        int iRow = 0;
                        int iGroup = 0;
                        int _totalpkgQty = 0;
                        int _totalOutPkgQty = 0;
                        decimal _totalpkgAmount = 0;
                        decimal _totalOutPkgAmount = 0;
                        //Begin set value into cell
                        //Group by Package (in/over package)
                        var dtRowGroups = dt.Select("ItemType=2 and (InPackageType=1 or InPackageType=2)").AsEnumerable().
                            GroupBy(r => new { PackageCode = r["PackageCode"], PacPackageName = r["PackageName"] })
                            .Select(g => g.OrderBy(r => r["PackageName"]).FirstOrDefault());
                        DataTable grPackage = null;
                        if (dtRowGroups.Any())
                        {
                            grPackage = dtRowGroups.CopyToDataTable();
                        }
                        if (grPackage != null && grPackage.Rows.Count > 0)
                        {
                            foreach (DataRow dtrGr in grPackage.Rows)
                            {
                                iGroup++;
                                string PackageCode = dtrGr["PackageCode"]?.ToString();
                                string PackageName = dtrGr["PackageName"]?.ToString();
                                ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 10].Merge = true;
                                ws.Cells[rowIndexCurrentRecord, 1].Style.Font.Bold = true;
                                //ws.Cells[rowIndexCurrentRecord, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                if (!string.IsNullOrEmpty(PackageCode))
                                    ws.Cells[rowIndexCurrentRecord, 1].Value = $"{PackageCode} - {PackageName}";
                                else
                                    ws.Cells[rowIndexCurrentRecord, 1].Value = PackageName;
                                rowIndexCurrentRecord++;
                                //Build Service in/over Package 
                                //var dtSv = dt.AsEnumerable().Where(r=>r.Field<String>("PackageCode").Equals(PackageCode) && r.Field<String>("PackageName").Equals(PackageName));
                                var dtrSv = dt.Select("PackageCode='" + PackageCode + "' and PackageName='" + PackageName + "'");
                                iRow = 0;
                                if (dtrSv?.Count() > 0)
                                {
                                    int totalpkgQty = 0;
                                    int totalOutPkgQty = 0;
                                    decimal totalpkgAmount = 0;
                                    decimal totalOutPkgAmount = 0;
                                    var dtSvG = dtrSv.GroupBy(x => new { ServiceCode = x["ServiceCode"], ServiceName = x["ServiceName"] }).Select(r => r.First());
                                    foreach (DataRow dtr in dtSvG)
                                    {
                                        iRow++;
                                        string serviceCode = dtr["ServiceCode"]?.ToString();
                                        ws.Cells[rowIndexCurrentRecord, 1].Value = iRow;
                                        ws.Cells[rowIndexCurrentRecord, 2].Value = serviceCode;
                                        ws.Cells[rowIndexCurrentRecord, 3].Value = dtr["ServiceName"]?.ToString();
                                        //Get Price In Package
                                        var pkrPriceRow = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(1) && r.Field<String>("ServiceCode").Equals(serviceCode)).FirstOrDefault();
                                        //tungdd14: trường hợp vượt gói không lên giá trị
                                        var OutPkrPriceRow = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(2) && r.Field<String>("ServiceCode").Equals(serviceCode)).FirstOrDefault();
                                        var pkrPrice = pkrPriceRow != null ? pkrPriceRow["Price"]?.ToString() : (OutPkrPriceRow != null ? OutPkrPriceRow["Price"]?.ToString() : string.Empty);
                                        decimal dpkrPrice = 0;
                                        decimal.TryParse(pkrPrice, out dpkrPrice);
                                        var outPkPrice = OutPkrPriceRow != null ? OutPkrPriceRow["ChargePrice"]?.ToString() : string.Empty;
                                        decimal dOutpkrPrice = 0;
                                        decimal.TryParse(outPkPrice, out dOutpkrPrice);
                                        //Get Total qty inpackage 
                                        var pkgQtyTotal = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(1) && r.Field<String>("ServiceCode").Equals(serviceCode)).Sum(x => Convert.ToInt32(x["QtyCharged"].ToString()));
                                        var pkgQtyReExamTotal = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(1) && r.Field<bool>("ChargeIsUseForReExam") && r.Field<String>("ServiceCode").Equals(serviceCode)).Sum(x => Convert.ToInt32(x["QtyCharged"].ToString()));
                                        totalpkgQty += pkgQtyTotal;
                                        ws.Cells[rowIndexCurrentRecord, 4].Value = dpkrPrice;
                                        ws.Cells[rowIndexCurrentRecord, 4].Style.Numberformat.Format = "#,##0";
                                        ws.Cells[rowIndexCurrentRecord, 5].Value = dOutpkrPrice;
                                        ws.Cells[rowIndexCurrentRecord, 5].Style.Numberformat.Format = "#,##0";
                                        if (pkgQtyTotal > 0)
                                            ws.Cells[rowIndexCurrentRecord, 6].Value = pkgQtyTotal;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 6].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 6].Style.Numberformat.Format = "#,##0";
                                        totalpkgAmount += (pkgQtyTotal - pkgQtyReExamTotal) * dpkrPrice;
                                        if (pkgQtyTotal * dpkrPrice > 0)
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 7].Value = (pkgQtyTotal - pkgQtyReExamTotal) * dpkrPrice;
                                        }
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 7].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 7].Style.Numberformat.Format = "#,##0";

                                        //Get Total qty inpackage 
                                        var outpkgQtyTotal = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(2) && r.Field<String>("ServiceCode").Equals(serviceCode)).Sum(x => Convert.ToInt32(x["QtyCharged"].ToString()));
                                        totalOutPkgQty += outpkgQtyTotal;
                                        if (outpkgQtyTotal > 0)
                                            ws.Cells[rowIndexCurrentRecord, 8].Value = outpkgQtyTotal;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 8].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 8].Style.Numberformat.Format = "#,##0";
                                        totalOutPkgAmount += outpkgQtyTotal * dOutpkrPrice;
                                        if (outpkgQtyTotal * dOutpkrPrice > 0)
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 9].Value = outpkgQtyTotal * dOutpkrPrice;
                                        }
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 9].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 9].Style.Numberformat.Format = "#,##0";
                                        if (!string.IsNullOrEmpty(dtr["Notes"]?.ToString()))
                                        {
                                            //string strNote = dtr["Notes"]?.ToString();
                                            //List<MessageModel> msEntity = JsonConvert.DeserializeObject<List<MessageModel>>(strNote);
                                            List<MessageModel> msEntity = (List<MessageModel>)dtr["Notes"];
                                            if (msEntity?.Count > 0)
                                            {
                                                ws.Cells[rowIndexCurrentRecord, 10].Style.WrapText = true;
                                                ws.Cells[rowIndexCurrentRecord, 10].Value = msEntity.Where(x => x.Code == "NOTE_CHARGE_INSIDE_SERVICEREPLACE").Select(x => x.ViMessage).FirstOrDefault();
                                            }
                                        }
                                        for (int i = 1; i <= 10; i++)
                                        {
                                            //ws.Cells[rowIndexCurrentRecord, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            ws.Cells[rowIndexCurrentRecord, i].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                            ws.Cells[rowIndexCurrentRecord, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                        }
                                        rowIndexCurrentRecord++;
                                    }

                                    #region row sum group
                                    ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 5].Merge = true;
                                    ws.Cells[rowIndexCurrentRecord, 1].Style.Font.Bold = true;
                                    ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    ws.Cells[rowIndexCurrentRecord, 1].Value = $"({iGroup})   Tổng / Sum";
                                    _totalpkgQty += totalpkgQty;
                                    if (totalpkgQty > 0)
                                        ws.Cells[rowIndexCurrentRecord, 6].Value = totalpkgQty;
                                    else
                                    {
                                        ws.Cells[rowIndexCurrentRecord, 6].Value = "-";
                                        ws.Cells[rowIndexCurrentRecord, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                    }
                                    ws.Cells[rowIndexCurrentRecord, 6].Style.Numberformat.Format = "#,##0";
                                    _totalpkgAmount += totalpkgAmount;
                                    if (totalpkgAmount > 0)
                                        ws.Cells[rowIndexCurrentRecord, 7].Value = totalpkgAmount;
                                    else
                                    {
                                        ws.Cells[rowIndexCurrentRecord, 7].Value = "-";
                                        ws.Cells[rowIndexCurrentRecord, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                    }
                                    ws.Cells[rowIndexCurrentRecord, 7].Style.Numberformat.Format = "#,##0";
                                    _totalOutPkgQty += totalOutPkgQty;
                                    if (totalOutPkgQty > 0)
                                        ws.Cells[rowIndexCurrentRecord, 8].Value = totalOutPkgQty;
                                    else
                                    {
                                        ws.Cells[rowIndexCurrentRecord, 8].Value = "-";
                                        ws.Cells[rowIndexCurrentRecord, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                    }
                                    ws.Cells[rowIndexCurrentRecord, 8].Style.Numberformat.Format = "#,##0";
                                    _totalOutPkgAmount += totalOutPkgAmount;
                                    if (totalOutPkgAmount > 0)
                                        ws.Cells[rowIndexCurrentRecord, 9].Value = totalOutPkgAmount;
                                    else
                                    {
                                        ws.Cells[rowIndexCurrentRecord, 9].Value = "-";
                                        ws.Cells[rowIndexCurrentRecord, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                    }
                                    ws.Cells[rowIndexCurrentRecord, 9].Style.Numberformat.Format = "#,##0";
                                    for (int i = 5; i <= 10; i++)
                                    {
                                        ws.Cells[rowIndexCurrentRecord, i].Style.Font.Bold = true;
                                        ws.Cells[rowIndexCurrentRecord, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    }
                                    rowIndexCurrentRecord++;
                                    #endregion .row sum group
                                }
                            }
                        }
                        //Group by Package (out package)
                        dtRowGroups = dt.Select("ItemType=2 and (InPackageType=3) and (IsDrugConsum='False')").AsEnumerable().
                            GroupBy(r => new { PacPackageName = r["PackageName"] })
                            .Select(g => g.OrderBy(r => r["PackageName"]).FirstOrDefault());
                        if (dtRowGroups.Any())
                        {
                            grPackage = dtRowGroups.CopyToDataTable();
                            if (grPackage != null && grPackage.Rows.Count > 0)
                            {
                                foreach (DataRow dtrGr in grPackage.Rows)
                                {
                                    iGroup++;
                                    string PackageName = dtrGr["PackageName"]?.ToString();
                                    ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 10].Merge = true;
                                    ws.Cells[rowIndexCurrentRecord, 1].Style.Font.Bold = true;
                                    //ws.Cells[rowIndexCurrentRecord, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    ws.Cells[rowIndexCurrentRecord, 1].Value = PackageName;
                                    rowIndexCurrentRecord++;
                                    //Build Service in/over Package 
                                    //var dtSv = dt.AsEnumerable().Where(r=>r.Field<String>("PackageCode").Equals(PackageCode) && r.Field<String>("PackageName").Equals(PackageName));
                                    var dtrSv = dt.Select("ItemType=2 and InPackageType=3 and (IsDrugConsum='False')");
                                    iRow = 0;
                                    if (dtrSv?.Count() > 0)
                                    {
                                        int totalpkgQty = 0;
                                        int totalOutPkgQty = 0;
                                        decimal totalpkgAmount = 0;
                                        decimal totalOutPkgAmount = 0;
                                        var dtSvG = dtrSv.GroupBy(x => new { ServiceCode = x["ServiceCode"], ServiceName = x["ServiceName"] }).Select(r => r.First());
                                        foreach (DataRow dtr in dtSvG)
                                        {
                                            iRow++;
                                            string serviceCode = dtr["ServiceCode"]?.ToString();
                                            ws.Cells[rowIndexCurrentRecord, 1].Value = iRow;
                                            ws.Cells[rowIndexCurrentRecord, 2].Value = serviceCode;
                                            ws.Cells[rowIndexCurrentRecord, 3].Value = dtr["ServiceName"]?.ToString();
                                            //Get Price In Package
                                            var pkgPriceRow = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(3) && r.Field<String>("ServiceCode").Equals(serviceCode)).FirstOrDefault();
                                            decimal dpkgPrice = 0;
                                            var outPkPrice = pkgPriceRow != null ? pkgPriceRow["ChargePrice"]?.ToString() : string.Empty;
                                            decimal dOutpkrPrice = 0;
                                            decimal.TryParse(outPkPrice, out dOutpkrPrice);
                                            //Get Total qty inpackage 
                                            var pkgQtyTotal = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(1) && r.Field<String>("ServiceCode").Equals(serviceCode)).Sum(x => Convert.ToInt32(x["QtyCharged"].ToString()));
                                            totalpkgQty += pkgQtyTotal;
                                            ws.Cells[rowIndexCurrentRecord, 4].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                            ws.Cells[rowIndexCurrentRecord, 4].Style.Numberformat.Format = "#,##0";

                                            ws.Cells[rowIndexCurrentRecord, 5].Value = dOutpkrPrice;
                                            ws.Cells[rowIndexCurrentRecord, 5].Style.Numberformat.Format = "#,##0";
                                            ws.Cells[rowIndexCurrentRecord, 6].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                            ws.Cells[rowIndexCurrentRecord, 6].Style.Numberformat.Format = "#,##0";
                                            ws.Cells[rowIndexCurrentRecord, 7].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                            ws.Cells[rowIndexCurrentRecord, 7].Style.Numberformat.Format = "#,##0";

                                            //Get Total qty inpackage 
                                            var outpkgQtyTotal = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(3) && r.Field<String>("ServiceCode").Equals(serviceCode)).Sum(x => Convert.ToInt32(x["QtyCharged"].ToString()));
                                            totalOutPkgQty += outpkgQtyTotal;
                                            ws.Cells[rowIndexCurrentRecord, 8].Value = outpkgQtyTotal;
                                            ws.Cells[rowIndexCurrentRecord, 8].Style.Numberformat.Format = "#,##0";
                                            totalOutPkgAmount += outpkgQtyTotal * dOutpkrPrice;
                                            ws.Cells[rowIndexCurrentRecord, 9].Value = outpkgQtyTotal * dOutpkrPrice;
                                            ws.Cells[rowIndexCurrentRecord, 9].Style.Numberformat.Format = "#,##0";
                                            for (int i = 1; i <= 10; i++)
                                            {
                                                //ws.Cells[rowIndexCurrentRecord, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                ws.Cells[rowIndexCurrentRecord, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                            }
                                            rowIndexCurrentRecord++;
                                        }
                                        #region row sum group
                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 5].Merge = true;
                                        ws.Cells[rowIndexCurrentRecord, 1].Style.Font.Bold = true;
                                        ws.Cells[rowIndexCurrentRecord, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                        ws.Cells[rowIndexCurrentRecord, 1].Value = $"({iGroup})   Tổng / Sum";
                                        _totalpkgQty += totalpkgQty;
                                        if (totalpkgQty > 0)
                                            ws.Cells[rowIndexCurrentRecord, 6].Value = totalpkgQty;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 6].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 6].Style.Numberformat.Format = "#,##0";
                                        _totalpkgAmount += totalpkgAmount;
                                        if (totalpkgAmount > 0)
                                            ws.Cells[rowIndexCurrentRecord, 7].Value = totalpkgAmount;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 7].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 7].Style.Numberformat.Format = "#,##0";
                                        _totalOutPkgQty += totalOutPkgQty;
                                        if (totalOutPkgQty > 0)
                                            ws.Cells[rowIndexCurrentRecord, 8].Value = totalOutPkgQty;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 8].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 8].Style.Numberformat.Format = "#,##0";
                                        _totalOutPkgAmount += totalOutPkgAmount;
                                        if (totalOutPkgAmount > 0)
                                            ws.Cells[rowIndexCurrentRecord, 9].Value = totalOutPkgAmount;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 9].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 9].Style.Numberformat.Format = "#,##0";
                                        for (int i = 5; i <= 10; i++)
                                        {
                                            ws.Cells[rowIndexCurrentRecord, i].Style.Font.Bold = true;
                                            ws.Cells[rowIndexCurrentRecord, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                        }
                                        rowIndexCurrentRecord++;
                                        #endregion .row sum group
                                    }
                                }
                            }
                        }

                        //Group by Package(isdrugconsum)
                        dtRowGroups = dt.Select("ItemType=2 and (InPackageType=3) and (IsDrugConsum='True')").AsEnumerable().
                            GroupBy(r => new { PacPackageName = r["PackageName"] })
                            .Select(g => g.OrderBy(r => r["PackageName"]).FirstOrDefault());
                        if (dtRowGroups.Any())
                        {
                            grPackage = dtRowGroups.CopyToDataTable();
                            if (grPackage != null && grPackage.Rows.Count > 0)
                            {
                                foreach (DataRow dtrGr in grPackage.Rows)
                                {
                                    iGroup++;
                                    string PackageName = dtrGr["PackageName"]?.ToString();
                                    ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 10].Merge = true;
                                    ws.Cells[rowIndexCurrentRecord, 1].Style.Font.Bold = true;
                                    //ws.Cells[rowIndexCurrentRecord, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 10].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                    ws.Cells[rowIndexCurrentRecord, 1].Value = PackageName;
                                    rowIndexCurrentRecord++;
                                    //Build Service in/over Package 
                                    //var dtSv = dt.AsEnumerable().Where(r=>r.Field<String>("PackageCode").Equals(PackageCode) && r.Field<String>("PackageName").Equals(PackageName));
                                    var dtrSv = dt.Select("ItemType=2 and InPackageType=3 and (IsDrugConsum='True')");
                                    iRow = 0;
                                    if (dtrSv?.Count() > 0)
                                    {
                                        int totalpkgQty = 0;
                                        int totalOutPkgQty = 0;
                                        decimal totalpkgAmount = 0;
                                        decimal totalOutPkgAmount = 0;
                                        var dtSvG = dtrSv.GroupBy(x => new { ServiceCode = x["ServiceCode"], ServiceName = x["ServiceName"] }).Select(r => r.First());
                                        foreach (DataRow dtr in dtSvG)
                                        {
                                            iRow++;
                                            string serviceCode = dtr["ServiceCode"]?.ToString();
                                            ws.Cells[rowIndexCurrentRecord, 1].Value = iRow;
                                            ws.Cells[rowIndexCurrentRecord, 2].Value = serviceCode;
                                            ws.Cells[rowIndexCurrentRecord, 3].Value = dtr["ServiceName"]?.ToString();
                                            //Get Price In Package
                                            var pkgPriceRow = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(3) && r.Field<String>("ServiceCode").Equals(serviceCode)).FirstOrDefault();
                                            decimal dpkgPrice = 0;
                                            var outPkPrice = pkgPriceRow != null ? pkgPriceRow["ChargePrice"]?.ToString() : string.Empty;
                                            decimal dOutpkrPrice = 0;
                                            decimal.TryParse(outPkPrice, out dOutpkrPrice);
                                            //Get Total qty inpackage 
                                            var pkgQtyTotal = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(1) && r.Field<String>("ServiceCode").Equals(serviceCode)).Sum(x => Convert.ToInt32(x["QtyCharged"].ToString()));
                                            totalpkgQty += pkgQtyTotal;
                                            ws.Cells[rowIndexCurrentRecord, 4].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                            ws.Cells[rowIndexCurrentRecord, 4].Style.Numberformat.Format = "#,##0";

                                            ws.Cells[rowIndexCurrentRecord, 5].Value = dOutpkrPrice;
                                            ws.Cells[rowIndexCurrentRecord, 5].Style.Numberformat.Format = "#,##0";
                                            ws.Cells[rowIndexCurrentRecord, 6].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                            ws.Cells[rowIndexCurrentRecord, 6].Style.Numberformat.Format = "#,##0";
                                            ws.Cells[rowIndexCurrentRecord, 7].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                            ws.Cells[rowIndexCurrentRecord, 7].Style.Numberformat.Format = "#,##0";

                                            //Get Total qty inpackage 
                                            var outpkgQtyTotal = dtrSv.AsEnumerable().Where(r => r.Field<int>("InPackageType").Equals(3) && r.Field<String>("ServiceCode").Equals(serviceCode)).Sum(x => Convert.ToInt32(x["QtyCharged"].ToString()));
                                            totalOutPkgQty += outpkgQtyTotal;
                                            ws.Cells[rowIndexCurrentRecord, 8].Value = outpkgQtyTotal;
                                            ws.Cells[rowIndexCurrentRecord, 8].Style.Numberformat.Format = "#,##0";
                                            totalOutPkgAmount += outpkgQtyTotal * dOutpkrPrice;
                                            ws.Cells[rowIndexCurrentRecord, 9].Value = outpkgQtyTotal * dOutpkrPrice;
                                            ws.Cells[rowIndexCurrentRecord, 9].Style.Numberformat.Format = "#,##0";
                                            for (int i = 1; i <= 10; i++)
                                            {
                                                //ws.Cells[rowIndexCurrentRecord, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                                ws.Cells[rowIndexCurrentRecord, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                            }
                                            rowIndexCurrentRecord++;
                                        }
                                        #region row sum group
                                        ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 5].Merge = true;
                                        ws.Cells[rowIndexCurrentRecord, 1].Style.Font.Bold = true;
                                        ws.Cells[rowIndexCurrentRecord, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                        ws.Cells[rowIndexCurrentRecord, 1].Value = $"({iGroup})   Tổng / Sum";
                                        _totalpkgQty += totalpkgQty;
                                        if (totalpkgQty > 0)
                                            ws.Cells[rowIndexCurrentRecord, 6].Value = totalpkgQty;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 6].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 6].Style.Numberformat.Format = "#,##0";
                                        _totalpkgAmount += totalpkgAmount;
                                        if (totalpkgAmount > 0)
                                            ws.Cells[rowIndexCurrentRecord, 7].Value = totalpkgAmount;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 7].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 7].Style.Numberformat.Format = "#,##0";
                                        _totalOutPkgQty += totalOutPkgQty;
                                        if (totalOutPkgQty > 0)
                                            ws.Cells[rowIndexCurrentRecord, 8].Value = totalOutPkgQty;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 8].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 8].Style.Numberformat.Format = "#,##0";
                                        _totalOutPkgAmount += totalOutPkgAmount;
                                        if (totalOutPkgAmount > 0)
                                            ws.Cells[rowIndexCurrentRecord, 9].Value = totalOutPkgAmount;
                                        else
                                        {
                                            ws.Cells[rowIndexCurrentRecord, 9].Value = "-";
                                            ws.Cells[rowIndexCurrentRecord, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                        }
                                        ws.Cells[rowIndexCurrentRecord, 9].Style.Numberformat.Format = "#,##0";
                                        for (int i = 5; i <= 10; i++)
                                        {
                                            ws.Cells[rowIndexCurrentRecord, i].Style.Font.Bold = true;
                                            ws.Cells[rowIndexCurrentRecord, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                        }
                                        rowIndexCurrentRecord++;
                                        #endregion .row sum group
                                    }
                                }
                            }
                        }

                        #region Row total
                        if (iGroup > 0)
                        {
                            ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 5].Merge = true;
                            ws.Cells[rowIndexCurrentRecord, 1].Style.Font.Bold = true;
                            ws.Cells[rowIndexCurrentRecord, 1, rowIndexCurrentRecord, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            string strTotal = string.Empty;
                            for (int i = 1; i <= iGroup; i++)
                            {
                                if (i == 1)
                                    strTotal += $"({i})";
                                else
                                    strTotal += $"+ ({i})";
                            }
                            ws.Cells[rowIndexCurrentRecord, 1].Value = $"({iGroup + 1})   =    {strTotal} Tổng thanh toán/(Total Amount)";
                            if (_totalpkgQty > 0)
                                ws.Cells[rowIndexCurrentRecord, 6].Value = _totalpkgQty;
                            else
                            {
                                ws.Cells[rowIndexCurrentRecord, 6].Value = "-";
                                ws.Cells[rowIndexCurrentRecord, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            ws.Cells[rowIndexCurrentRecord, 6].Style.Numberformat.Format = "#,##0";
                            if (_totalpkgAmount > 0)
                                ws.Cells[rowIndexCurrentRecord, 7].Value = _totalpkgAmount;
                            else
                            {
                                ws.Cells[rowIndexCurrentRecord, 7].Value = "-";
                                ws.Cells[rowIndexCurrentRecord, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            ws.Cells[rowIndexCurrentRecord, 7].Style.Numberformat.Format = "#,##0";
                            if (_totalOutPkgQty > 0)
                                ws.Cells[rowIndexCurrentRecord, 8].Value = _totalOutPkgQty;
                            else
                            {
                                ws.Cells[rowIndexCurrentRecord, 8].Value = "-";
                                ws.Cells[rowIndexCurrentRecord, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            ws.Cells[rowIndexCurrentRecord, 8].Style.Numberformat.Format = "#,##0";
                            if (_totalOutPkgAmount > 0)
                                ws.Cells[rowIndexCurrentRecord, 9].Value = _totalOutPkgAmount;
                            else
                            {
                                ws.Cells[rowIndexCurrentRecord, 9].Value = "-";
                                ws.Cells[rowIndexCurrentRecord, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            }
                            ws.Cells[rowIndexCurrentRecord, 9].Style.Numberformat.Format = "#,##0";
                            for (int i = 5; i <= 10; i++)
                            {
                                ws.Cells[rowIndexCurrentRecord, i].Style.Font.Bold = true;
                                ws.Cells[rowIndexCurrentRecord, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            }
                            rowIndexCurrentRecord++;
                        }
                        #endregion
                        #region Build Footer info
                        if (iGroup > 0)
                        {
                            rowIndexCurrentRecord++;
                            ws.Cells[rowIndexCurrentRecord, 2, rowIndexCurrentRecord, 3].Merge = true;
                            ws.Cells[rowIndexCurrentRecord, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndexCurrentRecord, 2].Style.Font.Bold = true;
                            ws.Cells[rowIndexCurrentRecord, 2].Value = "Khách hàng (Payor)";

                            ws.Cells[rowIndexCurrentRecord, 8, rowIndexCurrentRecord, 10].Merge = true;
                            ws.Cells[rowIndexCurrentRecord, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndexCurrentRecord, 8].Style.Font.Bold = true;
                            ws.Cells[rowIndexCurrentRecord, 8].Value = "Nhân viên thu ngân (Cashier)";

                            rowIndexCurrentRecord++;
                            ws.Cells[rowIndexCurrentRecord, 2, rowIndexCurrentRecord, 3].Merge = true;
                            ws.Cells[rowIndexCurrentRecord, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndexCurrentRecord, 2].Style.Font.Size = 9;
                            ws.Cells[rowIndexCurrentRecord, 2].Value = "Ký, ghi rõ họ tên";

                            ws.Cells[rowIndexCurrentRecord, 8, rowIndexCurrentRecord, 10].Merge = true;
                            ws.Cells[rowIndexCurrentRecord, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndexCurrentRecord, 8].Style.Font.Size = 9;
                            ws.Cells[rowIndexCurrentRecord, 8].Value = "Ký, ghi rõ họ tên";

                            rowIndexCurrentRecord++;
                            ws.Cells[rowIndexCurrentRecord, 2, rowIndexCurrentRecord, 3].Merge = true;
                            ws.Cells[rowIndexCurrentRecord, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndexCurrentRecord, 2].Style.Font.Size = 9;
                            ws.Cells[rowIndexCurrentRecord, 2].Value = "(Sign with full name)";

                            ws.Cells[rowIndexCurrentRecord, 8, rowIndexCurrentRecord, 10].Merge = true;
                            ws.Cells[rowIndexCurrentRecord, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[rowIndexCurrentRecord, 8].Style.Font.Size = 9;
                            ws.Cells[rowIndexCurrentRecord, 8].Value = "(Sign with full name)";

                        }
                        #endregion .Build Footer info
                    }

                    officePackage.Workbook.Properties.Title = "PMS | RECEIPT SERVICE USING";
                    officePackage.Workbook.Properties.Author = string.Join(Environment.NewLine, "info@vinmec.com", " | VINMEC INTERNATIONAL HOSPITAL");
                    officePackage.Workbook.Properties.Company = "VinMec International Hospital";
                    #region Stream file
                    var fileStream = new MemoryStream();
                    officePackage.SaveAs(fileStream);
                    fileStream.Position = 0;
                    var fsr = new System.Web.Mvc.FileStreamResult(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    fsr.FileDownloadName = fileName + ".xlsx";
                    return fsr;
                    #endregion
                }
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Error(string.Format("Error when export Report PatientInPackageStatChargeViaVisit_ExportExcel. ExDetail: {0}", ex));
                return null;
            }
        }
        #endregion
        #endregion .Function 4 Helper
    }
}