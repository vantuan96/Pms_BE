using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class PackagePriceDetail : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity,ICloneable
    {
        public PackagePriceDetail()
        {
            
        }
        [Column(Order = 0)]
        public Guid Id { get; set; }

        [Column(Order = 1)]
        public Guid? PackagePriceId { get; set; }
        [ForeignKey("PackagePriceId")]
        public virtual PackagePrice PackagePrice { get; set; }

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

        [Column(Order = 7)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 8)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 9)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 10)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 11)]
        public bool IsDeleted { get; set; }
        [Column(Order = 12)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 13)]
        public DateTime? DeletedAt { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
