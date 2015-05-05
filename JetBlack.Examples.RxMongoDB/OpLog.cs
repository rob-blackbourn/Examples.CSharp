using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JetBlack.Examples.RxMongoDB
{
    public class OpLog
    {
        [BsonElement("ts"), BsonRequired]
        public BsonTimestamp Timestamp { get; set; }

        [BsonElement("h"), BsonRequired]
        public long UniqueId { get; set; }

        [BsonElement("v"), BsonRequired]
        public int Version { get; set; }

        [BsonElement("op"), BsonRequired]
        public string Operation { get; set; }

        [BsonElement("ns"), BsonRequired]
        public string Namespace { get; set; }

        [BsonElement("o"), BsonRequired]
        public BsonDocument Document { get; set; }

        [BsonElement("o2"), BsonRequired]
        public BsonDocument Update { get; set; }

        public override string ToString()
        {
            return string.Format("Timestamp={0}, UniqueId={1}, Version={2}, Operation={3}, Namespace={4}, Document{5}, Update={6}", Timestamp, UniqueId, Version, Operation, Namespace, Document, Update);
        }
    }
}
