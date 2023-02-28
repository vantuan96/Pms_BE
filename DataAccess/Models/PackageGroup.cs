using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class PackageGroup : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        [StringLength(150)]
        [Index("IX_UniqueCode", 1, IsUnique = true)]
        public string Code { get; set; }
        [StringLength(150)]
        public string Name { get; set; }
        [DefaultValue(1)]
        public int Level { get; set; }
        [DefaultValue(1)]
        public bool IsActived { get; set; }
        public DateTime? CreatedAt { get; set; }
        [StringLength(150)]
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(150)]
        public string UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        [StringLength(150)]
        public string DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
