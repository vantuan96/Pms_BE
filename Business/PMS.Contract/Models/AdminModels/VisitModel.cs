using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.AdminModels
{
    public class VisitModel
    {
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string VisitCode { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string VisitDate { get; set; }
    }
}
