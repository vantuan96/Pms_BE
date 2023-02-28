using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GAPIT.MKT.Framework.Core
{
    public interface IBaseEntity
    {
        bool IsValid
        {
            get;
        }

        bool IsChanged
        {
            get;            
        }

        bool IsNew
        {
            get;            
        }

        bool IsDeleted
        {
            get;            
        }

        bool CanEdit
        {
            get;
        }

        bool CanRead
        {
            get;
        }

        bool CanDelete
        {
            get;
        }

        bool CanAdd
        {
            get;
        }

        bool IsAuthenticated
        {
            get;
        }        
    }
}
