using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class LogInFail : IGuidEntity, ICreateEntity, IUpdatEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        [StringLength(150)]
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        [StringLength(150)]
        public string UpdatedBy { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(20)]
        [Index]
        public string IPAddress { get; set; }
        public int Time { get; set; }
    }
}
