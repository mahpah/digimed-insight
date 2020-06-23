using MediatR;

namespace WebApp.Commands
{
    public class ProcessVideoCommand : IRequest
    {
        public string InputAssetName { get; set; }
    }
}
