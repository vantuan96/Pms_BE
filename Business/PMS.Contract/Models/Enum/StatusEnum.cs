using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.Enum
{
    public enum StatusEnum
    {
        UNSUCCESS=-1,
        /// <summary>
        /// Thành công
        /// </summary>
        SUCCESS = 200,
        /// <summary>
        /// Accepted (The request has been accepted for processing, but the processing has not been completed)
        /// </summary>
        ACCEPTED = 202,
        /// <summary>
        /// No Content
        /// </summary>
        NO_CONTENT = 204,
        /// <summary>
        /// Not Modified
        /// </summary>
        NOT_MODIFIED = 304,
        /// <summary>
        /// Found
        /// </summary>
        FOUND=302,
        /// <summary>
        /// Bad Request
        /// </summary>
        BAD_REQUEST = 400,
        /// <summary>
        /// Unauthorized
        /// </summary>
        UNAUTHORIZED = 401,
        /// <summary>
        /// Payment Required
        /// </summary>
        PAYMENT_REQUIRED = 402,
        /// <summary>
        /// Forbidden
        /// </summary>
        FORBIDDEN = 403,
        /// <summary>
        /// Không tìm thấy
        /// </summary>
        NOT_FOUND = 404,
        /// <summary>
        /// Method Not Allowed
        /// </summary>
        METHOD_NOT_ALLOWED = 405,
        /// <summary>
        /// Not Acceptable
        /// </summary>
        NOT_ACCEPTABLE = 406,
        /// <summary>
        /// Proxy Authentication Required
        /// </summary>
        PROXY_AUTHENTICATION_REQUIRED = 407,
        /// <summary>
        /// Request Timeout
        /// </summary>
        REQUEST_TIMEOUT = 408,
        /// <summary>
        /// Conflict
        /// </summary>
        CONFLICT = 409,
        /// <summary>
        /// Internal Server Error
        /// </summary>
        INTERNAL_SERVER_ERROR = 500,
        /// <summary>
        /// Yêu cầu mở Popup
        /// </summary>
        REQUIRE_OPEN_POPUP=1001,
        /// <summary>
        /// Ngày bắt đầu sử dụng sớm hơn ngày hợp đồng
        /// </summary>
        PATIENT_INPACKAGE_STARTDATE_CONTRACTDATE_INVALID_EARLIER=-1001,
        /// <summary>
        /// Ngày hợp đồng sơm hơn ngày active chính sách giá tại site
        /// </summary>
        PATIENT_INPACKAGE_CONTRACTDATE_PRICE_SITE_CREATEDATE_INVALID_EARLIER = -1002,
        /// <summary>
        /// Tổng thành tiền thuốc/VTTH lớn hơn NetAmount
        /// </summary>
        TOTAL_AMOUNT_DRUG_CONSUM_GREATER_THAN_NETAMOUNT = -1003,
        /// <summary>
        /// Tổng thành tiền khác NetAmount
        /// </summary>
        TOTAL_AMOUNT_SERVICE_NOT_EQUAL_NETAMOUNT = -1004
    }
}
