using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.Storage.Blob;

namespace Insights.VideoPackager.Azure
{
    public class AzureVideoService
    {
        private readonly IAzureMediaServicesClient _client;
        private readonly AzureVideoServiceOption _option;

        public AzureVideoService(IAzureMediaServicesClient client, AzureVideoServiceOption option)
        {
            _client = client;
            _option = option;
        }

        public async Task<string> CreateAsset(string fileName)
        {
            var assetName = $"{fileName}-{Guid.NewGuid()}";
            await _client.Assets.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName, assetName, new Asset());
            return assetName;
        }

        public async Task<string> GenerateUploadUrl(string assetName)
        {
            var assetContainerSas = await _client.Assets.ListContainerSasAsync(
                _option.ResourceGroupName,
                _option.AccountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());
            return assetContainerSas.AssetContainerSasUrls.First();
        }
    }
}
