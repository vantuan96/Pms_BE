using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Contract.Models.ApigwModels
{
    public class EmployeeInforModel
    {
        public string CostCentreName { get;set;}
        public string PersonId { get;set;}
        public string CostCentreId { get;set;}
        public string CostCentreCode { get;set;}
        public string AccountAD { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string EmployeeName { get; set; }
        public string CompanyCode { get; set; }
    }
}