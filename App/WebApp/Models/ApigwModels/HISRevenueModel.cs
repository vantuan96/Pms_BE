using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrFee.Models.ApigwModels
{
    public class HISRevenueModel
    {
        public int HISCode { get; set; }
        public string HospitalId { get; set; }
        public string Service { get; set; }
        public string ChargeId { get; set; }
        public int ChargeMonth { get; set; }
        public DateTime ChargeUpdatedDate { get; set; }
        public string ChargeDoctorDepartmentCode { get; set; }
        public string ChargeDoctor { get; set; }
        public string OperationId { get; set; }
        public string OperationDoctorDepartmentCode { get; set; }
        public string OperationDoctor { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPID { get; set; }
        public string PackageCode { get; set; }
        public double? AmountInPackage { get; set; }
        public bool IsPackage { get; set; }
        public string VisitType { get; set; }
        public string VisitCode { get; set; }
        public string InvoiceNumber { get; set; }
        public string GetDepartmentCodeExt()
        {
            try
            {
                var lst = this.ChargeDoctorDepartmentCode.Split('.');
                return lst[1].Length == 3 ? lst[1] : null;
            }
            catch
            {
                return null;
            }
        }
    }
}