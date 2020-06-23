using System.Threading;
using System.Threading.Tasks;
using Insights.VideoPackager.Azure;
using MediatR;

namespace WebApp.Commands.CommandHandlers
{
    public class CleanAssetCommandHandler : IRequestHandler<CleanAssetCommand>
    {
        private readonly AzureVideoService _azureVideoService;

        public CleanAssetCommandHandler(AzureVideoService azureVideoService)
        {
            _azureVideoService = azureVideoService;
        }

        public async Task<Unit> Handle(CleanAssetCommand request, CancellationToken cancellationToken)
        {
            await _azureVideoService.DeleteMultipleAssets(request.AssetNames);
            return Unit.Value;
        }
    }
}
