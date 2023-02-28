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
    public class AutoUpdateUsingServiceV2Job : IJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        protected PackageRepo _repoPkg = new PackageRepo();
        protected PatientInPackageRepo _repo = new PatientInPackageRepo();
        public void Execute(IJobExecutionContext context)
        {
            if (Globals.IsAutoCalculatePriceProcessing)
                return;
            Globals.IsAutoCalculatePriceProcessing = true;
            CustomLog.intervaljoblog.Info($"<Auto Update using service for patient> Start!");
            try
            {
                List<string> listPID = new List<string>() { "200218661", "200293471", "200918520", "200723448", "200910470" };
                //List<string> listPackageCode = new List<string>() { "HPQ.DP2022-02.VN", "HDN.DP2022-02.VN", "HDN.MP2022-01.VN", "HDN.DP2022-01.VN" };
                var results = unitOfWork.Temp_PatientInPackageRepository.AsEnumerable().Where(x=> x.StatusForProcess== ConfigHelper.CF_StatusForProcess && listPID.Contains(x.PID) /*&& listPackageCode.Contains(x.PackageCode)*/ && string.IsNullOrEmpty(x.Notes));
                CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> Total item: {0}", results?.Count()));
                foreach (var item in results)
                {
                    if (string.IsNullOrEmpty(item.PID) || string.IsNullOrEmpty(item.PackageCode))
                    {
                        CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> PID or PackageCode is null"));
                        item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "PID hoăc PackageCode null") : "PID hoăc PackageCode null";
                        item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                        unitOfWork.Temp_PatientInPackageRepository.Update(item);
                        continue;
                    }
                    if (item.NetAmount==null)
                    {
                        CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> update using service [{0}] for patient [{1}] NetAmount is null", item.PackageCode, item.PID));
                        item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "Giá chiết khấu/sau giảm giá null") : "Giá chiết khấu/sau giảm giá null";
                        item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_FAIL"];
                        unitOfWork.Temp_PatientInPackageRepository.Update(item);
                        //unitOfWork.Commit();
                        continue;
                    }
                    CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update using service for patient> Begin update using service [{0}] for patient [{1}]", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                    try {
                        //int returnValue = (int)StatusEnum.SUCCESS;
                        PatientInPackageRepo repoPiPkg = new PatientInPackageRepo();
                        PatientInPackage entityPiPkg = unitOfWork.PatientInPackageRepository.Find(x=>x.Concerto && x.PatientInformation.PID==item.PID && x.PackagePriceSite.PackagePrice.Package.Code==item.PackageCode && new List<int>(){ 1,2,5,6}.Contains(x.Status))?.FirstOrDefault();
                        if (entityPiPkg != null)
                        {
                            //Get All charge using from Temp_ServiceUsing
                            var XhisCharges = unitOfWork.Temp_ServiceUsingRepository.Find(x => x.PID == item.PID && x.PackageCode== item.PackageCode);
                            if (XhisCharges.Any())
                            { 
                                string arrCharges =string.Join(";",XhisCharges.Select(x => x.ChargeId)?.ToList());
                                if (!string.IsNullOrEmpty(arrCharges))
                                {
                                    List<HISChargeModel> oHEntities = new List<HISChargeModel>();
                                    repoPiPkg.GetAllChargeInpackge(item.PID, arrCharges, oHEntities);
                                    //Chỉ lấy những chỉ định chưa hủy
                                    oHEntities = oHEntities?.Where(x => !Constant.ChargeStatusCancel.Contains(x.ChargeStatus))?.ToList();
                                    var groupBy = oHEntities.GroupBy(u => u.ChargeId)
                                    .Select(grp => grp.ToList())
                                    .ToList();
                                    oHEntities = groupBy.Select(g => g.First()).ToList();
                                    if (oHEntities?.Count > 0)
                                    {
                                        //CreateOrUpdateHisChargeList
                                        //Gán PatientInPackageId cho HisCharges
                                        oHEntities?.ForEach(x => x.PatientInPackageId = entityPiPkg.Id);
                                        CustomLog.intervaljoblog.Info(JsonConvert.SerializeObject(oHEntities));
                                        repoPiPkg.CreateOrUpdateHisCharges(oHEntities, entityPiPkg: entityPiPkg);
                                        var listData = unitOfWork.PatientInPackageDetailRepository.Find(x => x.PatientInPackageId == entityPiPkg.Id && !x.ServiceInPackage.IsDeleted);
                                        if (listData.Any())
                                        {
                                            var entities = listData.OrderBy(x => x.ServiceInPackage.ServiceType).ThenBy(x => x.ServiceInPackage.IsPackageDrugConsum).ThenBy(x => x.ServiceInPackage.CreatedAt).Select(x => new ChargeInPackageModel()
                                            {
                                                ServiceInpackageId = x.ServiceInPackage.Id,
                                                ServiceType = x.ServiceInPackage.ServiceType,
                                                PatientInPackageId = entityPiPkg.Id,
                                                PatientInPackageDetailId = x.Id,
                                                ServiceCode = x.ServiceInPackage.Service.Code,
                                                ServiceName = x.ServiceInPackage.Service.ViName,
                                                QtyCharged = 0,
                                                QtyRemain = x.QtyRemain,
                                                Price = x.PkgPrice,
                                                RootId = x.ServiceInPackage.RootId
                                            })?.ToList();
                                            var xquery = (from a in entities
                                                          join b in oHEntities on a.ServiceCode equals b.ItemCode into bx
                                                          from b in bx.AsEnumerable()
                                                          join c in XhisCharges on b.ChargeId equals c.ChargeId into cx
                                                          from c in cx.AsEnumerable()
                                                          select new ChargeInPackageModel()
                                                          {
                                                              ChargeId = b?.ChargeId,
                                                              HisChargeId = b?.Id,
                                                              PatientInPackageId = b?.PatientInPackageId,
                                                              PatientInPackageDetailId = a.PatientInPackageDetailId,
                                                              ServiceInpackageId = a.ServiceInpackageId,
                                                              ServiceType = a.ServiceType,
                                                              ChargeDateTime = b?.ChargeDate,
                                                              ChargeDate = b?.ChargeDate?.ToString(Constant.DATE_TIME_FORMAT_WITHOUT_SECOND),
                                                              //Thông tin người sử dụng
                                                              PID = b?.PID,
                                                              PatientName = b?.CustomerName,
                                                              ServiceCode = a.ServiceCode,
                                                              ServiceName = a.ServiceName,
                                                              QtyCharged = b?.Quantity,
                                                              QtyRemain = a.QtyRemain,
                                                              Price = c?.ChargePrice,
                                                              PkgPrice = a.Price,
                                                              RootId = a.RootId
                                                          });
                                            entities = xquery?.ToList();
                                            if (entities?.Count > 0)
                                            {
                                                foreach (var itemC in entities)
                                                {
                                                    HISChargeDetail entity = unitOfWork.HISChargeDetailRepository.FirstOrDefault(x =>
                                                       x.HisChargeId == itemC.HisChargeId && !x.IsDeleted);

                                                    if (itemC.PatientInPackageDetailId != null)
                                                    {
                                                        if (entity != null)
                                                        {
                                                            //Update
                                                            entity.HisChargeId = itemC.HisChargeId.Value;
                                                            entity.PatientInPackageDetailId = itemC.PatientInPackageDetailId.Value;
                                                            entity.PatientInPackageId = entityPiPkg.Id;
                                                            entity.InPackageType = (int)InPackageType.INPACKAGE;
                                                            entity.ChargePrice = itemC.Price;
                                                            entity.UnitPrice = itemC.PkgPrice;
                                                            entity.Quantity = itemC.QtyCharged;
                                                            entity.NetAmount = itemC.Amount;
                                                            unitOfWork.HISChargeDetailRepository.Update(entity);
                                                        }
                                                        else
                                                        {
                                                            //Thêm mới
                                                            entity = new HISChargeDetail();
                                                            entity.HisChargeId = itemC.HisChargeId.Value;
                                                            entity.PatientInPackageDetailId = itemC.PatientInPackageDetailId.Value;
                                                            entity.PatientInPackageId = entityPiPkg.Id;
                                                            entity.InPackageType = (int)InPackageType.INPACKAGE;
                                                            entity.ChargePrice = itemC.Price;
                                                            entity.UnitPrice = itemC.PkgPrice;
                                                            entity.Quantity = itemC.QtyCharged;
                                                            entity.NetAmount = itemC.Amount;

                                                            unitOfWork.HISChargeDetailRepository.Add(entity);
                                                        }
                                                    }
                                                }
                                                CustomLog.Instant.IntervalJobLog(string.Format("<Auto Update using service for patient> update using service [{0}] for patient [{1}] PatientInPackage is OK", item.PackageCode, item.PID), Constant.Log_Type_Info, printConsole: true);
                                                item.Notes = null;
                                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_SUCCESS"];
                                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                                            }
                                            else
                                            {
                                                CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> update using service [{0}] for patient [{1}] PatientInPackage is not mapping PatientInPackageDetail", item.PackageCode, item.PID));
                                                item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "PatientInPackage is not mapping PatientInPackageDetail") : "PatientInPackage is not mapping PatientInPackageDetail";
                                                item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_NOTFOUND"];
                                                unitOfWork.Temp_PatientInPackageRepository.Update(item);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            CustomLog.intervaljoblog.Info(string.Format("<Auto Update using service for patient> update using service [{0}] for patient [{1}] PatientInPackage is null", item.PackageCode, item.PID));
                            item.Notes = !string.IsNullOrEmpty(item.Notes) ? string.Format("{0}. {1}", item.Notes, "PatientInPackage is null") : "PatientInPackage is null";
                            item.StatusForProcess = Constant.PROCESS_REVENUE_STATUS["PROCESS_NOTFOUND"];
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
                CustomLog.Instant.IntervalJobLog($"<Auto Update using service for patient> Success!", Constant.Log_Type_Info, printConsole: true);
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Auto Update using service for patient> Error: {0}", ex));
            }
            Globals.IsAutoCalculatePriceProcessing = false;
        }
    }
}
