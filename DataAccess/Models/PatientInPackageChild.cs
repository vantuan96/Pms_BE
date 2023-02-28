using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class PatientInPackageChild : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        public PatientInPackageChild()
        {
            
        }
        [Column(Order = 0)]
        public Guid Id { get; set; }

        [Column(Order = 1)]
        public Guid? PatientInPackageId { get; set; }
        [ForeignKey("PatientInPackageId")]
        public virtual PatientInPackage PatientInPackage { get; set; }

        [Column(Order = 2)]
        public Guid? PatientChildInforId { get; set; }
        [ForeignKey("PatientChildInforId")]
        public virtual PatientInformation PatientInformation { get; set; }
        [Column(Order = 9)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 10)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 11)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 12)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 13)]
        public bool IsDeleted { get; set; }
        [Column(Order = 14)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 15)]
        public DateTime? DeletedAt { get; set; }
    }
}
