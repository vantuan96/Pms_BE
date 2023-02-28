using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.Enum
{
    public enum ActionEnum
    {
        /// <summary>
        /// Đăng ký gói
        /// </summary>
        REGISTERED = 1,
        /// <summary>
        /// Kích hoạt
        /// </summary>
        ACTIVATED = 2,
        /// <summary>
        /// Hủy gói
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
        /// Xác nhận chỉ định thuộc gói
        /// </summary>
        APPLYINPACKAGE = 8,
        /// <summary>
        /// Export bảng tình hình sử dụng gói
        /// </summary>
        EXPORTSTATUSING = 9,
        /// <summary>
        /// Export thống kê theo lượt khám
        /// </summary>
        EXPORTSTATVIAVISIT = 10,
        /// <summary>
        /// Tạo mới master data gói
        /// </summary>
        REGISTERED_PACKAGE = 11,
        /// <summary>
        /// Cập nhật/thay đổi master data gói
        /// </summary>
        UPDATED_PACKAGE = 12,
        /// <summary>
        /// Xóa master data gói
        /// </summary>
        DELETED_PACKAGE = 13,
        /// <summary>
        /// Gia hạn gói
        /// </summary>
        SCALEUP = 14,
        /// <summary>
        /// Mở lại gói
        /// </summary>
        REOPEN = 15,
        /// <summary>
        /// Cập nhật dịch vụ thay thế
        /// </summary>
        UPDATE_REPLACE_SERVICE = 16,
        /// <summary>
        /// Chuyển gói ghi nhận chỉ định
        /// </summary>
        TRANSFER_PACKAGE = 17,
        /// <summary>
        /// Chuyển theo dõi tái khám
        /// </summary>
        REEXAM = 18,
        /// <summary>
        /// Ghi nhận tái khám
        /// </summary>
        MAPPING_REEXAM = 19,
        /// <summary>
        /// Hủy theo dõi tái khám
        /// </summary>
        CANCEL_REEXAM = 20,
        /// <summary>
        /// Thêm/Sửa/Xóa nhóm gói
        /// </summary>
        CRUD_PACKAGE_GROUP = 21,
        /// <summary>
        /// Thêm/Sửa/Xóa thông tin con
        /// </summary>
        CRUD_CHILD_GROUP = 22,
    }
}
