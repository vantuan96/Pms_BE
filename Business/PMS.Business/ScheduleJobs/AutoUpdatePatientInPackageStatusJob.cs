using DataAccess.Models;
using DataAccess.Repository;
using PMS.Business.Connection;
using PMS.Contract.Models.ApigwModels;
using PMS.Contract.Models.Enum;
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
    public class AutoUpdatePatientInPackageStatusJob:IJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
        public void Execute(IJobExecutionContext context)
        {
            if (Globals.IsAutoUpdateStatusProcessing)
                return;
            Globals.IsAutoUpdateStatusProcessing = true;
            CustomLog.intervaljoblog.Info($"<Auto Update Patient's Package Status> Start!");
            try
            {
                //tungdd14: thêm x.Status == (int)PatientInPackageEnum.RE_EXAMINATE
                //cập nhật status với các gói tái khám
                var results = unitOfWork.PatientInPackageRepository.AsEnumerable().Where(x => !x.IsDeleted && (x.Status == (int)PatientInPackageEnum.REGISTERED || x.Status == (int)PatientInPackageEnum.ACTIVATED || x.Status == (int)PatientInPackageEnum.RE_EXAMINATE));
                CustomLog.intervaljoblog.Info(string.Format("<Auto Update Patient's Package Status> Total item: {0}", results?.Count()));
                List<ServiceGroup> listSVGroup = new List<ServiceGroup>();
                bool isHaveBeenChange = false;
                foreach (var item in results)
                {
                    if (Constant.CurrentDate >= item.StartAt && item.Status == (int)PatientInPackageEnum.REGISTERED)
                    {
                        //linhht
                        item.LastStatus = item.Status;
                        item.Status = (int)PatientInPackageEnum.ACTIVATED;
                        CustomLog.intervaljoblog.Info(string.Format("<Update Patient's Package Status> Info: [Code: {0}; Name: {1}; PatientInPackageId: {2}; Status: {3}]", item.PackagePriceSite.PackagePrice.Package.Code, item.PackagePriceSite.PackagePrice.Package.Name, item.Id, item.Status));
                        unitOfWork.PatientInPackageRepository.Update(item);
                        isHaveBeenChange = true;
                    }
                    //tungdd14: thêm x.Status == (int)PatientInPackageEnum.RE_EXAMINATE
                    //cập nhật status với các gói tái khám
                    else if (Constant.CurrentDate > item.EndAt && (item.Status == (int)PatientInPackageEnum.ACTIVATED || item.Status == (int)PatientInPackageEnum.REGISTERED || item.Status == (int)PatientInPackageEnum.RE_EXAMINATE))
                    {
                        //linhht
                        item.LastStatus = item.Status;
                        item.Status = (int)PatientInPackageEnum.EXPIRED;
                        CustomLog.intervaljoblog.Info(string.Format("<Update Patient's Package Status> Info: [Code: {0}; Name: {1}; PatientInPackageId: {2}; Status: {3}]", item.PackagePriceSite.PackagePrice.Package.Code, item.PackagePriceSite.PackagePrice.Package.Name, item.Id, item.Status));
                        unitOfWork.PatientInPackageRepository.Update(item);
                        isHaveBeenChange = true;
                    }
                }
                if (isHaveBeenChange)
                {
                    unitOfWork.Commit();
                }
                
                CustomLog.intervaljoblog.Info($"<Auto Update Patient's Package Status> Success!");
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("<Auto Update Patient's Package Status> Error: {0}", ex));
            }
            Globals.IsAutoUpdateStatusProcessing = false;
        }
    }
}
