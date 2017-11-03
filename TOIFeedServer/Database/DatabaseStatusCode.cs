using System;
using System.Collections.Generic;
using System.Text;

namespace TOIFeedServer
{
    public enum DatabaseStatusCode
    {
        Error = -1,
        Ok = 0,
        Created = 1,
        Updated = 2,
        Deleted = 3,
        ListContainsDuplicate = 4,
        AlreadyContainsElement = 5,
        NoElement = 6

        
    }
}
