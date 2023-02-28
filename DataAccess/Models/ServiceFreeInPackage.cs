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
    public class ServiceFreeInPackage : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity,ICloneable
    {
        public ServiceFreeInPackage()
        {
            //this.Rules = new HashSet<ServiceRule>();
        }
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        [Index("IX_UniqueServiceFree", 1, IsUnique = true)]
        public Guid? ServiceId { get; set; }
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
        [Column(Order = 2)]
        [Index("IX_UniqueServiceFree", 2, IsUnique = true)]
        [StringLength(150)]
        public string GroupCode { get; set; }
        [Column(Order = 3)]
        public bool IsActived { get; set; }
        [Column(Order = 4)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 5)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 6)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 7)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 8)]
        public bool IsDeleted { get; set; }
        [Column(Order = 9)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 10)]
        public DateTime? DeletedAt { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
