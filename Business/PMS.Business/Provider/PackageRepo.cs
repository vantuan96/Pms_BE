using DataAccess.Models;
using DataAccess.Repository;
using PMS.Business.Connection;
using PMS.Contract.Models;
using PMS.Contract.Models.AdminModels;
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
    public class PackageRepo : IDisposable
    {
        private IUnitOfWork unitOfWork = new EfUnitOfWork();

        public void Dispose()
        {
            unitOfWork.Dispose();
        }
        #region Package Master
        public IQueryable<Package> GetPackages(PackageParameterModel request)
        {
            var results = unitOfWork.PackageRepository.AsQueryable().Where(e => !e.IsDeleted);
            if (!string.IsNullOrEmpty(request.ids))
            {
                if (request.ids == "0000")
                    results = results.Where(e => e.Id == null);
                else
                {
                    var ids = request.GetIds();
                    results = results.Where(e => ids.Contains(e.Id));
                }
            }
            else
            {
                if (request.keyword != null)
                    results = results.Where(e => e.Name.Contains(request.keyword) || e.Code.Contains(request.keyword));

                if (!string.IsNullOrEmpty(request.Groups))
                {
                    if (request.Groups == "0000")
                        results = results.Where(e => e.PackageGroupId == null);
                    else
                    {
                        var group_ids = request.GetGroups();
                        results = results.Where(e => group_ids.Contains(e.PackageGroupId));
                    }
                }
                if (!string.IsNullOrEmpty(request.Sites))
                {
                    if (request.Sites == "0000")
                        results = results.Where(e => e.PackagePrices.Any(x => x.PackagePriceSites.Any(y => y.SiteId == null)));
                    else
                    {
                        var site_ids = request.GetSites();
                        results = results.Where(e => e.PackagePrices.Any(x => x.PackagePriceSites.Any(y => site_ids.Contains(y.SiteId) && (!request.IsShowExpireDate && (y.EndAt == null || (y.EndAt != null && y.EndAt >= Constant.CurrentDate))))));
                    }
                }
                if (request.IsAvailable)
                {
                    results = results.Where(x => x.PackagePrices.Any(y => y.StartAt <= Constant.CurrentDate) && x.PackagePrices.Any(z=>z.PackagePriceDetails.Any(e=>e.PackagePriceId==z.Id)));
                }

                if (request.Status != -1)
                    results = results.Where(e => e.IsActived == request.Status > 0);
                if (request.SetPrice != -1)
                {
                    results = results.Where(e => e.PackagePrices.Any(x => x.PackageId == e.Id) == request.SetPrice > 0);
                }

                if (request.Limited != -1)
                    results = results.Where(e => e.IsLimitedDrugConsum == request.Limited > 0);
            }
            return results;
        }
        public IQueryable<Package> GetPackagesForMigrate()
        {
            List<string> listRun = new List<string>() { "G-MDR-01-01 (2022)", "G-MDR-01-05 (2022)", "G-MDR-01-10 (2022)", "G-MDR-01-15 (2022)", "G-MDR-01-20 (2022)", "G-MDR-01-25 (2022)", "G-MDR-02-01 (2022)", "G-MDR-02-05 (2022)", "G-MDR-02-10 (2022)", "G-MDR-02-15 (2022)", "G-MDR-02-20 (2022)", "G-MDR-02-25 (2022)", "G-MCR-MDR-01-01 (2022)", "G-MCR-MDR-01-05 (2022)", "G-MCR-MDR-01-10 (2022)", "G-MCR-MDR-01-15 (2022)", "G-MCR-MDR-01-20 (2022)", "G-MCR-MDR-01-25 (2022)", "G-MCR-MDR-02-01 (2022)", "G-MCR-MDR-02-05 (2022)", "G-MCR-MDR-02-10 (2022)", "G-MCR-MDR-02-15 (2022)", "G-MCR-MDR-02-20 (2022)", "G-MCR-MDR-02-25 (2022)", "G-MCR-01-01 (2022)", "G-MCR-01-05 (2022)", "G-MCR-01-10 (2022)", "G-MCR-01-15 (2022)", "G-MCR-01-20 (2022)", "G-MCR-01-25 (2022)", "G-MCR-02-01 (2022)", "G-MCR-02-05 (2022)", "G-MCR-02-10 (2022)", "G-MCR-02-15 (2022)", "G-MCR-02-20 (2022)", "G-MCR-02-25 (2022)", "G-DR-01-01 (2022)", "G-DR-01-05 (2022)", "G-DR-01-10 (2022)", "G-DR-01-15 (2022)", "G-DR-01-20 (2022)", "G-DR-01-25 (2022)", "G-DR-02-01 (2022)", "G-DR-02-05 (2022)", "G-DR-02-10 (2022)", "G-DR-02-15 (2022)", "G-DR-02-20 (2022)", "G-DR-02-25 (2022)", "G-MCR-DR-01-01 (2022)", "G-MCR-DR-01-05 (2022)", "G-MCR-DR-01-10 (2022)", "G-MCR-DR-01-15 (2022)", "G-MCR-DR-01-20 (2022)", "G-MCR-DR-01-25 (2022)", "G-MCR-DR-02-01 (2022)", "G-MCR-DR-02-05 (2022)", "G-MCR-DR-02-10 (2022)", "G-MCR-DR-02-15 (2022)", "G-MCR-DR-02-20 (2022)", "G-MCR-DR-02-25 (2022)" };
            //var results = unitOfWork.PackageRepository.AsEnumerable().Where(x => !x.IsDeleted && x.IsActived == true && x.IsFromeHos == true
            var results = unitOfWork.PackageRepository.Find(x => !x.IsDeleted && x.IsActived == true && x.IsFromeHos == true
            && listRun.Contains(x.Code) /*&& x.Code== "KSKCT-2021-183"*/);
            return results.AsQueryable();
        }
        public IQueryable<Package> GetPackagesForMigrateConcerto()
        {
            List<string> listRun = new List<string>() { "HCP.2018.003" };
            //var results = unitOfWork.PackageRepository.AsEnumerable().Where(x => !x.IsDeleted && x.IsActived == true && x.IsFromeHos == true
            var results = unitOfWork.PackageRepository.Find(x => !x.IsDeleted && x.IsActived == true && x.Concerto == true
            && !listRun.Contains(x.Code) /*&& x.Code== "KSKCT-2021-183"*/);
            return results.AsQueryable();
        }
        #endregion .Package Master

        #region Service
        public IQueryable<ServiceInPackage> GetServiceInPackages(ServiceInPackageParameterModel request,out IQueryable<ServiceInPackage> xqueryNoFilter)
        {
            var results = unitOfWork.ServiceInPackageRepository.AsQueryable().Where(e => !e.IsDeleted);
            IQueryable<ServiceInPackage> xqueryNoFil = results;
            if (!string.IsNullOrEmpty(request.ServiceIds))
            {
                if (request.ServiceIds == "0000")
                    results = results.Where(e => e.ServiceId == null);
                else
                {
                    var service_ids = request.GetServiceIds();
                    results = results.Where(e => service_ids.Contains(e.ServiceId));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(request.PackageId) && request.PackageId != "0000")
                {
                    results = results.Where(e => e.PackageId == new Guid(request.PackageId));
                }
                if (request.Status != -1)
                    results = results.Where(e => e.Service.IsActive == request.Status > 0);
                if (request.ServiceType != -1)
                    results = results.Where(e => e.ServiceType == request.ServiceType);
                if (request.IsServiceReplace != -1)
                {
                    results = results.Where(e => request.IsServiceReplace == 0 ? e.RootId == null : e.RootId != null);
                }
            }
            xqueryNoFilter = xqueryNoFil;
            return results;
        }
        public IQueryable<ServiceInPackage> GetXQueryInPackages()
        {
            var results = unitOfWork.ServiceInPackageRepository.AsQueryable().Where(e => !e.IsDeleted);
            return results;
        }
        #endregion .Service

        #region Service Package
        public List<PackagePriceSitesModel> GetListPricePolicyViaSite(Guid packageId)
        {
            List<PackagePriceSitesModel> listEntity = null;
            var iQuery = unitOfWork.PackagePriceSiteRepository.Find(x => !x.IsDeleted && x.PackagePrice.PackageId == packageId && (x.EndAt ==null || (x.EndAt !=null && x.EndAt>=Constant.CurrentDate)));
            if (iQuery.Any())
            {
                listEntity = new List<PackagePriceSitesModel>();
                var itemsDistint= iQuery.Select(x => new { x.Site, x.PackagePrice.Code }).Distinct();
                //var Entities = itemsDistint.ToList();
                foreach (var item in itemsDistint.Where(x=>x.Site!=null))
                {
                    var entity = new PackagePriceSitesModel()
                    {
                        SiteId = item.Site?.Id,
                        Site = item.Site,
                        PolicyCode=item.Code
                    };
                    foreach (var itemX in iQuery.Where(x => x.SiteId == item.Site?.Id && x.PackagePrice.Code== entity.PolicyCode))
                    {
                        if (itemX.PackagePrice.PersonalType == (int)PersonalEnum.VIETNAMESE)
                        {
                            #region Get Value for VietNamese
                            entity.PkgAmount = itemX.PackagePrice.Amount;
                            #endregion .Get Value for VietNamese
                        }
                        else
                        {
                            #region Get Value for Foreign
                            entity.PkgAmountForeign = itemX.PackagePrice.Amount;
                            #endregion .Get Value for Foreign
                        }
                        entity.StartAt = itemX.PackagePrice.StartAt?.ToString(Constant.DATE_FORMAT);
                        entity.EndAt = itemX.EndAt?.ToString(Constant.DATE_FORMAT);
                    }
                    
                    listEntity.Add(entity);
                }
                #region Re filter and set Note
                if(listEntity!=null && listEntity.Count > 0)
                {
                    listEntity = listEntity.OrderByDescending(x => x.StartAt).ToList();
                    StepReFilter:
                    foreach (var item in listEntity)
                    {
                        var itemNeedRemove = listEntity.RemoveAll(x => (x.SiteId == item.SiteId && x.PolicyCode != item.PolicyCode));
                        if(itemNeedRemove>0)
                            goto StepReFilter;
                        GetNoteForCurrentPolicy(packageId, item);
                        
                    }
                }
                #endregion .Re filter and set Note
            }
            return listEntity;
        }
        public IQueryable<dynamic> PricePolicyAvailable(Guid packageid, string sitecode, int? personaltype, string applydate, bool isForMigrate=false, double? NetAmountFilter=null)
        {
            DateTime applyDate = Constant.CurrentDate;
            if (!string.IsNullOrEmpty(applydate))
            {
                DateTime.TryParse(applydate, out applyDate);
            }
            var siteId = unitOfWork.SiteRepository.FirstOrDefault(x => x.ApiCode == sitecode)?.Id;
            //var xquery = unitOfWork.PackagePriceRepository.Find(x=> x.StartAt <= applyDate /*&& x.PackagePriceSites.Any(y => y.SiteId == siteId /*&& (y.EndAt >= applyDate || y.EndAt == null)*/).AsQueryable();
            var xqueryPolicy = unitOfWork.PackagePriceRepository.AsQueryable().Where(x => !x.IsDeleted && x.StartAt <= applyDate);
            if (!isForMigrate)
                xqueryPolicy = xqueryPolicy.Where(x => !x.IsNotForRegOnline);
            var xqueryPackage = unitOfWork.PackageRepository.AsQueryable().Where(x => !x.IsDeleted);

            var xquery = (from a in xqueryPolicy
                          join b in xqueryPackage
                               on a.PackageId equals b.Id into bx
                          from bxg in bx.DefaultIfEmpty()
                          where a.PackagePriceSites.Any(y => y.SiteId == siteId && (y.EndAt >= applyDate || y.EndAt == null))
                          && bxg.Id == packageid
                          select new
                          {
                              Id = a.Id,
                              Package = a.Package,
                              Code = a.Code,
                              Amount =a.Amount,
                              PersonalType=a.PersonalType,
                              PackagePriceSites=a.PackagePriceSites
                          });

            if (personaltype != null)
                xquery = xquery.Where(x => x.PersonalType == personaltype);
            if(NetAmountFilter!=null)
                xquery = xquery.Where(x => x.Amount >= NetAmountFilter);
            return xquery.OrderBy(x=>x.Amount);
        }
        public List<PackagePriceDetailModel> PackagePriceDetail(Guid packageId, string policyCode)
        {
            List<PackagePriceDetailModel> listEntity = null;
            var xPolicy = unitOfWork.PackagePriceRepository.Find(x => !x.IsDeleted && x.PackageId == packageId && x.Code == policyCode);
            if (xPolicy.Any())
            {
                var xServicePrice = unitOfWork.PackagePriceDetailRepository.Find(x => !x.IsDeleted && xPolicy.Any(y => y.Id == x.PackagePriceId)).OrderBy(x=>x.ServiceInPackage?.ServiceType).ThenBy(x=>x.ServiceInPackage?.IsPackageDrugConsum).ThenBy(x => x.ServiceInPackage?.CreatedAt);
                if (xServicePrice.Any())
                {
                    listEntity = new List<PackagePriceDetailModel>();
                    foreach (var item in xServicePrice.Select(x => x.ServiceInPackage).Distinct())
                    {
                        if (item == null)
                            continue;
                        var entity = new PackagePriceDetailModel()
                        {
                            ServiceInPackageId = item?.Id,
                            Service = item?.Service,
                            Qty = item?.LimitQty,
                            IsPackageDrugConsum =item.IsPackageDrugConsum,
                            ServiceType=item.ServiceType
                        };
                        foreach (var itemX in xServicePrice.Where(x => x.ServiceInPackageId == item?.Id))
                        {

                            if (itemX.PackagePrice.PersonalType == (int)PersonalEnum.VIETNAMESE)
                            {
                                #region Get Value for VietNamese
                                entity.BasePrice = itemX.BasePrice;
                                entity.BaseAmount = itemX.BaseAmount;
                                entity.PkgPrice = itemX.PkgPrice;
                                entity.PkgAmount = itemX.PkgAmount;
                                #endregion .Get Value for VietNamese
                            }
                            else
                            {
                                #region Get Value for Foreign
                                entity.BasePriceForeign = itemX.BasePrice;
                                entity.BaseAmountForeign = itemX.BaseAmount;
                                entity.PkgPriceForeign = itemX.PkgPrice;
                                entity.PkgAmountForeign = itemX.PkgAmount;
                                #endregion .Get Value for Foreign
                            }
                        }
                        listEntity.Add(entity);
                    }
                }
            }
            return listEntity;
        }
        /// <summary>
        /// Re-Calculate price/amount in package after edit price inside detail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //public List<PackagePriceDetailModel> CalculatePriceDetail(PackagePricePolicyModel request)
        //{
        //    List<PackagePriceDetailModel> listEntity = null;
        //    if (request != null && request.Policy != null && request.Policy.Count > 0)
        //    {
        //        var pkgEntity = unitOfWork.PackageRepository.Find(x => x.Id == request.PackageId).FirstOrDefault();
        //        listEntity = request.Details;
        //        double? pkgAmount, pkgAmount_FN;
        //        pkgAmount = request.Policy.Where(x => x.PersonalType == (int)PersonalEnum.VIETNAMESE).Select(y => y.Amount).FirstOrDefault();
        //        pkgAmount_FN = request.Policy.Where(x => x.PersonalType == (int)PersonalEnum.FOREIGN).Select(y => y.Amount).FirstOrDefault();
        //        if (listEntity != null && listEntity.Count > 0)
        //        {
        //            #region Set price & amount in package
        //            CalculateDetailService(pkgEntity.IsLimitedDrugConsum, listEntity, pkgAmount, pkgAmount_FN);
        //            #endregion .Set price & amount in package
        //        }
        //    }
        //    return listEntity;
        //}
        public List<PackagePriceDetailModel> GeneratePackagePriceDetail(Package pkgEntity, string chargeTypeCode, double? pkgAmount, string chargeTypeCode_FN, double? pkgAmount_FN,bool isLimitedDrugConsum, double? limitedDrugConsumAmount, double? rateINV = null)
        {
            List<PackagePriceDetailModel> listEntity = null;
            var listSericeInPkg = unitOfWork.ServiceInPackageRepository.Find(x => !x.IsDeleted && x.PackageId == pkgEntity.Id && x.RootId == null);
            var groupCode = pkgEntity.PackageGroup.Code;
            if (listSericeInPkg.Any())
            {
                //if (!pkgEntity.IsLimitedDrugConsum)
                //{
                //    //TH gói thuốc/VTTH là không định mức:
                //    listSericeInPkg=listSericeInPkg.Where(x=>x.ServiceType== (int)ServiceInPackageTypeEnum.SERVICE);
                //}
                listEntity = listSericeInPkg.OrderBy(x=>x.ServiceType).ThenBy(x=>x.IsPackageDrugConsum).ThenBy(x=>x.CreatedAt).Select(x => new PackagePriceDetailModel
                {
                    ServiceInPackageId = x.Id,
                    Qty = x.LimitQty,
                    Service = x.Service,
                    IsPackageDrugConsum = x.IsPackageDrugConsum,
                    ServiceType = x.ServiceType,
                    IsFree=unitOfWork.ServiceFreeInPackageRepository.Find(e => e.ServiceId == x.ServiceId && e.GroupCode == groupCode && !x.IsDeleted).Any()
                }).ToList();
                if (listEntity != null && listEntity.Count > 0)
                {
                    #region Set base price & base amount from Core (HIS)
                    /*Giá cho người Việt Nam*/
                    var entityPrices = OHConnectionAPI.GetServicePrice(chargeTypeCode, listEntity.Select(x => x.Service.Code).ToList());
                    if (entityPrices != null)
                    {
                        foreach (var item in entityPrices)
                        {
                            int index = listEntity.FindIndex(m => m.Service.Code == item.ServiceCode);
                            if (index >= 0)
                            {
                                if (pkgEntity.IsLimitedDrugConsum || listEntity[index].ServiceType == (int)ServiceInPackageTypeEnum.SERVICE)
                                {
                                    listEntity[index].BasePrice = item.Price;
                                }
                                else
                                {
                                    //TH IsLimitedDrugConsum=false & Service=SERVICE_DRUG_CONSUM
                                    listEntity[index].BasePrice = 0;
                                    
                                }
                                listEntity[index].BaseAmount = listEntity[index].BasePrice * listEntity[index].Qty;
                            }

                        }
                    }
                    /*Giá cho người Nước Ngoài*/
                    var entityPrices_FN = OHConnectionAPI.GetServicePrice(chargeTypeCode_FN, listEntity.Select(x => x.Service.Code).ToList());
                    if (entityPrices_FN != null)
                    {
                        foreach (var item in entityPrices_FN)
                        {
                            int index = listEntity.FindIndex(m => m.Service.Code == item.ServiceCode);
                            if (index >= 0)
                            {
                                if (pkgEntity.IsLimitedDrugConsum || listEntity[index].ServiceType == (int)ServiceInPackageTypeEnum.SERVICE)
                                {
                                    listEntity[index].BasePriceForeign = item.Price;
                                }
                                else
                                {
                                    //TH IsLimitedDrugConsum=false & Service=SERVICE_DRUG_CONSUM
                                    listEntity[index].BasePriceForeign = 0;

                                }
                                listEntity[index].BaseAmountForeign = listEntity[index].BasePriceForeign * listEntity[index].Qty;
                            }

                        }
                    }
                    #endregion .Set base price & base amount from Core (HIS)
                    #region Set price & amount in package
                    #region Get more information: IsVaccinePackage, List Policy
                    PackageGroupRepo groupRepo = new PackageGroupRepo();
                    var IsVaccinePackage = Constant.ListGroupCodeIsVaccinePackage.Contains(groupRepo.GetPackageGroupRoot(pkgEntity.PackageGroup)?.Code);
                    #endregion
                    //tungdd14 thêm rateINV tính giá vaccine theo hệ số
                    CalculateDetailService(groupCode, IsVaccinePackage, listEntity, pkgAmount, pkgAmount_FN,isLimitedDrugConsum, limitedDrugConsumAmount, rateINV);
                    #endregion .Set price & amount in package
                    #region Check and re-set pkgPrice first item, limit qty=1
                    //Giá dịch vụ đầu tiên trong gói bằng giá gói trừ tổng giá các item còn lại
                    #region Re calculate for Vietnamese
                    var TotalPkgAmount = listEntity?.Sum(x => x.PkgAmount);
                    if (pkgAmount != TotalPkgAmount)
                    {
                        //Reset pkgPrice for first item with qty=1
                        var firstItem = listEntity.Where(x=>x.Qty==1 && !x.IsFree).FirstOrDefault();
                        if (firstItem != null)
                        {
                            var pkgAmount_NotFirst = listEntity?.Where(x => x != firstItem).Sum(x => x.PkgAmount);
                            if (pkgAmount_NotFirst != null)
                            {
                                var pkgAmount_FirstItem = pkgAmount - pkgAmount_NotFirst;
                                var pkgPrice_FirstItem = Math.Round(pkgAmount_FirstItem.Value / firstItem.Qty.Value);
                                if (pkgAmount_FirstItem != null)
                                {
                                    firstItem.PkgPrice = pkgPrice_FirstItem;
                                    firstItem.PkgAmount = firstItem.PkgPrice * firstItem.Qty;
                                }
                            }
                        }
                    }
                    #endregion .Re calculate for Vietnamese
                    #region Re calculate for Foreign
                    var TotalPkgAmountForeign = listEntity?.Sum(x => x.PkgAmountForeign);
                    if(pkgAmount_FN!= TotalPkgAmountForeign)
                    {
                        //Reset pkgPrice for first item with qty=1
                        var firstItem_FN = listEntity.Where(x=>x.Qty==1 && !x.IsFree).FirstOrDefault();
                        if(firstItem_FN!=null)
                        {
                            var pkgAmount_NotFirst_FN = listEntity?.Where(x => x != firstItem_FN).Sum(x => x.PkgAmountForeign);
                            if(pkgAmount_NotFirst_FN != null)
                            {
                                var pkgAmount_FirstItem_FN = pkgAmount_FN - pkgAmount_NotFirst_FN;
                                var pkgPrice_FirstItem_FN = Math.Round(pkgAmount_FirstItem_FN.Value / firstItem_FN.Qty.Value);
                                if (pkgAmount_FirstItem_FN != null)
                                {
                                    firstItem_FN.PkgPriceForeign = pkgPrice_FirstItem_FN;
                                    firstItem_FN.PkgAmountForeign = firstItem_FN.PkgPriceForeign * firstItem_FN.Qty;
                                }
                            }
                        }
                    }
                    #endregion .Re calculate for Foreign
                    #endregion .Check and re-set pkgPrice first item
                }
            }
            return listEntity;
        }

        public List<PackagePriceDetailModel> GeneratePackagePriceDetail4MigrateConcerto(Package pkgEntity, string chargeTypeCode, double? pkgAmount, string chargeTypeCode_FN, double? pkgAmount_FN, bool isLimitedDrugConsum, double? limitedDrugConsumAmount)
        {
            List<PackagePriceDetailModel> listEntity = null;
            var listSericeInPkg = unitOfWork.ServiceInPackageRepository.Find(x => !x.IsDeleted && x.PackageId == pkgEntity.Id && x.RootId == null);
            var groupCode = pkgEntity.PackageGroup.Code;
            if (listSericeInPkg.Any())
            {
                //if (!pkgEntity.IsLimitedDrugConsum)
                //{
                //    //TH gói thuốc/VTTH là không định mức:
                //    listSericeInPkg=listSericeInPkg.Where(x=>x.ServiceType== (int)ServiceInPackageTypeEnum.SERVICE);
                //}
                listEntity = listSericeInPkg.OrderBy(x => x.ServiceType).ThenBy(x => x.IsPackageDrugConsum).ThenBy(x => x.CreatedAt).Select(x => new PackagePriceDetailModel
                {
                    ServiceInPackageId = x.Id,
                    Qty = x.LimitQty,
                    Service = x.Service,
                    IsPackageDrugConsum = x.IsPackageDrugConsum,
                    ServiceType = x.ServiceType,
                    IsFree = unitOfWork.ServiceFreeInPackageRepository.Find(e => e.ServiceId == x.ServiceId && e.GroupCode == groupCode && !x.IsDeleted).Any()
                }).ToList();
                if (listEntity != null && listEntity.Count > 0)
                {
                    #region Set base price & base amount from Core (HIS)
                    /*Giá cho người Việt Nam*/
                    var entityPrices = OHConnectionAPI.GetServicePrice(chargeTypeCode, listEntity.Select(x => x.Service.Code).ToList());
                    if (entityPrices != null)
                    {
                        foreach (var item in entityPrices)
                        {
                            int index = listEntity.FindIndex(m => m.Service.Code == item.ServiceCode);
                            if (index >= 0)
                            {
                                if (pkgEntity.IsLimitedDrugConsum || listEntity[index].ServiceType == (int)ServiceInPackageTypeEnum.SERVICE)
                                {
                                    listEntity[index].BasePrice = item.Price;
                                }
                                else
                                {
                                    //TH IsLimitedDrugConsum=false & Service=SERVICE_DRUG_CONSUM
                                    listEntity[index].BasePrice = 0;

                                }
                                listEntity[index].BaseAmount = listEntity[index].BasePrice * listEntity[index].Qty;
                            }

                        }
                    }
                    /*Giá cho người Nước Ngoài*/
                    var entityPrices_FN = OHConnectionAPI.GetServicePrice(chargeTypeCode_FN, listEntity.Select(x => x.Service.Code).ToList());
                    if (entityPrices_FN != null)
                    {
                        foreach (var item in entityPrices_FN)
                        {
                            int index = listEntity.FindIndex(m => m.Service.Code == item.ServiceCode);
                            if (index >= 0)
                            {
                                if (pkgEntity.IsLimitedDrugConsum || listEntity[index].ServiceType == (int)ServiceInPackageTypeEnum.SERVICE)
                                {
                                    listEntity[index].BasePriceForeign = item.Price;
                                }
                                else
                                {
                                    //TH IsLimitedDrugConsum=false & Service=SERVICE_DRUG_CONSUM
                                    listEntity[index].BasePriceForeign = 0;

                                }
                                listEntity[index].BaseAmountForeign = listEntity[index].BasePriceForeign * listEntity[index].Qty;
                            }

                        }
                    }
                    #endregion .Set base price & base amount from Core (HIS)
                    #region Set price & amount in package
                    //Get Price from Service Temp_ServiceInPackage
                    foreach(var item in listEntity)
                    {
                        var tempService = unitOfWork.Temp_ServiceInPackageRepository.Find(x => x.PackageCode == pkgEntity.Code && x.ServiceCode == item.Service.Code)?.FirstOrDefault();
                        if (tempService != null && tempService?.Price!=null)
                        {
                            item.PkgPrice = item.PkgPriceForeign = tempService?.Price;
                            item.PkgAmount = item.PkgAmountForeign = tempService?.Price * item.Qty;
                        }
                    }
                    #endregion .Set price & amount in package
                }
            }
            return listEntity;
        }
        /// <summary>
        /// Calculate price/amount in package detai service
        /// </summary>
        /// <param name="listEntity"></param>
        /// <param name="pkgAmount"></param>
        /// <param name="pkgAmount_FN"></param>
        /// <returns></returns>
        public List<PackagePriceDetailModel> CalculateDetailService(bool IsLimitedDrugConsum, List<PackagePriceDetailModel> listEntity, double? pkgAmount, double? pkgAmount_FN)
        {
            #region Set price & amount in package
            var listRate = GetRateInPackage(listEntity, pkgAmount, pkgAmount_FN);
            if (listRate != null)
            {
                var rate_VN = listRate.Where(x => x.PersonalType == (int)PersonalEnum.VIETNAMESE).Select(x => x.Rate).FirstOrDefault();
                var rate_FN = listRate.Where(x => x.PersonalType == (int)PersonalEnum.FOREIGN).Select(x => x.Rate).FirstOrDefault();
                foreach (var item in listEntity)
                {
                    if (item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE && !item.IsPackageDrugConsum)
                    {
                        item.PkgPrice = item.BasePrice!=null && rate_VN!=null? Math.Round(item.BasePrice.Value * rate_VN.Value,0): (double?)null;
                        item.PkgAmount = item.PkgPrice!=null && item.Qty!=null? Math.Round(item.PkgPrice.Value * item.Qty.Value,0): (double?)null;

                        item.PkgPriceForeign = item.BasePriceForeign!=null && rate_FN!=null? Math.Round(item.BasePriceForeign.Value * rate_FN.Value,0): (double?)null;
                        item.PkgAmountForeign = item.PkgPriceForeign!=null && item.Qty!=null? Math.Round(item.PkgPriceForeign.Value * item.Qty.Value,0) : (double?)null;
                    }
                    else
                    {
                        item.PkgPrice = item.BasePrice;
                        item.PkgAmount = item.PkgPrice * item.Qty;

                        item.PkgPriceForeign = item.BasePriceForeign;
                        item.PkgAmountForeign = item.PkgPriceForeign * item.Qty;
                        #region Comment old code (not use)
                        //if(IsLimitedDrugConsum || item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE)
                        //{
                        //    item.PkgPrice = item.BasePrice;
                        //    item.PkgAmount = item.PkgPrice * item.Qty;

                        //    item.PkgPriceForeign = item.BasePriceForeign;
                        //    item.PkgAmountForeign = item.PkgPriceForeign * item.Qty;
                        //}
                        //else
                        //{
                        //    //TH IsLimitedDrugConsum=false & Service=SERVICE_DRUG_CONSUM
                        //    item.BasePrice = 0;
                        //    item.BaseAmount = 0;

                        //    item.PkgPrice = 0;
                        //    item.PkgAmount = 0;

                        //    item.BasePriceForeign = 0;
                        //    item.BaseAmountForeign = 0;

                        //    item.PkgPriceForeign = 0;
                        //    item.PkgAmountForeign = 0;
                        //}
                        #endregion .Comment old code (not use)
                    }

                }
            }
            #endregion .Set price & amount in package
            return listEntity;
        }
        public List<PackagePriceDetailModel> CalculateDetailService(string groupCode, bool isVaccinePackage, List<PackagePriceDetailModel> listEntity, double? pkgAmount, double? pkgAmount_FN,bool IsLimitedDrugConsum, double? LimitedDrugConsumAmount, double? rateINV)
        {
            #region Set price & amount in package
            var listRate = GetRateInPackage(isVaccinePackage, listEntity, pkgAmount, pkgAmount_FN, IsLimitedDrugConsum, LimitedDrugConsumAmount);
            if (listRate != null)
            {
                var rate_VN = listRate.Where(x => x.PersonalType == (int)PersonalEnum.VIETNAMESE).Select(x => x.Rate).FirstOrDefault();
                var rate_FN = listRate.Where(x => x.PersonalType == (int)PersonalEnum.FOREIGN).Select(x => x.Rate).FirstOrDefault();
                var rate_DrugConsum_VN = listRate.Where(x => x.PersonalType == (int)PersonalEnum.VIETNAMESE).Select(x => x.RateDrugConsum).FirstOrDefault();
                var rate_DrugConsum_FN = listRate.Where(x => x.PersonalType == (int)PersonalEnum.FOREIGN).Select(x => x.RateDrugConsum).FirstOrDefault();
                //tungdd14 tính giá vaccine theo hệ số
                var rateSV_VN = (double?)null;
                var rateSV_FN = (double?)null;
                if (isVaccinePackage && rateINV != null && rateINV > 0)
                {
                    var totalDrugConsumINV = listEntity.Where(x => x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM).Sum(x => x.BasePrice*x.Qty*rateINV);
                    var totalDrugConsumSV = listEntity.Where(x => x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE).Sum(x => x.BasePrice * x.Qty);
                    //(Giá gói - thành tiền VC trong gói)/Tổng thành tiền cơ sở của dịch vụ
                    rateSV_VN = (pkgAmount - totalDrugConsumINV) / totalDrugConsumSV;
                    rateSV_FN = (pkgAmount_FN - totalDrugConsumINV) / totalDrugConsumSV;
                }
                foreach (var item in listEntity)
                {
                    //2022-07-25: Phubq sửa hỗ trợ apply free dich vu cho cả service/inventory
                    if (item.IsFree)
                    {
                        item.PkgPrice = 0;
                        item.PkgAmount = 0;

                        item.PkgPriceForeign = 0;
                        item.PkgAmountForeign = 0;
                    }
                    //tungdd14 tính giá vaccine theo hệ số
                    else if (isVaccinePackage && rateINV != null && rateINV > 0)
                    {
                        if (item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE)
                        {
                            //Giá của dịch vụ trong gói VC sẽ được phân bổ = Đơn giá lẻ tại thời điểm chỉ định * (Giá gói - thành tiền VC trong gói)/Tổng thành tiền cơ sở của dịch vụ
                            item.PkgPrice = item.BasePrice * rateSV_VN;
                            item.PkgAmount = item.PkgPrice * item.Qty;
                            item.PkgPriceForeign = item.BasePrice * rateSV_FN;
                            item.PkgAmountForeign = item.PkgPriceForeign * item.Qty;
                        }
                        else
                        {
                            //Đơn giá VC trong gói sẽ bằng = đơn giá lẻ tại thời điểm cấu hình * hệ số markup
                            item.PkgPrice = item.BasePrice * rateINV;
                            item.PkgAmount = item.PkgPrice * item.Qty;
                            item.PkgPriceForeign = item.BasePrice * rateINV;
                            item.PkgAmountForeign = item.PkgPriceForeign * item.Qty;
                        }
                    }
                    else
                    {
                        if (item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE && !item.IsPackageDrugConsum)
                        {
                            //TH dịch vụ set là miễn phí

                            //item.PkgPrice = item.BasePrice != null && rate_VN != null ? Math.Round(item.BasePrice.Value * rate_VN.Value, 0) : (double?)null;
                            item.PkgPrice = item.BasePrice != null && rate_VN != null ? item.BasePrice.Value * rate_VN.Value : (double?)null;
                            //item.PkgAmount = item.PkgPrice != null && item.Qty != null ? Math.Round(item.PkgPrice.Value * item.Qty.Value, 0) : (double?)null;
                            item.PkgAmount = item.PkgPrice * item.Qty;

                            //item.PkgPriceForeign = item.BasePriceForeign != null && rate_FN != null ? Math.Round(item.BasePriceForeign.Value * rate_FN.Value, 0) : (double?)null;
                            item.PkgPriceForeign = item.BasePriceForeign != null && rate_FN != null ? item.BasePriceForeign.Value * rate_FN.Value : (double?)null;
                            //item.PkgAmountForeign = item.PkgPriceForeign != null && item.Qty != null ? Math.Round(item.PkgPriceForeign.Value * item.Qty.Value, 0) : (double?)null;
                            item.PkgAmountForeign = item.PkgPriceForeign * item.Qty;
                        }
                        else
                        {
                            //item.PkgPrice = item.BasePrice != null && rate_DrugConsum_VN != null ? Math.Round(item.BasePrice.Value * rate_DrugConsum_VN.Value) : (double?)null;
                            item.PkgPrice = item.BasePrice != null && rate_DrugConsum_VN != null ? item.BasePrice.Value * rate_DrugConsum_VN.Value : (double?)null;
                            //item.PkgAmount = item.PkgPrice != null && item.Qty != null ? Math.Round(item.PkgPrice.Value * item.Qty.Value) : (double?)null;
                            item.PkgAmount = item.PkgPrice * item.Qty;

                            //item.PkgPriceForeign = item.BasePriceForeign != null && rate_DrugConsum_FN != null ? Math.Round(item.BasePriceForeign.Value * rate_DrugConsum_FN.Value) : (double?)null;
                            item.PkgPriceForeign = item.BasePriceForeign != null && rate_DrugConsum_FN != null ? item.BasePriceForeign.Value * rate_DrugConsum_FN.Value : (double?)null;
                            //item.PkgAmountForeign = item.PkgPriceForeign != null && item.Qty != null ? Math.Round(item.PkgPriceForeign.Value * item.Qty.Value) : (double?)null;
                            item.PkgAmountForeign = item.PkgPriceForeign * item.Qty;

                            #region Comment old code (not use)
                            //if(IsLimitedDrugConsum || item.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE)
                            //{
                            //    item.PkgPrice = item.BasePrice;
                            //    item.PkgAmount = item.PkgPrice * item.Qty;

                            //    item.PkgPriceForeign = item.BasePriceForeign;
                            //    item.PkgAmountForeign = item.PkgPriceForeign * item.Qty;
                            //}
                            //else
                            //{
                            //    //TH IsLimitedDrugConsum=false & Service=SERVICE_DRUG_CONSUM
                            //    item.BasePrice = 0;
                            //    item.BaseAmount = 0;

                            //    item.PkgPrice = 0;
                            //    item.PkgAmount = 0;

                            //    item.BasePriceForeign = 0;
                            //    item.BaseAmountForeign = 0;

                            //    item.PkgPriceForeign = 0;
                            //    item.PkgAmountForeign = 0;
                            //}
                            #endregion .Comment old code (not use)
                        }
                    }
                }
            }
            #endregion .Set price & amount in package
            return listEntity;
        }
        /// <summary>
        /// Get list rate in package
        /// </summary>
        /// <param name="listModel"></param>
        /// <param name="pkgAmount"></param>
        /// <param name="pkgAmount_FN"></param>
        /// <returns></returns>
        public List<ReturnRateInPackage> GetRateInPackage(List<PackagePriceDetailModel> listModel, double? pkgAmount, double? pkgAmount_FN)
        {
            List<ReturnRateInPackage> returnList = null;
            if (listModel != null && listModel.Count > 0)
            {
                double? totalAmount_VN = listModel.Where(x=>!x.IsPackageDrugConsum && x.ServiceType==(int)ServiceInPackageTypeEnum.SERVICE).Sum(x => x.BaseAmount);
                double? totalAmount_DrugConSum_VN = listModel.Where(x => x.IsPackageDrugConsum || x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM).Sum(x => x.BaseAmount);
                double? totalAmount_FN = listModel.Where(x => !x.IsPackageDrugConsum && x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE).Sum(x => x.BaseAmountForeign);
                double? totalAmount_DrugConSum_FN = listModel.Where(x => x.IsPackageDrugConsum || x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM).Sum(x => x.BaseAmountForeign);
                var rate_VN = (totalAmount_VN != null && totalAmount_VN != 0) ? (pkgAmount- totalAmount_DrugConSum_VN) / totalAmount_VN : 1;
                var rate_FN = (totalAmount_FN != null && totalAmount_FN != 0) ? (pkgAmount_FN- totalAmount_DrugConSum_FN) / totalAmount_FN : 1;
                returnList = new List<ReturnRateInPackage>();
                returnList.Add(new ReturnRateInPackage { PersonalType = (int)PersonalEnum.VIETNAMESE, Rate = rate_VN });
                returnList.Add(new ReturnRateInPackage { PersonalType = (int)PersonalEnum.FOREIGN, Rate = rate_FN });
            }
            return returnList;
        }
        public List<ReturnRateInPackage> GetRateInPackage(bool isVaccinePackage,List<PackagePriceDetailModel> listModel, double? pkgAmount, double? pkgAmount_FN,bool IsLimitedDrugConsum, double? LimitedDrugConsumAmount)
        {
            List<ReturnRateInPackage> returnList = null;
            if (listModel != null && listModel.Count > 0)
            {
                bool IsHaveItemDrugConsum = listModel.Any(x => x.IsPackageDrugConsum);
                double? totalAmount_VN = listModel.Where(x => !x.IsPackageDrugConsum && !x.IsFree).Sum(x => x.BaseAmount);
                double? totalAmount_SV_VN = listModel.Where(x => !x.IsPackageDrugConsum && x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE && !x.IsFree).Sum(x => x.BaseAmount);
                double? totalAmount_DrugConSum_VN = listModel.Where(x => (x.IsPackageDrugConsum || x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM) && !x.IsFree).Sum(x => x.BaseAmount);
                double? totalAmount_FN = listModel.Where(x => !x.IsPackageDrugConsum && !x.IsFree).Sum(x => x.BaseAmountForeign);
                double? totalAmount_SV_FN = listModel.Where(x => !x.IsPackageDrugConsum && x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE && !x.IsFree).Sum(x => x.BaseAmountForeign);
                double? totalAmount_DrugConSum_FN = listModel.Where(x => (x.IsPackageDrugConsum || x.ServiceType == (int)ServiceInPackageTypeEnum.SERVICE_DRUG_CONSUM) && !x.IsFree).Sum(x => x.BaseAmountForeign);
                double? rate_VN;
                double? rate_FN;
                double? rate_DrugConsum_VN=1;
                double? rate_DrugConsum_FN=1;
                if (isVaccinePackage)
                {
                    //TH Gói Vaccine
                    if (IsHaveItemDrugConsum)
                    {
                        rate_VN = (totalAmount_SV_VN != null && totalAmount_SV_VN != 0) ? (pkgAmount - totalAmount_DrugConSum_VN) / totalAmount_SV_VN : 1;
                        rate_FN = (totalAmount_SV_FN != null && totalAmount_SV_FN != 0) ? (pkgAmount_FN - totalAmount_DrugConSum_FN) / totalAmount_SV_FN : 1;
                    }
                    else
                    {
                        rate_VN = (totalAmount_VN != null && totalAmount_VN != 0) ? (pkgAmount) / totalAmount_VN : 1;
                        rate_FN = (totalAmount_FN != null && totalAmount_FN != 0) ? (pkgAmount_FN) / totalAmount_FN : 1;
                        rate_DrugConsum_VN = rate_VN;
                        rate_DrugConsum_FN = rate_FN;
                    }
                }
                else{
                    if (IsLimitedDrugConsum)
                    {
                        rate_VN = (totalAmount_SV_VN != null && totalAmount_SV_VN != 0) ? (pkgAmount - (LimitedDrugConsumAmount> 0? LimitedDrugConsumAmount:0)) / totalAmount_SV_VN : 1;
                        rate_FN = (totalAmount_SV_FN != null && totalAmount_SV_FN != 0) ? (pkgAmount_FN - (LimitedDrugConsumAmount > 0 ? LimitedDrugConsumAmount : 0)) / totalAmount_SV_FN : 1;

                        //Tỷ lệ thuốc/VTTH
                        rate_DrugConsum_VN = (totalAmount_DrugConSum_VN != null && totalAmount_DrugConSum_VN != 0) ? (LimitedDrugConsumAmount > 0 ? LimitedDrugConsumAmount : 0) / totalAmount_DrugConSum_VN : 1;
                        rate_DrugConsum_FN = (totalAmount_DrugConSum_FN != null && totalAmount_DrugConSum_FN != 0) ? (LimitedDrugConsumAmount > 0 ? LimitedDrugConsumAmount : 0) / totalAmount_DrugConSum_FN : 1;
                    }
                    else
                    {
                        rate_VN = (totalAmount_SV_VN != null && totalAmount_SV_VN != 0) ? (pkgAmount - totalAmount_DrugConSum_VN) / totalAmount_SV_VN : 1;
                        rate_FN = (totalAmount_SV_FN != null && totalAmount_SV_FN != 0) ? (pkgAmount_FN - totalAmount_DrugConSum_FN) / totalAmount_SV_FN : 1;
                    }
                    
                }
                returnList = new List<ReturnRateInPackage>();
                returnList.Add(new ReturnRateInPackage { PersonalType = (int)PersonalEnum.VIETNAMESE, Rate = rate_VN,RateDrugConsum= rate_DrugConsum_VN });
                returnList.Add(new ReturnRateInPackage { PersonalType = (int)PersonalEnum.FOREIGN, Rate = rate_FN,RateDrugConsum= rate_DrugConsum_FN });
            }
            return returnList;
        }

        public bool CheckExistPatientReg(Guid packageId)
        {
            bool returnValue = false;
            //returnValue = unitOfWork.PatientInPackageRepository.AsEnumerable().Any(x => x.PackagePriceSite.PackagePrice.PackagePriceSites.Any(y => y.PackagePrice.PackageId == packageId));
            returnValue = unitOfWork.PatientInPackageRepository.Find(x => x.PackagePriceSite.PackagePrice.PackagePriceSites.Any(y => y.PackagePrice.PackageId == packageId)).Any();
            return returnValue;
        }
        public bool CheckExistPatientRegWithPolicy(string policyCode)
        {
            bool returnValue = false;
            //returnValue = unitOfWork.PatientInPackageRepository.AsEnumerable().Any(x => x.PackagePriceSite.PackagePrice.PackagePriceSites.Any(y => y.PackagePrice.Code == policyCode));
            returnValue = unitOfWork.PatientInPackageRepository.Find(x => x.PackagePriceSite.PackagePrice.PackagePriceSites.Any(y => y.PackagePrice.Code == policyCode)).Any();
            return returnValue;
        }
        #endregion .Service Package
        #region Helper Function
        public string GetNoteForCurrentPolicy(Guid PackageId, PackagePriceSitesModel entity)
        {
            if (entity.GetEndAt() == null)
            {
                return string.Empty;
            }
            string strReturn = string.Empty;
            var viaSiteEnities = unitOfWork.PackagePriceSiteRepository.Find(x => !x.PackagePrice.IsNotForRegOnline && x.PackagePrice.PackageId == PackageId && x.SiteId==entity.SiteId && x.PackagePrice.Code!=entity.PolicyCode).Select(x=>new { PolicyCode=x.PackagePrice.Code, StartDate=x.PackagePrice.StartAt, Endate=x.EndAt}).Distinct().ToList();
            if (viaSiteEnities?.Count > 0)
            {
                //Tồn tại chính sách
                var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.NOTE_NEW_POLICY);
                MessageModel mdMsg = (MessageModel)msg.Clone();
                mdMsg.ViMessage = string.Format(msg.ViMessage, viaSiteEnities[0].StartDate.Value.ToString(Constant.DATE_FORMAT));
                mdMsg.EnMessage = string.Format(msg.EnMessage, viaSiteEnities[0].StartDate.Value.ToString(Constant.DATE_FORMAT));
                entity.Notes = mdMsg;
            }
            else
            {
                var msg = MessageManager.Messages.Find(x => x.Code == MessageCode.NOTE_NONEW_POLICY);
                MessageModel mdMsg = (MessageModel)msg.Clone();
                mdMsg.ViMessage = string.Format(msg.ViMessage, entity.GetEndAt().Value.AddDays(1).ToString(Constant.DATE_FORMAT));
                mdMsg.EnMessage = string.Format(msg.EnMessage, entity.GetEndAt().Value.AddDays(1).ToString(Constant.DATE_FORMAT));
                entity.Notes = mdMsg;
            }
            return strReturn;
        }
        #endregion .Helper Function
    }
    public class ReturnRateInPackage
    {
        public int PersonalType { get; set; }
        public double? Rate { get; set; }
        public double? RateDrugConsum { get; set; }
    }
}