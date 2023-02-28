using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrFee.Models.ApigwModels
{
    public class ViHCDataModel
    {
        public string VisitCode { get; set; }
        public string ServiceCode { get; set; }
        public string DoctorAD { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}