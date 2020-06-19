using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using FluentAssertions;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.Storage.Blob;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest.Azure.Authentication;
using Xunit;
using Xunit.Abstractions;

namespace Insights.VideoPackager.Tests
{
    public class AzureMediaProcessorTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private AzureMediaProcessor _sut;
        private AzureMediaServicesClient _client;
        private AzureMediaProcessorOption _options;

        public AzureMediaProcessorTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            var secret = ".Xb2v7adwQwodK79p1FKe-75.F.nWp.4BY";
            var clientId = "950c69c8-b022-4b93-8d47-0e452ad0567a";
            var tenantId = "e08a9711-1bd0-47dd-8686-28c016518286";
            var credendial =
                ApplicationTokenProvider.LoginSilentAsync(tenantId, new ClientCredential(clientId, secret),
                        ActiveDirectoryServiceSettings.Azure).GetAwaiter()
                    .GetResult();

            var armEndpoint = "https://management.azure.com";
            var client = new AzureMediaServicesClient(new Uri(armEndpoint), credendial)
            {
                SubscriptionId = "c104684d-4609-4677-b322-c8e66e49c5f1"
            };

            var options = new AzureMediaProcessorOption()
            {
                AccountName = "elnmedia",
                ResourceGroupName = "everlearn",
                TransformName = "EverlearnTransformWithAdaptiveStreamingPreset",
                StreamingEndpointName = "default",
                Issuer = "https://everlearn.vn",
                Audience = "https://abc.com",
                TokenKey = Convert.FromBase64String(
                    "TFBMU3ZCNVJ1ekZ4cFZxRnhXc1VzQ0wyRDgzekxPRmxHVjBSOHJUcDFmK3hyUVRXWE8vWEFRPT0K")
            };

            _sut = new AzureMediaProcessor(client, options);

            _client = client;
            _options = options;
        }

        [Fact]
        public async Task should_create_media_stream_urls()
        {
            var urls = await _sut.Process(Path.Combine(Directory.GetCurrentDirectory(), "files", "input.avi"));
            urls.Should().NotBeEmpty();
            foreach (var url in urls)
            {
                _testOutputHelper.WriteLine(url);
            }
        }

        [Fact]
        public async Task should_create_stream_locator()
        {
            var locator = await _sut.CreateStreamLocator("output-9c2913de4b414ad5b51d0e74113bd428",
                "locator-9c2913de4b414ad5b51d0e74113bd428");

            locator.Should().NotBeNull();
        }

        [Fact]
        public async Task should_create_stream_urls()
        {
            var urls = await _sut.CreateStreamingUrls("locator-9c2913de4b414ad5b51d0e74113bd428");
            urls.Should().NotBeEmpty();
            foreach (var url in urls)
            {
                _testOutputHelper.WriteLine(url);
            }
        }

        [Fact]
        public async Task should_create_protected_locator()
        {
            var locatorName = "locator-9c2913de4b414ad5b51d0e74113bd428-protected";
            var assetName = "output-9c2913de4b414ad5b51d0e74113bd428";
            var contentPolicyName = "content-policy-9c2913de4b414ad5b51d0e74113bd428";
            var keyIdentifier = await _sut.StreamProtected(assetName, locatorName, contentPolicyName);
            var urls = await _sut.CreateStreamingUrls(locatorName);

            foreach (var url in urls)
            {
                _testOutputHelper.WriteLine(url);
            }

            var token = await _sut.CreateToken(contentPolicyName, keyIdentifier);
            token.Should().NotBeEmpty();
            _testOutputHelper.WriteLine(token);
        }
    }
}
