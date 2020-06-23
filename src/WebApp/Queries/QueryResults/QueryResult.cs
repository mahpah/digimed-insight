namespace WebApp.Queries.QueryResults
{
    public class QueryResult<T>
    {
        public T[] Items { get; set; }
        public string NextToken { get; set; }
    }
}
