using System.Collections.Generic;

namespace DrFee.Common
{
    public class Constant
    {
        public readonly static string[] MALE_SAMPLE = { "Nam", "nam", "Male", "male", "M", "Trai", "T" };

        #region Datetime format
        public readonly static string DATETIME_SQL = "yyyy-MM-dd HH:mm:ss";
        public readonly static string TIME_FORMAT = "HH:mm:ss";
        public readonly static string TIME_FORMAT_WITHOUT_SECOND = "HH:mm";
        public readonly static string TIME_DATE_FORMAT = "HH:mm:ss dd/MM/yyyy";
        public readonly static string TIME_DATE_FORMAT_WITHOUT_SECOND = "HH:mm dd/MM/yyyy";
        public readonly static string MONTH_YEAR_FORMAT = "MM/yyyy";
        public readonly static string YEAR_MONTH_FORMAT = "yyyyMM";
        public readonly static string DATE_FORMAT = "dd/MM/yyyy";
        public readonly static string DATE_TIME_FORMAT = "dd/MM/yyyy HH:mm:ss";
        public readonly static string DATE_TIME_FORMAT_WITHOUT_SECOND = "dd/MM/yyyy HH:mm";
        #endregion
        #region DataFormatType
        public readonly static string FM_TABLE = "TABLE";
        public readonly static string FM_EXP_EXCEL = "EXP_EXCEL";
        #endregion .DataFormatType
        #region HIS
        public readonly static Dictionary<string, int> HIS_CODE = new Dictionary<string, int> {
            { "OH", 0 },
            { "EHos", 1 }
        };
        #endregion

        #region Code
        public readonly static string XRAY_CODE = "RIS";
        public readonly static string VIHC_CODE = "VMHC";
        public readonly static string PTTT_CODE = "PTTT";
        #endregion
        #region SpecialtyCode
        public readonly static string SPEC_NOIDAKHOA = "NOIDAKHOA";
        public readonly static string SPEC_MAT = "MAT";
        public readonly static string SPEC_RHM = "RHM";
        public readonly static string SPEC_TMH = "TMH";
        public readonly static string SPEC_SPK = "SPK";
        #endregion
        #region OR
        public readonly static int OR_DATE_RANGE = 3;
        #endregion

        #region Revenue
        public readonly static Dictionary<string, int> CALCULATED_REVENUE_STATUS = new Dictionary<string, int> {
            { "NotAvailable", -2 },
            { "Removed", -1 },
            { "CancelCharge", 0 },
            { "VirtualRevenue", 1 },
            { "CancelInvoice", 2 },
            { "Revenue", 3 },
            { "Debt", 4 },
            { "NotCalculating", 5 },
        };
        public readonly static int[] NEED_CALCULATE_REVENUE_STATUS = new int[] {1,3,4};
        #endregion

        #region System
        public readonly static string SERVICE_APIGW = "APIGW";
        public readonly static Dictionary<string, int> SYSTEM_NOTIFICATION_STATUS = new Dictionary<string, int> {
            { "Error", 0 },
            { "Sent", 1 },
            { "Done", 2 },
        };
        public readonly static string[] IGNORE_EXTEND_SESSION_PATH = { "/api/notification/", };
        public readonly static string[] IGNORE_UPDATE_SESSION_PATH = {
            "/api/account/login/",
            "/api/account/logout/",
            "/api/user/choosesite/"
        };
        #endregion

        #region PL01
        public readonly static string[] PL01_IGNORE_CALCULATE_FIELD = { "Doctor", "Specialty", "IsTotal" };
        public readonly static string[] PL05_IGNORE_CALCULATE_FIELD = { "CustomerPID", "CustomerName", "ServiceCode", "ServiceName", "RevenueDate", "ChargeDate", "BillingNumber", "CatName", "IsPackage", "CustomerName", "IsTotal" };
        #endregion
    }
}