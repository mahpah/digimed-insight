using System;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Azure.Management.Media;
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
            _sut = new AzureMediaProcessor(client, new AzureMediaProcessorOption()
            {
                AccountName = "elnmedia",
                ResourceGroupName = "everlearn",
                TransformName = "EverlearnTransformWithAdaptiveStreamingPreset",
                StreamingEndpointName = "default"
            });
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
    }
}
