using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Contract.Models.ApigwModels
{
    public class HISDepartmentModel
    {
        public string Code {get;set;}
        public string ViName {get;set;}
        public string EnName {get;set;}
        public string HospitalCode {get;set;}
        public string DepartmentId { get; set; }
        public bool IsActivated { get; set; }
    }
}