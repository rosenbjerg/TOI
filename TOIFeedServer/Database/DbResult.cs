namespace TOIFeedServer
{
    public class DbResult<T>
    {
        public DbResult(T result, DatabaseStatusCode status)
        {
            Result = result;
            Status = status;
        }
        public T Result { get; }
        public DatabaseStatusCode Status { get; }
    }
}