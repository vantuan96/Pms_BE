using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class RoleGroupAction : IGuidEntity
    {
        [Column(Order = 0)]
        [Key]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        public Guid? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        [Column(Order = 2)]
        public Guid? GaId { get; set; }
        [ForeignKey("GaId")]
        public virtual GroupAction GroupAction { get; set; }
        [Column(Order = 3)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 4)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 5)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 6)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 7)]
        public bool IsDeleted { get; set; }
        [Column(Order = 8)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 9)]
        public DateTime? DeletedAt { get; set; }
    }
}
