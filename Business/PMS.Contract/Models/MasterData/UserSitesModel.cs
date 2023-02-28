using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VM.Common;

namespace PMS.Contract.Models.MasterData
{
    public class UserSitesModel
    {
        public string QueueCode { get; set; }
        public ObjectId _Id;
        [BsonId]
        public ObjectId Id { get { return this._Id; } set { this._Id = value; } }
        public string UserSiteId { get; set; }
        public Guid SiteId { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// Number of the time that the message have been sent
        /// </summary>    
        public int process_fail = 0;
    }
}
