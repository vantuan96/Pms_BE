using DataAccess.Models;
using DataAccess.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Business.Provider
{
    public class MasterDataLib
    {

        protected static IUnitOfWork unitOfWork = new EfUnitOfWork();
        private static MasterDataLib _instant { get; set; }
        public static List<Site> listSite = null;
        public static MasterDataLib Instant
        {
            get
            {
                if (_instant == null)
                    _instant = new MasterDataLib();
                return _instant;
            }
            set
            {
                _instant = value;
            }
        }
        public static void SetMasterDataFromDb()
        {
            //Set Config rule
            listSite = unitOfWork.SiteRepository.AsQueryable().ToList();
        }
    }
}
