using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class UserPosition : IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public Guid PositionId { get; set; }
        [ForeignKey("PositionId")]
        public virtual Position Position { get; set; }
    }
}
