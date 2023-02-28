using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Module : IGuidEntity
    {
        public Module()
        {
            this.GroupActions = new HashSet<GroupAction>();
        }
        [Column(Order = 0)]
        [Key]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        [StringLength(150)]
        public string Code { get; set; }
        [Column(Order = 2)]
        [StringLength(250)]
        public string Name { get; set; }
        public virtual ICollection<GroupAction> GroupActions { get; set; }
        [Column(Order = 3)]
        public int OrderDisplay { get; set; }
        /// <summary>
        /// Hiển thị lên frontend hoặc ở chế độ ẩn
        /// </summary>
        [Column(Order = 4)]
        public bool IsDisplay { get; set; }
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
    }
}
