using DataAccess.Models;
using DataAccess.Repository;
using PMS.Business.Connection;
using PMS.Contract.Models.ApigwModels;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.Business.ScheduleJobs
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
                DepartmentId=!string.IsNullOrEmpty(item.DepartmentId)?new Guid(item.DepartmentId):(Guid?)null,
                IsActivated=item.IsActivated
            };
            unitOfWork.DepartmentRepository.Add(service);
            return service;
        }
        protected void UpdateDepartment(HISDepartmentModel item, Department entity)
        {
            entity.ViName = item.ViName;
            entity.EnName = item.EnName;
            entity.Code = item.Code;
            entity.HospitalCode = item.HospitalCode;
            entity.DepartmentId = !string.IsNullOrEmpty(item.DepartmentId) ? new Guid(item.DepartmentId) : (Guid?)null;
            entity.IsActivated = item.IsActivated;
            unitOfWork.DepartmentRepository.Update(entity);
        }
    }

    public class SyncOHDepartmentJob : SyncDepartmentJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            CustomLog.intervaljoblog.Info($"<Sync OH Department> Start!");

            var results = OHConnectionAPI.GetDepartment();
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
}
