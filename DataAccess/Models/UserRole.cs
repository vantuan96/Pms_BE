using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class UserRole : IGuidEntity
    {
        public Guid Id { get; set; }
        public Guid? RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
