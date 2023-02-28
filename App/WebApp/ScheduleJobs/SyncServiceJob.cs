using DataAccess.Models;
using DataAccess.Repository;
using DrFee.Clients;
using DrFee.Models.ApigwModels;
using DrFee.Utils;
using Quartz;
using System;
using System.Linq;
using System.Threading;

namespace DrFee.ScheduleJobs
{
    public class SyncServiceJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        protected Service GetOrCreateService(HISServiceModel item)
        {
            try {
                var service = unitOfWork.ServiceRepository.FirstOrDefault(e => e.Code == item.ServiceCode);
                if (service != null)
                    return service;

                var group = GetOrCreateServiceGroup(item);

                service = new Service
                {
                    Code = item.ServiceCode,
                    ViName = item.ServiceViName,
                    EnName = item.ServiceEnName,
                    HISCode = item.HISCode,
                    ServiceGroupId = group.Id
                };
                unitOfWork.ServiceRepository.Add(service);
                return service;
            }
            catch(Exception ex)
            {
                CustomLog.intervaljoblog.Info(string.Format("<Sync OH Service> Error: {0}", ex));
                return null;
            }
            
        }
        /// <summary>
        /// Using for test
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        //protected Services_Test GetOrCreateService_Test(HISServiceModel item)
        //{
        //    var service = unitOfWork.Service_TestsRepository.FirstOrDefault(e => e.Code == item.ServiceCode);
        //    if (service != null)
        //        return service;

        //    var group = GetOrCreateServiceGroup(item);

        //    service = new Services_Test
        //    {
        //        Code = item.ServiceCode,
        //        ViName = item.ServiceViName,
        //        EnName = item.ServiceEnName,
        //        HISCode = item.HISCode,
        //        ServiceGroupId = group.Id
        //    };
        //    unitOfWork.Service_TestsRepository.Add(service);
        //    return service;
        //}
        protected void UpdateService(HISServiceModel item, Service service)
        {
            service.ViName = item.ServiceGroupViName;
            service.EnName = item.ServiceGroupEnName;
            unitOfWork.ServiceRepository.Update(service);
        }
        /// <summary>
        /// Using for testing
        /// </summary>
        /// <param name="item"></param>
        /// <param name="service"></param>
        //protected void UpdateService_Tests(HISServiceModel item, Services_Test service)
        //{
        //    service.ViName = item.ServiceGroupViName;
        //    service.EnName = item.ServiceGroupEnName;
        //    unitOfWork.Service_TestsRepository.Update(service);
        //}
        protected ServiceGroup GetOrCreateServiceGroup(HISServiceModel item)
        {
            var group = unitOfWork.ServiceGroupRepository.FirstOrDefault(e => e.Code == item.ServiceGroupCode);
            if (group != null)
                return group;

            group = new ServiceGroup
            {
                Code = item.ServiceGroupCode,
                ViName = item.ServiceGroupViName,
                EnName = item.ServiceGroupEnName,
                HISCode = item.HISCode,
            };
            unitOfWork.ServiceGroupRepository.Add(group);
            return group;
        }
    }

    public class SyncOHServiceJob : SyncServiceJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            //var config = unitOfWork.SystemConfigRepository.AsQueryable().FirstOrDefault();
            //DateTime last_updated = (DateTime)config?.LastUpdatedOHService;
            //DateTime now = DateTime.Now;
            CustomLog.intervaljoblog.Info($"<Sync OH Service> Start!");
            try {
                //var results = OHClient.GetService(last_updated, now);
                //CustomLog.intervaljoblog.Info(string.Format("<Sync OH Service> Total item: {0}", results?.Count()));
                int countItem = 0;
                for(int i = 0; i < 1000000; i++)
                {
                    CustomLog.intervaljoblog.Info(string.Format("<Sync OH Service> Info {0}", (i+1)));
                    Thread.Sleep(100);
                }
                //foreach (HISServiceModel item in results)
                //{
                //    var service = GetOrCreateService(item);
                //    if (service != null)
                //    {
                //        countItem++;
                //        UpdateService(item, service);
                //        CustomLog.intervaljoblog.Info(string.Format("<Sync OH Service> Info {0}: [Code: {1} - Name: {2}]", countItem, item.ServiceCode, item.ServiceEnName));
                //    }
                //    //var service = GetOrCreateService_Test(item);
                //    //if (service != null)
                //    //    UpdateService_Tests(item, service);
                //}
                //config.LastUpdatedOHService = now;
                //unitOfWork.Commit();
                CustomLog.intervaljoblog.Info($"<Sync OH Service> Success!");
            }
            catch(Exception ex)
            {
                CustomLog.intervaljoblog.Info(string.Format("<Sync OH Service> Error: {0}", ex));
            }
        }
    }

    public class SyncEHosServiceJob : SyncServiceJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var config = unitOfWork.SystemConfigRepository.AsQueryable().FirstOrDefault();
            DateTime last_updated = (DateTime)config.LastUpdatedEHosService;
            DateTime now = DateTime.Now;
            CustomLog.intervaljoblog.Info($"<Sync EHos Service> Start!");

            var results = OHClient.GetService(last_updated, now);
            foreach (HISServiceModel item in results)
            {
                var service = GetOrCreateService(item);
                if (service != null)
                    UpdateService(item, service);
            }
            config.LastUpdatedEHosService = now;
            unitOfWork.Commit();
            CustomLog.intervaljoblog.Info($"<Sync EHos Service> Success!");
        }
    }
}