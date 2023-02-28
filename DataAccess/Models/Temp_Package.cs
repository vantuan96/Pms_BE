using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    /// <summary>
    /// Bảng tạm dữ liệu gói dịch vụ. Phục vụ Migrate dữ liệu từ eHos
    /// </summary>
    public class Temp_Package : IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string PackgeCode { get; set; }
        [StringLength(550)]
        public string PackageName { get; set; }
        [StringLength(500)]
        public string PackageGroupCode { get; set; }
        [StringLength(150)]
        public string DrugConsum { get; set; }
        [StringLength(150)]
        public string SiteApply { get; set; }
        public double? PkgAmount { get; set; }
        public DateTime? StartDate { get; set; }
        [StringLength(150)]
        public string BaseSiteCode { get; set; }
        [StringLength(150)]
        public string ChargeTypeCode { get; set; }
        [StringLength(500)]
        public string Notes { get; set; }
    }
}
