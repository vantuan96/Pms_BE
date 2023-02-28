using System;
using System.Collections.Generic;

namespace PMS.Contract.Models
{
    public class PackageGroupParameterModel: PagingParameterModel
    {
        public string Name { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// -1: ALL
        /// 0: InActivated
        /// 1: Activated
        /// </summary>
        public int Status { get; set; }
    }
    public class PackageParameterModel : PagingParameterModel
    {
        public string keyword { get; set; }
        public string ids { get; set; }
        public List<Guid?> GetIds()
        {
            string[] id = ids.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
        public string Groups { get; set; }
        public List<Guid?> GetGroups()
        {
            string[] id = Groups.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
        public string Sites { get; set; }
        public List<Guid?> GetSites()
        {
            string[] id = Sites.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
        /// <summary>
        /// -1: ALL
        /// 0: InActivated
        /// 1: Activated
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Thiết lập giá?
        /// -1: ALL
        /// 0: NotYet-setted
        /// 1: Setted
        /// </summary>
        public int SetPrice { get; set; }
        /// <summary>
        /// Thiết lập giá?
        /// -1: ALL
        /// 0: Unlimited
        /// 1: Limited
        /// </summary>
        public int Limited { get; set; }
        public bool IsShowExpireDate { get; set; }
        /// <summary>
        /// Đang có hiệu lực
        /// </summary>
        public bool IsAvailable { get; set; }
        public string CurrentGroupId { get; set; }
        /// <summary>
        /// Chỉ show những Gói cùng gốc
        /// </summary>
        public bool OnlyShowSameRoot { get; set; }
    }
    public class ServiceInPackageParameterModel : PagingParameterModel
    {
        public string ServiceIds { get; set; }
        public List<Guid?> GetServiceIds()
        {
            string[] id = ServiceIds.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
        public string PackageId { get; set; }
        /// <summary>
        /// -1: ALL
        /// 0: InActivated
        /// 1: Activated
        /// </summary>
        public int Status { get; set; }
        /// <summary>
        /// Loại dịch vụ trong gói
        /// -1: ALL
        /// 1: Dịch vụ thông thường
        /// 2: Dịch vụ là Thuốc / VTTH
        /// </summary>
        public int ServiceType { get; set; }
        /// <summary>
        /// Là dịch vụ thay thế
        /// -1: ALL
        /// 0: Dịch vụ gốc
        /// 1: Dịch vụ thay thế
        /// </summary>
        public int IsServiceReplace { get; set; }
    }
}