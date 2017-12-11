using System;
using System.Collections.Generic;
using System.Text;
using TOIClasses;
using TOIFeedServer;

namespace TOIFeedRepo.Database
{
    internal class FeedRepoDatabase
    {
        public IDbCollection<Feed> Feeds { get; }
        public IDbCollection<FeedOwner> Customers { get; }

        public FeedRepoDatabase(IDbCollection<Feed> feeds, IDbCollection<FeedOwner> customers)
        {
            Feeds = feeds;
            Customers = customers;
        }
    }
}
