using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Department: IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string ViName { get; set; }
        [StringLength(250)]
        public string EnName { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        [Index]
        public string Code { get; set; }
        [StringLength(50)]
        public string HospitalCode { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? SpecialtyId { get; set; }
        [ForeignKey("SpecialtyId")]
        public virtual Specialty Specialty { get; set; }
        public bool IsActivated { get; set; }
    }
}
