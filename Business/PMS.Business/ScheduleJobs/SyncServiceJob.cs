using DataAccess.Models;
using DataAccess.Repository;
using PMS.Business.Connection;
using PMS.Contract.Models.ApigwModels;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;
using VM.Data.Queue;

namespace PMS.Business.ScheduleJobs
{
    public class SyncServiceJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        protected Service GetOrCreateService(HISServiceModel item, out ServiceGroup svGroup, Guid gSvGroupId)
        {
            try
            {
                //var service = unitOfWork.ServiceRepository.FirstOrDefault(e => e.Code == item.ServiceCode);
                var service = unitOfWork.ServiceRepository.FirstOrDefault(e => e.ServiceId == item.ServiceId);
                if (service != null)
                {
                    svGroup = service.ServiceGroup;
                    if (item.ServiceType == Constant.SERVICE_TYPE_PCK)
                    {
                        //Đánh dấu là gói nên cần xóa
                        service.IsDeleted = true;
                    }
                    service.ServiceId = item.ServiceId;
                    service.ServiceType = item.ServiceType;
                    return service;
                }

                var group = GetOrCreateServiceGroup(item, gSvGroupId);
                svGroup = group;
                //Bỏ qua các dịch vụ là gói.
                if (item.ServiceType!= Constant.SERVICE_TYPE_PCK)
                {
                    service = new Service
                    {
                        ServiceId=item.ServiceId,
                        ServiceType=item.ServiceType,
                        Code = item.ServiceCode,
                        ViName = item.ServiceViName,
                        EnName = item.ServiceEnName,
                        ServiceGroupId = group.Id,
                        IsActive = item.IsActive
                    };
                    unitOfWork.ServiceRepository.Add(service);
                    return service;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                CustomLog.intervaljoblog.Info(string.Format("<Sync OH Service> Error: {0}", ex));
                svGroup = null;
                return null;
            }

        }
        protected void UpdateService(HISServiceModel item, Service service)
        {
            //service.ViName = item.ServiceGroupViName;
            //service.EnName = item.ServiceGroupEnName;
            service.ServiceType = item.ServiceType;
            service.Code = item?.ServiceCode;
            service.ViName = item.ServiceViName;
            service.EnName = item.ServiceEnName;
            service.IsActive = item.IsActive;
            //Get Service category id
            #region Get Service category id
            var param = new Dictionary<string, string>();
            param["serviceCode"] = item?.ServiceCode;
            param["groupCode"] = item?.ServiceGroupCode;
            param["groupNameEn"] = item?.ServiceGroupEnName;
            param["groupNameVi"] = item?.ServiceGroupViName;
            DataTable results = unitOfWork.ExecStore("GetServiceCategory", param);
            if (results != null && results.Rows.Count > 0)
            {
                if (results.Rows[0]["Id"]?.ToString() != "REMOVE_PACKAGE")
                    service.ServiceCategoryId = new Guid(results.Rows[0]["Id"]?.ToString());
                else
                {
                    //Loại bỏ các dịch vụ là gói
                    service.IsDeleted = true;
                }
            }

            #endregion
            unitOfWork.ServiceRepository.Update(service);
        }
        protected ServiceGroup GetOrCreateServiceGroup(HISServiceModel item, Guid gSvGroupId)
        {
            var group = unitOfWork.ServiceGroupRepository.FirstOrDefault(e => e.Code == item.ServiceGroupCode);
            if (group != null)
                return group;

            group = new ServiceGroup
            {
                Code = item.ServiceGroupCode,
                ViName = item.ServiceGroupViName,
                EnName = item.ServiceGroupEnName,
            };
            if (gSvGroupId == Guid.Empty)
            {
                unitOfWork.ServiceGroupRepository.Add(group);
            }
            else
            {
                group.Id = gSvGroupId;
            }
            return group;
        }
    }

    public class SyncOHServiceJob : SyncServiceJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {

            if (Globals.IsAutoUpdateStatusProcessing)
                return;
            Globals.IsAutoUpdateStatusProcessing = true;
            int iTypeConfig = Constant.SYSTEM_CONFIG["SYNC_SERVICE"];
            var config = unitOfWork.SystemConfigRepository.Find(x => x.TypeConfig == iTypeConfig).FirstOrDefault();
            DateTime last_updated = (DateTime)config?.LastUpdatedOHService;
            DateTime now = DateTime.Now;
            CustomLog.intervaljoblog.Info($"<Sync OH Service> Start!");
            try
            {
                //var results = OHConnectionAPI.GetService(last_updated, now);
                var results = OHConnectionAPI.GetService(string.Empty,string.Empty);
                CustomLog.intervaljoblog.Info(string.Format("<Sync OH Service> Total item: {0}", results?.Count()));
                int countItem = 0;
                List<ServiceGroup> listSVGroup = new List<ServiceGroup>();
                foreach (HISServiceModel item in results)
                {
                    ServiceGroup svGroup = null;
                    var gSvGroupIdExist = listSVGroup.Where(x => x.Code == item.ServiceGroupCode);
                    Guid gSvGroupId = gSvGroupIdExist.Any() ? gSvGroupIdExist.Select(x => x.Id).First() : Guid.Empty;

                    var service = GetOrCreateService(item, out svGroup, gSvGroupId);
                    if (!listSVGroup.Contains(svGroup) && svGroup != null)
                        listSVGroup.Add(svGroup);

                    if (service != null)
                    {
                        countItem++;
                        UpdateService(item, service);
                        CustomLog.intervaljoblog.Info(string.Format("<Sync OH Service> Info {0}: [Code: {1} - Name: {2}]", countItem, item.ServiceCode, item.ServiceEnName));
                    }
                }
                config.LastUpdatedOHService = now;
                unitOfWork.Commit();
                CustomLog.intervaljoblog.Info($"<Sync OH Service> Success!");
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Sync OH Service> Error: {0}", ex));
            }
            Globals.IsAutoUpdateStatusProcessing = false;
        }
    }
}
