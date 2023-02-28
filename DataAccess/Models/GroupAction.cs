using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class GroupAction : IGuidEntity
    {
        public GroupAction()
        {
            this.GroupAction_Maps = new HashSet<GroupAction_Map>();
        }
        [Column(Order = 0)]
        [Key]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        public Guid ModuleId { get; set; }
        [ForeignKey("ModuleId")]
        public virtual Module Module { get; set; }
        public virtual ICollection<GroupAction_Map> GroupAction_Maps { get; set; }
        [Column(Order = 2)]
        [StringLength(150)]
        public string GroupActionCode { get; set; }
        [Column(Order = 3)]
        [StringLength(250)]
        public string GroupActionName { get; set; }
        /// <summary>
        /// Là dạng menu
        /// </summary>
        [Column(Order = 4)]
        public bool IsMenu { get; set; }
        [Column(Order = 5)]
        [StringLength(1500)]
        public string Url { get; set; }
        /// <summary>
        /// _blank|_self|_parent|_top
        /// </summary>
        [Column(Order = 6)]
        [StringLength(100)]
        public string UrlTarget { get; set; }
        [Column(Order = 7)]
        public int OrderDisplay { get; set; }
        /// <summary>
        /// Hiển thị lên frontend hoặc ở chế độ ẩn
        /// </summary>
        [Column(Order = 8)]
        public bool IsDisplay { get; set; }
        [Column(Order = 9)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 10)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 11)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 12)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 13)]
        public bool IsDeleted { get; set; }
        [Column(Order = 14)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 15)]
        public DateTime? DeletedAt { get; set; }
    }
}
