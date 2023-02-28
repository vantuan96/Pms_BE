using System;

namespace DataAccess.Models.BaseModel
{
    public interface ICreateEntity
    {
        string CreatedBy { get; set; }
        DateTime? CreatedAt { get; set; }
    }
}
