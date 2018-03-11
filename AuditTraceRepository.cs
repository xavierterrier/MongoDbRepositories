using System;
using Com.Xterr.MongoDbRepositories.Model;
using MongoDB.Driver;

namespace Core.Repositories
{
    internal class AuditTraceRepository
    {
        private readonly IMongoCollection<AuditTrace> _collection;
        private const string collectionName = "AuditTraces";

        public AuditTraceRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<AuditTrace>(collectionName);
        }

        public void Trace(string userId, string entityType, string entityId, string trace, Object oldEntity, Object newEntity)
        {
            var model = new AuditTrace()
            {
                Timestamp = DateTime.Now.ToUniversalTime(),
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Trace = trace,
                OldEntity = oldEntity,
                NewEntity = newEntity
            };

            _collection.InsertOne(model);
        }      
    }
}

