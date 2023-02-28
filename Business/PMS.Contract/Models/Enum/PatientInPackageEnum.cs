using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.Enum
{
    public enum PatientInPackageEnum
    {
        /// <summary>
        /// Đăng ký
        /// </summary>
        REGISTERED = 1,
        /// <summary>
        /// Kích hoạt
        /// </summary>
        ACTIVATED = 2,
        /// <summary>
        /// HỦY
        /// </summary>
        CANCELLED = 3,
        /// <summary>
        /// Hủy ngang
        /// </summary>
        TERMINATED = 4,
        /// <summary>
        /// Nâng cấp gói
        /// </summary>
        TRANSFERRED = 5,
        /// <summary>
        /// Hết hạn
        /// </summary>
        EXPIRED = 6,
        /// <summary>
        /// Đóng/Tất toán gói
        /// </summary>
        CLOSED = 7,
        /// <summary>
        /// linhht Theo dõi tái khám
        /// </summary>
        RE_EXAMINATE = 8
    }
    public enum InPackageType
    {
        /// <summary>
        /// Charge move (Move từ chỉ định gói sang chỉ định lẻ)
        /// </summary>
        CHARGE_MOVEOUTPACKGE = -3,
        /// <summary>
        /// Invoice was cancelled (Hủy bảng kê - thanh toán)
        /// </summary>
        INVOICE_CANCELLED = -2,
        /// <summary>
        /// Charge was cancelled (Hủy chỉ định)
        /// </summary>
        CHARGE_CANCELLED = -1,
        /// <summary>
        /// Trong gói
        /// </summary>
        INPACKAGE = 1,
        /// <summary>
        /// Vượt gói
        /// </summary>
        OVERPACKAGE = 2,
        /// <summary>
        /// Ngoài gói
        /// </summary>
        OUTSIDEPACKAGE = 3,
        /// <summary>
        /// Số lượng dịch vụ chỉ định trong 1 chỉ định lớn hơn định mức còn lại
        /// </summary>
        QTYINCHARGEGREATTHANREMAIN = 4
    }
}
