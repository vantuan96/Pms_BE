using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class PackagePrice : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity,ICloneable
    {
        public PackagePrice()
        {
            this.PackagePriceSites = new HashSet<PackagePriceSite>();
            this.PackagePriceDetails = new HashSet<PackagePriceDetail>();
        }
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        [Index("IX_UniquePolicy", 1, IsUnique = true)]
        public Guid? PackageId { get; set; }
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }
        /// <summary>
        /// Chọn site để lấy giá cơ sở
        /// </summary>
        [Column(Order = 2)]
        [StringLength(50)]
        public string SiteBaseCode { get; set; }
        /// <summary>
        /// Giá trị ChargeType trên HIS (OH)
        /// </summary>
        [Column(Order = 3)]
        [StringLength(250)]
        public string ChargeType { get; set; }
        [Column(Order = 4)]
        [StringLength(250)]
        [Index("IX_UniquePolicy", 2, IsUnique = true)]
        public string Code { get; set; }
        /// <summary>
        /// 1: Người Việt Nam
        /// 2: Người Nước ngoài
        /// </summary>
        [Column(Order = 5)]
        [DefaultValue(1)]
        [Index("IX_UniquePolicy", 3, IsUnique = true)]
        public int PersonalType { get; set; }
        [Column(Order = 6)]
        public double? Amount{ get; set; }
        /// <summary>
        /// False: Cho phép đăng ký online & check trung
        /// True: Chỉ phục vụ Migrate
        /// </summary>
        [Column(Order = 7)]
        public bool IsNotForRegOnline { get; set; }
        #region Giới hạn tiền thuốc & VTTH
        /// <summary>
        /// Thuốc & VTTH: Không định mức/ Theo định mức
        /// 0: Unlimited
        /// 1: Limited
        /// </summary>
        [DefaultValue(true)]
        [Column(Order = 8)]
        public bool IsLimitedDrugConsum { get; set; }
        [Column(Order = 9)]
        public double? LimitedDrugConsumAmount { get; set; }
        #endregion .Giới hạn tiền thuốc & VTTH
        #region Tỷ lệ X đơn giá trong gói
        [Column(Order = 10)]
        public double? RateINV { get; set; }
        #endregion .Tỷ lệ X đơn giá trong gói
        #region Hiệu Lực (Date Between Apply)
        /// <summary>
        /// Bắt đầu cho phép đăng ký gói
        /// </summary>
        [Column(Order = 11)]
        public DateTime? StartAt { get; set; }
        #endregion .Hiệu Lực (Date Between Apply)
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

        public virtual ICollection<PackagePriceSite> PackagePriceSites { get; set; }
        public virtual ICollection<PackagePriceDetail> PackagePriceDetails { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
