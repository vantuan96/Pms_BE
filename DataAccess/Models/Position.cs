using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class Position: IGuidEntity
    {
        [Key]
        public Guid Id { get; set; }
        [StringLength(250)]
        public string ViName { get; set; }
        [StringLength(250)]
        public string EnName { get; set; }
    }
}
