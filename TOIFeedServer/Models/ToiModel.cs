using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using TOIClasses;

namespace TOIFeedServer.Models
{
    public class ToiModel : ToiInfo, IModel
    {
        [BsonId]
        public string Id { get; set; }

        [BsonElement(nameof(Tags))]
        public List<string> Tags { get; set; }
        [BsonElement(nameof(Contexts))]
        public List<string> Contexts { get; set; }

        public object GetToiInfo()
        {
            return new { Title, Description, Url, Image };
        }

        public override bool Equals(object obj)
        {
            return obj is ToiModel t && t.Id == Id;
        }

        protected bool Equals(ToiModel other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
