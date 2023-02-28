using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMS.Contract.Models
{
    public class SearchPagingParameterModel: PagingParameterModel
    {
        public string Search { get; set; }
        public string ServiceCode { get; set; }

        public string GetSearch()
        {
            return Search.Trim().ToLower();
        }
        public string GetServiceCode()
        {
            return ServiceCode.Trim().ToLower();
        }
    }
}