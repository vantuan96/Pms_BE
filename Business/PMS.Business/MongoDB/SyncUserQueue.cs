using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using DataAccess.MongoDB;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using PMS.Contract.Models.MasterData;

namespace PMS.Business.MongoDB
{
    public class SyncUserQueue
    {
        static object objLock=new object();
        private readonly static string queueCode = ConfigurationManager.AppSettings["SyncUserQueue"] !=null? ConfigurationManager.AppSettings["SyncUserQueue"] : "SyncUserQueue";
        private readonly static MGQueue mgQueue = new MGQueue(queueCode);
        public static long Count()
        {
            long countCheck = mgQueue.Count(new QueryDocument { { "QueueCode", queueCode } });
            return countCheck;
        }
        public static void Send(UserSitesModel entity)
        {
            if (entity != null)
            {
                long countCheck = mgQueue.Count(new QueryDocument { { "UserSiteId", entity.UserSiteId }, { "QueueCode", queueCode } });
                if (countCheck > 0)
                    return;
                entity.Id = ObjectId.GenerateNewId();
                entity.QueueCode = queueCode;
                mgQueue.Send(entity.ToBsonDocument());
            }
        }

        public static UserSitesModel Receiver()
        {
            UserSitesModel entityReturn = null;
            lock (objLock)
            {
                var message = mgQueue.Get(new QueryDocument("QueueCode", queueCode), TimeSpan.FromMinutes(1));
                if (message != null)
                {
                    entityReturn = BsonSerializer.Deserialize<UserSitesModel>(message.Payload);
                    mgQueue.Ack(message.Handle);
                }
            }
            return entityReturn;
        }
    }
}
