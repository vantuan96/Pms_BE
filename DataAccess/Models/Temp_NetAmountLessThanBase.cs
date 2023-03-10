using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    /// <summary>
    /// Bảng tạm dữ liệu gói dịch vụ. Phục vụ Migrate dữ liệu từ eHos
    /// </summary>
    public class Temp_NetAmountLessThanBase : IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string PackageCode { get; set; }
        [StringLength(150)]
        public string SiteApply { get; set; }
        public double? NetAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [StringLength(150)]
        public string BaseSiteCode { get; set; }
        [StringLength(150)]
        public string ChargeTypeCode { get; set; }
        [StringLength(1500)]
        public string Notes { get; set; }
    }
}
