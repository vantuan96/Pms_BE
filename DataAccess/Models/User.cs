using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace DataAccess.Models
{
    public class User : IGuidEntity, ICreateEntity, IUpdatEntity, IDeleteEntity
    {
        public User()
        {
            this.UserSites = new HashSet<UserSite>();
            this.UserRoles = new HashSet<UserRole>();
            this.UserPositions = new HashSet<UserPosition>();
        }
        public static ClaimsIdentity Identity { get; internal set; }
        [Key]
        public Guid Id { get; set; }
        [Column(TypeName = "VARCHAR")]
        [StringLength(100)]
        [Index]
        public string Username { get; set; }
        [StringLength(250)]
        public string Roles { get; set; }
        [StringLength(250)]
        public string Fullname { get; set; }
        [StringLength(150)]
        public string FirstName { get; set; }
        [StringLength(150)]
        public string LastName { get; set; }
        [StringLength(150)]
        public string MiddleName { get; set; }
        [StringLength(250)]
        public string DisplayName { get; set; }
        [StringLength(250)]
        public string LoginNameWithDomain { get; set; }
        [StringLength(150)]
        public string Mobile { get; set; }
        [StringLength(150)]
        public string EmailAddress { get; set; }
        [StringLength(250)]
        public string Department { get; set; }
        [StringLength(250)]
        public string Title { get; set; }
        [StringLength(1500)]
        public string Description { get; set; }
        [StringLength(250)]
        public string Company { get; set; }
        [StringLength(250)]
        public string ManagerName { get; set; }
        [StringLength(250)]
        public string ManagerId { get; set; }
        [StringLength(150)]
        public string SessionId { get; set; }
        [StringLength(1500)]
        public string Session { get; set; }
        [StringLength(150)]
        public string EhosAccount { get; set; }
        public Guid? NotifyID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public virtual ICollection<UserSite> UserSites { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<UserPosition> UserPositions { get; set; }
    }
}
