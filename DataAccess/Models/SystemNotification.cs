using DataAccess.Models.BaseModel;
using System;

namespace DataAccess.Models
{
    public class SystemNotification: IGuidEntity, ICreateEntity
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Service { get; set; }
        public string Subject { get; set; }
        public string Scope { get; set; }
        public string Content { get; set; }
        // 0: error
        // 1: sent
        // 2: done
        public int Status { get; set; }
    }
}
