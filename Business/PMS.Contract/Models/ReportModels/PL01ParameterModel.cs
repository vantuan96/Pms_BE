using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PMS.Contract.Models.ReportModels
{
    public class PL01ParameterModel
    {
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "StartAt is invalid!")]
        public string StartAt { get; set; }
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "EndAt is invalid!")]
        public string EndAt { get; set; }
        [RegularExpression(@"(?im)^[{(]?[0-9A-F]{8}[-]?(?:[0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$", ErrorMessage = "Sites is invalid!")]
        public string Sites { get; set; }
        public bool? IsPackage { get; set; }
        [RegularExpression(@"^(?=[A-Za-z0-9])(?!.*[._\[\]-]{2})[A-Za-z0-9._\[\]-]{3,50}$", ErrorMessage = "PID is invalid!")]
        public string PID { get; set; }
        [RegularExpression(@"^(?=[A-Za-z0-9])(?!.*[._\[\]-]{2})[A-Za-z0-9._\[\]-]{3,50}$", ErrorMessage = "VisitCode is invalid!")]
        public string VisitCode { get; set; }
        [RegularExpression(@"^(?=[A-Za-z0-9])(?!.*[._\[\]-]{2})[A-Za-z0-9._\[\]-]{3,50}$", ErrorMessage = "ServiceCode is invalid!")]
        public string ServiceCode { get; set; }
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

    public class PL01TempModel
    {
        public string Doctor { get; set; }
        public string Specialty { get; set; }
        public string Category { get; set; }
        public double? Amount { get; set; }
        public double? ChargeAmount { get; set; }
        public double? OperationAmount { get; set; }
        public bool IsPackage { get; set; }
    }


    public class PL01ParameterDetailModel: PagingParameterModel
    {
        public int? StartAt { get; set; }
        public int? EndAt { get; set; }
        public string Sites { get; set; }
        public string Categories { get; set; }
        public string Username { get; set; }
        public bool? IsPackage { get; set; }
        public List<Guid?> GetSites()
        {
            string[] id = Sites.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
        public List<Guid?> GetCategories()
        {
            string[] id = Categories.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
    }

    public class PL01TempDetailModel
    {
        public string Username { get; set; }
        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool? IsPackage { get; set; }
        public double? Amount { get; set; }
        public double? NotCalculate { get; set; }
        public double? Calculate { get; set; }
    }
}