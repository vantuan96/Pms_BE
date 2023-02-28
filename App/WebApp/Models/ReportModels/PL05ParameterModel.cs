using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DrFee.Models.ReportModels
{
    public class PL05ParameterModel
    {
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "StartAt is invalid!")]
        public string StartAt { get; set; }
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "EndAt is invalid!")]
        public string EndAt { get; set; }
        [RegularExpression(@"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$", ErrorMessage = "Sites is invalid!")]
        public string Sites { get; set; }
        /// <summary>
        /// Filter chỉ gói
        /// </summary>
        public bool? IsPackage { get; set; }
        /// <summary>
        /// Filter chỉ lẻ
        /// </summary>
        public bool? IsSingle { get; set; }
        [RegularExpression(@"^(?=[A-Za-z0-9])(?!.*[._\[\]-]{2})[A-Za-z0-9._\[\]-]{3,50}$", ErrorMessage = "PID is invalid!")]
        public string PID { get; set; }
        [RegularExpression(@"^(?=[A-Za-z0-9])(?!.*[._\[\]-]{2})[A-Za-z0-9._\[\]-]{3,50}$", ErrorMessage = "VisitCode is invalid!")]
        public string VisitCode { get; set; }
        [RegularExpression(@"^(?=[A-Za-z0-9])(?!.*[._\[\]-]{2})[A-Za-z0-9._\[\]-]{3,50}$", ErrorMessage = "ServiceCode is invalid!")]
        /// <summary>
        /// Mã dịch vụ (Nội dung trên layout)
        /// </summary>
        public string ServiceCode { get; set; }
        /// <summary>
        /// Tài khoản AD Bác Sĩ
        /// </summary>
        [RegularExpression(@"^(?=[A-Za-z0-9])(?!.*[._\[\]-]{2})[A-Za-z0-9._\[\]-]{3,50}$", ErrorMessage = "AdAccount is invalid!")]
        public string AdAccount { get; set; }
        /// <summary>
        /// Mã nhóm dịch vụ
        /// </summary>
        [RegularExpression(@"^(?=[A-Za-z0-9])(?!.*[._\[\]-]{2})[A-Za-z0-9._\[\]-]{3,50}$", ErrorMessage = "ServiceCat is invalid!")]
        public string ServiceCat { get; set; }
        /// <summary>
        /// TABLE: Lấy danh sách dữ liệu
        /// EXP_EXCEL: Xuất dữ liệu - file excel
        /// </summary>
        public string DataFMType { get; set; } = "TABLE";
        public string GetSites()
        {
            string[] ids = Sites.Trim().Split(',');
            return string.Join("','", ids);
        }
    }
}