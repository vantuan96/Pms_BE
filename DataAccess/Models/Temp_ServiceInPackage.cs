using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    /// <summary>
    /// Bảng tạm dữ liệu dịch vụ trong gói.Phục vụ Migrate dữ liệu từ eHos
    /// </summary>
    public class Temp_ServiceInPackage : IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string PackageCode { get; set; }
        [StringLength(450)]
        public string ServiceCode { get; set; }
        [StringLength(500)]
        public string ServiceName { get; set; }
        [StringLength(450)]
        public string ReplaceServiceCode { get; set; }
        public int QtyLimit { get; set; }
        /// <summary>
        /// Giá fix cho chính sách giá
        /// </summary>
        public double? Price { get; set; }
        [StringLength(500)]
        public string Notes { get; set; }
        public bool? IsPackageDrugConsum { get; set; }
    }
}
