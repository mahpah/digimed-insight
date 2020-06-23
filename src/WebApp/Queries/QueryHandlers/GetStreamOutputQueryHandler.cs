using System;
using System.Threading;
using System.Threading.Tasks;
using Insights.VideoPackager.Azure;
using MediatR;

namespace WebApp.Queries.QueryHandlers
{
    public class GetStreamOutputQueryHandler : IRequestHandler<GetStreamOutputQuery, StreamingOutput>
    {
        private readonly AzureVideoService _azureVideoService;

        public GetStreamOutputQueryHandler(AzureVideoService azureVideoService)
        {
            _azureVideoService = azureVideoService;
        }

        public async Task<StreamingOutput> Handle(GetStreamOutputQuery request, CancellationToken cancellationToken)
        {
            return request.ProtectionType switch
            {
                ProtectionType.Drm => await _azureVideoService.GetDrmProtectedStreamingOutput(request.AssetName),
                ProtectionType.Aes => await _azureVideoService.GetAesProtectedStreamingOutput(request.AssetName),
                _ => throw new ArgumentException("Protection type invalid")
            };
        }
    }
}
