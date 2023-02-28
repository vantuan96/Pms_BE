using DataAccess.Repository;
using DrFee.Clients;
using DrFee.Utils;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrFee.ScheduleJobs
{
    public class SyncRevenueJob
    {
        protected IUnitOfWork unitOfWork = new EfUnitOfWork();
    }

    public class SyncOHRevenueJob : SyncRevenueJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            CustomLog.intervaljoblog.Info($"<OH revenue> Start job");
            //var to = DateTime.Now;
            //var from = to.AddHours(-1);
            //var from = DateTime.Now.AddDays(-10);
            ////var from = DateTime.Now.AddYears(-7);
            //var to = DateTime.Now;
            var from = Convert.ToDateTime("2021-06-07 00:00:07");
            var to = Convert.ToDateTime("2021-06-07 18:47:07.590");
            OHClient.SyncRevenue(from, to);
            CustomLog.intervaljoblog.Info($"<OH revenue> End job");
        }
    }

    public class SyncEHosRevenueJob : SyncRevenueJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            CustomLog.intervaljoblog.Info($"<OH revenue> Start job");
            var to = DateTime.Now;
            var from = to.AddHours(-1);
            EHosClient.SyncRevenue(from, to);
            CustomLog.intervaljoblog.Info($"<OH revenue> End job");
        }
    }

    public class SyncViHCRevenueJob : SyncRevenueJob, IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            CustomLog.intervaljoblog.Info($"<ViHC revenue> Start job");
            //var to = DateTime.Now;
            //var from = to.AddHours(-1);
            var from = DateTime.Now.AddMonths(-3);
            var to = DateTime.Now;
            //var from = Convert.ToDateTime("2021-04-12 00:00:00");
            //var to = Convert.ToDateTime("2021-04-14 23:59:59");
            ViHCClient.SyncRevenue(from, to);
            CustomLog.intervaljoblog.Info($"<ViHC revenue> End job");
        }
    }
}