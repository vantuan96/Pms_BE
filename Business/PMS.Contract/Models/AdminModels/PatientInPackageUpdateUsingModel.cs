using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMS.Contract.Models.AdminModels
{
    public class PatientInPackageUpdateUsingModel
    {
        public string QueueCode { get; set; }
        public ObjectId _Id;
        [BsonId]
        public ObjectId Id { get { return this._Id; } set { this._Id = value; } }
        public Guid? PatientInPackageId { get; set; }
        public string PID { get; set; }
        public string PackageCode { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime last_fail = DateTime.MinValue;
        public int count_fail = 0;
        /// <summary>
        /// Number of the time that the message have been sent
        /// </summary>    
        public int process_fail = 0;
    }
}
