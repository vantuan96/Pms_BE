using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class ServiceInPackage : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity,ICloneable
    {
        public ServiceInPackage()
        {
            //this.Rules = new HashSet<ServiceRule>();
        }
        [Column(Order = 0)]
        public Guid Id { get; set; }
        /// <summary>
        /// Id gốc được thay thế
        /// </summary>
        [Column(Order = 1)]
        public Guid? RootId { get; set; }
        [Column(Order = 2)]
        public Guid? PackageId { get; set; }
        [ForeignKey("PackageId")]
        public virtual Package Package { get; set; }
        [Column(Order = 3)]
        public Guid? ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
        /// <summary>
        /// Định mức /Limit sử dụng
        /// </summary>
        [Column(Order = 4)]
        [DefaultValue(1)]
        public int? LimitQty { get; set; }
        /// <summary>
        /// Là gói Thuốc/VTTH
        /// </summary>
        [Column(Order = 5)]
        public bool IsPackageDrugConsum{ get; set; }
        /// <summary>
        /// Loại dịch vụ:
        /// 1: Dịch vụ
        /// 2: Thuốc & VTTH
        /// </summary>
        [Column(Order = 6)]
        [DefaultValue(1)]
        public int ServiceType { get; set; }
        /// <summary>
        /// Là dịch vụ tái khám (Follow up)
        /// </summary>
        [Column(Order = 7)]
        [DefaultValue(true)]
        public bool IsReExamService { get; set; }
        //[Column(Order = 7)]
        //public bool IsActived { get; set; }
        [Column(Order = 8)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 9)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 10)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 11)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 12)]
        public bool IsDeleted { get; set; }
        [Column(Order = 13)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 14)]
        public DateTime? DeletedAt { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
