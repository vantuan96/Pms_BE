using DataAccess.Models;
using DataAccess.Repository;
using Newtonsoft.Json;
using PMS.Business.Connection;
using PMS.Business.Helper;
using PMS.Business.MongoDB;
using PMS.Contract.Models;
using PMS.Contract.Models.AdminModels;
using PMS.Contract.Models.ApigwModels;
using PMS.Contract.Models.Enum;
using PMS.Contract.Models.MasterData;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using VM.Common;

namespace PMS.Business.Provider
{
    public class PatientInPackageRepo : IDisposable
    {
        private IUnitOfWork unitOfWork = new EfUnitOfWork();
        List<int> listStatusIGNOREUpdateStatUsing = new List<int>() { /*(int)PatientInPackageEnum.CLOSED, */(int)PatientInPackageEnum.CANCELLED, (int)PatientInPackageEnum.TERMINATED, (int)PatientInPackageEnum.TRANSFERRED };
        List<int> listStatusAllowUpdateOrginalPriceWhenPricingClass = new List<int>() { (int)PatientInPackageEnum.ACTIVATED, (int)PatientInPackageEnum.EXPIRED };
        List<int> listInPackageTypeAllowUpdateOrginalPriceWhenPricingClass = new List<int>() { (int)InPackageType.INPACKAGE, (int)InPackageType.OVERPACKAGE };
        //linhht trạng thái gói check cho theo dõi tái khám
        List<int> listStatusCheckForReExaminate = new List<int>() { (int)PatientInPackageEnum.EXPIRED, (int)PatientInPackageEnum.ACTIVATED, (int)PatientInPackageEnum.RE_EXAMINATE };
        public void Dispose()
        {
            unitOfWork.Dispose();
        }
        #region Patient In Package General
        public int RegisterPackage(PatientInPackageModel request, out PatientInPackage outOverlapPiPkg)
        {
            int returnValue = (int)StatusEnum.SUCCESS;
            PatientInPackage overlapPiPkg = null;
            try
            {
                var patientModel = CreateOrUpdatePatient(request);
                if (patientModel != null)
                {
                    request.PatientModel = patientModel;
                    //Create or Update Registration package
                    var statusPatientInPackage = CreateOrUpdatePatientInPackage(request, out overlapPiPkg);
                    if (statusPatientInPackage == (int)StatusEnum.SUCCESS)
                    {
                        //Cập nhật thông tin chi tiết dịch vụ trong gói của khách hàng
                        var statusService = CreateOrUpdatePatientInPackageDetail(request);
                        if (statusService)
                        {
                            unitOfWork.Commit();
                            //Lưu log action khi thực hiện thành công
                            #region store log action
                            LogRepo.AddLogAction(request.Id, "PatientInPackages", (int)ActionEnum.REGISTERED, string.Empty);
                            #endregion store log action
                        }
                        else
                        {
                            returnValue = (int)StatusEnum.UNSUCCESS;
                        }
                    }
                    else
                    {
                        returnValue = statusPatientInPackage;
                    }
                }
                else
                {
                    returnValue = (int)StatusEnum.NOT_FOUND;
                }
            }
            catch (Exception ex)
            {
                returnValue = (int)StatusEnum.INTERNAL_SERVER_ERROR;
                VM.Common.CustomLog.accesslog.Error(string.Format("RegisterPackage fail. Ex: {0}", ex));
            }
            outOverlapPiPkg = overlapPiPkg;
            return returnValue;
        }
        public int TransferredPackage(PatientInPackageTransferredModel request, out string msg, out PatientInPackage outOverlapPiPkg)
        {
            int returnValue = (int)StatusEnum.SUCCESS;
            PatientInPackage overLapPiPkg = null;
            string outMsg = string.Empty;
            try
            {
                var patientModel = SyncPatient(request.PatientModel.PID);
                if (patientModel != null)
                {
                    request.PatientModel = patientModel;
                    //Create or Update Registration package
                    var statusPatientInPackage = CreateOrUpdatePatientInPackage(request, out overLapPiPkg);
                    if (statusPatientInPackage == (int)StatusEnum.SUCCESS)
                    {
                        #region Cập nhật thông tin gói 1 (Gói cũ)
                        var ptInpackageOld = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == request.OldPatientInPackageId);
                        if (ptInpackageOld != null)
                        {
                            ptInpackageOld.NewPatientInPackageId = request.Id;
                            //Capture statistic data using current package
                            #region Capture statistic data using current package
                            var UsingEntities = GetListPatientInPackageServiceUsing(ptInpackageOld.Id);
                            ptInpackageOld.DataStatUsing = JsonConvert.SerializeObject(UsingEntities);
                            #endregion .Capture statistic data using current package
                            //linhht
                            ptInpackageOld.LastStatus = ptInpackageOld.Status;
                            ptInpackageOld.Status = (int)PatientInPackageEnum.TRANSFERRED;
                            ptInpackageOld.TransferredAt = DateTime.Now;
                            unitOfWork.PatientInPackageRepository.Update(ptInpackageOld);
                        }
                        #endregion .Cập nhật thông tin gói 1 (Gói cũ)
                        //Cập nhật thông tin chi tiết dịch vụ trong gói của khách hàng
                        var statusService = CreateOrUpdatePatientInPackageDetail(request);
                        if (statusService)
                        {
                            #region Confirm and update price for charge belong to package
                            //Update PatientInPackageDetailId
                            #region Update PatientInPackageDetailId
                            if (request.listCharge?.Count > 0 && request.Services?.Count > 0)
                            {
                                foreach (var item in request.listCharge)
                                {
                                    item.PatientInPackageDetailId = request.Services.Where(y => y.Service.Code == item.ServiceCode).Select(y => y.Id).FirstOrDefault();
                                    if (item.PatientInPackageDetailId == null)
                                    {
                                        item.PatientInPackageDetailId = request.Services.Where(y => y.ModelReplaces?.Count > 0 && y.ModelReplaces.Any(z => z.ServiceInPackageId == item.ServiceInpackageId))?.Select(y => y.ModelReplaces.Select(y1 => y1.Id)?.FirstOrDefault())?.FirstOrDefault();
                                    }
                                    if (request.Id.HasValue)
                                        item.PatientInPackageId = request.Id.Value;
                                }
                            }
                            #endregion .Update PatientInPackageDetailId
                            var modelConfirmCharge = new ConfirmServiceInPackageModel()
                            {
                                SessionProcessId = request.SessionProcessId,
                                PID = request.PatientModel.PID,
                                PatientInPackageId = request.Id.Value,
                                listCharge = request.listCharge,
                                Children = request.Children
                            };
                            //Release/Giải phóng các chỉ định khỏi gói dịch vụ cũ
                            #region Release charge
                            ReleaseChargeFromPatientInPackage(ptInpackageOld.Id, unitOfWork);
                            #endregion .Release charge 
                            if (request.listCharge == null)
                            {
                                unitOfWork.Commit();
                                msg = string.Empty;
                                outOverlapPiPkg = overLapPiPkg;
                                //Lưu log action khi thực hiện thành công
                                #region store log action
                                LogRepo.AddLogAction(ptInpackageOld.Id, "PatientInPackages", (int)ActionEnum.TRANSFERRED, string.Format("Tranferred from {0} to {1}", request.OldPackageCode, request.PackageCode));
                                #endregion store log action
                                return returnValue;
                            }
                            var returnValueCf = ConfirmChargeBelongPackage(modelConfirmCharge, IsCommit: false, out outMsg, IsTranferredPackage: true, OldPatientInPackageId: request.OldPatientInPackageId);
                            #endregion .Confirm and update price for charge belong to package
                            #region Move thông tin con từ gói cũ sang
                            bool statusMoveChild = true;
                            if (request.IsIncludeChild && request.Children?.Count > 0)
                            {
                                statusMoveChild = CreateOrUpdateChild(request.Id.Value, request.Children, IsCommit: false);
                            }
                            #endregion
                            if (returnValueCf && statusMoveChild && Constant.StatusUpdatePriceOKs.Contains(outMsg))
                            {
                                unitOfWork.Commit();
                                //Lưu log action khi thực hiện thành công
                                #region store log action
                                LogRepo.AddLogAction(ptInpackageOld.Id, "PatientInPackages", (int)ActionEnum.TRANSFERRED, string.Format("Tranferred from {0} to {1}", request.OldPackageCode, request.PackageCode));
                                #endregion store log action
                            }
                            else
                            {
                                returnValue = (int)StatusEnum.UNSUCCESS;
                            }
                        }
                        else
                        {
                            returnValue = (int)StatusEnum.UNSUCCESS;
                        }
                    }
                    else
                    {
                        returnValue = statusPatientInPackage;
                    }
                }
                else
                {
                    returnValue = (int)StatusEnum.NOT_FOUND;
                }
            }
            catch (Exception ex)
            {
                returnValue = (int)StatusEnum.INTERNAL_SERVER_ERROR;
                VM.Common.CustomLog.accesslog.Error(string.Format("RegisterPackage fail. Ex: {0}", ex));
            }
            msg = outMsg;
            outOverlapPiPkg = overLapPiPkg;
            return returnValue;
        }
        /// <summary>
        /// Create or update Patient Information
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public PatientInformationModel CreateOrUpdatePatient(PatientInPackageModel request)
        {
            PatientInformationModel model = null;
            try
            {
                if (request != null && request.PatientModel != null)
                {
                    PatientInformation entity = null;
                    entity = unitOfWork.PatientInformationRepository.FirstOrDefault(x => /*x.PatientId == request.PatientModel.PatientId || */x.PID == request.PatientModel.PID);
                    if (entity != null)
                    {
                        //Update
                        entity.PID = request.PatientModel.PID;
                        entity.PatientId = request.PatientModel.PatientId;
                        entity.FullName = request.PatientModel.FullName;
                        entity.DateOfBirth = request.PatientModel.DateOfBirth;
                        entity.Email = request.PatientModel.Email;
                        entity.Gender = request.PatientModel.Gender;
                        entity.Mobile = request.PatientModel.Mobile;
                        entity.Address = request.PatientModel.Address;
                        entity.National = request.PatientModel.National;

                        unitOfWork.PatientInformationRepository.Update(entity);
                    }
                    else
                    {
                        //Thêm mới
                        entity = new PatientInformation();
                        entity.PID = request.PatientModel.PID;
                        entity.PatientId = request.PatientModel.PatientId;
                        entity.FullName = request.PatientModel.FullName;
                        entity.DateOfBirth = request.PatientModel.DateOfBirth;
                        entity.Email = request.PatientModel.Email;
                        entity.Gender = request.PatientModel.Gender;
                        entity.Mobile = request.PatientModel.Mobile;
                        entity.Address = request.PatientModel.Address;
                        entity.National = request.PatientModel.National;

                        unitOfWork.PatientInformationRepository.Add(entity);
                    }
                    model = new PatientInformationModel()
                    {
                        Id = entity.Id,
                        PatientId = entity.PatientId,
                        PID = entity.PID,
                        FullName = entity.FullName,
                        DateOfBirth = entity.DateOfBirth,
                        Gender = entity.Gender,
                        Email = entity.Email,
                        Mobile = entity.Mobile,
                        Address = entity.Address,
                        National = entity.National
                    };
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpdatePatient fail. Ex: {0}", ex));
            }
            return model;
        }
        /// <summary>
        /// Create or update Patient Information V2
        /// </summary>
        /// <param name="entityModel"></param>
        /// <returns></returns>
        public PatientInformationModel CreateOrUpdatePatient(PatientInformationModel entityModel)
        {
            PatientInformationModel model = null;
            try
            {
                if (entityModel != null && entityModel != null)
                {
                    PatientInformation entity = unitOfWork.PatientInformationRepository.FirstOrDefault(x => x.PatientId == entityModel.PatientId || x.PID == entityModel.PID);
                    if (entity != null)
                    {
                        //Update
                        entity.PID = entityModel.PID;
                        entity.PatientId = entityModel.PatientId;
                        entity.FullName = entityModel.FullName;
                        entity.DateOfBirth = entityModel.DateOfBirth;
                        entity.Email = entityModel.Email;
                        entity.Gender = entityModel.Gender;
                        entity.Mobile = entityModel.Mobile;
                        entity.Address = entityModel.Address;
                        entity.National = entityModel.National;

                        unitOfWork.PatientInformationRepository.Update(entity);
                    }
                    else
                    {
                        //Thêm mới
                        entity = new PatientInformation();
                        entity.PID = entityModel.PID;
                        entity.PatientId = entityModel.PatientId;
                        entity.FullName = entityModel.FullName;
                        entity.DateOfBirth = entityModel.DateOfBirth;
                        entity.Email = entityModel.Email;
                        entity.Gender = entityModel.Gender;
                        entity.Mobile = entityModel.Mobile;
                        entity.Address = entityModel.Address;
                        entity.National = entityModel.National;

                        unitOfWork.PatientInformationRepository.Add(entity);
                    }
                    unitOfWork.Commit();
                    model = new PatientInformationModel()
                    {
                        Id = entity.Id,
                        PatientId = entity.PatientId,
                        PID = entity.PID,
                        FullName = entity.FullName,
                        DateOfBirth = entity.DateOfBirth,
                        Gender = entity.Gender,
                        Email = entity.Email,
                        Mobile = entity.Mobile,
                        Address = entity.Address,
                        National = entity.National
                    };
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpdatePatient fail. Ex: {0}", ex));
            }
            return model;
        }
        public PatientInformationModel SyncPatient(string pId, List<PatientInformationModel> children = null)
        {
            PatientInformationModel returnModel = null;
            var patients = OHConnectionAPI.GetPatients(new PatientParameterModel() { Pid = pId });
            if (patients?.Count > 0)
            {
                returnModel = CreateOrUpdatePatient(patients[0]);
            }
            #region Sync child
            if (children?.Count > 0)
            {
                foreach (var item in children)
                {
                    SyncPatient(item.PID);
                }
            }
            #endregion .Sync child
            return returnModel;
        }
        /// <summary>
        /// Refresh update information of patient in package (Cập nhật thông tin, thông tin charge liên quan đến gói dịch vụ của Khách Hàng)
        /// </summary>
        /// <param name="patientinpackageid"></param>
        public PatientInPackage RefreshInformationPatientInPackage(Guid patientinpackageid)
        {
            #region For statistic performance
            var start_time = DateTime.Now;
            var start_time_total = DateTime.Now;
            TimeSpan tp;
            #endregion .For statistic performance

            PatientInPackage entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
            if (entity != null)
            {
                #region Update status
                if (Constant.CurrentDate >= entity.StartAt && entity.Status == (int)PatientInPackageEnum.REGISTERED)
                {
                    //linhht
                    entity.LastStatus = entity.Status;
                    entity.Status = (int)PatientInPackageEnum.ACTIVATED;
                    CustomLog.intervaljoblog.Info(string.Format("<Update Patient's Package Status> Info: [Code: {0}; Name: {1}; PatientInPackageId: {2}; Status: {3}]", entity.PackagePriceSite.PackagePrice.Package.Code, entity.PackagePriceSite.PackagePrice.Package.Name, entity.Id, entity.Status));
                }
                //tungdd14: thêm entity.Status == (int)PatientInPackageEnum.RE_EXAMINATE
                //cập nhật status với các gói tái khám
                else if (Constant.CurrentDate > entity.EndAt && (entity.Status == (int)PatientInPackageEnum.ACTIVATED || entity.Status == (int)PatientInPackageEnum.REGISTERED || entity.Status == (int)PatientInPackageEnum.RE_EXAMINATE))
                {
                    //linhht
                    entity.LastStatus = entity.Status;
                    entity.Status = (int)PatientInPackageEnum.EXPIRED;
                    CustomLog.intervaljoblog.Info(string.Format("<Update Patient's Package Status> Info: [Code: {0}; Name: {1}; PatientInPackageId: {2}; Status: {3}]", entity.PackagePriceSite.PackagePrice.Package.Code, entity.PackagePriceSite.PackagePrice.Package.Name, entity.Id, entity.Status));
                }
                unitOfWork.PatientInPackageRepository.Update(entity);
                entity.IsMaternityPackage = /*Constant*/HelperBusiness.Instant.ListGroupCodeIsMaternityPackage.Contains(entity.PackagePriceSite.PackagePrice.Package.PackageGroup.Code);
                entity.IsBundlePackage = /*Constant*/HelperBusiness.Instant.ListGroupCodeIsBundlePackage.Contains(entity.PackagePriceSite.PackagePrice.Package.PackageGroup.Code);
                #endregion .Update status
                if (!listStatusIGNOREUpdateStatUsing.Contains(entity.Status))
                {
                    #region Sync new information charge in package
                    start_time = DateTime.Now;
                    //Các gói có trạng thái là “Đang hoạt động”/ “Hết hạn”/ “Đã đóng”, hệ thống gói API sang OH để lấy thông tin mới nhất của các chỉ định trong gói, vượt gói, ngoài gói trong các lần sử dụng gói
                    SynChargeInformtionFromOH(entity);
                    //Commit after syn from OH
                    unitOfWork.Commit();
                    #region Log Performace
                    tp = DateTime.Now - start_time;
                    CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step processing spen time in {2} (ms)", patientinpackageid, "RefreshInformationPatientInPackage.SynChargeInformtionFromOH", tp.TotalMilliseconds));
                    #endregion .Log Performace
                    start_time = DateTime.Now;

                    #endregion .Sync new information charge in package
                    #region Update Statistic (QtyWasUsed in PatientInPackageDetails)
                    //Cập nhật lại thống kê
                    UpgradeStatictisUsingServiceInPackage(entity.Id);
                    //Commit after update statistic
                    unitOfWork.Commit();
                    #region Log Performace
                    tp = DateTime.Now - start_time;
                    CustomLog.performancejoblog.Info(string.Format("PackageDetail[Id={0}]: {1} step processing spen time in {2} (ms)", patientinpackageid, "RefreshInformationPatientInPackage.UpgradeStatictisUsingServiceInPackage", tp.TotalMilliseconds));
                    #endregion .Log Performace

                    #endregion .Update Statistic (QtyWasUsed in PatientInPackageDetails)
                }
                //unitOfWork.Commit();
            }
            return entity;
        }
        /// <summary>
        /// Đồng bộ và cập nhật thông tin chỉ định trong/vượt/ngoài gói (Coding)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool SynChargeInformtionFromOH(PatientInPackage model)
        {
            bool returnValue = true;
            //26-07-2022 tungdd14: fix bug gói đã đóng thì không update lại HisCharges
            //tungdd14 thêm model.Status == (int)PatientInPackageEnum.RE_EXAMINATE cập nhật cả trường hợp tái khám
            if (model.Status == (int)PatientInPackageEnum.ACTIVATED /*|| model.Status == (int)PatientInPackageEnum.CLOSED*/ || model.Status == (int)PatientInPackageEnum.EXPIRED || model.Status == (int)PatientInPackageEnum.RE_EXAMINATE
                    || (model.Status == (int)PatientInPackageEnum.ACTIVATED && model.EndAt < Constant.CurrentDate))
            {
                //Get information from OH
                var XhisCharges = unitOfWork.HISChargeRepository.Find(x => !x.IsDeleted && x.PatientInPackageId == model.Id);
                if (XhisCharges.Any())
                {
                    string arrCharges = string.Join(";", XhisCharges.Select(x => x.ChargeId)?.ToList());
                    if (!string.IsNullOrEmpty(arrCharges))
                    {
                        //CustomLog.intervaljoblog.Info(string.Format("<SynChargeInformtionFromOH> List charges inside PatientInPackages: {0}", arrCharges));
                        List<HISChargeModel> oHEntities = new List<HISChargeModel>();
                        GetAllChargeInpackge(model.PatientInformation.PID, arrCharges, oHEntities);
                        //Là gói Thai Sản || TBG/CNG (Is Include Child)
                        #region Get Charge what is Maternity/MCR package
                        bool IsIncludeChild = Constant.ListGroupCodeIsIncludeChildPackage.Contains(new PackageGroupRepo().GetPackageGroupRoot(model.PackagePriceSite.PackagePrice.Package.PackageGroup).Code);
                        //if (model.IsMaternityPackage==true)
                        if (IsIncludeChild)
                        {
                            List<PatientInformationModel> children = GetChildrenByPatientInPackageId(model.Id);
                            if (children?.Count > 0)
                            {
                                foreach (var itemChild in children)
                                {
                                    var arrChildCharges = string.Join(";", XhisCharges?.Where(e => e.PID == itemChild.PID).Select(x => x.ChargeId)?.ToList());
                                    if (!string.IsNullOrEmpty(arrChildCharges))
                                        oHEntities.AddRange(OHConnectionAPI.GetCharges(itemChild.PID, string.Empty, arrChildCharges));
                                }
                            }
                        }
                        #endregion .Get Charge what is Maternity/MCR package
                        //CustomLog.intervaljoblog.Info(string.Format("<SynChargeInformtionFromOH> List charges get from OH: {0}", JsonConvert.SerializeObject(oHEntities)));
                        if (oHEntities?.Count > 0)
                        {
                            //CreateOrUpdateHisChargeList
                            //Gán PatientInPackageId cho HisCharges
                            oHEntities?.ForEach(x => x.PatientInPackageId = model.Id);
                            CreateOrUpdateHisCharges(oHEntities, entityPiPkg: model);
                            foreach (var item in oHEntities)
                            {
                                //Update hischarge
                                //item.PatientInPackageId = model.Id;
                                #region Update hischarge. Comment lại do tốc độ chậm
                                //var hisCharge = unitOfWork.HISChargeRepository.FirstOrDefault(x => x.ChargeId == item.ChargeId);
                                //if (hisCharge != null)
                                //{
                                //    //Cập nhật
                                //    CreateOrUpdateHisCharge(item);
                                //}
                                //else
                                //{
                                //    //Tạo mới
                                //    CreateOrUpdateHisCharge(item);
                                //}
                                #endregion .Update hischarge
                                #region Update hischange detail
                                //tungdd14 thêm model.Status == (int)PatientInPackageEnum.RE_EXAMINATE trường hợp tái khám
                                if (model.Status == (int)PatientInPackageEnum.ACTIVATED || model.Status == (int)PatientInPackageEnum.EXPIRED
                                    || (model.Status == (int)PatientInPackageEnum.ACTIVATED && model.EndAt < Constant.CurrentDate) || model.Status == (int)PatientInPackageEnum.RE_EXAMINATE)
                                {
                                    var hisChargeDetail = unitOfWork.HISChargeDetailRepository.Find(x => x.HisChargeId == item.Id);
                                    if (hisChargeDetail?.Count() > 0)
                                    {
                                        List<ChargeInPackageModel> listChargesMoveOutPackge = new List<ChargeInPackageModel>();
                                        foreach (var itemX in hisChargeDetail)
                                        {
                                            //TH Hủy chỉ định
                                            if (Constant.ChargeStatusCancel.Contains(item.ChargeStatus))
                                            {
                                                itemX.InPackageType = (int)InPackageType.CHARGE_CANCELLED;
                                                unitOfWork.HISChargeDetailRepository.Update(itemX);
                                                CustomLog.intervaljoblog.Info(string.Format("<SynChargeInformtionFromOH> Update charge when cancelled: {0}", item.ChargeId));
                                            }
                                            //13-07-2022:Phubq xử lý TH
                                            //TH Move từ chỉ định gói sang chỉ định lẻ
                                            if (!Constant.VISIT_TYPE_PACKAGES.Contains(item.VisitType))
                                            {
                                                itemX.InPackageType = (int)InPackageType.CHARGE_MOVEOUTPACKGE;
                                                //25-07-2022: tungdd14 update thêm giá vào hisChargeDetail khi move ra khỏi gói 
                                                itemX.UnitPrice = itemX.ChargePrice;
                                                itemX.NetAmount = itemX.UnitPrice * item.Quantity;
                                                unitOfWork.HISChargeDetailRepository.Update(itemX);
                                                //20-07-2022: Tungdd14 Xử lý trường hợp update giá các chỉ định move ra khỏi gói lên OH
                                                ChargeInPackageModel chargePackage = new ChargeInPackageModel()
                                                {
                                                    IsChecked = true,
                                                    ChargeId = itemX.HISCharge.ChargeId,
                                                    InPackageType = itemX.InPackageType,
                                                    Price = itemX.ChargePrice,
                                                };
                                                listChargesMoveOutPackge.Add(chargePackage);
                                                CustomLog.intervaljoblog.Info(string.Format("<SynChargeInformtionFromOH> Update charge when move out package: {0}", item.ChargeId));
                                            }
                                        }
                                        //20-07-2022: Tungdd14 Xử lý trường hợp update giá các chỉ định move ra khỏi gói lên OH
                                        if (listChargesMoveOutPackge.Count > 0)
                                        {
                                            string returnMsg = string.Empty;
                                            var returnUpdateOH = OHConnectionAPI.UpdateChargePrice(listChargesMoveOutPackge, out returnMsg);
                                            if (returnUpdateOH)
                                            {
                                                if (Constant.StatusUpdatePriceOKs.Contains(returnMsg))
                                                {
                                                    //Cập nhật thành công
                                                    returnValue = true;
                                                }
                                                else
                                                {
                                                    //Cập nhật thất bại
                                                    returnValue = false;
                                                }
                                            }
                                            else
                                            {
                                                returnValue = false;
                                                //Cập nhật thất bại
                                            }
                                        }
                                    }

                                }
                                //else if(model.Status == (int)PatientInPackageEnum.CLOSED)
                                //{

                                //}
                                #endregion .Update hischange detail
                            }
                        }
                    }
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Cập nhật tình hình sử dụng các dịch vụ trong gói
        /// </summary>
        /// <param name="patientinpackageid"></param>
        public void UpgradeStatictisUsingServiceInPackage(Guid patientinpackageid)
        {
            var xQuery = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientinpackageid && !x.IsDeleted && !x.ServiceInPackage.IsDeleted);
            //var entities = xQuery?.ToList();
            //if (entities?.Count>0)
            if (xQuery.Any())
            {
                foreach (var item in xQuery?.ToList())
                //foreach (var item in entities)
                {
                    //QtyWasUsed
                    var QtyCharged = unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageDetailId == item.Id && x.InPackageType == (int)InPackageType.INPACKAGE && !x.ChargeIsUseForReExam && !x.IsDeleted).Sum(x => x.Quantity);
                    item.QtyWasUsed = QtyCharged;
                    //tungdd14 thêm cập nhật ReExamQtyWasUsed
                    var QtyReExamCharged = unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageDetailId == item.Id && x.InPackageType == (int)InPackageType.INPACKAGE && x.ChargeIsUseForReExam && !x.IsDeleted).Sum(x => x.Quantity);
                    item.ReExamQtyWasUsed = QtyReExamCharged;
                    //get total was used
                    var TotalQtyCharged = unitOfWork.HISChargeDetailRepository.Find(x => !x.IsDeleted && x.PatientInPackageId == patientinpackageid && (x.PatientInPackageDetailId == item.Id
                        || (x.PatientInPackageDetail.ServiceInPackage.RootId != null && x.PatientInPackageDetail.ServiceInPackage.RootId == item.ServiceInPackageId)
                        || (item.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId == x.PatientInPackageDetail.ServiceInPackageId)
                        || (x.PatientInPackageDetail.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId == x.PatientInPackageDetail.ServiceInPackage.RootId)
                        )
                        && x.InPackageType == (int)InPackageType.INPACKAGE && !x.ChargeIsUseForReExam).Sum(x => x.Quantity);
                    //get total reexam was used
                    var TotalQtyReExamCharged = unitOfWork.HISChargeDetailRepository.Find(x => !x.IsDeleted && x.PatientInPackageId == patientinpackageid && (x.PatientInPackageDetailId == item.Id
                        || (x.PatientInPackageDetail.ServiceInPackage.RootId != null && x.PatientInPackageDetail.ServiceInPackage.RootId == item.ServiceInPackageId)
                        || (item.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId == x.PatientInPackageDetail.ServiceInPackageId)
                        || (x.PatientInPackageDetail.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId == x.PatientInPackageDetail.ServiceInPackage.RootId)
                        )
                        && x.InPackageType == (int)InPackageType.INPACKAGE && x.ChargeIsUseForReExam).Sum(x => x.Quantity);
                    item.QtyRemain = item.ServiceInPackage.LimitQty - TotalQtyCharged;
                    //Định mức còn lại >=0
                    item.QtyRemain = item.QtyRemain > 0 ? item.QtyRemain : 0;
                    //tungdd thêm cập nhật số lượng tám khám còn lại
                    item.ReExamQtyRemain = item.ReExamQtyLimit - TotalQtyReExamCharged;
                    item.ReExamQtyRemain = item.ReExamQtyRemain > 0 ? item.ReExamQtyRemain : 0;
                    unitOfWork.PatientInPackageDetailRepository.Update(item);

                    CustomLog.intervaljoblog.Info(string.Format("<Update Patient's Package detail QtyWasUsed> Info: [Code: {0}; Name: {1}; PatientInPackageId: {2}; QtyWasUsed: {3}]", item.ServiceInPackage.Service.Code, item.ServiceInPackage.Service.ViName, patientinpackageid, QtyCharged));
                }
            }
        }
        public void ReleaseChargeFromPatientInPackage(Guid patientinpackageid, IUnitOfWork unitOfWorkLocal)
        {
            #region Delete HisChageDetail
            var XHisChargeDetail = unitOfWorkLocal.HISChargeDetailRepository.Find(x => x.PatientInPackageId == patientinpackageid);
            if (XHisChargeDetail.Any())
            {
                foreach (var item in XHisChargeDetail)
                    unitOfWorkLocal.HISChargeDetailRepository.HardDelete(item);
            }
            #endregion Delete HisChageDetail
            #region Delete HisChage
            var XHisCharge = unitOfWorkLocal.HISChargeRepository.Find(x => x.PatientInPackageId == patientinpackageid);
            if (XHisCharge.Any())
            {
                foreach (var item in XHisCharge)
                {
                    item.PatientInPackageId = null;
                    unitOfWorkLocal.HISChargeRepository.Update(item);
                }
            }
            #endregion Delete HisChageDetail
            CustomLog.intervaljoblog.Info(string.Format("<Release Charge From PatientInPackage> Info: [PatientInPackageId: {0};]", patientinpackageid));
        }
        public void ReleaseChargeFromPatientInPackageWhenClosed(Guid patientinpackageid, IUnitOfWork unitOfWorkLocal)
        {
            List<int> listInPackageTypeOverOutSideInvalid = new List<int>() { (int)InPackageType.OVERPACKAGE, (int)InPackageType.OUTSIDEPACKAGE, (int)InPackageType.QTYINCHARGEGREATTHANREMAIN };
            #region Delete HisChageDetail
            var XHisChargeDetail = unitOfWorkLocal.HISChargeDetailRepository.Find(x => x.PatientInPackageId == patientinpackageid && listInPackageTypeOverOutSideInvalid.Contains(x.InPackageType));
            if (XHisChargeDetail.Any())
            {
                foreach (var item in XHisChargeDetail)
                {
                    unitOfWorkLocal.HISChargeDetailRepository.HardDelete(item);
                    #region Delete HisChage
                    var XHisCharge = unitOfWorkLocal.HISChargeRepository.Find(x => x.Id == item.HisChargeId);
                    if (XHisCharge.Any())
                    {
                        foreach (var itemH in XHisCharge)
                        {
                            itemH.PatientInPackageId = null;
                            unitOfWorkLocal.HISChargeRepository.Update(itemH);
                        }
                    }
                    #endregion Delete HisChageDetail
                }

            }
            #endregion Delete HisChageDetail
            CustomLog.intervaljoblog.Info(string.Format("<Release Charge From PatientInPackage when close package> Info: [PatientInPackageId: {0};]", patientinpackageid));
        }
        public void GetAllChargeInpackge(string pId, string listCharges, List<HISChargeModel> listEntities)
        {
            List<HISChargeModel> oHEntities = null;
            if (!string.IsNullOrEmpty(listCharges))
            {
                //Get from OH
                oHEntities = OHConnectionAPI.GetCharges(pId, string.Empty, listCharges);
            }
            if (oHEntities?.Count > 0)
            {
                listEntities = listEntities == null ? new List<HISChargeModel>() : listEntities;
                listEntities.AddRange(oHEntities);
                //Get new charge
                string arrCharges = string.Join(";", oHEntities.Where(x => x.NewChargeId != null)?.Select(x => x.NewChargeId)?.ToList());
                if (!string.IsNullOrEmpty(arrCharges))
                {
                    GetAllChargeInpackge(pId, arrCharges, listEntities);
                }
            }
        }
        /// <summary>
        /// Create or update Patient in package
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public int CreateOrUpdatePatientInPackage(PatientInPackageModel request, out PatientInPackage outOvelapPiPkg)
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
                    //var pkgEntity = unitOfWork.PackageRepository.AsEnumerable().Where(x => x.PackagePrices.Any(y => y.Id == request.PolicyId)).FirstOrDefault();
                    var pkgEntity = unitOfWork.PackageRepository.FirstOrDefault(x => x.PackagePrices.Any(y => y.Id == request.PolicyId));
                    if (pkgEntity == null)
                    {
                        //Gói khám không tồn tại
                        outOvelapPiPkg = null;
                        return (int)StatusEnum.NOT_FOUND;
                    }
                    PatientInPackage entityPiPkg = null;
                    if (CheckDupplicateRegistered(request.PatientModel.Id.Value, pkgEntity.Id, request.GetStartAt(), request.GetEndAt(), out entityPiPkg))
                    {
                        //Overlap
                        outOvelapPiPkg = entityPiPkg;
                        return (int)StatusEnum.CONFLICT;
                    }
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
                    #region Check Tổng tiền
                    if (request.Services?.Count > 0)
                    {
                        #region Check tổng tiền trong dịch vụ <> với tổng tiền sau giảm giá
                        //2022-08-02:Phubq tạm bỏ điều kiện !x.IsServiceFreeInPackage
                        var TotalAmount = request.Services.Where(x => /*!x.IsServiceFreeInPackage &&*/ x.ServiceInPackageRootId == null).Sum(x => x.PkgAmount);
                        TotalAmount = TotalAmount != null ? Math.Round(TotalAmount.Value) : 0;
                        //Check lệch hơn 10 đồng thì sẽ thông báo
                        var otherAmount = TotalAmount - request.NetAmount;
                        if (otherAmount >= 10 || otherAmount <= -10)
                        {
                            //Total Amount service not equal netamount
                            outOvelapPiPkg = null;
                            return (int)StatusEnum.TOTAL_AMOUNT_SERVICE_NOT_EQUAL_NETAMOUNT;
                        }
                        #endregion .Check tổng tiền trong dịch vụ <> với tổng tiền sau giảm giá
                        #region Check Tổng tiền thuốc /VTTH> NetAmount
                        var TotalAmountDrugConsum = request.Services.Where(x => (x.ServiceType == 2 || x.IsPackageDrugConsum) && x.ServiceInPackageRootId == null).Sum(x => x.PkgAmount);
                        TotalAmountDrugConsum = TotalAmountDrugConsum != null ? Math.Round(TotalAmountDrugConsum.Value) : 0;
                        if (TotalAmountDrugConsum > request.NetAmount)
                        {
                            //Total drug and consum greater than netamount
                            outOvelapPiPkg = null;
                            return (int)StatusEnum.TOTAL_AMOUNT_DRUG_CONSUM_GREATER_THAN_NETAMOUNT;
                        }
                        #endregion
                    }
                    #endregion .Check Tổng tiền
                    PatientInPackage entity = null;
                    #region Comment Code update (Modify information)
                    //entity = unitOfWorkLocal.PatientInPackageRepository.AsEnumerable().Where(x => x.PatientInforId == request.PatientModel.Id && x.PackagePriceSiteId == pkgPriceSite.Id
                    //&& ((x.StartAt<=request.GetStartAt() && x.EndAt>=request.GetStartAt()) || (x.StartAt<=request.GetEndAt() && x.EndAt>=request.GetEndAt()) 
                    //|| (x.StartAt >= request.GetStartAt() && x.EndAt <= request.GetEndAt()))
                    //).FirstOrDefault();
                    //if (entity != null)
                    //{
                    //    returnValue = (int)StatusEnum.CONFLICT;
                    //    return returnValue;
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
                    //    entity.NetAmount = request.NetAmount>0?request.NetAmount: pkgPriceSite.PackagePrice.Amount.Value;
                    //    if (DateTime.Now >= entity.StartAt)
                    //        entity.Status = (int)PatientInPackageEnum.ACTIVATED;
                    //    else
                    //        entity.Status = (int)PatientInPackageEnum.REGISTERED;

                    //    unitOfWorkLocal.PatientInPackageRepository.Update(entity);
                    //    #endregion .BUZ Edit
                    //}
                    //else
                    #endregion .Comment Code update (Modify information)
                    {
                        //Thêm mới
                        entity = new PatientInPackage();
                        if (request.NewPatientInPackageId != null)
                            entity.Id = request.NewPatientInPackageId.Value;
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
        public PatientInPackage CreateUpdatePatientInPackage(PatientInPackageInfoModel model, PackagePriceSite pkgPriceSite)
        {
            var entity = new PatientInPackage();
            entity.PackagePriceSiteId = pkgPriceSite.Id;
            entity.PatientInforId = model.PatientModel.Id.Value;
            #region Contract Info
            entity.ContractNo = model.ContractNo;
            entity.ContractDate = model.GetContractDate();
            entity.ContractOwner = model.ContractOwnerAd;
            entity.ContractOwnerFullName = model.ContractOwnerFullName;
            #endregion .Contract Info
            #region Doctor Consult Info
            entity.DoctorConsult = model.DoctorConsultAd;
            entity.DoctorConsultFullName = model.DoctorConsultFullName;
            entity.DepartmentId = model.DepartmentId;
            #endregion .Doctor Consult Info
            entity.StartAt = model.GetStartAt();
            entity.EndAt = model.GetEndAt();
            entity.IsMaternityPackage = model.IsMaternityPackage;
            entity.EstimateBornDate = model.GetEstimateBornDate();
            #region Discount Info
            entity.IsDiscount = model.IsDiscount;
            entity.DiscountType = model.DiscountType;
            entity.DiscountAmount = model.DiscountAmount;
            entity.DiscountNote = model.DiscountNote;
            #endregion .Discount Info
            entity.NetAmount = model.NetAmount > 0 ? model.NetAmount : pkgPriceSite.PackagePrice.Amount.Value;
            if (Constant.CurrentDate >= entity.StartAt && entity.Status == (int)PatientInPackageEnum.REGISTERED)
                entity.Status = (int)PatientInPackageEnum.ACTIVATED;
            else
            {
                entity.Status = (int)PatientInPackageEnum.REGISTERED;
            }
            unitOfWork.PatientInPackageRepository.Add(entity);
            return entity;
        }
        public IEnumerable<PatientInPackage> GetPatientInPackageInfo(Guid Id)
        {
            //var results = unitOfWork.PatientInPackageRepository.AsEnumerable().Where(e => !e.IsDeleted && e.Id == Id);
            var results = unitOfWork.PatientInPackageRepository.Find(e => !e.IsDeleted && e.Id == Id);
            return results;
        }
        /// <summary>
        /// linhht Update contract, scale enddate PatientInPackage (chỉnh sửa hợp đồng, gia hạn gói)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public dynamic ScaleUpPatientInPackage(PatientInPackageUpdateModel request, bool isScaleUp = true)
        {
            dynamic returnValue = null;
            try
            {
                #region validate Logic
                //check exist patientInPackage
                var patientInPackage = unitOfWork.PatientInPackageRepository.Find(x => x.Id == request.Id && !x.IsDeleted).FirstOrDefault();
                if (patientInPackage == null)
                {
                    return returnValue = Message.NOT_FOUND_PACKAGE;
                }
                if (patientInPackage.EndAt > request.GetEndFullDate())
                {
                    return returnValue = Message.NOTE_MODIFY_PRICEPOLICY_ENDDATE_EARLIERTHAN;
                }

                //Kiểm tra tại [Ngày hợp đồng], site đăng ký có chính sách giá không (nếu ngày hợp đồng trống, bỏ qua bước này)
                if (!string.IsNullOrEmpty(request.ContractDate))
                {
                    //check exist packagePriceSite
                    var pkgPriceSite = unitOfWork.PackagePriceSiteRepository.FirstOrDefault(x => x.Id == patientInPackage.PackagePriceSiteId);
                    if (pkgPriceSite == null)
                    {
                        return returnValue = Message.NOT_FOUND_POLICY;
                    }
                    //Nếu có, chuyển sang Bước 6.2
                    //Đối chiếu gía trị [Ngày hợp đồng] và [Ngày bắt đầu sử dụng gói] (nếu ngày hợp đồng trống, bỏ qua bước này)
                    //Nếu [Ngày hợp đồng] > [Ngày bắt đầu sử dụng gói], hiển thị thông báo lỗi MSG 34: Ngày hợp đồng không được lớn hơn ngày bắt đầu sử dụng gói

                    /*Thời gian bắt đầu sử dụng luôn luôn bằng hoặc sau Thời gian hợp đồng.*/
                    if (request.GetStartAt().Date < request.GetContractDate().Value.Date)
                    {
                        //Thời gian bắt đầu sử dụng trước ngày hợp đồng
                        return returnValue = Message.PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER1;
                    }

                    //Ngược lại, hiển thị thông báo lỗi MSG 21: Cơ sở khởi tạo <<mã facility cơ sở khởi tạo>> chưa được áp dụng chính sách giá gói này ngày [Ngày hợp đồng]
                    /*Thời gian hợp đồng luôn bằng hoặc sau thời gian acitve giá tại site đang đăng ký gói*/
                    if (pkgPriceSite.EndAt < request.GetContractDate() || pkgPriceSite.CreatedAt.Value.Date > request.GetContractDate().Value.Date)
                    {
                        var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.MSG21_OVER_POLICY);
                        returnValue = (MessageModel)msg.Clone();
                        returnValue.ViMessage = string.Format(msg.ViMessage,
                            pkgPriceSite.Site.Code,
                            request.GetContractDate().Value.ToString(Constant.DATE_FORMAT));
                        returnValue.EnMessage = string.Format(msg.EnMessage,
                            pkgPriceSite.Site.Code,
                            request.GetContractDate().Value.ToString(Constant.DATE_FORMAT));
                        return returnValue;
                    }

                }
                //Nếu [Ngày hợp đồng] ≤ [Ngày bắt đầu sử dụng gói] hoặc người dùng không nhập [Ngày hợp đồng], chuyển sang Bước 6.3
                //Kiểm tra KH có được đăng ký mã gói này nhiều lần không. Nếu có, thì có gói nào đang ở trạng thái “Đã đăng ký”/ “Đang sử dụng”/ “Hết hạn”/ “Theo dõi tái khám” và bị trùng thời gian (overlap) với hạn sử dụng mới không?
                var lstStatusPatientInPackage = new int[] { (int)PatientInPackageEnum.REGISTERED, (int)PatientInPackageEnum.ACTIVATED, (int)PatientInPackageEnum.EXPIRED/*linhht Chưa có theo dõi tái khám*/ };
                var package = unitOfWork.PackageRepository.FirstOrDefault(x => x.Code == request.PackageCode);
                if (package == null)
                {
                    return returnValue = Message.NOT_FOUND_PACKAGE;
                }
                //lấy danh sách gói khám của bệnh nhân
                var lstPatientInPackage = unitOfWork.PatientInPackageRepository.Find(x => lstStatusPatientInPackage.Contains(x.Status) && x.PackagePriceSite.PackagePrice.PackageId == package.Id && x.Id != patientInPackage.Id && x.PatientInforId == request.PatientId).ToList();
                if (lstPatientInPackage.Any())
                {
                    if (lstPatientInPackage.Count(x =>
                    //end date trong khoảng đã có, start date trước start
                    (x.StartAt > request.GetStartFullDate() && x.StartAt < request.GetEndFullDate() && x.EndAt > request.GetEndFullDate())
                    //start date trong khoảng đã có, end date sau end
                    || (x.EndAt < request.GetEndFullDate() && x.StartAt < request.GetStartFullDate() && x.EndAt > request.GetStartFullDate())
                    //date bao khoảng đã có
                    || (x.StartAt > request.GetStartFullDate() && x.EndAt < request.GetEndFullDate())
                    //khoảng đã có bao date
                    || (x.StartAt < request.GetStartFullDate() && x.EndAt > request.GetEndFullDate())
                    ) > 0)
                    {
                        var overLapPtInPackage = lstPatientInPackage.FirstOrDefault(x =>
                    //end date trong khoảng đã có, start date trước start
                    (x.StartAt > request.GetStartFullDate() && x.StartAt < request.GetEndFullDate() && x.EndAt > request.GetEndFullDate())
                    //start date trong khoảng đã có, end date sau end
                    || (x.EndAt < request.GetEndFullDate() && x.StartAt < request.GetStartFullDate() && x.EndAt > request.GetStartFullDate())
                    //date bao khoảng đã có
                    || (x.StartAt > request.GetStartFullDate() && x.EndAt < request.GetEndFullDate())
                    //khoảng đã có bao date
                    || (x.StartAt < request.GetStartFullDate() && x.EndAt > request.GetEndFullDate())
                    );
                        //Overlap
                        var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.OVERLAP_PACKAGE_WARNING);
                        returnValue = (MessageModel)msg.Clone();
                        returnValue.ViMessage = string.Format(msg.ViMessage, patientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Code,
                            patientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Name,
                            overLapPtInPackage.StartAt.ToString(Constant.DATE_FORMAT),
                            overLapPtInPackage.EndAt?.ToString(Constant.DATE_FORMAT));
                        returnValue.EnMessage = string.Format(msg.EnMessage, patientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Code,
                            patientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Name, overLapPtInPackage.StartAt.ToString(Constant.DATE_FORMAT),
                            overLapPtInPackage.EndAt?.ToString(Constant.DATE_FORMAT));
                        return returnValue;
                    }
                }
                #endregion validate Logic
                #region update data
                #region update patientInPackage
                patientInPackage.EndAt = request.GetEndFullDate();
                patientInPackage.ContractNo = request.ContractNo;
                patientInPackage.ContractDate = request.GetContractDate();
                patientInPackage.ContractOwner = request.ContractOwnerAd;
                patientInPackage.ContractOwnerFullName = request.ContractOwnerFullName;
                //Cập nhật trạng thái gói theo quy tắc
                //Trường hợp 2: Nếu [Ngày hết hạn gói] ≥ {Ngày hiện tại}, kiểm tra và cập nhật trạng thái gói theo quy tắc tại Hình 1: Bảng quy tắc cập nhật trạng thái gói sau khi gia hạn.  
                var statusBeforeChange = patientInPackage.Status;
                patientInPackage.Status = (patientInPackage.LastStatus != null && patientInPackage.LastStatus != 0) ? patientInPackage.LastStatus.Value : (int)PatientInPackageEnum.ACTIVATED;
                if (statusBeforeChange == (int)PatientInPackageEnum.REGISTERED)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.REGISTERED;
                }
                //tungdd14: thêm trường hợp gói ở trạng thái tái khám
                else if (statusBeforeChange == (int)PatientInPackageEnum.RE_EXAMINATE)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.RE_EXAMINATE;
                }
                //Trường hợp 1: Nếu[Ngày hết hạn gói mới] < { Ngày hiện tại} => cập nhật trạng thái gói là “Hết hạn”
                if (patientInPackage.EndAt < DateTime.Now)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.EXPIRED;
                }
                else if (patientInPackage.Status == (int)PatientInPackageEnum.EXPIRED)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.ACTIVATED;
                }

                unitOfWork.PatientInPackageRepository.Update(patientInPackage);
                #endregion
                #region update patientInPackageDetail với các dịch vụ thay thế
                //dịch vụ trong gói cấu hình
                var lstServiceInPackage = unitOfWork.ServiceInPackageRepository.Find(x => x.PackageId == package.Id && x.RootId != null);
                if (lstServiceInPackage.Any())
                    UpdatePatientInPackageDetailWhenHaveNewReplaceService(lstServiceInPackage.Select(x => x.Id).ToList(), package.Id, patientInPackage.Id);
                #endregion
                unitOfWork.Commit();
                #endregion
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("ScaleUpPatientInPackage fail. Ex: {0}", ex));
                //returnValue = false;
                returnValue = Message.INTERAL_SERVER_ERROR;
            }
            return returnValue;
        }

        /// <summary>
        /// linhht Reopen (Mở lại gói đã đóng)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public dynamic ReOpenPatientInPackage(PatientInPackageReopenModel request)
        {
            dynamic returnValue = null;
            try
            {
                #region validate Logic
                var now = DateTime.Now;
                var patientInPackage = unitOfWork.PatientInPackageRepository.GetById(request.Id);
                if (patientInPackage == null)
                {
                    return returnValue = Message.NOT_FOUND_PACKAGE;
                }
                if (patientInPackage.Status != (int)PatientInPackageEnum.CLOSED)
                {
                    return returnValue = Message.NOTE_NOTALLOW_MODIFY_STATUS;
                }
                //tungdd14: Kiểm tra KH có được đăng ký mã gói này nhiều lần không. Nếu có, thì có gói nào đang ở trạng thái “Đã đăng ký”/ “Đang sử dụng”/ “Hết hạn”/ “Theo dõi tái khám” và bị trùng thời gian (overlap) với hạn sử dụng mới không?
                var lstStatusPatientInPackage = new int[] { (int)PatientInPackageEnum.REGISTERED, (int)PatientInPackageEnum.ACTIVATED, (int)PatientInPackageEnum.EXPIRED/*linhht Chưa có theo dõi tái khám*/ };
                var package = unitOfWork.PackageRepository.FirstOrDefault(x => x.Code == patientInPackage.PackagePriceSite.PackagePrice.Package.Code);
                if (package == null)
                {
                    return returnValue = Message.NOT_FOUND_PACKAGE;
                }
                //lấy danh sách gói khám của bệnh nhân
                var lstPatientInPackage = unitOfWork.PatientInPackageRepository.Find(x => lstStatusPatientInPackage.Contains(x.Status) && x.PackagePriceSite.PackagePrice.PackageId == package.Id && x.Id != patientInPackage.Id && x.PatientInforId == request.PatientId).ToList();
                if (lstPatientInPackage.Any())
                {
                    if (lstPatientInPackage.Count(x =>
                    //end date trong khoảng đã có, start date trước start
                    (x.StartAt >= patientInPackage.StartAt && x.StartAt <= patientInPackage.EndAt && x.EndAt >= patientInPackage.EndAt)
                    //start date trong khoảng đã có, end date sau end
                    || (x.EndAt <= patientInPackage.EndAt && x.StartAt <= patientInPackage.StartAt && x.EndAt >= patientInPackage.StartAt)
                    //date bao khoảng đã có
                    || (x.StartAt > patientInPackage.StartAt && x.EndAt <= patientInPackage.EndAt)
                    //khoảng đã có bao date
                    || (x.StartAt <= patientInPackage.StartAt && x.EndAt >= patientInPackage.EndAt)
                    ) > 0)
                    {
                        var overLapPtInPackage = lstPatientInPackage.FirstOrDefault(x =>
                    //end date trong khoảng đã có, start date trước start
                    (x.StartAt >= patientInPackage.StartAt && x.StartAt <= patientInPackage.EndAt && x.EndAt >= patientInPackage.EndAt)
                    //start date trong khoảng đã có, end date sau end
                    || (x.EndAt <= patientInPackage.EndAt && x.StartAt <= patientInPackage.StartAt && x.EndAt >= patientInPackage.StartAt)
                    //date bao khoảng đã có
                    || (x.StartAt >= patientInPackage.StartAt && x.EndAt <= patientInPackage.EndAt)
                    //khoảng đã có bao date
                    || (x.StartAt <= patientInPackage.StartAt && x.EndAt >= patientInPackage.EndAt)
                    );
                        //Overlap
                        var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.OVERLAP_PACKAGE_WARNING);
                        returnValue = (MessageModel)msg.Clone();
                        returnValue.ViMessage = string.Format(msg.ViMessage, patientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Code,
                            patientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Name,
                            overLapPtInPackage.StartAt.ToString(Constant.DATE_FORMAT),
                            overLapPtInPackage.EndAt?.ToString(Constant.DATE_FORMAT));
                        returnValue.EnMessage = string.Format(msg.EnMessage, patientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Code,
                            patientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Name, overLapPtInPackage.StartAt.ToString(Constant.DATE_FORMAT),
                            overLapPtInPackage.EndAt?.ToString(Constant.DATE_FORMAT));
                        return returnValue;
                    }
                }

                //Trường hợp 1: Nếu [Ngày bắt đầu gói] < {Ngày hiện tại} => cập nhật trạng thái gói là “Đã đăng kí”.
                //Lưu ý: Trường hợp User yêu cầu gia hạn gói này, thì cần cập nhật lại trạng thái theo trường hợp 2.
                if (patientInPackage.StartAt < now)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.REGISTERED;
                }
                //Trường hợp 1: Nếu [Ngày hết hạn gói] < {Ngày hiện tại} => cập nhật trạng thái gói là “Hết hạn”.
                //Lưu ý: Trường hợp User yêu cầu gia hạn gói này, thì cần cập nhật lại trạng thái theo trường hợp 2.
                if (patientInPackage.EndAt < now)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.EXPIRED;
                }
                else if (patientInPackage.Status == (int)PatientInPackageEnum.EXPIRED)
                {
                    patientInPackage.Status = (int)PatientInPackageEnum.ACTIVATED;
                }
                //Trường hợp 2: Nếu [Ngày hết hạn gói] ≥{Ngày hiện tại}: Kiểm tra trạng thái trước khi đóng gói:
                //Nếu trước khi đóng, trạng thái gói = “Theo dõi tái khám” thì cập nhật trạng thái là “Theo dõi tái khám”. 
                //TODO: trạng thái tái khám
                //Nếu trước khi đóng gói, gói có trạng thái là “Đang sử dụng” thì cập nhật trạng thái là “Đang sử dụng”
                if (patientInPackage.LastStatus == (int)PatientInPackageEnum.RE_EXAMINATE || patientInPackage.LastStatus == (int)PatientInPackageEnum.ACTIVATED)
                {
                    patientInPackage.Status = patientInPackage.LastStatus.Value;
                }

                #endregion
                #region Cập nhật thông tin: Bảng tình hình sử dụng gói, Thông tin lượt khám & Thông tin Theo dõi tái khám (nếu có)
                unitOfWork.PatientInPackageRepository.Update(patientInPackage);
                //2022-08-09:Phubq. bug 5215- Gói được reopen thành công -> Không cập nhật DV thay thế. 
                #region update patientInPackageDetail với các dịch vụ thay thế
                //dịch vụ trong gói cấu hình
                var lstServiceInPackage = unitOfWork.ServiceInPackageRepository.Find(x => x.PackageId == patientInPackage.PackagePriceSite.PackagePrice.PackageId && x.RootId != null);
                if (lstServiceInPackage.Any())
                    new PatientInPackageRepo().UpdatePatientInPackageDetailWhenHaveNewReplaceService(lstServiceInPackage.Select(x => x.Id).ToList(), patientInPackage.PackagePriceSite.PackagePrice.PackageId.Value, patientInPackage.Id);
                #endregion

                unitOfWork.Commit();
                #endregion
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("ReOpenPatientInPackage fail. Ex: {0}", ex));
                //returnValue = false;
                returnValue = Message.INTERAL_SERVER_ERROR;
            }
            return returnValue;
        }
        /// <summary>
        /// linhht Get re-exmainate service's 
        /// lấy các dịch vụ tab theo dõi tái khám
        /// </summary>
        /// <param name="patientInPackageId"></param>
        /// <returns></returns>
        //public List<PatientInPackageServiceUsingStatusModel> GetListPatientInPackageServiceReExaminate(Guid patientInPackageId)
        //{
        //    List<PatientInPackageServiceUsingStatusModel> entities = null;
        //    try
        //    {
        //        var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientInPackageId);
        //        if (entity == null)
        //        {
        //            return null;
        //        }

        //        //Get list PackagePriceDetails
        //        var listData = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientInPackageId && !x.IsDeleted && !x.ServiceInPackage.IsDeleted);
        //        entities = listData.OrderBy(x => x.ServiceInPackage.ServiceType).ThenBy(x => x.ServiceInPackage.IsPackageDrugConsum).ThenBy(x => x.ServiceInPackage.CreatedAt).Select(x => new PatientInPackageServiceUsingStatusModel()
        //        {
        //            Id = x.Id,
        //            ServiceInPackageId = x.ServiceInPackage.Id,
        //            ServiceCode = x.ServiceInPackage.Service.Code,
        //            ServiceName = x.ServiceInPackage.Service.ViName,
        //            Qty = x.ServiceInPackage.LimitQty,
        //            PkgPrice = x.PkgPrice,
        //            IsPackageDrugConsum = x.ServiceInPackage.IsPackageDrugConsum,
        //            ServiceType = x.ServiceInPackage.ServiceType,
        //            QtyWasUsed = 0,
        //            QtyNotUsedYet = x.ServiceInPackage.LimitQty,
        //            AmountNotUsedYet = x.ServiceInPackage.LimitQty * x.PkgPrice,
        //            QtyOver = 0,
        //            ItemsReplace = unitOfWork.ServiceInPackageRepository.Find(y => y.RootId == x.ServiceInPackageId)?.Select(y => new { ServiceInPackageId = y.Id, y.ServiceId, ServiceName = y.Service?.ViName, ServiceCode = y.Service?.Code }),
        //            //linhht 
        //            QtyReExamWasUsed = x.ReExamQtyWasUsed,
        //            QtyReExamNotUsed = x.ReExamQtyRemain
        //            //----
        //        })?.ToList();
        //        #region Stat current using service in package
        //        if (entities?.Count > 0)
        //        {
        //            var listHisChargeDetail = unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageId == patientInPackageId && !x.IsDeleted && !x.PatientInPackageDetail.ServiceInPackage.IsDeleted);
        //            if (listHisChargeDetail.Any())
        //            {
        //                foreach (var item in listHisChargeDetail)
        //                {
        //                    var itemX = entities.Find(x => x.ServiceCode == item.PatientInPackageDetail.ServiceInPackage.Service.Code || item.PatientInPackageDetail.ServiceInPackage.RootId == x.ServiceInPackageId);
        //                    if (itemX != null)
        //                    {
        //                        if (item.InPackageType == (int)InPackageType.INPACKAGE)
        //                        {
        //                            //Cập nhật thông tin trong gói
        //                            itemX.QtyWasUsed = (itemX.QtyWasUsed == null ? 0 : itemX.QtyWasUsed) + (item.Quantity == null ? 0 : item.Quantity);
        //                            //Số lượng Sdung luôn bằng định mức
        //                            //itemX.QtyWasUsed = itemX.QtyWasUsed > itemX.Qty ? itemX.Qty : itemX.QtyWasUsed;
        //                            itemX.QtyWasInvoiced = (itemX.QtyWasInvoiced == null ? 0 : itemX.QtyWasInvoiced) + (item.Quantity != null && item.HISCharge.InvoicePaymentStatus == Constant.PAYMENT_PSL_STATUS ? item.Quantity : 0);
        //                            itemX.QtyNotUsedYet = (itemX.Qty == null ? 0 : itemX.Qty) - (itemX.QtyWasUsed == null ? 0 : itemX.QtyWasUsed);
        //                            itemX.AmountWasUsed = itemX.QtyWasUsed * itemX.PkgPrice;
        //                            itemX.AmountNotUsedYet = itemX.QtyNotUsedYet * itemX.PkgPrice;
        //                        }
        //                        else if (item.InPackageType == (int)InPackageType.OVERPACKAGE)
        //                        {
        //                            //Cập nhật thông tin vượt gói
        //                            itemX.QtyOver = (itemX.QtyOver == null ? 0 : itemX.QtyOver) + (item.Quantity == null ? 0 : item.Quantity);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        #endregion .Stat current using service in package

        //        if (entities != null && entities.Count > 0)
        //        {
        //            #region Add more Total Row
        //            double? total_AmountWasUsed = entities.Sum(x => x.AmountWasUsed);
        //            double? total_AmountNotUsedYet = entities.Sum(x => x.AmountNotUsedYet);
        //            entities.Add(new PatientInPackageServiceUsingStatusModel() { ServiceType = 0, AmountWasUsed = total_AmountWasUsed, AmountNotUsedYet = total_AmountNotUsedYet });
        //            #endregion .Add more Total Row
        //            return entities;
        //        }
        //        else
        //        {
        //            return entities;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        VM.Common.CustomLog.accesslog.Error(string.Format("GetListPatientInPackageServiceUsing fail. Ex: {0}", ex));
        //    }
        //    return entities;
        //}
        /// <summary>
        /// linhht Get detail re-exmainate service's tracking 
        /// mở popup chuyển theo dõi tái khám, lấy các dịch vụ đủ điều kiện chuyển tái khám để chọn
        /// </summary>
        /// <param name="patientInPackageId"></param>
        /// <returns></returns>
        //public List<PatientInPackageServiceUsingStatusModel> GetListPatientInPackageTrackingReExaminate(Guid patientInPackageId)
        //{
        //    List<PatientInPackageServiceUsingStatusModel> entities = null;
        //    try
        //    {
        //        var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientInPackageId);
        //        if (entity == null)
        //        {
        //            return null;
        //        }
        //        //Gói của KH đã sử dụng, đang ở trạng thái “Đang sử dụng” và thuộc nhóm Bundle Payment (DT) hoặc Thai sản (TS)
        //        //Là service trên OH
        //        //Được cấu hình là dịch vụ thuộc gói đang thao tác
        //        //Số lần đã sử dụng trong gói > 0
        //        if (entity.Status != (int)PatientInPackageEnum.ACTIVATED && (!entity.IsMaternityPackage.Value || !entity.IsBundlePackage.Value)) { return null; }

        //        //Get list PackagePriceDetails
        //        var listData = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientInPackageId && !x.IsDeleted && !x.ServiceInPackage.IsDeleted && x.ServiceInPackage.RootId == null);
        //        entities = listData.OrderBy(x => x.ServiceInPackage.ServiceType).ThenBy(x => x.ServiceInPackage.IsPackageDrugConsum).ThenBy(x => x.ServiceInPackage.CreatedAt).Select(x => new PatientInPackageServiceUsingStatusModel()
        //        {
        //            Id = x.Id,
        //            ServiceInPackageId = x.ServiceInPackage.Id,
        //            ServiceCode = x.ServiceInPackage.Service.Code,
        //            ServiceName = x.ServiceInPackage.Service.ViName,
        //            Qty = x.ServiceInPackage.LimitQty,
        //            PkgPrice = x.PkgPrice,
        //            IsPackageDrugConsum = x.ServiceInPackage.IsPackageDrugConsum,
        //            ServiceType = x.ServiceInPackage.ServiceType,
        //            QtyWasUsed = 0,
        //            QtyNotUsedYet = x.ServiceInPackage.LimitQty,
        //            AmountNotUsedYet = x.ServiceInPackage.LimitQty * x.PkgPrice,
        //            QtyOver = 0,
        //            ItemsReplace = unitOfWork.ServiceInPackageRepository.Find(y => y.RootId == x.ServiceInPackageId)?.Select(y => new { ServiceInPackageId = y.Id, y.ServiceId, ServiceName = y.Service?.ViName, ServiceCode = y.Service?.Code }),
        //            //linhht 
        //            QtyReExamWasUsed = x.ReExamQtyWasUsed,
        //            QtyReExamNotUsed = x.ReExamQtyRemain
        //            //----
        //        })?.ToList();
        //        #region Stat current using service in package
        //        if (entities?.Count > 0)
        //        {
        //            var listHisChargeDetail = unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageId == patientInPackageId && !x.IsDeleted && !x.PatientInPackageDetail.ServiceInPackage.IsDeleted);
        //            if (listHisChargeDetail.Any())
        //            {
        //                foreach (var item in listHisChargeDetail)
        //                {
        //                    var itemX = entities.Find(x => x.ServiceCode == item.PatientInPackageDetail.ServiceInPackage.Service.Code || item.PatientInPackageDetail.ServiceInPackage.RootId == x.ServiceInPackageId);
        //                    if (itemX != null)
        //                    {
        //                        if (item.InPackageType == (int)InPackageType.INPACKAGE)
        //                        {
        //                            //Cập nhật thông tin trong gói
        //                            itemX.QtyWasUsed = (itemX.QtyWasUsed == null ? 0 : itemX.QtyWasUsed) + (item.Quantity == null ? 0 : item.Quantity);
        //                            //Số lượng Sdung luôn bằng định mức
        //                            //itemX.QtyWasUsed = itemX.QtyWasUsed > itemX.Qty ? itemX.Qty : itemX.QtyWasUsed;
        //                            itemX.QtyWasInvoiced = (itemX.QtyWasInvoiced == null ? 0 : itemX.QtyWasInvoiced) + (item.Quantity != null && item.HISCharge.InvoicePaymentStatus == Constant.PAYMENT_PSL_STATUS ? item.Quantity : 0);
        //                            itemX.QtyNotUsedYet = (itemX.Qty == null ? 0 : itemX.Qty) - (itemX.QtyWasUsed == null ? 0 : itemX.QtyWasUsed);
        //                            itemX.AmountWasUsed = itemX.QtyWasUsed * itemX.PkgPrice;
        //                            itemX.AmountNotUsedYet = itemX.QtyNotUsedYet * itemX.PkgPrice;
        //                        }
        //                        else if (item.InPackageType == (int)InPackageType.OVERPACKAGE)
        //                        {
        //                            //Cập nhật thông tin vượt gói
        //                            itemX.QtyOver = (itemX.QtyOver == null ? 0 : itemX.QtyOver) + (item.Quantity == null ? 0 : item.Quantity);
        //                        }
        //                        //linhht Số lần đã sử dụng trong gói > 0
        //                        if (itemX.QtyWasUsed < 1)
        //                        {
        //                            entities.Remove(itemX);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        #endregion .Stat current using service in package

        //        if (entities != null && entities.Count > 0)
        //        {
        //            #region Add more Total Row
        //            double? total_AmountWasUsed = entities.Sum(x => x.AmountWasUsed);
        //            double? total_AmountNotUsedYet = entities.Sum(x => x.AmountNotUsedYet);
        //            entities.Add(new PatientInPackageServiceUsingStatusModel() { ServiceType = 0, AmountWasUsed = total_AmountWasUsed, AmountNotUsedYet = total_AmountNotUsedYet });
        //            #endregion .Add more Total Row
        //            return entities;
        //        }
        //        else
        //        {
        //            return entities;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        VM.Common.CustomLog.accesslog.Error(string.Format("GetListPatientInPackageServiceUsing fail. Ex: {0}", ex));
        //    }
        //    return entities;
        //}
        /// <summary>
        /// linhht save re-exmainate service's tracking 
        /// lưu các dịch vụ theo dõi tái khám
        /// </summary>
        /// <param name="patientInPackageId"></param>
        /// <returns></returns>
        public dynamic SaveReExaminateServices(Guid patientinpackageid)
        {
            dynamic returnValue = null;
            try
            {
                #region validate Logic
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientinpackageid);
                if (entity == null)
                {
                    return Message.NOT_FOUND_PACKAGE;
                }
                entity.IsBundlePackage = HelperBusiness.Instant.ListGroupCodeIsBundlePackage.Contains(entity.PackagePriceSite.PackagePrice.Package.PackageGroup.Code);
                //Gói của KH đã sử dụng, đang ở trạng thái “Đang sử dụng” và có dịch vụ được đánh dấu là dịch vụ tái khám.   
                if (!((entity.Status == (int)PatientInPackageEnum.ACTIVATED ))) { return Message.OWNER_FORBIDDEN; }

                #endregion
                #region update
                //Cập nhật trạng thái của gói thành “Theo dõi tái khám”
                entity.LastStatus = entity.Status;
                entity.Status = (int)PatientInPackageEnum.RE_EXAMINATE;
                //Lưu thông tin các dịch vụ tái khám và tự động điều hướng về màn hình Chi tiết Khách hàng sử dụng gói, tab đầu tiên (Tab Theo dõi tái khám). 
                //var listPatientInPakageDetailReExam = listPatientInPackageServiceUsing.Where(x => x.IsReExamService && x.QtyWasUsed > 0).ToList();
                var listPatientInPakageDetailReExam = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientinpackageid && !x.IsDeleted && !x.ServiceInPackage.IsDeleted);
                var listPatientInPakageDetailReExamUpdate = listPatientInPakageDetailReExam;
                foreach (var item in listPatientInPakageDetailReExam)
                {
                    bool isItemReExam = item.ServiceInPackage.IsReExamService;
                    if (!isItemReExam && item.ServiceInPackage.RootId != null)
                    {
                        isItemReExam = listPatientInPakageDetailReExam.FirstOrDefault(x => x.ServiceInPackageId == item.ServiceInPackage.RootId).ServiceInPackage.IsReExamService;
                    }
                    if (isItemReExam)
                    {
                        item.ReExamQtyLimit = listPatientInPakageDetailReExamUpdate.Where(x => x.ServiceInPackage.Id == item.ServiceInPackage.Id || (x.ServiceInPackage.RootId != null && item.ServiceInPackageId != null && x.ServiceInPackage.RootId == item.ServiceInPackageId) || (x.ServiceInPackageId != null && item.ServiceInPackage.RootId != null && x.ServiceInPackageId == item.ServiceInPackage.RootId)).ToList().Sum(x => x.QtyWasUsed);
                        item.ReExamQtyLimit = item.ReExamQtyLimit > item.ServiceInPackage.LimitQty ? item.ServiceInPackage.LimitQty : item.ReExamQtyLimit;
                    }
                    

                    //TODO lưu lại thông tin service tái khám
                }
                //TODO Gọi API sang DIMS để hủy doanh thu của các chỉ định dịch vụ tái khám lần 1 (xem lại khái niệm Chỉ định dịch vụ tái khám lần 1 tại mục 2.1 Quy trình thực tế.)
                unitOfWork.PatientInPackageRepository.Update(entity);
                unitOfWork.Commit();
                #endregion

            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("SaveReExaminateServices fail. Ex: {0}", ex));
                returnValue = Message.INTERAL_SERVER_ERROR;
            }
            return returnValue;
        }
        /// <summary>
        /// linhht save re-exmainate service's tracking 
        /// lưu ghi nhận tái khám
        /// </summary>
        /// <param name="patientInPackageId"></param>
        /// <returns></returns>
        //public dynamic SaveReExaminateRecord(PatientInPackageReExaminateModel request)
        //{
        //    dynamic returnValue = null;
        //    try
        //    {
        //        #region validate Logic
        //        var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == request.Id);
        //        if (entity == null)
        //        {
        //            return Message.NOT_FOUND_PACKAGE;
        //        }
        //        //Gói của KH thuộc các loại gói cho phép theo dõi tái khám (gói thuộc nhóm Gói Điều trị (Bundle payment) - DT hoặc Gói thai sản – TS)
        //        if (!entity.IsMaternityPackage.Value || !entity.IsBundlePackage.Value) { return Message.OWNER_FORBIDDEN; }
        //        //Trạng thái hiện tại là “Theo dõi tái khám” 
        //        // Trạng thái hiện tại là “Hết hạn” và trạng thái trước đó là “Theo dõi tái khám”
        //        // Gói mới được mở lại và có trạng thái “Hết hạn” và trạng thái trước “Đã đóng” là “Theo dõi tái khám” (Trường hợp đóng gói khi chưa hết hạn, nhưng khi mở lại gói thì gói đã hết hạn). 
        //        if (entity.Status != (int)PatientInPackageEnum.RE_EXAMINATE && entity.LastStatus != (int)PatientInPackageEnum.RE_EXAMINATE)
        //        {
        //            var serviceIds = request.SelectedServices.Select(x => x.Id);
        //            var listData = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == request.Id && !x.IsDeleted && !x.ServiceInPackage.IsDeleted && serviceIds.Contains(x.Id));
        //            foreach (var item in listData)
        //            {
        //                //TODO lưu lại thông tin service tái khám

        //                unitOfWork.PatientInPackageDetailRepository.Update(item);
        //            }
        //            //TODO	Ghi nhận doanh thu trên DIMS cho bác sĩ thực hiện dịch vụ tái khám
        //        }

        //        unitOfWork.Commit();
        //        #endregion

        //    }
        //    catch (Exception ex)
        //    {
        //        VM.Common.CustomLog.accesslog.Error(string.Format("ReOpenPatientInPackage fail. Ex: {0}", ex));
        //        //returnValue = false;
        //        returnValue = Message.INTERAL_SERVER_ERROR;
        //    }
        //    return returnValue;
        //}
        #endregion .Patient In Package General
        #region Patient In Package detail (in service)
        /// <summary>
        /// Create or update patient in package service
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CreateOrUpdatePatientInPackageDetail(PatientInPackageModel request)
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
        /// <summary>
        /// Add and update PatientInPackageDetail when have changes config service replace inside package
        /// </summary>
        /// <param name="listServiceInPackagesReplace"></param>
        /// <param name="packageId"></param>
        /// <returns></returns>
        public bool UpdatePatientInPackageDetailWhenHaveNewReplaceService(List<Guid> listServiceInPackagesReplace, Guid packageId, Guid? patienInPackageId = null)
        {
            bool returnValue = true;
            if (listServiceInPackagesReplace.Count() > 0)
            {

                #region Thêm các dịch vụ thay thế vào các gói khách hàng đã đăng ký (Trạng thái=Đăng ký, Đang sử dụng)
                //Cần cập nhật thêm item thay thế trong bảng PatientInPackageDetails
                try
                {
                    //var xQueryListReg = unitOfWork.PatientInPackageRepository.AsEnumerable().Where(x => !x.IsDeleted && x.PackagePriceSite.PackagePrice.PackageId == packageId && (x.Status == (int)PatientInPackageEnum.REGISTERED || x.Status == (int)PatientInPackageEnum.ACTIVATED));
                    IEnumerable<PatientInPackage> xQueryListReg = null;
                    if (patienInPackageId != null)
                    {
                        xQueryListReg = unitOfWork.PatientInPackageRepository.Find(x => !x.IsDeleted && x.Id == patienInPackageId.Value);
                    }
                    else
                    {
                        //29-07-2022 tungdd14: thêm điều kiện  x.Status == (int)PatientInPackageEnum.EXPIRED || x.Status == (int)PatientInPackageEnum.RE_EXAMINATE
                        //Trường hợp import dịch vụ thay thế cập nhật cho các goi ở trạng thái hết hạn và theo dói tái khám
                        xQueryListReg = unitOfWork.PatientInPackageRepository.Find(x => !x.IsDeleted && x.PackagePriceSite.PackagePrice.PackageId == packageId && (x.Status == (int)PatientInPackageEnum.REGISTERED || x.Status == (int)PatientInPackageEnum.ACTIVATED || x.Status == (int)PatientInPackageEnum.EXPIRED || x.Status == (int)PatientInPackageEnum.RE_EXAMINATE));
                    }

                    if (xQueryListReg.Any())
                    {
                        foreach (var itemReged in xQueryListReg)
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
                                        unitOfWork.PatientInPackageDetailRepository.Add(PiInPkDetail);
                                    }
                                }
                            }
                        }
                        unitOfWork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    VM.Common.CustomLog.accesslog.Error(string.Format("UpdatePatientInPackageDetailWhenHaveNewReplaceService fail. Ex: {0}", ex));
                    returnValue = false;
                }

                #endregion .Thêm các dịch vụ thay thế vào các gói khách hàng đã đăng ký (Trạng thái=Đăng ký, Đang sử dụng)
            }
            return returnValue;
        }
        /// <summary>
        /// Get detail service in policy & re calculate price/amount in package (If have discount)
        /// </summary>
        /// <param name="policyId"></param>
        /// <param name="pkgAmountAfterDiscount"></param>
        /// <returns></returns>
        public List<PatientInPackageDetailModel> GetListPatientInPackageService(Guid policyId, string pkgAmountAfterDiscount, out double netAmount, out int outStatus)
        {
            List<PatientInPackageDetailModel> entities = null;
            double outNetAmount = 0;
            int outStatusValue = 1;
            try
            {
                var policy = unitOfWork.PackagePriceRepository.FirstOrDefault(x => x.Id == policyId && !x.IsDeleted);
                if (policy != null)
                {
                    //Get list PackagePriceDetails
                    var listData = unitOfWork.PackagePriceDetailRepository.Find(x => x.PackagePriceId == policyId && !x.ServiceInPackage.IsDeleted);
                    entities = listData.OrderBy(e => e.ServiceInPackage.ServiceType).ThenBy(e => e.ServiceInPackage.IsPackageDrugConsum).ThenBy(e => e.ServiceInPackage.CreatedAt).Select(x => new PatientInPackageDetailModel()
                    {
                        ServiceInPackageId = x.ServiceInPackage.Id,
                        ServiceInPackageRootId = x.ServiceInPackage.RootId,
                        Service = x.ServiceInPackage.Service,
                        ItemsReplace = unitOfWork.ServiceInPackageRepository.Find(y => y.RootId == x.ServiceInPackage.Id).Select(z => z.Service)?.ToList(),
                        Qty = x.ServiceInPackage.LimitQty,
                        BasePrice = x.BasePrice,
                        BaseAmount = x.BaseAmount,
                        PkgPrice = x.PkgPrice,
                        PkgAmount = x.PkgAmount,
                        IsPackageDrugConsum = x.ServiceInPackage.IsPackageDrugConsum,
                        ServiceType = x.ServiceInPackage.ServiceType,
                        IsServiceFreeInPackage = unitOfWork.ServiceFreeInPackageRepository.Find(y => y.ServiceId == x.ServiceInPackage.ServiceId && y.GroupCode == policy.Package.PackageGroup.Code && !y.IsDeleted).Any(),
                    })?.ToList();
                    if (policy.Amount?.ToString() != pkgAmountAfterDiscount && !string.IsNullOrEmpty(pkgAmountAfterDiscount))
                    {
                        double dPkgAmountAfterDiscount = 0;
                        double.TryParse(pkgAmountAfterDiscount, out dPkgAmountAfterDiscount);
                        if (dPkgAmountAfterDiscount >= 0)
                        {
                            #region Set price & amount in package
                            CalculateDetailPatientService(policy, entities, dPkgAmountAfterDiscount, out outStatusValue);
                            #endregion .Set price & amount in package
                            #region Check and re-set pkgPrice first item, limit qty=1
                            var TotalPkgAmount = entities?.Sum(x => x.PkgAmount);
                            if (dPkgAmountAfterDiscount != TotalPkgAmount)
                            {
                                //Reset pkgPrice for first item with qty=1
                                var firstItem = entities.Where(x => x.Qty == 1 && !x.IsServiceFreeInPackage).FirstOrDefault();
                                if (firstItem != null)
                                {
                                    var pkgAmount_NotFirst = entities?.Where(x => x != firstItem).Sum(x => x.PkgAmount);
                                    if (pkgAmount_NotFirst != null)
                                    {
                                        var pkgAmount_FirstItem = dPkgAmountAfterDiscount - pkgAmount_NotFirst;
                                        var pkgPrice_FirstItem = Math.Round(pkgAmount_FirstItem.Value / firstItem.Qty.Value);
                                        if (pkgAmount_FirstItem != null)
                                        {
                                            firstItem.PkgPrice = pkgPrice_FirstItem;
                                            firstItem.PkgAmount = firstItem.PkgPrice * firstItem.Qty;
                                        }
                                    }
                                }
                            }
                            #endregion Check and re-set pkgPrice first item, limit qty=1
                            outNetAmount = dPkgAmountAfterDiscount;
                        }
                        else
                        {
                            outNetAmount = policy.Amount != null ? policy.Amount.Value : 0;
                        }
                        netAmount = outNetAmount;
                        outStatus = outStatusValue;
                        return entities;
                    }
                    else
                    {
                        outNetAmount = policy.Amount != null ? policy.Amount.Value : 0;
                        netAmount = outNetAmount;
                        outStatus = outStatusValue;
                        return entities;
                    }
                }

            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("GetListPatientInPackageService fail. Ex: {0}", ex));
            }
            netAmount = outNetAmount;
            outStatus = outStatusValue;
            return entities;
        }
        /// <summary>
        /// Get detail service's using status
        /// </summary>
        /// <param name="patientInPackageId"></param>
        /// <returns></returns>
        public List<PatientInPackageServiceUsingStatusModel> GetListPatientInPackageServiceUsing(Guid patientInPackageId)
        {
            List<PatientInPackageServiceUsingStatusModel> entities = null;
            try
            {
                var entity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientInPackageId);
                if (entity == null)
                {
                    return null;
                }
                //Lấy dữ liệu thống kê tình hình sử dụng gói từ dữ liệu Capture
                else if (listStatusIGNOREUpdateStatUsing.Contains(entity.Status) && !string.IsNullOrEmpty(entity.DataStatUsing))
                {
                    try
                    {
                        entities = JsonConvert.DeserializeObject<List<PatientInPackageServiceUsingStatusModel>>(entity.DataStatUsing);
                        if (entities?.Count > 0)
                            return entities;
                    }
                    catch (Exception ex)
                    {
                        VM.Common.CustomLog.accesslog.Error(string.Format("GetListPatientInPackageServiceUsing.GetFromCapture fail. Ex: {0}", ex));
                    }
                }
                //Get list PackagePriceDetails
                //07-22-2022 tungdd14: Chuyển điều kiện x => x.ServiceInPackage.RootId == null xuống dưới
                var listData = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientInPackageId && !x.IsDeleted && !x.ServiceInPackage.IsDeleted);
                var listServiceInpackageId = listData.Select(x => x.ServiceInPackage.Id).ToList();
                entities = listData.Where(x => x.ServiceInPackage.RootId == null).OrderBy(x => x.ServiceInPackage.ServiceType).ThenBy(x => x.ServiceInPackage.IsPackageDrugConsum).ThenBy(x => x.ServiceInPackage.CreatedAt).Select(x => new PatientInPackageServiceUsingStatusModel()
                {
                    Id = x.Id,
                    ServiceInPackageId = x.ServiceInPackage.Id,
                    ServiceCode = x.ServiceInPackage.Service.Code,
                    ServiceName = x.ServiceInPackage.Service.ViName,
                    Qty = x.ServiceInPackage.LimitQty,
                    PkgPrice = x.PkgPrice,
                    IsPackageDrugConsum = x.ServiceInPackage.IsPackageDrugConsum,
                    ServiceType = x.ServiceInPackage.ServiceType,
                    QtyWasUsed = 0,
                    QtyNotUsedYet = x.ServiceInPackage.LimitQty,
                    AmountNotUsedYet = x.ServiceInPackage.LimitQty * x.PkgPrice,
                    QtyOver = 0,
                    //07-22-2022 tungdd14: Kiểm tra nếu dịch vụ thay thế được insert vào PatientInPackageDetail thì mới hiển thị
                    ItemsReplace = unitOfWork.ServiceInPackageRepository.Find(y => y.RootId == x.ServiceInPackageId && listServiceInpackageId.Contains(y.Id))?.Select(y => new { ServiceInPackageId = y.Id, y.ServiceId, ServiceName = y.Service?.ViName, ServiceCode = y.Service?.Code }),
                    //tungdd14 bổ xung phần dịch vụ tái khám
                    IsReExamService = x.ServiceInPackage.IsReExamService,
                    QtyReExamWasUsed = 0,
                    QtyReExamNotUsed = 0,
                    QtyReExamOver = 0,
                    ReExamQtyLimit = x.ReExamQtyLimit
                })?.ToList();
                #region Stat current using service in package
                if (entities?.Count > 0)
                {
                    var listHisChargeDetail = unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageId == patientInPackageId && !x.IsDeleted && !x.PatientInPackageDetail.ServiceInPackage.IsDeleted);
                    if (listHisChargeDetail.Any())
                    {
                        foreach (var item in listHisChargeDetail)
                        {
                            var itemX = entities.Find(x => x.ServiceCode == item.PatientInPackageDetail.ServiceInPackage.Service.Code || item.PatientInPackageDetail.ServiceInPackage.RootId == x.ServiceInPackageId);
                            if (itemX != null)
                            {
                                if (item.InPackageType == (int)InPackageType.INPACKAGE)
                                {
                                    //Cập nhật thông tin trong gói
                                    //tungdd14 check thêm ChargeIsUseForReExam với charge khác tái khám hoặc tái khám
                                    itemX.QtyWasUsed = (itemX.QtyWasUsed == null ? 0 : itemX.QtyWasUsed) + (item.Quantity != null && !item.ChargeIsUseForReExam ? item.Quantity : 0);
                                    itemX.QtyReExamWasUsed = (itemX.QtyReExamWasUsed == null ? 0 : itemX.QtyReExamWasUsed) + (item.Quantity != null && item.ChargeIsUseForReExam ? item.Quantity : 0);
                                    //Số lượng Sdung luôn bằng định mức
                                    //itemX.QtyWasUsed = itemX.QtyWasUsed > itemX.Qty ? itemX.Qty : itemX.QtyWasUsed;
                                    itemX.QtyWasInvoiced = (itemX.QtyWasInvoiced == null ? 0 : itemX.QtyWasInvoiced) + (item.Quantity != null && item.HISCharge.InvoicePaymentStatus == Constant.PAYMENT_PSL_STATUS && !item.ChargeIsUseForReExam ? item.Quantity : 0);
                                    itemX.QtyReExamWasInvoiced = (itemX.QtyReExamWasInvoiced == null ? 0 : itemX.QtyReExamWasInvoiced) + (item.Quantity != null && item.HISCharge.InvoicePaymentStatus == Constant.PAYMENT_PSL_STATUS && item.ChargeIsUseForReExam ? item.Quantity : 0);
                                    itemX.QtyNotUsedYet = (itemX.Qty == null ? 0 : itemX.Qty) - (itemX.QtyWasUsed == null ? 0 : itemX.QtyWasUsed);
                                    itemX.AmountWasUsed = itemX.QtyWasUsed * itemX.PkgPrice;
                                    itemX.AmountNotUsedYet = itemX.QtyNotUsedYet * itemX.PkgPrice;
                                    //tungdd14 tính lại số lượng tám khám còn lại
                                    itemX.QtyReExamNotUsed = (itemX.ReExamQtyLimit == null ? 0 : itemX.ReExamQtyLimit) - (itemX.QtyReExamWasUsed == null ? 0 : itemX.QtyReExamWasUsed);
                                    //check nếu QtyReExamNotUsed < 0 thì set là 0
                                    itemX.QtyReExamNotUsed = itemX.QtyReExamNotUsed > 0 ? itemX.QtyReExamNotUsed : 0;
                                }
                                else if (item.InPackageType == (int)InPackageType.OVERPACKAGE)
                                {
                                    //Cập nhật thông tin vượt gói
                                    itemX.QtyOver = (itemX.QtyOver == null ? 0 : itemX.QtyOver) + (item.Quantity != null && !item.ChargeIsUseForReExam ? item.Quantity : 0);
                                    itemX.QtyReExamOver = (itemX.QtyReExamOver == null ? 0 : itemX.QtyReExamOver) + (item.Quantity != null && item.ChargeIsUseForReExam ? item.Quantity : 0);
                                }
                                //tungdd14 trường hủy, move chỉ định tái khám lần 1
                                else if (item.InPackageType == (int)InPackageType.CHARGE_CANCELLED || item.InPackageType == (int)InPackageType.CHARGE_MOVEOUTPACKGE)
                                {
                                    itemX.QtyReExamWasInvoiced = (itemX.QtyReExamWasInvoiced == null ? 0 : itemX.QtyReExamWasInvoiced) + (item.Quantity != null && item.HISCharge.InvoicePaymentStatus == Constant.PAYMENT_PSL_STATUS && item.ChargeIsUseForReExam ? item.Quantity : 0);
                                    //tungdd14 tính lại số lượng tám khám còn lại
                                    itemX.QtyReExamNotUsed = (itemX.ReExamQtyLimit == null ? 0 : itemX.ReExamQtyLimit) - (itemX.QtyReExamWasUsed == null ? 0 : itemX.QtyReExamWasUsed);
                                    //check nếu QtyReExamNotUsed < 0 thì set là 0
                                    itemX.QtyReExamNotUsed = itemX.QtyReExamNotUsed > 0 ? itemX.QtyReExamNotUsed : 0;
                                }
                            }
                        }
                    }
                }
                #endregion .Stat current using service in package

                if (entities != null && entities.Count > 0)
                {
                    #region Add more Total Row
                    double? total_AmountWasUsed = entities.Sum(x => x.AmountWasUsed);
                    double? total_AmountNotUsedYet = entities.Sum(x => x.AmountNotUsedYet);
                    entities.Add(new PatientInPackageServiceUsingStatusModel() { ServiceType = 0, AmountWasUsed = total_AmountWasUsed, AmountNotUsedYet = total_AmountNotUsedYet });
                    #endregion .Add more Total Row
                    return entities;
                }
                else
                {
                    return entities;
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("GetListPatientInPackageServiceUsing fail. Ex: {0}", ex));
            }
            return entities;
        }
        /// <summary>
        /// Get Patient in package visit list (Danh sách lượt khám trong gói)
        /// </summary>
        /// <param name="patientInPackageId"></param>
        /// <returns></returns>
        public List<PatientInPackageVisitModel> GetListPatientInPackageVisit(Guid patientInPackageId)
        {
            List<PatientInPackageVisitModel> entities = null;
            try
            {
                //Get list PackagePriceDetails
                //tungdd14: thêm điều kiện Constant.VISIT_TYPE_PACKAGES.Contains(x.VisitType)
                //do đã comment code set patientInPackageId = null khi là dịch vụ ngoài gói
                var listData = unitOfWork.HISChargeRepository.Find(x => x.PatientInPackageId == patientInPackageId && Constant.ChargeStatusAvailable.Contains(x.ChargeStatus) && !x.IsDeleted && Constant.VISIT_TYPE_PACKAGES.Contains(x.VisitType));
                //tungdd14: thêm điều kiện lọc chỉ hiển thị nếu có ít nhất 1 chỉ định trong gói
                var listHisChargeDetail = unitOfWork.HISChargeDetailRepository.Find(x => !x.IsDeleted && listInPackageTypeAllowUpdateOrginalPriceWhenPricingClass.Contains(x.InPackageType)).Select(x => x.HisChargeId).ToList();
                listData = listData.Where(x => listHisChargeDetail.Contains(x.Id));
                entities = listData.GroupBy(x => new { x.VisitCode, x.VisitDate, x.PID, x.CustomerName, x.HospitalCode }).OrderByDescending(x => x.Key.VisitDate).Select(x => new PatientInPackageVisitModel()
                {
                    SiteCode = x.Key.HospitalCode,
                    PID = x.Key.PID,
                    PatientName = x.Key.CustomerName,
                    VisitCode = x.Key.VisitCode,
                    VisitDate = x.Key.VisitDate?.ToString(Constant.DATE_FORMAT)
                })?.ToList();
                var sites = unitOfWork.SiteRepository.AsQueryable();
                var xQuery = (from a in entities
                              join b in sites on a.SiteCode equals b.Code into bx
                              from b in bx.DefaultIfEmpty()
                              select new PatientInPackageVisitModel()
                              {
                                  SiteCode = a.SiteCode,
                                  SiteName = b.Name,
                                  PID = a.PID,
                                  PatientName = a.PatientName,
                                  VisitCode = a.VisitCode,
                                  VisitDate = a.VisitDate
                              });
                if (xQuery.Any())
                {
                    entities = xQuery/*.OrderByDescending(x=>x.VisitDate)*/.ToList();
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("GetListPatientInPackageVisit fail. Ex: {0}", ex));
            }
            return entities;
        }
        /// <summary>
        /// Mapping charge into Service in Package of Patient
        /// </summary>
        /// <param name="patientInPackageId"></param>
        /// <param name="pid"></param>
        /// <param name="visitCode"></param>
        /// <returns></returns>
        public List<ChargeInPackageModel> MappingChargeIntoServiceInPackage(ConfirmServiceInPackageModel model, Guid patientInPackageId, string pid, List<string> ChargeUnCheckeds, string visitCode = "")
        {
            List<ChargeInPackageModel> entities = null;
            List<string> listPID = new List<string>() { pid };
            try
            {
                //tungdd14 thêm kiểm tra có phải trạng thái tái khám không
                var patientInPackage = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == patientInPackageId);
                bool IsReExamService = false;
                if (patientInPackage.Status == (int)PatientInPackageEnum.RE_EXAMINATE || (patientInPackage.Status == (int)PatientInPackageEnum.EXPIRED && patientInPackage.LastStatus == (int)PatientInPackageEnum.RE_EXAMINATE))
                {
                    IsReExamService = true;
                }
                List<HISChargeModel> oHEntities = null;
                oHEntities = OHConnectionAPI.GetCharges(pid, visitCode, string.Empty, IsIncludeChild: model.IsIncludeChild, Children: model.Children);
                //Get Charge what is Maternity/MCR (Is include child) package
                #region Get Charge what is Maternity package
                //if (model.IsMaternityPackage && model.Children?.Count > 0)
                if (model.IsIncludeChild && model.Children?.Count > 0)
                {
                    //Get List Charge by child 
                    //if (oHEntities == null) oHEntities = new List<HISChargeModel>();
                    foreach (var itemChild in model.Children)
                    {
                        listPID.Add(itemChild.PID);
                        //oHEntities.AddRange(OHConnectionAPI.GetCharges(itemChild.PID, string.Empty, string.Empty));
                    }
                }
                #endregion .Get Charge what is Maternity package
                if (oHEntities?.Count > 0)
                {
                    //Store Charge into Database
                    foreach (var item in oHEntities)
                    {
                        //item.PatientInPackageId = patientInPackageId;
                        CreateOrUpdateHisCharge(item);
                    }
                    //tungdd14 update thêm cả charges ở ngoài gói
                    var XhisCharges = unitOfWork.HISChargeRepository.Find(x => !x.IsDeleted && x.PID == pid && Constant.ChargeStatusAvailable.Contains(x.ChargeStatus) && x.InvoicePaymentStatus != Constant.PAYMENT_PSL_STATUS && x.ChargeDate <= model.EndDateFull);
                    if (XhisCharges.Any())
                    {
                        var oHEntitiesChargeOutPackage = OHConnectionAPI.GetCharges(pid, string.Empty, string.Join(";", XhisCharges?.Where(e => e.PID == pid).Select(x => x.ChargeId)?.ToList()));
                        if (oHEntitiesChargeOutPackage?.Count > 0)
                        {
                            //update HisCharge với các visit ngoài gói
                            foreach (var item in oHEntitiesChargeOutPackage)
                            {
                                CreateOrUpdateHisCharge(item);
                            }
                        }
                    }
                    if (model != null)
                    {
                        //Sau bo doan multi visit
                        List<string> visities = new List<string>();
                        visities = oHEntities.GroupBy(x => x.VisitCode).Select(x => x.Key).ToList();
                        if (visities?.Count > 0)
                        {
                            model.Visits = new List<VisitModel>();
                            foreach (var item in visities)
                            {
                                var fItem = oHEntities.FirstOrDefault(x => x.VisitCode == item);
                                ////Set for master
                                //model.VisitCode = oHEntities[0].VisitCode;
                                //model.VisitDate = oHEntities[0].VisitDate?.ToString(Constant.DATE_FORMAT);
                                string strSiteCode = fItem?.HospitalCode;
                                string siteCode = string.Empty;
                                string siteName = string.Empty;
                                var hosEntity = unitOfWork.SiteRepository.FirstOrDefault(x => x.Code == strSiteCode);
                                if (hosEntity != null)
                                {
                                    siteCode = hosEntity.Code;
                                    siteName = hosEntity.Name;
                                }
                                model.Visits.Add(new VisitModel()
                                {
                                    PID = fItem?.PID,
                                    PatientName = fItem?.CustomerName,
                                    SiteCode = siteCode,
                                    SiteName = siteName,
                                    VisitCode = fItem?.VisitCode,
                                    VisitDate = fItem?.VisitDate?.ToString(Constant.DATE_FORMAT)
                                });
                            }
                        }
                        //Single Visit
                        //var listCharge = unitOfWork.HISChargeRepository.Find(x => x.PID == model.PID && x.VisitCode== model.VisitCode && Constant.ChargeStatusAvailable.Contains(x.ChargeStatus) && x.InvoicePaymentStatus!=Constant.PAYMENT_PSL_STATUS && x.ChargeDate<= model.EndDateFull).OrderByDescending(x=>x.ChargeDate).AsEnumerable();
                        //Multi Visit
                        var listCharge = unitOfWork.HISChargeRepository.Find(x => listPID.Contains(x.PID) && visities.Contains(x.VisitCode) && Constant.ChargeStatusAvailable.Contains(x.ChargeStatus) && x.InvoicePaymentStatus != Constant.PAYMENT_PSL_STATUS && x.ChargeDate <= model.EndDateFull).OrderByDescending(x => x.ChargeDate)/*.AsEnumerable()*/;
                        //Get List Service InPackage of patient
                        var listData = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientInPackageId && !x.ServiceInPackage.IsDeleted);
                        //tungdd14 Thêm trường hợp tái khám chỉ load lên service có chỉ định tái khám
                        var listServiceInPackageReExam = unitOfWork.ServiceInPackageRepository.Find(x => x.IsReExamService).Select(x => x?.Id).ToList();
                        if (IsReExamService)
                        {
                            listData = listData.Where(x => x.ServiceInPackage.IsReExamService || listServiceInPackageReExam.Contains(x.ServiceInPackage.RootId));
                        }
                        if (listData.Any())
                        {
                            //var listCharDB = listCharge?.ToList();
                            entities = listData.OrderBy(x => x.ServiceInPackage.ServiceType).ThenBy(x => x.ServiceInPackage.IsPackageDrugConsum).ThenBy(x => x.ServiceInPackage.CreatedAt).Select(x => new ChargeInPackageModel()
                            {
                                ServiceInpackageId = x.ServiceInPackage.Id,
                                ServiceType = x.ServiceInPackage.ServiceType,
                                PatientInPackageId = patientInPackageId,
                                PatientInPackageDetailId = x.Id,
                                ServiceCode = x.ServiceInPackage.Service.Code,
                                ServiceName = x.ServiceInPackage.Service.ViName,
                                QtyCharged = 0,
                                QtyRemain = x.QtyRemain,
                                Price = x.PkgPrice,
                                RootId = x.ServiceInPackage.RootId,
                                ReExamQtyRemain = x.ReExamQtyRemain,
                                ServiceUseForReExam = CheckServiceReExam(listData, x.ServiceInPackageId)
                            })?.ToList();
                            var listChargeInPackage = unitOfWork.HISChargeDetailRepository.Find(x => !x.PatientInPackageDetail.ServiceInPackage.IsDeleted && !x.IsDeleted && !Constant.ListStatusCancel_Terminated.Contains(x.PatientInPackageDetail.PatientInPackage.Status))/*.AsEnumerable()*/;
                            var listTempServiceInPackage = entities.GetRange(0, entities.Count);
                            //var listE = listCharge?.ToList();
                            //Mapping charge into service inpackage
                            var xquery = (from a in entities
                                          join b in listCharge on a.ServiceCode equals b.ItemCode into bx
                                          from b in bx.AsEnumerable()
                                          join c in listChargeInPackage on b?.Id equals c.HisChargeId into cx
                                          from c in cx.DefaultIfEmpty()
                                              //where !cx.Any(x=>(x.PatientInPackageId== patientInPackageId && x.InPackageType != (int)InPackageType.QTYINCHARGEGREATTHANREMAIN) || (x.PatientInPackageId!= patientInPackageId && x.InPackageType!= (int)InPackageType.QTYINCHARGEGREATTHANREMAIN && x.PatientInPackage.Status!=(int)PatientInPackageEnum.ACTIVATED))
                                          select new ChargeInPackageModel()
                                          {
                                              ChargeId = b?.ChargeId,
                                              HisChargeId = b?.Id,
                                              PatientInPackageId = c?.PatientInPackageId == null ? a.PatientInPackageId : c?.PatientInPackageId,
                                              PatientInPackageStatus = c?.PatientInPackage?.Status,
                                              PatientInPackageLastStatus = c?.PatientInPackage?.LastStatus,
                                              PatientInPackageDetailId = a.PatientInPackageDetailId,
                                              ServiceInpackageId = a.ServiceInpackageId,
                                              ServiceType = a.ServiceType,
                                              ChargeDateTime = b?.ChargeDate,
                                              ChargeDate = b?.ChargeDate?.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND),
                                              //Thông tin người sử dụng
                                              PID = b?.PID,
                                              PatientName = b?.CustomerName,
                                              InPackageType = c?.InPackageType != null ? c.InPackageType : 0,
                                              ServiceCode = a.ServiceCode,
                                              ServiceName = a.ServiceName,
                                              QtyCharged = b?.Quantity,
                                              QtyRemain = a.QtyRemain,
                                              Price = b?.UnitPrice,
                                              PkgPrice = a.Price,
                                              IsChecked = (b?.ChargeId != null && c?.Id == null || (c?.Id != null && c?.PatientInPackageId == patientInPackageId)),
                                              //infor for notes
                                              WasPackageId = c?.PatientInPackageDetail.ServiceInPackage.Package.Id,
                                              WasPackageCode = c?.PatientInPackageDetail.ServiceInPackage.Package.Code,
                                              WasPackageName = c?.PatientInPackageDetail.ServiceInPackage.Package.Name,
                                              RootId = a.RootId,
                                              //21-07-2022: tungdd14 thêm trường check là chỉ đỉnh tái khám
                                              ChargeIsUseForReExam = c != null ? c.ChargeIsUseForReExam : false,
                                              ServiceUseForReExam = !a.ServiceUseForReExam ? (c != null ? (c.PatientInPackageDetail.ServiceInPackage.RootId == null ? c.PatientInPackageDetail.ServiceInPackage.IsReExamService : listServiceInPackageReExam.Contains(c.PatientInPackageDetail.ServiceInPackage.RootId)) : false) : a.ServiceUseForReExam,
                                              //tungdd14 thêm điều kiện check chỉ định trong gói
                                              VisitType = b.VisitType,
                                              ReExamQtyRemain = a?.ReExamQtyRemain
                                          });
                            //tungdd14: thêm (int)InPackageType.CHARGE_MOVEOUTPACKGE
                            //Trường hợp chỉ định move ra ngoài gói rồi move vào gói áp dụng giá gói, số lượng còn lại bằng 0 không lên vượt gói
                            List<int> listInPackageTypeAvailable = new List<int>() { 0, (int)InPackageType.QTYINCHARGEGREATTHANREMAIN, (int)InPackageType.CHARGE_MOVEOUTPACKGE };
                            List<int> listInPackageTypeOverOutSideInvalid = new List<int>() { (int)InPackageType.OVERPACKAGE, (int)InPackageType.OUTSIDEPACKAGE, (int)InPackageType.QTYINCHARGEGREATTHANREMAIN };
                            entities = xquery?.ToList();
                            if (ChargeUnCheckeds?.Count > 0)
                                entities = entities.Where(x => (x.ChargeId == null || (x.ChargeId != null && !ChargeUnCheckeds.Any(y => y.ToLower() == x.ChargeId.Value.ToString().ToLower()))))?.ToList();
                            //Cập nhật lại QtyRemain
                            //listTempServiceInPackage.ForEach(x => x.QtyRemain = x.QtyRemain - entities.Where(y => y.ServiceCode == x.ServiceCode && y.InPackageType == (int)InPackageType.INPACKAGE && y.PatientInPackageId== patientInPackageId).Sum(y => y.QtyCharged));
                            //Cập nhật lại QtyRemain
                            //entities.ForEach(x=>x.QtyRemain= x.QtyRemain-entities.Where(y=>y.ServiceCode==x.ServiceCode && y.InPackageType==(int)InPackageType.INPACKAGE && y.PatientInPackageId == patientInPackageId).Sum(y=>y.QtyRemain));
                            //tungdd14 chuyển điều kiện && Constant.VISIT_TYPE_PACKAGES.Contains(x.VisitType) 
                            //Lọc những chỉ định ngoài gói
                            entities = entities.Where(x => (x.PatientInPackageId == patientInPackageId && Constant.VISIT_TYPE_PACKAGES.Contains(x.VisitType) && (IsReExamService ? x.ServiceUseForReExam : true) && (listInPackageTypeAvailable.Contains(x.InPackageType) || (((IsReExamService && x.ServiceUseForReExam) ? x.ReExamQtyRemain : x.QtyRemain) > 0 && (x.InPackageType == (int)InPackageType.OVERPACKAGE || x.InPackageType == (int)InPackageType.OUTSIDEPACKAGE))) && x.PatientInPackageStatus != (int)PatientInPackageEnum.CLOSED)
                            || (x.PatientInPackageId != patientInPackageId && (x.PatientInPackageStatus == (int)PatientInPackageEnum.ACTIVATED || (x.PatientInPackageStatus == (int)PatientInPackageEnum.EXPIRED && x.PatientInPackageLastStatus != (int)PatientInPackageEnum.RE_EXAMINATE)))
                            || (x.PatientInPackageId != patientInPackageId && listInPackageTypeOverOutSideInvalid.Contains(x.InPackageType) && !IsReExamService)
                            //tungdd14: trường hợp tái khám
                            || (x.PatientInPackageId != patientInPackageId && (x.PatientInPackageStatus == (int)PatientInPackageEnum.RE_EXAMINATE || (x.PatientInPackageStatus == (int)PatientInPackageEnum.EXPIRED && x.PatientInPackageLastStatus == (int)PatientInPackageEnum.RE_EXAMINATE)) && ((!x.ServiceUseForReExam && x.InPackageType == (int)InPackageType.INPACKAGE) || x.ChargeIsUseForReExam))
                            )?.OrderBy(x => x.ChargeDateTime).ToList();

                            if (entities?.Count > 0)
                            {
                                #region Rebuild to split In package or over package
                                List<ChargeInPackageModel> returnEntities = new List<ChargeInPackageModel>();

                                //if (listChargeInPackage.Any())
                                //{
                                //    //returnEntities = entities;
                                //}
                                foreach (var item in entities)
                                {
                                    //tungdd14: check với trường hợp là tái khám
                                    int? QtyRemain = 0;
                                    if (IsReExamService)
                                    {
                                        QtyRemain = listTempServiceInPackage.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).Select(x => x.ReExamQtyRemain)?.FirstOrDefault();
                                    }
                                    else
                                    {
                                        QtyRemain = listTempServiceInPackage.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).Select(x => x.QtyRemain)?.FirstOrDefault();
                                    }
                                    
                                    //var QtyRemain = item.QtyRemain;
                                    if (item.QtyCharged > QtyRemain && QtyRemain == 0
                                        && (item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE || (model.IsLimitedDrugConsum && item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM)))
                                    {
                                        //Create new item over
                                        ChargeInPackageModel overEntity = (ChargeInPackageModel)item.Clone();
                                        overEntity.InPackageType = (int)InPackageType.OVERPACKAGE;
                                        overEntity.QtyRemain = 0;
                                        overEntity.QtyCharged = item.QtyCharged - QtyRemain;
                                        //Set bằng giá lẻ
                                        overEntity.Price = item.Price;
                                        overEntity.Amount = overEntity.Price * overEntity.QtyCharged;
                                        if (item.PatientInPackageStatus != (int)PatientInPackageEnum.CLOSED)
                                        {
                                            SetNotes4ConfirmServiceBelongPackage(model.PID, overEntity, patientInPackageId);
                                        }
                                        else
                                        {
                                            overEntity.IsChecked = true;
                                        }
                                        #region Comment old rule
                                        //if (QtyRemain == 1)
                                        //{
                                        //    //Set entity in package
                                        //    item.InPackageType = (int)InPackageType.INPACKAGE;
                                        //    //Set bằng giá gói
                                        //    item.Price = item.PkgPrice;
                                        //    item.QtyCharged = QtyRemain;
                                        //    item.Amount = item.Price * QtyRemain;
                                        //    SetNotes4ConfirmServiceBelongPackage(item, patientInPackageId);
                                        //    //Add item in package
                                        //    returnEntities.Add(item);
                                        //}
                                        #endregion .Comment old rule
                                        //Add item over
                                        returnEntities.Add(overEntity);
                                        //Cập nhật số lượng còn lại
                                        //kiểm tra nếu là tái khám thì cập nhật lại ReExamQtyRemain
                                        if (IsReExamService)
                                        {
                                            listTempServiceInPackage.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).ToList().ForEach(x => x.ReExamQtyRemain = x.ReExamQtyRemain >= item.QtyCharged ? x.ReExamQtyRemain - item.QtyCharged : 0);
                                        }
                                        else
                                        {
                                            listTempServiceInPackage.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).ToList().ForEach(x => x.QtyRemain = x.QtyRemain >= item.QtyCharged ? x.QtyRemain - item.QtyCharged : 0);
                                        }
                                    }
                                    else if (item.QtyCharged > QtyRemain && QtyRemain != 0
                                        && (item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE || (model.IsLimitedDrugConsum && item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM)))
                                    {
                                        //Create new item Charge invalid qtycharge
                                        ChargeInPackageModel InvalidEntity = (ChargeInPackageModel)item.Clone();
                                        InvalidEntity.InPackageType = (int)InPackageType.QTYINCHARGEGREATTHANREMAIN;
                                        InvalidEntity.QtyRemain = QtyRemain;
                                        InvalidEntity.QtyCharged = item.QtyCharged;
                                        //Set bằng giá lẻ
                                        InvalidEntity.Price = 0;
                                        InvalidEntity.Amount = InvalidEntity.Price * InvalidEntity.QtyCharged;
                                        if (item.PatientInPackageStatus != (int)PatientInPackageEnum.CLOSED)
                                        {
                                            SetNotes4ConfirmServiceBelongPackage(model.PID, InvalidEntity, patientInPackageId);
                                        }
                                        InvalidEntity.IsChecked = false;
                                        returnEntities.Add(InvalidEntity);
                                    }
                                    else
                                    {
                                        int? currentInPackageType = item.InPackageType;
                                        item.InPackageType = (int)InPackageType.INPACKAGE;
                                        item.QtyRemain = QtyRemain;
                                        //tungdd14 trường hợp tái khám trong gói cập nhật giá = 0
                                        item.Price = IsReExamService ? 0 : item.PkgPrice;
                                        item.Amount = item.Price * item.QtyCharged;
                                        if (item.PatientInPackageStatus != (int)PatientInPackageEnum.CLOSED)
                                        {
                                            //Build Notes
                                            SetNotes4ConfirmServiceBelongPackage(model.PID, item, patientInPackageId, currentInPackageType);
                                        }
                                        else
                                        {
                                            item.IsChecked = true;
                                        }
                                        returnEntities.Add(item);
                                        //Cập nhật số lượng còn lại
                                        //kiểm tra nếu là tái khám thì cập nhật lại ReExamQtyRemain
                                        if (IsReExamService)
                                        {
                                            listTempServiceInPackage.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).ToList().ForEach(x => x.ReExamQtyRemain = x.ReExamQtyRemain >= item.QtyCharged ? x.ReExamQtyRemain - item.QtyCharged : 0);
                                        }
                                        else
                                        {
                                            listTempServiceInPackage.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).ToList().ForEach(x => x.QtyRemain = x.QtyRemain >= item.QtyCharged ? x.QtyRemain - item.QtyCharged : 0);
                                        }
                                    }
                                }
                                entities = returnEntities;
                                #endregion .Rebuild to split In package or over package
                            }
                            model.listCharge = entities;
                            #region Filter Visits show when have charge did not inside PatientInPackage
                            model.Visits = model.Visits?.Where(x => entities.Any(e => e.PID == x.PID))?.ToList();
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("MappingChargeIntoServiceInPackage fail. Ex: {0}", ex));
            }
            return entities;
        }

        public bool CheckServiceReExam(IEnumerable<PatientInPackageDetail> listPatientInpackageDetail, Guid? serviceInpackageId)
        {
            if (serviceInpackageId == null)
            {
                return false;
            }
            var patientInpackageDetail = listPatientInpackageDetail.FirstOrDefault(x => x.ServiceInPackageId == serviceInpackageId);
            if (patientInpackageDetail != null)
            {
                //Trường hợp dịch vụ hiện tại là tái khám và có slsd > 0
                if (patientInpackageDetail.ServiceInPackage.IsReExamService && patientInpackageDetail.ReExamQtyLimit > 0)
                {
                    return true;
                }
                //Trường hợp các dịch vụ thay thế có slsd > 0
                var listPatientInpackageChild = listPatientInpackageDetail.Where(x => x.ServiceInPackage.RootId == serviceInpackageId);
                if (listPatientInpackageChild != null && patientInpackageDetail.ServiceInPackage.IsReExamService && listPatientInpackageChild.Sum(x => x.ReExamQtyLimit) > 0)
                {
                    return true;
                }
                //Trường hợp là dịch vụ thay thế
                if (patientInpackageDetail.ServiceInPackage.RootId != null)
                {
                    //Trường hợp dịch vụ chính là tái khám và có slsd > 0
                    var patientInpackageDetailParent = listPatientInpackageDetail.FirstOrDefault(x => x.ServiceInPackageId == patientInpackageDetail.ServiceInPackage.RootId);
                    if (patientInpackageDetailParent.ServiceInPackage.IsReExamService && (patientInpackageDetailParent.ReExamQtyLimit > 0 || patientInpackageDetail.ReExamQtyLimit > 0))
                    {
                        return true;
                    }
                    //Trường hợp các dịch vụ thay thế cùng dịch vụ chính có slsd > 0
                    var patientInpackageDetailTogetherParent = listPatientInpackageDetail.Where(x => x.ServiceInPackage.RootId == patientInpackageDetail.ServiceInPackage.RootId);
                    if (patientInpackageDetailParent.ServiceInPackage.IsReExamService && (patientInpackageDetailParent.ReExamQtyLimit > 0 || patientInpackageDetailTogetherParent.Sum(x => x.QtyWasUsed) > 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public List<ChargeInPackageModel> MappingChargeIntoServiceInPackageTransferred(PatientInPackageTransferredModel model, string pid, string visitCode = "")
        {
            List<ChargeInPackageModel> entities = null;
            try
            {
                #region For statistic performance
                var start_time = DateTime.Now;
                var start_time_total = DateTime.Now;
                TimeSpan tp;
                #endregion .For statistic performance
                //Get list charge in current visit package
                List<HISChargeModel> oHEntities = OHConnectionAPI.GetCharges(pid, visitCode, string.Empty, IsIncludeChild: model.IsIncludeChild, Children: model.Children);
                //Get list charge in old package
                #region Get list charge in old package

                //var XhisCharges = unitOfWork.HISChargeRepository.AsEnumerable().Where(x => !x.IsDeleted && x.PatientInPackageId == model.OldPatientInPackageId);
                var XhisCharges = unitOfWork.HISChargeRepository.Find(x => !x.IsDeleted && x.PatientInPackageId == model.OldPatientInPackageId);
                //var XhisChargeDetails = unitOfWork.HISChargeDetailRepository.AsEnumerable().Where(x => !x.IsDeleted && x.PatientInPackageId == model.OldPatientInPackageId && x.InPackageType==(int)InPackageType.INPACKAGE);
                var XhisChargeDetails = unitOfWork.HISChargeDetailRepository.Find(x => !x.IsDeleted && x.PatientInPackageId == model.OldPatientInPackageId && x.InPackageType == (int)InPackageType.INPACKAGE);
                var xhisChargeInPackage = (from a in XhisCharges
                                           join b in XhisChargeDetails on a.Id equals b.HisChargeId into bx
                                           from b in bx.AsEnumerable()
                                           select new HISChargeModel()
                                           {
                                               PID = a.PID,
                                               ChargeId = a.ChargeId,
                                               UnitPrice = b.ChargePrice,
                                           });
                List<HISChargeModel> listChargeInOldPackage = null;
                if (xhisChargeInPackage.Any())
                {
                    listChargeInOldPackage = xhisChargeInPackage?.ToList();
                }
                if (listChargeInOldPackage?.Count > 0)
                {
                    List<HISChargeModel> listOldCharge = null;
                    listOldCharge = OHConnectionAPI.GetCharges(pid, string.Empty, string.Join(";", listChargeInOldPackage?.Where(e => e.PID == pid).Select(x => x.ChargeId)?.ToList()));
                    #region Get Charge what is Maternity package
                    //if (model.IsMaternityPackage && model.Children?.Count > 0)
                    if (model.IsIncludeChild && model.Children?.Count > 0)
                    {
                        //Get List Charge by child 
                        if (listOldCharge == null) listOldCharge = new List<HISChargeModel>();
                        foreach (var itemChild in model.Children)
                        {
                            listOldCharge.AddRange(OHConnectionAPI.GetCharges(itemChild.PID, string.Empty, string.Join(";", listChargeInOldPackage?.Where(e => e.PID == itemChild.PID).Select(x => x.ChargeId)?.ToList())));
                        }
                    }
                    #endregion .Get Charge what is Maternity package
                    if (listOldCharge?.Count > 0)
                    {
                        var xOldQuery = (from a in listOldCharge
                                         join b in listChargeInOldPackage on a.ChargeId equals b.ChargeId into bx
                                         from b in bx.AsEnumerable()
                                         select new HISChargeModel()
                                         {
                                             ItemId = a.ItemId,
                                             ItemCode = a.ItemCode,
                                             ChargeId = a.ChargeId,
                                             NewChargeId = a.NewChargeId,
                                             ChargeSessionId = a.ChargeSessionId,
                                             ChargeDate = a.ChargeDate,
                                             ChargeCreatedDate = a.ChargeCreatedDate,
                                             ChargeUpdatedDate = a.ChargeUpdatedDate,
                                             ChargeDeletedDate = a.ChargeDeletedDate,
                                             ChargeStatus = a.ChargeStatus,
                                             VisitType = a.VisitType,
                                             VisitCode = a.VisitCode,
                                             VisitDate = a.VisitDate,
                                             InvoicePaymentStatus = a.InvoicePaymentStatus,
                                             HospitalId = a.HospitalId,
                                             HospitalCode = a.HospitalCode,
                                             PID = a.PID,
                                             CustomerId = a.CustomerId,
                                             CustomerName = a.CustomerName,
                                             UnitPrice = b.UnitPrice,
                                             Quantity = a.Quantity
                                         });
                        listChargeInOldPackage = xOldQuery?.ToList();
                        if (listChargeInOldPackage?.Count > 0)
                        {
                            listChargeInOldPackage.AddRange(oHEntities.Where(x => !listChargeInOldPackage.Any(y => y.ChargeId == x.ChargeId)));
                            oHEntities = listChargeInOldPackage;
                        }
                    }
                }
                #region Log Performace
                tp = DateTime.Now - start_time;
                CustomLog.performancejoblog.Info(string.Format("MappingChargeIntoServiceInPackageTransferred[Id={0}]: {1} step processing spen time in {2} (ms)", model.OldPatientInPackageId, "join 1", tp.TotalMilliseconds));
                #endregion .Log Performace
                start_time = DateTime.Now;
                #endregion .Get list charge in old package
                if (oHEntities?.Count > 0)
                {

                    //Store Charge into Database
                    foreach (var item in oHEntities)
                    {
                        //item.PatientInPackageId = patientInPackageId;
                        CreateOrUpdateHisCharge(item);
                    }
                    #region Filter charge
                    oHEntities = oHEntities?.Where(x => Constant.ChargeStatusAvailable.Contains(x.ChargeStatus) && x.InvoicePaymentStatus != Constant.PAYMENT_PSL_STATUS && x.ChargeDate <= model.GetEndFullDate())?.ToList();
                    #endregion .Filter charge
                    if (model != null)
                    {
                        //Set for master
                        string strSiteCode = oHEntities[0].HospitalCode;
                        var hosEntity = unitOfWork.SiteRepository.Find(x => x.Code == strSiteCode).FirstOrDefault();
                        if (hosEntity != null)
                        {
                            model.SiteCode = hosEntity.Code;
                            model.SiteName = hosEntity.Name;
                        }
                        //var srv = unitOfWork.ServiceRepository.AsEnumerable().Where(x => x.IsActive && !x.IsDeleted);
                        var srv = unitOfWork.ServiceRepository.Find(x => x.IsActive && !x.IsDeleted);
                        var xquerySrv = (from a in oHEntities
                                         join b in srv on a.ItemCode equals b.Code into bx
                                         from b in bx.AsEnumerable()
                                         select new ChargeInPackageModel()
                                         {
                                             HisChargeId = a.Id,
                                             VisitCode = a.VisitCode,
                                             ChargeId = a.ChargeId,
                                             ChargeDateTime = a.ChargeDate,
                                             ChargeDate = a.ChargeDate.Value.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND),
                                             //Thông tin người sử dụng
                                             PID = a.PID,
                                             PatientName = a.CustomerName,
                                             ServiceCode = b.Code,
                                             ServiceName = b.ViName,
                                             QtyCharged = a.Quantity,
                                             QtyRemain = 0,
                                             Price = a.UnitPrice
                                         });
                        entities = xquerySrv?.ToList();
                        if (model.Services?.Count > 0)
                        {
                            #region Rebuild Services
                            List<PatientInPackageDetailModel> Services = new List<PatientInPackageDetailModel>();
                            foreach (var item in model.Services)
                            {
                                Services.Add(item);
                                if (item.ItemsReplace?.Count > 0)
                                {
                                    var itemReplaces = unitOfWork.ServiceInPackageRepository.Find(x => x.RootId == item.ServiceInPackageId);
                                    if (itemReplaces.Any())
                                    {
                                        foreach (var itemRpl in itemReplaces)
                                        {
                                            var itemReplace = new PatientInPackageDetailModel();
                                            itemReplace.ServiceInPackageId = itemRpl.Id;
                                            itemReplace.ServiceInPackageRootId = item.ServiceInPackageId;
                                            itemReplace.Qty = item.Qty;
                                            itemReplace.BasePrice = item.BasePrice;
                                            itemReplace.BaseAmount = item.BaseAmount;
                                            itemReplace.PkgPrice = item.PkgPrice;
                                            itemReplace.PkgAmount = item.PkgAmount;
                                            itemReplace.ServiceType = item.ServiceType;
                                            itemReplace.Service = itemRpl.Service;
                                            Services.Add(itemReplace);
                                        }
                                    }
                                }
                            }
                            #region Log Performace
                            tp = DateTime.Now - start_time;
                            CustomLog.performancejoblog.Info(string.Format("MappingChargeIntoServiceInPackageTransferred[Id={0}]: {1} step processing spen time in {2} (ms)", model.OldPatientInPackageId, "join 2", tp.TotalMilliseconds));
                            #endregion .Log Performace
                            start_time = DateTime.Now;
                            #endregion .Rebuild Services
                            var xquery = (from a in entities
                                          join b in Services/*model.Services*/ on a.ServiceCode equals b.Service.Code into bx
                                          from b in bx.DefaultIfEmpty()
                                          select new ChargeInPackageModel()
                                          {
                                              HisChargeId = a.HisChargeId,
                                              VisitCode = a.VisitCode,
                                              ServiceInpackageId = b?.ServiceInPackageId,
                                              ServiceType = b?.ServiceType != null ? b.ServiceType : (int)ServiceInPackageTypeEnum.UNKNOWN,
                                              RootId = b?.ServiceInPackageRootId,
                                              ChargeId = a.ChargeId,
                                              ChargeDateTime = a.ChargeDateTime,
                                              ChargeDate = a.ChargeDate,
                                              //Thông tin người sử dụng
                                              PID = a.PID,
                                              PatientName = a.PatientName,
                                              ServiceCode = a.ServiceCode,
                                              ServiceName = a.ServiceName,
                                              QtyCharged = a.QtyCharged,
                                              QtyRemain = b?.Qty != null ? b?.Qty : 0,
                                              Price = a.Price,
                                              PkgPrice = b?.PkgPrice != null ? b?.PkgPrice : a.Price
                                          });
                            entities = xquery?.OrderBy(x => x.ChargeDateTime).ToList();
                            #region Log Performace
                            tp = DateTime.Now - start_time;
                            CustomLog.performancejoblog.Info(string.Format("MappingChargeIntoServiceInPackageTransferred[Id={0}]: {1} step processing spen time in {2} (ms)", model.OldPatientInPackageId, "join 3", tp.TotalMilliseconds));
                            #endregion .Log Performace
                        }
                        else
                        {
                            return null;
                        }
                        if (entities?.Count > 0)
                        {
                            #region Rebuild to split In package or over package
                            List<ChargeInPackageModel> returnEntities = new List<ChargeInPackageModel>();
                            //Loại bỏ các charge đã được ghi nhận vào gói đã đóng, đang sử dụng, hết hạn
                            start_time = DateTime.Now;
                            #region Loại bỏ các charge đã được ghi nhận vào gói đã đóng, đang sử dụng, hết hạn
                            //cach 1
                            //var listChargeInPackageNotGet = unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageId != model.OldPatientInPackageId && x.InPackageType == (int)InPackageType.INPACKAGE && !x.PatientInPackageDetail.ServiceInPackage.IsDeleted && !x.IsDeleted && Constant.ListStatusNotGetTranferred.Contains(x.PatientInPackage.Status))/*.AsEnumerable()*/;
                            //entities = entities.Where(x => !listChargeInPackageNotGet.Any(y =>  y.HisChargeId == x.HisChargeId ))?.ToList();
                            //end cach 1
                            //cach 2
                            //foreach (var item in entities)
                            //{
                            //    var hisChargeDetailEntity = unitOfWork.HISChargeDetailRepository.FirstOrDefault(x => x.HisChargeId == item.HisChargeId && x.PatientInPackageId != model.OldPatientInPackageId && x.InPackageType == (int)InPackageType.INPACKAGE && !x.PatientInPackageDetail.ServiceInPackage.IsDeleted && !x.IsDeleted && Constant.ListStatusNotGetTranferred.Contains(x.PatientInPackage.Status));
                            //    if (hisChargeDetailEntity != null)
                            //    {
                            //        entities.Remove(item);
                            //    }
                            //}
                            //cach 3
                            //tungdd14 lọc thêm điều kiện PID
                            var listPID = new List<string>();
                            var patientInPackageEntity = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == model.OldPatientInPackageId && !x.IsDeleted);
                            if (patientInPackageEntity != null)
                            {
                                listPID.Add(patientInPackageEntity.PatientInformation.PID);
                                var patientInPackageChildEntity = GetChildrenByPatientInPackageId(model.OldPatientInPackageId);
                                if (patientInPackageChildEntity != null)
                                {
                                    foreach (var item in patientInPackageChildEntity)
                                    {
                                        listPID.Add(item.PID);
                                    }
                                }
                            }
                            var listChargeInPackageNotGet = unitOfWork.HISChargeDetailRepository.Find(x => !x.PatientInPackageDetail.ServiceInPackage.IsDeleted && !x.IsDeleted && listPID.Contains(x.PatientInPackage.PatientInformation.PID))/*.AsEnumerable()*/;
                            entities = entities.Where(x => !listChargeInPackageNotGet.Any(y => y.PatientInPackageId != model.OldPatientInPackageId && y.InPackageType == (int)InPackageType.INPACKAGE && y.HisChargeId == x.HisChargeId && Constant.ListStatusNotGetTranferred.Contains(y.PatientInPackage.Status)))?.ToList();
                            //end cach 3
                            //end cach 3
                            //cach cu
                            //var listChargeInPackageNotGet = unitOfWork.HISChargeDetailRepository.Find(x => !x.PatientInPackageDetail.ServiceInPackage.IsDeleted && !x.IsDeleted)/*.AsEnumerable()*/;
                            //entities = entities.Where(x => !listChargeInPackageNotGet.Any(y => y.PatientInPackageId != model.OldPatientInPackageId && y.InPackageType == (int)InPackageType.INPACKAGE && y.HisChargeId == x.HisChargeId && Constant.ListStatusNotGetTranferred.Contains(y.PatientInPackage.Status)))?.ToList();
                            #endregion .Loại bỏ các charge đã được ghi nhận vào gói đã đóng, đang sử dụng, hết hạn
                            #region Log Performace
                            tp = DateTime.Now - start_time;
                            CustomLog.performancejoblog.Info(string.Format("MappingChargeIntoServiceInPackageTransferred[Id={0}]: {1} step processing spen time in {2} (ms)", model.OldPatientInPackageId, "ChargeInPackageNotGet", tp.TotalMilliseconds));
                            #endregion .Log Performace
                            foreach (var item in entities)
                            {
                                var QtyRemain = entities.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).Select(x => x.QtyRemain)?.FirstOrDefault();
                                //var QtyRemain = item.QtyRemain;
                                if (item.QtyCharged > QtyRemain && QtyRemain == 0
                                    && (item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE || (model.IsLimitedDrugConsum && item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM)))
                                {
                                    //Create new item over
                                    ChargeInPackageModel overEntity = (ChargeInPackageModel)item.Clone();
                                    overEntity.InPackageType = (int)InPackageType.OVERPACKAGE;
                                    overEntity.QtyRemain = 0;
                                    overEntity.QtyCharged = item.QtyCharged - QtyRemain;
                                    //Set bằng giá lẻ
                                    overEntity.Price = item.Price;
                                    overEntity.Amount = overEntity.Price * overEntity.QtyCharged;
                                    overEntity.IsChecked = true;
                                    SetNotes4ConfirmServiceBelongPackage(model.PatientModel?.PID, overEntity, model.OldPatientInPackageId.Value);
                                    //Add item over
                                    returnEntities.Add(overEntity);
                                    //Cập nhật số lượng còn lại
                                    entities.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).ToList().ForEach(x => x.QtyRemain = x.QtyRemain >= item.QtyCharged ? x.QtyRemain - item.QtyCharged : 0);
                                }
                                else if (item.QtyCharged > QtyRemain && QtyRemain != 0
                                    && (item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE || (model.IsLimitedDrugConsum && item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM)))
                                {
                                    //Create new item Charge invalid qtycharge
                                    ChargeInPackageModel InvalidEntity = (ChargeInPackageModel)item.Clone();
                                    InvalidEntity.InPackageType = (int)InPackageType.QTYINCHARGEGREATTHANREMAIN;
                                    InvalidEntity.QtyRemain = QtyRemain;
                                    InvalidEntity.QtyCharged = item.QtyCharged;
                                    ////Set bằng giá =0
                                    #region Tạm thời đóng lại nghiệp vụ Set bằng giá =0 (18/01/2022)
                                    //InvalidEntity.Price = 0;
                                    #endregion Tạm thời đóng lại nghiệp vụ Set bằng giá =0 (18/01/2022)
                                    //Set bằng giá lẻ
                                    #region Set bằng giá lẻ(18/01/2022)
                                    InvalidEntity.Price = item.Price;
                                    #endregion Set bằng giá lẻ(18/01/2022)
                                    InvalidEntity.Amount = InvalidEntity.Price * InvalidEntity.QtyCharged;
                                    //Build Notes
                                    SetNotes4ConfirmServiceBelongPackage(model.PatientModel?.PID, InvalidEntity, model.OldPatientInPackageId.Value);
                                    InvalidEntity.IsChecked = true;
                                    returnEntities.Add(InvalidEntity);
                                }
                                else if (item.ServiceType == (int)ServiceInPackageTypeEnum.UNKNOWN)
                                {
                                    ChargeInPackageModel outsideEntity = (ChargeInPackageModel)item.Clone();
                                    outsideEntity.InPackageType = (int)InPackageType.OUTSIDEPACKAGE;
                                    outsideEntity.QtyCharged = item.QtyCharged;
                                    //Set bằng giá lẻ
                                    outsideEntity.Price = item.Price;
                                    outsideEntity.Amount = outsideEntity.Price * outsideEntity.QtyCharged;
                                    outsideEntity.IsChecked = true;
                                    returnEntities.Add(outsideEntity);
                                }
                                else
                                {
                                    int? currentInPackageType = item.InPackageType;
                                    item.InPackageType = (int)InPackageType.INPACKAGE;
                                    item.QtyRemain = QtyRemain;
                                    item.Price = item.PkgPrice;
                                    item.Amount = item.Price * item.QtyCharged;
                                    item.IsChecked = true;
                                    //Build Notes
                                    SetNotes4ConfirmServiceBelongPackage(model.PatientModel?.PID, item, model.OldPatientInPackageId.Value, currentInPackageType);
                                    returnEntities.Add((ChargeInPackageModel)item.Clone());
                                    //Cập nhật số lượng còn lại
                                    entities.Where(x => x.ServiceCode == item.ServiceCode || (x.RootId != null && item.ServiceInpackageId != null && x.RootId == item.ServiceInpackageId) || (x.ServiceInpackageId != null && item.RootId != null && x.ServiceInpackageId == item.RootId)).ToList().ForEach(x => x.QtyRemain = x.QtyRemain >= item.QtyCharged ? x.QtyRemain - item.QtyCharged : 0);
                                }
                            }
                            entities = returnEntities;
                            #endregion .Rebuild to split In package or over package
                        }
                        model.listCharge = entities;
                    }
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("MappingChargeIntoServiceInPackage fail. Ex: {0}", ex));
            }
            return entities;
        }
        /// <summary>
        /// Statistic charge via visit in package
        /// </summary>
        /// <param name="patientInPackageId"></param>
        /// <param name="pid"></param>
        /// <param name="visitCode"></param>
        /// <returns></returns>
        public List<ChargeStatisticDetailModel> StatisticChargeViaVisitInPackage(ChargeStatisticModel model, string pid, string visitCode = "")
        {
            List<ChargeStatisticDetailModel> entities = null;
            try
            {
                var oHEntities = OHConnectionAPI.GetCharges(pid, visitCode, string.Empty);
                if (oHEntities?.Count > 0)
                {
                    if (model != null)
                    {
                        //Set for master
                        model.VisitCode = oHEntities[0].VisitCode;
                        model.VisitDate = oHEntities[0].VisitDate?.ToString(Constant.DATE_FORMAT);
                        string strSiteCode = oHEntities[0].HospitalCode;
                        var hosEntity = unitOfWork.SiteRepository.Find(x => x.Code == strSiteCode).FirstOrDefault();
                        if (hosEntity != null)
                        {
                            model.SiteCode = hosEntity.Code;
                            model.SiteName = hosEntity.Name;
                        }
                        entities = oHEntities.Where(x => Constant.ChargeStatusAvailable.Contains(x.ChargeStatus))?.Select(x => new ChargeStatisticDetailModel()
                        {
                            ServiceCode = x.ItemCode,
                            ChargeId = x.ChargeId,
                            ChargeDate = x.ChargeDate.Value.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND),
                            Price = x.UnitPrice,
                            QtyCharged = x.Quantity,
                            IsInvoiced = x.InvoicePaymentStatus == Constant.PAYMENT_PSL_STATUS
                        })?.ToList();

                        var listCharge = unitOfWork.HISChargeRepository.Find(x => x.PID == model.PID && x.VisitCode == model.VisitCode && Constant.ChargeStatusAvailable.Contains(x.ChargeStatus));
                        //var listService = unitOfWork.ServiceRepository.AsEnumerable();
                        var listService = unitOfWork.ServiceRepository.Find(x => !x.IsDeleted);
                        //var listPatientInPackage = unitOfWork.PatientInPackageRepository.AsEnumerable();
                        //Get List Service InPackage of patient
                        var countListCharge = listCharge.Count();
                        //var countListCharge = listCharge.Count();
                        if (listCharge.Any())
                        {
                            var listHisChargeDetail = unitOfWork.HISChargeDetailRepository.Find(x => !x.IsDeleted)/*.AsEnumerable()*/;
                            //Mapping charge into service inpackage
                            var xquery = (from a in entities
                                          join b in listCharge on a.ChargeId equals b?.ChargeId into bx
                                          from b in bx.DefaultIfEmpty()
                                          join c in listService on a.ServiceCode equals c?.Code into cx
                                          from c in cx.DefaultIfEmpty()
                                              //join d in listPatientInPackage on b.PatientInPackageId equals d.Id into dx
                                              //from d in dx.DefaultIfEmpty()
                                          join d in listHisChargeDetail on b?.Id equals d?.HisChargeId into dx
                                          from d in dx.DefaultIfEmpty()
                                          select new ChargeStatisticDetailModel()
                                          {
                                              ChargeId = a.ChargeId,
                                              ChargeDate = a.ChargeDate,
                                              RootId = d?.PatientInPackageDetail?.ServiceInPackage?.RootId,
                                              ServiceCode = a.ServiceCode,
                                              ServiceName = c?.ViName,
                                              QtyCharged = a.QtyCharged,
                                              QtyInPackage = d?.PatientInPackageDetail?.ServiceInPackage?.LimitQty,
                                              PatientInPackageId = b?.PatientInPackageId,
                                              PackageCode = d?.PatientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Code,
                                              PackageName = d?.PatientInPackage?.PackagePriceSite?.PackagePrice?.Package?.Name,
                                              InPackageType = d?.InPackageType != null ? d.InPackageType : (int)InPackageType.OUTSIDEPACKAGE,
                                              Price = d?.UnitPrice != null ? d?.UnitPrice : a.Price,
                                              ChargePrice = d?.ChargePrice != null ? d?.ChargePrice : a.Price,
                                              IsInvoiced = a.IsInvoiced,
                                              ItemType = 2,
                                              IsTotal = false,
                                              //tungdd14 thêm giá trị check ngoài gói
                                              VisitType = b?.VisitType,
                                              ChargeIsUseForReExam = d != null ? d.ChargeIsUseForReExam : false,
                                          });
                            //15-07-2022:PhuBQ fix bug duplicate item (.GroupBy(x=>x.ChargeId).Select(grp => grp.First()))
                            entities = xquery?.GroupBy(x => x.ChargeId).Select(grp => grp.First()).ToList();
                            if (entities?.Count > 0)
                            {
                                #region Rebuild to split In package or over package
                                //List<ChargeStatisticDetailModel> returnEntities = new List<ChargeStatisticDetailModel>();
                                //var listServiceInPackage = unitOfWork.PatientInPackageDetailRepository.Find(x => !x.ServiceInPackage.IsDeleted).AsEnumerable();
                                //if (listServiceInPackage.Any())
                                {
                                    #region Comment old code
                                    //var xquery2 = (from a in entities
                                    //              join b in listServiceInPackage on a.ServiceCode equals b.ServiceInPackage.Service.Code into bx
                                    //               from b in bx.DefaultIfEmpty()
                                    //               where a.PatientInPackageId == b?.PatientInPackageId /*&& b.PatientInPackageId==c.PatientInPackageId*/   
                                    //               //where b?.ServiceInPackage?.Service.Code==a.ServiceCode
                                    //               select new ChargeStatisticDetailModel()
                                    //               {
                                    //                   ChargeId = a.ChargeId,
                                    //                   ChargeDate = a.ChargeDate,
                                    //                   ServiceCode = a.ServiceCode,
                                    //                   ServiceName = a.ServiceName,
                                    //                   QtyCharged = a.QtyCharged,
                                    //                   QtyInPackage = b?.ServiceInPackage.LimitQty,
                                    //                   PatientInPackageId = a.PatientInPackageId,
                                    //                   PackageCode = a.PackageCode,
                                    //                   PackageName = a.PackageName,
                                    //                   Price = a.Price,
                                    //                   IsInvoiced = a.IsInvoiced,
                                    //                   //InPackageType=a.QtyCharged> b?.ServiceInPackage.LimitQty?2:1,
                                    //                   //InPackageType=c?.InPackageType!=null? c.InPackageType: (int)InPackageType.OVERPACKAGE,
                                    //                   //PackageCode= b?.PatientInPackage.PackagePriceSite.PackagePrice.Package.Code,
                                    //                   //PackageName= b?.PatientInPackage.PackagePriceSite.PackagePrice.Package.Name,
                                    //                   ItemType = 2,
                                    //                   IsTotal=false
                                    //               });
                                    //if (xquery2.Any())
                                    //{
                                    //    entities = xquery2.ToList();
                                    //}
                                    #endregion
                                    #region Rebuild for stat
                                    //tungdd14 check service is drugconsum
                                    var listServiceIsDrugConsum = unitOfWork.ServiceRepository.Find(x => x.ServiceType == Constant.SERVICE_TYPE_INV && !x.IsDeleted).Select(x => x.Code).ToList();
                                    if (entities?.Count > 0)
                                    {
                                        foreach (var item in entities)
                                        {
                                            List<MessageModel> listNode = null;
                                            //tungdd14 thêm điều kiện !Constant.VISIT_TYPE_PACKAGES.Contains(item.VisitType)
                                            //trường hợp PatientInPackageId != null nhưng ở ngoài gói
                                            if (item.PatientInPackageId == null || item.InPackageType == (int)InPackageType.QTYINCHARGEGREATTHANREMAIN || !Constant.VISIT_TYPE_PACKAGES.Contains(item.VisitType))
                                            {
                                                //tungdd14 check service is drugconsum
                                                item.IsDrugConsum = listServiceIsDrugConsum.Contains(item.ServiceCode);
                                                item.PackageName = MessageManager.Messages.Where(x => x.Code == (item.IsDrugConsum ? MessageCode.LABEL_IS_DRUGCONSUM : MessageCode.LABEL_OUTSIDEPACKAGE_SERVICE)).Select(x => x.ViMessage).FirstOrDefault();
                                                item.InPackageType = (int)InPackageType.OUTSIDEPACKAGE;
                                                item.Amount = item.Price * item.QtyCharged;
                                                item.Notes = GetListNotesReplaceService(item.RootId, item.ChargeIsUseForReExam && item.InPackageType == (int)InPackageType.INPACKAGE);
                                            }
                                            else
                                            {
                                                if (item.InPackageType == (int)InPackageType.OUTSIDEPACKAGE && string.IsNullOrEmpty(item.PackageCode))
                                                {
                                                    //tungdd14 check service is drugconsum
                                                    item.IsDrugConsum = listServiceIsDrugConsum.Contains(item.ServiceCode);
                                                    item.PackageName = MessageManager.Messages.Where(x => x.Code == (item.IsDrugConsum ? MessageCode.LABEL_IS_DRUGCONSUM : MessageCode.LABEL_OUTSIDEPACKAGE_SERVICE)).Select(x => x.ViMessage).FirstOrDefault();
                                                }
                                                item.Amount = item.Price * item.QtyCharged;
                                                item.Notes = GetListNotesReplaceService(item.RootId, item.ChargeIsUseForReExam && item.InPackageType == (int)InPackageType.INPACKAGE);
                                            }
                                        }
                                        #region Add row total group
                                        //Total amount
                                        double? totalAmount = entities.Sum(x => x.Amount);
                                        double? totalQtyCharge = entities.Sum(x => x.QtyCharged);
                                        entities.Add(new ChargeStatisticDetailModel()
                                        {
                                            ItemType = 0,
                                            PackageName = MessageManager.Messages.Where(x => x.Code == MessageCode.LABEL_TOTAL_AMOUNT).Select(x => x.ViMessage).FirstOrDefault(),
                                            IsTotal = true,
                                            QtyCharged = totalQtyCharge,
                                            Amount = totalAmount
                                        });
                                        double? totalReceivables = 0;
                                        //Total Inside/over package
                                        var groupPackage = entities.GroupBy(x => x.PackageCode);
                                        if (groupPackage.Any())
                                        {
                                            foreach (var item in groupPackage)
                                            {
                                                if (item.Key != null)
                                                {
                                                    var entityPackage = entities.FirstOrDefault(x => x.PackageCode == item.Key);
                                                    if (entityPackage != null)
                                                    {
                                                        //Total Inside package
                                                        double? total_inside_package = entities.Where(x => x.PackageCode == entityPackage.PackageCode && x.InPackageType == (int)InPackageType.INPACKAGE)?.Sum(x => x.Amount);
                                                        double? total_inside_QtyCharge = entities.Where(x => x.PackageCode == entityPackage.PackageCode && x.InPackageType == (int)InPackageType.INPACKAGE)?.Sum(x => x.QtyCharged);
                                                        string labelInsidePackage = MessageManager.Messages.Where(x => x.Code == MessageCode.LABEL_INSIDEPACKAGE_GROUPTOTAL).Select(x => x.ViMessage).FirstOrDefault();
                                                        labelInsidePackage = !string.IsNullOrEmpty(labelInsidePackage) ? string.Format(labelInsidePackage, entityPackage.PackageName, entityPackage.PackageCode) : string.Empty;
                                                        entities.Add(new ChargeStatisticDetailModel()
                                                        {
                                                            ItemType = 1,
                                                            PackageName = labelInsidePackage,
                                                            IsTotal = true,
                                                            InPackageType = (int)InPackageType.INPACKAGE,
                                                            QtyCharged = total_inside_QtyCharge,
                                                            Amount = total_inside_package
                                                        });

                                                        //Total Over package
                                                        double? total_over_package = entities.Where(x => x.PackageCode == entityPackage.PackageCode && x.InPackageType == (int)InPackageType.OVERPACKAGE)?.Sum(x => x.Amount);
                                                        double? total_over_QtyCharge = entities.Where(x => x.PackageCode == entityPackage.PackageCode && x.InPackageType == (int)InPackageType.OVERPACKAGE)?.Sum(x => x.QtyCharged);
                                                        string labelOverPackage = MessageManager.Messages.Where(x => x.Code == MessageCode.LABEL_OVERPACKAGE_GROUPTOTAL).Select(x => x.ViMessage).FirstOrDefault();
                                                        labelOverPackage = !string.IsNullOrEmpty(labelOverPackage) ? string.Format(labelOverPackage, entityPackage.PackageName, entityPackage.PackageCode) : string.Empty;
                                                        entities.Add(new ChargeStatisticDetailModel()
                                                        {
                                                            ItemType = 1,
                                                            PackageName = labelOverPackage,
                                                            IsTotal = true,
                                                            InPackageType = (int)InPackageType.OVERPACKAGE,
                                                            QtyCharged = total_over_QtyCharge,
                                                            Amount = total_over_package
                                                        });
                                                        if (total_over_package != null)
                                                            totalReceivables += total_over_package;
                                                    }
                                                }
                                            }
                                        }
                                        //Total outsite package
                                        //tungdd14 check service is drugconsum
                                        double? total_outside_package = entities.Where(x => x.InPackageType == (int)InPackageType.OUTSIDEPACKAGE && !x.IsDrugConsum)?.Sum(x => x.Amount);
                                        double? total_outside_QtyCharge = entities.Where(x => x.InPackageType == (int)InPackageType.OUTSIDEPACKAGE && !x.IsDrugConsum)?.Sum(x => x.QtyCharged);
                                        entities.Add(new ChargeStatisticDetailModel()
                                        {
                                            ItemType = 1,
                                            PackageName = MessageManager.Messages.Where(x => x.Code == MessageCode.LABEL_OUTSIDEPACKAGE_GROUPTOTAL).Select(x => x.ViMessage).FirstOrDefault(),
                                            IsTotal = true,
                                            InPackageType = (int)InPackageType.OUTSIDEPACKAGE,
                                            QtyCharged = total_outside_QtyCharge,
                                            Amount = total_outside_package
                                        });

                                        //tungdd14 check service is drugconsum
                                        //Total drugconsum
                                        double? total_drugconsum_package = entities.Where(x => x.InPackageType == (int)InPackageType.OUTSIDEPACKAGE && x.IsDrugConsum)?.Sum(x => x.Amount);
                                        double? total_drugconsum_QtyCharge = entities.Where(x => x.InPackageType == (int)InPackageType.OUTSIDEPACKAGE && x.IsDrugConsum)?.Sum(x => x.QtyCharged);
                                        entities.Add(new ChargeStatisticDetailModel()
                                        {
                                            ItemType = 1,
                                            PackageName = MessageManager.Messages.Where(x => x.Code == MessageCode.LABEL_DRUGCONSUM_GROUPTOTAL).Select(x => x.ViMessage).FirstOrDefault(),
                                            IsTotal = true,
                                            InPackageType = (int)InPackageType.OUTSIDEPACKAGE,
                                            QtyCharged = total_drugconsum_QtyCharge,
                                            Amount = total_drugconsum_package
                                        });
                                        //Total Receivables
                                        if (total_outside_package != null)
                                            totalReceivables += total_outside_package;
                                        if (total_drugconsum_package != null)
                                            totalReceivables += total_drugconsum_package;
                                        entities.Add(new ChargeStatisticDetailModel()
                                        {
                                            ItemType = -1,
                                            PackageName = MessageManager.Messages.Where(x => x.Code == MessageCode.LABEL_RECEIVABLES).Select(x => x.ViMessage).FirstOrDefault(),
                                            IsTotal = true,
                                            Amount = totalReceivables
                                        });

                                        #endregion .Add row total group
                                    }
                                    #endregion .Rebuild for stat
                                }
                                #endregion .Rebuild to split In package or over package
                            }
                            model.Details = entities;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("StatisticChargeViaVisitInPackage fail. Ex: {0}", ex));
            }
            return entities;
        }
        public List<ChargeStatisticWhenCancelledDetailModel> StatisticChargeInPackageWhenCancelled(ChargeStatisticWhenCancelledModel model, Guid PatientInPackageId)
        {
            List<ChargeStatisticWhenCancelledDetailModel> entities = null;
            try
            {
                //var xqueryChargeHisDetail = unitOfWork.HISChargeDetailRepository.AsEnumerable().Where(x=>x.PatientInPackageId== PatientInPackageId && !x.IsDeleted && x.InPackageType==(int)InPackageType.INPACKAGE);
                var xqueryChargeHisDetail = unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageId == PatientInPackageId && !x.IsDeleted && x.InPackageType == (int)InPackageType.INPACKAGE);
                if (xqueryChargeHisDetail.Any())
                {
                    entities = xqueryChargeHisDetail.OrderBy(x => x.HISCharge.ChargeDate).Select(x => new ChargeStatisticWhenCancelledDetailModel()
                    {
                        ChargeId = x.HISCharge.ChargeId,
                        ChargeDate = x.HISCharge.ChargeDate?.ToString(Constant.DATE_TIME_FORMAT),
                        ServiceCode = x.PatientInPackageDetail.ServiceInPackage.Service.Code,
                        ServiceName = x.PatientInPackageDetail.ServiceInPackage.Service.ViName,
                        ItemType = 2,
                        PatientInPackageId = PatientInPackageId,
                        VisitCode = x.HISCharge.VisitCode,
                        Qty = x.HISCharge.Quantity,
                        PkgPrice = x.UnitPrice,
                        PkgAmount = x.UnitPrice * x.HISCharge.Quantity,
                        Price = x.ChargePrice,
                        Amount = x.ChargePrice * x.HISCharge.Quantity
                    })?.ToList();
                    #region Add more Total Row
                    if (entities != null && entities.Count > 0)
                    {
                        double? total_Base = entities.Sum(x => x.Amount);
                        double? total_Package = entities.Sum(x => x.PkgAmount);
                        entities.Add(new ChargeStatisticWhenCancelledDetailModel() { ItemType = 0, PkgAmount = total_Package, Amount = total_Base });
                        #region Build row refund 4 Patient
                        if (total_Base >= model.PkgAmount)
                        {
                            //Phải thu khách hàng
                            entities.Add(new ChargeStatisticWhenCancelledDetailModel() { ItemType = -1, Amount = (total_Base - model.PkgAmount) });
                        }
                        else if (total_Base < model.PkgAmount)
                        {
                            //Phải trả khách hàng
                            entities.Add(new ChargeStatisticWhenCancelledDetailModel() { ItemType = -2, Amount = (model.PkgAmount - total_Base) });
                        }
                        #endregion Build row refund 4 Patient
                    }
                    #endregion .Add more Total Row
                    model.Details = entities;
                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("StatisticChargeInPackageWhenCancelled fail. Ex: {0}", ex));
            }
            return entities;
        }
        /// <summary>
        /// Create or update his charge
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HISCharge CreateOrUpdateHisCharge(HISChargeModel model)
        {
            HISCharge entity = unitOfWork.HISChargeRepository.FirstOrDefault(
                e => e.ChargeId == model.ChargeId && e.ChargeType == model.ChargeType &&
                e.ItemCode == model.ItemCode);
            if (entity == null)
            {
                entity = new HISCharge();
            }
            entity.ItemId = model.ItemId;
            entity.ItemCode = model.ItemCode;
            entity.ChargeId = model.ChargeId;
            entity.NewChargeId = model.NewChargeId;
            entity.ChargeSessionId = model.ChargeSessionId;
            entity.ChargeDate = model.ChargeDate;
            entity.ChargeCreatedDate = model.ChargeCreatedDate;
            entity.ChargeUpdatedDate = model.ChargeUpdatedDate;
            entity.ChargeDeletedDate = model.ChargeDeletedDate;
            entity.ChargeStatus = model.ChargeStatus;
            entity.VisitType = model.VisitType;
            entity.VisitCode = model.VisitCode;
            entity.VisitDate = model.VisitDate;
            entity.InvoicePaymentStatus = model.InvoicePaymentStatus;
            entity.HospitalId = model.HospitalId;
            entity.HospitalCode = model.HospitalCode;
            entity.PID = model.PID;
            entity.CustomerId = model.CustomerId;
            entity.CustomerName = model.CustomerName;
            entity.UnitPrice = model.UnitPrice;
            entity.Quantity = model.Quantity;
            entity.PricingClass = model.PricingClass;
            if (model.PatientInPackageId != null)
            {
                entity.PatientInPackageId = model.PatientInPackageId;
            }
            //tungdd14: comment để fix lỗi move chỉ định trong gói về chỉ định lẻ
            //Tự động loại bỏ khỏi gói nếu chỉ định bị move khỏi visit package
            //#region Tự động loại bỏ khỏi gói nếu chỉ định bị move khỏi visit package
            //if (!Constant.VISIT_TYPE_PACKAGES.Contains(model.VisitType))
            //{
            //    entity.PatientInPackageId = null;
            //}
            //#endregion
            //entity.PatientInPackageId = Constant.ChargeStatusCancel.Contains(entity.ChargeStatus) ? null : entity.PatientInPackageId;
            if (entity.Id != Guid.Empty)
            {
                unitOfWork.HISChargeRepository.Update(entity);
            }
            else
            {
                unitOfWork.HISChargeRepository.Add(entity);
            }
            model.Id = entity.Id;
            unitOfWork.Commit();
            return entity;
        }
        public List<HISCharge> CreateOrUpdateHisCharges(List<HISChargeModel> models, PatientInPackage entityPiPkg = null)
        {
            bool statusUpdatePricingClass = true;
            List<HISCharge> listReturn = null;
            if (models?.Count > 0)
            {
                //using(IUnitOfWork unitOfWorklocal = new EfUnitOfWork())
                {
                    listReturn = new List<HISCharge>();
                    foreach (var item in models)
                    {
                        HISCharge entity = unitOfWork.HISChargeRepository.FirstOrDefault(
                    e => e.ChargeId == item.ChargeId && e.ChargeType == item.ChargeType &&
                    e.ItemCode == item.ItemCode);
                        if (entity == null)
                        {
                            entity = new HISCharge();
                        }
                        entity.ItemId = item.ItemId;
                        entity.ItemCode = item.ItemCode;
                        entity.ChargeId = item.ChargeId;
                        entity.NewChargeId = item.NewChargeId;
                        entity.ChargeSessionId = item.ChargeSessionId;
                        entity.ChargeDate = item.ChargeDate;
                        entity.ChargeCreatedDate = item.ChargeCreatedDate;
                        entity.ChargeUpdatedDate = item.ChargeUpdatedDate;
                        entity.ChargeDeletedDate = item.ChargeDeletedDate;
                        entity.ChargeStatus = item.ChargeStatus;
                        entity.VisitType = item.VisitType;
                        entity.VisitCode = item.VisitCode;
                        entity.VisitDate = item.VisitDate;
                        entity.InvoicePaymentStatus = item.InvoicePaymentStatus;
                        entity.HospitalId = item.HospitalId;
                        entity.HospitalCode = item.HospitalCode;
                        entity.PID = item.PID;
                        entity.CustomerId = item.CustomerId;
                        entity.CustomerName = item.CustomerName;
                        entity.UnitPrice = item.UnitPrice;
                        entity.Quantity = item.Quantity;
                        //Cập nhật lại giá gốc (ChargePrice) tại bảng HISChargeDetails
                        #region Cập nhật lại giá gốc (ChargePrice) tại bảng HISChargeDetails
                        if (entityPiPkg != null && entity.Id != null
                            && listStatusAllowUpdateOrginalPriceWhenPricingClass.Contains(entityPiPkg.Status)
                            && entity.PricingClass != item.PricingClass && !string.IsNullOrEmpty(entity.PricingClass)
                            && item.InvoicePaymentStatus != Constant.PAYMENT_PSL_STATUS && !Constant.ChargeStatusCancel.Contains(item.ChargeStatus)
                            && item.ChargeDeletedDate == null)
                        {
                            statusUpdatePricingClass = UpdatePriceWhenPricingClass(entity, isCommit: false);
                            if (!statusUpdatePricingClass)
                            {
                                break;
                            }
                        }
                        #endregion
                        entity.PricingClass = item.PricingClass;
                        if (item.PatientInPackageId != null)
                        {
                            entity.PatientInPackageId = item.PatientInPackageId;
                        }
                        //Tự động loại bỏ khỏi gói nếu chỉ định bị move khỏi visit package
                        //tungdd14: comment để fix lỗi move chỉ định trong gói về chỉ định lẻ
                        //#region Tự động loại bỏ khỏi gói nếu chỉ định bị move khỏi visit package
                        //if (!Constant.VISIT_TYPE_PACKAGES.Contains(item.VisitType))
                        //{
                        //    entity.PatientInPackageId = null;
                        //}
                        //#endregion
                        //entity.PatientInPackageId = Constant.ChargeStatusCancel.Contains(entity.ChargeStatus) ? null : entity.PatientInPackageId;
                        if (entity.Id != Guid.Empty)
                        {
                            unitOfWork.HISChargeRepository.Update(entity);
                        }
                        else
                        {
                            unitOfWork.HISChargeRepository.Add(entity);
                        }
                        item.Id = entity.Id;

                        listReturn.Add(entity);
                    }
                }
            }
            if (statusUpdatePricingClass)
                unitOfWork.Commit();
            return listReturn;
        }
        public bool UpdatePriceWhenPricingClass(HISCharge model, bool isCommit = true)
        {
            bool returnValue = false;
            HISChargeDetail entity = unitOfWork.HISChargeDetailRepository.FirstOrDefault(
                e => e.HisChargeId == model.Id && listInPackageTypeAllowUpdateOrginalPriceWhenPricingClass.Contains(e.InPackageType) && !e.IsDeleted);
            if (entity != null)
            {
                //tungdd14: nếu thay đổi pricingClass và chuyển sang visit lẻ thì không cập nhật giá gói
                if (Constant.VISIT_TYPE_PACKAGES.Contains(model.VisitType))
                {
                    entity.ChargePrice = model.UnitPrice;
                }
                
                if (entity.InPackageType == (int)InPackageType.OVERPACKAGE)
                {
                    entity.UnitPrice = model.UnitPrice;
                    entity.NetAmount = entity.Quantity * entity.UnitPrice;
                    returnValue = true;
                }
                else if (entity.InPackageType == (int)InPackageType.INPACKAGE)
                {
                    //Cần cập nhật ngược giá gói về OH cho ChargePrice
                    List<ChargeInPackageModel> listCharges = new List<ChargeInPackageModel>()
                    {
                        new ChargeInPackageModel()
                        {
                            IsChecked = true,
                            ChargeId = entity.HISCharge.ChargeId,
                            InPackageType = entity.InPackageType,
                            Price = entity.ChargePrice,
                            //Cập nhật lại giá cho charge= giá gói
                            PkgPrice = entity.PatientInPackageDetail.PkgPrice
                        }
                    };
                    string returnMsg = string.Empty;
                    var returnUpdateOH = OHConnectionAPI.UpdateChargePrice(listCharges, out returnMsg);
                    if (returnUpdateOH)
                    {
                        if (Constant.StatusUpdatePriceOKs.Contains(returnMsg))
                        {
                            //Cập nhật thành công
                            returnValue = true;
                        }
                        else
                        {
                            //Cập nhật thất bại
                            returnValue = false;
                        }
                    }
                    else
                    {
                        returnValue = false;
                        //Cập nhật thất bại
                    }
                }
                unitOfWork.HISChargeDetailRepository.Update(entity);
                if (isCommit && returnValue)
                    unitOfWork.Commit();
                return returnValue && true;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Confirm apply charge into (Belong) package
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool ConfirmChargeBelongPackage(ConfirmServiceInPackageModel model, bool IsCommit, out string strOutMsg, bool IsTranferredPackage = false, Guid? OldPatientInPackageId = null)
        {
            bool returnValue = false;
            string strMsg = string.Empty;
            try
            {
                #region Check current confirm belong package
                var patientEntity = unitOfWork.PatientInformationRepository.Find(x => x.PID == model.PID && !x.IsDeleted)?.FirstOrDefault();
                if (patientEntity != null)
                {
                    if (patientEntity.CurrentPatientInPackageId != model.PatientInPackageId)
                    {
                        //current process other patient in package
                        strOutMsg = Constant.Confirm_Apply_Charge_IsOtherPatientInPackage;
                        return false;
                    }
                    else if (patientEntity.CurrentUserProcess != unitOfWork.PatientInformationRepository.GetUserName())
                    {
                        //current process by other user
                        strOutMsg = Constant.Confirm_Apply_Charge_IsOtherUserProcess;
                        return false;
                    }
                }
                else
                {
                    //Not found patient with pid
                    strOutMsg = Constant.Patient_Not_Found;
                    return false;
                }
                #endregion
                #region Check current confirm belong package with children
                if (model.Children?.Count > 0)
                {
                    foreach (var item in model.Children)
                    {
                        var patientEntityChild = unitOfWork.PatientInformationRepository.Find(x => x.PID == item.PID && !x.IsDeleted)?.FirstOrDefault();
                        if (patientEntityChild != null)
                        {
                            if (patientEntityChild.CurrentPatientInPackageId != model.PatientInPackageId)
                            {
                                //current process other patient in package
                                strOutMsg = Constant.Confirm_Apply_Charge_IsOtherPatientInPackage;
                                return false;
                            }
                            else if (patientEntityChild.CurrentUserProcess != unitOfWork.PatientInformationRepository.GetUserName())
                            {
                                //current process by other user
                                strOutMsg = Constant.Confirm_Apply_Charge_IsOtherUserProcess;
                                return false;
                            }
                        }
                        else
                        {
                            //Not found patient with pid
                            strOutMsg = Constant.Patient_Not_Found;
                            return false;
                        }
                    }
                }
                #endregion
                #region Check SessionProcessId
                var PATInPkgEntity = !IsTranferredPackage ? unitOfWork.PatientInPackageRepository.Find(x => x.Id == model.PatientInPackageId)?.FirstOrDefault() :
                    unitOfWork.PatientInPackageRepository.Find(x => x.Id == OldPatientInPackageId)?.FirstOrDefault();
                if (PATInPkgEntity != null)
                {
                    if (PATInPkgEntity.SessionProcessId != model.SessionProcessId)
                    {
                        //current process by other session
                        strOutMsg = Constant.Confirm_Apply_Charge_IsOtherSession;
                        return false;
                    }
                }
                else
                {
                    //Not found patient with pid
                    strOutMsg = Constant.PATInPkg_Not_Found;
                    return false;
                }
                #endregion
                //tungdd14 lấy thêm thông tin lastStatus để check tái khám
                var patientInPackageStatus = PATInPkgEntity.Status;
                var patientInPackageLastStatus = PATInPkgEntity.LastStatus;
                var IsReExam = (patientInPackageStatus == (int)PatientInPackageEnum.RE_EXAMINATE || (patientInPackageStatus == (int)PatientInPackageEnum.EXPIRED && patientInPackageLastStatus == (int)PatientInPackageEnum.RE_EXAMINATE));
                if (model != null && model.listCharge != null)
                {
                    foreach (var item in model.listCharge)
                    {
                        if ((!item.IsChecked && item.InPackageType == (int)InPackageType.INPACKAGE))
                        {
                            //Trong gói mà ko tick chọn thì bỏ qua ko lưu
                            continue;
                        }
                        //tungdd14 Nếu là dịch vụ tái khám lần 2 trong gói thì cập nhật giá = 0
                        if (IsReExam && item.InPackageType == (int)InPackageType.INPACKAGE)
                        {
                            item.PkgPrice = 0;
                        }
                        //Cân nhắc ko where theo PatientInPackageId ở đoạn này. Đã thử đóng để cho test thử
                        //08-04/2022:Mới thêm filter (x.PatientInPackageId != item.PatientInPackageId && x.InPackageType!=(int)InPackageType.INPACKAGE)
                        HISChargeDetail entity = !IsTranferredPackage ?
                            unitOfWork.HISChargeDetailRepository.FirstOrDefault(x =>
                            //23-04-2022:Comment đoạn này
                            /*(x.PatientInPackageId==item.PatientInPackageId || (x.PatientInPackageId != item.PatientInPackageId && x.InPackageType != (int)InPackageType.INPACKAGE)) && */
                            x.HisChargeId == item.HisChargeId && !x.IsDeleted /*&& x.InPackageType==item.InPackageType*/)
                            //23-04-2022: Bổ sung TH khi tranfer
                            : unitOfWork.HISChargeDetailRepository.FirstOrDefault(x =>
                            //23-05-2022: Phubq thêm đk (x.PatientInPackageId == item.PatientInPackageId || (x.PatientInPackageId != OldPatientInPackageId && x.InPackageType != (int)InPackageType.INPACKAGE))
                            (
                                x.PatientInPackageId == item.PatientInPackageId || (x.PatientInPackageId != OldPatientInPackageId && x.InPackageType != (int)InPackageType.INPACKAGE)
                            ) && x.HisChargeId == item.HisChargeId && !x.IsDeleted);

                        bool isFirstConfirm = false;
                        if (item.PatientInPackageDetailId != null)
                        {
                            if (entity != null)
                            {
                                //Update
                                entity.HisChargeId = item.HisChargeId.Value;
                                entity.PatientInPackageDetailId = item.PatientInPackageDetailId.Value;
                                if (item.IsChecked)
                                {
                                    entity.PatientInPackageId = model.PatientInPackageId;
                                }
                                else
                                {
                                    entity.PatientInPackageId = item.PatientInPackageId.Value;
                                }
                                //entity.InPackageType = item.IsChecked ? item.InPackageType : (int)InPackageType.OUTSIDEPACKAGE;
                                entity.InPackageType = item.InPackageType != 0 ? item.InPackageType : (int)InPackageType.OUTSIDEPACKAGE;
                                entity.UnitPrice = item.Price;
                                entity.Quantity = item.QtyCharged;
                                entity.NetAmount = item.Amount;
                                entity.Notes = JsonConvert.SerializeObject(item.Notes);
                                entity.ChargeIsUseForReExam = IsReExam;

                                unitOfWork.HISChargeDetailRepository.Update(entity);
                            }
                            else
                            {
                                isFirstConfirm = true;
                                //Thêm mới
                                entity = new HISChargeDetail();
                                entity.HisChargeId = item.HisChargeId.Value;
                                entity.PatientInPackageDetailId = item.PatientInPackageDetailId.Value;
                                if (item.IsChecked)
                                {
                                    entity.PatientInPackageId = model.PatientInPackageId;
                                }
                                else
                                {
                                    entity.PatientInPackageId = item.PatientInPackageId.Value;
                                }
                                //entity.InPackageType = item.IsChecked ? item.InPackageType : (int)InPackageType.OUTSIDEPACKAGE;
                                entity.InPackageType = item.InPackageType != 0 ? item.InPackageType : (int)InPackageType.OUTSIDEPACKAGE;
                                entity.UnitPrice = item.Price;
                                entity.Quantity = item.QtyCharged;
                                entity.NetAmount = item.Amount;
                                entity.Notes = JsonConvert.SerializeObject(item.Notes);
                                entity.ChargeIsUseForReExam = IsReExam;

                                unitOfWork.HISChargeDetailRepository.Add(entity);
                            }
                        }

                        #region Update PatientInPackageId HisCharge
                        var hisChargeEntity = unitOfWork.HISChargeRepository.Find(x => x.Id == item.HisChargeId);
                        if (hisChargeEntity.Any())
                        {
                            foreach (var itemHis in hisChargeEntity)
                            {
                                if (isFirstConfirm)
                                {
                                    //Lưu lại giá chỉ định tại lần đầu xác nhận thuộc gói
                                    entity.ChargePrice = itemHis.UnitPrice;
                                }
                                item.ChargeId = itemHis.ChargeId;
                                if (item.PatientInPackageDetailId != null)
                                    //25-04-2022: Chuyển từ patientInPackage này sang patientInPackage khác. Thêm đk (item.IsChecked && item.IsBelongOtherPackakge)
                                    itemHis.PatientInPackageId = (item.IsChecked && item.IsBelongOtherPackakge) ? model.PatientInPackageId : entity.PatientInPackageId;
                                else
                                    itemHis.PatientInPackageId = item.PatientInPackageId;
                                unitOfWork.HISChargeRepository.Update(itemHis);
                            }
                        }
                        #region Cập nhật lại UnitPrice & NetAmount
                        if (item.PatientInPackageDetailId != null)
                        {
                            if (entity.InPackageType != (int)InPackageType.INPACKAGE && entity.InPackageType != (int)InPackageType.INVOICE_CANCELLED)
                            {
                                entity.UnitPrice = entity.ChargePrice;
                                entity.NetAmount = entity.ChargePrice * entity.Quantity;
                            }
                        }
                        #endregion .Cập nhật lại UnitPrice & NetAmount
                        #endregion .Update PatientInPackageId HisCharge
                    }
                    if (model.listCharge?.Count > 0 && model.listCharge.Any(x => x.IsChecked && x.InPackageType != (int)InPackageType.QTYINCHARGEGREATTHANREMAIN))
                    {
                        #region Update price for charge on OH
                        string returnMsg = string.Empty;
                        var returnUpdateOH = OHConnectionAPI.UpdateChargePrice(model.listCharge, out returnMsg);
                        if (returnUpdateOH)
                        {
                            if (Constant.StatusUpdatePriceOKs.Contains(returnMsg))
                            {
                                if (IsCommit)
                                {
                                    unitOfWork.Commit();
                                }
                                returnValue = true;
                                strMsg = returnMsg;
                                //Insert to queue to process update DIMS HisRevenue Service
                                #region Insert to queue to process update DIMS HisRevenue Service
                                foreach (var item in model.listCharge)
                                {
                                    var hisEntity = new HisChargeRevenueModel()
                                    {
                                        ChargeId = item.ChargeId,
                                        InPackageType = item.InPackageType,
                                        PatientInPackageId = item.PatientInPackageId,
                                        PackageCode = model.PackageCode,
                                        GroupPackageCode = model.GroupPackageCode
                                    };
                                    HisChargeQueue.Send(hisEntity);
                                }
                                #endregion .Insert to queue to process update DIMS HisRevenue Service
                            }
                            else
                            {
                                returnValue = false;
                                strMsg = returnMsg;
                            }
                        }
                        else
                        {
                            returnValue = false;
                            strMsg = returnMsg;
                        }
                        #endregion .Update price for charge on OH
                    }
                    else
                    {
                        if (IsCommit)
                        {
                            unitOfWork.Commit();
                        }
                        returnValue = true;
                        strMsg = "OK";
                    }

                }
            }
            catch (Exception ex)
            {
                VM.Common.CustomLog.accesslog.Error(string.Format("ConfirmChargeBelongPackage fail. Ex: {0}. DataPost: {1}", ex, JsonConvert.SerializeObject(model)));
            }
            strOutMsg = strMsg;
            return returnValue;
        }
        /// <summary>
        /// Calculate price/amount in package detai service for Patient
        /// </summary>
        /// <param name="listEntity"></param>
        /// <param name="pkgAmount"></param>
        /// <returns></returns>
        public List<PatientInPackageDetailModel> CalculateDetailPatientService(PackagePrice policy, List<PatientInPackageDetailModel> listEntity, double? pkgAmount, out int outStatus)
        {
            int outStatusValue = 1;
            #region Set price & amount in package
            PackageGroupRepo groupRepo = new PackageGroupRepo();
            var isVaccinePackage = Constant.ListGroupCodeIsVaccinePackage.Contains(groupRepo.GetPackageGroupRoot(policy.Package.PackageGroup)?.Code);
            if (pkgAmount > 0)
            {
                //Get rate Service
                var rate = GetRateInPackage(policy, listEntity, pkgAmount, 1, isVaccinePackage);
                var rateDrugConsum = GetRateInPackage(policy, listEntity, pkgAmount, 2, isVaccinePackage);
                if (rateDrugConsum == -2)
                {
                    outStatusValue = -2;
                }
                //tungdd14 tính giá vaccine theo hệ số
                var rateVC = (double?)null;
                if (isVaccinePackage && policy.RateINV != null && policy.RateINV > 0)
                {
                    var totalAmountpackage = listEntity.Sum(x => x.PkgPrice * x.Qty);
                    //(Giá gói - thành tiền VC trong gói)/Tổng thành tiền cơ sở của dịch vụ
                    rateVC = pkgAmount/ totalAmountpackage;
                }
                if (rate != null)
                {
                    foreach (var item in listEntity.Where(x => !x.IsServiceFreeInPackage))
                    {
                        //tungdd14 tính giá vaccine theo hệ số
                        if (isVaccinePackage && policy.RateINV != null && policy.RateINV > 0)
                        {
                            item.PkgPrice = item.PkgPrice * rateVC;
                            item.PkgAmount = item.PkgPrice * item.Qty;
                        }
                        else if (item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE && !item.IsPackageDrugConsum)
                        {
                            item.PkgPrice = item.BasePrice * rate;
                            //28-07-2022:Phubq bo lam tron don gia
                            //item.PkgPrice = item.PkgPrice != null ? Math.Round(item.PkgPrice.Value) : item.PkgPrice;
                            item.PkgPrice = item.PkgPrice != null ? item.PkgPrice.Value : item.PkgPrice;
                            item.PkgAmount = item.PkgPrice * item.Qty;
                            //item.PkgAmount = item.PkgPrice != null && item.Qty != null? Math.Round(item.PkgPrice.Value * item.Qty.Value): (double?)null;
                        }
                        else
                        {
                            item.PkgPrice = item.BasePrice * rateDrugConsum;
                            //28-07-2022:Phubq bo lam tron don gia
                            //item.PkgPrice = item.PkgPrice != null ? Math.Round(item.PkgPrice.Value) : item.PkgPrice;
                            item.PkgPrice = item.PkgPrice != null ? item.PkgPrice.Value : item.PkgPrice;
                            item.PkgAmount = item.PkgPrice * item.Qty;
                            //item.PkgAmount = item.PkgPrice != null && item.Qty != null ? Math.Round(item.PkgPrice.Value * item.Qty.Value) : (double?)null;
                        }
                    }
                }
            }
            else
            {
                var totalAmountDrugConsum = listEntity.Where(x => (x.IsPackageDrugConsum || x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM) && !x.IsServiceFreeInPackage).Sum(x => x.BaseAmount);
                if (totalAmountDrugConsum > pkgAmount && !isVaccinePackage)
                {
                    outStatusValue = -2;
                }
                listEntity.ForEach(x => { x.PkgPrice = 0; x.PkgAmount = 0; });
            }
            #endregion .Set price & amount in package
            outStatus = outStatusValue;
            return listEntity;
        }
        /// <summary>
        /// Get rate in package
        /// </summary>
        /// <param name="listModel"></param>
        /// <param name="pkgAmount"></param>
        /// <returns></returns>

        #endregion .Patient In Package detail (in service)
        #region Children in PatientInPackage
        public List<PatientInformationModel> GetChildrenByPatientInPackageId(Guid? patientInPackageId)
        {
            List<PatientInformationModel> listReturn = null;
            if (patientInPackageId != null)
            {
                var result = unitOfWork.PatientInPackageChildRepository.Find(x => x.PatientInPackageId == patientInPackageId && !x.IsDeleted);
                if (result.Any())
                {
                    listReturn = result.Select(x => new PatientInformationModel()
                    {
                        Id = x.PatientInformation?.Id,
                        PatientId = x.PatientInformation?.PatientId,
                        PID = x.PatientInformation?.PID,
                        FullName = x.PatientInformation?.FullName,
                        DateOfBirth = x.PatientInformation?.DateOfBirth,
                        Gender = x.PatientInformation?.Gender,
                        Email = x.PatientInformation?.Email,
                        Mobile = x.PatientInformation?.Mobile,
                        Address = x.PatientInformation?.Address,
                        National = x.PatientInformation?.National

                    })?.ToList();
                }
            }
            return listReturn;
        }
        #endregion Children in PatientInPackage
        #region Funtion Helper
        public double? GetRateInPackage(List<PatientInPackageDetailModel> listModel, double? pkgAmount)
        {
            if (listModel != null && listModel.Count > 0)
            {
                double? totalAmount = listModel.Where(x => !x.IsPackageDrugConsum && x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE).Sum(x => x.BaseAmount);
                double? totalAmount_DrugConSum = listModel.Where(x => x.IsPackageDrugConsum || x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM).Sum(x => x.BaseAmount);
                var rate = pkgAmount > 0 ? ((totalAmount != null && totalAmount != 0) ? (pkgAmount - totalAmount_DrugConSum) / totalAmount : 1) : 0;
                return rate;
            }
            return null;
        }
        /// <summary>
        /// rateType=1: Service
        /// rateType=2: Inventory
        /// </summary>
        /// <param name="policy"></param>
        /// <param name="listModel"></param>
        /// <param name="pkgAmount"></param>
        /// <param name="rateType"></param>
        /// <param name="isVaccinePacakge"></param>
        /// <returns></returns>
        public double? GetRateInPackage(PackagePrice policy, List<PatientInPackageDetailModel> listModel, double? pkgAmount, int rateType = 1, bool isVaccinePacakge = false)
        {

            if (listModel != null && listModel.Count > 0)
            {
                double? totalAmount = listModel.Where(x => !x.IsPackageDrugConsum && !x.IsServiceFreeInPackage).Sum(x => x.BaseAmount);
                double? totalAmount_SV = listModel.Where(x => !x.IsPackageDrugConsum && x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE && !x.IsServiceFreeInPackage).Sum(x => x.BaseAmount);
                double? totalAmount_DrugConSum = listModel.Where(x => (x.IsPackageDrugConsum || x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM) && !x.IsServiceFreeInPackage).Sum(x => x.BaseAmount);
                double? totalAmountInPK_DrugConSum = listModel.Where(x => (x.IsPackageDrugConsum || x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM) && !x.IsServiceFreeInPackage).Sum(x => x.PkgAmount);
                double? rate = 1;
                if (isVaccinePacakge)
                {
                    ////TH Gói Vaccine
                    //rate = pkgAmount > 0 ? ((totalAmount != null && totalAmount != 0) ? (pkgAmount)/ (totalAmount_SV + totalAmount_DrugConSum) : 1) : 0;
                    bool IsHaveItemDrugConsum = listModel.Any(x => x.IsPackageDrugConsum);
                    //TH Gói Vaccine
                    if (IsHaveItemDrugConsum)
                    {
                        if (rateType == 1)
                            rate = pkgAmount > 0 ? ((totalAmount_SV != null && totalAmount_SV != 0) ? (pkgAmount - totalAmount_DrugConSum) / totalAmount_SV : 1) : 0;
                    }
                    else
                    {
                        rate = pkgAmount > 0 ? ((totalAmount != null && totalAmount != 0) ? (pkgAmount) / (totalAmount_SV + totalAmount_DrugConSum) : 1) : 0;
                    }
                }
                else
                {
                    if (policy.IsLimitedDrugConsum)
                    {
                        if (rateType == 1)
                        {
                            //Tỷ lệ service
                            rate = (totalAmount_SV != null && totalAmount_SV != 0) ? (pkgAmount - (policy.LimitedDrugConsumAmount > 0 ? policy.LimitedDrugConsumAmount : 0)) / totalAmount_SV : 1;
                        }
                        else if (rateType == 2)
                        {
                            //2022-07-28:Phubq sua doan check thanh totalAmountInPK_DrugConSum
                            //if (pkgAmount < totalAmount_DrugConSum)
                            if (pkgAmount < totalAmountInPK_DrugConSum)
                            {
                                //TH giá sau giảm giá ck < tổng tiền thuốc và VTTH
                                rate = -2;
                            }
                            else
                            {
                                //Tỷ lệ thuốc/VTTH
                                rate = (totalAmount_DrugConSum != null && totalAmount_DrugConSum != 0) ? (policy.LimitedDrugConsumAmount > 0 ? policy.LimitedDrugConsumAmount : 0) / totalAmount_DrugConSum : 1;
                            }
                        }
                    }
                    else
                    {
                        if (rateType == 1)
                        {
                            rate = (totalAmount_SV != null && totalAmount_SV != 0) ? (pkgAmount - totalAmount_DrugConSum) / totalAmount_SV : 1;
                        }
                        if (rateType == 2)
                        {
                            //2022-07-28:Phubq sua doan check thanh totalAmountInPK_DrugConSum
                            //if (pkgAmount < totalAmount_DrugConSum)
                            if (pkgAmount < totalAmountInPK_DrugConSum)
                            {
                                //TH giá sau giảm giá ck < tổng tiền thuốc và VTTH
                                rate = -2;
                            }
                        }
                    }
                }

                return rate;
            }
            return null;
        }
        public void SetNotes4ConfirmServiceBelongPackage(string PIDOwner, ChargeInPackageModel entity, Guid crPatientInPackageId, int? iInPackageType = null)
        {
            List<MessageModel> listNode = null;
            if (entity.InPackageType != (int)InPackageType.QTYINCHARGEGREATTHANREMAIN)
            {
                //Build Notes
                if (entity.WasPackageId != null && entity.PatientInPackageId != crPatientInPackageId)
                {
                    //Đã được ghi nhận vào gói
                    var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.NOTE_CHARGE_BELONG_PACKAGE);
                    MessageModel mdMsg = (MessageModel)msg.Clone();
                    mdMsg.ViMessage = string.Format(msg.ViMessage, string.Format("{0} ({1})", entity.WasPackageName, entity.WasPackageCode));
                    mdMsg.EnMessage = string.Format(msg.EnMessage, string.Format("{0} ({1})", entity.WasPackageName, entity.WasPackageCode));
                    listNode = listNode?.Count > 0 ? listNode : new List<MessageModel>();
                    listNode.Add(mdMsg);
                    entity.IsChecked = false;
                    entity.Notes = listNode;
                }
                if (entity.RootId != null)
                {
                    // Get parent service information
                    var pEntity = unitOfWork.ServiceInPackageRepository.Find(x => x.Id == entity.RootId).FirstOrDefault();
                    if (pEntity != null)
                    {
                        //Dịch vụ thay thế
                        var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.NOTE_CHARGE_INSIDE_SERVICEREPLACE);
                        MessageModel mdMsg = (MessageModel)msg.Clone();
                        mdMsg.ViMessage = string.Format(msg.ViMessage, string.Format("{0} ({1})", pEntity.Service.ViName, pEntity.Service.Code));
                        mdMsg.EnMessage = string.Format(msg.EnMessage, string.Format("{0} ({1})", pEntity.Service.EnName, pEntity.Service.Code));
                        listNode = listNode?.Count > 0 ? listNode : new List<MessageModel>();
                        listNode.Add(mdMsg);
                        entity.Notes = listNode;
                    }
                }
                //Get and set note Cập nhật giá dịch vụ trong gói
                //24-05-2022: Phubq thêm đk: && entity.WasPackageId==null (Nếu chỉ định đã được ghi nhận vào gói hiện tại) 
                if (iInPackageType == (int)InPackageType.OVERPACKAGE && entity.InPackageType != iInPackageType && entity.WasPackageId == null)
                {
                    //Chỉ định thay đổi từ vượt gói -> trong gói
                    var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.NOTE_CHARGE_IS_UPDATE_PRICE);
                    MessageModel mdMsg = (MessageModel)msg.Clone();
                    listNode = listNode?.Count > 0 ? listNode : new List<MessageModel>();
                    listNode.Add(mdMsg);
                    entity.Notes = listNode;
                }
            }
            if (/*iInPackageType == (int)InPackageType.INPACKAGE && */PIDOwner != entity.PID)
            {
                //Được sử dụng bởi/ Using by
                var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.NOTE_CHARGE_USING_BY);
                MessageModel mdMsg = (MessageModel)msg.Clone();
                mdMsg.ViMessage = string.Format(msg.ViMessage, entity.PatientName);
                mdMsg.EnMessage = string.Format(msg.EnMessage, entity.PatientName);
                listNode = listNode?.Count > 0 ? listNode : new List<MessageModel>();
                listNode.Add(mdMsg);
                entity.Notes = listNode;
            }
            //26-05-2022:Phubq thêm đoạn này. set để show confirm chuyển chỉ định thuộc gói
            if (entity.WasPackageId != null && entity.PatientInPackageId != crPatientInPackageId)
            {
                entity.IsBelongOtherPackakge = true;
            }
        }
        /// <summary>
        /// Get note message replace service in package
        /// </summary>
        /// <param name="rootId"></param>
        /// <returns></returns>
        public List<MessageModel> GetListNotesReplaceService(Guid? rootId, bool isShowNoteReExam = false)
        {
            List<MessageModel> listNode = null;
            if (rootId != null)
            {
                // Get parent service information
                var pEntity = unitOfWork.ServiceInPackageRepository.Find(x => x.Id == rootId).FirstOrDefault();
                if (pEntity != null)
                {
                    //Dịch vụ thay thế
                    var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.NOTE_CHARGE_INSIDE_SERVICEREPLACE);
                    MessageModel mdMsg = (MessageModel)msg.Clone();
                    mdMsg.ViMessage = string.Format(msg.ViMessage, string.Format("{0} - {1}", pEntity.Service.Code, pEntity.Service.ViName));
                    mdMsg.EnMessage = string.Format(msg.EnMessage, string.Format("{0} - {1}", pEntity.Service.Code, pEntity.Service.EnName));
                    listNode = listNode?.Count > 0 ? listNode : new List<MessageModel>();
                    listNode.Add(mdMsg);
                }
            }
            //tungdd14 note trường hợp tái khám
            if (isShowNoteReExam)
            {
                var noteReExam = MessageManager.Messages.Find(x => x.Code == MessageCode.NOTEREEXAM);
                MessageModel mdMsgReExam = (MessageModel)noteReExam.Clone();
                mdMsgReExam.ViMessage = noteReExam.ViMessage;
                mdMsgReExam.EnMessage = noteReExam.EnMessage;
                listNode = listNode?.Count > 0 ? listNode : new List<MessageModel>();
                listNode.Add(mdMsgReExam);
            }
            return listNode;
        }
        /// <summary>
        /// Check to know have exist charge in package
        /// </summary>
        /// <param name="PatientInforId"></param>
        /// <param name="PackageId"></param>
        /// <param name="StartDate"></param>
        /// <param name="EndDate"></param>
        /// <returns></returns>
        public bool CheckDupplicateRegistered(Guid PatientInforId, Guid PackageId, DateTime StartDate, DateTime? EndDate, out PatientInPackage entityOverlap)
        {
            PatientInPackage outputEntity = null;
            bool returnValue = false;
            //var xquery= unitOfWork.PatientInPackageRepository.AsEnumerable().Where(x => !new List<int>() { (int)PatientInPackageEnum.CLOSED, (int)PatientInPackageEnum.CANCELLED, (int)PatientInPackageEnum.TERMINATED, (int)PatientInPackageEnum.TRANSFERRED }.Contains(x.Status) && x.PatientInforId == PatientInforId && x.PackagePriceSite.PackagePrice.PackageId == PackageId
            //         && ((x.StartAt <= StartDate && x.EndAt >= StartDate) || (x.StartAt <= EndDate && x.EndAt >= EndDate)
            //         || (x.StartAt >= StartDate && x.EndAt <= EndDate))
            //         );
            var xquery = unitOfWork.PatientInPackageRepository.Find(x => !new List<int>() { (int)PatientInPackageEnum.CLOSED, (int)PatientInPackageEnum.CANCELLED, (int)PatientInPackageEnum.TERMINATED, (int)PatientInPackageEnum.TRANSFERRED }.Contains(x.Status) && x.PatientInforId == PatientInforId && x.PackagePriceSite.PackagePrice.PackageId == PackageId
                      && ((x.StartAt <= StartDate && x.EndAt >= StartDate) || (x.StartAt <= EndDate && x.EndAt >= EndDate)
                      || (x.StartAt >= StartDate && x.EndAt <= EndDate))
                     );
            if (xquery.Any())
            {
                outputEntity = xquery.FirstOrDefault();
                returnValue = true;
            }
            entityOverlap = outputEntity;
            return returnValue;
        }
        public bool CheckExistChargeInsidePackage(Guid PatientInPackageId)
        {
            //return unitOfWork.HISChargeDetailRepository.AsEnumerable().Any(x => !x.IsDeleted && x.PatientInPackageId == PatientInPackageId && x.InPackageType == (int)InPackageType.INPACKAGE);
            return unitOfWork.HISChargeDetailRepository.Find(x => !x.IsDeleted && x.PatientInPackageId == PatientInPackageId && x.InPackageType == (int)InPackageType.INPACKAGE).Any();
        }
        /// <summary>
        /// Check to known have charge been invoiced
        /// </summary>
        /// <param name="PatientInPackageId"></param>
        /// <returns></returns>
        public bool CheckExistChargeInsidePackageInvoiced(Guid PatientInPackageId)
        {
            //return unitOfWork.HISChargeDetailRepository.AsEnumerable().Any(x => !x.IsDeleted && x.PatientInPackageId == PatientInPackageId && x.InPackageType == (int)InPackageType.INPACKAGE && x.HISCharge.InvoicePaymentStatus== Constant.PAYMENT_PSL_STATUS);
            return unitOfWork.HISChargeDetailRepository.Find(x => !x.IsDeleted && x.PatientInPackageId == PatientInPackageId && x.InPackageType == (int)InPackageType.INPACKAGE && x.HISCharge.InvoicePaymentStatus == Constant.PAYMENT_PSL_STATUS).Any();
        }
        public bool CheckChildExistServiceUsingInPatientPackage(string pid, Guid PatientInPackageId)
        {
            return unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageId == PatientInPackageId && x.HISCharge.PID == pid && x.InPackageType == (int)InPackageType.INPACKAGE && !x.IsDeleted).Any();
        }
        public PatientInformation CheckChildBelongOtherMother(string pid, Guid PatientInPackageId)
        {
            PatientInformation returnValue = null;
            //get patient in patient package
            var guidMother = unitOfWork.PatientInPackageRepository.FirstOrDefault(x => x.Id == PatientInPackageId)?.PatientInforId;
            if (guidMother != null)
            {
                returnValue = unitOfWork.PatientInPackageChildRepository.Find(x => x.PatientInformation.PID == pid && x.PatientInPackage.PatientInforId != guidMother && !x.IsDeleted)?.Select(x => x.PatientInPackage?.PatientInformation)?.FirstOrDefault();
            }
            return returnValue;
        }
        public bool CreateOrUpdateChild(Guid patientInPackageId, List<PatientInformationModel> listChild, bool IsCommit = true)
        {
            bool returnValue = false;
            if (listChild?.Count > 0)
            {
                try
                {
                    var _repo = new PatientInPackageRepo();
                    foreach (var itemChild in listChild)
                    {
                        var itemInDB = unitOfWork.PatientInPackageChildRepository.FirstOrDefault(x => x.PatientInformation.PID == itemChild.PID && x.PatientInPackageId == patientInPackageId);
                        if (itemInDB != null)
                        {
                            //Đã tồn tại trong DB
                            var patient = _repo.SyncPatient(itemChild.PID);
                            if (patient != null)
                            {
                                itemInDB.PatientChildInforId = patient.Id;
                            }
                            unitOfWork.PatientInPackageChildRepository.Update(itemInDB);
                        }
                        else
                        {
                            //Get and Syn Patient from OH
                            var patient = _repo.SyncPatient(itemChild.PID);
                            if (patient != null)
                            {
                                itemInDB = new PatientInPackageChild();
                                itemInDB.PatientChildInforId = patient.Id;
                                itemInDB.PatientInPackageId = patientInPackageId;
                                itemInDB.IsDeleted = false;
                                unitOfWork.PatientInPackageChildRepository.Add(itemInDB);
                            }
                        }
                    }
                    if (IsCommit)
                        unitOfWork.Commit();
                    returnValue = true;
                }
                catch (Exception ex)
                {
                    VM.Common.CustomLog.accesslog.Error(string.Format("CreateOrUpdateChild fail. Ex: {0}", ex));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Update current patient in package id
        /// </summary>
        /// <param name="patientId"></param>
        /// <param name="currentPatientInPackageId"></param>
        /// <returns></returns>
        public bool UpdateCurrentPatientInPackageId(Guid patientId, List<PatientInformationModel> children, Guid? currentPatientInPackageId, Guid patientInPackageId, Guid? sessionProcessId)
        {
            using (var unitOfWorkLocal = new EfUnitOfWork())
            {
                bool updatePatient = false;
                bool updatePATInPackage = false;
                var xPAT = unitOfWorkLocal.PatientInformationRepository.AsEnumerable().Where(x => x.Id == patientId || (children != null && children.Any(y => y.PID == x.PID)));
                if (xPAT.Any())
                {
                    foreach (var item in xPAT)
                    {
                        item.CurrentPatientInPackageId = currentPatientInPackageId;
                        item.CurrentUserProcess = unitOfWorkLocal.PatientInformationRepository.GetUserName();
                        unitOfWorkLocal.PatientInformationRepository.Update(item);
                    }
                    updatePatient = true;
                }
                var entityPATInPkg = unitOfWorkLocal.PatientInPackageRepository.Find(x => x.Id == patientInPackageId)?.FirstOrDefault();
                if (entityPATInPkg != null)
                {
                    entityPATInPkg.SessionProcessId = sessionProcessId;
                    unitOfWorkLocal.PatientInPackageRepository.Update(entityPATInPkg);
                    updatePATInPackage = true;
                }
                if (updatePatient && updatePATInPackage)
                {
                    unitOfWorkLocal.Commit();
                    return true;
                }
                return false;
            }
        }
        #endregion .Funtion Helper
        #region For Migrate
        public IQueryable<Temp_PatientInPackage> GetPatientInPackagesForMigrate()
        {
            //var results = unitOfWork.Temp_PatientInPackageRepository.AsEnumerable().Where(x => string.IsNullOrEmpty(x.Notes));
            var results = unitOfWork.Temp_PatientInPackageRepository.Find(x => string.IsNullOrEmpty(x.Notes));
            return results.AsQueryable();
        }
        public static void UpdateHisChargeNextTimeForProcess(HisChargeRevenueModel entity)
        {
            using (IUnitOfWork unitOfWorkLocal = new EfUnitOfWork())
            {
                var hisEntity = unitOfWorkLocal.HISChargeRepository.FirstOrDefault(
                e => e.ChargeId == entity.ChargeId
                );
                if (hisEntity != null)
                {
                    hisEntity.NextProcessTime = entity.NextTimeForProcess;
                    unitOfWorkLocal.HISChargeRepository.Update(hisEntity, is_time_change: false);
                    unitOfWorkLocal.Commit();
                }
            }
        }
        #endregion .For Migrate
    }
}