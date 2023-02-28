using System;

namespace DataAccess.Models.BaseModel
{
    public interface IUpdatEntity
    {
        string UpdatedBy { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
