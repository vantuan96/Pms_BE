using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class PatientInPackageDetail : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        public PatientInPackageDetail()
        {

        }
        [Column(Order = 0)]
        public Guid Id { get; set; }

        [Column(Order = 1)]
        public Guid? PatientInPackageId { get; set; }
        [ForeignKey("PatientInPackageId")]
        public virtual PatientInPackage PatientInPackage { get; set; }

        [Column(Order = 2)]
        public Guid? ServiceInPackageId { get; set; }
        [ForeignKey("ServiceInPackageId")]
        public virtual ServiceInPackage ServiceInPackage { get; set; }
        /// <summary>
        /// Đơn giá cơ sở
        /// </summary>
        [Column(Order = 3)]
        public double? BasePrice { get; set; }

        /// <summary>
        /// Thành tiền cơ sơ
        /// </summary>
        [Column(Order = 4)]
        public double? BaseAmount { get; set; }

        /// <summary>
        /// Đơn giá trong gói
        /// </summary>
        [Column(Order = 5)]
        public double? PkgPrice { get; set; }

        /// <summary>
        /// Thành tiền trong gói
        /// </summary>
        [Column(Order = 6)]
        public double? PkgAmount { get; set; }
        /// <summary>
        /// Số lượng đã sử dụng
        /// </summary>
        [Column(Order = 7)]
        [DefaultValue(0)]
        public int? QtyWasUsed { get; set; }
        /// <summary>
        /// Số lượng còn lại
        /// </summary>
        [Column(Order = 8)]
        public int? QtyRemain { get; set; }
        /// <summary>
        /// Định mức tái khám
        /// </summary>
        [Column(Order = 9)]
        [DefaultValue(0)]
        public int? ReExamQtyLimit { get; set; }
        /// <summary>
        /// Số lượng tái khám đã sử dụng 
        /// </summary>
        [Column(Order = 10)]
        [DefaultValue(0)]
        public int? ReExamQtyWasUsed { get; set; }
        /// <summary>
        /// Số lượng tái khám còn lại
        /// </summary>
        [Column(Order = 11)]
        public int? ReExamQtyRemain { get; set; }

        [Column(Order = 12)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 13)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 14)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 15)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 16)]
        public bool IsDeleted { get; set; }
        [Column(Order = 17)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 18)]
        public DateTime? DeletedAt { get; set; }
    }
}