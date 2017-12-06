using System;
using System.Collections.Generic;
using System.Text;

namespace TOIFeedRepo.Database
{
    internal class FeedRepoDatabaseFactory
    {
        public static FeedRepoDatabase Build(TOIFeedServer.DatabaseFactory.DatabaseType type)
        {
            //TODO implement this correctly
            return new FeedRepoDatabase();
        }
    }
}
