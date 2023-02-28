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
    public class ServiceCategory : IGuidEntity
    {
        [Column(Order = 0)]
        public Guid Id { get; set; }
        [Column(Order = 1)]
        [StringLength(150)]
        public string Code { get; set; }
        [Column(Order = 2)]
        [StringLength(250)]
        public string ViName { get; set; }
        [Column(Order = 3)]
        [StringLength(250)]
        public string EnName { get; set; }
        [Column(Order = 4)]
        public int Order { get; set; }
        [Column(Order = 5)]
        public bool IsShow { get; set; }
        [Column(Order = 6)]
        /// <summary>
        /// 0: Không config dịch vụ vào nhóm; 1: Sẽ config dịch vụ vào nhóm này.
        /// </summary>
        public bool IsConfig { get; set; }
    }
}
