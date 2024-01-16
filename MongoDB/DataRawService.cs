using Job_By_SAP.WCM;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.MongoDB
{
    public class MongoService<T>
    {
        private readonly IMongoCollection<T> _collection;

        public MongoService(IMongoClient mongoClient, string databaseName, string collectionName)
        {
            var database = mongoClient.GetDatabase(databaseName);
            _collection = database.GetCollection<T>(collectionName);
        }
        public void InsertData(List<T> data)
        {
            _collection.InsertMany(data);
        }


    }
}
