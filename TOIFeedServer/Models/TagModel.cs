

using MongoDB.Bson.Serialization.Attributes;

namespace TOIFeedServer.Models
{
    public class TagModel
    {
        public TagModel()
        {
            
        }
        public TagModel(string id, TagType type)
        {
            TagId = id;
            TagType = type;
        }

        [BsonId]
        public string TagId { get; set; }

        [BsonElement(nameof(Name))]
        public string Name { get; set; }

        [BsonElement(nameof(TagType))]
        public TagType TagType { get; set; }

        [BsonElement(nameof(Latitude))]
        public double Latitude { get; set; }

        [BsonElement(nameof(Longitude))]
        public double Longitude { get; set; }

        [BsonElement(nameof(Radius))]
        public int Radius { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TagModel t && t.TagId == TagId;
        }

        protected bool Equals(TagModel other)
        {
            return TagId.Equals(other.TagId);
        }

        public override int GetHashCode()
        {
            return TagId.GetHashCode();
        }
    }
}
