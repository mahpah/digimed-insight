using MediatR;

namespace WebApp.Commands
{
    public class CleanAssetCommand : IRequest<Unit>
    {
        public string[] AssetNames { get; set; }
    }
}
