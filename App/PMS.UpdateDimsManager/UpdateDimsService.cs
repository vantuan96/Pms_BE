using PMS.Business.ScheduleJobs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using VM.Common;
using VM.Data.Queue;

namespace PMS.UpdateDimsManager
{
    public partial class UpdateDimsService : ServiceBase
    {
        static string TEMP_DIR_PATH = "";
        private static UpdateDimsHisRevenueJob[] _updateDimsHisRevenueJobs = null;
        private static GetDimsHisRevenueJob[] _getDimsHisRevenueJobs = null;
        public UpdateDimsService()
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
                StartProgram();
                CustomLog.intervaljoblog.Info("UpdateDimsService was started");

            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("UpdateDimsService start Error: {0}", ex));
            }
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            try
            {
                StopProgram();
                CustomLog.intervaljoblog.Info("UpdateDimsService was stoped");
            }
            catch (Exception ex)
            {
                CustomLog.errorlog.Info(string.Format("UpdateDimsService stop Error: {0}", ex));
            }
        }

        #region Function start job
        static void StartProgram()
        {
            Console.Clear();
            TEMP_DIR_PATH = System.IO.Directory.GetCurrentDirectory() + "\\Temp";
            //DisplayMessage("Loading config data ...\r\n");
            //Load data here
            AppConfiguration cons = null;
            //CPCatalog cps = null;
            var cnFile = ConfigurationManager.AppSettings["CF_DATA_PATH"];
            var code = ConfigurationManager.AppSettings["CF_CODE"];
            try
            {
                cons = AppConfigurationSerializer.ReadFile(cnFile);
                //cps = CPCatalogSerializer.ReadFile(cpFile);
            }
            catch (Exception ex)
            {
                //DisplayMessage("Error when load config data: \r\n" + ex.Message + "\r\n");
                //DisplayMessage("Press any key to return.");
                Console.ReadLine();
                return;
            }


            VMConfiguration cn = null;

            if (cons == null)
            {
                //DisplayMessage("No config data loaded\r\n");
                //DisplayMessage("Press any key to return.");
                Console.ReadLine();
                return;
            }
            else
            {
                foreach (VMConfiguration c in cons.VMConfigurations)
                {
                    if (c.CODE.ToUpper() == code.ToUpper())
                    {
                        cn = c;
                        break;
                    }
                }
            }

            if (cn == null)
            {
                CustomLog.Instant.IntervalJobLog("No configuration data with code: " + code + ".\r\n Please check value in App.config or Connection Manager tool.\r\n", Constant.Log_Type_Info, printConsole: true);
                CustomLog.Instant.IntervalJobLog("Press any key to return.", Constant.Log_Type_Info, printConsole: true);
                Console.ReadLine();
                return;
            }


            Console.Title = cn.NAME;
            CustomLog.Instant.IntervalJobLog("Press any key to start PMS | UPDATE DIMS PROGRAM", Constant.Log_Type_Info, printConsole: true);
            Console.ReadLine();

            if (System.IO.Directory.GetFileSystemEntries(TEMP_DIR_PATH).Length != 0)
            {
                CustomLog.Instant.IntervalJobLog("Restore queue saved before (N to inogre)?", Constant.Log_Type_Info, printConsole: true);
                if (Console.ReadKey().Key != ConsoleKey.N)
                {
                    Console.WriteLine();
                    InitGlobalData(cn);
                }
            }
            CustomLog.Instant.IntervalJobLog("Start Get HisCharge 4 Update Dims", Constant.Log_Type_Info, printConsole: true);
            StartGetHisCharge4Update(cn);

            CustomLog.Instant.IntervalJobLog("Start Update Dims HisRevenue", Constant.Log_Type_Info, printConsole: true);
            StartUpdateDimsHis(cn);

            CustomLog.Instant.IntervalJobLog("Program started successfully", Constant.Log_Type_Info, printConsole: true);
        }
        private static void StartGetHisCharge4Update(VMConfiguration cn)
        {
            try
            {
                _getDimsHisRevenueJobs = new GetDimsHisRevenueJob[cn.THREAD.GETHISCHARGE4UPDATEDIMS_THREADS];
                for (int i = 0; i < _getDimsHisRevenueJobs.Length; i++)
                {
                    _getDimsHisRevenueJobs[i] = new GetDimsHisRevenueJob { Cn = cn };
                    string sPName = "Get HisCharge 4 Update Dims (" + (i + 1).ToString(CultureInfo.InvariantCulture) + ") started: " + Guid.NewGuid().ToString();
                    _getDimsHisRevenueJobs[i].Start(sPName);
                }
            }
            catch (Exception pException)
            {
                CustomLog.Instant.ErrorLog("Get HisCharge 4 Update Dims starting error: " + pException.Message, Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Press any key to return.", Constant.Log_Type_Error, printConsole: true);
                Console.ReadLine();
                return;
            }
        }
        private static void StartUpdateDimsHis(VMConfiguration cn)
        {
            try
            {
                _updateDimsHisRevenueJobs = new UpdateDimsHisRevenueJob[cn.THREAD.UPDATEDIMSREVENUE_THREADS];
                for (int i = 0; i < _updateDimsHisRevenueJobs.Length; i++)
                {
                    _updateDimsHisRevenueJobs[i] = new UpdateDimsHisRevenueJob { Cn = cn };
                    string sPName = "Update Dims HisRevenue (" + (i + 1).ToString(CultureInfo.InvariantCulture) + ") started: " + Guid.NewGuid().ToString();
                    _updateDimsHisRevenueJobs[i].Start(sPName);
                }
            }
            catch (Exception pException)
            {
                CustomLog.Instant.ErrorLog("Update Dims HisRevenue starting error: " + pException.Message, Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Press any key to return.", Constant.Log_Type_Error, printConsole: true);
                Console.ReadLine();
                return;
            }
        }
        #endregion
        static void InitGlobalData(VMConfiguration cn)
        {
            if (!System.IO.Directory.Exists(TEMP_DIR_PATH))
            {
                CustomLog.Instant.IntervalJobLog("Temp dir not exist for re-loading", Constant.Log_Type_Info, printConsole: true);
                return;
            }
            try
            {
                Globals.LoadAllQueue(TEMP_DIR_PATH);
                CustomLog.Instant.IntervalJobLog("All queue loaded", Constant.Log_Type_Info, printConsole: true);
            }
            catch (Exception ex)
            {
                CustomLog.Instant.ErrorLog(ex.ToString(), Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Init global data error: " + ex.Message, Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Press any key to return.", Constant.Log_Type_Error, printConsole: true);
                Console.ReadLine();
                return;
            }
        }
        static void StopProgram()
        {
            try
            {
                CustomLog.Instant.IntervalJobLog("Program stopping!", Constant.Log_Type_Info, printConsole: true);
                try
                {
                    Globals.SaveAllQueue(TEMP_DIR_PATH);
                    CustomLog.Instant.IntervalJobLog("All queue saved", Constant.Log_Type_Info, printConsole: true);
                }
                catch (Exception ex)
                {
                    CustomLog.Instant.ErrorLog("Save queue error:" + ex.ToString(), Constant.Log_Type_Error, printConsole: true);
                }
                if (_getDimsHisRevenueJobs != null)
                {
                    for (int i = 0; i < _getDimsHisRevenueJobs.Length; i++)
                    {
                        _getDimsHisRevenueJobs[i].Stop();
                        _getDimsHisRevenueJobs[i] = null;
                    }
                }
                if (_updateDimsHisRevenueJobs != null)
                {
                    for (int i = 0; i < _updateDimsHisRevenueJobs.Length; i++)
                    {
                        _updateDimsHisRevenueJobs[i].Stop();
                        _updateDimsHisRevenueJobs[i] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLog.Instant.ErrorLog(ex.Message, Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog("Program stopped by error!", Constant.Log_Type_Error, printConsole: true);
                CustomLog.Instant.ErrorLog(ex.ToString(), Constant.Log_Type_Error, printConsole: true);
            }
        }
    }
}
