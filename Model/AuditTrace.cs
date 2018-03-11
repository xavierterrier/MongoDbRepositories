using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Com.Xterr.MongoDbRepositories.Model
{
    public class AuditTrace
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; }

        [BsonDateTimeOptions(Representation = BsonType.Document, Kind = DateTimeKind.Utc)]
        public DateTime? Timestamp { get; set; }

        public string UserId { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string Trace { get; set; }

        public Object OldEntity { get; set; }

        public Object NewEntity { get; set; }
    }
}
