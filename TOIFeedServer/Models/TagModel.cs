

using MongoDB.Bson.Serialization.Attributes;

namespace TOIFeedServer.Models
{
    public class TagModel : IModel
    {
        public TagModel()
        {
            
        }
        public TagModel(string id, TagType type)
        {
            Id = id;
            TagType = type;
        }

        [BsonId]
        public string Id { get; set; }

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
            return obj is TagModel t && t.Id == Id;
        }

        protected bool Equals(TagModel other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
