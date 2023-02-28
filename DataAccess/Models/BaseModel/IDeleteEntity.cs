using System;

namespace DataAccess.Models.BaseModel
{
    public interface IDeleteEntity
    {
        bool IsDeleted { get; set; }
        string DeletedBy { get; set; }
        DateTime? DeletedAt { get; set; }
    }
}
