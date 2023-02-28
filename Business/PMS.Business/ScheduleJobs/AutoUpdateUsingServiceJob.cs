using DataAccess.Models;
using DataAccess.Repository;
using Newtonsoft.Json;
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
    public class AutoUpdateUsingServiceJob : IJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        protected PackageRepo _repoPkg = new PackageRepo();
        protected PatientInPackageRepo _repo = new PatientInPackageRepo();
        protected HisRevenueRepo _repoHis = new HisRevenueRepo();
        public void Execute(IJobExecutionContext context)
        {
            if (Globals.IsAutoCalculatePriceProcessing)
                return;
            Globals.IsAutoCalculatePriceProcessing = true;
            CustomLog.intervaljoblog.Info($"<Auto Update using service for patient> Start!");
            try
            {
                //Get top list service using to process
                var listEntity = _repoHis.GetServiceUsingForConfirmApplyPackage(1000);
                if (listEntity?.Count > 0)
                {
                    foreach(var item in listEntity)
                    {
                        var usingEntity = unitOfWork.Temp_ServiceUsingRepository.FirstOrDefault(x => x.Id == item.Id);
                        string packageCode = item?.PackageCode;
                        string sPid = item?.PID;
                        if (string.IsNullOrEmpty(sPid) || string.IsNullOrEmpty(packageCode))
                        {
                            CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> PID or PackageCode is null"));
                            usingEntity.Notes = "PId hoặc Package có giá trị trống";
                            usingEntity.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                            unitOfWork.Temp_ServiceUsingRepository.Update(usingEntity);
                            unitOfWork.Commit();
                            continue;
                        }
                        //Get PatientInPackage 
                        var xPkgQuery = unitOfWork.PatientInPackageRepository.Find(x=>x.PackagePriceSite.PackagePrice.Package.Code== packageCode && x.PatientInformation.PID==sPid);
                        if (!xPkgQuery.Any())
                        {
                            CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Not Found package", item.PackageCode, item.PID));
                            usingEntity.Notes = "KH chưa đăng ký gói này";
                            usingEntity.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                            unitOfWork.Temp_ServiceUsingRepository.Update(usingEntity);
                            unitOfWork.Commit();
                            continue;
                        }
                        //Get Charge from OH
                        //Get List charge via PID
                        List<HISChargeModel> oHEntities = OHConnectionAPI.GetCharges(sPid, string.Empty, string.Empty);
                        if (oHEntities?.Count > 0)
                        {
                            var xCharge = oHEntities.Where(x => x.ItemCode == item.ServiceCode && x.Quantity == item.UsingNumber);
                            var xChargeMapped = unitOfWork.HISChargeDetailRepository.AsEnumerable();
                            var xChargeMap = xCharge.Where(x => !xChargeMapped.Any(y => y.HISCharge.ChargeId == x.ChargeId));
                            StepNotFoundChargeMapping:
                            if (!xChargeMap.Any())
                            {
                                //Không tìm thấy charge nào phù hợp
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Not Found charge", item.PackageCode, item.PID));
                                usingEntity.Notes = "Không tìm thấy chỉ định phù hợp";
                                usingEntity.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_ServiceUsingRepository.Update(usingEntity);
                                unitOfWork.Commit();
                                continue;
                            }
                            var chargeUsing = xChargeMap.OrderBy(x => x.ChargeDate).FirstOrDefault();
                            if (chargeUsing == null)
                            {
                                goto StepNotFoundChargeMapping;
                            }
                            //Store Charge into Database
                            CreateOrUpdateHisCharge(chargeUsing);
                            //Tìm gói dịch vụ phù hợp
                            var pkgEntity = unitOfWork.PatientInPackageDetailRepository.AsEnumerable().Where(x => xPkgQuery.Any(y => y.Id == x.PatientInPackageId) && x.ServiceInPackage.Service.Code == item.ServiceCode && x.QtyRemain>= item.UsingNumber)?.OrderBy(x=>x.CreatedAt).Select(x => x.PatientInPackage).FirstOrDefault();
                            if (pkgEntity == null)
                            {
                                //Không tìm thấy gói dịch vụ nào phù hợp
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Not Found patient in package mapping", item.PackageCode, item.PID));
                                usingEntity.Notes = "Không tìm thấy gói dịch vụ nào phù hợp";
                                usingEntity.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_ServiceUsingRepository.Update(usingEntity);
                                unitOfWork.Commit();
                                continue;
                            }
                            //Khởi tạo ConfirmServiceInPackage Model
                            ConfirmServiceInPackageModel modelConfirmCharge = new ConfirmServiceInPackageModel();
                            modelConfirmCharge.PatientInPackageId = pkgEntity.Id;
                            #region Create list charge to confirm belongs package
                            List<ChargeInPackageModel> listCharge = new List<ChargeInPackageModel>();
                            ChargeInPackageModel chargeCfEntity = new ChargeInPackageModel();
                            //Find PatientInPackageDetails
                            var piPkgEntity = unitOfWork.PatientInPackageDetailRepository.FirstOrDefault(x => x.PatientInPackageId == pkgEntity.Id && (x.ServiceInPackage.Service.Code == item.ServiceCode));
                            if (piPkgEntity == null)
                            {
                                CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Not found service in package mapping", item.PackageCode, item.PID));
                                usingEntity.Notes = "Không tìm thấy dịch vụ trong gói phù hợp";
                                usingEntity.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_ServiceUsingRepository.Update(usingEntity);
                                unitOfWork.Commit();
                            }
                            chargeCfEntity.PatientInPackageId = pkgEntity.Id;
                            chargeCfEntity.HisChargeId = chargeUsing.Id;
                            chargeCfEntity.ChargeId = chargeUsing.ChargeId;
                            chargeCfEntity.PatientInPackageDetailId = piPkgEntity.Id;
                            chargeCfEntity.InPackageType = (int)InPackageType.INPACKAGE;
                            chargeCfEntity.QtyCharged = item.UsingNumber;
                            chargeCfEntity.QtyRemain = 0;
                            chargeCfEntity.PkgPrice = piPkgEntity?.PkgPrice;
                            chargeCfEntity.Price = piPkgEntity?.PkgPrice;
                            chargeCfEntity.Amount = chargeCfEntity.Price * item.UsingNumber;
                            chargeCfEntity.IsChecked = true;
                            listCharge.Add(chargeCfEntity);
                            modelConfirmCharge.listCharge = listCharge;
                            #endregion .Create list charge to confirm belongs package
                            //Ghi nhận chỉ định vào gói
                            string outMsg = string.Empty;
                            var returnValueCf = ConfirmChargeBelongPackage(modelConfirmCharge, IsCommit: false, out outMsg);
                            if (returnValueCf && Constant.StatusUpdatePriceOKs.Contains(outMsg))
                            {
                                //CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Success", item.PackageCode, item.PID));
                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Success", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                usingEntity.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_SUCCESS"];
                                usingEntity.ChargeId = chargeCfEntity.ChargeId;
                                unitOfWork.Temp_ServiceUsingRepository.Update(usingEntity);
                                //Update statistic using service
                                UpgradeStatictisUsingServiceInPackage(pkgEntity.Id);
                                unitOfWork.Commit();
                            }
                            else
                            {
                                //CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Success", item.PackageCode, item.PID));
                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Fail", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                usingEntity.Notes = outMsg;
                                usingEntity.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                unitOfWork.Temp_ServiceUsingRepository.Update(usingEntity);
                                unitOfWork.Commit();
                            }
                        }
                        else
                        {
                            //Không tìm thấy charge nào
                            //CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Not Found charge", item.PackageCode, item.PID));
                            CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update using service for patient> Data [packageCode: {0}] for patient [{1}] Not Found charge", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                            usingEntity.Notes = "Không tìm thấy chỉ định phù hợp";
                            usingEntity.StatusForProcess= Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                            //usingEntity.NextProcessTime = DateTime.Now.AddMinutes(ConfigHelper.CF_ExMinutesToNextProcess * (usingEntity.ProcessNumber + 1));
                            unitOfWork.Temp_ServiceUsingRepository.Update(usingEntity);
                            unitOfWork.Commit();
                            continue;
                        }
                    }
                }
                //CustomLog.intervaljoblog.Info($"<Auto Update using service for patient> Success!");
                CustomLog.Instant.IntervalJobLog($"<Auto Update using service for patient> Success!", Constant.Log_Type_Info, printConsole: true);
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Auto Update using service for patient> Error: {0}", ex));
            }
            Globals.IsAutoCalculatePriceProcessing = false;
        }
        #region Function Helper
        private HISCharge CreateOrUpdateHisCharge(HISChargeModel model)
        {
            HISCharge entity = unitOfWork.HISChargeRepository.FirstOrDefault(
                e => e.ChargeId == model.ChargeId &&
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
            if (model.PatientInPackageId != null)
            {
                entity.PatientInPackageId = model.PatientInPackageId;
            }
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
        private bool ConfirmChargeBelongPackage(ConfirmServiceInPackageModel model, bool IsCommit, out string strOutMsg)
        {
            bool returnValue = false;
            string strMsg = string.Empty;
            try
            {
                if (model != null && model.listCharge != null)
                {
                    foreach (var item in model.listCharge)
                    {
                        if ((!item.IsChecked && item.InPackageType == (int)InPackageType.INPACKAGE))
                        {
                            //Trong gói mà ko tick chọn thì bỏ qua ko lưu
                            continue;
                        }
                        //Cân nhắc ko where theo PatientInPackageId ở đoạn này
                        HISChargeDetail entity = unitOfWork.HISChargeDetailRepository.FirstOrDefault(x => x.PatientInPackageId == item.PatientInPackageId && x.HisChargeId == item.HisChargeId && !x.IsDeleted /*&& x.InPackageType==item.InPackageType*/);
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
                                    itemHis.PatientInPackageId = entity.PatientInPackageId;
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
        private void UpgradeStatictisUsingServiceInPackage(Guid patientinpackageid)
        {
            var xQuery = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == patientinpackageid && !x.IsDeleted && !x.ServiceInPackage.IsDeleted);
            if (xQuery.Any())
            {
                foreach (var item in xQuery?.ToList())
                {
                    //QtyWasUsed
                    var QtyCharged = unitOfWork.HISChargeDetailRepository.AsEnumerable().Where(x => x.PatientInPackageDetailId == item.Id && x.InPackageType == (int)InPackageType.INPACKAGE && !x.IsDeleted).Sum(x => x.Quantity);
                    item.QtyWasUsed = QtyCharged;
                    //get total was used
                    var TotalQtyCharged = unitOfWork.HISChargeDetailRepository.AsEnumerable().Where(x => !x.IsDeleted && x.PatientInPackageId == patientinpackageid && (x.PatientInPackageDetailId == item.Id
                        || (x.PatientInPackageDetail.ServiceInPackage.RootId != null && x.PatientInPackageDetail.ServiceInPackage.RootId == item.ServiceInPackageId)
                        || (item.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId == x.PatientInPackageDetail.ServiceInPackageId)
                        || (x.PatientInPackageDetail.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId != null && item.ServiceInPackage.RootId == x.PatientInPackageDetail.ServiceInPackage.RootId)
                        )
                        && x.InPackageType == (int)InPackageType.INPACKAGE).Sum(x => x.Quantity);
                    item.QtyRemain = item.ServiceInPackage.LimitQty - TotalQtyCharged;
                    //Định mức còn lại >=0
                    item.QtyRemain = item.QtyRemain > 0 ? item.QtyRemain : 0;
                    unitOfWork.PatientInPackageDetailRepository.Update(item);

                    CustomLog.intervaljoblog.Info(string.Format("<Update Patient's Package detail QtyWasUsed> Info: [Code: {0}; Name: {1}; PatientInPackageId: {2}; QtyWasUsed: {3}]", item.ServiceInPackage.Service.Code, item.ServiceInPackage.Service.ViName, patientinpackageid, QtyCharged));
                }
            }
        }
        #endregion .Function Helper
    }
}
