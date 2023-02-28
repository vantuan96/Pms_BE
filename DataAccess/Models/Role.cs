using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Role : IGuidEntity
    {
        public Role()
        {
            this.RoleGroupActions = new HashSet<RoleGroupAction>();
        }
        [Key]
        public Guid Id { get; set; }
        [StringLength(100)]
        public string Code { get; set; }
        [StringLength(250)]
        public string ViName { get; set; }
        [StringLength(250)]
        public string EnName { get; set; }
        /// <summary>
        /// Mô tả
        /// </summary>
        [StringLength(2000)]
        public string Description { get; set; }
        //Đánh giá mức độ Power của mỗi nhóm quyền (Tạm thời giá trị chỉ dùng để order và tham khảo)
        public int Level { get; set; }
        public Guid? DefaultMenuId { get; set; }
        [ForeignKey("DefaultMenuId")]
        public virtual GroupAction DefaultMenu { get; set; }
        public virtual ICollection<RoleGroupAction> RoleGroupActions { get; set; }
    }
}
