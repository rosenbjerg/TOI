namespace TOIFeedServer
{
    public struct DbResult<T>
    {
        public DbResult(T result, DatabaseStatusCode status)
        {
            Result = result;
            Status = status;
        }
        public readonly T Result;
        public readonly DatabaseStatusCode Status;
    }
}