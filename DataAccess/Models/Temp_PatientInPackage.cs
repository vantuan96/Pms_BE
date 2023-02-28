using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    /// <summary>
    /// Bảng tạm dữ liệu khách hàng đăng ký gói dịch vụ. Phục vụ Migrate dữ liệu từ eHos
    /// </summary>
    public class Temp_PatientInPackage:IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string PID { get; set; }
        [StringLength(450)]
        public string PatientName { get; set; }
        [StringLength(500)]
        public string PackageCode { get; set; }
        [StringLength(150)]
        public string SiteRegCode { get; set; }
        ///// <summary>
        ///// Số lần đăng ký
        ///// </summary>
        //public int RegCount { get; set; }
        ///// <summary>
        ///// Số gói đang hoạt động
        ///// </summary>
        //public int UsingCount { get; set; }
        ///// <summary>
        ///// Số gói ở trạng thái đăng ký
        ///// </summary>
        //public int RegistedCount { get; set; }
        ///// <summary>
        ///// Số lượng thời gian sử dụng/ lần đăng ký
        ///// </summary>
        //public int QtyTimeUsing { get; set; }
        ///// <summary>
        ///// Đơn vị tính thời gian sử dụng
        ///// </summary>
        //[StringLength(150)]
        //public string UnitTimeUsing { get; set; }
        /// <summary>
        /// Ngày bắt đầu có hiệu lực sử dụng
        /// </summary>
        public DateTime StartAt { get; set; }
        /// <summary>
        /// Ngày hết hiệu lực
        /// </summary>
        public DateTime EndAt { get; set; }
        /// <summary>
        /// Giá gốc của gói
        /// </summary>
        public double? Amount { get; set; }
        /// <summary>
        /// Giá sau giảm giá cho KH
        /// </summary>
        public double? NetAmount { get; set; }
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
