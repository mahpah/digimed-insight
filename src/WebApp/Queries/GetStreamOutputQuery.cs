using Insights.VideoPackager.Azure;
using MediatR;

namespace WebApp.Queries
{
    public class GetStreamOutputQuery : IRequest<StreamingOutput>
    {
        public GetStreamOutputQuery(string assetName, ProtectionType protectionType)
        {
            AssetName = assetName;
            ProtectionType = protectionType;
        }

        public string AssetName { get; }
        public ProtectionType ProtectionType { get; }
    }

    public enum ProtectionType
    {
        Drm = 1,
        Aes = 2
    }
}
