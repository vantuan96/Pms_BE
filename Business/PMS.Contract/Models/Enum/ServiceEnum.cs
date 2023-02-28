using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.Enum
{
    public enum ServiceInPackageTypeEnum
    {
        /// <summary>
        /// Không Xác Đinh/ Ngoài gói
        /// </summary>
        UNKNOWN = -1,
        /// <summary>
        /// Dịch vụ thông thường
        /// </summary>
        TOTAL = 0,
        /// <summary>
        /// Dịch vụ thông thường
        /// </summary>
        SERVICE = 1,
        /// <summary>
        /// Dịch vụ Thuốc/ VTTH
        /// </summary>
        SERVICE_DRUG_CONSUM = 2
    }
}
