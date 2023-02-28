using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Contract.Models.ApigwModels
{
    public class HISServiceModel
    {
        public Guid? ServiceId { get; set; }
        public string ServiceType { get; set; }
        public string ServiceGroupCode { get; set; }
        public string ServiceGroupViName { get; set; }
        public string ServiceGroupEnName { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceViName { get; set; }
        public string ServiceEnName { get; set; }
        public bool IsActive { get; set; }
    }
}