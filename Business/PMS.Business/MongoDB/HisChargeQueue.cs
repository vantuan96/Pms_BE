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
using PMS.Contract.Models.AdminModels;

namespace PMS.Business.MongoDB
{
    public class HisChargeQueue
    {
        static object objLock=new object();
        private readonly static string queueCode = ConfigurationManager.AppSettings["UpdateDimsChargeQueue"] !=null? ConfigurationManager.AppSettings["UpdateDimsChargeQueue"] : "UpdateDimsChargeQueue";
        private readonly static MGQueue mgQueue = new MGQueue(queueCode);
        public static long Count()
        {
            long countCheck = mgQueue.Count(new QueryDocument { { "QueueCode", queueCode } });
            return countCheck;
        }
        public static void Send(HisChargeRevenueModel entity)
        {
            if (entity != null)
            {
                long countCheck = mgQueue.Count(new QueryDocument { { "ChargeId", entity.ChargeId }, { "QueueCode", queueCode } });
                if (countCheck > 0)
                    return;
                entity.Id = ObjectId.GenerateNewId();
                entity.QueueCode = queueCode;
                mgQueue.Send(entity.ToBsonDocument());
            }
        }

        public static HisChargeRevenueModel Receiver()
        {
            try {
                HisChargeRevenueModel entityReturn = null;
                lock (objLock)
                {
                    var message = mgQueue.Get(new QueryDocument("QueueCode", queueCode), TimeSpan.FromMinutes(1));
                    if (message != null)
                    {
                        entityReturn = BsonSerializer.Deserialize<HisChargeRevenueModel>(message.Payload);
                        mgQueue.Ack(message.Handle);
                    }
                }
                return entityReturn;
            } catch
            {
                return null;
            }
        }
    }
}
