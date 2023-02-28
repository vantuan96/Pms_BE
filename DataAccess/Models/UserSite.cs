using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    public class UserSite : IGuidEntity
    {
        public Guid Id { get; set; }
        public Guid? SiteId { get; set; }
        [ForeignKey("SiteId")]
        public virtual Site Site { get; set; }
        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public Guid? SpecialtyId { get; set; }
        [ForeignKey("SpecialtyId")]
        public virtual Specialty Specialty { get; set; }
    }
}
