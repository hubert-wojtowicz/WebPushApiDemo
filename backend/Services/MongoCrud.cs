using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebPushApiDemo.Services
{
    public class MongoCrud : IMongoCrud
    {
        private readonly MongoSettings _mongoSettings;
        private IMongoDatabase _db;

        public MongoCrud(IOptions<MongoSettings> mongoOptions)
        {
            if (mongoOptions?.Value == null)
            {
                throw new ArgumentNullException(nameof(mongoOptions));
            }

            _mongoSettings = mongoOptions.Value;

            var client = new MongoClient(_mongoSettings.ConnectionString);
            _db = client.GetDatabase(_mongoSettings.DatabaseName);
        }

        public async Task InsertRecord<T>(string table, T record)
        {
            var c = _db.GetCollection<T>(table);
            await c.InsertOneAsync(record);
        }

        public async Task<List<T>> LoadRecords<T>(string table)
        {
            var c = _db.GetCollection<T>(table);
            return (await c.FindAsync(new BsonDocument())).ToList();
        }
    }
}