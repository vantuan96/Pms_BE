using DataAccess.Models;
using DataAccess.Repository;
using DrFee.Clients;
using DrFee.Models.ApigwModels;
using DrFee.Utils;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrFee.ScheduleJobs
{
    public class SyncDepartmentJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();

        protected Department GetOrCreateDepartment(HISDepartmentModel item)
        {
            var service = unitOfWork.DepartmentRepository.FirstOrDefault(e => e.Code == item.Code);
            if (service != null)
                return service;

            service = new Department
            {
                ViName = item.ViName,
                EnName = item.EnName,
                Code = item.Code,
                HospitalCode = item.HospitalCode,
            };
            unitOfWork.DepartmentRepository.Add(service);
            return service;
        }
        protected void UpdateDepartment(HISDepartmentModel item, Department service)
        {
            service.ViName = item.ViName;
            service.EnName = item.EnName;
            service.Code = item.Code;
            service.HospitalCode = item.HospitalCode;
            unitOfWork.DepartmentRepository.Update(service);
        }
    }

    public class SyncOHDepartmentJob : SyncDepartmentJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            CustomLog.intervaljoblog.Info($"<Sync OH Department> Start!");

            var results = OHClient.GetDepartment();
            foreach (HISDepartmentModel item in results)
            {
                var service = GetOrCreateDepartment(item);
                if (service != null)
                    UpdateDepartment(item, service);
            }
            unitOfWork.Commit();
            CustomLog.intervaljoblog.Info($"<Sync OH Department> Success!");
        }
    }

    public class SyncEHosDepartmentJob : SyncDepartmentJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            CustomLog.intervaljoblog.Info($"<Sync EHos Department> Start!");

            //var results = EHosClient.GetDepartment();
            //foreach (HISDepartmentModel item in results)
            //{
            //    var service = GetOrCreateDepartment(item);
            //    if (service != null)
            //        UpdateDepartment(item, service);
            //}
            //unitOfWork.Commit();
            CustomLog.intervaljoblog.Info($"<Sync EHos Department> Success!");
        }
    }
}