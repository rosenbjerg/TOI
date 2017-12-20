using System.Dynamic;
using System.IO;
using System.Text;
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
        public string ApiKey { get; private set; }

        private const string ApiKeyFile = "./key";

        internal Database(IDbCollection<TagModel> tags, IDbCollection<ToiModel> tois, IDbCollection<ContextModel> contexts, IDbCollection<User> users, IDbCollection<StaticFile> files)
        {
            Tags = tags;
            Tois = tois;
            Contexts = contexts;
            Users = users;
            Files = files;

            if (!File.Exists(ApiKeyFile))
            {
                return;
            }

            using (var reader = File.Open(ApiKeyFile, FileMode.Open))
            {
                var fileBytes = new byte[reader.Length];
                reader.Read(fileBytes, 0, fileBytes.Length);
                ApiKey = Encoding.ASCII.GetString(fileBytes);
            }
        }

        public async Task StoreApiKey(string apiKey)
        {
            ApiKey = apiKey;
            if (File.Exists(ApiKeyFile))
            {
                File.Delete(ApiKeyFile);
            }

            using (var writer = File.Create(ApiKeyFile))
            {
                var strBytes = Encoding.ASCII.GetBytes(apiKey);
                await writer.WriteAsync(strBytes, 0, strBytes.Length);
            }
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
