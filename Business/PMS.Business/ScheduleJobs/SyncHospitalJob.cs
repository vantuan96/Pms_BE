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
    public class SyncHospitalJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();

        protected Site GetOrCreateHos(Site item)
        {
            var entity = unitOfWork.SiteRepository.FirstOrDefault(e => e.HospitalId == item.HospitalId);
            if (entity != null)
                return entity;

            entity = new Site
            {
                HospitalId = item.HospitalId,
                Code = item.Code,
                FullNameL = item.FullNameL,
                FullNameE = item.FullNameE,
                AddressL = item.AddressL,
                AddressE = item.AddressE,
                Tel = item.Tel,
                Fax = item.Fax,
                Hotline = item.Hotline,
                Emergency = item.Emergency,
                IsActived=item.IsActived
            };
            unitOfWork.SiteRepository.Add(entity);
            return entity;
        }
        protected void UpdateSite(Site item, Site entity)
        {
            entity.HospitalId = item.HospitalId;
            entity.Code = item.Code;
            entity.FullNameL = item.FullNameL;
            entity.FullNameE = item.FullNameE;
            entity.AddressL = item.AddressL;
            entity.AddressE = item.AddressE;
            entity.Tel = item.Tel;
            entity.Fax = item.Fax;
            entity.Hotline = item.Hotline;
            entity.Emergency = item.Emergency;
            entity.IsActived = item.IsActived;
            unitOfWork.SiteRepository.Update(entity);
        }
    }

    public class SyncHospitalOHJob : SyncHospitalJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            CustomLog.intervaljoblog.Info($"<Sync OH Hospital> Start!");

            var results = OHConnectionAPI.GetSites(string.Empty);
            foreach (Site item in results)
            {
                var service = GetOrCreateHos(item);
                if (service != null)
                    UpdateSite(item, service);
            }
            unitOfWork.Commit();
            CustomLog.intervaljoblog.Info($"<Sync OH Hospital> Success!");
        }
    }
}
