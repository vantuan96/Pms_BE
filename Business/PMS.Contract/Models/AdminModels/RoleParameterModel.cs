using System;
using System.Collections.Generic;

namespace PMS.Contract.Models
{
    public class GroupActionParameterModel : PagingParameterModel
    {
        public string keyword { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// -1: ALL
        /// 0: Hide
        /// 1: Show
        /// </summary>
        public int IsDisplay { get; set; }
    }
}