using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class PatientInPackage : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        public Guid PatientInforId { get; set; }
        [ForeignKey("PatientInforId")]
        public virtual PatientInformation PatientInformation { get; set; }
        [Column(Order=2)]
        public Guid PackagePriceSiteId { get; set; }
        [ForeignKey("PackagePriceSiteId")]
        public virtual PackagePriceSite PackagePriceSite { get; set; }
        /// <summary>
        /// Id patient in package gói nâng cấp - Gói dịch vụ thay thế
        /// </summary>
        [Column(Order = 3)]
        public Guid? NewPatientInPackageId { get; set; }
        #region Contract Information
        [Column(Order = 4)]
        [StringLength(50)]
        public string ContractNo { get; set; }
        [Column(Order = 5)]
        public DateTime? ContractDate { get; set; }
        /// <summary>
        /// Nhân viên phụ trách hợp đồng
        /// </summary>
        [Column(Order = 6)]
        [StringLength(50)]
        public string ContractOwner { get; set; }
        [Column(Order = 7)]
        [StringLength(250)]
        public string ContractOwnerFullName { get; set; }
        #endregion .Contract Information
        #region Doctor consult
        [Column(Order = 8)]
        [StringLength(50)]
        public string DoctorConsult { get; set; }
        [Column(Order = 9)]
        [StringLength(250)]
        public string DoctorConsultFullName { get; set; }
        [Column(Order = 10)]
        public Guid? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        #endregion .Doctor consult
        /// <summary>
        /// Ngày bắt đầu có hiệu lực sử dụng
        /// </summary>
        [Column(Order = 11)]
        public DateTime StartAt { get; set; }
        /// <summary>
        /// Ngày hết hạn sử dụng
        /// </summary>
        [Column(Order = 12)]
        public DateTime? EndAt { get; set; }
        /// <summary>
        /// Là gói thai sản
        /// </summary>
        [Column(Order = 13)]
        public bool? IsMaternityPackage { get; set; }
        /// <summary>
        /// linhht là gói bundle payment, không lưu DB
        /// </summary>
        [NotMapped]
        public virtual bool? IsBundlePackage { get; set; }
        /// <summary>
        /// Ngày dự sinh
        /// </summary>
        [Column(Order = 14)]
        public DateTime? EstimateBornDate { get; set; }
        #region Discount Info
        /// <summary>
        /// Là TH giảm giá - chiết khấu
        /// </summary>
        [Column(Order = 15)]
        public bool IsDiscount { get; set; }
        /// <summary>
        /// 1: Chiết khấu theo %
        /// 2: Chiết khấu theo VNĐ
        /// </summary>
        [Column(Order = 16)]
        public int? DiscountType { get; set; }
        /// <summary>
        /// % hoặc số tiền chiết khấu
        /// </summary>
        [Column(Order = 17)]
        public double? DiscountAmount { get; set; }
        /// <summary>
        /// Giá sau chiết khâu (Nếu có)
        /// </summary>
        [Column(Order = 18)]
        public double NetAmount { get; set; }
        /// <summary>
        /// Lý do giảm giá/chiết khấu
        /// </summary>
        [Column(Order = 19)]
        [StringLength(1500)]
        public string DiscountNote { get; set; }
        #endregion .Discount Info
        /// <summary>
        /// Thống kê tình hình sử dụng gói
        /// </summary>
        [Column(Order =20)]
        public string DataStatUsing { get; set; }
        /// <summary>
        /// Được Migrate từ eHos
        /// </summary>
        [Column(Order = 21)]
        public bool IsFromeHos { get; set; }
        /// <summary>
        /// Được Migrate từ Concerto
        /// </summary>
        [Column(Order = 22)]
        public bool Concerto { get; set; }
        /// <summary>
        /// ID phiên xử lý
        /// </summary>
        [Column(Order = 23)]
        public Guid? SessionProcessId { get; set; }
        /// <summary>
        /// 1: Registered
        /// 2: Activated
        /// 3: Cancelled
        /// 4: Terminated
        /// 5: Transferred
        /// 6: Expired
        /// 7: Closed
        /// </summary>
        [Column(Order = 24)]
        public int Status { get; set; }
        /// <summary>
        /// Has status same Status
        /// </summary>
        [Column(Order = 25)]
        public int? LastStatus { get; set; }
        [Column(Order = 26)]
        public DateTime? ActivatedAt { get; set; }
        [Column(Order = 27)]
        public DateTime? ClosedAt { get; set; }
        [Column(Order = 28)]
        public DateTime? CancelledAt { get; set; }
        [Column(Order = 29)]
        public DateTime? TerminatedAt { get; set; }
        [Column(Order = 30)]
        public DateTime? TransferredAt { get; set; }
        [Column(Order = 31)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 32)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 33)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 34)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 35)]
        public bool IsDeleted { get; set; }
        [Column(Order = 36)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 37)]
        public DateTime? DeletedAt { get; set; }
    }
}
