using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class Log: IGuidEntity, ICreateEntity
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        [StringLength(150)]
        public string CreatedBy { get; set; }
        [StringLength(250)]
        public string Action { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(450)]
        [Index]
        public string URI { get; set; }
        [StringLength(250)]
        public string Name { get; set; }
        [StringLength(1500)]
        public string Request { get; set; }
        [StringLength(1500)]
        public string Response { get; set; }
        [StringLength(150)]
        public string Ip { get; set; }
        [StringLength(1500)]
        public string Reason { get; set; }
    }
}
