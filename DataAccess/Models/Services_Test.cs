using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    
    public class Services_Test : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        public Services_Test()
        {
            this.Rules = new HashSet<ServiceRule>();
        }

        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string ViName { get; set; }
        public string EnName { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        [Index]
        public string Code { get; set; }
        public int HISCode { get; set; }
        // 0: OH
        // 1: EHOS
        public Guid? ServiceGroupId { get; set; }
        [ForeignKey("ServiceGroupId")]
        public virtual ServiceGroup ServiceGroup { get; set; }
        public virtual ICollection<ServiceRule> Rules { get; set; }

        public Guid? ServiceCategoryId { get; set; }
        [ForeignKey("ServiceCategoryId")]
        public virtual ServiceCategory ServiceCategory { get; set; }
    }
}
