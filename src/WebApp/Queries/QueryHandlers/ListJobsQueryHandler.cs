using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Insights.VideoPackager.Azure;
using MediatR;
using Microsoft.Azure.Management.Media.Models;
using WebApp.Queries.QueryResults;

namespace WebApp.Queries.QueryHandlers
{
    public class ListJobsQueryHandler : IRequestHandler<ListJobsQuery, QueryResult<JobInformation>>
    {
        private readonly AzureVideoService _azureVideoService;

        public ListJobsQueryHandler(AzureVideoService azureVideoService)
        {
            _azureVideoService = azureVideoService;
        }

        public async Task<QueryResult<JobInformation>> Handle(ListJobsQuery request, CancellationToken cancellationToken)
        {
            var page = await _azureVideoService.ListJobs(request.NextToken);
            return new QueryResult<JobInformation>()
            {
                Items = page.Select(t =>
                {
                    var inputAsset = t.Input as JobInputAsset;
                    return new JobInformation()
                    {
                        File = inputAsset?.Files.FirstOrDefault(),
                        Input = inputAsset?.AssetName,
                        Name = t.Name,
                        Output = t.Outputs.OfType<JobOutputAsset>().Select(x => x.AssetName).First(),
                        CreatedDate = t.Created,
                        State = t.State.ToString(),
                        ElapsedTime = (t.EndTime - t.StartTime).ToString()
                    };
                }).ToArray(),
                NextToken = page.NextPageLink
            };
        }
    }
}
