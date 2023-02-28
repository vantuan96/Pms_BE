namespace DrFee.Common
{
    public class Message
    {
        public readonly static dynamic CSRF_MISSING = new { ViMessage = "Thiếu CSRF token", EnMessage = "CSRF token missing" };
        public readonly static dynamic EHOS_ACCOUNT_MISSING = new { ViMessage = "Chưa cập nhật tài khoản EHOS của bác sĩ", EnMessage = "EHOS account is missing" };
        public readonly static dynamic FORBIDDEN = new { ViMessage = "Bạn không có quyền truy cập", EnMessage = "You do NOT have permission to access" };
        public readonly static dynamic FORMAT_INVALID = new { ViMessage = "Dữ liệu sai định dạng", EnMessage = "Format is NOT correct" };
        public readonly static dynamic INTERAL_SERVER_ERROR = new { ViMessage = "Có lỗi xảy ra", EnMessage = "Internal server error" };
        public readonly static dynamic LOGIN_ERROR = new { ViMessage = "Thông tin đăng nhập chưa đúng", EnMessage = "Login information is incorrect" };
        public readonly static dynamic NOT_FOUND = new { ViMessage = "Không tìm thấy thông tin", EnMessage = "Not found" };
        public readonly static dynamic EXIST = new { ViMessage = "Đã tồn tại", EnMessage = "Existed" };
        public readonly static dynamic OWNER_FORBIDDEN = new { ViMessage = "Bạn không có quyền chỉnh sửa", EnMessage = "You do NOT have permission to update" };
        public readonly static dynamic UNAUTHORIZED = new { ViMessage = "Bạn chưa đăng nhập", EnMessage = "Please login" };
        public readonly static dynamic USER_NOT_FOUND = new { ViMessage = "Tên đăng nhập không tồn tại", EnMessage = "User is NOT found" };
        public readonly static dynamic USER_EXIST = new { ViMessage = "Tên đăng nhập đã tồn tại", EnMessage = "User is exist" };
        public readonly static dynamic SUCCESS = new { ViMessage = "Thành công", EnMessage = "Success" };
    }
}