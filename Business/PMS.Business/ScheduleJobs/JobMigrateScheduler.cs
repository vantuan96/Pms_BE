using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.Business.ScheduleJobs
{
    public class JobMigrateScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            ////Auto tính giá dịch vụ trong gói
            //#region Tự động tính giá dịch vụ trong gói
            //IJobDetail sync_auto_calculate_price_policy_job = JobBuilder.Create<AutoMigrateCalculatePriceJob>().Build();
            //ITrigger sync_auto_calculate_price_policy_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule(ConfigHelper.CF_AutoCalculatePricePolicy_CS)
            //    //.WithCronSchedule("0 50 11 ? * *")
            //    .Build();
            //scheduler.ScheduleJob(sync_auto_calculate_price_policy_job, sync_auto_calculate_price_policy_trigger);
            //CustomLog.Instant.IntervalJobLog("AutoMigrateCalculatePrice job was created", Constant.Log_Type_Info, printConsole: true);
            //#endregion .Tự động tính giá dịch vụ trong gói

            //Auto tính giá dịch vụ trong gói - Concerto
            //#region Tự động tính giá dịch vụ trong gói Concerto
            //IJobDetail sync_concerto_auto_calculate_price_policy_job = JobBuilder.Create<Concerto_AutoMigrateCalculatePriceJob>().Build();
            //ITrigger sync_concerto_auto_calculate_price_policy_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule(ConfigHelper.CF_AutoCalculatePricePolicy_CS)
            //    //.WithCronSchedule("0 50 11 ? * *")
            //    .Build();
            //scheduler.ScheduleJob(sync_concerto_auto_calculate_price_policy_job, sync_concerto_auto_calculate_price_policy_trigger);
            //CustomLog.Instant.IntervalJobLog("Concerto AutoMigrateCalculatePrice job was created", Constant.Log_Type_Info, printConsole: true);
            //#endregion .Tự động tính giá dịch vụ trong gói Concerto

            //Auto Reg service package for patient
            #region Reg service package for patient
            IJobDetail sync_auto_regpackage_job = JobBuilder.Create<AutoRegPackageServiceV2Job>().Build();
            ITrigger sync_auto_regpackage_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_AutoRegPackageService_CS)
                .Build();
            scheduler.ScheduleJob(sync_auto_regpackage_job, sync_auto_regpackage_trigger);
            CustomLog.Instant.IntervalJobLog("AutoRegPackageService job was created", Constant.Log_Type_Info, printConsole: true);
            #endregion .Reg service package for patient

            //Auto update using stat for patient in package
            #region Update using stat for patient in package
            IJobDetail sync_auto_mappingcharge_job = JobBuilder.Create<AutoUpdateUsingServiceV2Job>().Build();
            ITrigger sync_auto_mappingcharge_trigger = TriggerBuilder.Create()
                .WithCronSchedule(ConfigHelper.CF_AutoUpdateUsingService_CS)
                .Build();
            scheduler.ScheduleJob(sync_auto_mappingcharge_job, sync_auto_mappingcharge_trigger);
            CustomLog.Instant.IntervalJobLog("AutoUpdateUsingService job was created", Constant.Log_Type_Info, printConsole: true);
            #endregion .Auto update using stat for patient in package

            ////Auto Refund service package for patient
            //#region Refund service package for patient
            //IJobDetail sync_auto_refundusing_job = JobBuilder.Create<AutoUpdateRefundServiceJob>().Build();
            //ITrigger sync_auto_refundusing_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule(ConfigHelper.CF_AutoRefundUsingService_CS)
            //    .Build();
            //scheduler.ScheduleJob(sync_auto_refundusing_job, sync_auto_refundusing_trigger);
            //CustomLog.Instant.IntervalJobLog("AutoRefundUsingService job was created", Constant.Log_Type_Info, printConsole: true);
            //#endregion .Refund service package for patient

            ////Auto update original price for charge
            //#region Update original price for charge
            //IJobDetail sync_auto_updateoriginalprice_job = JobBuilder.Create<AutoUpdateOriginalPriceJob>().Build();
            //ITrigger sync_auto_updateoriginalprice_trigger = TriggerBuilder.Create()
            //    .WithCronSchedule(ConfigHelper.CF_AutoUpdateOriginalPrice_CS)
            //    .Build();
            //scheduler.ScheduleJob(sync_auto_updateoriginalprice_job, sync_auto_updateoriginalprice_trigger);
            //CustomLog.Instant.IntervalJobLog("AutoUpdateOriginalPriceCharge job was created", Constant.Log_Type_Info, printConsole: true);
            //#endregion .Update original price for charge
        }
    }
}
