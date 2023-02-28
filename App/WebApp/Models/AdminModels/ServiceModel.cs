using DrFee.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrFee.Models.AdminModels
{
    public class ServiceParameterModel : PagingParameterModel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int? HISCode { get; set; }
        public string Groups { get; set; }
        public string Categories { get; set; }
        public bool? IsCalculated { get; set; }
        public string GroupName { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }

        public string GetFormatedCode()
        {
            return Code.Trim().ToLower();
        }
        public string GetFormatedName()
        {
            return Name.Trim().ToLower();
        }
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
        public List<Guid?> GetCategories()
        {
            string[] id = Categories.Trim().Split(',');
            List<Guid?> guid = new List<Guid?>();
            foreach (var i in id)
            {
                guid.Add(new Guid(i));
            }
            return guid;
        }
        public DateTime? GetStartAt()
        {
            if (string.IsNullOrEmpty(StartAt))
                return null;
            try
            {
                return DateTime.ParseExact(StartAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
        public DateTime? GetEndAt()
        {
            if (string.IsNullOrEmpty(EndAt))
                return null;
            try
            {
                return DateTime.ParseExact(EndAt, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
    }

    public class ServiceViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string ViName { get; set; }
        public string EnName { get; set; }
        public int HISCode { get; set; }
        public Guid? GroupId { get; set; }
        public string GroupCode { get; set; }
        public string GroupViName { get; set; }
        public string GroupEnName { get; set; }
        public Guid? CategoryId { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryViName { get; set; }
        public string CategoryEnName { get; set; }
        public bool IsCalculated { get; set; }
    }

    public class ServiceShortViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
    }
}