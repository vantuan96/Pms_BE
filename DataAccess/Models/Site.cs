using DataAccess.Models.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Site : IGuidEntity
    {
        public Guid Id { get; set; }
        //public Guid? ParentId { get; set; }
        [StringLength(250)]
        public string Name { get; set; }
        [StringLength(50)]
        public string Code { get; set; }
        public int HISCode { get; set; }
        [StringLength(50)]
        public string ApiCode { get; set; }
        /// <summary>
        /// Code Site Xét Nghiêm
        /// </summary>
        [StringLength(50)]
        public string ApiLabCode { get; set; }
        /// <summary>
        /// Code Site CĐHA
        /// </summary>
        [StringLength(50)]
        public string ApiXRayCode { get; set; }

        [StringLength(50)]
        public string HospitalId { get; set; }
        [StringLength(250)]
        public string FullNameL { get; set; }
        [StringLength(250)]
        public string FullNameE { get; set; }
        [StringLength(500)]
        public string AddressL { get; set; }
        [StringLength(500)]
        public string AddressE { get; set; }
        [StringLength(150)]
        public string Tel { get; set; }
        [StringLength(150)]
        public string Fax { get; set; }
        [StringLength(150)]
        public string Hotline { get; set; }
        [StringLength(150)]
        public string Emergency { get; set; }
        public int Level { get; set; }
        public double OnsitePercent { get; set; }
        /// <summary>
        /// false: InActive
        /// true: Active
        /// </summary>
        public bool IsActived { get; set; }
    }
}
