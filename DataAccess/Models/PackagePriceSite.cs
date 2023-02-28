using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class PackagePriceSite : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity,ICloneable
    {
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        public Guid? SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site Site { get; set; }
        [Column(Order = 2)]
        public Guid? PackagePriceId { get; set; }
        [ForeignKey("PackagePriceId")]
        public virtual PackagePrice PackagePrice { get; set; }
        /// <summary>
        /// Ngày hết hiệu lực. Kết thúc & không cho phép đăng ký gói
        /// </summary>
        [Column(Order = 3)]
        public DateTime? EndAt { get; set; }
        [Column(Order = 4)]
        [StringLength(50)]
        public string Notes { get; set; }
        [Column(Order = 5)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 6)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 7)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 8)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 9)]
        public bool IsDeleted { get; set; }
        [Column(Order = 10)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 11)]
        public DateTime? DeletedAt { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
