using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VM.Common;

namespace PMS.Contract.Models.AdminModels
{
    public class PatientInPackageInfoModel
    {
        /// <summary>
        /// Id Patient In Package
        /// </summary>
        public Guid? Id { get; set; }
        [Required]
        public Guid? PolicyId { get; set; }
        public bool IsLimitedDrugConsum { get; set; }
        [Required]
        public Guid? SiteId { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
        public Guid? GroupPackageId { get; set; }
        public string GroupPackageCode { get; set; }
        public string GroupPackageName { get; set; }
        [Required]
        /// <summary>
        /// 1: Vietnamese
        /// 2: Foreign
        /// </summary>
        public int? PersonalType { get; set; }
        #region Contract Information
        [StringLength(50, ErrorMessage = "Số hợp đồng có độ dài tối đa là 50 ký tự")]
        public string ContractNo { get; set; }
        public string ContractDate { get; set; }
        public DateTime? GetContractDate()
        {
            if (string.IsNullOrEmpty(ContractDate))
                return null;
            try
            {
                return DateTime.ParseExact(ContractDate, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Nhân viên phụ trách hợp đồng
        /// </summary>
        public string ContractOwnerAd { get; set; }
        public string ContractOwnerFullName { get; set; }
        #endregion .Contract Information
        #region Doctor consult
        public string DoctorConsultAd { get; set; }
        public string DoctorConsultFullName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        #endregion .Doctor consult
        [Required]
        /// <summary>
        /// Ngày bắt đầu có hiệu lực sử dụng
        /// </summary>
        public string StartAt { get; set; }
        public DateTime GetStartAt()
        {
            if (string.IsNullOrEmpty(StartAt))
                return DateTime.Now;
            try
            {
                return DateTime.ParseExact(StartAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return DateTime.Now;
            }
        }
        /// <summary>
        /// Ngày hết hạn sử dụng
        /// </summary>
        public string EndAt { get; set; }
        public DateTime? GetEndAt()
        {
            if (string.IsNullOrEmpty(EndAt))
                return null;
            try
            {
                return DateTime.ParseExact(EndAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
        public DateTime? GetEndFullDate()
        {
            if (string.IsNullOrEmpty(EndAt))
                return null;
            try
            {
                var endDate = DateTime.ParseExact(EndAt, Constant.DATE_FORMAT, null);
                return endDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Là gói thai sản
        /// </summary>
        public bool IsMaternityPackage
        {
            get
            {
                if (Constant.ListGroupCodeIsMaternityPackage.Contains(this.GroupPackageCode))
                    return true;
                return false;
            }
        }
        /// <summary>
        /// linhht là gói bundle payment
        /// </summary>
        public bool IsBundlePackage
        {
            get
            {
                if (Constant.ListGroupCodeIsBundlePackage.Contains(this.GroupPackageCode))
                    return true;
                return false;
            }
        }
        public bool IsIncludeChild
        {
            get
            {
                if (Constant.ListGroupCodeIsIncludeChildPackage.Contains(this.GroupPackageCode))
                    return true;
                return false;
            }
        }
        /// <summary>
        /// tungdd14 kiểm tra gói khách hàng đăng ký có dịch vụ được cấu hình tái khảm không
        /// </summary>
        public bool hasReExamService { get; set; }
        /// <summary>
        /// tungdd14 trạng thái Hiện thị với các gói có trạng thái “Theo dõi tái khám” hoặc “Hết hạn” nhưng trước đó đã được chuyển sang trạng thái “Theo dõi tái khám”
        /// </summary>
        public bool IsPackageReExam { get; set; }
        public List<PatientInformationModel> Children { get; set; }
        /// <summary>
        /// Ngày dự sinh
        /// </summary>
        public string EstimateBornDate { get; set; }
        public DateTime? GetEstimateBornDate()
        {
            if (string.IsNullOrEmpty(EstimateBornDate))
                return null;
            try
            {
                return DateTime.ParseExact(EstimateBornDate, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
        //Package Amount
        public double? Amount { get; set; }
        #region Discount Info
        /// <summary>
        /// Là TH giảm giá - chiết khấu
        /// </summary>
        public bool IsDiscount { get; set; }
        /// <summary>
        /// 1: Chiết khấu theo %
        /// 2: Chiết khấu theo VNĐ
        /// </summary>
        public int? DiscountType { get; set; }
        /// <summary>
        /// % hoặc số tiền chiết khấu
        /// </summary>
        public double? DiscountAmount { get; set; }
        /// <summary>
        /// Giá sau chiết khâu (Nếu có)
        /// </summary>
        public double NetAmount { get; set; }
        /// <summary>
        /// Lý do giảm giá/chiết khấu
        /// </summary>
        public string DiscountNote { get; set; }
        #endregion .Discount Info
        /// <summary>
        /// 1: Registered
        /// 2: Activated
        /// 3: Cancelled
        /// 4: Terminated
        /// 5: Transferred
        /// 6: Expired
        /// 7: Closed
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// tungdd14: thêm trường check trạng thái trước đó
        /// </summary>
        public int? LastStatus { get; set; }
        #region Time log Action update 
        public DateTime? CreatedAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime? TerminatedAt { get; set; }
        /// <summary>
        /// Các thông tin liên quan đến Nâng cấp gói
        /// </summary>
        #region Tranfer info
        public DateTime? TransferredAt { get; set; }
        public Guid? NewPatientInPackageId { get; set; }
        public Guid? PackageTranferId { get; set; }
        public string PackageTranferCode { get; set; }
        public string PackageTranferName { get; set; }
        #endregion . Tranfer info
        #region Tranfer info from
        public DateTime? TransferredFromAt { get; set; }
        public Guid? OldPatientInPackageId { get; set; }
        public Guid? FromPackageId { get; set; }
        public string FromPackageCode { get; set; }
        public string FromPackageName { get; set; }
        #endregion . Tranfer info
        #endregion .Time log Action update 
        [Required]
        public PatientInformationModel PatientModel { get; set; }
    }
    public class PatientInPackageModel : PatientInPackageInfoModel
    {
        public List<PatientInPackageDetailModel> Services { get; set; }

    }
    /// <summary>
    /// Class model for Transferred package
    /// </summary>
    public class PatientInPackageTransferredModel : PatientInPackageModel
    {
        public Guid? SessionProcessId { get; set; }
        [Required]
        /// <summary>
        /// Id của gói dịch vụ cũ của khách hàng  (Gói được được nâng cấp)
        /// </summary>
        public Guid? OldPatientInPackageId { get; set; }
        /// <summary>
        /// Code của gói dịch vụ cũ
        /// </summary>
        public string OldPackageCode { get; set; }
        /// <summary>
        /// Name của gói dịch vụ cũ
        /// </summary>
        public string OldPackageName { get; set; }
        /// <summary>
        /// Nguyên giá của gói dịch vụ cũ
        /// </summary>
        public double? OldPackageOriginalAmount { get; set; }
        /// <summary>
        /// giá sau giảm giá/chiết khấu
        /// </summary>
        public double? OldPackageNetAmount { get; set; }
        #region Số tiền phải thu khách hàng
        /// <summary>
        /// Số tiền phải thu khách hàng
        /// </summary>
        public double? ReceivableAmount { get; set; }
        /// <summary>
        /// Công nợ
        /// </summary>
        public double? DebitAmount { get; set; }
        /// <summary>
        /// Phí dịch vụ vượt/ ngoài gói
        /// </summary>
        public double? Over_OutSidePackageFee { get; set; }
        #endregion .Số tiền phải thu khách hàng
        /// <summary>
        /// Danh sách chỉ định xác nhận thuộc gói
        /// </summary>
        public List<ChargeInPackageModel> listCharge { get; set; }
    }
    public class PatientInformationModel
    {
        public Guid? Id { get; set; }
        /// <summary>
        /// ID trên OH
        /// </summary>
        public Guid? PatientId { get; set; }
        [Required]
        public string PID { get; set; }
        public string FullName { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? DateOfBirth { get; set; }
        public string Age
        {
            get
            {
                if (DateOfBirth.HasValue)
                {
                    return (DateTime.Now.Year - DateOfBirth.Value.Year).ToString();
                }
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// 0: Không xác định
        /// 1: Nam
        /// 2: Nữ
        /// </summary>
        public int? Gender { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string National { get; set; }
        public static int GenderFromText(string genderText)
        {
            int returnValue = 0;
            List<string> listMale = new List<string>() { "M", "T" };
            List<string> listFaMale = new List<string>() { "F", "N" };
            if (listMale.Contains(genderText))
            {
                returnValue = 1;
            }
            else if (listFaMale.Contains(genderText))
            {
                returnValue = 2;
            }
            return returnValue;
        }
    }
    public class PatientInPackageDetailModel
    {
        public Guid? Id { get; set; }
        public List<PatientInPackageDetailModel> ModelReplaces { get; set; }
        public Guid? ServiceInPackageId { get; set; }
        public Guid? ServiceInPackageRootId { get; set; }
        public Service Service { get; set; }
        public List<Service> ItemsReplace { get; set; }
        /// <summary>
        /// Số lượng định mức
        /// </summary>
        public int? Qty { get; set; }
        /// <summary>
        /// Số lượng đã dùng
        /// </summary>
        public int? QtyWasUsed { get; set; }
        /// <summary>
        /// linhht số lượng tái khám đã dùng
        /// </summary>
        public int? QtyReExaminated { get; set; }
        /// <summary>
        /// Đơn giá cơ sở
        /// </summary>
        public double? BasePrice { get; set; }

        /// <summary>
        /// Thành tiền cơ sơ
        /// </summary>
        public double? BaseAmount { get; set; }

        /// <summary>
        /// Đơn giá trong gói
        /// </summary>
        public double? PkgPrice { get; set; }

        /// <summary>
        /// Thành tiền trong gói
        /// </summary>
        public double? PkgAmount { get; set; }
        /// <summary>
        /// Là gói Thuốc/VTTH
        public bool IsPackageDrugConsum { get; set; }
        /// <summary>
        /// Loại dịch vụ:
        /// 0: Total/Tổng
        /// 1: Dịch vụ
        /// 2: Thuốc & VTTH
        /// </summary>
        public int ServiceType { get; set; }
        /// <summary>
        /// Miễn phí trong gói
        /// </summary>
        public bool IsServiceFreeInPackage { get; set; }
    }

    public class PatientInPackageServiceUsingStatusModel
    {
        public Guid Id { get; set; }
        public Guid? ServiceInPackageId { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        //public List<Service> ItemsReplace { get; set; }
        /// <summary>
        /// Số lượng định mức
        /// </summary>
        public int? Qty { get; set; }
        /// <summary>
        /// Đơn giá trong gói
        /// </summary>
        public double? PkgPrice { get; set; }
        #region Thông tin đã sử dụng
        /// <summary>
        /// Số lượng đã dùng
        /// </summary>
        public int? QtyWasUsed { get; set; }
        /// <summary>
        /// Tungdd14 hạn mức tái khám
        /// </summary>
        public int? ReExamQtyLimit { get; set; }
        /// <summary>
        /// linhht số lượng đã dùng tái khám
        /// </summary>
        public int? QtyReExamWasUsed { get; set; }
        /// <summary>
        /// Số lượng đã dùng
        /// </summary>
        public int? QtyWasInvoiced { get; set; }
        /// <summary>
        /// tungdd14 số lượng đã dùng tái khám
        /// </summary>
        public int? QtyReExamWasInvoiced { get; set; }
        /// <summary>
        /// Thành tiền đã sử dụng
        /// </summary>
        public double? AmountWasUsed { get; set; }
        #endregion .Thông tin đã sử dụng
        #region Thông tin chưa sử dụng
        /// <summary>
        /// Số lượng chưa sử dụng
        /// </summary>
        public int? QtyNotUsedYet { get; set; }
        /// <summary>
        /// linhht số lượng chưa dùng tái khám
        /// </summary>
        public int? QtyReExamNotUsed { get; set; }
        /// <summary>
        /// Thành tiền chưa sử dụng
        /// </summary>
        public double? AmountNotUsedYet { get; set; }
        #endregion .Thông tin chưa sử dụng
        #region Thông tin vượt gói
        /// <summary>
        /// Số lượng vượt gói
        /// </summary>
        public int? QtyOver { get; set; }
        /// <summary>
        /// tungdd14 Số lượng tái khám vượt gói
        /// </summary>
        public int? QtyReExamOver { get; set; }
        #endregion .Thông tin vượt gói
        /// <summary>
        /// Là gói Thuốc/VTTH
        public bool IsPackageDrugConsum { get; set; }
        /// <summary>
        /// Loại dịch vụ:
        /// 0: Total/Tổng
        /// 1: Dịch vụ
        /// 2: Thuốc & VTTH
        /// </summary>
        public int ServiceType { get; set; }
        //public DateTime? ServiceInPackageCreatedAt { get; set; }
        /// <summary>
        /// Service Replace/ Dịch vụ thay thế
        /// </summary>
        public dynamic ItemsReplace { get; set; }
        /// <summary>
        /// tungdd14 Là dịch vụ tái khám (Follow up)
        /// </summary>
        public bool IsReExamService { get; set; }
    }
    public class PatientInPackageVisitModel
    {
        public Guid? PatientId { get; set; }
        public Guid? PatientInPackageId { get; set; }
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string VisitCode { get; set; }
        public string VisitDate { get; set; }
        public string VisitClosedDate { get; set; }
        public string VisitType { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public int Status { get; set; }
    }
    /// <summary>
    /// linhht model cho chỉnh sửa hợp đồng, gia hạn gói
    /// </summary>
    public class PatientInPackageUpdateModel
    {
        public Guid PatientId { get; set; }
        public Guid Id { get; set; }
        public string PackageCode { get; set; }
        public string ContractDate { get; set; }
        public DateTime? GetContractDate()
        {
            if (string.IsNullOrEmpty(ContractDate))
                return null;
            try
            {
                return DateTime.ParseExact(ContractDate, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
        public string StartAt { get; set; }
        public DateTime GetStartAt()
        {
            if (string.IsNullOrEmpty(StartAt))
                return DateTime.Now;
            try
            {
                return DateTime.ParseExact(StartAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return DateTime.Now;
            }
        }
        public DateTime? GetStartFullDate()
        {
            if (string.IsNullOrEmpty(StartAt))
                return null;
            try
            {
                var endDate = DateTime.ParseExact(StartAt, Constant.DATE_FORMAT, null);
                return endDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            catch
            {
                return null;
            }
        }
        public string EndAt { get; set; }
        public DateTime GetEndAt()
        {
            if (string.IsNullOrEmpty(EndAt))
                return DateTime.Now;
            try
            {
                return DateTime.ParseExact(EndAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return DateTime.Now;
            }
        }
        public DateTime? GetEndFullDate()
        {
            if (string.IsNullOrEmpty(EndAt))
                return null;
            try
            {
                var endDate = DateTime.ParseExact(EndAt, Constant.DATE_FORMAT, null);
                return endDate.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            catch
            {
                return null;
            }
        }
        [StringLength(50, ErrorMessage = "Số hợp đồng có độ dài tối đa là 50 ký tự")]
        public string ContractNo { get; set; }
        public string ContractOwnerAd { get; set; }
        public string ContractOwnerFullName { get; set; }
    }
    /// <summary>
    /// linhht model mở lại gói
    /// </summary>
    public class PatientInPackageReopenModel
    {
        public Guid PatientId { get; set; }
        public Guid Id { get; set; }
    }
    /// <summary>
    /// linhht model Lưu dịch vụ tái khám
    /// </summary>
    public class PatientInPackageReExaminateModel
    {
        public string PackageCode { get; set; }
        public Guid Id { get; set; }//patientinpackage
        public List<PatientInPackageServiceUsingStatusModel> SelectedServices { get; set; }
    }
    #region Charge refer
    public class ConfirmServiceInPackageModel
    {
        public Guid? SessionProcessId { get; set; }
        public Guid PatientInPackageId { get; set; }
        /// <summary>
        /// Ngày gói hết hiệu lực
        /// </summary>
        public DateTime? EndDate { get; set; }
        public DateTime? EndDateFull
        {
            get
            {
                if (this.EndDate != null)
                    return this.EndDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                else
                    return (DateTime?)null;
            }
        }
        public Guid PolicyId { get; set; }
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string GroupPackageCode { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
        /// <summary>
        /// Thuốc & VTTH: Không định mức/ Theo định mức
        /// 0: Unlimited
        /// 1: Limited
        /// </summary>
        public bool IsLimitedDrugConsum { get; set; }
        public bool IsMaternityPackage
        {
            get; set;
        }
        /// <summary>
        /// linhht bundle payment
        /// </summary>
        public bool IsBundlePackage
        {
            get; set;
        }
        public bool IsIncludeChild { get; set; }
        //public string VisitCode { get; set; }
        //public string SiteCode { get; set; }
        //public string SiteName { get; set; }
        //public string VisitDate { get; set; 
        /// <summary>
        /// Danh sách lượt khám
        /// </summary>
        public List<VisitModel> Visits { get; set; }
        public List<PatientInformationModel> Children { get; set; }
        public List<ChargeInPackageModel> listCharge { get; set; }
    }
    /// <summary>
    /// Model for module confirm charge belongs in package
    /// </summary>
    public class ChargeInPackageModel : ICloneable
    {
        public Guid? ServiceInpackageId { get; set; }
        public int ServiceType { get; set; }
        public bool IsChecked { get; set; }
        //public Guid Id { get; set; }
        public Guid? PatientInPackageId { get; set; }
        public int? PatientInPackageStatus { get; set; }
        public int? PatientInPackageLastStatus { get; set; }
        public Guid? PatientInPackageDetailId { get; set; }
        public Guid? HisChargeId { get; set; }
        public Guid? ChargeId { get; set; }
        public DateTime? ChargeDateTime { get; set; }
        public string ChargeDate { get; set; }
        public string PID { get; set; }
        public string PatientName { get; set; }
        /// <summary>
        /// Số lượng chỉ định
        /// </summary>
        public int? QtyCharged { get; set; }
        /// <summary>
        /// Số lượng/Định mức còn lại
        /// </summary>
        public int? QtyRemain { get; set; }
        /// <summary>
        /// Số lượng tái khám còn lại
        /// </summary>
        public int? ReExamQtyRemain { get; set; }
        /// <summary>
        /// Đơn giá
        /// </summary>
        public double? Price { get; set; }
        /// <summary>
        /// Đơn giá trong gói (Chỉ sử dụng cho tính toán
        /// </summary>
        public double? PkgPrice { get; set; }
        /// <summary>
        /// Thành tiền
        /// </summary>
        public double? Amount { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        /// <summary>
        /// 1: Trong gói
        /// 2: Vượt gói
        /// 3: Ngoài gói
        /// 4: Invalid Qty (số lượng trong 1 chỉ định > Định mức)
        /// </summary>
        public int InPackageType { get; set; }
        //tungdd14 thêm điều kiện check service thuộc gói
        public string VisitType { get; set; }
        public string VisitCode { get; set; }
        #region Infor for notes
        public bool IsBelongOtherPackakge { get; set; }
        /// <summary>
        /// ID gói đã được ghi nhận
        /// </summary>
        public Guid? WasPackageId { get; set; }
        /// <summary>
        /// Mã gói đã được ghi nhận
        /// </summary>
        public string WasPackageCode { get; set; }
        /// <summary>
        /// Tên gói đã được ghi nhận
        /// </summary>
        public string WasPackageName { get; set; }
        /// <summary>
        /// ID dịch vụ được thay thế
        /// </summary>
        public Guid? RootId { get; set; }
        /// <summary>
        /// 21-07-2022 tungdd14: Charge chỉ định và thực hiện khi Gói ở trạng thái tái khám lần 2 (Follow up)
        /// </summary>
        public bool ChargeIsUseForReExam { get; set; }
        /// <summary>
        /// tungdd14: service đánh dấu là tái khám lần 1
        /// </summary>
        public bool ServiceUseForReExam { get; set; }

        #endregion .Infor for notes
        #region Infor for update price OH
        public string Result_Code_OH { get; set; }
        public string Result_Message_OH { get; set; }
        public string UpdatedDateTime_OH { get; set; }
        #endregion .Infor for update price OH
        public dynamic Notes { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    #endregion .Charge refer
    #region Charge Statistic via Visitcode
    public class ChargeStatisticModel
    {
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string VisitCode { get; set; }
        public string VisitDate { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public List<ChargeStatisticDetailModel> Details { get; set; }
    }
    public class ChargeStatisticDetailModel : ICloneable
    {
        public Guid ChargeId { get; set; }
        public string ChargeDate { get; set; }
        /// <summary>
        /// Số lượng chỉ định
        /// </summary>
        public double? QtyCharged { get; set; }
        public double? QtyInPackage { get; set; }

        /// <summary>
        /// Đơn giá lẻ/ hoặc đơn giá trong gói
        /// </summary>
        public double? Price { get; set; }
        /// <summary>
        /// Đơn giá chỉ định
        /// </summary>
        public double? ChargePrice { get; set; }
        /// <summary>
        /// Thành tiền
        /// </summary>
        public double? Amount { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public Guid? PatientInPackageId { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
        /// <summary>
        /// 1: Trong gói
        /// 2: Vượt gói
        /// 3: Ngoài gói
        /// </summary>
        public int InPackageType { get; set; }
        /// <summary>
        /// -1: Phải thu
        /// 0: Total/Tổng
        /// 1: Group/ Nhóm
        /// 2: Item/ chi tiết
        /// </summary>
        public int ItemType { get; set; }
        /// <summary>
        /// check là Thuốc & VTHH
        /// </summary>
        public bool IsDrugConsum { get; set; }
        public bool IsTotal { get; set; }
        public bool IsInvoiced { get; set; }
        /// <summary>
        /// tungdd14: Charge chỉ định và thực hiện khi Gói ở trạng thái tái khám (Follow up)
        /// </summary>
        public bool ChargeIsUseForReExam { get; set; }
        /// <summary>
        /// Check visit in package
        /// </summary>
        public string VisitType { get; set; }
        /// <summary>
        /// ID dịch vụ được thay thế
        /// </summary>
        public Guid? RootId { get; set; }
        public dynamic Notes { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    #endregion .Charge Statistic via Visitcode
    #region Statistic charge in package when cancelled
    public class ChargeStatisticWhenCancelledModel
    {
        public string PID { get; set; }
        public string PatientName { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
        /// <summary>
        /// Giá  gói apply cho gói khám của khách hàng
        /// </summary>
        public double? PkgAmount { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public List<ChargeStatisticWhenCancelledDetailModel> Details { get; set; }
    }
    public class ChargeStatisticWhenCancelledDetailModel : ICloneable
    {
        public Guid ChargeId { get; set; }
        public string ChargeDate { get; set; }
        public string VisitCode { get; set; }
        /// <summary>
        /// Số lượng chỉ định
        /// </summary>
        public double? Qty { get; set; }
        /// <summary>
        /// Đơn giá
        /// </summary>
        public double? PkgPrice { get; set; }
        /// <summary>
        /// Thành tiền
        /// </summary>
        public double? PkgAmount { get; set; }
        /// <summary>
        /// Đơn giá
        /// </summary>
        public double? Price { get; set; }
        /// <summary>
        /// Thành tiền
        /// </summary>
        public double? Amount { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public Guid? PatientInPackageId { get; set; }
        /// <summary>
        /// -2: Phải trả khách hàng
        /// -1: Phải thu khách hàng
        /// 0: Total/Tổng
        /// 1: Group/ Nhóm
        /// 2: Item/ chi tiết
        /// </summary>
        public int ItemType { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
    #endregion .Statistic charge in package when cancelled
}