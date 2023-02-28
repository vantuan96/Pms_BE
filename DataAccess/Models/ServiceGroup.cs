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
    public class ServiceGroup : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(TypeName = "VARCHAR", Order = 1)]
        [StringLength(50)]
        [Index]
        public string Code { get; set; }
        [Column(Order = 2)]
        [StringLength(250)]
        public string ViName { get; set; }
        [Column(Order = 3)]
        [StringLength(250)]
        public string EnName { get; set; }
        [Column(Order = 4)]

        public DateTime? CreatedAt { get; set; }
        [Column(Order = 5)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 6)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 7)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 8)]
        public bool IsDeleted { get; set; }
        [Column(Order = 9)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 10)]
        public DateTime? DeletedAt { get; set; }
    }
}
