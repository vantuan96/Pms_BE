using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrFee.Models.ApigwModels
{
    public class HISServiceModel
    {
        public string ServiceGroupCode { get; set; }
        public string ServiceGroupViName { get; set; }
        public string ServiceGroupEnName { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceViName { get; set; }
        public string ServiceEnName { get; set; }
        public int HISCode { get; set; }
    }
}