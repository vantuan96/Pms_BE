using System;
using System.Collections.Generic;
using VM.Common;

namespace PMS.Contract.Models
{
    public class PatientParameterModel : PagingParameterModel
    {
        public string Name { get; set; }
        public string Pid { get; set; }
        public string Mobile { get; set; }
        public string Birthday { get; set; }
        public DateTime? GetBirthDay()
        {
            if (string.IsNullOrEmpty(Birthday))
                return null;
            try
            {
                return DateTime.ParseExact(Birthday, Constant.DATE_FORMAT, null);
            }
            catch
            {
                return null;
            }
        }
    }
    public class PatientInPackageParameterModel : PagingParameterModel
    {
        public string Search { get; set; }
        public string Pid { get; set; }
        public string PackageCode { get; set; }
        public string PackageName { get; set; }
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
        public string Statuses { get; set; }
        public List<int?> GetStatus()
        {
            string[] id = Statuses.Trim().Split(',');
            List<int?> list = new List<int?>();
            foreach (var i in id)
            {
                int iStatus = 0;
                int.TryParse(i,out iStatus);
                list.Add(iStatus);
            }
            return list;
        }
        public string ContractOwner { get; set; }
    }
}