using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using System.Configuration;
using VM.Common;
using Newtonsoft.Json;

namespace DataAccess.MongoDB
{
    public class MongoHelpers<T> where T : class
    {
        public MongoCollection<T> Collection { get; private set; }
        public MongoHelpers(string strConnection,string collectionName)
        {
            //var server = MongoServer.Create("mongodb://localhost/GAPLuckyQueue");
            MongoClient mgClient = new MongoClient(strConnection);
            var db = mgClient.GetServer().GetDatabase(new MongoUrl(strConnection).DatabaseName);
            //Collection = db.GetCollection("mycollection");
            //var db = server.GetDatabase("GAPLuckyQueue");
            Collection = db.GetCollection<T>(collectionName);

            //var server = MongoServer.Create("mongodb://10.2.0.173");
            //var db = server.GetDatabase("GAPLuckyQueue");
            //Collection = db.GetCollection<T>(typeof(T).Name.ToLower());
        }
        public T Find<T>(IMongoQuery query)
        {
            var entities=Collection.Find(query).ToList<dynamic>();
            var entity = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(entities));
            return entity;
        }
    }
}
