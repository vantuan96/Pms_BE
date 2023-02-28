using PMS.Business.ScheduleJobs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.SynDataManager
{
    partial class PMSSynDataService : ServiceBase
    {
        public PMSSynDataService()
        {
            InitializeComponent();
        }
        public void OnDebug()
        {
            OnStart(null);
        }
        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            try
            {
                //Console.Title = "DIMS | DrFee Sync Data Manager";
                //Console.Title = ConfigHelper.AppName;
                //Using Quartz to create job
                JobScheduler.Start();
                CustomLog.intervaljoblog.Info("SynDataManager was started");

            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("SynDataManager start Error: {0}", ex));
            }
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            try
            {
                CustomLog.intervaljoblog.Info("SynDataManager was stoped");
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("SynDataManager stop Error: {0}", ex));
            }
        }
    }
}
