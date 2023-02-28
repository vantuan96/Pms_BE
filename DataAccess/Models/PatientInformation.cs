using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class PatientInformation : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        [Column(Order = 0)]
        public Guid Id { get; set; }
        /// <summary>
        /// ID trên OH
        /// </summary>
        [Column(Order = 1)]
        public Guid? PatientId { get; set; }
        [Column(Order = 2)]
        [StringLength(50)]
        public string PID { get; set; }
        [Column(Order = 3)]
        [StringLength(250)]
        public string FullName { get; set; }
        /// <summary>
        /// Ngày sinh
        /// </summary>
        [Column(Order = 4)]
        public DateTime? DateOfBirth { get; set; }
        [Column(Order = 5)]
        /// <summary>
        /// 0: Không xác định
        /// 1: Nam
        /// 2: Nữ
        /// </summary>
        public int? Gender { get; set; }
        [Column(Order = 6)]
        [StringLength(150)]
        public string Mobile { get; set; }
        [Column(Order = 7)]
        [StringLength(150)]
        public string Email { get; set; }
        
        [Column(Order = 8)]
        [StringLength(500)]
        public string Address { get; set; }
        [Column(Order = 9)]
        [StringLength(150)]
        public string IdentityNo { get; set; }
        [Column(Order = 10)]
        [StringLength(150)]
        public string National { get; set; }
        /// <summary>
        /// Đang xử lý gói khám
        /// </summary>
        [Column(Order = 11)]
        public Guid? CurrentPatientInPackageId { get; set; }
        /// <summary>
        /// User đang xử lý
        /// </summary>
        [Column(Order = 12)]
        [StringLength(50)]
        public string CurrentUserProcess { get; set; }
        [Column(Order = 13)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 14)]
        [StringLength(50)]
        public string CreatedBy { get; set; }
        [Column(Order = 15)]
        public DateTime? UpdatedAt { get; set; }
        [Column(Order = 16)]
        [StringLength(50)]
        public string UpdatedBy { get; set; }
        [Column(Order = 17)]
        public bool IsDeleted { get; set; }
        [Column(Order = 18)]
        [StringLength(50)]
        public string DeletedBy { get; set; }
        [Column(Order = 19)]
        public DateTime? DeletedAt { get; set; }
    }
}
