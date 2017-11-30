using System.Threading.Tasks;
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
        public IDbCollection<StaticFile> Files { get; }

        internal Database(IDbCollection<TagModel> tags, IDbCollection<ToiModel> tois, IDbCollection<ContextModel> contexts, IDbCollection<User> users, IDbCollection<StaticFile> files)
        {
            Tags = tags;
            Tois = tois;
            Contexts = contexts;
            Users = users;
            Files = files;
        }

        public async Task TruncateDatabase()
        {
            await Tags.DeleteAll();
            await Tois.DeleteAll();
            await Contexts.DeleteAll();
            await Users.DeleteAll();
        }
    }
}
