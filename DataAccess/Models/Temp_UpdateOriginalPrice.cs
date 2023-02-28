using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    /// <summary>
    /// Bảng tạm dữ liệu khách hàng đăng ký gói dịch vụ. Phục vụ Migrate dữ liệu từ eHos
    /// </summary>
    public class Temp_UpdateOriginalPrice : IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string PID { get; set; }
        [StringLength(500)]
        public string PackageCode { get; set; }
        [StringLength(150)]
        public string ServiceCode { get; set; }
        /// <summary>
        /// Giá gốc của gói
        /// </summary>
        public int? OrginalPrice { get; set; }
        public bool IsUpdateOH { get; set; }
        /// <summary>
        /// -1: Lỗi
        /// 0: Không cần xử lý
        /// 1: Cần xử lý
        /// 2: Đã lấy ra xử lý
        /// 3: Đã xử lý
        /// </summary>
        public int StatusForProcess { get; set; }
        [StringLength(500)]
        public string Notes { get; set; }
    }
}
