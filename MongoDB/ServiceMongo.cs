using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Job_By_SAP.MongoDB
{
    public class ServiceMongo
    {
        public MongoClient SeviceData()
        {
            const string connectionUri = "mongodb://10.235.55.125:27017";
            var settings = MongoClientSettings.FromConnectionString(connectionUri);
            var clientdb = new MongoClient(settings);
            return clientdb;

        }
    }
}
