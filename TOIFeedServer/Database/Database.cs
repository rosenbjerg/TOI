using TOIClasses;
using TOIFeedServer.Managers;

namespace TOIFeedServer
{
    public class Database
    {
        public IDbCollection<TagModel> Tags { get; }
        public IDbCollection<ToiModel> Tois { get; }
        public IDbCollection<ContextModel> Contexts { get; }
        public IDbCollection<User> Users { get; }

        internal Database(IDbCollection<TagModel> tags, IDbCollection<ToiModel> tois, IDbCollection<ContextModel> contexts, IDbCollection<User> users)
        {
            Tags = tags;
            Tois = tois;
            Contexts = contexts;
            Users = users;
        }

    }
}
