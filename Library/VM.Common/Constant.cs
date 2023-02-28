using System;
using System.Collections.Generic;

namespace VM.Common
{
    public class Constant
    {
        #region Define App Key
        public readonly static string Key_ListGroupCodeIsIncludeChildPackage = "LISTGROUPCODEISINCLUDECHILDPACKAGE";
        public readonly static string Key_ListGroupCodeIsMaternityPackage = "LISTGROUPCODEISMATERNITYPACKAGE";
        public readonly static string Key_ListGroupCodeIsVaccinePackage = "LISTGROUPCODEISVACCINEPACKAGE";
        public readonly static string Key_ListGroupCodeIsMCRPackage = "LISTGROUPCODEISMCRPACKAGE";
        //linhht key gói Bundle Payment
        public readonly static string Key_ListGroupCodeIsBundlePackage = "LISTGROUPCODEISBUNDLEPACKAGE";
        #endregion .Define App Key
        public readonly static string[] MALE_SAMPLE = { "Nam", "nam", "Male", "male", "M", "Trai", "T" };
        #region Log
        public readonly static string Log_Type_Info = "Log_Info";
        public readonly static string Log_Type_Debug = "Log_Debug";
        public readonly static string Log_Type_Error = "Log_Error";
        #endregion
        #region Datetime format
        public readonly static string DATE_SQL = "yyyy-MM-dd";
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
        public static DateTime CurrentDate
        {
            get
            {
                return DateTime.Now.Date;
            }
        }
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
        public readonly static string[] ChargeStatusAvailable = { "1" };
        public readonly static string[] ChargeStatusCancel = { "0" };
        public readonly static List<int> ListStatusCancel_Terminated = new List<int>(){ 3, 4 };
        public readonly static List<int> ListStatusNotGetTranferred = new List<int>() {2, 6, 7 , 8 };

        public readonly static string[] StatusUpdatePriceOKs = { "OK" };
        public readonly static string[] StatusUpdatePriceFAILs = { "ERR_NO_USER", "ERR_RHAP" };
        public readonly static string[] StatusUpdatePriceError_DonotExist_ChargeId = { "ERR_NO_CHARGE_ID" };
        public readonly static string StatusUpdatePriceError_No_User = "ERR_NO_USER";
        public readonly static string Patient_Not_Found = "PATIENT_NOT_FOUND";
        public readonly static string PATInPkg_Not_Found = "PATIENT_INPACKAGE_NOT_FOUND";
        public readonly static string Confirm_Apply_Charge_IsOtherPatientInPackage = "CONFIRM_BELONG_CHARGE_ISOTHERPATIENTINPACKAGE";
        public readonly static string Confirm_Apply_Charge_IsOtherUserProcess = "CONFIRM_BELONG_CHARGE_ISOTHERUSERPROCESS";
        public readonly static string Confirm_Apply_Charge_IsOtherSession = "CONFIRM_BELONG_CHARGE_ISOTHERSESSION";
        #endregion
        #region Personal Type
        #endregion .Personal Type
        #region Service Type
        /// <summary>
        /// Dịch vụ
        /// </summary>
        public readonly static string SERVICE_TYPE_SRV = "SRV";
        /// <summary>
        /// Thuốc / VTTH
        /// </summary>
        public readonly static string SERVICE_TYPE_INV = "INV";
        /// <summary>
        /// Gói/Package
        /// </summary>
        public readonly static string SERVICE_TYPE_PCK = "PCK";
        #endregion .Service Type
        #region Service Cat Code
        public readonly static string LAB_CODE = "LIS";
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
        public readonly static string SPEC_NURSE = "NURSE";
        #endregion
        #region Package Code
        /// <summary>
        /// Thai sản
        /// </summary>
        public readonly static List<string> ListGroupCodeIsMaternityPackage = new List<string>() { "TS"};
        /// <summary>
        /// linhht Bundle Payment
        /// </summary>
        public readonly static List<string> ListGroupCodeIsBundlePackage = new List<string>() { "DT" };
        /// <summary>
        /// Công nghệ TBG/ Biobank
        /// </summary>
        public readonly static List<string> ListGroupCodeIsMCRPackage = new List<string>() { "TBG" };
        /// <summary>
        /// Vaccine
        /// </summary>
        public readonly static List<string> ListGroupCodeIsVaccinePackage = new List<string>() { "VC" };
        /// <summary>
        /// List group package code to is include child
        /// </summary>
        public readonly static List<string> ListGroupCodeIsIncludeChildPackage = new List<string>() { "TS", "TBG" };
        public readonly static string PK_HC_CODE = "PK.HC";
        public readonly static string PK_SPK_CODE = "PK.SPK";
        public readonly static string PK_VC_CODE = "PK.VACCINCE";
        public readonly static string PK_SL_CODE = "PK.SL";
        public readonly static string PK_SPK_OR_VC_CODE = "PK.SPK_OR_VC";
        #endregion
        #region ConfigRule DataType
        public readonly static List<string> VISIT_TYPE_PACKAGES = new List<string>() { "VMPK", "PKIPD" };
        public readonly static string CF_DATATYPE_VISITTYPE = "VISITTYPE";
        public readonly static string CF_DATATYPE_PACKAGE_CODE = "PACKAGE_CODE";
        public readonly static string CF_DATATYPE_VMHC = "VMHC";
        public readonly static Dictionary<string, int> CF_TYPE_APPLY_CAL = new Dictionary<string, int> {
            { "BOTH", 1 },
            { "ONLY_CHARGE", 2 },
            { "ONLY_OPERATION", 3 },
            { "NOTCALCULATING", 4 }
        };
        #endregion .ConfigRule DataType
        #region OR
        public readonly static int OR_DATE_RANGE = 3;
        #endregion

        #region Revenue
        public readonly static string PAYMENT_VOI_STATUS = "VOI";
        public readonly static string PAYMENT_PSL_STATUS = "PSL";
        public readonly static string PAYMENT_UNK_STATUS = "UNK";
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
        public readonly static Dictionary<string, int> PROCESS_REVENUE_STATUS = new Dictionary<string, int> {
            { "PROCESS_FAIL", -1 },
            { "PROCESS_DONTNEED", 0},
            { "PROCESS_NEED", 1 },
            { "PROCESS_KEPT4PROCESS", 2 },
            { "PROCESS_SUCCESS", 3 },
            { "PROCESS_NOTFOUND", 404 }
        };
        public readonly static Dictionary<string, int> CHARGE_REVENUE_DATE = new Dictionary<string, int> {
            { "CHARGE_DATE", 1 },
            { "INVOICE_DATE", 2},
            { "INVOICE_UPDATE_DATE", 3},
            { "PACKAGE_CANCELLED_DATE", 4 }
        };
        public readonly static string PACKAGE_OPEN = "OPEN";
        public readonly static string PACKAGE_CANCELED = "PATIENT_CANCELED";
        /// <summary>
        /// Danh sách trạng thái StatusForProcess: Trạng thái cần xử lý, tính toán DT
        /// </summary>
        public readonly static int[] NEED_PROCESS_REVENUE_STATUS = new int[] { 1};
        #endregion

        #region System
        public readonly static string SERVICE_APP = "APP";
        public readonly static string SERVICE_APIGW = "APIGW";
        public readonly static Dictionary<string, int> SYSTEM_CONFIG = new Dictionary<string, int> {
            { "SYNC_SERVICE", 1 },
            { "SYNC_DEPARTMENT", 2 },
            { "SYNC_REVENUE", 3 },
            { "SYNC_REVENUE_BYDAY", 4 },
            { "GET_HISCHARGE_4UPDATEDIMS",5 }
        };
        public readonly static Dictionary<string, int> SYSTEM_CALCULATE_STATUS = new Dictionary<string, int> {
            { "Error", -500 },
            { "HaveNoSite", -1 },
            { "Done", 1 },
        };
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

    }
}