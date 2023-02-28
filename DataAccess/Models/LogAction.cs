using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class LogAction : IGuidEntity, ICreateEntity
    {
        [Column(Order = 0)]
        [Key]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        /// <summary>
        /// ID entity action on
        /// </summary>
        public Guid? ObjectId { get; set; }
        [Column(Order = 2)]
        [StringLength(150)]
        public string ObjectName { get; set; }
        /// <summary>
        /// 1: Registered;
        //2: Activated;
        //3: Cancelled;
        //4: Terminated;
        //5: Transferred;
        //6: Expired;
        //7: Closed
        /// 11: Created
        /// 12: Update/Modify
        /// 13: Deleted
        /// </summary>
        [Column(Order = 3)]
        public int ActionType { get; set; }
        [Column(Order = 4)]
        [StringLength(1500)]
        public string Notes { get; set; }
        [Column(Order = 5)]
        public DateTime? CreatedAt { get; set; }
        [Column(Order = 6)]
        [StringLength(150)]
        public string CreatedBy { get; set; }
    }
}
