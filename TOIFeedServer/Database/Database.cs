using TOIFeedServer.Models;

namespace TOIFeedServer.Database
{
    public class Database
    {
        public IDbCollection<TagModel> Tags { get; }
        public IDbCollection<ToiModel> Tois { get; }
        public IDbCollection<ContextModel> Contexts { get; }

        internal Database(IDbCollection<TagModel> tags, IDbCollection<ToiModel> tois, IDbCollection<ContextModel> contexts)
        {
            Tags = tags;
            Tois = tois;
            Contexts = contexts;
        }
        
    }
}
