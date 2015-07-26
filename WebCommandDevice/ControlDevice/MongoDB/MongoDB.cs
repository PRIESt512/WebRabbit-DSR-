using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using WebCommandDevice.ControlDevice.Pool;

namespace WebCommandDevice.ControlDevice.MongoDB
{
    public abstract class MongoDb : IPoolable, IDisposable
    {
        private static readonly String Connection;
        private static readonly String Database;

        protected static MongoClient _client;
        protected static IMongoDatabase _database;

        static MongoDb()
        {
            Connection = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;
            Database = ConfigurationManager.AppSettings.Get("Database");

            _client = new MongoClient(Connection);
            _database = _client.GetDatabase(Database);
        }

        public abstract void InstallationState(string deviceId);
        public abstract void ResetState();

        /// <summary>
        /// Очистка не нужна, БД сама закрывает соединение
        /// </summary>
        public void Dispose() { }
    }

    public sealed class Logging : MongoDb, IPoolable
    {
        private IMongoCollection<BsonDocument> _collection;
        /// <summary>
        /// Сохранение команды в истории команд устройства
        /// </summary>
        /// <param name="jsonCommand">Команда, посылаемая устройству</param>
        /// <returns></returns>
        public async Task SaveHistoryCommandAsync(String jsonCommand)
        {
            using (var jsonReader = new JsonReader(jsonCommand))
            {
                var context = BsonDeserializationContext.CreateRoot(jsonReader);
                var document = _collection.DocumentSerializer.Deserialize(context);
                await _collection.InsertOneAsync(document);
            }
        }

        /// <summary>
        /// Получение истории команд конкретного устройства
        /// </summary>
        /// <returns></returns>
        public async Task<List<String>> GetHistoryCommandAsync()
        {
            var listCommand = new List<String>();

            await _collection.Find(new BsonDocument())
                .ForEachAsync(document =>
                {
                    using (var stringWriter = new StringWriter())
                    using (var jsonWriter = new JsonWriter(stringWriter))
                    {
                        var context = BsonSerializationContext.CreateRoot(jsonWriter);
                        _collection.DocumentSerializer.Serialize(context, document);
                        listCommand.Add(stringWriter.ToString());
                    }
                });

            return listCommand;
        }

        public override void InstallationState(String deviceId)
        {
            _collection = _database.GetCollection<BsonDocument>(deviceId);
        }

        public override void ResetState()
        {
            _collection = null;
        }
    }
}