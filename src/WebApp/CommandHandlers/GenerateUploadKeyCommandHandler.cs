using System.Threading;
using System.Threading.Tasks;
using Insights.VideoPackager.Azure;
using MediatR;
using WebApp.CommandResults;
using WebApp.Commands;

namespace WebApp.CommandHandlers
{
    public class GenerateUploadKeyCommandHandler : IRequestHandler<GenerateUploadKeyCommand, UploadKey>
    {
        private readonly AzureVideoService _azureVideoService;

        public GenerateUploadKeyCommandHandler(AzureVideoService azureVideoService)
        {
            _azureVideoService = azureVideoService;
        }

        public async Task<UploadKey> Handle(GenerateUploadKeyCommand request, CancellationToken cancellationToken)
        {
            var assetName = await _azureVideoService.CreateAsset(request.FileName);
            var uploadUrl = await _azureVideoService.GenerateUploadUrl(assetName);
            return new UploadKey
            {
                AssetName = assetName,
                UploadUrl = uploadUrl
            };
        }
    }
}
