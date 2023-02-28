using DataAccess.MongoDB;
using PMS.Contract.Models.MasterData;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.Business.Provider
{
    public class MGMasterDataRepo
    {
        private static MGMasterDataRepo _instant { get; set; }
        public static MGMasterDataRepo Instant
        {
            get
            {
                if (_instant == null)
                {
                    _instant = new MGMasterDataRepo();
                    mgHelpers_site = new MongoHelpers<dynamic>(ConfigHelper.UriMongDB_MasterData, "Sites");
                    mgHelpers_spec = new MongoHelpers<dynamic>(ConfigHelper.UriMongDB_MasterData, "Specialties");
                }

                return _instant;
            }
            set
            {
                _instant = value;
            }
        }
        #region Function for Sites entity
        private static MongoHelpers<dynamic> mgHelpers_site = new MongoHelpers<dynamic>(ConfigHelper.UriMongDB_MasterData, "Sites");
        public Sites FindSiteByHosId(string hosId)
        {
            var enities = mgHelpers_site.Find<List<Sites>>(Query.And(Query.EQ("HospitalId", hosId)));
            return enities != null && enities.Count > 0 ? enities[0] : null;
        }
        public Sites FindSiteById(string Id)
        {
            var enities = mgHelpers_site.Find<List<Sites>>(Query.And(Query.EQ("Id", Id)));
            return enities != null && enities.Count > 0 ? enities[0] : null;
        }
        #endregion .Function for Sites entity
        #region Function for Specialty entity
        private static MongoHelpers<dynamic> mgHelpers_spec = new MongoHelpers<dynamic>(ConfigHelper.UriMongDB_MasterData, "Specialties");
        public Specialties FindSpecById(string Id)
        {
            var enities = mgHelpers_spec.Find<List<Specialties>>(Query.And(Query.EQ("Id", Id)));
            return enities != null && enities.Count > 0 ? enities[0] : null;
        }
        public Specialties FindSpecByCode(string Code)
        {
            var enities = mgHelpers_spec.Find<List<Specialties>>(Query.And(Query.Matches("Code", Code)));
            return enities != null && enities.Count > 0 ? enities[0] : null;
        }
        #endregion .Function for Sites entity
    }
}
