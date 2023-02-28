using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Specialty : IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string ViName { get; set; }
        [StringLength(250)]
        public string EnName { get; set; }
        [StringLength(1500)]
        public string Code { get; set; }
        [StringLength(1500)]
        public string SAPCode { get; set; }
        [StringLength(100)]
        public string SpecialtyCode { get; set; }
        [StringLength(1000)]
        public string ServiceCode { get; set; }
    }
}
