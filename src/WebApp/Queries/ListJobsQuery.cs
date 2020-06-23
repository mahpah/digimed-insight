using MediatR;
using WebApp.Queries.QueryResults;

namespace WebApp.Queries
{
    public class ListJobsQuery : IRequest<QueryResult<JobInformation>>
    {
        public ListJobsQuery(string nextToken)
        {
            NextToken = nextToken;
        }

        public string NextToken { get; private set; }
    }
}
