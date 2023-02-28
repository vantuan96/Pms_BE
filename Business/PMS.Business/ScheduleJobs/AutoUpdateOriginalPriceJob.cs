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
    public class AutoUpdateOriginalPriceJob : IJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        protected PackageRepo _repoPkg = new PackageRepo();
        protected PatientInPackageRepo _repo = new PatientInPackageRepo();
        public void Execute(IJobExecutionContext context)
        {
            if (Globals.IsAutoCalculatePriceProcessing)
                return;
            Globals.IsAutoCalculatePriceProcessing = true;
            CustomLog.intervaljoblog.Info($"<Auto Update update original price for charge> Start!");
            try
            {
                //Cần filter các gói cần refund
                List<string> listPk = new List<string>() { "DP2020.10-01" };
                var results = unitOfWork.Temp_UpdateOriginalPriceRepository.AsEnumerable().Where(x => x.StatusForProcess == 1 /*&& listPk.Contains(x.PackageCode) && x.PID == "817024059"*/ && string.IsNullOrEmpty(x.Notes));
                CustomLog.intervaljoblog.Info(string.Format("<Auto Update update original price for charge> Total item: {0}", results?.Count()));
                if (results.Any())
                {
                    foreach(var item in results)
                    {
                        //Find HISChargeDetails
                        var xQueryHisDT = unitOfWork.HISChargeDetailRepository.Find(x => !x.IsDeleted && x.HISCharge.PID == item.PID && x.HISCharge.ItemCode==item.ServiceCode && x.PatientInPackage.Status == (int)PatientInPackageEnum.ACTIVATED);
                        if (xQueryHisDT.Any())
                        {
                            foreach(var itemHisDT in xQueryHisDT)
                            {
                                //Cập nhật giá gốc khi chỉ định
                                itemHisDT.ChargePrice = item.OrginalPrice;
                                if(item.IsUpdateOH && (itemHisDT.InPackageType == (int)InPackageType.OVERPACKAGE))
                                {
                                    itemHisDT.UnitPrice = item.OrginalPrice;
                                    itemHisDT.NetAmount = item.OrginalPrice * itemHisDT.Quantity;
                                    //Update price on OH
                                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update update original price for charge> ChargeId [{0}] for HISChargeDetailId [{1}] & Price [{2}] Begin", itemHisDT.HISCharge.ChargeId, itemHisDT.Id, item.OrginalPrice), Constant.Log_Type_Info, printConsole: true);
                                    List<ChargeInPackageModel> listCharges = new List<ChargeInPackageModel>();
                                    listCharges.Add(
                                    new ChargeInPackageModel(){ 
                                        IsChecked = true,
                                        ChargeId = itemHisDT.HISCharge.ChargeId,
                                        InPackageType = itemHisDT.InPackageType,
                                        Price = item.OrginalPrice,
                                        //Cập nhật lại giá tại thời điểm chỉ định
                                        PkgPrice = item.OrginalPrice,
                                    });
                                    if (listCharges != null)
                                    {
                                        string returnMsg = string.Empty;
                                        var returnUpdateOH = OHConnectionAPI.UpdateChargePrice(listCharges, out returnMsg);
                                        if (returnUpdateOH)
                                        {
                                            if (Constant.StatusUpdatePriceOKs.Contains(returnMsg))
                                            {
                                                //Cập nhật refund giá gốc OK
                                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update update original price for charge> ChargeId [{0}] for HISChargeDetailId [{1}] & Price [{2}] Update OH Price Success", itemHisDT.HISCharge.ChargeId, itemHisDT.Id, item.OrginalPrice), Constant.Log_Type_Info, printConsole: true);
                                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_SUCCESS"];
                                            }
                                            else if (returnMsg == Constant.StatusUpdatePriceError_No_User)
                                            {
                                                //Cập nhật refund giá gốc Not OK. User ko tồn tại trên OH
                                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. User ko tồn tại trên OH" : "User ko tồn tại trên OH";
                                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update update original price for charge> ChargeId [{0}] for HISChargeDetailId [{1}] & Price [{2}] Not exist User OH", itemHisDT.HISCharge.ChargeId, itemHisDT.Id, item.OrginalPrice), Constant.Log_Type_Info, printConsole: true);
                                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                            }
                                            else
                                            {
                                                //Cập nhật refund giá gốc Not OK
                                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. Cập nhật refund giá gốc Not OK: {returnMsg}" : $"Cập nhật refund giá gốc Not OK: {returnMsg}";
                                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update update original price for charge> ChargeId [{0}] for HISChargeDetailId [{1}] & Price [{2}] Not OK", itemHisDT.HISCharge.ChargeId, itemHisDT.Id, item.OrginalPrice), Constant.Log_Type_Info, printConsole: true);
                                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(returnMsg))
                                            {
                                                // Cập nhật refund giá gốc Not OK
                                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. Cập nhật refund giá gốc Not OK" : "Cập nhật refund giá gốc Not OK";
                                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                            }
                                            else if (returnMsg == Constant.StatusUpdatePriceError_No_User)
                                            {
                                                //Cập nhật refund giá gốc Not OK. User ko tồn tại trên OH
                                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. User ko tồn tại trên OH" : "User ko tồn tại trên OH";
                                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update update original price for charge> ChargeId [{0}] for HISChargeDetailId [{1}] & Price [{2}] Not exist User OH", itemHisDT.HISCharge.ChargeId, itemHisDT.Id, item.OrginalPrice), Constant.Log_Type_Info, printConsole: true);
                                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                            }
                                            else
                                            {
                                                // Cập nhật refund giá gốc Not OK
                                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? $"{item.Notes}. Cập nhật refund giá gốc Not OK" : "Cập nhật refund giá gốc Not OK";
                                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update update original price for charge> ChargeId [{0}] for HISChargeDetailId [{1}] & Price [{2}] Not OK", itemHisDT.HISCharge.ChargeId, itemHisDT.Id, item.OrginalPrice), Constant.Log_Type_Info, printConsole: true);
                                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update update original price for charge> ChargeId [{0}] for HISChargeDetailId [{1}] & Price [{2}] Update original price on Success", itemHisDT.HISCharge.ChargeId, itemHisDT.Id, item.OrginalPrice), Constant.Log_Type_Info, printConsole: true);
                                    item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_SUCCESS"];
                                }
                            }
                        }
                    }
                    unitOfWork.Commit();
                    CustomLog.Instant.IntervalJobLog($"<Auto Update update original price for charge> DONE!", Constant.Log_Type_Info, printConsole: true);
                }
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Auto Update update original price for charge> Error: {0}", ex));
            }
            Globals.IsAutoCalculatePriceProcessing = false;
        }
        #region Function Helper
       
        #endregion .Function Helper
    }
}
