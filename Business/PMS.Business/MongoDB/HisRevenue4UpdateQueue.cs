using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using DataAccess.MongoDB;
using PMS.Contract.Models.ApigwModels;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using VM.Common;

namespace PMS.Business.MongoDB
{
    public class HisRevenue4UpdateQueue
    {
        static object objLock=new object();
        //private readonly static MongoHelpers<HISRevenueModel> userHelpers = new MongoHelpers<HISRevenueModel>(ConfigHelper.UriMongDB_Queue);
        private readonly static string queueCode = ConfigurationManager.AppSettings["HisRevenue4UpdateQueue"] !=null? ConfigurationManager.AppSettings["HisRevenue4UpdateQueue"] : "HisRevenue4UpdateQueue";
        private readonly static MGQueue mgQueue = new MGQueue(queueCode);
        public static long Count()
        {
            long countCheck = mgQueue.Count(new QueryDocument { { "QueueCode", queueCode } });
            return countCheck;
        }
        public static void Send(HISRevenueModel entity)
        {
            if (entity != null)
            {
                long countCheck = mgQueue.Count(new QueryDocument { { "_Id", entity.Id }, { "QueueCode", queueCode } });
                if (countCheck > 0)
                    return;
                entity.QueueCode = queueCode;
                entity.Id = ObjectId.GenerateNewId();
                mgQueue.Send(entity.ToBsonDocument());
            }
        }

        public static HISRevenueModel Receiver()
        {
            HISRevenueModel entityReturn = null;
            lock (objLock)
            {
                var message = mgQueue.Get(new QueryDocument("QueueCode", queueCode), TimeSpan.FromMinutes(1));
                if (message != null)
                {
                    entityReturn = BsonSerializer.Deserialize<HISRevenueModel>(message.Payload);
                    mgQueue.Ack(message.Handle);
                }
            }
            return entityReturn;
        }
    }
}
