using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class HISCharge : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        [Key]
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        //[Index("IX_ChargeService", 1, IsUnique = true)]
        public Guid? ItemId { get; set; }
        [Column(Order = 2)]
        [StringLength(250)]
        [Index("IX_Charge", 3, IsUnique = true)]
        public string ItemCode { get; set; }
        #region Charge Info
        [Column(Order = 3)]
        [Index("IX_Charge", 1, IsUnique = true)]
        public Guid ChargeId { get; set; }
        [Column(Order = 4)]
        public Guid? NewChargeId { get; set; }
        [Column(Order = 5)]
        public Guid? ChargeSessionId { get; set; }
        [Column(Order = 6)]
        public DateTime? ChargeDate { get; set; }
        [Column(Order = 7)]
        public DateTime? ChargeCreatedDate { get; set; }
        [Column(Order = 8)]
        public DateTime? ChargeUpdatedDate { get; set; }
        [Column(Order = 9)]
        public DateTime? ChargeDeletedDate { get; set; }
        [Column(Order = 10)]
        [StringLength(250)]
        public string ChargeStatus { get; set; }
        [Column(Order = 11)]
        [StringLength(250)]
        public string ChargeBy { get; set; }
        #endregion .Charge Info
        [Column(Order = 12)]
        [StringLength(150)]
        public string VisitType { get; set; }
        [Column(Order = 13)]
        [StringLength(150)]
        public string VisitCode { get; set; }
        [Column(Order = 14)]
        public DateTime? VisitDate { get; set; }

        #region Invoice info
        [Column(Order = 15)]
        [StringLength(150)]
        public string InvoicePaymentStatus { get; set; }
        #endregion .Invoice info
        [Column(Order = 16)]
        public Guid HospitalId { get; set; }
        [Column(Order = 17)]
        [StringLength(150)]
        public string HospitalCode { get; set; }
        #region Customer info basic
        [Column(Order = 18)]
        [StringLength(150)]
        public string PID { get; set; }
        [Column(Order = 19)]
        public Guid? CustomerId { get; set; }
        [Column(Order = 20)]
        [StringLength(250)]
        public string CustomerName { get; set; }
        #endregion .Customer info basic
        #region Package infor
        [Column(Order = 21)]
        public Guid? PatientInPackageId { get; set; }
        #endregion .Package infor
        #region Price/Quantity information
        [Column(Order = 22)]
        public double? UnitPrice { get; set; }
        [Column(Order = 23)]
        public int? Quantity { get; set; }
        #endregion .Price/Quantity information
        /// <summary>
        /// Thông tin chính sách giá Vietnamese or Foreigner của charge
        /// UNK=Vietnamese
        /// </summary>
        [StringLength(150)]
        [Column(Order = 24)]
        public string PricingClass { get; set; }
        [Column(Order = 25)]
        [Index("IX_Charge", 2, IsUnique = true)]
        /// <summary>
        /// 0: Charge on OH
        /// 1: Charge Fake
        /// </summary>
        public int ChargeType { get; set; }
        #region properties for Create, Update, Delete info
        [Column(Order = 26)]
        public DateTime? NextProcessTime { get; set; }
        [Column(Order = 27)]
        public DateTime? CreatedAt { get; set; }
        [StringLength(150)]
        [Column(Order = 28)]
        public string CreatedBy { get; set; }
        [Column(Order = 29)]
        public DateTime? UpdatedAt { get; set; }
        [StringLength(150)]
        [Column(Order = 30)]
        public string UpdatedBy { get; set; }
        [Column(Order = 31)]
        public bool IsDeleted { get; set; }
        [StringLength(150)]
        [Column(Order = 32)]
        public string DeletedBy { get; set; }
        [Column(Order = 33)]
        public DateTime? DeletedAt { get; set; }
        #endregion .properties for Create, Update, Delete info
    }
}
