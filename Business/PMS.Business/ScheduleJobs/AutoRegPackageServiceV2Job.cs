using DataAccess.Models;
using DataAccess.Repository;
using PMS.Business.Connection;
using PMS.Business.Provider;
using PMS.Contract.Models.AdminModels;
using PMS.Contract.Models.ApigwModels;
using PMS.Contract.Models.Enum;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VM.Common;
using VM.Data.Queue;

namespace PMS.Business.ScheduleJobs
{
    public class AutoRegPackageServiceV2Job : IJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        protected PackageRepo _repoPkg = new PackageRepo();
        protected PatientInPackageRepo _repo = new PatientInPackageRepo();
        public void Execute(IJobExecutionContext context)
        {
            if (Globals.IsAutoCalculatePriceProcessing)
                return;
            Globals.IsAutoCalculatePriceProcessing = true;
            CustomLog.intervaljoblog.Info($"<Auto Reg package service for patient> Start!");
            try
            {
                List<string> listPID = new List<string>() { "200218661", "200293471", "200918520", "200723448", "200910470" };
                var results = unitOfWork.Temp_PatientInPackageRepository.AsEnumerable().Where(x=> x.StatusForProcess==ConfigHelper.CF_StatusForProcess && listPID.Contains(x.PID) && string.IsNullOrEmpty(x.Notes));
                CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Total item: {0}", results?.Count()));
                foreach (var item in results)
                {
                    if (string.IsNullOrEmpty(item.PID) || string.IsNullOrEmpty(item.PackageCode))
                    {
                        CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> PID or PackageCode is null"));
                        item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "PID hoăc PackageCode null") : "PID hoăc PackageCode null";
                        item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                        unitOfWork.Temp_PatientInPackageRepository.Update(item);
                        continue;
                    }
                    if (item.NetAmount==null)
                    {
                        CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] NetAmount is null", item.PackageCode, item.PID));
                        item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Giá chiết khấu/sau giảm giá null") : "Giá chiết khấu/sau giảm giá null";
                        item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                        unitOfWork.Temp_PatientInPackageRepository.Update(item);
                        //unitOfWork.Commit();
                        continue;
                    }
                    //Check để bỏ qua các gói có Notes
                    var temPackage = unitOfWork.Temp_PackageRepository.FirstOrDefault(x => x.PackgeCode == item.PackageCode);
                    if (temPackage == null)
                    {
                        CustomLog.intervaljoblog.Info(string.Format("<Auto MigrateCalculatePrice job> Package[Code={0}] is not found in Temp_Package", item.PackageCode));
                        item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "not found in Temp_Package") : "not found in Temp_Package";
                        item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                        unitOfWork.Temp_PatientInPackageRepository.Update(item);
                        continue;
                    }
                    if (!string.IsNullOrEmpty(temPackage.Notes))
                    {
                        CustomLog.intervaljoblog.Info(string.Format("<Auto MigrateCalculatePrice job> Package[Code={0}] is have note", item.PackageCode));
                        item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, temPackage.Notes) : temPackage.Notes;
                        item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                        unitOfWork.Temp_PatientInPackageRepository.Update(item);
                        continue;
                    }
                    //CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Begin reg package [{0}] for patient [{1}]", item.PackageCode, item.PID));
                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Reg package service for patient> Begin reg package [{0}] for patient [{1}]", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                    try {
                        int returnValue = (int)StatusEnum.SUCCESS;
                        PatientInPackage overlapPiPkg = null;
                        var patientModel = _repo.SyncPatient(item.PID);
                        if (patientModel != null)
                        {
                            #region Set PatientInPackage  model
                            PatientInPackageModel request = new PatientInPackageModel();
                            request.PatientModel = patientModel;
                            #region Set Master data
                            request.PersonalType = patientModel.National == "VNM" ? 1 : 2;
                            request.SiteCode = item.SiteRegCode;
                            //Get site
                            var site = unitOfWork.SiteRepository.Find(x => x.Code == request.SiteCode).FirstOrDefault();
                            if (site == null)
                            {
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] Not Found Site", item.PackageCode, item.PID));
                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Not Found Site") : "Not Found Site";
                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                                //unitOfWork.Commit();
                                continue;
                            }
                            request.SiteId = site.Id;
                            //Get Package info
                            var pkgEntity = unitOfWork.PackageRepository.Find(x => x.Code == item.PackageCode).FirstOrDefault();
                            if (pkgEntity == null)
                            {
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] Not Found Package", item.PackageCode, item.PID));
                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Not Found Package") : "Not Found Package";
                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                                //unitOfWork.Commit();
                                continue;
                            }
                            //Get PolicyId
                            var xPolicy = _repoPkg.PricePolicyAvailable(pkgEntity.Id, request.SiteCode, request.PersonalType, string.Empty,isForMigrate:true, NetAmountFilter:item.NetAmount);
                            if (!xPolicy.Any())
                            {
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] Not Found Policy mapping", item.PackageCode, item.PID));
                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Not Found Policy mapping") : "Not Found Policy mapping";
                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                                //unitOfWork.Commit();
                                continue;
                            }
                            var policy= xPolicy.FirstOrDefault();
                            request.PolicyId = policy?.Id;
                            #region Set start date & end date
                            DateTime dStartDate = item.StartAt;
                            DateTime endate = item.EndAt;
                            request.StartAt = dStartDate.ToString(Constant.DATE_FORMAT);
                            request.EndAt= endate.ToString(Constant.DATE_FORMAT);
                            //Check nguyên giá nhỏ hơn giá sau giảm giá/chiết khâu
                            if (policy?.Amount < item.NetAmount)
                            {
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] Amount less than NetAmount", item.PackageCode, item.PID));
                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Nguyên giá nhỏ hơn giá chiết khấu") : "Nguyên giá nhỏ hơn giá chiết khấu";
                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                                //unitOfWork.Commit();
                                continue;
                            }
                            #endregion .Set start date & end date
                            //request.IsMaternityPackage = false;
                            if (policy?.Amount > item.NetAmount)
                            {
                                request.IsDiscount = true;
                                request.DiscountType = 2;
                                request.DiscountAmount = policy?.Amount - item.NetAmount;
                                request.DiscountNote = "Auto note when migrate Concerto to PMS";
                            }
                            else
                            {
                                request.IsDiscount = false;
                                request.DiscountAmount = 0;
                            }
                            #endregion .Set Master data
                            #region Get and set Services data
                            double netAmount = 0;
                            int outStatusValue = 1;
                            var entities = _repo.GetListPatientInPackageService(request.PolicyId.Value, item.NetAmount.ToString(), out netAmount, out outStatusValue);
                            if (outStatusValue == -2)
                            {
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] NetAmount is smaller than drugs and consum total amount", item.PackageCode, item.PID));
                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "NetAmount is smaller than drugs and consum total amount") : "NetAmount is smaller than drugs and consum total amount";
                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                                continue;
                            }
                            if (entities?.Count <= 0)
                            {
                                //Chi tiết dịch vụ trong gói chưa được thiết lập giá
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] detail service have not settup price in package", item.PackageCode, item.PID));
                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Chi tiết dịch vụ trong gói chưa được thiết lập giá") : "Chi tiết dịch vụ trong gói chưa được thiết lập giá";
                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                                continue;
                            }
                            request.NetAmount = item.NetAmount.Value;
                            request.Services = entities;
                            #endregion .Get and set Services data
                            #endregion .Set PatientInPackage  model
                            //Create or Update Registration package
                            var statusPatientInPackage = CreateOrUpdatePatientInPackage(request,true, out overlapPiPkg);
                            if (statusPatientInPackage == (int)StatusEnum.SUCCESS)
                            {
                                //Cập nhật thông tin chi tiết dịch vụ trong gói của khách hàng
                                var statusService = CreateOrUpdatePatientInPackageDetail(request);
                                if (!statusService)
                                {
                                    returnValue = (int)StatusEnum.UNSUCCESS;
                                }
                            }
                            else
                            {
                                returnValue = statusPatientInPackage;
                            }
                            if(returnValue== (int)StatusEnum.SUCCESS)
                            {
                                //CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] Success", item.PackageCode, item.PID));
                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] Success", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_SUCCESS"];
                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                            }
                            else
                            {
                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}, status: {2}", item.Notes, "UNSUCCESS", returnValue) : string.Format("UNSUCCESS, status: {0}", returnValue);
                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                            }
                        }
                        else
                        {
                            CustomLog.intervaljoblog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] Not Found Patient", item.PackageCode, item.PID));
                            item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Not Found Policy") : "Not Found Patient";
                            item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                            unitOfWork.Temp_PatientInPackageRepository.Update(item);
                        }
                        Thread.Sleep(200);
                    }catch(Exception ex)
                    {
                        CustomLog.errorlog.Info(string.Format("<Auto Reg package service for patient> Reg package [{0}] for patient [{1}] Ex: {2}", item.PackageCode, item.PID, ex));
                        item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Exception") : "Exception";
                        item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                        unitOfWork.Temp_PatientInPackageRepository.Update(item);
                        //CustomLog.errorlog.Info(string.Format("<Auto Reg package service for patient> Reg patient into package: {0}", ex));
                    }
                }
                unitOfWork.Commit();
                //CustomLog.intervaljoblog.Info($"<Auto Reg package service for patient> Success!");
                CustomLog.Instant.IntervalJobLog($"<Auto Reg package service for patient> Success!", Constant.Log_Type_Info, printConsole: true);
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Auto Reg package service for patient> Error: {0}", ex));
            }
            Globals.IsAutoCalculatePriceProcessing = false;
        }
        private int CreateOrUpdatePatientInPackage(PatientInPackageModel request,/*bool isMigrateFromEhos*/bool isMigrateFromConcerto, out PatientInPackage outOvelapPiPkg)
        {
            int returnValue = (int)StatusEnum.SUCCESS;
            //bool returnValue = true;
            try
            {
                if (request != null && request.PolicyId != null && request.SiteId != null)
                {
                    var pkgPriceSite = unitOfWork.PackagePriceSiteRepository.FirstOrDefault(x => x.SiteId == request.SiteId && x.PackagePriceId == request.PolicyId);
                    if (pkgPriceSite == null)
                    {
                        //return false;
                        returnValue = (int)StatusEnum.NOT_FOUND;
                    }
                    #region Check Have exist reg the same time
                    var pkgEntity = unitOfWork.PackageRepository.AsEnumerable().Where(x => x.PackagePrices.Any(y => y.Id == request.PolicyId)).FirstOrDefault();
                    if (pkgEntity == null)
                    {
                        //Gói khám không tồn tại
                        outOvelapPiPkg = null;
                        return (int)StatusEnum.NOT_FOUND;
                    }
                    PatientInPackage entityPiPkg = null;
                    //if (_repo.CheckDupplicateRegistered(request.PatientModel.Id.Value, pkgEntity.Id, request.GetStartAt(), request.GetEndAt(), out entityPiPkg))
                    //{
                    //    //Overlap
                    //    outOvelapPiPkg = entityPiPkg;
                    //    return (int)StatusEnum.CONFLICT;
                    //}
                    if (request.GetContractDate() != null)
                    {
                        /*Thời gian bắt đầu sử dụng luôn luôn bằng hoặc sau Thời gian hợp đồng.*/
                        if (request.GetStartAt().Date < request.GetContractDate().Value.Date)
                        {
                            //Thời gian bắt đầu sử dụng trước ngày hợp đồng
                            outOvelapPiPkg = null;
                            return (int)StatusEnum.PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER;
                        }
                        /*Thời gian hợp đồng luôn bằng hoặc sau thời gian acitve giá tại site đang đăng ký gói*/
                        if (pkgPriceSite.CreatedAt.Value.Date > request.GetContractDate().Value.Date)
                        {
                            //Ngày hợp đồng sớm hơn ngày active giá tại site
                            outOvelapPiPkg = null;
                            return (int)StatusEnum.PATIENT_INPACKAGE_CONTRACTDATE_PRICE_SITE_CREATEDATE_INVALID_EARLIER;
                        }
                    }
                    #endregion
                    PatientInPackage entity = null;
                    #region Comment Code update (Modify information)
                    //entity = unitOfWork.PatientInPackageRepository.AsEnumerable().Where(x => x.PatientInforId == request.PatientModel.Id && x.PackagePriceSiteId == pkgPriceSite.Id
                    //&& ((x.StartAt <= request.GetStartAt() && x.EndAt >= request.GetStartAt()) || (x.StartAt <= request.GetEndAt() && x.EndAt >= request.GetEndAt())
                    //|| (x.StartAt >= request.GetStartAt() && x.EndAt <= request.GetEndAt()))
                    //).FirstOrDefault();
                    //if (entity != null)
                    //{
                    //    #region BUZ Edit
                    //    //Update (Tạm thời đóng nghiệp vụ hỗ trợ update)
                    //    #region Contract Info
                    //    entity.ContractNo = request.ContractNo;
                    //    entity.ContractDate = request.GetContractDate();
                    //    entity.ContractOwner = request.ContractOwnerAd;
                    //    entity.ContractOwnerFullName = request.ContractOwnerFullName;
                    //    #endregion .Contract Info
                    //    #region Doctor Consult Info
                    //    entity.DoctorConsult = request.DoctorConsultAd;
                    //    entity.DoctorConsultFullName = request.DoctorConsultFullName;
                    //    entity.DepartmentId = request.DepartmentId;
                    //    #endregion .Doctor Consult Info
                    //    entity.StartAt = request.GetStartAt();
                    //    entity.EndAt = request.GetEndAt();
                    //    entity.IsMaternityPackage = request.IsMaternityPackage;
                    //    entity.EstimateBornDate = request.GetEstimateBornDate();
                    //    #region Discount Info
                    //    entity.IsDiscount = request.IsDiscount;
                    //    entity.DiscountType = request.DiscountType;
                    //    entity.DiscountAmount = request.DiscountAmount;
                    //    entity.DiscountNote = request.DiscountNote;
                    //    #endregion .Discount Info
                    //    entity.NetAmount = request.NetAmount > 0 ? request.NetAmount : pkgPriceSite.PackagePrice.Amount.Value;
                    //    if (DateTime.Now >= entity.StartAt)
                    //        entity.Status = (int)PatientInPackageEnum.ACTIVATED;
                    //    else
                    //        entity.Status = (int)PatientInPackageEnum.REGISTERED;
                    //    entity.IsFromeHos = isMigrateFromEhos;
                    //    unitOfWork.PatientInPackageRepository.Update(entity);
                    //    #endregion .BUZ Edit
                    //}
                    //else
                    #endregion .Comment Code update (Modify information)
                    {
                        //Thêm mới
                        entity = new PatientInPackage();
                        entity.PackagePriceSiteId = pkgPriceSite.Id;
                        entity.PatientInforId = request.PatientModel.Id.Value;
                        #region Contract Info
                        entity.ContractNo = request.ContractNo;
                        entity.ContractDate = request.GetContractDate();
                        entity.ContractOwner = request.ContractOwnerAd;
                        entity.ContractOwnerFullName = request.ContractOwnerFullName;
                        #endregion .Contract Info
                        #region Doctor Consult Info
                        entity.DoctorConsult = request.DoctorConsultAd;
                        entity.DoctorConsultFullName = request.DoctorConsultFullName;
                        entity.DepartmentId = request.DepartmentId;
                        #endregion .Doctor Consult Info
                        entity.StartAt = request.GetStartAt();
                        entity.EndAt = request.GetEndAt();
                        entity.IsMaternityPackage = request.IsMaternityPackage;
                        entity.EstimateBornDate = request.GetEstimateBornDate();
                        #region Discount Info
                        entity.IsDiscount = request.IsDiscount;
                        entity.DiscountType = request.DiscountType;
                        entity.DiscountAmount = request.DiscountAmount;
                        entity.DiscountNote = request.DiscountNote;
                        #endregion .Discount Info
                        entity.NetAmount = request.NetAmount >= 0 ? request.NetAmount : pkgPriceSite.PackagePrice.Amount.Value;
                        if (Constant.CurrentDate >= entity.StartAt /*&& entity.Status== (int)PatientInPackageEnum.REGISTERED*/)
                            entity.Status = (int)PatientInPackageEnum.ACTIVATED;
                        else
                        {
                            entity.Status = (int)PatientInPackageEnum.REGISTERED;
                        }
                        //entity.IsFromeHos = isMigrateFromEhos;
                        entity.Concerto = isMigrateFromConcerto;
                        unitOfWork.PatientInPackageRepository.Add(entity);
                    }
                    #region Rebuild model return
                    request.Id = entity.Id;
                    request.ContractNo = entity.ContractNo;
                    request.ContractDate = entity.ContractDate?.ToString(Constant.DATE_FORMAT);
                    request.ContractOwnerAd = entity.ContractOwner;
                    request.ContractOwnerFullName = entity.ContractOwnerFullName;
                    request.DoctorConsultAd = entity.DoctorConsult;
                    request.DoctorConsultFullName = entity.DoctorConsultFullName;
                    request.DepartmentId = entity.DepartmentId;
                    request.StartAt = entity.StartAt.ToString(Constant.DATE_FORMAT);
                    request.EndAt = entity.EndAt?.ToString(Constant.DATE_FORMAT);
                    //request.IsMaternityPackage = entity.IsMaternityPackage;
                    request.EstimateBornDate = entity.EstimateBornDate?.ToString(Constant.DATE_FORMAT);
                    #region Discount Info
                    request.IsDiscount = entity.IsDiscount;
                    request.DiscountType = entity.DiscountType;
                    request.DiscountAmount = entity.DiscountAmount;
                    request.DiscountNote = entity.DiscountNote;
                    #endregion .Discount Info
                    request.NetAmount = entity.NetAmount;
                    request.Status = entity.Status;
                    #endregion .Rebuild model return
                }
                else
                {
                    //returnValue = false;
                    outOvelapPiPkg = null;
                    returnValue = (int)StatusEnum.NOT_FOUND;
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpdatePatientInPackage fail. Ex: {0}", ex));
                //returnValue = false;
                returnValue = (int)StatusEnum.INTERNAL_SERVER_ERROR;
            }
            outOvelapPiPkg = null;
            return returnValue;
        }
        private bool CreateOrUpdatePatientInPackageDetail(PatientInPackageModel request)
        {
            bool returnValue = true;
            try
            {
                if (request != null && request.Services != null)
                {
                    foreach (var item in request.Services)
                    {
                        PatientInPackageDetail entity = unitOfWork.PatientInPackageDetailRepository.FirstOrDefault(x => x.ServiceInPackageId == item.ServiceInPackageId && x.PatientInPackageId == request.Id);
                        if (entity != null)
                        {
                            //Update
                            entity.QtyWasUsed = item.QtyWasUsed;
                            entity.PkgPrice = item.PkgPrice;
                            entity.PkgAmount = item.PkgAmount;
                            unitOfWork.PatientInPackageDetailRepository.Update(entity);
                        }
                        else
                        {
                            //Thêm mới
                            entity = new PatientInPackageDetail();
                            entity.PatientInPackageId = request.Id;
                            entity.ServiceInPackageId = item.ServiceInPackageId;
                            entity.QtyWasUsed = item.QtyWasUsed;
                            entity.QtyRemain = item.Qty;
                            entity.PkgPrice = item.PkgPrice;
                            entity.PkgAmount = item.PkgAmount;
                            unitOfWork.PatientInPackageDetailRepository.Add(entity);
                        }
                        #region Thêm/Cập nhật dịch vụ thay thế
                        var xQueryReplace = unitOfWork.ServiceInPackageRepository.Find(x => x.RootId == entity.ServiceInPackageId && !x.IsDeleted);
                        if (xQueryReplace.Any())
                        {
                            item.ModelReplaces = new List<PatientInPackageDetailModel>();
                            foreach (var itemX in xQueryReplace)
                            {
                                PatientInPackageDetail entityReplace = unitOfWork.PatientInPackageDetailRepository.FirstOrDefault(x => x.ServiceInPackageId == itemX.Id && x.PatientInPackageId == request.Id);
                                if (entityReplace != null)
                                {
                                    //Update
                                    entityReplace.QtyWasUsed = item.QtyWasUsed;
                                    entityReplace.PkgPrice = item.PkgPrice;
                                    entityReplace.PkgAmount = item.PkgAmount;
                                    unitOfWork.PatientInPackageDetailRepository.Update(entityReplace);
                                }
                                else
                                {
                                    //Thêm mới
                                    entityReplace = new PatientInPackageDetail();
                                    entityReplace.PatientInPackageId = request.Id;
                                    entityReplace.ServiceInPackageId = itemX.Id;
                                    entityReplace.QtyWasUsed = item.QtyWasUsed;
                                    entityReplace.QtyRemain = item.Qty;
                                    entityReplace.PkgPrice = item.PkgPrice;
                                    entityReplace.PkgAmount = item.PkgAmount;
                                    unitOfWork.PatientInPackageDetailRepository.Add(entityReplace);
                                }
                                item.ModelReplaces.Add(new PatientInPackageDetailModel()
                                {
                                    Id = entityReplace.Id,
                                    ServiceInPackageId = entityReplace.ServiceInPackageId
                                });
                            }
                        }
                        item.Id = entity.Id;
                        #endregion .Thêm/Cập nhật dịch vụ thay thế
                    }
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpdatePatientInPackageDetail fail. Ex: {0}", ex));
                returnValue = false;
            }
            return returnValue;
        }
    }
}
