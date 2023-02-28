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
    public class AppConstant : IGuidEntity
    {
        public Guid Id { get; set; }
        
        [StringLength(150)]
        [Index("IX_AppContant", 1, IsUnique = true)]
        public string ConstantKey { get; set; }
        [StringLength(1500)]
        public string ConstantValue { get; set; }
        /// <summary>
        /// false: InActive
        /// true: Active
        /// </summary>
        public bool IsActived { get; set; }
    }
}
