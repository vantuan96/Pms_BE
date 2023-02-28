using DataAccess.Models.BaseModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Models
{
    /// <summary>
    /// Bảng tạm dữ liệu nhóm gói dịch vụ. Phục vụ Migrate dữ liệu từ eHos
    /// </summary>
    public class Temp_PackageGroup : IGuidEntity
    {
        public Guid Id { get; set; }
        [StringLength(250)]
        public string Code { get; set; }
        [StringLength(450)]
        public string ParrentCode { get; set; }
        [StringLength(500)]
        public string Name { get; set; }
        [StringLength(150)]
        public string Status { get; set; }
    }
}
