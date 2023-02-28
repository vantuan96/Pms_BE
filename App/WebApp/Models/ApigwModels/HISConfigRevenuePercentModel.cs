using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrFee.Models.ApigwModels
{
    public class HISConfigRevenuePercentModel
    {
        public double ChargePercent { get; set; }
        public double OperationPercent { get; set; }
        public double ChargePackagePercent { get; set; }
        public double OperationPackagePercent { get; set; }
        public string HealthCheckDoctorService { get; set; }
    }
}