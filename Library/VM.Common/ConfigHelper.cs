using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

namespace VM.Common
{
    public static class  ConfigHelper
    {
        public static string AppKey { get { return ConfigurationManager.AppSettings["appkey"] != null ? ConfigurationManager.AppSettings["appkey"].ToString() : string.Empty; } }
        public static string AppName { get { return ConfigurationManager.AppSettings["AppName"] != null ? ConfigurationManager.AppSettings["AppName"].ToString() : string.Empty; } }
        public static int CF_ApiTimeout_minutes { get { return ConfigurationManager.AppSettings["ApiTimeout.minutes"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["ApiTimeout.minutes"].ToString()) : 3; } }
        public static int CF_ExMinutesToNextProcess { get { return ConfigurationManager.AppSettings["ExMinutesToNextProcess"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["ExMinutesToNextProcess"].ToString()) : 60; } }
        public static string CF_SetMasterData_CS { get { return ConfigurationManager.AppSettings["SetMasterData_CS"] != null ? ConfigurationManager.AppSettings["SetMasterData_CS"].ToString() : "0 0/5 0/1 ? * * *"; } }
        public static string CF_SyncOHHospital_CS { get { return ConfigurationManager.AppSettings["SyncOHHospital_CS"] != null ? ConfigurationManager.AppSettings["SyncOHHospital_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SyncOHService_CS { get { return ConfigurationManager.AppSettings["SyncOHService_CS"] != null ? ConfigurationManager.AppSettings["SyncOHService_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SynceHosService_CS { get { return ConfigurationManager.AppSettings["SynceHosService_CS"] != null ? ConfigurationManager.AppSettings["SynceHosService_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SyncOHDepartment_CS { get { return ConfigurationManager.AppSettings["SyncOHDepartment_CS"] != null ? ConfigurationManager.AppSettings["SyncOHDepartment_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SynceHosDepartment_CS { get { return ConfigurationManager.AppSettings["SynceHosDepartment_CS"] != null ? ConfigurationManager.AppSettings["SynceHosDepartment_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_AutoUpdatePatientInPackageStatus_CS { get { return ConfigurationManager.AppSettings["AutoUpdatePatientInPackageStatus_CS"] !=null? ConfigurationManager.AppSettings["AutoUpdatePatientInPackageStatus_CS"].ToString(): "0 0/5 0/1 ? * * *"; } }
        public static string CF_AutoUpdatePatientInPackageUsing_CS { get { return ConfigurationManager.AppSettings["AutoUpdatePatientInPackageUsing_CS"] != null ? ConfigurationManager.AppSettings["AutoUpdatePatientInPackageUsing_CS"].ToString() : "0 0/5 0/1 ? * * *"; } }
        public static string CF_AutoCalculatePricePolicy_CS { get { return ConfigurationManager.AppSettings["AutoCalculatePricePolicy_CS"] != null ? ConfigurationManager.AppSettings["AutoCalculatePricePolicy_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_AutoRegPackageService_CS { get { return ConfigurationManager.AppSettings["AutoRegPackageService_CS"] != null ? ConfigurationManager.AppSettings["AutoRegPackageService_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_AutoUpdateUsingService_CS { get { return ConfigurationManager.AppSettings["AutoUpdateUsingService_CS"] != null ? ConfigurationManager.AppSettings["AutoUpdateUsingService_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_AutoRefundUsingService_CS { get { return ConfigurationManager.AppSettings["AutoRefundUsingService_CS"] != null ? ConfigurationManager.AppSettings["AutoRefundUsingService_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_AutoUpdateOriginalPrice_CS { get { return ConfigurationManager.AppSettings["AutoUpdateOriginalPrice_CS"] != null ? ConfigurationManager.AppSettings["AutoUpdateOriginalPrice_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SyncViHCRevenue_CS { get { return ConfigurationManager.AppSettings["SyncViHCRevenue_CS"] != null ? ConfigurationManager.AppSettings["SyncViHCRevenue_CS"].ToString() : "0 0/15 0/1 ? * * *"; } }
        public static string UriMongDB_Queue { get { return ConfigurationManager.AppSettings["UriMongoDBCn.Queue"] != null ? ConfigurationManager.AppSettings["UriMongoDBCn.Queue"].ToString() : string.Empty; } }
        public static string UriMongDB_MasterData { get { return ConfigurationManager.AppSettings["UriMongoDBCn.MasterData"] != null ? ConfigurationManager.AppSettings["UriMongoDBCn.MasterData"].ToString() : string.Empty; } }
        public static string VisitType_List { get { return ConfigurationManager.AppSettings["VisitType_List"] != null ? ConfigurationManager.AppSettings["VisitType_List"].ToString() : string.Empty; } }
        public static string SiteCode { get { return ConfigurationManager.AppSettings["SiteCode"] != null ? ConfigurationManager.AppSettings["SiteCode"].ToString() : "ALL"; } }
        public static string Folder_store_temp_barcode { get { return ConfigurationManager.AppSettings["folder_store_temp_barcode"] != null ? ConfigurationManager.AppSettings["folder_store_temp_barcode"].ToString() : string.Empty; } }
        #region OHService config
        public static string OHService_URL { get { return ConfigurationManager.AppSettings["OHService_URL"] != null ? ConfigurationManager.AppSettings["OHService_URL"].ToString() : string.Empty; } }
        public static string OHServiceUsername { get { return ConfigurationManager.AppSettings["OHServiceUsername"] != null ? ConfigurationManager.AppSettings["OHServiceUsername"].ToString() : string.Empty; } }
        public static string OHServicePassword { get { return ConfigurationManager.AppSettings["OHServicePassword"] != null ? ConfigurationManager.AppSettings["OHServicePassword"].ToString() : string.Empty; } }
        public static string OHService_Token
        {
            get
            {
                string strToken = string.Empty;
                var authenticationString = $"{ConfigHelper.OHServiceUsername}:{ConfigHelper.OHServicePassword}";
                strToken = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
                return strToken;
            }
        }
        public static string OHUserDefault { get { return ConfigurationManager.AppSettings["OHUserDefault"] != null ? ConfigurationManager.AppSettings["OHUserDefault"].ToString() : string.Empty; } }
        public static int CF_StatusForProcess { get { return ConfigurationManager.AppSettings["StatusForProcess"] != null ? Convert.ToInt32(ConfigurationManager.AppSettings["StatusForProcess"].ToString()) : 1; } }
        #endregion .OHService config
        public static string GetDefaultConnectionString()
        {
            if (ConfigurationManager.ConnectionStrings["PMSContext"] != null)
            {
                return ConfigurationManager.ConnectionStrings["PMSContext"].ConnectionString;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}