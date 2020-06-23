using System.Threading;
using System.Threading.Tasks;
using Insights.VideoPackager.Azure;
using MediatR;

namespace WebApp.Commands.CommandHandlers
{
    public class ProcessVideoCommandHandler : IRequestHandler<ProcessVideoCommand>
    {
        private readonly AzureVideoService _azureVideoService;

        public ProcessVideoCommandHandler(AzureVideoService azureVideoService)
        {
            _azureVideoService = azureVideoService;
        }

        public async Task<Unit> Handle(ProcessVideoCommand request, CancellationToken cancellationToken)
        {
            await _azureVideoService.Process(request.InputAssetName);
            return Unit.Value;
        }
    }
}
