using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Microsoft.Azure.Storage.Blob;

namespace Insights.VideoPackager
{
    public class AzureMediaProcessor
    {
        private readonly IAzureMediaServicesClient _client;
        private readonly AzureMediaProcessorOption _option;

        public async Task<string[]> Process(string filePath)
        {
            var uniqueness = Guid.NewGuid().ToString("N");
            var jobName = $"job-{uniqueness}";
            var outputAssetName = $"output-{uniqueness}";
            var inputAssetName = $"input-{uniqueness}";
            var locatorName = $"locator-{uniqueness}";

            var jobInput = await CreateInput(inputAssetName, filePath);
            await CreateOutput(outputAssetName);
            await GetOrCreateTransformAsync();

            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };

            var job = await _client.Jobs.CreateAsync(
                _option.ResourceGroupName,
                _option.AccountName,
                _option.TransformName,
                jobName,
                new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs,
                });

            // please check if job is finished
            await WaitForJobToFinishAsync(jobName);
            await CreateStreamLocator(outputAssetName, locatorName);
            return await CreateStreamingUrls(locatorName);
        }

        public AzureMediaProcessor(IAzureMediaServicesClient client, AzureMediaProcessorOption option)
        {
            _client = client;
            _option = option;
        }

        public async Task<StreamingLocator> CreateStreamLocator(string assetName, string locatorName)
        {
            return await _client.StreamingLocators.CreateAsync(_option.ResourceGroupName, _option.AccountName,
                locatorName, new StreamingLocator()
                {
                    AssetName = assetName,
                    StreamingPolicyName = PredefinedStreamingPolicy.ClearStreamingOnly
                });
        }

        private async Task<Job> WaitForJobToFinishAsync(string jobName)
        {
            const int SleepIntervalMs = 20 * 1000;

            Job job = null;

            do
            {
                job = await _client.Jobs.GetAsync(_option.ResourceGroupName, _option.AccountName, _option.TransformName, jobName);

                Console.WriteLine($"Job is '{job.State}'.");
                for (int i = 0; i < job.Outputs.Count; i++)
                {
                    JobOutput output = job.Outputs[i];
                    Console.Write($"\tJobOutput[{i}] is '{output.State}'.");
                    if (output.State == JobState.Processing)
                    {
                        Console.Write($"  Progress: '{output.Progress}'.");
                    }

                    Console.WriteLine();
                }

                if (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled)
                {
                    await Task.Delay(SleepIntervalMs);
                }
            }
            while (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled);

            return job;
        }

        public async Task<string[]> CreateStreamingUrls(string locatorName)
        {
            var streamingEndpoint = await _client.StreamingEndpoints.GetAsync(_option.ResourceGroupName, _option.AccountName,
                _option.StreamingEndpointName);

            if (streamingEndpoint == null)
            {
                throw new StreamingEndpointMissingException();
            }

            if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
            {
                await _client.StreamingEndpoints.StartAsync(_option.ResourceGroupName, _option.AccountName,
                    _option.StreamingEndpointName);
            }

            var paths = await _client.StreamingLocators.ListPathsAsync(_option.ResourceGroupName, _option.AccountName,
                locatorName);

            return paths.StreamingPaths.Select(p =>
            {
                var urlBuilder = new UriBuilder();
                urlBuilder.Scheme = "https";
                urlBuilder.Host = streamingEndpoint.HostName;
                urlBuilder.Path = p.Paths[0];
                return urlBuilder.ToString();
            }).ToArray();
        }

        private async Task<JobInputAsset> CreateInput(string assetName, string fileToUpload)
        {
            await _client.Assets.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName, assetName, new Asset());
            var response = await _client.Assets.ListContainerSasAsync(
                _option.ResourceGroupName,
                _option.AccountName,
                assetName,
                permissions: AssetContainerPermission.ReadWrite,
                expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());
            var sasUri = new Uri(response.AssetContainerSasUrls.First());
            var container = new CloudBlobContainer(sasUri);
            var blob = container.GetBlockBlobReference(Path.GetFileName(fileToUpload));
            await blob.UploadFromFileAsync(fileToUpload);

            return new JobInputAsset(assetName);
        }

        private async Task<Asset> CreateOutput(string assetName)
        {
            var outputAsset = await _client.Assets.GetAsync(_option.ResourceGroupName, _option.AccountName, assetName);
            if (outputAsset != null)
            {
                throw new OutputExistsException();
            }

            var asset = new Asset();
            return await _client.Assets.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName, assetName, asset);
        }

        private async Task<Transform> GetOrCreateTransformAsync()
        {
            var transform =
                await _client.Transforms.GetAsync(_option.ResourceGroupName, _option.AccountName, _option.TransformName);

            if (transform == null)
            {
                var output = new []
                {
                    new TransformOutput
                    {
                        Preset = new BuiltInStandardEncoderPreset()
                        {
                            PresetName = EncoderNamedPreset.AdaptiveStreaming
                        }
                    }
                };

                // Create the Transform with the output defined above
                transform = await _client.Transforms.CreateOrUpdateAsync(_option.ResourceGroupName, _option.AccountName,
                    _option.TransformName, output);
            }

            return transform;
        }

    }

    internal class StreamingEndpointMissingException : Exception
    {
    }

    internal class OutputExistsException : Exception
    {
    }
}
