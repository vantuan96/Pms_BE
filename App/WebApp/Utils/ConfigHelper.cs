using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace DrFee.Utils
{
    public static class  ConfigHelper
    {
        public static string CF_SyncOHService_CS { get { return ConfigurationManager.AppSettings["SyncOHService_CS"] != null ? ConfigurationManager.AppSettings["SyncOHService_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SynceHosService_CS { get { return ConfigurationManager.AppSettings["SynceHosService_CS"] != null ? ConfigurationManager.AppSettings["SynceHosService_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SyncOHDepartment_CS { get { return ConfigurationManager.AppSettings["SyncOHDepartment_CS"] != null ? ConfigurationManager.AppSettings["SyncOHDepartment_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SynceHosDepartment_CS { get { return ConfigurationManager.AppSettings["SynceHosDepartment_CS"] != null ? ConfigurationManager.AppSettings["SynceHosDepartment_CS"].ToString() : "0 0/45 0/1 ? * * *"; } }
        public static string CF_SyncOHRevenue_CS { get { return ConfigurationManager.AppSettings["SyncOHRevenue_CS"]!=null? ConfigurationManager.AppSettings["SyncOHRevenue_CS"].ToString(): "0 0/5 0/1 ? * * *"; } }
        public static string CF_SyncViHCRevenue_CS { get { return ConfigurationManager.AppSettings["SyncViHCRevenue_CS"] != null ? ConfigurationManager.AppSettings["SyncViHCRevenue_CS"].ToString() : "0 0/15 0/1 ? * * *"; } }
    }
}