﻿using System;
using System.Collections.Generic;
using System.Text;
using TOIFeedServer;

namespace TOIFeedRepo.Database
{
    internal class FeedRepoDatabase
    {
        public IDbCollection<Feed> Feeds { get; }
    }
}