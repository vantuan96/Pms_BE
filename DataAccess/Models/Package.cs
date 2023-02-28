using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Package : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity,ICloneable
    {
        public Package()
        {
            this.PackagePrices = new HashSet<PackagePrice>();
        }
        [Key]
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        public Guid PackageGroupId { get; set; }
        [ForeignKey("PackageGroupId")]
        public virtual PackageGroup PackageGroup { get; set; }
        [StringLength(150)]
        [Index("IX_UniqueCode", 1, IsUnique = true)]
        [Column(Order = 2)]
        public string Code { get; set; }
        [StringLength(150)]
        [Column(Order =3)]
        public string Name { get; set; }
        /// <summary>
        /// 0: InActivated
        /// 1: Activated
        /// </summary>
        [DefaultValue(true)]
        [Column(Order = 4)]
        public bool IsActived { get; set; }
        /// <summary>
        /// Thiết lập giá?
        /// 0: NotYet-setted
        /// 1: Setted
        /// </summary>
        [DefaultValue(false)]
        [Column(Order = 5)]
        public bool IsPriceSetted { get; set; }
        /// <summary>
        /// Thuốc & VTTH: Không định mức/ Theo định mức
        /// 0: Unlimited
        /// 1: Limited
        /// </summary>
        [DefaultValue(true)]
        [Column(Order = 6)]
        public bool IsLimitedDrugConsum { get; set; } = true;
        /// <summary>
        /// Được Migrate từ eHos
        /// </summary>
        [Column(Order = 7)]
        public bool IsFromeHos { get; set; }
        /// <summary>
        /// Được Migrate từ Concerto
        /// </summary>
        [Column(Order = 8)]
        public bool Concerto { get; set; }
        [Column(Order = 9)]
        public DateTime? CreatedAt { get; set; }
        [StringLength(150)]
        [Column(Order = 10)]
        public string CreatedBy { get; set; }
        [Column(Order = 11)]
        public DateTime? UpdatedAt { get; set; }
        [StringLength(150)]
        [Column(Order = 12)]
        public string UpdatedBy { get; set; }
        [Column(Order = 13)]
        public bool IsDeleted { get; set; }
        [StringLength(150)]
        [Column(Order = 14)]
        public string DeletedBy { get; set; }
        [Column(Order = 15)]
        public DateTime? DeletedAt { get; set; }

        public virtual ICollection<PackagePrice> PackagePrices { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
