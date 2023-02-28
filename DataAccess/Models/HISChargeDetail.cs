using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class HISChargeDetail : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        [Key]
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        public Guid HisChargeId { get; set; }
        [ForeignKey("HisChargeId")]
        public virtual HISCharge HISCharge { get; set; }
        [Column(Order = 2)]
        public Guid PatientInPackageDetailId { get; set; }
        [ForeignKey("PatientInPackageDetailId")]
        public virtual PatientInPackageDetail PatientInPackageDetail { get; set; }
        #region Package infor
        [Column(Order = 3)]
        public Guid PatientInPackageId { get; set; }
        [ForeignKey("PatientInPackageId")]
        public virtual PatientInPackage PatientInPackage { get; set; }
        [Column(Order = 4)]
        /// <summary>
        /// 1: Trong gói
        /// 2: Vượt gói
        /// </summary>
        public int InPackageType { get; set; }

        #endregion .Package infor
        #region Price/Amount information
        /// <summary>
        /// Giá tại thời điểm chỉ định
        /// </summary>
        [Column(Order = 5)]
        public double? ChargePrice { get; set; }
        /// <summary>
        /// Giá apply cho KH
        /// </summary>
        [Column(Order = 6)]
        public double? UnitPrice { get; set; }
        [Column(Order = 7)]
        public int? Quantity { get; set; }
        [Column(Order = 8)]
        public double? NetAmount { get; set; }

        #endregion .Price/Amount information
        /// <summary>
        /// Charge chỉ định và thực hiện khi Gói ở trạng thái tái khám (Follow up)
        /// </summary>
        [Column(Order = 9)]
        public bool ChargeIsUseForReExam { get; set; }
        [StringLength(1500)]
        [Column(Order = 10)]
        public string Notes { get; set; }
        #region properties for Create, Update, Delete info
        [Column(Order = 11)]
        public DateTime? CreatedAt { get; set; }
        [StringLength(150)]
        [Column(Order = 12)]
        public string CreatedBy { get; set; }
        [Column(Order = 13)]
        public DateTime? UpdatedAt { get; set; }
        [StringLength(150)]
        [Column(Order = 14)]
        public string UpdatedBy { get; set; }
        [Column(Order = 15)]
        public bool IsDeleted { get; set; }
        [StringLength(150)]
        [Column(Order = 16)]
        public string DeletedBy { get; set; }
        [Column(Order = 17)]
        public DateTime? DeletedAt { get; set; }
        #endregion .properties for Create, Update, Delete info
    }
}
