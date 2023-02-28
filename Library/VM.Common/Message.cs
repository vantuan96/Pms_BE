using System;
using System.Collections.Generic;

namespace VM.Common
{
    public class Message
    {
        public readonly static MessageModel CSRF_MISSING = new MessageModel { ViMessage = "Thiếu CSRF token", EnMessage = "CSRF token missing" };
        public readonly static MessageModel EHOS_ACCOUNT_MISSING = new MessageModel { ViMessage = "Chưa cập nhật tài khoản EHOS của bác sĩ", EnMessage = "EHOS account is missing" };
        public readonly static MessageModel FORBIDDEN = new MessageModel { ViMessage = "Bạn không có quyền truy cập", EnMessage = "You do NOT have permission to access" };
        public readonly static MessageModel FORMAT_INVALID = new MessageModel { ViMessage = "Dữ liệu sai định dạng", EnMessage = "Format is NOT correct" };
        public readonly static MessageModel FORMAT_CODE_INVALID = new MessageModel { ViMessage = "Mã code sai định dạng", EnMessage = "Format code is NOT correct" };
        public readonly static MessageModel FORMAT_NAME_INVALID = new MessageModel { ViMessage = "Tên không được chứa các ký tự đặc biệt", EnMessage = "Format Name is NOT correct" };
        public readonly static MessageModel REQUIRE = new MessageModel { ViMessage = "Một số trường dữ liệu đang để trống", EnMessage = "Some field is require" };
        public readonly static MessageModel CODE_DUPLICATE = new MessageModel { ViMessage = "Mã code trùng", EnMessage = "Code data is duplicate" };
        public readonly static MessageModel INTERAL_SERVER_ERROR = new MessageModel { ViMessage = "Có lỗi xảy ra", EnMessage = "Internal server error" };
        public readonly static MessageModel LOGIN_ERROR = new MessageModel { ViMessage = "Thông tin đăng nhập chưa đúng", EnMessage = "Login information is incorrect" };
        public readonly static MessageModel NOT_FOUND = new MessageModel { ViMessage = "Không tìm thấy thông tin", EnMessage = "Not found" };
        public readonly static MessageModel DATA_NOT_FOUND = new MessageModel { ViMessage = "Không tìm thấy dữ liệu", EnMessage = "Data not found" };
        public readonly static MessageModel NOT_FOUND_POLICY = new MessageModel { ViMessage = "Chính sách giá không tồn tại", EnMessage = "The policy price not found" };
        public readonly static MessageModel NOT_FOUND_POLICY_DETAIL = new MessageModel { ViMessage = "Chưa thiết lập giá chi tiết dịch vụ trong gói cho chính sách này", EnMessage = "detail price policy have'nt setted" };
        public readonly static MessageModel NOT_FOUND_PACKAGE = new MessageModel { ViMessage = "Gói dịch vụ không tồn tại", EnMessage = "The package not found" };
        public readonly static MessageModel NOT_FOUND_SERVICE = new MessageModel { ViMessage = "Không tìm thấy dịch vụ", EnMessage = "The service not found" };
        public readonly static MessageModel NOT_FOUND_SERVICEINPACAKGE = new MessageModel { ViMessage = "Không tìm thấy dịch vụ trong gói", EnMessage = "The service root in package not found" };
        public readonly static MessageModel NOT_FOUND_PACKAGE_OLD = new MessageModel { ViMessage = "Gói dịch vụ cũ không tồn tại", EnMessage = "The old package not found" };
        public readonly static MessageModel NOT_FOUND_PACKAGEGROUP = new MessageModel { ViMessage = "Nhóm gói dịch vụ không tồn tại", EnMessage = "The group package not found" };
        public readonly static MessageModel NOT_FOUND_PACKAGEGROUP_OLD = new MessageModel { ViMessage = "Nhóm gói dịch vụ cũ không tồn tại", EnMessage = "The old group package not found" };
        public readonly static MessageModel NOT_FOUND_PATIENT = new MessageModel { ViMessage = "Không tìm thấy thông tin khách hàng này", EnMessage = "Patient is not found" };
        public readonly static MessageModel SERVICE_ISROOT_NOTALLOW_REPLACE = new MessageModel { ViMessage = "Là dịch vụ gốc, không được phép sử dụng để thay thế", EnMessage = "The service is root in package" };
        public readonly static MessageModel SERVICE_REPLACED_OTHERSERVICE = new MessageModel { ViMessage = "Đã được sử dụng để thay thế cho dịch vụ khác", EnMessage = "Was used to replace other service" };
        public readonly static MessageModel EXIST = new MessageModel { ViMessage = "Đã tồn tại", EnMessage = "Existed" };
        public readonly static MessageModel OWNER_FORBIDDEN = new MessageModel { ViMessage = "Bạn không có quyền chỉnh sửa", EnMessage = "You do NOT have permission to update" };
        public readonly static MessageModel CREATE_UPDATE_USER_OVERROLE = new MessageModel { ViMessage = "Bạn đang thêm hoặc cập nhật người dùng có nhóm quyền vượt quyền hạn của mình", EnMessage = "Over role to modify" };
        public readonly static MessageModel UNAUTHORIZED = new MessageModel { ViMessage = "Bạn chưa đăng nhập", EnMessage = "Please login" };
        public readonly static MessageModel USER_NOT_FOUND = new MessageModel { ViMessage = "Tài khoản AD không tồn tại", EnMessage = "User AD is NOT found" };
        public readonly static MessageModel USER_EXIST = new MessageModel { ViMessage = "Tên đăng nhập đã tồn tại", EnMessage = "User is exist" };
        public readonly static MessageModel SUCCESS = new MessageModel { ViMessage = "Thành công", EnMessage = "Success" };
        public readonly static MessageModel FAIL = new MessageModel { ViMessage = "Lỗi", EnMessage = "Fail" };
        public readonly static MessageModel OH_USER_NOT_FOUND = new MessageModel { ViMessage = "Tài khoản của bạn chưa được phân quyền trên OH", EnMessage = "Your Account is not exist on OH" };
        public readonly static MessageModel TOTAL_AMOUNT_INPACKAGE_NOTEQUAL_VN = new MessageModel { ViMessage = "Chính sách đối với người Việt: Tổng tiền trong gói và giá gói không bằng nhau!", EnMessage = "Policy for Vietnamese: Total amount in package is not equal package's price!" };
        public readonly static MessageModel TOTAL_AMOUNT_INPACKAGE_NOTEQUAL_FN = new MessageModel { ViMessage = "Chính sách đối với người Nước Ngoài: Tổng tiền trong gói và giá gói không bằng nhau!", EnMessage = "Policy for Foreign: Total amount in package is not equal package's price!" };
        public readonly static MessageModel LIMITDRUGCONSUMAMOUNT_GREATER_THAN_AMOUNTPACAKGE = new MessageModel { ViMessage = "Giới hạn tiền thuốc/VTTH lớn hơn giá gói!", EnMessage = "Limit amount drug and consum is greater than amount in package!" };
        public readonly static MessageModel TOTAL_AMOUNT_DRUG_CONSUM_GREATER_THAN_NETAMOUNT = new MessageModel { ViMessage = "Giá gói sau giảm giá nhỏ hơn tổng tiền thuốc và VTTH trong gói. Vui lòng nhập lại Mức giảm giá, chiết khấu.", EnMessage = "NetAmount is smaller than drug and consum total amount!" };
        public readonly static MessageModel TOTAL_AMOUNT_SERVICE_NOT_EQUAL_NETAMOUNT = new MessageModel { ViMessage = "Giá gói (Sau giảm giá chiết khấu) khác với tổng thành tiền trong gói. Vui lòng kiểm tra lại!", EnMessage = "NetAmount is not equal total amount service!" };
        public readonly static MessageModel LIMITDRUGCONSUMAMOUNT_INVALID = new MessageModel { ViMessage = "Giới hạn tiền thuốc/VTTH không hợp lệ!", EnMessage = "Limit amount drug and consum is invalid!" };
        public readonly static MessageModel DUPLICATE_SITE = new MessageModel { ViMessage = "Trùng cơ sở (Bệnh viện)", EnMessage = "The site is duplicate" };
        public readonly static MessageModel DUPLICATE_SITE_TIME = new MessageModel { ViMessage = "Cơ sở (Bệnh viện) đã tồn tại chính sách giá của gói trong khoảng thời gian này. Vui lòng kiểm tra lại!", EnMessage = "This package have policy for site at the same time. Try again!" };
        public readonly static MessageModel OVERLAP_RANGETIME = new MessageModel { ViMessage = "Khách hàng đã được đăng ký trong khung thời gian này. Vui lòng kiểm tra lại", EnMessage = "The registration is duplicate" };
        public readonly static MessageModel LABEL_OUTSIDEPACKAGE_SERVICE = new MessageModel { ViMessage = "Dịch vụ ngoài gói", EnMessage = "Service outside package" };
        public readonly static MessageModel SETTING_PACKAGE_SERVICE_DUPLICATE_SERVICE = new MessageModel { ViMessage = "Danh sách dịch vụ trong gói có ít nhất 1 dịch vụ (hoặc dịch vụ thay thế) đang trùng nhau", EnMessage = "Have at least 1 service (or service replacement) that is overlapping " };
        public readonly static MessageModel SETTING_PACKAGE_SERVICE_NO_HAVE_SERVICE = new MessageModel { ViMessage = "Vui lòng chọn ít nhất 1 dịch vụ trong gói", EnMessage = "Please select at least 1 service from the package" };
        public readonly static MessageModel STARTDATE_EARLER_CONTRACTDATE = new MessageModel { ViMessage = "Ngày bắt đầu sử dụng sớm hơn ngày hợp đồng", EnMessage = "Startdate patient in package using is earlier than contract date" };
        public readonly static MessageModel CONTRACTDATE_EARLER_ACTIVEDATE_POLICY_SITE = new MessageModel { ViMessage = "Ngày hợp đồng sớm hơn ngày gán chính sách giá cho site", EnMessage = "Contract date is earlier than site's price policy active date" };
        public readonly static MessageModel NETAMOUNT_VALUE_SMALLERTHANDRUGNCONSUM = new MessageModel { ViMessage = "Giá gói sau giảm giá nhỏ hơn tổng tiền thuốc và VTTH trong gói. Vui lòng nhập lại Mức giảm giá, chiết khấu.", EnMessage = "The price of the packages after the discount is smaller than the total cost of drugs and consum in the packages. Please re-enter the Discount, Discount Level." };
        //tungdd14 thêm message import
        public readonly static MessageModel NOT_FOUND_SITE = new MessageModel { ViMessage = "Phạm vi áp dụng tồn tại", EnMessage = "Site not found" };
        public readonly static MessageModel NOT_FOUND_PACKAGE_PRICE = new MessageModel { ViMessage = "Không có chính sách giá áp dụng cho site", EnMessage = "No pricing policy applies to the site" };
        public readonly static MessageModel NOTE_FAIL = new MessageModel { ViMessage = "Thất bại", EnMessage = "Fail" };
        public readonly static MessageModel PACKAGE_CODE_REQUIRED = new MessageModel { ViMessage = "Thiếu thông tin Mã gói", EnMessage = "The package Code column is required" };
        public readonly static MessageModel PACKAGE_NAME_REQUIRED = new MessageModel { ViMessage = "Thiếu thông tin Tên gói", EnMessage = "The package name column is required" };
        public readonly static MessageModel EXPIRSTION_DATE_REQUIRED = new MessageModel { ViMessage = "Thiếu thông tin Ngày hết hiệu lực", EnMessage = "The ExpirstionDate column is required" };
        public readonly static MessageModel EXPIRSTION_DATE_FAIL_FORMAT = new MessageModel { ViMessage = "Cột Ngày hết hiệu lực sai định dạng", EnMessage = "Column Expiration date wrong format" };
        public readonly static MessageModel ENDAT_GREATER_THAN_EXPIRSTION_DATE = new MessageModel { ViMessage = "Chính sách giá tại site có Ngày bắt đầu lớn hơn Ngày hết hiệu lực", EnMessage = "The pricing policy at the site has a Start Date greater than an Expiry Date" };
        public readonly static MessageModel PACKAGE_PRICE_EXPIRSTION = new MessageModel { ViMessage = "Chính sách giá đã hết hiệu lực", EnMessage = "Price policy has expired" };
        //tungdd14 message không cho hủy theo dõi tái khám
        public readonly static MessageModel CANCEL_REEXAM_WAS_USE = new MessageModel { ViMessage = "Khách hàng đã sử dụng dịch vụ tái khám, bạn không thể hủy theo dõi tái khám.", EnMessage = "The customer has used the follow-up service, you cannot unfollow the follow-up examination." };

        //linhht
        public readonly static MessageModel PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER = new MessageModel { ViMessage = "Ngày bắt đầu sử dụng sớm hơn ngày hợp đồng.", EnMessage = "Start date more than contract date." };
        public readonly static MessageModel PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER1 = new MessageModel { ViMessage = "Ngày hợp đồng không được lớn hơn ngày bắt đầu sử dụng gói.", EnMessage = "Start date more than contract date." };
        public readonly static MessageModel PATIENT_INPACKAGE_CONTRACTDATE_PRICE_SITE_CREATEDATE_INVALID_EARLIER = new MessageModel { ViMessage = "Ngày hợp đồng sớm hơn ngày active chính sách giá tại site.", EnMessage = "Contract date ealier than date package price site." };
        public readonly static MessageModel MSG38 = new MessageModel { ViMessage = "Chưa chỉ định hoặc ghi nhận dịch vụ tái khám vào gói.", EnMessage = "No follow-up service has been specified or recorded in the package." };
        public readonly static MessageModel PATIENT_INPACKAGE_NOT_MATERNITY_BUNLDE = new MessageModel { ViMessage = "Gói không thuộc nhóm dịch vụ được quyền thao tác.", EnMessage = "Package not has permission." };
        //---
        #region Message for create or update Package
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_CODE = new MessageModel { ViMessage = "Mã code không được phép thay đổi", EnMessage = "The code is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_TYPE_ISLIMITEDDRUGCONSUM = new MessageModel { ViMessage = "Loại gói định mức Thuốc và VTTH không được phép thay đổi", EnMessage = "The Drug and consumables type is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_STATUS = new MessageModel { ViMessage = "Trạng thái không được phép thay đổi", EnMessage = "The status is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_ROOTGROUP = new MessageModel { ViMessage = "Nhóm gói cấp 1 (Root) không được phép thay đổi", EnMessage = "The root group is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_ITEMSERVICE = new MessageModel { ViMessage = "Thiết lập dịch vụ trong gói không được phép thay đổi", EnMessage = "The services in package is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_ITEMDRUG_CONSUM = new MessageModel { ViMessage = "Thiết lập thuốc & VTTH trong gói không được phép thay đổi", EnMessage = "The drug and consumables in package is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_ITEMREPLACE = new MessageModel { ViMessage = "Thiết lập dịch vụ thay thế trong gói không được phép thay đổi", EnMessage = "The service replacing in package is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_REMOVE_ITEMREPLACE = new MessageModel { ViMessage = "Thiết lập dịch vụ thay thế trong gói không được phép xóa", EnMessage = "The service replacing in package is not allowed to remove" };
        #endregion .Message for create or update Package

        #region Message for create or update Policy price 
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_PRICEPOLICY = new MessageModel { ViMessage = "Đã có khách hàng đăng ký gói này. Một số thông tin không được phép thay đổi. Vui lòng kiểm tra lại!", EnMessage = "Some information is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_PRICEPOLICY_PERSONAL = new MessageModel { ViMessage = "Đã có khách hàng đăng ký gói này. Không được thay đổi thông tin đối tượng áp dụng.", EnMessage = "Have patient was registered this package. The personal policy is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_PRICEPOLICY_DETAIL = new MessageModel { ViMessage = "Đã có khách hàng đăng ký gói này. Không được thay đổi chi tiết giá gói.", EnMessage = "Have patient was registered this package.The detail policy is not allowed to modify" };
        public readonly static MessageModel NOTE_NOTALLOW_MODIFY_PRICEPOLICY_REMOVESITE = new MessageModel { ViMessage = "Bệnh viện đã áp dụng không được xóa", EnMessage = "The site was applied is not allowed to remove" };
        public readonly static MessageModel NOTE_MODIFY_PRICEPOLICY_ENDDATE_EARLIERTHAN = new MessageModel { ViMessage = "Ngày hết hiệu lực không được nhỏ hơn ngày cuối phát sinh khách hàng đăng ký gói", EnMessage = "The expire date is not earlier than the last register" };
        #endregion .Message for create or update Policy price
        #region Message for Cancel patient's package
        public readonly static MessageModel EXIST_CHARGE_INSIDE_PACKAGE = new MessageModel { ViMessage = "Đã có chỉ định được xác nhận thuộc gói.", EnMessage = "The exist charge inside this package." };
        public readonly static MessageModel EXIST_CHARGE_INSIDE_PACKAGE_INVOICED = new MessageModel { ViMessage = "Đã có chỉ định được thanh toán. Bạn không thể hủy gói này.", EnMessage = "The exist charge inside this package have been invoiced." };
        public readonly static MessageModel NOTEXIST_VISIT_PACKAGE_OPEN = new MessageModel { ViMessage = "Khách hàng đang không có lượt khám gói.", EnMessage = "Have no visit package was open on OH." };
        public readonly static MessageModel NOTEXIST_VISIT_OPEN = new MessageModel { ViMessage = "Khách hàng chưa được mở lượt khám trên OH.", EnMessage = "Have no visit open on OH." };
        public readonly static MessageModel UPDATEPRICE_OH_FAIL = new MessageModel { ViMessage = "Cập nhật giá lên OH không thành công.", EnMessage = "Update price on OH is fail." };
        public readonly static MessageModel UPDATEPRICE_OH_FAIL_CHARGEID_DONOT_EXIST = new MessageModel { ViMessage = "Không tồn tại charge này trên OH.", EnMessage = "Charge ID is not exist!" };
        #endregion .Message for Cancel patient's package

        #region Message for Transferred patient in package
        public readonly static MessageModel TRANSFERRED_EXIST_CHARGE_INSIDE_PACKAGE_INVOICED = new MessageModel { ViMessage = "Đã có chỉ định trong gói được xuất hóa đơn. Bạn cần hủy hóa đơn trước khi thực hiện nâng cấp.", EnMessage = "The exist charge inside this package have been invoiced. You should cancel invoice before transferring." };
        #endregion .Message for Transferred patient in package
        public readonly static dynamic CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER = new { MessageCode = MessageCode.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER, ViMessage = "Gói đang được cập nhật bởi một user khác. Vui lòng thử lại.", EnMessage = "The package is being updated by another user. Please try again." };
        public readonly static dynamic CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER_USER = new { MessageCode = MessageCode.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER, ViMessage = "Gói đang được cập nhật bởi một user khác. Vui lòng thử lại.", EnMessage = "The package is being updated by another user. Please try again." };
        public readonly static dynamic CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER_SESSION = new
        {
            MessageCode = MessageCode.CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER,
            ViMessage = "Tại thời điểm hiện tại gói này đang được xử lý bởi phiên khác.Vui lòng thử lại!",
            EnMessage = "In the current time, this package was appling by other session.Try again plz!"
        };
    }
    public class MessageManager
    {
        public static List<MessageModel> Messages = new List<MessageModel>() {
            new MessageModel{Code=MessageCode.NOTE_NONEW_POLICY, ViMessage = "Chưa thiết lập giá mới (áp dụng từ ngày {0})", EnMessage = "Have no new policy (apply from {0})" },
            new MessageModel{Code=MessageCode.NOTE_NEW_POLICY, ViMessage = "Áp dụng CS giá mới từ ngày {0}", EnMessage = "Apply new policy from {0}" },
            new MessageModel{Code=MessageCode.NOTE_CHARGE_BELONG_PACKAGE, ViMessage = "Đã được ghi nhận vào gói {0}", EnMessage = "Applied for package {0}" },
            new MessageModel{Code=MessageCode.NOTE_CHARGE_INSIDE_SERVICEREPLACE, ViMessage = "Dịch vụ thay thế cho dịch vụ chính {0}", EnMessage = "Is service replace for main service {0}" },
            new MessageModel{Code=MessageCode.NOTE_CHARGE_IS_UPDATE_PRICE, ViMessage = "Cập nhật giá dịch vụ trong gói", EnMessage = "Upgrade price service in package" },
            new MessageModel{Code=MessageCode.LABEL_OUTSIDEPACKAGE_SERVICE, ViMessage = "Dịch vụ ngoài gói", EnMessage = "Service outside package" },
            new MessageModel{Code=MessageCode.LABEL_INSIDEPACKAGE_GROUPTOTAL, ViMessage = "Trong gói {0} ({1})", EnMessage = "Inside package {0} (1)" },
            new MessageModel{Code=MessageCode.LABEL_OVERPACKAGE_GROUPTOTAL, ViMessage = "Vượt gói {0} ({1})", EnMessage = "Over package {0} (1)" },
            new MessageModel{Code=MessageCode.LABEL_OUTSIDEPACKAGE_GROUPTOTAL, ViMessage = "Dịch vụ ngoài gói", EnMessage = "Service outside package" },
            new MessageModel{Code=MessageCode.LABEL_RECEIVABLES, ViMessage = "Phải thu", EnMessage = "Receivables" },
            new MessageModel{Code=MessageCode.LABEL_TOTAL_AMOUNT, ViMessage = "Tổng tiền", EnMessage = "Total amount" },
            new MessageModel{Code=MessageCode.LABEL_WARNING_SITEAPPLY_ENDDATE_LESS_START, ViMessage = "Site {0} có ngày hết hiệu lực nhỏ hơn thời gian áp dụng của chính sách giá", EnMessage = "Site {0} has an expiration date that is less than the applicable time of the pricing policy" },
            new MessageModel{Code=MessageCode.OVERLAP_PACKAGE_WARNING, ViMessage = "KH đang được đăng ký gói {0} - {1} có hiệu lực từ ngày {2} đến ngày {3}", EnMessage = "Customers was registering for package {0} - {1} valid from {2} to {3}" },
            new MessageModel{Code=MessageCode.MSG31_TRANSFERRED_CONFIRM, ViMessage = "Giá của các dịch vụ đã sử dụng tại gói {0} - {1} có thể bị thay đổi và KH có thể phải trả thêm phí khi nâng cấp gói. Bạn có chắc chắc muốn Nâng cấp gói?", EnMessage = "The price of the services used in package {0} - {1} may be changed and the customer must pay an additional fee when upgrading the package. Are you sure you want to upgrade package?" },
            new MessageModel{Code=MessageCode.CHILD_DELETE_INSIDE_PACKAGE_WARNING_USED, ViMessage = "Bạn không thể xóa PID này. {0} (PID: {1}) đã sử dụng dịch vụ trong gói.", EnMessage = "You can't delete this PID. {0} (PID: {1}) was used service inside this package." },
            new MessageModel{Code=MessageCode.CHILD_BELONG_OTHERMOTHER, ViMessage = "PID {0} là con của Sản phụ {1} - {2}. Vui lòng chọn giá trị khác", EnMessage = "PID {0} belong mother {1} - {2}. Plz select other value" },
            new MessageModel{Code=MessageCode.NOTE_CHARGE_USING_BY, ViMessage = "Được sử dụng bởi {0}", EnMessage = "Using by {0}" },
            new MessageModel{Code=MessageCode.ITEM_NOT_SET_PRICE_ON_OH, ViMessage = "Không thể thiết lập giá cho gói này do có dịch vụ chưa được thiết lập giá trên OH ({0})", EnMessage = "Can not setup policy price because have some item is not set price on OH ({0})" },
            //linhht
            new MessageModel{Code=MessageCode.MSG21_OVER_POLICY, ViMessage = "Cơ sở khởi tạo {0} chưa được áp dụng chính sách giá gói này ngày {1}", EnMessage = "Policy price {0} isn't inited for contract date {1}" },
            //tungdd14 note tái khám
            new MessageModel{Code=MessageCode.NOTEREEXAM, ViMessage = "Tái khám", EnMessage = "ReExam" },
            new MessageModel{Code=MessageCode.LABEL_IS_DRUGCONSUM, ViMessage = "Thuốc & VTHH ngoài gói", EnMessage = "Is Drugconsum" },
            new MessageModel{Code=MessageCode.LABEL_DRUGCONSUM_GROUPTOTAL, ViMessage = "Thuốc & VTHH ngoài gói", EnMessage = "Is Drugconsum" },
    };
    }
    /// <summary>
    /// linhht thêm MessageCode
    /// </summary>
    public class MessageCode
    {
        public static string NOTE_NONEW_POLICY = "NOTE_NONEW_POLICY";
        public static string NOTE_NEW_POLICY = "NOTE_NEW_POLICY";
        public static string NOTE_CHARGE_BELONG_PACKAGE = "NOTE_CHARGE_BELONG_PACKAGE";
        public static string NOTE_CHARGE_INSIDE_SERVICEREPLACE = "NOTE_CHARGE_INSIDE_SERVICEREPLACE";
        public static string NOTE_CHARGE_IS_UPDATE_PRICE = "NOTE_CHARGE_IS_UPDATE_PRICE";
        public static string LABEL_OUTSIDEPACKAGE_SERVICE = "LABEL_OUTSIDEPACKAGE_SERVICE";
        public static string LABEL_INSIDEPACKAGE_GROUPTOTAL = "LABEL_INSIDEPACKAGE_GROUPTOTAL";
        public static string LABEL_OVERPACKAGE_GROUPTOTAL = "LABEL_OVERPACKAGE_GROUPTOTAL";
        public static string LABEL_OUTSIDEPACKAGE_GROUPTOTAL = "LABEL_OUTSIDEPACKAGE_GROUPTOTAL";
        public static string LABEL_RECEIVABLES = "LABEL_RECEIVABLES";
        public static string LABEL_TOTAL_AMOUNT = "LABEL_TOTAL_AMOUNT";
        public static string LABEL_WARNING_SITEAPPLY_ENDDATE_LESS_START = "LABEL_WARNING_SITEAPPLY_ENDDATE_LESS_START";
        public static string OVERLAP_PACKAGE_WARNING = "OVERLAP_PACKAGE_WARNING";
        public static string MSG31_TRANSFERRED_CONFIRM = "MSG31_TRANSFERRED_CONFIRM";
        public static string CHILD_DELETE_INSIDE_PACKAGE_WARNING_USED = "CHILD_DELETE_INSIDE_PACKAGE_WARNING_USED";
        public static string CHILD_BELONG_OTHERMOTHER = "CHILD_BELONG_OTHERMOTHER";
        public static string NOTE_CHARGE_USING_BY = "NOTE_CHARGE_USING_BY";
        public static string ITEM_NOT_SET_PRICE_ON_OH = "ITEM_NOT_SET_PRICE_ON_OH";
        public static string MSG21_OVER_POLICY = "MSG21_OVER_POLICY";
        public static string CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER = "CONFIRM_BELONG_PACKAGE_CURRENTPATIENT_INPACKAGE_ISOTHER";
        public static string PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER = "PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER";
        public static string PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER1 = "PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER1";
        public static string PATIENT_INPACKAGE_CONTRACTDATE_PRICE_SITE_CREATEDATE_INVALID_EARLIER = "PATIENT_INPACKAGE_CONTRACTDATE_PRICE_SITE_CREATEDATE_INVALID_EARLIER";
        public static string MSG38 = "MSG38";
        //tungdd14 note tái khám
        public static string NOTEREEXAM = "NOTEREEXAM";
        public static string LABEL_IS_DRUGCONSUM = "LABEL_IS_DRUGCONSUM";
        public static string LABEL_DRUGCONSUM_GROUPTOTAL = "LABEL_DRUGCONSUM_GROUPTOTAL";

    }

    public class MessageModel : ICloneable
    {
        public string Code { get; set; }
        public string ViMessage { get; set; }
        public string EnMessage { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class ImportExpirationDateNoteModel
    {
        public string PackageCode { get; set; }
        public string packageName { get; set; }
        public string site { get; set; }
        public MessageModel StatusName { get; set; }
        public List<MessageModel> ErrorMessage { get; set; }
    }
}