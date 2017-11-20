using System.Threading.Tasks;

namespace TOIFeedServer.Database
{
    public partial class DatabaseService
    {
        private readonly Database _db;

        public DatabaseService(DatabaseFactory.DatabaseType dbType)
        {
            _db = DatabaseFactory.BuildDatabase(dbType);
        }

        public async Task<DatabaseStatusCode> TruncateDatabase()
        {
            await _db.Contexts.DeleteAll();
            await _db.Tags.DeleteAll();
            await _db.Tois.DeleteAll();
            return DatabaseStatusCode.Ok;
        }
    }
}
