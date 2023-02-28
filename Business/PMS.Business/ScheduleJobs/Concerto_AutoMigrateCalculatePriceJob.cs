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
    public class Concerto_AutoMigrateCalculatePriceJob : IJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        public void Execute(IJobExecutionContext context)
        {
            if (Globals.IsAutoCalculatePriceProcessing)
                return;
            Globals.IsAutoCalculatePriceProcessing = true;
            CustomLog.intervaljoblog.Info($"<Concerto - Auto Calculate price Inside setting policy> Start!");
            try
            {
                //var results = unitOfWork.PackageRepository.AsEnumerable().Where(x => !x.IsDeleted && x.IsActived == true && x.IsFromeHos == true);
                var results = new PackageRepo().GetPackagesForMigrateConcerto();
                CustomLog.intervaljoblog.Info(string.Format("<Concerto - Auto Calculate price Inside setting policy> Total item: {0}", results?.Count()));
                
                foreach (var item in results)
                {
                    //Check để bỏ qua các gói có Notes
                    var temPackage = unitOfWork.Temp_PackageRepository.FirstOrDefault(x => x.PackgeCode == item.Code);
                    if(temPackage==null)
                    {
                        CustomLog.intervaljoblog.Info(string.Format("<Concerto - Auto MigrateCalculatePrice job> Package[Code={0}] is not found in Temp_Package", item.Code));
                        continue;
                    }
                    if (!string.IsNullOrEmpty(temPackage.Notes))
                    {
                        CustomLog.intervaljoblog.Info(string.Format("<Concerto - Auto MigrateCalculatePrice job> Package[Code={0}] is have note", item.Code));
                        continue;
                    }
                    try {
                        if (item.PackagePrices?.Count > 0)
                        {
                            var groupPolicy = item.PackagePrices?.GroupBy(x => new { x.PackageId, x.Code }).Select(x => x.Key);
                            if (groupPolicy.Any())
                            {
                                var packageId = item.Id;
                                foreach(var itemx in groupPolicy)
                                {
                                    string chargetypecode = string.Empty;
                                    double? pkgAmount = null;
                                    string chargetypecode_fn = string.Empty;
                                    double? pkgAmount_fn = null;

                                    foreach (var itemPrice in item.PackagePrices.Where(x=>x.Code==itemx.Code))
                                    {
                                        if (itemPrice.PersonalType == (int)PersonalEnum.VIETNAMESE)
                                        {
                                            chargetypecode = itemPrice.ChargeType;
                                            pkgAmount = itemPrice.Amount;
                                        }
                                        else if (itemPrice.PersonalType == (int)PersonalEnum.FOREIGN)
                                        {
                                            chargetypecode_fn = itemPrice.ChargeType;
                                            pkgAmount_fn = itemPrice.Amount;
                                        }
                                    }
                                    //Get Price detail
                                    var entities = new PackageRepo().GeneratePackagePriceDetail4MigrateConcerto(item, chargetypecode, pkgAmount, chargetypecode_fn, pkgAmount_fn,false,null);
                                    if (entities?.Count > 0)
                                    {
                                        //Check valid chi tiết dịch vụ trong gói
                                        #region Check valid chi tiết dịch vụ trong gói
                                        //CustomLog.intervaljoblog.Info(JsonConvert.SerializeObject(entities));
                                        //Check TH có dịch vụ giá =0 
                                        var existPkgPriceZero = entities.Where(x => x.PkgPrice == null || x.BasePrice==null/* || (x.PkgPrice != null && x.PkgPrice <= 0)*/);
                                        if (existPkgPriceZero.Any())
                                        {
                                            //Có dịch vụ ko có giá trong gói hoặc =0
                                            CustomLog.intervaljoblog.Info(string.Format("<Concerto - Auto MigrateCalculatePrice job> Package[Code={0}] is have service price is zero or null", item.Code));
                                            temPackage.Notes = !string.IsNullOrEmpty(temPackage.Notes) ? string.Format("{0}. {1}", temPackage.Notes, "have service price is zero or null") : string.Format("have service price is null: {0}", string.Join(";",existPkgPriceZero?.Select(x=>x.Service.Code).ToArray()));
                                            unitOfWork.Temp_PackageRepository.Update(temPackage);
                                            unitOfWork.Commit();
                                            continue;
                                        }
                                        //Check TH số lượng dịch vụ đã thiết lập giá so với số lượng dịch vụ trong gói
                                        var countServiceInpackage = unitOfWork.ServiceInPackageRepository.Count(x => !x.IsDeleted && x.PackageId == packageId && x.RootId==null);
                                        if (countServiceInpackage != entities?.Count)
                                        {
                                            //Số lượng dịch vụ thiết lập giá và dịch vụ trong giá ko bằng nhau
                                            CustomLog.intervaljoblog.Info(string.Format("<Concerto - Auto MigrateCalculatePrice job> Package[Code={0}] have service in package number is not equal service in policy setting", item.Code));
                                            temPackage.Notes = !string.IsNullOrEmpty(temPackage.Notes) ? string.Format("{0}. {1}", temPackage.Notes, "Số lượng dịch vụ thiết lập giá và dịch vụ trong giá ko bằng nhau") : "Số lượng dịch vụ thiết lập giá và dịch vụ trong giá ko bằng nhau";
                                            unitOfWork.Temp_PackageRepository.Update(temPackage);
                                            unitOfWork.Commit();
                                            continue;
                                        }
                                        #endregion .Check valid chi tiết dịch vụ trong gói
                                        foreach (var itemPriceDetail in entities)
                                            CreateOrUpdatePricePolicyDetail(itemPriceDetail, item.PackagePrices.Where(x=>x.Code==itemx.Code)?.ToList());
                                        CustomLog.Instant.IntervalJobLog(string.Format("<Concerto - Auto CreateOrUpdatePricePolicyDetail> Package[Code={0}]", item.Code), Constant.Log_Type_Info, printConsole: true);
                                    }
                                    else
                                    {
                                        //Số lượng dịch vụ thiết lập giá không có
                                        CustomLog.intervaljoblog.Info(string.Format("<Concerto - Auto MigrateCalculatePrice job> Package[Code={0}] have any more service in policy", item.Code));
                                        temPackage.Notes = !string.IsNullOrEmpty(temPackage.Notes) ? string.Format("{0}. {1}", temPackage.Notes, "Số lượng dịch vụ thiết lập giá không có") : "Số lượng dịch vụ thiết lập giá không có";
                                        unitOfWork.Temp_PackageRepository.Update(temPackage);
                                        unitOfWork.Commit();
                                        continue;
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        CustomLog.errorlog.Info(string.Format("<Concerto - Auto MigrateCalculatePrice job> Package[Code={0}]. Ex: {1}", item.Code,ex.Message));
                        temPackage.Notes = !string.IsNullOrEmpty(temPackage.Notes) ? string.Format("{0}. {1}", temPackage.Notes, ex.Message) : ex.Message;
                        unitOfWork.Temp_PackageRepository.Update(temPackage);
                        unitOfWork.Commit();
                    }
                    
                    unitOfWork.Commit();
                    //Dừng để tránh DDOS API
                    Thread.Sleep(1000);
                }
                
                //CustomLog.intervaljoblog.Info($"<Auto Calculate price Inside setting policy> Success!");
                CustomLog.Instant.IntervalJobLog($"<Concerto - Auto MigrateCalculatePrice> Success!", Constant.Log_Type_Info, printConsole: true);
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Concerto - Auto Calculate price Inside setting policy> Error: {0}", ex));
            }
            Globals.IsAutoCalculatePriceProcessing = false;
        }
        private void CreateOrUpdatePricePolicyDetail(PackagePriceDetailModel model, List<PackagePrice> listMaster)
        {
            if (listMaster != null && listMaster.Count > 0 && model != null)
            {
                foreach (var item in listMaster)
                {
                    //K.Tra xem đã tồn tại trong setting detail hay chưa
                    var entityDetail = unitOfWork.PackagePriceDetailRepository.FirstOrDefault(e => !e.IsDeleted && e.PackagePriceId == item.Id && e.ServiceInPackageId == model.ServiceInPackageId);
                    if (entityDetail != null)
                    {
                        entityDetail.BasePrice = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.BasePrice : model.BasePriceForeign;
                        entityDetail.BaseAmount = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.BaseAmount : model.BaseAmountForeign;
                        entityDetail.PkgPrice = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.PkgPrice : model.PkgPriceForeign;
                        entityDetail.PkgAmount = (item.PersonalType == (int)PersonalEnum.VIETNAMESE) ? model.PkgAmount : model.PkgAmountForeign;
                        unitOfWork.PackagePriceDetailRepository.Update(entityDetail);
                    }
                    else
                    {
                        entityDetail = new PackagePriceDetail
                        {
                            PackagePriceId = item.Id,
                            ServiceInPackageId = model.ServiceInPackageId,
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
    }
}
