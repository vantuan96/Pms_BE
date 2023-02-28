using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccess.Models
{
    public class SystemConfig : IGuidEntity
    {
        public Guid Id { get; set; }
        /// <summary>
        /// 1: Sync Service 
        /// 2: Sync Department
        /// 3: Sync Revenue
        /// </summary>
        public int TypeConfig { get; set; }
        [StringLength(50)]
        public string SiteCode { get; set; }
        [StringLength(1500)]
        public string NotificationEmail { get; set; }
        public DateTime? LastUpdatedEHosService { get; set; }
        public DateTime? LastUpdatedOHService { get; set; }
        /// <summary>
        /// If have endDate to get via EndDate
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// So phut mo rong them
        /// </summary>
        public int ExMinute { get; set; }
    }
}
