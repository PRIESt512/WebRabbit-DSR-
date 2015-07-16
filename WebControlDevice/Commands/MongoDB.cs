using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace WebControlDevice.Commands
{
    class MongoDB
    {
        private static String Connection => ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
        private static String Database => ConfigurationManager.AppSettings.Get("Database");

        protected MongoClient client;
        protected IMongoDatabase database;

        public MongoDB()
        {
            client = new MongoClient(Connection);
            database = client.GetDatabase(Database);
        }
    }

    class Logging : MongoDB
    {
        public async Task SaveCommandDeviceAsync(String deviceId, String jsonCommand)
        {
            var collection = database.GetCollection<BsonDocument>(deviceId);

            using (var jsonReader = new JsonReader(jsonCommand))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                var document = collection.DocumentSerializer.Deserialize(context);
                await collection.InsertOneAsync(document);
            }
        }

        public async Task<List<String>> CommandHistory(String deviceId)
        {
            var collection = database.GetCollection<BsonDocument>(deviceId);

            var listCommand = new List<String>();

            await collection.Find(new BsonDocument())
                .ForEachAsync((document) =>
                {
                    using (var stringWriter = new StringWriter())
                    using (var jsonWriter = new JsonWriter(stringWriter))
                    {
                        var context = BsonSerializationContext.CreateRoot(jsonWriter);
                        collection.DocumentSerializer.Serialize(context, document);
                        listCommand.Add(stringWriter.ToString());
                    }
                });

            return listCommand;
        }
    }
}