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
    public class AutoUpdateRefundServiceJob : IJob
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
            CustomLog.intervaljoblog.Info($"<Auto Update refund service for patient> Start!");
            try
            {
                //Cần filter các gói cần refund
                List<string> listPk = new List<string>() { "DP2020.10-07" };
                var results = unitOfWork.Temp_PatientInPackageRepository.AsEnumerable().Where(x => x.StatusForProcess == 3 && listPk.Contains(x.PackageCode) && x.PID== "817028470" && string.IsNullOrEmpty(x.Notes));
                CustomLog.intervaljoblog.Info(string.Format("<Auto Update refund service for patient> Total item: {0}", results?.Count()));
                if (results.Any())
                {
                    foreach(var item in results)
                    {
                        //Find PatientInPackage
                        var xQueryPiPkg = unitOfWork.PatientInPackageRepository.Find(x => !x.IsDeleted && x.PackagePriceSite.PackagePrice.Package.Code == item.PackageCode && x.PatientInformation.PID == item.PID && x.Status==(int)PatientInPackageEnum.ACTIVATED);
                        if (xQueryPiPkg.Any())
                        {
                            foreach(var itemPiPkg in xQueryPiPkg)
                            {
                                bool isReset = false;
                                var xQueryPiPkgDt = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId==itemPiPkg.Id && !x.IsDeleted);
                                if (xQueryPiPkgDt.Any())
                                {
                                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update refund service for patient> package [{0}] for patient [{1}] Begin", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                    //Cập nhật giá trên OH
                                    #region Update price charge on OH
                                    var xQueryChargeDetail = unitOfWork.HISChargeDetailRepository.AsQueryable().Where(x => !x.IsDeleted && x.PatientInPackageId == itemPiPkg.Id && x.InPackageType == (int)InPackageType.INPACKAGE);
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
                                                    //Cập nhật refund giá gốc OK
                                                    //Đánh dấu cần reset lại tình hình sử dụng gói về ban đầu
                                                    isReset = true;
                                                }
                                                else if (returnMsg == Constant.StatusUpdatePriceError_No_User)
                                                {
                                                    //Cập nhật refund giá gốc Not OK. User ko tồn tại trên OH
                                                    item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. User ko tồn tại trên OH" : "User ko tồn tại trên OH";
                                                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update refund service for patient> package [{0}] for patient [{1}] Not exist User OH", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                                }
                                                else
                                                {
                                                    //Cập nhật refund giá gốc Not OK
                                                    item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. Cập nhật refund giá gốc Not OK: {returnMsg}" : $"Cập nhật refund giá gốc Not OK: {returnMsg}";
                                                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update refund service for patient> package [{0}] for patient [{1}] Not OK", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                                }
                                            }
                                            else
                                            {
                                                if (string.IsNullOrEmpty(returnMsg))
                                                {
                                                    // Cập nhật refund giá gốc Not OK
                                                    item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. Cập nhật refund giá gốc Not OK" : "Cập nhật refund giá gốc Not OK";
                                                }
                                                else if (returnMsg == Constant.StatusUpdatePriceError_No_User)
                                                {
                                                    //Cập nhật refund giá gốc Not OK. User ko tồn tại trên OH
                                                    item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. User ko tồn tại trên OH" : "User ko tồn tại trên OH";
                                                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update refund service for patient> package [{0}] for patient [{1}] Not exist User OH", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                                }
                                                else
                                                {
                                                    // Cập nhật refund giá gốc Not OK
                                                    item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. Cập nhật refund giá gốc Not OK" : "Cập nhật refund giá gốc Not OK";
                                                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update refund service for patient> package [{0}] for patient [{1}] Not OK", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //Đánh dấu cần reset lại tình hình sử dụng gói về ban đầu
                                        isReset = true;
                                    }
                                    #endregion .Update price charge on OH
                                    if (isReset)
                                    {
                                        foreach (var itemPiPkgDt in xQueryPiPkgDt)
                                        {
                                            itemPiPkgDt.QtyWasUsed = 0;
                                            itemPiPkgDt.QtyRemain = itemPiPkgDt.ServiceInPackage.LimitQty;
                                            unitOfWork.PatientInPackageDetailRepository.Update(itemPiPkgDt);
                                        }
                                        //Xóa tất cả HisChargeDetails
                                        var xHisChargeDetail=unitOfWork.HISChargeDetailRepository.Find(x => x.PatientInPackageId == itemPiPkg.Id);
                                        if (xHisChargeDetail.Any())
                                            unitOfWork.HISChargeDetailRepository.HardDeleteRange(xHisChargeDetail.AsQueryable());
                                        //Xóa tất cả HisCharge
                                        var xHisCharge = unitOfWork.HISChargeRepository.Find(x => x.PatientInPackageId == itemPiPkg.Id);
                                        if (xHisCharge.Any())
                                            unitOfWork.HISChargeRepository.HardDeleteRange(xHisCharge.AsQueryable());
                                        item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. Đã refund thành công" : "Đã refund thành công";
                                        CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update refund service for patient> package [{0}] for patient [{1}] Success", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                    }
                                }
                            }
                        }
                    }
                    unitOfWork.Commit();
                }
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Auto Update refund service for patient> Error: {0}", ex));
            }
            Globals.IsAutoCalculatePriceProcessing = false;
        }
        #region Function Helper
       
        #endregion .Function Helper
    }
}
