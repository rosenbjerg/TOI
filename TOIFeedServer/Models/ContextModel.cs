using MongoDB.Bson.Serialization.Attributes;


namespace TOIFeedServer.Models
{
    public class ContextModel : IModel
    {
        public ContextModel()
        {
            
        }
        public ContextModel(string id, string title, string description = null)
        {
            Id = id;
            Title = title;
            Description = description;
        }

        [BsonId]
        public string Id { get; set; }

        [BsonElement(nameof(Description))]
        public string Description { get; set; }

        [BsonElement(nameof(Title))]
        public string Title { get; set; }

        public override bool Equals(object obj)
        {
            return obj != null && obj is ContextModel t && t.Id == Id;
        }

        protected bool Equals(ContextModel other)
        {
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        
    }
}
