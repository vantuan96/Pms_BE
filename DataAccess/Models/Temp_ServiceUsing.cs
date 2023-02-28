using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    /// <summary>
    /// Bảng tạm dữ liệu tình hình sử dụng dịch vụ trong gói.Phục vụ Migrate dữ liệu từ eHos
    /// </summary>
    public class Temp_ServiceUsing : IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string PID { get; set; }
        [StringLength(450)]
        public string PatientName { get; set; }
        [StringLength(500)]
        public string PackageCode { get; set; }
        [StringLength(500)]
        public string ServiceCode { get; set; }
        public int UsingNumber { get; set; }
        public Guid? ChargeId { get; set; }
        /// <summary>
        /// Giá lúc chỉ định
        /// </summary>
        public double? ChargePrice { get; set; }
        [StringLength(500)]
        public string Notes { get; set; }
        /// <summary>
        /// -1: Lỗi
        /// 0: Không cần xử lý
        /// 1: Cần xử lý
        /// 2: Đã lấy ra xử lý
        /// 3: Đã xử lý
        /// </summary>
        public int StatusForProcess { get; set; }
        /// <summary>
        /// Thời gian bắt đầu xử lý, null là xử lý ngay
        /// </summary>
        public DateTime? NextProcessTime { get; set; }
        /// <summary>
        /// Số lần đã xử lý
        /// </summary>
        public int ProcessNumber { get; set; }
    }
}
