using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Service : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        public Service()
        {
            //this.ServiceFreeInPackages = new HashSet<ServiceFreeInPackage>();
        }
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        public Guid? ServiceId { get; set; }
        [Column(Order = 2)]
        public Guid? ServiceGroupId { get; set; }
        [ForeignKey("ServiceGroupId")]
        public virtual ServiceGroup ServiceGroup { get; set; }
        [Column(Order = 3)]
        public Guid? ServiceCategoryId { get; set; }
        [ForeignKey("ServiceCategoryId")]
        public virtual ServiceCategory ServiceCategory { get; set; }
        [Column(TypeName = "VARCHAR", Order =4)]
        [StringLength(50)]
        [Index]
        public string ServiceType { get; set; }
        [Column(TypeName = "VARCHAR", Order = 5)]
        [StringLength(50)]
        [Index]
        public string Code { get; set; }
        [Column(Order = 6)]
        [StringLength(500)]
        public string ViName { get; set; }
        [Column(Order = 7)]
        [StringLength(500)]
        public string EnName { get; set; }
        [Column(Order = 8)]
        public bool IsActive { get; set; }
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
        //public virtual ICollection<ServiceFreeInPackage> ServiceFreeInPackages { get; set; }

    }
}
